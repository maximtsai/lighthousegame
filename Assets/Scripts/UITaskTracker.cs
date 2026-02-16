using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UITaskTracker : MonoBehaviour
{
    // ============================= PUBLIC CONFIG =============================
    [Header("UI References")]
    [SerializeField] TMP_Text text_tasklist;
    [SerializeField] Image tasklist_bg;
    [SerializeField] Canvas canvas_full_list;

    [Header("Audio")]
    [SerializeField] AudioClip whoosh_in;
    [SerializeField] AudioClip scribble_sfx;
    [SerializeField] AudioClip interrupt;

    [Header("Animation Settings")]
    [SerializeField] float revealDuration = 0.6f;
    private float typewriterDelay = 0.055f;
    [SerializeField] float hideDuration = 0.5f;

    // ============================= INTERNAL STATE =============================
    private enum UIState { Hidden, Revealing, Typing, IdleVisible, Hiding }
    private UIState state = UIState.Hidden;

    private Coroutine driverCoroutine;
    private int animationToken = 0;
    private bool gotMask = false;
    private RectTransform mask;

    private string textToDisplay = "";
    private bool posflipped = false;
    private int countDownFlip = 30;

    private Vector2 basePos;
    private Vector2 basePosText;
    private MessageBus.SubscriptionHandle changeQuestTextHandle;

    void Start()
    {
        basePos = tasklist_bg.rectTransform.anchoredPosition;
        basePosText = text_tasklist.rectTransform.anchoredPosition;

        changeQuestTextHandle = MessageBus.Instance.Subscribe("ChangeQuestText", (args) =>
        {
            // ================== HARD INTERRUPT MODE ==================

            // 1) Kill all animation logic instantly
            animationToken++;

            if (driverCoroutine != null)
            {
                StopCoroutine(driverCoroutine);
                driverCoroutine = null;
            }

            // 2) Force final visible state
            state = UIState.IdleVisible;
            ForceVisible();

            // 3) Read message safely
            string message = args != null && args.Length > 0
                ? args[0] as string
                : "~~~";

            SetTextDisplay(message);

            // 4) Snap final visuals instantly
            text_tasklist.text = message;

            // 5) Ensure mask fully open
            if (!gotMask)
            {
                gotMask = true;
                mask = tasklist_bg.transform.parent.GetComponent<RectTransform>();
            }

            if (mask != null)
            {
                mask.sizeDelta = tasklist_bg.rectTransform.sizeDelta;
            }

            // 6) Sound + shake
            PlaySound(interrupt);
            StartCoroutine(ShakeTaskList());
        });
    }

    private void FixedUpdate()
    {
        if (!tasklist_bg.IsActive()) return;

        countDownFlip--;
        if (countDownFlip <= 0)
        {
            countDownFlip = 75;
            posflipped = !posflipped;

            RectTransform rt = tasklist_bg.rectTransform;
            RectTransform textRT = text_tasklist.rectTransform;
            Vector2 offset = posflipped ? new Vector2(0, 0.5f) : new Vector2(0, -0.5f);

            rt.anchoredPosition = basePos + offset;
            textRT.anchoredPosition = basePosText + offset;
        }
    }

    // ============================= SHAKE =============================

    private IEnumerator ShakeTaskList(float stepDuration = 0.04f)
    {
        float[] offsets = { 1.8f, -1.8f, 0.9f, -0.9f, 0.4f, -0.4f, 0f };

        RectTransform bgRT = tasklist_bg.rectTransform;
        RectTransform textRT = text_tasklist.rectTransform;

        foreach (float y in offsets)
        {
            bgRT.anchoredPosition = basePos + new Vector2(0, y);
            textRT.anchoredPosition = basePosText + new Vector2(0, y);
            yield return new WaitForSeconds(stepDuration);
        }

        bgRT.anchoredPosition = basePos;
        textRT.anchoredPosition = basePosText;
    }

    // ============================= PUBLIC API =============================

    public void SetTextDisplay(string text)
    {
        textToDisplay = text;
    }

    public void AnimateInNewTask()
    {
        RequestState(UIState.Revealing);
    }

    public void HideDisplay()
    {
        RequestState(UIState.Hiding);
    }

    public void ShowTaskList()
    {
        canvas_full_list.gameObject.SetActive(true);
    }

    public void HideTaskList()
    {
        canvas_full_list.gameObject.SetActive(false);
    }

    // ============================= STATE MACHINE =============================

    private void RequestState(UIState newState)
    {
        animationToken++;
        state = newState;

        if (driverCoroutine != null)
            StopCoroutine(driverCoroutine);

        driverCoroutine = StartCoroutine(StateMachineDriver(animationToken));
    }

    private IEnumerator StateMachineDriver(int token)
    {
        while (true)
        {
            if (token != animationToken) yield break;

            switch (state)
            {
                case UIState.Revealing:
                    yield return RevealAnimation(token);
                    if (token != animationToken) yield break;
                    state = UIState.Typing;
                    break;

                case UIState.Typing:
                    yield return TypewriterAnimation(token);
                    if (token != animationToken) yield break;
                    state = UIState.IdleVisible;
                    break;

                case UIState.IdleVisible:
                    yield break;

                case UIState.Hiding:
                    yield return HideAnimation(token);
                    if (token != animationToken) yield break;
                    state = UIState.Hidden;
                    yield break;

                case UIState.Hidden:
                default:
                    tasklist_bg.gameObject.SetActive(false);
                    text_tasklist.text = "";
                    yield break;
            }
        }
    }

    // ============================= ANIMATIONS =============================

    private IEnumerator RevealAnimation(int token)
    {
        ForceVisible();
        text_tasklist.text = "";
        PlaySound(whoosh_in);

        RectTransform bg = tasklist_bg.rectTransform;
        float targetWidth = bg.sizeDelta.x;
        float height = bg.sizeDelta.y;

        if (!gotMask)
        {
            gotMask = true;
            mask = tasklist_bg.transform.parent.GetComponent<RectTransform>();
        }

        mask.sizeDelta = new Vector2(0, height);

        float t = 0f;
        while (t < revealDuration)
        {
            if (token != animationToken) yield break;

            t += Time.deltaTime;
            float eased = Mathf.SmoothStep(0f, 1f, t / revealDuration);
            float w = Mathf.Lerp(0f, targetWidth, eased);
            mask.sizeDelta = new Vector2(w, height);

            yield return null;
        }

        mask.sizeDelta = new Vector2(targetWidth, height);
    }

    private IEnumerator TypewriterAnimation(int token)
    {
        text_tasklist.text = "";
        PlaySound(scribble_sfx);
        yield return new WaitForSeconds(0.22f);

        bool shouldWait = false;

        for (int i = 0; i <= textToDisplay.Length; i++)
        {
            if (token != animationToken) yield break;

            text_tasklist.text = textToDisplay.Substring(0, i);

            if (shouldWait)
                yield return new WaitForSeconds(typewriterDelay);

            shouldWait = !shouldWait;
        }
    }

    private IEnumerator HideAnimation(int token)
    {
        float t = 0;
        float startAlpha = tasklist_bg.color.a;
        while (t < hideDuration)
        {
            if (token != animationToken) yield break;

            t += Time.deltaTime;
            float eased = Mathf.SmoothStep(0f, 1f, t / hideDuration);
            float a = Mathf.Lerp(startAlpha, 0f, eased);

            tasklist_bg.color = new Color(1, 1, 1, a);
            text_tasklist.color = new Color(1, 1, 1, a);

            yield return null;
        }

        tasklist_bg.color = new Color(1, 1, 1, 0);
        text_tasklist.color = new Color(1, 1, 1, 0);
        tasklist_bg.gameObject.SetActive(false);
    }

    // ============================= HELPERS =============================

    private void ForceVisible()
    {
        tasklist_bg.gameObject.SetActive(true);
        tasklist_bg.color = Color.white;
        text_tasklist.color = Color.white;
    }

    private void PlaySound(AudioClip clip)
    {
        if (!clip || !AudioManager.Instance) return;
        AudioManager.Instance.AudioSource.PlayOneShot(clip);
    }
    
    private void OnDestroy()
    {
        changeQuestTextHandle?.Unsubscribe();
    }
}

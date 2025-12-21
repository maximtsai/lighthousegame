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

    [Header("Animation Settings")]
    [SerializeField] float revealDuration = 0.6f;
    [SerializeField] float typewriterDelay = 0.06f;
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

    void Start()
    {
        basePos = tasklist_bg.rectTransform.anchoredPosition;
        basePosText = text_tasklist.rectTransform.anchoredPosition;
    }

    private void FixedUpdate()
    {
        if (tasklist_bg.IsActive())
        {
            countDownFlip--;
            if (countDownFlip <= 0)
            {
                countDownFlip = 75;
                posflipped = !posflipped;
                RectTransform rt = tasklist_bg.rectTransform;
                RectTransform textRT = text_tasklist.rectTransform;
                if (posflipped)
                {
                    rt.anchoredPosition = basePos + new Vector2(0, 0.5f);
                    textRT.anchoredPosition = basePosText + new Vector2(0, 0.5f);
                }
                else
                {
                    rt.anchoredPosition = basePos + new Vector2(0, -0.5f);
                    textRT.anchoredPosition = basePosText + new Vector2(0, -0.5f);
                }
            }
        }
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
        // GameState.Set("task_list_open", true); // no longer needed maybe
    }

    public void HideTaskList()
    {
        canvas_full_list.gameObject.SetActive(false);
        // GameState.Set("task_list_open", false);
    }

    // ============================= CORE STATE MACHINE =============================

    private void RequestState(UIState newState)
    {
        animationToken++; // invalidate all running animations
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
                    yield break; // Nothing to do; wait for next request

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

    // ============================= ANIMATION ROUTINES =============================

    private IEnumerator RevealAnimation(int token)
    {
        tasklist_bg.gameObject.SetActive(true);
        text_tasklist.text = "";
        text_tasklist.color = new Color(1,1,1,1);

        // Play sound
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
        yield return new WaitForSeconds(0.23f);

        for (int i = 0; i <= textToDisplay.Length; i++)
        {
            if (token != animationToken) yield break;

            text_tasklist.text = textToDisplay.Substring(0, i);

            yield return new WaitForSeconds(typewriterDelay);
        }
    }

    private IEnumerator HideAnimation(int token)
    {
        Color startC = tasklist_bg.color;
        float t = 0;

        while (t < hideDuration)
        {
            if (token != animationToken) yield break;

            t += Time.deltaTime;
            float eased = Mathf.SmoothStep(0f, 1f, t / hideDuration);
            float a = Mathf.Lerp(1f, 0f, eased);

            tasklist_bg.color = new Color(1, 1, 1, a);
            text_tasklist.color = new Color(1, 1, 1, a);

            yield return null;
        }

        tasklist_bg.color = new Color(1,1,1,0);
        text_tasklist.color = new Color(1,1,1,0);
        tasklist_bg.gameObject.SetActive(false);
    }

    // ============================= AUDIO =============================

    private void PlaySound(AudioClip clip)
    {
        if (!clip) return;
        if (!AudioManager.Instance) return;

        var src = AudioManager.Instance.AudioSource;
        src.PlayOneShot(clip);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The close-up of the hole on climb floor 4 with the day 3 scissors stuck in it. Clicking
/// anywhere works them loose, then the MC finds them broken and patches them up as best they
/// can, then it fades back to the stairs.
/// The collider on this object covers the screen so every click lands here.
/// </summary>
public class ScissorsCloseUp : MonoBehaviour
{
    // Scaled and moved as one piece, so its position is the middle of the scissors art.
    [SerializeField] private Transform scissorsPivot;
    [SerializeField] private SpriteRenderer scissorsRenderer;
    [SerializeField] private AudioClip wiggleSound;
    [SerializeField] private AudioClip pullSound;

    [Header("Extraction")]
    [SerializeField] private int wiggles = 2;
    [SerializeField] private float wiggleAngle = 5f;
    [SerializeField] private float wiggleDuration = 0.14f;
    [SerializeField] private float pullDuration = 0.4f;
    [SerializeField] private Vector3 pullOffset = new Vector3(-0.35f, -0.6f, 0f);
    [SerializeField] private float pullScale = 1.3f;

    private MiscObjectClick miscObjectClick;
    private Vector3 restPosition;
    private Vector3 restScale;
    private bool restCaptured;
    private bool pulling;

    void Awake()
    {
        miscObjectClick = FindFirstObjectByType<MiscObjectClick>();
        CaptureRest();
    }

    void OnEnable()
    {
        CaptureRest();
        pulling = false;
        scissorsPivot.localPosition = restPosition;
        scissorsPivot.localRotation = Quaternion.identity;
        scissorsPivot.localScale = restScale;
        scissorsRenderer.color = Color.white;
        scissorsRenderer.gameObject.SetActive(true);
    }

    private void CaptureRest()
    {
        if (restCaptured || scissorsPivot == null)
            return;

        restPosition = scissorsPivot.localPosition;
        restScale = scissorsPivot.localScale;
        restCaptured = true;
    }

    void OnMouseOver()
    {
        if (CanClick())
            CustomCursor.SetCursorToPointer();
    }

    void OnMouseExit()
    {
        CustomCursor.SetCursorToNormal();
    }

    void OnMouseUp()
    {
        if (!CanClick())
            return;

        pulling = true;
        CustomCursor.SetCursorToNormal();
        StartCoroutine(PullOut());
    }

    private bool CanClick()
    {
        return !pulling
            && LighthouseScissors.Stuck
            && !GameState.Get<bool>("navigationBlocked", false)
            && !GameState.Get<bool>("task_list_open", false)
            && !DialogueManager.DialogueIsOpen();
    }

    private IEnumerator PullOut()
    {
        PlaySound(wiggleSound);
        for (int i = 0; i < wiggles; i++)
        {
            yield return Rotate(0f, wiggleAngle, wiggleDuration * 0.5f);
            yield return Rotate(wiggleAngle, -wiggleAngle, wiggleDuration);
            yield return Rotate(-wiggleAngle, 0f, wiggleDuration * 0.5f);
        }

        // Out toward the viewer: it drops, grows, and fades before it reaches full size.
        PlaySound(pullSound);
        float t = 0f;
        while (t < pullDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / pullDuration);
            float eased = 1f - (1f - p) * (1f - p);
            scissorsPivot.localPosition = restPosition + pullOffset * eased;
            scissorsPivot.localScale = restScale * Mathf.Lerp(1f, pullScale, eased);
            scissorsRenderer.color = new Color(1f, 1f, 1f, 1f - Mathf.Clamp01((p - 0.35f) / 0.65f));
            yield return null;
        }

        scissorsRenderer.gameObject.SetActive(false);
        LighthouseScissors.PullOut();

        yield return new WaitForSeconds(0.25f);

        Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
        dialogue.text = new List<string>(new string[]
        {
            "How did the scissors end up all the way in there?",
            "It's like something was trying to drag them into the wall.",
            "...The blades have come loose at the pivot.",
            "I try to tighten the bolt, but the nut won't catch.",
            "There.## That's the best I can do."
        });
        dialogue.choices = new List<string>();
        dialogue.consequences = new List<UnityEngine.Events.UnityEvent>();
        dialogue.onDialogueEnd = new UnityEngine.Events.UnityEvent();
        dialogue.onDialogueEndImmediate = new UnityEngine.Events.UnityEvent();
        dialogue.onDialogueEnd.AddListener(Navigation.CloseScissorsCloseUp);
        DialogueManager.ShowDialogue(dialogue);
    }

    private IEnumerator Rotate(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float angle = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            scissorsPivot.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && miscObjectClick != null)
            miscObjectClick.PlaySound(clip);
    }
}

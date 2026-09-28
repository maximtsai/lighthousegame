using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The close-up of the hole on climb floor 4 with the day 3 scissors stuck in it. Clicking
/// anywhere works them loose, then a line of dialogue, then the close-up fades away to the
/// stairs, where the way up is open again.
/// The collider on this object covers the screen so every click lands here. Hovering the
/// scissors themselves (their drawn outline, not the whole image) swaps in the highlighted art,
/// purely for looks.
/// </summary>
public class ScissorsCloseUp : MonoBehaviour
{
    // Scaled and moved as one piece, so its position is the middle of the scissors art.
    [SerializeField] private Transform scissorsPivot;
    [SerializeField] private SpriteRenderer scissorsRenderer;
    [SerializeField] private AudioClip wiggleSound;
    [SerializeField] private AudioClip pullSound;
    [SerializeField] private Sprite hoverSprite;

    [Header("Extraction")]
    [SerializeField] private int wiggles = 2;
    [SerializeField] private float wiggleAngle = 5f;
    [SerializeField] private float wiggleDuration = 0.14f;
    [SerializeField] private float pullDuration = 0.4f;
    [SerializeField] private Vector3 pullOffset = new Vector3(-0.35f, -0.6f, 0f);
    [SerializeField] private float pullScale = 1.3f;

    [Header("Closing")]
    [SerializeField] private float fadeAwayDuration = 0.6f;

    private MiscObjectClick miscObjectClick;
    private Vector3 restPosition;
    private Vector3 restScale;
    private bool restCaptured;
    private bool pulling;
    private Sprite normalSprite;
    private readonly List<List<Vector2>> outline = new List<List<Vector2>>();

    void Awake()
    {
        miscObjectClick = FindFirstObjectByType<MiscObjectClick>();
        CaptureRest();
        CaptureOutline();
    }

    // The scissors' own shape, from the physics shape Unity traces around the drawn pixels.
    private void CaptureOutline()
    {
        normalSprite = scissorsRenderer.sprite;
        if (normalSprite == null)
            return;

        for (int i = 0; i < normalSprite.GetPhysicsShapeCount(); i++)
        {
            List<Vector2> shape = new List<Vector2>();
            normalSprite.GetPhysicsShape(i, shape);
            outline.Add(shape);
        }
    }

    void Update()
    {
        if (hoverSprite == null || normalSprite == null || !scissorsRenderer.gameObject.activeSelf)
            return;

        Sprite target = !pulling && CanClick() && MouseOverScissors() ? hoverSprite : normalSprite;
        if (scissorsRenderer.sprite != target)
            scissorsRenderer.sprite = target;
    }

    private bool MouseOverScissors()
    {
        if (Camera.main == null)
            return false;

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 local = scissorsRenderer.transform.InverseTransformPoint(world);

        // No traced shape to go on, so fall back to the trimmed sprite's box.
        if (outline.Count == 0)
            return normalSprite.bounds.Contains(new Vector3(local.x, local.y, normalSprite.bounds.center.z));

        foreach (List<Vector2> shape in outline)
        {
            if (InsidePolygon(local, shape))
                return true;
        }
        return false;
    }

    private static bool InsidePolygon(Vector2 point, List<Vector2> polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[j];
            if ((a.y > point.y) != (b.y > point.y)
                && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
            {
                inside = !inside;
            }
        }
        return inside;
    }

    void OnEnable()
    {
        CaptureRest();
        pulling = false;
        scissorsPivot.localPosition = restPosition;
        scissorsPivot.localRotation = Quaternion.identity;
        scissorsPivot.localScale = restScale;
        scissorsRenderer.color = Color.white;
        if (normalSprite != null)
            scissorsRenderer.sprite = normalSprite;
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
        if (normalSprite != null)
            scissorsRenderer.sprite = normalSprite;
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

        Dialogue dialogue = DialogueManager.ShowDialogueFromText(new string[]
        {
            "How did the scissors end up all the way in there?"
        });
        dialogue.onDialogueEnd.AddListener(() => StartCoroutine(FadeAway()));
    }

    // The close-up fades out over the stairs rather than cutting through black, then the stairs'
    // own arrows come back with the way up no longer blocked.
    private IEnumerator FadeAway()
    {
        GameState.Set("navigationBlocked", true);

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        Color[] startColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            startColors[i] = renderers[i].color;

        float t = 0f;
        while (t < fadeAwayDuration)
        {
            t += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(t / fadeAwayDuration);
            for (int i = 0; i < renderers.Length; i++)
            {
                Color c = startColors[i];
                c.a *= alpha;
                renderers[i].color = c;
            }
            yield return null;
        }

        // Put the colors back for the next time it opens; it's about to be switched off.
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = startColors[i];

        GameState.Set("navigationBlocked", false);
        Navigation.CloseScissorsCloseUp();
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

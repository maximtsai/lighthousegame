using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The fish-peeling minigame inside Day 5.
//
// Picking up the fish dims the mermaid under a dark overlay and brings the fish up in the middle of
// the screen. There's one skin to peel, and each click takes it one stage further, peel 1 through
// peel 7, until it lies flat on top of the bare fish. Then the fish and the overlay go away, and
// the tray swaps the fish for the peeled skin.
//
// The fish base and every peel stage are drawn on one shared 640x374 canvas, so they all sit at
// local (0,0) and line up by themselves. Coming and going, the fish fades as one combined picture
// instead, so the layers can't show through each other halfway.
public class MermaidFishPeel : MermaidTarget
{
    [Header("Layers")]
    [Tooltip("The skin, drawn over the fish at whatever stage the peel has reached.")]
    [SerializeField] private SpriteRenderer skinRenderer;
    [Tooltip("Dark overlay that dims the mermaid while the fish is up.")]
    [SerializeField] private SpriteRenderer overlay;

    [Header("Art")]
    [Tooltip("peel 1 (skin on) through peel 7 (skin off, lying flat), in order. One click per stage.")]
    [SerializeField] private Sprite[] peelFrames;
    [Tooltip("The fish base with peel 1 on it, as one picture, for fading in.")]
    [SerializeField] private Sprite wholeFish;
    [Tooltip("The fish base with peel 7 on it, as one picture, for fading out.")]
    [SerializeField] private Sprite peeledFish;

    [Header("Timing")]
    [SerializeField] private float showFadeDuration = 0.3f;
    [Tooltip("How long the peeled skin stays in view before the fish goes away.")]
    [SerializeField] private float finishHold = 0.6f;

    [Header("Progress")]
    [SerializeField] private int stage;

    private bool animating;
    private bool onScreen;
    private Camera cam;

    public override bool IsBusy => animating;

    private int LastStage => peelFrames != null ? peelFrames.Length - 1 : 0;

    public override bool CanApply(MermaidTool tool)
    {
        return tool == MermaidTool.Fish && stage < LastStage;
    }

    public override int RemainingSteps(MermaidTool tool)
    {
        return tool == MermaidTool.Fish ? Mathf.Max(0, LastStage - stage) : 0;
    }

    public override void Apply(MermaidTool tool)
    {
        stage++;
        if (skinRenderer != null) skinRenderer.sprite = peelFrames[stage];

        if (stage == LastStage) StartCoroutine(HoldFinishedPeel());
    }

    public override void OnToolReady()
    {
        onScreen = true;
        FollowCamera();
        if (skinRenderer != null && peelFrames != null && peelFrames.Length > 0)
            skinRenderer.sprite = peelFrames[stage];

        StartCoroutine(Arrive());
    }

    // No peeling until it's all on screen, or the peel and the fade fight over the skin.
    private IEnumerator Arrive()
    {
        animating = true;
        yield return FadeAsOnePicture(wholeFish, 0f, 1f);
        animating = false;
    }

    // Busy for a moment, so the phase doesn't whisk the fish away the instant the skin comes off.
    private IEnumerator HoldFinishedPeel()
    {
        animating = true;
        yield return new WaitForSeconds(finishHold);
        animating = false;
    }

    public override void OnPhaseFinished()
    {
        StartCoroutine(LeaveScreen());
    }

    private IEnumerator LeaveScreen()
    {
        yield return FadeAsOnePicture(peeledFish, 1f, 0f);
        onScreen = false;
    }

    // Faded as two layers, the bare fish would show through the half-faded skin. So while it fades,
    // the skin layer shows the whole fish as one picture and the base underneath is hidden. At full
    // strength the picture and the layers look exactly alike, so swapping back can't be seen.
    private IEnumerator FadeAsOnePicture(Sprite picture, float from, float to)
    {
        if (picture == null)
        {
            yield return Fade(from, to, spriteRenderer, skinRenderer, overlay);
            yield break;
        }

        Sprite layer = skinRenderer.sprite;
        skinRenderer.sprite = picture;
        SetAlpha(spriteRenderer, 0f);

        yield return Fade(from, to, skinRenderer, overlay);

        skinRenderer.sprite = layer;
        SetAlpha(spriteRenderer, to);
    }

    // Keep the fish in the middle of the view even if the camera drifts while it's up.
    void LateUpdate()
    {
        if (onScreen) FollowCamera();
    }

    private void FollowCamera()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        Vector3 c = cam.transform.position;
        transform.position = new Vector3(c.x, c.y, transform.position.z);
    }

    private IEnumerator Fade(float from, float to, params SpriteRenderer[] renderers)
    {
        var fading = new List<SpriteRenderer>();
        foreach (SpriteRenderer r in renderers)
        {
            if (r != null) fading.Add(r);
        }

        float elapsed = 0f;
        while (elapsed < showFadeDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / showFadeDuration));
            foreach (SpriteRenderer r in fading) SetAlpha(r, a);
            yield return null;
        }

        foreach (SpriteRenderer r in fading) SetAlpha(r, to);
    }

    protected override void SetHover(bool on)
    {
        base.SetHover(on);

        // The skin covers most of the fish, so tint it too or the hover barely shows.
        if (skinRenderer == null) return;
        Color tint = on ? hoverTint : Color.white;
        skinRenderer.color = new Color(tint.r, tint.g, tint.b, skinRenderer.color.a);
    }

    private static void SetAlpha(SpriteRenderer r, float a)
    {
        Color c = r.color;
        r.color = new Color(c.r, c.g, c.b, a);
    }
}

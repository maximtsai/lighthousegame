using System.Collections;
using UnityEngine;

// One bandage patch, laid over the finished wounds in the last phase.
//
// Bandages are their own objects rather than a fourth stage on MermaidSpot because bandage 1
// covers two sites at once, and four of the sites never get bandaged at all.
//
// A patch sits in the scene with its sprite assigned but the renderer at alpha 0, so what's
// saved is already the right starting state and there's nothing to reset when the game starts.
public class MermaidBandage : MermaidTarget
{
    [Header("Stage")]
    [SerializeField] private Sprite bandageSprite;

    [Header("Progress")]
    [SerializeField] private bool applied;

    [Header("Fade In")]
    [SerializeField] private float fadeInDuration = 0.22f;

    [Header("Hover")]
    [Tooltip("An unapplied patch is invisible, so hovering ghosts it in to show where it lands.")]
    [SerializeField] private float hoverPreviewAlpha = 0.35f;

    public override bool CanApply(MermaidTool tool)
    {
        return tool == MermaidTool.Bandages && !applied;
    }

    public override void Apply(MermaidTool tool)
    {
        applied = true;
        StartCoroutine(FadeIn());
    }

    public override int RemainingSteps(MermaidTool tool)
    {
        return tool == MermaidTool.Bandages && !applied ? 1 : 0;
    }

    protected override void SetHover(bool on)
    {
        if (spriteRenderer == null) return;

        if (applied)
        {
            base.SetHover(on);
            return;
        }

        // The base class keeps whatever alpha is there, which would leave an unapplied patch
        // invisible and you clicking blind. Ghost it in instead.
        spriteRenderer.color = new Color(1f, 1f, 1f, on ? hoverPreviewAlpha : 0f);
    }

    private IEnumerator FadeIn()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.sprite = bandageSprite;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            spriteRenderer.color = new Color(1f, 1f, 1f, t);
            yield return null;
        }

        spriteRenderer.color = Color.white;
    }
}

using System.Collections;
using UnityEngine;

// One wound site on the mermaid's body. It runs through three stages:
//
//     barnacle  ->  wound (gloves pick the barnacle off)  ->  sutures (fish bones, 2 or 3 passes)
//
// Alcohol sits between the wound and the sutures and doesn't change the sprite at all, it just
// flashes. The stomach gash is the odd one out: it's exposed from the start, so it has no wound
// sprite and gloves skip it entirely.
public class MermaidSpot : MermaidTarget
{
    [Header("Stages")]
    [Tooltip("Barnacle, or the stomach gash. Whatever the site starts on.")]
    [SerializeField] private Sprite startSprite;
    [Tooltip("What the gloves reveal. Leave empty for the gash, which has no barnacle to remove.")]
    [SerializeField] private Sprite woundSprite;
    [Tooltip("Applied in order by the fish bones. Empty for sites that never get stitched.")]
    [SerializeField] private Sprite[] sutureStages;

    [Header("Progress")]
    [SerializeField] private bool barnacleRemoved;
    [SerializeField] private bool disinfected;
    [SerializeField] private int sutureStage;

    [Header("Alcohol Flash")]
    [SerializeField] private Color alcoholFlash = new Color(0.72f, 0.9f, 1f, 1f);
    [SerializeField] private float alcoholFlashDuration = 0.35f;

    private Coroutine flashRoutine;

    // False for the stomach gash, which is exposed from the start.
    private bool HasBarnacle => woundSprite != null;
    private bool HasSutures => sutureStages != null && sutureStages.Length > 0;
    private bool Cleaned => barnacleRemoved || !HasBarnacle;

    public override bool CanApply(MermaidTool tool)
    {
        switch (tool)
        {
            case MermaidTool.Gloves:
                return HasBarnacle && !barnacleRemoved;
            case MermaidTool.Alcohol:
                return Cleaned && !disinfected;
            case MermaidTool.FishBones:
                return Cleaned && disinfected && HasSutures && sutureStage < sutureStages.Length;
            default:
                return false;
        }
    }

    public override void Apply(MermaidTool tool)
    {
        switch (tool)
        {
            case MermaidTool.Gloves:
                barnacleRemoved = true;
                if (spriteRenderer != null) spriteRenderer.sprite = woundSprite;
                break;

            case MermaidTool.Alcohol:
                disinfected = true;
                if (flashRoutine != null) StopCoroutine(flashRoutine);
                flashRoutine = StartCoroutine(AlcoholFlash());
                break;

            case MermaidTool.FishBones:
                if (spriteRenderer != null) spriteRenderer.sprite = sutureStages[sutureStage];
                sutureStage++;
                break;
        }
    }

    public override int RemainingSteps(MermaidTool tool)
    {
        switch (tool)
        {
            case MermaidTool.Gloves:
                return HasBarnacle && !barnacleRemoved ? 1 : 0;
            case MermaidTool.Alcohol:
                return disinfected ? 0 : 1;
            case MermaidTool.FishBones:
                // Counted before the phase starts, so ignore the disinfect prerequisite here.
                return HasSutures ? sutureStages.Length - sutureStage : 0;
            default:
                return 0;
        }
    }

    // Snap to a cold alcohol tint, then ease back to normal.
    private IEnumerator AlcoholFlash()
    {
        if (spriteRenderer == null) yield break;

        float alpha = spriteRenderer.color.a;
        float elapsed = 0f;

        while (elapsed < alcoholFlashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / alcoholFlashDuration;
            Color c = Color.Lerp(alcoholFlash, Color.white, t * t * (3f - 2f * t));
            spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
        flashRoutine = null;
    }
}

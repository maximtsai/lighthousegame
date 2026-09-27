using System.Collections;
using UnityEngine;

// Something on the body that a tool takes away. On Day 5 that's the pus (alcohol) and the dirty
// bandages (gloves). It starts visible and fades out when treated.
public class MermaidRemovable : MermaidTarget
{
    [Header("Tool")]
    [SerializeField] private MermaidTool removedWith = MermaidTool.Gloves;

    [Header("Progress")]
    [SerializeField] private bool removed;

    [Header("Fade Out")]
    [SerializeField] private float fadeOutDuration = 0.25f;

    private bool fading;

    public override bool IsBusy => fading;

    public override bool CanApply(MermaidTool tool)
    {
        return tool == removedWith && !removed;
    }

    public override void Apply(MermaidTool tool)
    {
        removed = true;
        StartCoroutine(FadeOut());
    }

    public override int RemainingSteps(MermaidTool tool)
    {
        return tool == removedWith && !removed ? 1 : 0;
    }

    private IEnumerator FadeOut()
    {
        if (spriteRenderer == null) yield break;

        fading = true;
        Color start = spriteRenderer.color;

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            spriteRenderer.color = new Color(start.r, start.g, start.b, start.a * (1f - t));
            yield return null;
        }

        spriteRenderer.enabled = false;
        fading = false;
    }
}

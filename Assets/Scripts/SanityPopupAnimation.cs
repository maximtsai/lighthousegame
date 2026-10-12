using System.Collections;
using TMPro;
using UnityEngine;

public static class SanityPopupAnimation
{
    public static IEnumerator Play(TextMeshProUGUI label, string text, Color color,
        RectTransform anchor, float offset, float rise, float duration)
    {
        label.gameObject.SetActive(true);
        label.text = text;
        color.a = 1f;
        label.color = color;
        RectTransform rect = label.rectTransform;
        Vector3 start = (anchor != null ? anchor.position : rect.position) + Vector3.up * offset;
        rect.position = start;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.position = start + Vector3.up * (rise * t);
            color.a = 1f - t;
            label.color = color;
            yield return null;
        }
        label.gameObject.SetActive(false);
    }
}

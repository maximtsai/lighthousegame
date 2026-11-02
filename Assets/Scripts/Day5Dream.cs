using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Day 5's dream, played over the bedroom before he wakes: she's asleep beside him, peaceful, and
/// then she's a rotting corpse. The images come in as plain sprites (BedroomMiscLogic), so this
/// works with placeholder art and takes the final art without code changes.
/// </summary>
public class Day5Dream : MonoBehaviour
{
    // Above Navigation's blackout (10) so the new-day fade-in happens unseen underneath, and under
    // the dialogue box (100) so the dream's line still shows
    private const int SortingOrder = 90;
    private const string ScareSound = "camborne_jumpscare";

    private Image black;
    private Image sleeping;
    private Image corpse;
    private Action onWake;

    public static void Play(Sprite sleepingSprite, Sprite corpseSprite, Action onWake)
    {
        GameObject root = new GameObject("Day5Dream");
        Day5Dream dream = root.AddComponent<Day5Dream>();
        dream.onWake = onWake;
        dream.Build(sleepingSprite, corpseSprite);
        dream.StartCoroutine(dream.DreamRoutine());
    }

    private void Build(Sprite sleepingSprite, Sprite corpseSprite)
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SortingOrder;
        gameObject.AddComponent<GraphicRaycaster>();

        black = CreateImage("black", null, Vector2.zero, Vector2.one);
        black.color = Color.black;
        // Swallows UI clicks while the dream is on
        black.raycastTarget = true;

        // Her asleep, in the middle of the dark
        sleeping = CreateImage("sleeping", sleepingSprite, new Vector2(0.15f, 0.2f), new Vector2(0.85f, 0.8f));
        sleeping.preserveAspect = true;
        SetAlpha(sleeping, 0f);

        // The corpse up close: as wide as the screen, top-aligned, running off the bottom
        corpse = CreateImage("corpse", corpseSprite, new Vector2(0f, 1f), new Vector2(1f, 1f));
        corpse.rectTransform.pivot = new Vector2(0.5f, 1f);
        if (corpseSprite != null)
        {
            AspectRatioFitter fitter = corpse.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            fitter.aspectRatio = corpseSprite.rect.width / corpseSprite.rect.height;
        }
        corpse.enabled = false;
    }

    private Image CreateImage(string name, Sprite sprite, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(transform, false);
        RectTransform rect = imageObject.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = imageObject.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        return image;
    }

    private IEnumerator DreamRoutine()
    {
        // Keep the room behind it from taking clicks or panning about
        GameState.Set("minigame_open", true);

        yield return new WaitForSeconds(0.8f);
        yield return Fade(sleeping, 0f, 1f, 1.5f);
        yield return new WaitForSeconds(0.8f);

        bool lineDone = false;
        Dialogue line = DialogueManager.ShowDialogueFromText(new string[] { "She looks peaceful." });
        line.onDialogueEnd.AddListener(() => lineDone = true);
        while (!lineDone)
            yield return null;

        // Let the quiet sit a moment
        yield return new WaitForSeconds(1.2f);

        sleeping.enabled = false;
        corpse.enabled = corpse.sprite != null;
        MessageBus.Instance.Publish("PlaySound", ScareSound);
        yield return Shake(corpse.rectTransform, 0.7f, 14f);

        // Snap awake
        corpse.enabled = false;
        yield return new WaitForSeconds(0.8f);
        yield return Fade(black, 1f, 0f, 1.2f);

        GameState.Set("minigame_open", false);
        Action wake = onWake;
        Destroy(gameObject);
        wake?.Invoke();
    }

    private static IEnumerator Shake(RectTransform rect, float duration, float strength)
    {
        Vector2 origin = rect.anchoredPosition;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float falloff = 1f - t / duration;
            rect.anchoredPosition = origin + UnityEngine.Random.insideUnitCircle * strength * falloff;
            yield return null;
        }
        rect.anchoredPosition = origin;
    }

    private static IEnumerator Fade(Image image, float from, float to, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            SetAlpha(image, Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetAlpha(image, to);
    }

    private static void SetAlpha(Image image, float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Day 4: what he's carrying (a body up to the grave, or her up to Camborne's bed), as a strip along
/// the bottom of the screen like looking down at it. Unlike Day4Dock's own overlay it outlives the
/// pier, since he carries them on through other scenes. It hides on the overhead outdoors view, and
/// for good once he's put them down.
/// </summary>
public class Day4CarryStrip : MonoBehaviour
{
    private static Day4CarryStrip instance;
    private Image image;
    private Image black;

    // Fades to black, lets go of what he's carrying while it's dark (whileDark), holds, and fades back.
    // The black covers the scene, the strip and the hand but not the dialogue box. Run it from a
    // coroutine: yield return Day4CarryStrip.PutDownInDark(...).
    public static IEnumerator PutDownInDark(float fade, float hold, Action whileDark)
    {
        Ensure();
        Image black = instance.black;
        black.enabled = true;
        yield return Fade(black, 0f, 1f, fade);
        whileDark?.Invoke();
        Hide();
        yield return new WaitForSeconds(hold);
        yield return Fade(black, 1f, 0f, fade);
        black.enabled = false;
    }

    public static void Show(Sprite sprite)
    {
        if (sprite == null) return;
        Ensure();
        instance.image.sprite = sprite;
        instance.image.GetComponent<AspectRatioFitter>().aspectRatio = sprite.rect.width / sprite.rect.height;
        instance.image.enabled = true;
        instance.StopAllCoroutines();
        instance.StartCoroutine(instance.FadeIn(0.35f));
    }

    public static void Hide()
    {
        if (instance == null) return;
        instance.image.sprite = null;
        instance.image.enabled = false;
    }

    // Outdoors is the overhead map, not his own eyes, so there's nothing in his arms to see there.
    // Also covers the carry ending some other way (the day reset from the menu).
    void Update()
    {
        bool visible = image.sprite != null && Day4Beach.ArmsFull
            && SceneManager.GetActiveScene().name != GameConsts.OUTDOORSSCENE;
        if (image.enabled != visible)
            image.enabled = visible;
    }

    private static void Ensure()
    {
        if (instance != null) return;

        GameObject root = new GameObject("Day4CarryStrip");
        DontDestroyOnLoad(root);
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // Above the scene, under Navigation's blackout (10) so it goes dark with the fades
        canvas.sortingOrder = 5;

        GameObject imageObject = new GameObject("carry");
        imageObject.transform.SetParent(root.transform, false);
        RectTransform rect = imageObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = imageObject.AddComponent<Image>();
        image.color = Color.white;
        image.raycastTarget = false;
        image.preserveAspect = true;
        image.enabled = false;

        AspectRatioFitter fitter = imageObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;

        // Its own canvas so it can sit above the rest of the UI (still under the dialogue box at 100)
        GameObject blackObject = new GameObject("black");
        blackObject.transform.SetParent(root.transform, false);
        Canvas blackCanvas = blackObject.AddComponent<Canvas>();
        blackCanvas.overrideSorting = true;
        blackCanvas.sortingOrder = 50;
        RectTransform blackRect = blackObject.GetComponent<RectTransform>();
        blackRect.anchorMin = Vector2.zero;
        blackRect.anchorMax = Vector2.one;
        blackRect.offsetMin = Vector2.zero;
        blackRect.offsetMax = Vector2.zero;
        Image black = blackObject.AddComponent<Image>();
        black.color = new Color(0f, 0f, 0f, 0f);
        black.raycastTarget = false;
        black.enabled = false;

        instance = root.AddComponent<Day4CarryStrip>();
        instance.image = image;
        instance.black = black;
    }

    private IEnumerator FadeIn(float duration)
    {
        yield return Fade(image, 0f, 1f, duration);
    }

    private static IEnumerator Fade(Image target, float from, float to, float duration)
    {
        Color c = target.color;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(from, to, t / duration);
            target.color = c;
            yield return null;
        }
        c.a = to;
        target.color = c;
    }
}

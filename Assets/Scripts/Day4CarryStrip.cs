using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Day 4: the body he's hauling up to the grave, as a strip along the bottom of the screen like
/// looking down at it. Unlike Day4Dock's own overlay it outlives the pier, since on the first trip
/// he carries it out through the outdoors to the burial scene. It hides on the overhead outdoors
/// view, and for good once he's put it down.
/// </summary>
public class Day4CarryStrip : MonoBehaviour
{
    private static Day4CarryStrip instance;
    private Image image;

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
        bool visible = image.sprite != null && Day4Beach.CarryingBody
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

        instance = root.AddComponent<Day4CarryStrip>();
        instance.image = image;
    }

    private IEnumerator FadeIn(float duration)
    {
        Color c = image.color;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            c.a = t / duration;
            image.color = c;
            yield return null;
        }
        c.a = 1f;
        image.color = c;
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PierMiscLogic : MonoBehaviour
{
    // The frames of one "float dock" sheet (see FloatingLayer).
    [System.Serializable]
    public class FloatSheet
    {
        public Sprite[] frames;
    }

    [SerializeField] private AudioClip finishLoop;
    [SerializeField] private AudioClip rainLoop;
    [SerializeField] private GameObject rainOverlay;
    [SerializeField] private MiscObjectClick miscObjectClick;

    [Header("Floating In The Water")]
    // The dock background the floating layers sit on.
    [SerializeField] private SpriteRenderer dockBackground;
    // "corpse 1-5 float dock": the drowned. The first four are the Day 4 bodies; all five show in
    // the Day 3 flash.
    [SerializeField] private FloatSheet[] floatingCorpses;
    [SerializeField] private FloatSheet mermaidFloat;
    // Day 4 on: wreckage from the supply ship.
    [SerializeField] private FloatSheet driftwoodFloat;

    [Header("Day 3 Corpse Flash")]
    [SerializeField] private Color redFlashColor = new Color(0.55f, 0f, 0f, 0.75f);

    [Header("Day 4 Bodies")]
    // Carrying each of the bodies to the grave, one per body.
    [SerializeField] private Sprite[] carryBodySprites;
    // Carrying her inside.
    [SerializeField] private Sprite mermaidCarrySprite;
    [SerializeField] private AudioClip shovelClip;

    private const string Day3FlashPlayedKey = "day3_pier_flash_played";

    // Both flash images share one screen-space canvas above everything else in the scene.
    private Canvas flashCanvas;

    void Start()
    {
        bool isRaining = GameState.Get<bool>("thunderstorm", false);
        if (rainOverlay != null)
        {
            rainOverlay.SetActive(isRaining);
        }
        else
        {
            foreach (Animator anim in FindObjectsByType<Animator>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (anim.gameObject.name == "rain_0")
                {
                    anim.gameObject.SetActive(isRaining);
                    break;
                }
            }
        }

        AudioClip clipToPlay = (isRaining && rainLoop != null) ? rainLoop : finishLoop;
        StartCoroutine(PlaySoundDelayedRoutine(clipToPlay, 0.35f, true, 0.001f));

        // Day 3: checking the fish traps after the lighthouse is fixed
        if (GameState.Get<int>("day") == 3
            && GameState.Get<bool>("lighthouse_fixed", false)
            && !GameState.Get<bool>("gathered_fish", false)
            && !GameState.Get<bool>(Day3FlashPlayedKey, false))
        {
            StartCoroutine(Day3CorpseFlashRoutine());
        }

        // Day 4 on: the supply ship's wreckage has started washing up
        if (GameState.Get<int>("day") >= 4 && driftwoodFloat != null)
        {
            FloatingLayer.Spawn("floating_driftwood", dockBackground, driftwoodFloat.frames);
        }

        // Day 4: the bodies from the wreck, out in the water
        if (Day4Beach.IsToday)
        {
            int count = floatingCorpses != null ? floatingCorpses.Length : 0;
            Sprite[][] bodyFrames = new Sprite[count][];
            for (int i = 0; i < count; i++)
                bodyFrames[i] = floatingCorpses[i] != null ? floatingCorpses[i].frames : null;

            gameObject.AddComponent<Day4Dock>().Init(dockBackground, bodyFrames,
                mermaidFloat != null ? mermaidFloat.frames : null,
                carryBodySprites, mermaidCarrySprite, shovelClip, miscObjectClick);
        }
    }

    private IEnumerator PlaySoundDelayedRoutine(AudioClip sfx, float volume, bool loop, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (sfx != null && miscObjectClick != null)
        {
            miscObjectClick.PlaySound(sfx, volume, loop);
        }
    }

    // Purely visual: a red flash, then a red flash into a glimpse of corpses. Doesn't block
    // clicks or touch sanity.
    private IEnumerator Day3CorpseFlashRoutine()
    {
        Image red = CreateOverlayImage("Day3RedFlash", null, redFlashColor);

        // The drowned, out in the water, hidden until the flash
        List<FloatingLayer> corpses = new List<FloatingLayer>();
        if (floatingCorpses != null)
        {
            for (int i = 0; i < floatingCorpses.Length; i++)
            {
                if (floatingCorpses[i] == null) continue;
                FloatingLayer layer = FloatingLayer.Spawn("day3_flash_corpse_" + i, dockBackground, floatingCorpses[i].frames);
                if (layer == null) continue;
                layer.Renderer.enabled = false;
                corpses.Add(layer);
            }
        }

        yield return new WaitForSeconds(1f);
        yield return FadeOutFlash(red, 0.08f, 0.35f);

        yield return new WaitForSeconds(4f);
        red.enabled = true;
        red.color = redFlashColor;
        yield return new WaitForSeconds(0.1f);
        red.enabled = false;
        if (corpses.Count > 0)
        {
            foreach (FloatingLayer layer in corpses) layer.Renderer.enabled = true;
            yield return new WaitForSeconds(0.18f);
            foreach (FloatingLayer layer in corpses) Destroy(layer.gameObject);
        }

        GameState.Set(Day3FlashPlayedKey, true);
        Destroy(red.canvas.gameObject);
    }

    private IEnumerator FadeOutFlash(Image image, float hold, float fade)
    {
        Color start = image.color;
        Color end = new Color(start.r, start.g, start.b, 0f);
        image.enabled = true;
        yield return new WaitForSeconds(hold);

        float t = 0f;
        while (t < fade)
        {
            t += Time.deltaTime;
            image.color = Color.Lerp(start, end, t / fade);
            yield return null;
        }
        image.enabled = false;
        image.color = start;
    }

    private Image CreateOverlayImage(string name, Sprite sprite, Color color)
    {
        if (flashCanvas == null)
        {
            GameObject canvasObject = new GameObject("Day3FlashCanvas");
            canvasObject.transform.SetParent(transform, false);
            flashCanvas = canvasObject.AddComponent<Canvas>();
            flashCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            flashCanvas.sortingOrder = 1000;
        }

        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(flashCanvas.transform, false);
        RectTransform rect = imageObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = imageObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        image.enabled = false;
        return image;
    }
}

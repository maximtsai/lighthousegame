using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PierMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip finishLoop;
    [SerializeField] private AudioClip rainLoop;
    [SerializeField] private GameObject rainOverlay;
    [SerializeField] private MiscObjectClick miscObjectClick;

    [Header("Day 3 Corpse Flash")]
    // Placeholder until the drowned corpses art is in: Camborne's dead body.
    [SerializeField] private Sprite corpseFlashSprite;
    [SerializeField] private Color redFlashColor = new Color(0.55f, 0f, 0f, 0.75f);

    [Header("Day 4+ Floating Wreckage")]
    // Placeholder until there's floating wreckage art: fishing junk sprites, shrunk down.
    [SerializeField] private Sprite[] floatingDebrisSprites;
    [SerializeField] private float debrisScale = 0.2f;
    [SerializeField] private float bobHeight = 0.04f;

    [Header("Day 4 Bodies")]
    // Carrying each of the bodies to the grave, one per body. Also the placeholder floating bodies.
    [SerializeField] private Sprite[] carryBodySprites;
    [SerializeField] private AudioClip shovelClip;

    private const string Day3FlashPlayedKey = "day3_pier_flash_played";

    // In the water either side of the dock, in world units with the dock background at the origin.
    // Kept out towards the horizon and the edges so the day 4 bodies have the near water.
    private static readonly Vector3[] DebrisPositions =
    {
        new Vector3(-2.9f, -0.3f, -0.05f),
        new Vector3(-1.5f, -0.4f, -0.05f),
        new Vector3(1.75f, -0.25f, -0.05f),
        new Vector3(2.95f, -0.35f, -0.05f),
        new Vector3(-1.95f, -1.1f, -0.05f),
    };

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

        // Day 4 on: things have started washing up
        if (GameState.Get<int>("day") >= 4)
        {
            SpawnFloatingDebris();
        }

        // Day 4: the bodies from the wreck, out in the water
        if (Day4Beach.IsToday)
        {
            gameObject.AddComponent<Day4Dock>().Init(carryBodySprites, shovelClip, miscObjectClick);
        }
    }

    private void SpawnFloatingDebris()
    {
        if (floatingDebrisSprites == null) return;

        for (int i = 0; i < floatingDebrisSprites.Length && i < DebrisPositions.Length; i++)
        {
            if (floatingDebrisSprites[i] == null) continue;
            GameObject piece = new GameObject("floating_debris_" + i);
            piece.transform.position = DebrisPositions[i];
            piece.transform.localScale = Vector3.one * debrisScale;
            SpriteRenderer sr = piece.AddComponent<SpriteRenderer>();
            sr.sprite = floatingDebrisSprites[i];
            StartCoroutine(BobRoutine(piece.transform, i));
        }
    }

    // Rises and falls with the waves, rocking a little, each piece out of step with the rest.
    private IEnumerator BobRoutine(Transform piece, int index)
    {
        Vector3 rest = piece.position;
        float phase = index * 1.7f;
        float speed = 1.1f + index * 0.15f;
        while (piece != null)
        {
            float t = Time.time * speed + phase;
            piece.position = rest + new Vector3(0f, Mathf.Sin(t) * bobHeight, 0f);
            piece.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.7f) * 6f);
            yield return null;
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
        Image corpse = CreateOverlayImage("Day3CorpseFlash", corpseFlashSprite, Color.white);

        yield return new WaitForSeconds(1f);
        yield return FadeOutFlash(red, 0.08f, 0.35f);

        yield return new WaitForSeconds(4f);
        red.enabled = true;
        red.color = redFlashColor;
        yield return new WaitForSeconds(0.1f);
        red.enabled = false;
        if (corpseFlashSprite != null)
        {
            corpse.enabled = true;
            yield return new WaitForSeconds(0.18f);
            corpse.enabled = false;
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

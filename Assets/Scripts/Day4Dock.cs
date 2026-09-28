using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Day 4 at the dock: the bodies from the wreck are floating in the water. Clicking one carries
/// it off to the mass grave, with the usual "Scratch?" after. One of them is a bride.
/// </summary>
public class Day4Dock : MonoBehaviour
{
    private const string SeenKey = "day4_dock_bodies_seen";
    private const string BodyBuriedKeyPrefix = "day4_body_buried_";
    private const int BrideIndex = 2;

    // Where each body floats, as the middle of the body itself, clear of the dock and fish trap.
    private static readonly Vector3[] BodyPositions =
    {
        new Vector3(-2.45f, -1.45f, -0.06f),
        new Vector3(2.4f, -1.35f, -0.06f),
        new Vector3(-2.6f, -0.75f, -0.06f),
        new Vector3(2.5f, -0.65f, -0.06f),
    };

    // Placeholder floating bodies, until there's floating corpse art: the carrying images, shrunk
    // down. The body runs along the bottom of those, so the sprite sits above where it floats.
    private const float BodyScale = 0.25f;
    private static readonly Vector3 BodyInSprite = new Vector3(0f, -1.2f, 0f);
    private static readonly Vector2 BodyColliderSize = new Vector2(6.4f, 1.3f);
    private const float BobHeight = 0.035f;

    private Sprite[] carrySprites;
    private AudioClip shovelClip;
    private MiscObjectClick miscObjectClick;

    private Canvas overlayCanvas;
    private Image blackImage;
    private Image carryImage;
    private bool busy;

    public void Init(Sprite[] carrySprites, AudioClip shovelClip, MiscObjectClick miscObjectClick)
    {
        this.carrySprites = carrySprites;
        this.shovelClip = shovelClip;
        this.miscObjectClick = miscObjectClick;
    }

    void Start()
    {
        if (!Day4Beach.Surveyed || Day4Beach.Buried >= Day4Beach.BodyCount)
            return;

        for (int i = 0; i < Day4Beach.BodyCount && i < BodyPositions.Length; i++)
        {
            if (!GameState.Get<bool>(BodyBuriedKeyPrefix + i, false))
                SpawnBody(i);
        }

        if (!GameState.Get<bool>(SeenKey, false))
            StartCoroutine(FirstSightRoutine());
    }

    private IEnumerator FirstSightRoutine()
    {
        yield return new WaitForSeconds(0.6f);
        GameState.Set(SeenKey, true);
        MessageBus.Instance.Publish("FloatText", 0f, 0.3f, "-SANITY", "purple");
        MessageBus.Instance.Publish("PlusSanity", -10);
        DialogueManager.ShowDialogueFromText(new string[]
        {
            "Bodies.## Floating in the water.",
            "The tide keeps bringing them in.",
            "I can't leave them like this."
        });
    }

    private void SpawnBody(int index)
    {
        GameObject body = new GameObject("day4_floating_body_" + index);
        body.transform.SetParent(transform, false);
        body.transform.position = BodyPositions[index] - BodyInSprite * BodyScale;
        body.transform.localScale = Vector3.one * BodyScale;

        SpriteRenderer sr = body.AddComponent<SpriteRenderer>();
        sr.sprite = CarrySprite(index);

        BoxCollider2D box = body.AddComponent<BoxCollider2D>();
        box.size = BodyColliderSize;
        box.offset = BodyInSprite;

        body.AddComponent<Day4ClickTarget>().onClick = () => OnBodyClicked(index, body);
        StartCoroutine(BobRoutine(body.transform, index));
    }

    private Sprite CarrySprite(int index)
    {
        if (carrySprites == null || carrySprites.Length == 0) return null;
        return carrySprites[index % carrySprites.Length];
    }

    private void OnBodyClicked(int index, GameObject body)
    {
        if (busy) return;

        if (index == BrideIndex)
        {
            Dialogue d = DialogueManager.ShowDialogueFromText(new string[]
            {
                "Her dress.##.##.## a wedding dress.",
                "She died on her wedding day."
            });
            d.onDialogueEnd.AddListener(() => StartCoroutine(CarryAndBuryRoutine(index, body)));
            return;
        }

        StartCoroutine(CarryAndBuryRoutine(index, body));
    }

    private IEnumerator CarryAndBuryRoutine(int index, GameObject body)
    {
        busy = true;
        GameState.Set("navigationBlocked", true);
        EnsureOverlay();

        // Haul them out of the water
        Sprite carry = CarrySprite(index);
        carryImage.sprite = carry;
        carryImage.enabled = carry != null;
        yield return Fade(carryImage, 0f, 1f, 0.35f);

        // Drag them up to the grave until you click
        yield return null;
        while (!Input.GetMouseButtonDown(0))
            yield return null;

        // Into the mass grave
        blackImage.enabled = true;
        yield return Fade(blackImage, 0f, 1f, 0.6f);
        carryImage.enabled = false;
        Destroy(body);
        if (shovelClip != null && miscObjectClick != null)
            miscObjectClick.PlaySound(shovelClip, 0.6f);
        yield return new WaitForSeconds(1.6f);

        GameState.Set(BodyBuriedKeyPrefix + index, true);
        int buried = GameState.Increment(Day4Beach.BuriedKey);
        yield return Fade(blackImage, 1f, 0f, 0.6f);
        blackImage.enabled = false;

        GameState.Set("navigationBlocked", false);
        busy = false;

        HandScratch.Prompt("Scratch?", () =>
        {
            if (buried >= Day4Beach.BodyCount)
            {
                MessageBus.Instance.Publish("CompleteTask", "task_bury_bodies");
                DialogueManager.ShowDialogueFromText(new string[] { "That's all of them.## All in one grave." });
            }
        });
    }

    // Rises and falls with the waves, each body out of step with the rest.
    private IEnumerator BobRoutine(Transform body, int index)
    {
        Vector3 rest = body.position;
        float phase = index * 2.1f;
        float speed = 0.8f + index * 0.1f;
        while (body != null)
        {
            float t = Time.time * speed + phase;
            body.position = rest + new Vector3(0f, Mathf.Sin(t) * BobHeight, 0f);
            body.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.6f) * 2f);
            yield return null;
        }
    }

    private void EnsureOverlay()
    {
        if (overlayCanvas != null) return;

        GameObject canvasObject = new GameObject("Day4BurialOverlay");
        canvasObject.transform.SetParent(transform, false);
        overlayCanvas = canvasObject.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // Above the scene, under the dialogue box
        overlayCanvas.sortingOrder = 50;

        carryImage = CreateFullScreenImage("carry", Color.white);
        blackImage = CreateFullScreenImage("black", Color.black);
    }

    private Image CreateFullScreenImage(string name, Color color)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(overlayCanvas.transform, false);
        RectTransform rect = imageObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        image.enabled = false;
        return image;
    }

    private static IEnumerator Fade(Image image, float from, float to, float duration)
    {
        Color c = image.color;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / duration);
            image.color = c;
            yield return null;
        }
        c.a = to;
        image.color = c;
    }
}

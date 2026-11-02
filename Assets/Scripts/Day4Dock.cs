using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Day 4 at the dock: the wreck has washed in with the weather. Picking through the scraps of it
/// is the survey; after that the floating bodies are picked up one at a time and dropped into the
/// mass grave (see BurialScript). The first he carries up there himself; after that the trip is
/// skipped with a fade. One of them is a bride.
/// Once they're all buried a fifth turns up: the mermaid. She's breathing, so instead of the
/// grave she's carried inside to Camborne's bed (see MiscObjectClick.ClickPartnerBed).
/// </summary>
public class Day4Dock : MonoBehaviour
{
    private const string SeenKey = "day4_dock_bodies_seen";
    private const string MermaidSeenKey = "day4_dock_mermaid_seen";
    private const int BrideIndex = 2;

    // What he notices picking each one up. Body i is "corpse i+1 float dock".
    private static readonly string[][] PickUpLines =
    {
        new[] { "His eyes are still open." },
        new[] { "His coat is full of seawater." },
        new[] { "Her dress.##.##.## a wedding dress.", "She died on her wedding day." },
        new[] { "Younger than me." },
    };

    // Where each floating sheet's body is in its 660x400 frames (pixels from the top-left, covering
    // every frame of its bob), for the click boxes. Body i is "corpse i+1 float dock".
    private static readonly RectInt[] BodyBoxes =
    {
        new RectInt(10, 289, 170, 76),
        new RectInt(13, 224, 103, 52),
        new RectInt(133, 229, 101, 60),
        new RectInt(494, 235, 162, 55),
    };
    // She floats where the first body was.
    private static readonly RectInt MermaidBox = new RectInt(10, 242, 221, 122);
    // The survey: the washed-up planks are in the bottom-right corner, but the click covers the
    // whole frame down to the Return strip along the bottom, so it's hard to miss.
    private static readonly RectInt ScrapsBox = new RectInt(0, 0, 660, 336);
    // It sits in front of the bodies' boxes, so clicking a body surveys first, but stays behind the
    // dialogue box's click catcher (z -3 or nearer) so dialogues still advance.
    private const float ScrapsBoxForward = 0.5f;

    private SpriteRenderer background;
    private FloatingLayer driftwood;
    private Sprite[][] bodyFrames;
    private Sprite[] mermaidFrames;
    private Sprite[] carrySprites;
    private Sprite mermaidSprite;

    private Canvas overlayCanvas;
    private Image blackImage;
    private Image carryImage;
    private bool busy;

    public void Init(SpriteRenderer background, FloatingLayer driftwood, Sprite[][] bodyFrames, Sprite[] mermaidFrames,
        Sprite[] carrySprites, Sprite mermaidSprite)
    {
        this.background = background;
        this.driftwood = driftwood;
        this.bodyFrames = bodyFrames;
        this.mermaidFrames = mermaidFrames;
        this.carrySprites = carrySprites;
        this.mermaidSprite = mermaidSprite;
    }

    void Start()
    {
        if (!Day4Beach.WreckageWashedUp)
            return;

        if (!Day4Beach.Surveyed)
            WireScraps();

        if (Day4Beach.Buried >= Day4Beach.BodyCount)
        {
            if (!Day4Beach.MermaidRescued)
            {
                SpawnMermaid();
                // Back from the grave with the last of them in it, and someone's still out there
                if (!GameState.Get<bool>(MermaidSeenKey, false))
                    StartCoroutine(MermaidFirstSightRoutine());
            }
            return;
        }

        for (int i = 0; i < Day4Beach.BodyCount && i < BodyBoxes.Length; i++)
        {
            if (!Day4Beach.BodyBuried(i) && Day4Beach.CarriedBody != i)
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
            "The tide keeps bringing them in."
        });
    }

    private IEnumerator MermaidFirstSightRoutine()
    {
        yield return new WaitForSeconds(0.6f);
        GameState.Set(MermaidSeenKey, true);
        DialogueManager.ShowDialogueFromText(new string[] { "Wait.## There's one more." });
    }

    private void SpawnBody(int index)
    {
        Sprite[] frames = bodyFrames != null && index < bodyFrames.Length ? bodyFrames[index] : null;
        FloatingLayer layer = FloatingLayer.Spawn("day4_floating_body_" + index, background, frames);
        if (layer == null) return;

        BoxCollider2D box = layer.AddClickBox(BodyBoxes[index]);
        GameObject body = layer.gameObject;
        box.gameObject.AddComponent<Day4ClickTarget>().onClick = () => OnBodyClicked(index, body);
    }

    private Sprite CarrySprite(int index)
    {
        if (carrySprites == null || carrySprites.Length == 0) return null;
        return carrySprites[index % carrySprites.Length];
    }

    private void OnBodyClicked(int index, GameObject body)
    {
        if (busy) return;

        if (!Day4Beach.Surveyed)
        {
            DialogueManager.ShowDialogueFromText(new string[] { "I should see what else washed up first." });
            return;
        }

        if (Day4Beach.CarryingBody)
        {
            DialogueManager.ShowDialogueFromText(new string[] { "One at a time." });
            return;
        }

        PickUp(index, body);
    }

    // Out of the water and into his arms. The first one he carries up to the grave himself (the strip
    // follows him there); after that the walk is skipped and it fades straight to the grave.
    private void PickUp(int index, GameObject body)
    {
        bool firstTrip = Day4Beach.Buried == 0;

        Day4Beach.SetCarriedBody(index);
        Destroy(body);
        Day4CarryStrip.Show(CarrySprite(index));

        string[] lines = index < PickUpLines.Length ? PickUpLines[index] : new[] { "Heavier than they look." };
        Dialogue d = DialogueManager.ShowDialogueFromText(lines);
        if (!firstTrip)
            d.onDialogueEnd.AddListener(() => Navigation.GoToScene(GameConsts.BURIALSCENE));
    }

    private void WireScraps()
    {
        if (driftwood == null) return;

        BoxCollider2D box = driftwood.AddClickBox(ScrapsBox);
        box.transform.localPosition += new Vector3(0f, 0f, -ScrapsBoxForward);
        box.gameObject.AddComponent<Day4ClickTarget>().onClick = () =>
        {
            // Only once: the scraps go back to just floating there
            Destroy(box.gameObject);
            Survey();
        };
    }

    private void Survey()
    {
        GameState.Set(Day4Beach.SurveyedKey, true);
        DialogueManager.ShowDialogueFromText(new string[]
        {
            "You pick through the scraps on the shore.",
            "A can of corn chowder.## A different brand.",
            "Be grateful."
        });
        MessageBus.Instance.Publish("CompleteTask", "task_survey_beach");
    }

    private void SpawnMermaid()
    {
        FloatingLayer layer = FloatingLayer.Spawn("day4_floating_mermaid", background, mermaidFrames);
        if (layer == null) return;

        BoxCollider2D box = layer.AddClickBox(MermaidBox);
        GameObject body = layer.gameObject;
        box.gameObject.AddComponent<Day4ClickTarget>().onClick = () => OnMermaidClicked(body);
    }

    private void OnMermaidClicked(GameObject body)
    {
        if (busy) return;

        Dialogue d = DialogueManager.ShowDialogueFromText(new string[]
        {
            "She's not like the others.",
            "Her chest.##.##.## it's moving.",
            "She's alive.## She can be saved."
        });
        d.onDialogueEnd.AddListener(() => StartCoroutine(CarryInsideRoutine(body)));
    }

    // Not to the grave: up to the house, and Camborne's empty bed.
    private IEnumerator CarryInsideRoutine(GameObject body)
    {
        busy = true;
        GameState.Set("navigationBlocked", true);
        EnsureOverlay();
        // Stays hidden: handlogic keeps it off while he's carrying her up to the house
        SetInjuredHandVisible(false);

        if (body != null) body.SetActive(false);
        Sprite carry = mermaidSprite != null ? mermaidSprite : CarrySprite(0);
        SetCarrySprite(carry);
        yield return Fade(carryImage, 0f, 1f, 0.35f);

        yield return null;
        while (!Input.GetMouseButtonDown(0))
            yield return null;

        blackImage.enabled = true;
        yield return Fade(blackImage, 0f, 1f, 0.6f);
        carryImage.enabled = false;
        if (body != null) Destroy(body);

        GameState.Set(Day4Beach.MermaidRescuedKey, true);
        // There are more important things than the lighthouse now
        MessageBus.Instance.Publish("CompleteTask", "task_work");

        // He carries her up to the house himself; Navigation keeps him on the way to the bedroom
        yield return Fade(blackImage, 1f, 0f, 0.6f);
        blackImage.enabled = false;

        GameState.Set("navigationBlocked", false);
        busy = false;
        MessageBus.Instance.Publish("AddTaskStringImportant", "generic/task_mermaid_bed");
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

        carryImage = CreateCarryImage();
        blackImage = CreateFullScreenImage("black", Color.black);
    }

    // The carrying images are a strip along the bottom of the screen, as wide as the screen and as
    // tall as their own proportions make them, like looking down at what you're hauling.
    private Image CreateCarryImage()
    {
        GameObject imageObject = new GameObject("carry");
        imageObject.transform.SetParent(overlayCanvas.transform, false);
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
        return image;
    }

    // His own hand stays off screen while his arms are full
    private static void SetInjuredHandVisible(bool visible)
    {
        handlogic hand = FindFirstObjectByType<handlogic>();
        if (hand != null && hand.TryGetComponent(out SpriteRenderer handRenderer))
            handRenderer.enabled = visible;
    }

    private void SetCarrySprite(Sprite sprite)
    {
        carryImage.sprite = sprite;
        carryImage.enabled = sprite != null;
        if (sprite != null)
            carryImage.GetComponent<AspectRatioFitter>().aspectRatio = sprite.rect.width / sprite.rect.height;
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

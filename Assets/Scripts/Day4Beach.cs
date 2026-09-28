using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Day 4 outside: the wreckage washes up on the shore once the weather is recorded. The first
/// click surveys it, then each click after carries one of the four bodies off and buries it next
/// to Camborne, with the usual "Scratch?" after. Nothing else outside can be done until they're
/// all buried.
/// </summary>
public class Day4Beach : MonoBehaviour
{
    public const int Day = 4;
    public const int BodyCount = 4;
    public const string SurveyedKey = "day4_beach_surveyed";
    public const string BuriedKey = "day4_bodies_buried";

    // Placeholder pile, until there's washed-up wreckage art: some of the fishing junk, shrunk
    // down and scattered on the shore.
    private static readonly Vector3 PilePosition = new Vector3(2.3f, -1.35f, 0.15f);
    private static readonly Vector3[] PieceOffsets =
    {
        new Vector3(-0.35f, 0.05f, 0f),
        new Vector3(0.05f, 0.12f, -0.01f),
        new Vector3(0.35f, -0.02f, -0.02f),
        new Vector3(-0.1f, -0.12f, -0.03f),
        new Vector3(0.2f, -0.15f, -0.04f),
    };
    private const float PieceScale = 0.22f;

    private Sprite[] pileSprites;
    private Sprite[] carrySprites;
    private AudioClip shovelClip;
    private AudioClip scratchSound;
    private MiscObjectClick miscObjectClick;

    private GameObject pile;
    private Canvas overlayCanvas;
    private Image blackImage;
    private Image carryImage;
    private bool busy;

    public static bool IsToday => GameState.Get<int>("day") == Day;
    public static int Buried => GameState.Get<int>(BuriedKey, 0);
    public static bool Surveyed => GameState.Get<bool>(SurveyedKey, false);

    // The shore is waiting to be dealt with: from the weather log until the last body is buried.
    public static bool BlocksLeaving =>
        IsToday && GameState.Get<bool>("recorded_weather", false) && Buried < BodyCount;

    public static string BlockedLine =>
        Surveyed ? "I can't just leave them out there." : "Something's washed up on the shore.";

    public void Init(Sprite[] pileSprites, Sprite[] carrySprites, AudioClip shovelClip, AudioClip scratchSound,
        MiscObjectClick miscObjectClick)
    {
        this.pileSprites = pileSprites;
        this.carrySprites = carrySprites;
        this.shovelClip = shovelClip;
        this.scratchSound = scratchSound;
        this.miscObjectClick = miscObjectClick;
    }

    void Update()
    {
        // The weather log is recorded in this scene, so the pile can turn up while we're here.
        if (pile == null && BlocksLeaving)
            CreatePile();
    }

    private void CreatePile()
    {
        pile = new GameObject("day4_washed_up_pile");
        pile.transform.SetParent(transform, false);
        pile.transform.position = PilePosition;

        if (pileSprites != null)
        {
            for (int i = 0; i < pileSprites.Length && i < PieceOffsets.Length; i++)
            {
                if (pileSprites[i] == null) continue;
                GameObject piece = new GameObject("piece_" + i);
                piece.transform.SetParent(pile.transform, false);
                piece.transform.localPosition = PieceOffsets[i];
                piece.transform.localScale = Vector3.one * PieceScale;
                piece.transform.localRotation = Quaternion.Euler(0f, 0f, (i % 2 == 0 ? -1f : 1f) * 12f * i);
                SpriteRenderer sr = piece.AddComponent<SpriteRenderer>();
                sr.sprite = pileSprites[i];
            }
        }

        BoxCollider2D box = pile.AddComponent<BoxCollider2D>();
        box.size = new Vector2(1.2f, 0.55f);
        pile.AddComponent<Day4PileClick>().beach = this;
    }

    public void OnPileClicked()
    {
        if (busy) return;

        if (!Surveyed)
        {
            Survey();
        }
        else if (Buried < BodyCount)
        {
            StartCoroutine(CarryAndBuryRoutine());
        }
    }

    private void Survey()
    {
        GameState.Set(SurveyedKey, true);
        MessageBus.Instance.Publish("FloatText", 0f, 0.3f, "-SANITY", "purple");
        MessageBus.Instance.Publish("PlusSanity", -10);

        DialogueManager.ShowDialogueFromText(new string[]
        {
            "The sea has thrown up a ship.##.##.## what's left of it.",
            "Splintered planks. Crates. Cans.",
            "And bodies.",
            "Canned cabbages.## Be grateful."
        });
        MessageBus.Instance.Publish("CompleteTask", "task_survey_beach");
    }

    private IEnumerator CarryAndBuryRoutine()
    {
        busy = true;
        GameState.Set("navigationBlocked", true);
        EnsureOverlay();

        int index = Buried;

        // Pick one up
        Sprite carry = (carrySprites != null && carrySprites.Length > 0)
            ? carrySprites[index % carrySprites.Length]
            : null;
        carryImage.sprite = carry;
        carryImage.enabled = carry != null;
        yield return Fade(carryImage, 0f, 1f, 0.35f);

        // Drag them over until you click
        yield return null;
        while (!Input.GetMouseButtonDown(0))
            yield return null;

        // Bury them next to Camborne
        blackImage.enabled = true;
        yield return Fade(blackImage, 0f, 1f, 0.6f);
        carryImage.enabled = false;
        if (shovelClip != null && miscObjectClick != null)
            miscObjectClick.PlaySound(shovelClip, 0.6f);
        yield return new WaitForSeconds(1.6f);

        int buried = GameState.Increment(BuriedKey);
        if (buried >= BodyCount && pile != null)
        {
            Destroy(pile);
        }
        yield return Fade(blackImage, 1f, 0f, 0.6f);
        blackImage.enabled = false;

        GameState.Set("navigationBlocked", false);
        busy = false;

        HandScratch.Prompt("Scratch?", () =>
        {
            if (buried >= BodyCount)
            {
                MessageBus.Instance.Publish("CompleteTask", "task_bury_bodies");
                DialogueManager.ShowDialogueFromText(new string[] { "That's all of them." });
            }
        }, scratchSound);
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

/// <summary>Click target for the day 4 pile, with the same guards as InteractableObject.</summary>
public class Day4PileClick : MonoBehaviour
{
    public Day4Beach beach;

    private static bool Blocked =>
        GameState.Get<bool>("task_list_open", false)
        || GameState.Get<bool>("minigame_open", false)
        || GameState.Get<bool>("treasure_inspect_open", false)
        || GameState.Get<bool>("navigationBlocked", false)
        || GameState.Get<bool>("is_recording_weather", false)
        || GameState.Get<bool>("picking_choice", false)
        || DialogueManager.DialogueIsOpen();

    void OnMouseOver()
    {
        if (!Blocked)
            CustomCursor.SetCursorToPointer();
    }

    void OnMouseExit()
    {
        CustomCursor.SetCursorToNormal();
    }

    void OnMouseUp()
    {
        if (Blocked) return;
        CustomCursor.SetCursorToNormal();
        beach.OnPileClicked();
    }
}

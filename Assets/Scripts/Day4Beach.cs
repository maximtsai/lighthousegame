using System;
using UnityEngine;

/// <summary>
/// Day 4 outside: the supply ship's wreckage washes up on the shore once the weather is
/// recorded. Clicking it picks through the scraps and lands the guilt of the flickering light.
/// The bodies are out in the water at the dock (see Day4Dock), and he can't head in or up the
/// lighthouse until they're all buried.
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
    private GameObject pile;

    public static bool IsToday => GameState.Get<int>("day") == Day;
    public static int Buried => GameState.Get<int>(BuriedKey, 0);
    public static bool Surveyed => GameState.Get<bool>(SurveyedKey, false);

    private static bool WreckageWashedUp => IsToday && GameState.Get<bool>("recorded_weather", false);

    // Home and the lighthouse wait until the shore's been seen to and the bodies are buried.
    public static bool BlocksHeadingIn => WreckageWashedUp && Buried < BodyCount;

    // The dock waits until the shore's been seen to.
    public static bool BlocksPier => WreckageWashedUp && !Surveyed;

    public static string BlockedLine =>
        Surveyed ? "There are still bodies in the water." : "Something's washed up on the shore.";

    public void Init(Sprite[] pileSprites)
    {
        this.pileSprites = pileSprites;
    }

    void Update()
    {
        // The weather log is recorded in this scene, so the pile can turn up while we're here.
        if (pile == null && WreckageWashedUp && !Surveyed)
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
        pile.AddComponent<Day4ClickTarget>().onClick = Survey;
    }

    // He picks through what's left of the supply ship, and knows whose fault it is.
    private void Survey()
    {
        GameState.Set(SurveyedKey, true);
        Destroy(pile);

        DialogueManager.ShowDialogueFromText(new string[]
        {
            "The supply ship.##.##.## what's left of it.",
            "The light was flickering all night.## They couldn't see the rocks.",
            "...This is my fault.",
            "You pick through the scraps on the shore.",
            "A can of corn chowder.## A different brand.",
            "Be grateful."
        });
        MessageBus.Instance.Publish("CompleteTask", "task_survey_beach");
    }
}

/// <summary>Click target for the day 4 pile and bodies, with the same guards as InteractableObject.</summary>
public class Day4ClickTarget : MonoBehaviour
{
    public Action onClick;

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
        onClick?.Invoke();
    }
}

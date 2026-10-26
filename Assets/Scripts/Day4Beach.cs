using System;
using UnityEngine;

/// <summary>
/// Day 4 state shared between scenes. Once the weather is recorded, the supply ship's wreckage
/// and its dead wash up at the dock (see Day4Dock): picking through the scraps there is the
/// survey, then the bodies get buried and the mermaid turns up. He can't head in or up the
/// lighthouse until all that's done.
/// </summary>
public static class Day4Beach
{
    public const int Day = 4;
    public const int BodyCount = 4;
    public const string SurveyedKey = "day4_beach_surveyed";
    public const string BuriedKey = "day4_bodies_buried";
    // The fifth body at the dock is the mermaid. She's alive, and she goes to Camborne's bed.
    public const string MermaidRescuedKey = "day4_mermaid_rescued";
    public const string MermaidInBedKey = "day4_mermaid_in_bed";
    public const string MermaidTreatedKey = "mermaid_treated";

    public static bool IsToday => GameState.Get<int>("day") == Day;
    public static int Buried => GameState.Get<int>(BuriedKey, 0);
    public static bool Surveyed => GameState.Get<bool>(SurveyedKey, false);
    public static bool MermaidRescued => GameState.Get<bool>(MermaidRescuedKey, false);

    // Tending to her replaces the lighthouse on Day 4, so it counts as the day's work being done
    // (the fish traps wait on that).
    public static bool WorkDone =>
        GameState.Get<bool>("lighthouse_fixed", false)
        || (IsToday && GameState.Get<bool>(MermaidTreatedKey, false));

    // The wreck comes in with the weather.
    public static bool WreckageWashedUp => IsToday && GameState.Get<bool>("recorded_weather", false);

    // Home and the lighthouse wait until the dock's been seen to, the bodies are buried and
    // she's been picked up.
    public static bool BlocksHeadingIn => WreckageWashedUp && (Buried < BodyCount || !MermaidRescued);

    // From picking her up at the dock until she's laid in Camborne's bed, he's carrying her, and the
    // only way to go is up to the bedroom (see Navigation.BlockedWhileCarryingHer).
    public static bool CarryingHer =>
        IsToday && MermaidRescued && !GameState.Get<bool>(MermaidInBedKey, false);

    public static string CarryingLine(string destination) =>
        destination == GameConsts.LHFLOORSCENE ? "The lighthouse can wait.## She can't."
        : "Camborne's bed first.";

    public static string BlockedLine =>
        !Surveyed ? "Something's washed up by the dock."
        : Buried < BodyCount ? "There are still bodies in the water."
        : "There's still someone in the water.";
}

/// <summary>Click target for the day 4 pile and bodies, with the same guards as InteractableObject.</summary>
public class Day4ClickTarget : MonoBehaviour
{
    public Action onClick;

    private static bool Blocked => InputGate.WorldBlocked;

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

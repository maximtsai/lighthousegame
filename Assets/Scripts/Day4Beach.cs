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

    public const string BodyBuriedKeyPrefix = "day4_body_buried_";
    // Which body he's carrying up to the grave, stored as index + 1 so 0 means empty-handed.
    private const string CarriedBodyKey = "day4_carried_body";

    public static bool IsToday => GameState.Get<int>("day") == Day;
    public static int Buried => GameState.Get<int>(BuriedKey, 0);
    public static bool BodyBuried(int index) => GameState.Get<bool>(BodyBuriedKeyPrefix + index, false);

    public static int CarriedBody => GameState.Get<int>(CarriedBodyKey, 0) - 1;
    public static bool CarryingBody => IsToday && CarriedBody >= 0;
    public static void SetCarriedBody(int index) => GameState.Set(CarriedBodyKey, index + 1);

    // The grave stands open from when the wreck comes in until the last body is in it.
    public static bool MassGraveOpen => WreckageWashedUp && Buried < BodyCount;
    public static bool Surveyed => GameState.Get<bool>(SurveyedKey, false);
    public static bool MermaidRescued => GameState.Get<bool>(MermaidRescuedKey, false);

    // Tending to her replaces the lighthouse on Days 4 and 5, so it counts as the day's work being
    // done (the fish traps wait on that).
    public static bool WorkDone =>
        GameState.Get<bool>("lighthouse_fixed", false)
        || (IsToday && GameState.Get<bool>(MermaidTreatedKey, false))
        || Day5.Treated;

    // The wreck comes in with the weather.
    public static bool WreckageWashedUp => IsToday && GameState.Get<bool>("recorded_weather", false);

    // Home and the lighthouse wait until the dock's been seen to, the bodies are buried and
    // she's been picked up.
    public static bool BlocksHeadingIn => WreckageWashedUp && (Buried < BodyCount || !MermaidRescued);

    // From picking her up at the dock until she's laid in Camborne's bed, he's carrying her, and the
    // only way to go is up to the bedroom (see Navigation.BlockedWhileCarrying).
    public static bool CarryingHer =>
        IsToday && MermaidRescued && !GameState.Get<bool>(MermaidInBedKey, false);

    // Once she's been picked up, the lighthouse is off for the rest of the day: she comes first, and
    // fixing it would count as the day's work and open the fish traps without her being tended.
    public static bool LighthouseSetAside => IsToday && MermaidRescued;
    public const string LighthouseSetAsideLine = "I have more important things to handle now.";

    // Carrying her or one of the bodies: no hand on screen, nothing else to be done but get there.
    public static bool ArmsFull => CarryingHer || CarryingBody;

    public static string ArmsFullLine => CarryingHer ? "Not with her in my arms." : "Not with my arms full.";

    public static string CarryingLine(string destination) =>
        CarryingHer
            ? (destination == GameConsts.LHFLOORSCENE ? "The lighthouse can wait.## She can't." : "Camborne's bed first.")
            : (destination == GameConsts.LHFLOORSCENE ? "The lighthouse can wait." : "The grave first.");

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

using UnityEngine;

/// <summary>
/// Editor-only: applies Tools / Lighthouse / Test Day before the first scene loads,
/// so day-gated content (treasures, etc.) sees the forced day on Start.
/// Starts from a fresh New Game state set up as a real playthrough would reach that day.
/// </summary>
public static class TestDayBootstrap
{
#if UNITY_EDITOR
    // Must match TestDayTool.PrefKey (Editor assembly — cannot reference from runtime).
    private const string PrefKey = "Lighthouse.ForceTestDay";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyForcedTestDay()
    {
        TryApplyForcedTestDay();
    }
#endif

    public static bool HasForcedTestDay
    {
        get
        {
#if UNITY_EDITOR
            int day = UnityEditor.EditorPrefs.GetInt(PrefKey, 0);
            return day >= 1 && day <= 7;
#else
            return false;
#endif
        }
    }

    // Continue can replace the state initialized before scene load with an older save.
    // Reapply the selected test day there, while leaving New Game and player builds on the normal flow.
    public static bool TryApplyForcedTestDay()
    {
#if UNITY_EDITOR
        int day = UnityEditor.EditorPrefs.GetInt(PrefKey, 0);
        if (day < 1 || day > 7)
            return false;

        // Same setup a real playthrough reaches at the start of this day: reset, then run the
        // day's own setup (hand cut, infestation, ...) and fill in what earlier days leave behind.
        GameState.FullReset();
        GameState.Set("day", day);
        if (day >= 2)
        {
            // Day 1: Camborne buried, intro cutscene and journal already seen
            GameState.Set("has_dug", true);
            GameState.Set("cutscene_outdoors_played", true);
            GameState.Set("introduced_journal", true);
        }
        if (day >= 3)
        {
            // Day 2: grave fixed and the cut hand bandaged at the sink
            GameState.Set("grave_inspected", true);
            GameState.Set("hand_cleaned", true);
        }
        GameState.StartNewDay();
        Debug.Log("TestDayBootstrap: forced GameState day = " + day);
        return true;
#else
        return false;
#endif
    }
}

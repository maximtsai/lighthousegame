using UnityEngine;

/// <summary>
/// Editor-only: applies Tools / Lighthouse / Test Day before the first scene loads,
/// so day-gated content (treasures, etc.) sees the forced day on Start.
/// </summary>
public static class TestDayBootstrap
{
#if UNITY_EDITOR
    // Must match TestDayTool.PrefKey (Editor assembly — cannot reference from runtime).
    private const string PrefKey = "Lighthouse.ForceTestDay";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyForcedTestDay()
    {
        int day = UnityEditor.EditorPrefs.GetInt(PrefKey, 0);
        if (day < 1 || day > 5)
            return;

        GameState.Set("day", day);
        Debug.Log("TestDayBootstrap: forced GameState day = " + day);
    }
#endif
}
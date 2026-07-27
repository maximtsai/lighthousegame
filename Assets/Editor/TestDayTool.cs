#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Edit Mode tool: pick which day GameState should use when you enter Play Mode.
/// For testing day-gated content (treasures, etc.) through day 5.
/// Menu: Tools / Lighthouse / Test Day
/// </summary>
public static class TestDayTool
{
    public const string PrefKey = "Lighthouse.ForceTestDay";

    [MenuItem("Tools/Lighthouse/Test Day/Off (use saved day)", false, 10)]
    public static void SetOff() => SetDay(0);

    [MenuItem("Tools/Lighthouse/Test Day/Off (use saved day)", true)]
    public static bool SetOffValidate()
    {
        Menu.SetChecked("Tools/Lighthouse/Test Day/Off (use saved day)", GetForcedDay() == 0);
        return !Application.isPlaying;
    }

    [MenuItem("Tools/Lighthouse/Test Day/Day 1", false, 20)]
    public static void SetDay1() => SetDay(1);

    [MenuItem("Tools/Lighthouse/Test Day/Day 1", true)]
    public static bool SetDay1Validate() => ValidateDayItem(1);

    [MenuItem("Tools/Lighthouse/Test Day/Day 2", false, 21)]
    public static void SetDay2() => SetDay(2);

    [MenuItem("Tools/Lighthouse/Test Day/Day 2", true)]
    public static bool SetDay2Validate() => ValidateDayItem(2);

    [MenuItem("Tools/Lighthouse/Test Day/Day 3", false, 22)]
    public static void SetDay3() => SetDay(3);

    [MenuItem("Tools/Lighthouse/Test Day/Day 3", true)]
    public static bool SetDay3Validate() => ValidateDayItem(3);

    [MenuItem("Tools/Lighthouse/Test Day/Day 4", false, 23)]
    public static void SetDay4() => SetDay(4);

    [MenuItem("Tools/Lighthouse/Test Day/Day 4", true)]
    public static bool SetDay4Validate() => ValidateDayItem(4);

    [MenuItem("Tools/Lighthouse/Test Day/Day 5", false, 24)]
    public static void SetDay5() => SetDay(5);

    [MenuItem("Tools/Lighthouse/Test Day/Day 5", true)]
    public static bool SetDay5Validate() => ValidateDayItem(5);

    private static bool ValidateDayItem(int day)
    {
        Menu.SetChecked("Tools/Lighthouse/Test Day/Day " + day, GetForcedDay() == day);
        return !Application.isPlaying;
    }

    private static void SetDay(int day)
    {
        EditorPrefs.SetInt(PrefKey, day);
        if (day <= 0)
            Debug.Log("Test Day: Off — next Play uses the saved/default GameState day.");
        else
            Debug.Log("Test Day: Day " + day + " will be forced the next time you enter Play Mode.");
    }

    public static int GetForcedDay()
    {
        return EditorPrefs.GetInt(PrefKey, 0);
    }
}
#endif
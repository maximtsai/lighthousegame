using System.Collections.Generic;
using UnityEngine;

public static class GameState
{
    public static System.Action OnDataChanged;

    // These catches supply skin for the mermaid rather than ingredients for dinner.
    public static bool FishIsForTreatment(int day) => day >= 5 && day <= 7;
    public static bool DinnerRequiresFish(int day) => day != 2 && !FishIsForTreatment(day);

    private static Dictionary<string, object> data;

    static GameState()
    {
        data = new Dictionary<string, object>();
        // Start from the same state as New Game, so pressing Play in any scene (not just
        // MainScene) gives a clean Day 1 instead of a half-set-up day.
        FullReset();
    }


    // Set a value
    public static void Set<T>(string key, T value)
    {
        data[key] = value;
        OnDataChanged?.Invoke();
    }

    // Set a value (non-generic version for convenience or explicitly object type)
    public static void Set(string key, object value)
    {
        data[key] = value;
        OnDataChanged?.Invoke();
    }

    // Get a value with type casting
    public static T Get<T>(string key, T defaultValue = default)
    {
        if (data.TryGetValue(key, out object value))
        {
            if (value is T typedValue)
                return typedValue;
            else
                Debug.LogWarning($"GameState: Key '{key}' exists but is not of type {typeof(T).Name}");
        }
        return defaultValue;
    }

    // Check if key exists
    public static bool HasKey(string key)
    {
        return data.ContainsKey(key);
    }

    // Remove a key
    public static void Remove(string key)
    {
        data.Remove(key);
    }

    // Clear all data
    public static void Clear()
    {
        data.Clear();
    }

    public static int Increment(string key, int amount = 1)
    {
        int current = Get(key, 0);
        current += amount;
        Set(key, current);
        return current;
    }

    public static bool Toggle(string key)
    {
        bool current = Get(key, false);
        current = !current;
        Set(key, current);
        return current;
    }

    // For saving and loading
    public static Dictionary<string, object> GetAllData()
    {
        return new Dictionary<string, object>(data);
    }

    public static void SetAllData(Dictionary<string, object> newData)
    {
        data = new Dictionary<string, object>(newData);
    }

    public static string StringifyData()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var pair in data)
        {
            sb.Append(pair.Key);
            sb.Append(": ");
            sb.Append(pair.Value);
            sb.Append("\n");
        }

        return sb.ToString();
    }

    public static void FullReset()
    {
        Clear();
        Set("day", 1);
        Set("sanity", GameConsts.SANITY_START); // Starting sanity
        
        // Burial states
        Set("has_dug", false);
        Set("has_buried", false);
        Set("grave_revealed", false);
        Set("grave_inspected", false);
        Set("hand_cut", false);
        Set("hand_cleaned", false);
        Set("handInfested", false);
        Set("scissorsDrop", false);
        Set("scissors_found", false);
        Set("lighthouseFixForgotten", false);
        Set("ScissorsDisappeared", false);

        // Events
        Set("introduced_journal", false);
        Set("cutscene_outdoors_played", false);
        Set("thunderstorm", false);

        StartNewDay();
    }

    // Flags that only mean something while a piece of UI or a transition is on screen. The UI is
    // gone after a new day or a load, so a stale true here would swallow every click. Saves happen
    // at day transitions, so these can also end up in a save file.
    public static void ResetTransientFlags()
    {
        Set("navigationBlocked", false);
        Set("day_transition_started", false);
        Set("dialogue_is_open", false);
        Set("minigame_open", false);
        Set("picking_choice", false);
        Set("treasure_inspect_open", false);
        Set("task_list_open", false);
        Set("pause_open", false);
        Set("is_recording_weather", false);
    }

    // Game specific function
    public static void StartNewDay() {
        MessageBus.Instance.Publish("ClearAllTasks");
        int day = Get<int>("day", 1);

        Set("day_began", false);

        Set("is_clean", "false");

        Set("corn_clicked", false);
        Set("pepper_clicked", false);
        Set("alcohol_clicked", false);
        Set("fish_clicked", false);

        Set("ate_breakfast", false);
        Set("ate_dinner", false);
        Set("hungry", true);
        Set("can_sleep", false);
        
        Set("lighthouse_opened", false);
        Set("lighthouse_fixed", false);
        LighthouseClimb.Reset();
        LighthouseBrick.ResetForNewDay();
        Set("wrench_used", false);
        Set("oil_used", false);
        Set("scissors_used", false);
        Set("mercury_used", false);

        Set("gathered_fish", false);
        Set("is_nighttime", false);
        Set("near_nighttime", false);
        
        Set("recorded_weather", false);
        Set("thunderstorm", false);
        
        Set("do_burial", false);

        Set("ready_to_sleep", false);
        Set("intro_played", false);
        ResetTransientFlags();

        // Day specific start logic
        if (day >= 2)
        {
            Set("has_buried", true);
        }
        
        if (day >= 3 && day <= 7)
        {
            Set("hand_cut", true);
        }

        // Day 4 onward the wound has festered, and pus starts oozing at Day 4 breakfast (then stays)
        Set("hand_oozing", day >= 5);
        if (day >= 4)
        {
            Set("handInfested", true);
        }
    }
}

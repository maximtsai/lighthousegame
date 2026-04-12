using System.Collections.Generic;
using UnityEngine;

public static class GameState
{
    public static System.Action OnDataChanged;

    private static Dictionary<string, object> data;

    static GameState()
    {
        data = new Dictionary<string, object>();
        // We can write whatever else code below to initialize data
        data["day"] = 1;
        data["lighthouse_fixed"] = false;
        data["dropped_tool"] = false;
        data["gathered_fish"] = false;
        data["checked_weather"] = false;
        data["ate_breakfast"] = false;
        data["ate_dinner"] = false;
        data["hungry"] = true;
        data["intro_played"] = false;
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
        Set("sanity", 3); // Starting sanity
        
        // Burial states
        Set("has_dug", false);
        Set("has_buried", false);
        Set("grave_revealed", false);
        Set("grave_inspected", false);
        Set("hand_cut", false);

        // Events
        Set("introduced_journal", false);
        Set("cutscene_outdoors_played", false);

        StartNewDay();
    }

    // Game specific function
    public static void StartNewDay() {
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
        Set("wrench_used", false);
        Set("oil_used", false);
        Set("scissors_used", false);
        Set("mercury_used", false);

        Set("gathered_fish", false);
        Set("is_nighttime", false);
        Set("near_nighttime", false);
        
        Set("recorded_weather", false);
        
        Set("do_burial", false);
        Set("navigationBlocked", false);

        Set("ready_to_sleep", false);
        Set("intro_played", false);
    }
}
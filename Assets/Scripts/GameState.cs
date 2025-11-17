using System.Collections.Generic;
using UnityEngine;

public static class GameState
{
    private static Dictionary<string, object> data;

    static GameState()
    {
        data = new Dictionary<string, object>();
        // We can write whatever else code below to initialize data
        data["day"] = 1;
        data["fixed_lighthouse"] = false;
        data["dropped_tool"] = false;
        data["gathered_fish"] = false;
        data["checked_weather"] = false;
        data["ate_breakfast"] = false;
        data["ate_dinner"] = false;

    }


    // Set a value
    public static void Set(string key, object value)
    {
        data[key] = value;
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

}
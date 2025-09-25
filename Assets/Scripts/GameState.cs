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
}
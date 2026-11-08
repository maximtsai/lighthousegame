using System.Collections.Generic;
using UnityEngine;

//// Store data
// GameState.Set("day", 3);
// GameState.Set("hasKey", true);
// GameState.Set("currentScene", "Lighthouse");

//// Save
// SaveManager.Save();

//// Later...
// SaveManager.Load();

//// Retrieve data
// int day = GameState.Get<int>("day");
// bool hasKey = GameState.Get<bool>("hasKey");
// string scene = GameState.Get<string>("currentScene");

[System.Serializable]
public class SerializableGameState
{
    public List<string> keys = new List<string>();
    public List<ValueWrapper> values = new List<ValueWrapper>();

    public SerializableGameState() { }

    public SerializableGameState(Dictionary<string, object> data)
    {
        foreach (var kvp in data)
        {
            keys.Add(kvp.Key);
            values.Add(new ValueWrapper(kvp.Value));
        }
    }

    public Dictionary<string, object> ToDictionary()
    {
        var dict = new Dictionary<string, object>();
        for (int i = 0; i < keys.Count && i < values.Count; i++)
        {
            dict[keys[i]] = values[i].GetValue();
        }
        return dict;
    }

    // Handles typed serialization
    [System.Serializable]
    public class ValueWrapper
    {
        public string type;
        public string stringValue;
        public int intValue;
        public float floatValue;
        public bool boolValue;

        public ValueWrapper(object obj)
        {
            if (obj == null)
            {
                type = "null";
                return;
            }

            var objType = obj.GetType();

            if (objType == typeof(int))
            {
                type = "int";
                intValue = (int)obj;
            }
            else if (objType == typeof(float))
            {
                type = "float";
                floatValue = (float)obj;
            }
            else if (objType == typeof(bool))
            {
                type = "bool";
                boolValue = (bool)obj;
            }
            else // default to string
            {
                type = "string";
                stringValue = obj.ToString();
            }
        }

        public object GetValue()
        {
            switch (type)
            {
                case "int": return intValue;
                case "float": return floatValue;
                case "bool": return boolValue;
                case "string": return stringValue;
                default: return null;
            }
        }
    }
}

public static class SaveManager
{
    private const string SaveKey = "GameState_Save";

    public static void Save()
    {
        var serializable = new SerializableGameState(GameState.GetAllData());
        string json = JsonUtility.ToJson(serializable, true);

        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log("Game saved to PlayerPrefs.");
    }

    public static void Load()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            var serializable = JsonUtility.FromJson<SerializableGameState>(json);
            var data = serializable.ToDictionary();
            GameState.SetAllData(data);
            GameState.ResetTransientFlags();

            Debug.Log("Game loaded from PlayerPrefs.");
        }
        else
        {
            Debug.Log("No saved data found.");
        }
    }

    // Records one flag in the save without re-saving everything else. The save is a snapshot from the
    // start of the day, and the rest of it should stay that way; this is for one-off moments mid-day
    // (like a dream that's been seen) that shouldn't come back after Continue.
    public static void SaveFlag(string key, bool value)
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;

        var serializable = JsonUtility.FromJson<SerializableGameState>(PlayerPrefs.GetString(SaveKey));
        var data = serializable.ToDictionary();
        data[key] = value;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(new SerializableGameState(data), true));
        PlayerPrefs.Save();
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
        Debug.Log("Save data cleared.");
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }
}
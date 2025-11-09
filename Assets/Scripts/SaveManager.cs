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

            Debug.Log("Game loaded from PlayerPrefs.");
        }
        else
        {
            Debug.Log("No saved data found.");
        }
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
        Debug.Log("Save data cleared.");
    }
}
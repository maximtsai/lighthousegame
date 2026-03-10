using UnityEngine;

/// <summary>
/// This script ensures an object is only active during the day.
/// It will automatically disable itself if "is_nighttime" is true.
/// </summary>
public class DaytimeOnly : MonoBehaviour
{
    void Start()
    {
        // If it's NOT nighttime, it stays active. 
        // If it IS nighttime, it disappears.
        bool isNight = GameState.Get<bool>("is_nighttime", false);
        gameObject.SetActive(!isNight);
    }
}

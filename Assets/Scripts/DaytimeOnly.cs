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
        // Exception: The sink button should remain active at night.
        if (gameObject.name.ToLower().Contains("sink"))
        {
            return;
        }

        bool isNight = GameState.Get<bool>("is_nighttime", false);
        gameObject.SetActive(!isNight);
    }
}

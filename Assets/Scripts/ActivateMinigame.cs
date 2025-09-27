using UnityEngine;

public class ActivateMinigame : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void StartMinigame()
    {
        GameState.Set("minigame_open", true);
        gameObject.SetActive(true);
    }

    public void StopMinigame()
    {
        GameState.Set("minigame_open", false);
        gameObject.SetActive(false);
    }
}

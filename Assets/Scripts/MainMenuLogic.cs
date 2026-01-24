using System;
using UnityEngine;
using UnityEngine.SceneManagement; // required for SceneManager

public class MainMenuLogic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
	    GameState.Set("day", 1);
        MessageBus.Instance.Publish("PlayCutscene", "Intro", true, (Action)(() =>
        {
            SceneManager.LoadScene("BedroomScene");
        }), true);
        
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene("BedroomScene");
    }
}

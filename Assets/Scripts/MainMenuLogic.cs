using System;
using UnityEngine;
using UnityEngine.SceneManagement; // required for SceneManager

public class MainMenuLogic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            GameObject quitBtn = GameObject.Find("QuitButton");
            if (quitBtn != null)
            {
                quitBtn.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame called");
        Application.Quit();
    }

    public void StartGame()
    {
        SaveManager.ClearSave(); // Clear any old saves when starting fresh
        GameState.FullReset();
        MessageBus.Instance.Publish("PlayCutscene", "Intro", true, (Action)(() =>
        {
            SceneManager.LoadScene(GameConsts.BEDROOMSCENE);
        }), true);
    }

    public void ContinueGame()
    {
        SaveManager.Load(); // Load the saved state
        SceneManager.LoadScene(GameConsts.BEDROOMSCENE);
    }
}

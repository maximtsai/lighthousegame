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
        GameState.FullReset();
        // TODO: Change this to 1 for any release build
        int startDay = 1;
        GameState.Set("day", startDay);
        string introToPlay = "Intro";
        switch (startDay)
        {
            case 1:
                introToPlay = "Intro";
                break;
            case 2:
                introToPlay = "Day2";
                break;
            default:
                introToPlay = "Intro";
                break;
        }
        MessageBus.Instance.Publish("PlayCutscene", introToPlay, true, (Action)(() =>
        {
            SceneManager.LoadScene(GameConsts.BEDROOMSCENE);
        }), true);
        
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene(GameConsts.BEDROOMSCENE);
    }
}

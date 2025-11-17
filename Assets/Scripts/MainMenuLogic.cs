using UnityEngine;
using UnityEngine.SceneManagement; // required for SceneManager

public class MainMenuLogic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] CutsceneManager CsManager;
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
        CsManager.PlayCutscene("Intro", true, () => {
            SceneManager.LoadScene("BedroomScene");

        });
    }

    public void ContinueGame()
    {
        CsManager.PlayCutscene("Intro", true, () => {
            SceneManager.LoadScene("BedroomScene");
        });
    }
}

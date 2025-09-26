using UnityEngine;
using UnityEngine.SceneManagement; // required for SceneManager

public class Navigation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoToKitchen()
    {
        SceneManager.LoadScene("KitchenScene");
    }
    public void GoToBedroom()
    {
        SceneManager.LoadScene("BedroomScene");
    }

    public void GoTo(string scene)
    {
        if (DialogueManager.DialogueIsOpen() || GameState.Get<bool>("task_list_open", false))
            return;
        SceneManager.LoadScene(scene);
    }
    
}

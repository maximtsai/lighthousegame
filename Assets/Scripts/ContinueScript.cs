using UnityEngine;

// Logic for the Continue button.
public class ContinueScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int day = GameState.Get<int>("day", 0);
        if (day == 0)
        {
            gameObject.SetActive(false);
        }
    }
}

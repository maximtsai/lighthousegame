using UnityEngine;

// this class contains functions modifying specific GameState values

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetClean(bool is_clean)
    {
		if (is_clean) {
	        GameState.Set("is_clean", "true");
		} else {
	        GameState.Set("is_clean", "false");
		}
        Debug.Log("setting clean state");
    }
}

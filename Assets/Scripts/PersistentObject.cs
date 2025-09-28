using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    // Static reference to ensure only one Main Camera persists
    private static PersistentObject instance;

    void Awake()
    {
        // Ensure only one instance of the Main Camera persists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Make Main Camera persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate Main Cameras in new scenes
        }
    }
}
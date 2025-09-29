using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    private AudioSource audioSource;
    private Camera mainCamera;
    // Expose AudioSource for external access
    public static AudioManager Instance => instance;
    public AudioSource AudioSource => audioSource;
    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            transform.position = mainCamera.transform.position;
        }
        
        if (instance == null)
        {
            instance = this;
            audioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LateUpdate()
    {
        // Follow the main camera's position after all updates
        if (mainCamera != null)
        {
            transform.position = mainCamera.transform.position;
        }
    }
}
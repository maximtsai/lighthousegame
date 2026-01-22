using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    private AudioSource audioSource;
    private Camera mainCamera;

    private AudioManager prefab;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();
                if (instance == null)
                {
                    CreateFromPrefab();
                }
            }
            return instance;
        }
    }
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

    private static void CreateFromPrefab()
    {
        AudioManager prefab = Resources.Load<AudioManager>("AudioSourceFollowingCam");
        if (prefab == null)
        {
            Debug.LogError("AudioSourceFollowingCam not found in resources folder");
            return;
        }

        instance = Instantiate(prefab);
        instance.name = nameof(AudioManager);
    }

    void LateUpdate()
    {
        // Follow the main camera's position after all updates
        if (mainCamera != null)
        {
            transform.position = mainCamera.transform.position;
        }
    }
    
    public void SetMuted(bool muted)
    {
        AudioListener.pause = muted;
    }
}
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    private AudioSource audioSource;
    private Camera mainCamera;
    private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

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
            
            // Subscribe to global sound events
            MessageBus.Instance.Subscribe("PlaySound", OnPlaySoundMessage);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnPlaySoundMessage(object[] args)
    {
        if (args == null || args.Length == 0) return;
        
        if (args[0] is AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        else if (args[0] is string clipName)
        {
            PlaySoundByName(clipName);
        }
    }

    public void PlaySoundByName(string clipName)
    {
        if (string.IsNullOrEmpty(clipName)) return;

        if (!clipCache.TryGetValue(clipName, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>("Audio/" + clipName);
            if (clip != null)
            {
                clipCache[clipName] = clip;
            }
            else
            {
                Debug.LogWarning($"AudioManager: Could not find sound clip 'Audio/{clipName}' in Resources.");
                return;
            }
        }

        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
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
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    private AudioSource audioSource;
    // Scene loops (lighthouse hum, pier water) get their own source so the one-shot source's
    // loop flag, volume and pitch never change under them, and they can't leak into other scenes.
    private AudioSource loopSource;
    private MessageBus.SubscriptionHandle playSoundHandle;
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
    // Fetched lazily in case another script's Awake asks for it before ours has run
    public AudioSource AudioSource => audioSource != null ? audioSource : (audioSource = GetComponent<AudioSource>());
    
    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            transform.position = mainCamera.transform.position;
        }
        
        // Instance may already point at us if another script's Awake ran first and found us
        if (instance == null || instance == this)
        {
            instance = this;
            audioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);

            // Subscribe to global sound events
            playSoundHandle = MessageBus.Instance.Subscribe("PlaySound", OnPlaySoundMessage);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (instance != this) return;

        // The bus is static and outlives Play Mode when domain reload is off
        playSoundHandle?.Unsubscribe();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Each scene starts its own loop in Start (which runs after this), so whatever the last scene
    // was looping stops at the door.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
            StopLoop();
    }

    public void PlayLoop(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        if (loopSource == null)
        {
            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.playOnAwake = false;
            loopSource.loop = true;
            if (AudioSource != null)
                loopSource.outputAudioMixerGroup = AudioSource.outputAudioMixerGroup;
        }

        loopSource.volume = Mathf.Clamp01(volume);
        if (loopSource.clip == clip && loopSource.isPlaying) return;

        loopSource.clip = clip;
        loopSource.Play();
    }

    public void StopLoop()
    {
        if (loopSource == null) return;

        loopSource.Stop();
        loopSource.clip = null;
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
        PlaySoundWithRandomPitch(clipName, 1f, 1f, 1f);
    }

    public void PlaySoundWithRandomPitch(string clipName, float minPitch, float maxPitch, float volume = 1f)
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
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(clip, volume);
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
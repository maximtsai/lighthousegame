using UnityEngine;
using UnityEngine.SceneManagement;

public class Ambience : MonoBehaviour
{
    // Singleton instance
    private static Ambience instance;

    // Audio sources for the two tracks
    private AudioSource track1Source;
    private AudioSource track2Source;

    // Starting audio tracks
    public AudioClip newTrack1;
    public AudioClip newTrack2;
    public AudioClip rainIndoorsClip;
    
    // Ensure this object persists across scenes
    private void Awake()
    {
        // Implement singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Add two AudioSource components
            track1Source = gameObject.AddComponent<AudioSource>();
            track2Source = gameObject.AddComponent<AudioSource>();

            // Configure audio sources
            track1Source.loop = true;
            track2Source.loop = true;

            if (rainIndoorsClip == null)
            {
                rainIndoorsClip = Resources.Load<AudioClip>("Audio/rain_indoors");
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckAndApplyRainIndoors(scene.name);
    }

    public static bool IsOutdoorScene(string sceneName)
    {
        return sceneName == GameConsts.OUTDOORSSCENE ||
               sceneName == GameConsts.BURIALSCENE ||
               sceneName == GameConsts.PIERSCENE;
    }

    public void CheckAndApplyRainIndoors(string sceneName = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName = SceneManager.GetActiveScene().name;
        }

        bool isThunderstorm = GameState.Get<bool>("thunderstorm", false);
        if (isThunderstorm && !IsOutdoorScene(sceneName))
        {
            if (rainIndoorsClip == null)
            {
                rainIndoorsClip = Resources.Load<AudioClip>("Audio/rain_indoors");
            }

            if (rainIndoorsClip != null)
            {
                if (GetCurrentClip(2) != rainIndoorsClip)
                {
                    PlayTrack(rainIndoorsClip, 0.35f, 2);
                }
            }
        }
    }
    void Start()
    {
        PlayTrack(newTrack1, 0.5f, 1); // Play track 1 at 50% volume

        // Decide which clip to use for track 2 based on weather state
        bool isThunderstorm = GameState.Get<bool>("thunderstorm", false);
        string sceneName = SceneManager.GetActiveScene().name;
        bool useRain = isThunderstorm && !IsOutdoorScene(sceneName);

        if (useRain && rainIndoorsClip != null)
        {
            PlayTrack(rainIndoorsClip, 0.35f, 2);
        }
        else
        {
            PlayTrack(newTrack2, 0.15f, 2); // Play track 2 at 15% volume
        }
    }

    // Play a new track on the specified channel (1 or 2)
    public void PlayTrack(AudioClip clip, float volume = 1f, int channel = 1)
    {
        if (channel < 1 || channel > 2)
        {
            Debug.LogWarning("Invalid channel number. Use 1 or 2.");
            return;
        }

        AudioSource targetSource = channel == 1 ? track1Source : track2Source;

        if (clip != null)
        {
            targetSource.clip = clip;
            targetSource.volume = Mathf.Clamp01(volume);
            targetSource.Play();
        }
        else
        {
            Debug.LogWarning("Attempted to play null AudioClip.");
        }
    }

    // Stop the track on the specified channel
    public void StopTrack(int channel)
    {
        if (channel < 1 || channel > 2)
        {
            Debug.LogWarning("Invalid channel number. Use 1 or 2.");
            return;
        }

        AudioSource targetSource = channel == 1 ? track1Source : track2Source;
        targetSource.Stop();
        targetSource.clip = null;
    }

    // Set volume for the specified channel
    public void SetVolume(int channel, float volume)
    {
        if (channel < 1 || channel > 2)
        {
            Debug.LogWarning("Invalid channel number. Use 1 or 2.");
            return;
        }

        AudioSource targetSource = channel == 1 ? track1Source : track2Source;
        targetSource.volume = Mathf.Clamp01(volume);
    }

    // Get current volume for the specified channel
    public float GetVolume(int channel)
    {
        if (channel < 1 || channel > 2)
        {
            Debug.LogWarning("Invalid channel number. Use 1 or 2.");
            return 0f;
        }

        AudioSource targetSource = channel == 1 ? track1Source : track2Source;
        return targetSource.volume;
    }

    // Get current clip for the specified channel
    public AudioClip GetCurrentClip(int channel)
    {
        if (channel < 1 || channel > 2)
        {
            Debug.LogWarning("Invalid channel number. Use 1 or 2.");
            return null;
        }

        AudioSource targetSource = channel == 1 ? track1Source : track2Source;
        return targetSource.clip;
    }

    // Get the singleton instance
    public static Ambience Instance
    {
        get { return instance; }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

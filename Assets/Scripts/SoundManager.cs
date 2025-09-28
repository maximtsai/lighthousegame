using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Sound
{
    public string name;       // Name to call this sound
    public AudioClip clip;    // Audio clip
    [Range(0f, 1f)]
    public float volume = 1f; // Default volume
    public bool loop = false; // Should this sound loop?

    [HideInInspector]
    public AudioSource source; // Runtime AudioSource
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public List<Sound> sounds = new List<Sound>();

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Create AudioSources for each sound
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
        }
    }

    // Play a sound by name
    public static void Play(string name)
    {
        if (Instance == null)
        {
            Debug.LogWarning("SoundManager instance not found!");
            return;
        }

        Sound s = Instance.sounds.Find(sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound not found: " + name);
            return;
        }

        s.source.Play();
    }

    // Stop a sound by name
    public static void Stop(string name)
    {
        if (Instance == null) return;

        Sound s = Instance.sounds.Find(sound => sound.name == name);
        if (s != null && s.source.isPlaying)
        {
            s.source.Stop();
        }
    }
}
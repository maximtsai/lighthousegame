using UnityEngine;
using UnityEngine.SceneManagement; // required for SceneManager
using System.Collections;
using UnityEngine.UI;

public class Navigation : MonoBehaviour
{
    public static Navigation Instance { get; private set; }

    [SerializeField] private Image blackoutImage;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Get Image on prefab
        if (blackoutImage == null)
        {
            blackoutImage = gameObject.AddComponent<Image>();
            Debug.LogWarning("Navigation prefab had no Image. Added one dynamically.");
        }
    }

    public void GoToBedroom()
    {
        SceneManager.LoadScene("BedroomScene");
    }

    public void GoTo(string scene)
    {
        if (GameState.Get<bool>("task_list_open", false) || GameState.Get<bool>("minigame_open"))
            return;

        GoToTransition(scene, 0.35f);
    }
    
    public void GoToSink()
    {
        if (GameState.Get<string>("is_clean") == "true")
        {
            DialogueManager.ShowDialogue(getDialog("Bedroom/already_washed"));

            return;
        }

        GoToTransition("SinkScene", 0.25f);
    }

    public void GoToSlow(string scene)
    {
        if (GameState.Get<bool>("task_list_open", false) || GameState.Get<bool>("minigame_open"))
            return;

        GoToTransition(scene, 0.85f);
    }
    
    private void GoToTransition(string scene, float duration)
    {
        // Ensure the Navigation instance exists
        if (Instance == null)
        {
            // Load the Navigation prefab from Resources
            GameObject prefab = Resources.Load<GameObject>("Navigation");
            if (prefab == null)
            {
                Debug.LogError("Navigation prefab not found in Resources folder!");
                return;
            }

            GameObject go = Instantiate(prefab);
            Instance = go.GetComponent<Navigation>();
        }

        Instance.StartCoroutine(Instance.FadeInThenGoTo(scene, duration));
    }
    
    private IEnumerator FadeInThenGoTo(string scene, float duration)
    {
        // Start fully transparent
        Color c = new Color(0, 0, 0, blackoutImage.color.a);
        c.a = 0f;
        blackoutImage.color = c;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / duration) + 0.1f;
            blackoutImage.color = c;
            yield return null;
        }

        c.a = 1f;
        blackoutImage.color = c;

        SceneManager.LoadScene(scene);

        t += 0.15f;
        while (t > 0)
        {
            t -= Time.deltaTime * 2.2f;
            c.a = Mathf.Clamp01(t / duration);
            blackoutImage.color = c;
            yield return null;
        }

        // Destroy the Navigation object in the old scene
        Destroy(gameObject);

        // Clear the singleton reference
        Instance = null;
    }
    
    // Sorry J3ranch, I needed some additional custom logic for this function
    public void LeaveBedroom(SceneTransition transition)
    {
        // query GameState static class and check value 
        // optionally play dialogue if go to is not possible via DialogueManager
        // Debug.Log(transition.gamestate_key + " " + GameState.Get<string>(transition.gamestate_key, "false"));
        if (GameState.Get<int>("day") > 1)
        {
            playSoundClip(transition.travelSound);
            GoTo(transition.destination_scene);
        }
        if (GameState.Get<string>(transition.gamestate_key, "false") == transition.gamestate_value)
        {
            playSoundClip(transition.travelSound);
            GoTo(transition.destination_scene);
        }
        else if (null != transition.dialogue_on_stay)
        {
            DialogueManager.ShowDialogue(transition.dialogue_on_stay);
        }
    }

    private void playSoundClip(AudioClip clip)
    {
        if (clip)
        {
            // Play sound if it's available
            if (AudioManager.Instance)
            {
                AudioSource audioSource = AudioManager.Instance.AudioSource;
                audioSource.clip = clip;
                audioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning("No AudioSource found in the scene!");
            }
        }
    }

    private Dialogue getDialog(string name)
    {
        string fullPath = "ScriptableObjects/Dialogues/" + name;
        Dialogue dialogue = Resources.Load<Dialogue>(fullPath);
        
        if (dialogue == null)
        {
            Debug.LogWarning("Dialogue not found: " + fullPath);
        }
        return dialogue;
    }
    
}

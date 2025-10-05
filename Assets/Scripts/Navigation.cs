using UnityEngine;
using UnityEngine.SceneManagement; // required for SceneManager
using System.Collections;

public class Navigation : MonoBehaviour
{
    public static Navigation Instance { get; private set; }

    [SerializeField] private SpriteRenderer srender;

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

        // Get SpriteRenderer on prefab
        srender = GetComponent<SpriteRenderer>();
        if (srender == null)
        {
            srender = gameObject.AddComponent<SpriteRenderer>();
            Debug.LogWarning("Navigation prefab had no SpriteRenderer. Added one dynamically.");
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
        Color c = srender.color;
        c.a = 0f;
        srender.color = c;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / duration) + 0.1f;
            srender.color = c;
            yield return null;
        }

        c.a = 1f;
        srender.color = c;

        SceneManager.LoadScene(scene);

        t += 0.15f;
        while (t > 0)
        {
            t -= Time.deltaTime * 2.2f;
            c.a = Mathf.Clamp01(t / duration);
            srender.color = c;
            yield return null;
        }

        // Destroy the Navigation object in the old scene
        Destroy(gameObject);

        // Clear the singleton reference
        Instance = null;
    }

    public void GoToIfGameState(SceneTransition transition)
    {
        // query GameState static class and check value 
        // optionally play dialogue if go to is not possible via DialogueManager
        Debug.Log(transition.gamestate_key + " " + GameState.Get<string>(transition.gamestate_key, "false"));
        if (GameState.Get<string>(transition.gamestate_key, "false") == transition.gamestate_value)
        {
            GoTo(transition.destination_scene);
        }
        else if (null != transition.dialogue_on_stay)
        {
            DialogueManager.ShowDialogue(transition.dialogue_on_stay);
        }
    }
}

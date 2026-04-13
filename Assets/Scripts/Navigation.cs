using System;
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
        SceneManager.LoadScene(GameConsts.BEDROOMSCENE);
    }

    public void GoTo(string scene)
    {
        if (GameState.Get<bool>("task_list_open", false) || GameState.Get<bool>("minigame_open"))
            return;

        GoToTransition(scene, 0.35f);
    }

    public void GoToStove()
    {
        if (!GameState.Get<bool>("hungry"))
        {
            DialogueManager.ShowDialogue(getDialog("kitchen/not_hungry"));
            return;
        }

        GoToTransition(GameConsts.STOVESCENE, 0.25f);
    }

    public void GoToIndoors()
    {
        if (GameState.Get<bool>("do_burial", false))
        {
            DialogueManager.ShowDialogue(getDialog("outdoors/burial_blocked"));
            return;
        }
        
        if (GameState.Get<bool>("lighthouse_fixed") && !GameState.Get<bool>("gathered_fish"))
        {
            if (GameState.Get<int>("day") == 2)
            {
                DialogueManager.ShowDialogueFromText(new string[] { "I haven't checked the fish traps yet." });
            }
            else
            {
                DialogueManager.ShowDialogue(getDialog("outdoors/missing_fish"));
            }
            return;
        }

        // Day 2 specific block: Must check the grave before going inside
        bool isDay2 = GameState.Get<int>("day") == 2;
        if (isDay2 && GameState.Get<bool>("lighthouse_fixed") && GameState.Get<bool>("gathered_fish") && !GameState.Get<bool>("grave_inspected"))
        {
            DialogueManager.ShowDialogueFromText(new string[] { "Something's wrong with the grave." });
            return;
        }

        GoToTransition(GameConsts.KITCHENSCENE, 0.35f);
    }

    public void GoToLighthouse()
    {
        if (GameState.Get<bool>("do_burial", false))
        {
            DialogueManager.ShowDialogue(getDialog("outdoors/burial_blocked"));
            return;
        }
        if (GameState.Get<bool>("ready_to_sleep", false))
        {
            DialogueManager.ShowDialogue(getDialog("time_for_bed"));
            return;
        }
        GoToTransition(GameConsts.LHFLOORSCENE, 0.35f);
    }

    public void GoToPier()
    {
        if (GameState.Get<bool>("do_burial", false))
        {
            DialogueManager.ShowDialogue(getDialog("outdoors/burial_blocked"));
            return;
        }
        if (GameState.Get<bool>("ready_to_sleep", false))
        {
            DialogueManager.ShowDialogue(getDialog("time_for_bed"));
            return;
        }
        GoToTransition(GameConsts.PIERSCENE, 0.35f);
    }

    public void GoToBurial()
    {
        // Day 2: Allow access to inspect the uncovered grave
        bool isDay2 = GameState.Get<int>("day") == 2;
        bool graveNeedsInspection = isDay2 && !GameState.Get<bool>("grave_inspected") && GameState.Get<bool>("lighthouse_fixed") && GameState.Get<bool>("gathered_fish");

        if (!GameState.Get<bool>("do_burial", false) && !graveNeedsInspection)
        {
            DialogueManager.ShowDialogue(getDialog("outdoors/resting_place"));
            return;
        }
        if (GameState.Get<bool>("ready_to_sleep", false))
        {
            DialogueManager.ShowDialogue(getDialog("time_for_bed"));
            return;
        }
        GoToTransition(GameConsts.BURIALSCENE, 0.5f);
    }

    
    public void GoToSink(SceneTransition transition)
    {
        if (GameState.Get<string>("is_clean") == "true")
        {
            DialogueManager.ShowDialogue(getDialog("Bedroom/already_washed"));
            return;
        }

        playSoundClip(transition.travelSound);
        GoToTransition(GameConsts.SINKSCENE, 0.25f);
    }
    
    public void GoToOutdoors(SceneTransition transition)
    {
        if (GameState.Get<bool>("ready_to_sleep", false))
        {
            DialogueManager.ShowDialogue(getDialog("kitchen/time_for_bed"));
        } else if (!GameState.Get<bool>("ate_breakfast"))
        {
            if (GameState.Get<bool>("corn_clicked"))
            {
                DialogueManager.ShowDialogue(getDialog("kitchen/hungry_forgot"));
            }
            else
            {
                DialogueManager.ShowDialogue(getDialog("kitchen/hungry"));
            }
            return;
        }

        playSoundClip(transition.travelSound);
        
        bool cutscenePlayed = GameState.Get<bool>("cutscene_outdoors_played", false) || GameState.Get<int>("day", 1) > 1;
        if (!cutscenePlayed)
        {
            GameState.Set("cutscene_outdoors_played", true);
            MessageBus.Instance.Publish("PlayCutscene", "Lighthouse", true, (Action)(() =>
            {
                Debug.Log("going to outdoors");
                GoToTransition(GameConsts.OUTDOORSSCENE, 0.35f);
                
            }), true);
        }
        else
        {
            GoToTransition(GameConsts.OUTDOORSSCENE, 0.35f);
        }
        
        
    }
    
    public void GoToUpstairs(SceneTransition transition)
    {
        if (GameState.Get<bool>("lighthouse_fixed") && !GameState.Get<bool>("ate_dinner"))
        {
            DialogueManager.ShowDialogue(getDialog("Kitchen/hungry_dinner"));
            return;
        }

        playSoundClip(transition.travelSound);
        GoToTransition(GameConsts.BEDROOMSCENE, 0.35f);
    }

    public void GoToMainMenu()
    {
        MessageBus.Instance.Publish("ResetToMainMenu");
        GameState.StartNewDay();
        GameState.Set("pause_open", false);
        GoToTransition(GameConsts.MAINSCENE, 0.3f);
    }

    public void GoToJournal()
    {
        GoToTransition(GameConsts.JOURNALSCENE, 0.35f);
    }
    
    public void GoToSlow(string scene)
    {
        if (GameState.Get<bool>("task_list_open", false) || GameState.Get<bool>("minigame_open"))
            return;

        GoToTransition(scene, 0.85f);
    }
    
    private void GoToTransition(string scene, float duration)
    {
        if (GameState.Get<bool>("navigationBlocked"))
        {
            Debug.Log("Navigation blocked");
            return;
        }
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

        GameState.Set("navigationBlocked", true);
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

        GameState.Set("navigationBlocked", false);
        // Destroy the Navigation object in the old scene
        // Destroy(gameObject);

        // Clear the singleton reference
        // Instance = null;
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
            return;
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

    public void BuryCampborne(SceneTransition transition)
    {
        MessageBus.Instance.Publish("ClearAllTasks");
        GameState.Set("do_burial", true); // activate burial flag so we can bury this guy
        playSoundClip(transition.travelSound);
        GoToTransition(GameConsts.OUTDOORSSCENE, 0.8f);
        
        MessageBus.Instance.Publish("AddTaskString", "generic/bury_body");
        MessageBus.Instance.Publish("AddTaskString", "generic/go_to_sleep");
    }

    private void playSoundClip(AudioClip clip)
    {
        if (clip)
        {
            // Play sound if it's available
            if (AudioManager.Instance && !GameState.Get<bool>("navigationBlocked"))
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

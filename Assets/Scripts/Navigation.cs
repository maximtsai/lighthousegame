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
        if (Day4Beach.BlocksHeadingIn)
        {
            DialogueManager.ShowDialogueFromText(new string[] { Day4Beach.BlockedLine });
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
        if (Day4Beach.BlocksHeadingIn)
        {
            DialogueManager.ShowDialogueFromText(new string[] { Day4Beach.BlockedLine });
            return;
        }
        if (GameState.Get<bool>("ready_to_sleep", false))
        {
            DialogueManager.ShowDialogue(getDialog("time_for_bed"));
            return;
        }
        if (!GameState.Get<bool>("recorded_weather", false))
        {
            DialogueManager.ShowDialogueFromText(new string[] { "You need to record the weather first" });
            return;
        }

        LighthouseClimb.Reset();
        GoToTransition(GameConsts.LHFLOORSCENE, 0.35f);
    }

    public void EnterLighthouseAscent()
    {
        if (LighthouseNavigationBlocked())
            return;

        LighthouseClimb.EnterFromGround();
        GoToSlow(GameConsts.LHCLIMBSCENE);
    }

    public void EnterLighthouseDescent()
    {
        if (LighthouseNavigationBlocked())
            return;

        LighthouseClimb.EnterFromLightRoom();
        GoToSlow(GameConsts.LHCLIMBSCENE);
    }

    public void GoLighthouseUp()
    {
        if (LighthouseNavigationBlocked())
            return;

        if (LighthouseScissors.BlocksClimbUp(LighthouseClimb.Floor, LighthouseClimb.GoingUp))
        {
            DialogueManager.ShowDialogueFromText(new string[] { LighthouseScissors.NoticedLine });
            return;
        }

        string scene = LighthouseClimb.StepUp();

        // Fade through the footsteps, then keep black only for the brick drop sound.
        if (scene == GameConsts.LHCLIMBSCENE)
        {
            LighthouseBrick.Arrival arrival =
                LighthouseBrick.Arrive(LighthouseClimb.Floor, LighthouseClimb.GoingUp);

            if (arrival == LighthouseBrick.Arrival.Fell)
            {
                LHClimbScript climb = FindFirstObjectByType<LHClimbScript>();
                EnsureInstance();
                if (climb != null && Instance != null)
                {
                    GameState.Set("navigationBlocked", true);
                    Instance.StartCoroutine(Instance.PlayBrickDrop(climb));
                    return;
                }
            }
        }

        GoToSlow(scene);
    }

    private IEnumerator PlayBrickDrop(LHClimbScript climb)
    {
        Color c = blackoutImage.color;
        c.a = 0f;
        blackoutImage.color = c;

        // The arrow already started the footsteps; let them finish before the brick drops.
        float footstepsDuration = climb.FootstepsDuration;
        float elapsed = 0f;
        while (elapsed < footstepsDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / footstepsDuration);
            blackoutImage.color = c;
            yield return null;
        }

        c.a = 1f;
        blackoutImage.color = c;
        climb.PlayBrickDrop();

        if (climb.BrickDropDuration > 0f)
            yield return new WaitForSeconds(climb.BrickDropDuration);

        climb.ShowFloor(LighthouseClimb.Floor, LighthouseClimb.GoingUp);
        c.a = 0f;
        blackoutImage.color = c;
        GameState.Set("navigationBlocked", false);
    }

    // Clicking the brick on the floor pushes it back into the wall. The climb scene is already
    // loaded, so this fades out and in over the same scene rather than reloading it.
    public static void PlaceBrickInWall()
    {
        if (LighthouseBrick.InWall || GameState.Get<bool>("navigationBlocked"))
            return;

        // The scissors are sparkling in the hole the brick left; they have to come out first.
        if (LighthouseScissors.Stuck)
        {
            DialogueManager.ShowDialogueFromText(new string[] { LighthouseScissors.NoticedLine });
            return;
        }

        LighthouseBrick.PutInWall();

        LHClimbScript climb = FindFirstObjectByType<LHClimbScript>();
        if (climb != null)
            climb.PlayBrickPlace();

        EnsureInstance();
        if (Instance != null)
        {
            float fadeOut = climb != null ? climb.BrickPlaceFadeOutDuration() : 0.5f;
            float hold = climb != null ? climb.BrickPlaceHoldDuration() : 0f;
            Instance.GoToTransition(GameConsts.LHCLIMBSCENE, fadeOut, 0f, hold);
        }
    }

    // The scissors close-up is part of the climb scene, so like the brick this fades over the
    // same scene and the climb script swaps the view while it's black.
    public static void OpenScissorsCloseUp()
    {
        if (LighthouseScissors.CloseUpOpen || GameState.Get<bool>("navigationBlocked"))
            return;

        LighthouseScissors.SetCloseUpOpen(true);
        FadeClimbScene(0.35f);
    }

    // The close-up fades itself away over the stairs, so this just swaps back to them.
    public static void CloseScissorsCloseUp()
    {
        LighthouseScissors.SetCloseUpOpen(false);
        LHClimbScript climb = FindFirstObjectByType<LHClimbScript>();
        if (climb != null)
            climb.ShowFloor(LighthouseClimb.Floor, LighthouseClimb.GoingUp);
    }

    private static void FadeClimbScene(float duration)
    {
        EnsureInstance();
        if (Instance != null)
            Instance.GoToTransition(GameConsts.LHCLIMBSCENE, duration);
    }

    public void GoLighthouseDown()
    {
        if (LighthouseNavigationBlocked())
            return;

        GoToSlow(LighthouseClimb.StepDown());
    }

    private static bool LighthouseNavigationBlocked()
    {
        return GameState.Get<bool>("navigationBlocked") || GameState.Get<bool>("task_list_open") || GameState.Get<bool>("minigame_open");
    }

    public void GoToPier()
    {
        if (GameState.Get<bool>("do_burial", false))
        {
            DialogueManager.ShowDialogue(getDialog("outdoors/burial_blocked"));
            return;
        }
        if (Day4Beach.BlocksPier)
        {
            DialogueManager.ShowDialogueFromText(new string[] { Day4Beach.BlockedLine });
            return;
        }
        if (GameState.Get<bool>("ready_to_sleep", false))
        {
            DialogueManager.ShowDialogue(getDialog("time_for_bed"));
            return;
        }
        MessageBus.Instance.Publish("PlaySound", "wooden_steps3");
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
        bool handNeedsCleaning = GameState.Get<bool>("hand_cut") && !GameState.Get<bool>("hand_cleaned");
        if (GameState.Get<string>("is_clean") == "true" && !handNeedsCleaning)
        {
            DialogueManager.ShowDialogue(getDialog("Bedroom/already_washed"));
            return;
        }

        if (transition != null && transition.travelSound != null)
        {
            playSoundClip(transition.travelSound);
        }
        else
        {
            MessageBus.Instance.Publish("PlaySound", "floorboard_creak");
        }
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
                GoToTransition(GameConsts.OUTDOORSSCENE, 0.35f, 1f);
                
            }), true);
        }
        else
        {
            GoToTransition(GameConsts.OUTDOORSSCENE, 0.35f);
        }
        
        
    }
    
    public void GoToUpstairs(SceneTransition transition)
    {
        if (GameState.Get<int>("day") == 1 && GameState.Get<bool>("ate_breakfast") && !GameState.Get<bool>("lighthouse_fixed"))
        {
            DialogueManager.ShowDialogueFromText(new string[] { "It's time for work now." });
            return;
        }

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

    public void ClickCalendar()
    {
        int day = GameState.Get<int>("day", 1);
        DialogueManager.ShowDialogueFromText(new string[] { "Today is Day " + day + "." });
    }

    
    public void GoToSlow(string scene)
    {
        if (GameState.Get<bool>("task_list_open", false) || GameState.Get<bool>("minigame_open"))
            return;

        GoToTransition(scene, 0.85f);
    }
    
    private void GoToTransition(string scene, float duration, float startAlpha = 0f, float holdBlack = 0f)
    {
        if (GameState.Get<bool>("navigationBlocked"))
        {
            Debug.Log("Navigation blocked");
            return;
        }

        // Intercept leaving LHFloorScene to OutdoorsScene on Day 3 while looking for scissors
        if (SceneManager.GetActiveScene().name == GameConsts.LHFLOORSCENE
            && scene == GameConsts.OUTDOORSSCENE
            && GameState.Get<int>("day") == 3
            && GameState.Get<bool>("scissorsDrop", false)
            && (!GameState.Get<bool>("lighthouse_fixed", false) || (TaskManager.instance != null && TaskManager.instance.GetCurrentTasks().Exists(t => t.id == "task_find_scissors" || t.id == "task_finish_maintenance"))))
        {
            DialogueManager.ShowDialogueFromText(new string[] { "The scissors have to be somewhere in the lighthouse." });
            return;
        }

        // Intercept leaving LHFloorScene to OutdoorsScene while the Fix Lighthouse task is still active
        if (SceneManager.GetActiveScene().name == GameConsts.LHFLOORSCENE
            && scene == GameConsts.OUTDOORSSCENE
            && TaskManager.instance != null
            && TaskManager.instance.GetCurrentTasks().Exists(t => t.id == "task_lighthouse"))
        {
            DialogueManager.ShowDialogueFromText(new string[] { "You have work to do.", "Head upstairs." });
            return;
        }

        // Intercept leaving LHFloorScene to OutdoorsScene on Day 2 if lighthouse is fixed and stain question not answered
        if (SceneManager.GetActiveScene().name == GameConsts.LHFLOORSCENE
            && scene == GameConsts.OUTDOORSSCENE
            && GameState.Get<int>("day") == 2
            && GameState.Get<bool>("lighthouse_fixed")
            && !GameState.Get<bool>("stain_question_answered", false))
        {
            TriggerStainQuestion();
            return;
        }

        // Intercept leaving BurialScene to OutdoorsScene on Day 2 if grave_revealed is true
        if (SceneManager.GetActiveScene().name == GameConsts.BURIALSCENE
            && scene == GameConsts.OUTDOORSSCENE
            && GameState.Get<int>("day") == 2
            && GameState.Get<bool>("grave_revealed", false))
        {
            DialogueManager.ShowDialogueFromText(new string[] { "The weather has undone your shoddy grave.## Go fix your mistake." });
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == GameConsts.PIERSCENE && scene != GameConsts.PIERSCENE)
        {
            MessageBus.Instance.Publish("PlaySound", "wooden_steps3");
        }

        EnsureInstance();
        if (Instance == null)
            return;

        GameState.Set("navigationBlocked", true);
        Instance.StartCoroutine(Instance.FadeInThenGoTo(scene, duration, startAlpha, holdBlack));
    }
    
    private static void EnsureInstance()
    {
        if (Instance != null)
            return;

        GameObject prefab = Resources.Load<GameObject>("Navigation");
        if (prefab == null)
        {
            Debug.LogError("Navigation prefab not found in Resources folder!");
            return;
        }

        Instance = Instantiate(prefab).GetComponent<Navigation>();
    }

    private IEnumerator FadeInThenGoTo(string scene, float duration, float startAlpha = 0f, float holdBlack = 0f)
    {
        // Start from specified alpha
        Color c = new Color(0, 0, 0, blackoutImage.color.a);
        c.a = startAlpha;
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

        if (scene == SceneManager.GetActiveScene().name && scene == GameConsts.LHCLIMBSCENE)
        {
            LHClimbScript climb = FindFirstObjectByType<LHClimbScript>();
            if (climb != null)
                climb.ShowFloor(LighthouseClimb.Floor, LighthouseClimb.GoingUp);
            else
                SceneManager.LoadScene(scene);
        }
        else
        {
            SceneManager.LoadScene(scene);
        }

        if (holdBlack > 0f)
            yield return new WaitForSeconds(holdBlack);

        // Long brick fades use the usual climb fade-in so coming back isn't sluggish.
        float fadeIn = holdBlack > 0f ? 0.85f : duration;
        t = fadeIn + 0.15f;
        while (t > 0)
        {
            t -= Time.deltaTime * 2.2f;
            c.a = Mathf.Clamp01(t / fadeIn);
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

    private void TriggerStainQuestion()
    {
        Dialogue d = DialogueManager.ShowDialogueFromText(new string[] { "Wait.", "Has that crack above the door always been there?" });
        d.onDialogueEnd.AddListener(() =>
        {
            MessageBus.Instance.Publish("ShowChoiceDialog", "Has that crack above the door always been there?");
            
            MessageBus.Instance.Publish(
                "ShowTwoChoice",
                "YES",
                "NO",
                (Action)(() =>
                {
                    MessageBus.Instance.Publish("FloatText", 0f, 0.3f, "+SANITY", "green");
                    MessageBus.Instance.Publish("PlusSanity", 1);
                    GameState.Set("stain_question_answered", true);
                }),
                (Action)(() =>
                {
                    MessageBus.Instance.Publish("FloatText", 0f, 0.3f, "-SANITY", "purple");
                    MessageBus.Instance.Publish("PlusSanity", -2);
                    GameState.Set("stain_question_answered", true);
                })
            );
        });
    }
    
}

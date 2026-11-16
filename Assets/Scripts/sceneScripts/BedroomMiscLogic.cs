using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BedroomMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    [SerializeField] private AudioClip wakeupSound;

    [Header("Day 4: Mermaid In Camborne's Bed")]
    [SerializeField] private Sprite mermaidOnBedDay;
    [SerializeField] private Sprite mermaidOnBedNight;
    [Tooltip("Shown over her while Camborne's bed is hovered.")]
    [SerializeField] private Sprite mermaidOnBedOutline;
    [SerializeField] private Vector3 mermaidOnBedPosition = new Vector3(-2.8f, -0.1f, -1.1f);

    [Header("Day 5: Dream")]
    [Tooltip("Her as a rotting corpse, for the jumpscare. Placeholder: the Day 7 body. The peaceful half " +
             "of the dream uses Mermaid On Bed Night.")]
    [SerializeField] private Sprite day5DreamCorpse;

    private const string Day6FlashSeenKey = "day6_lighthouse_flash_seen";
    // At or below this the Day 6 flash comes in red. Same cut-off as the mirror's two lowest lines.
    private const int LowSanity = 0;
    private const string Day6NoFlowersChargedKey = "day6_no_flowers_charged";

    void Start()
    {
        gameObject.AddComponent<MermaidOnBed>().Init(mermaidOnBedDay, mermaidOnBedNight, mermaidOnBedOutline, mermaidOnBedPosition);

        if (Ambience.Instance != null) {
            Ambience ambience = Ambience.Instance;
    
            // Update track 1
            UpdateTrack(ambience, bgLoop1, 0.6f, 1);

            bool isThunderstorm = GameState.Get<bool>("thunderstorm", false);
            if (isThunderstorm)
            {
                ambience.CheckAndApplyRainIndoors(GameConsts.BEDROOMSCENE);
            }
            else
            {
                // Update track 2
                UpdateTrack(ambience, bgLoop2, 0.21f, 2);
            }
        }
        Debug.Log("bedroom misc logic");

        BeginDay();

        // Day 6 night: the lighthouse he's been ignoring flashes through the window
        if (GameState.Get<int>("day") == 6 && GameState.Get<bool>("is_nighttime")
            && !GameState.Get<bool>(Day6FlashSeenKey, false))
        {
            StartCoroutine(Day6LighthouseFlash());
        }

        if (GameState.Get<bool>("hand_cut") && !GameState.Get<bool>("hand_cleaned"))
        {
            if (GameState.Get<int>("day") != 3)
            {
                DialogueManager.ShowDialogueFromText(new string[] { "I should bandage my hand." });
            }
        }
    }

    // Two sweeps of light across the room, like the beam turning past. He takes it for lightning.
    private IEnumerator Day6LighthouseFlash()
    {
        // Hold the room still from the moment he walks in, so he can't get into bed (and into the
        // next day's cutscene) before the flash has played
        GameState.Set(Day6FlashSeenKey, true);
        GameState.Set("navigationBlocked", true);
        yield return new WaitForSeconds(1.2f);
        while (DialogueManager.DialogueIsOpen())
            yield return null;

        GameObject root = new GameObject("Day6LighthouseFlash");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // Under the dialogue box (100)
        canvas.sortingOrder = 50;

        GameObject flashObject = new GameObject("flash");
        flashObject.transform.SetParent(root.transform, false);
        RectTransform rect = flashObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image flash = flashObject.AddComponent<Image>();
        // Low sanity: the light that sweeps the room is red, the lighthouse's own colour. He
        // still calls it lightning.
        bool red = GameState.Get<int>("sanity") <= LowSanity;
        flash.color = red ? new Color(0.85f, 0.08f, 0.05f, 0f) : new Color(1f, 0.97f, 0.85f, 0f);
        flash.raycastTarget = false;

        for (int i = 0; i < 2; i++)
        {
            yield return FadeFlash(flash, 0f, 0.55f, 0.08f);
            yield return FadeFlash(flash, 0.55f, 0f, 0.5f);
            yield return new WaitForSeconds(0.6f);
        }

        Destroy(root);
        GameState.Set("navigationBlocked", false);

        // Storm clouds is what he wrote down this morning, if he got it right, so it must be lightning
        List<string> lines = new List<string> { "A flash through the window." };
        if (GameState.Get<bool>("recorded_weather") && GameState.Get<bool>("weather_correct"))
            lines.Add("Cumulonimbus.## Called it.");
        lines.Add("...No thunder.");
        lines.Add("Just lightning.");
        DialogueManager.ShowDialogueFromText(lines.ToArray());
    }

    private static IEnumerator FadeFlash(Image image, float from, float to, float duration)
    {
        Color c = image.color;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(from, to, t / duration);
            image.color = c;
            yield return null;
        }
        c.a = to;
        image.color = c;
    }

    private void BeginDay()
    {
        if (GameState.Get<bool>("day_began"))
            return;

        GameState.Set("day_began", true);

        int day = GameState.Get<int>("day");
        Debug.Log("Begin day " + day);

        // Day 5 wakes from its dream first, so the sound waits until then
        if (day != Day5.Day)
            PlayWakeupSound();
        switch (day)
        {
            case 1:
                MessageBus.Instance.Publish("AddTaskString", "generic/task_wash_up");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_breakfast");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_weather");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_lighthouse");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_fish");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_dinner");
                break;
            case 2:
                MessageBus.Instance.Publish("AddTaskString", "generic/task_breakfast");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_weather");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_work");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_fish");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_dinner");
                MessageBus.Instance.Publish("AddTaskString", "generic/go_to_sleep");
                break;
            case 3:
                MessageBus.Instance.Publish("AddTaskString", "generic/task_wash_hand");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_breakfast");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_weather");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_work");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_find_scissors");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_finish_maintenance");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_fish");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_dinner");
                MessageBus.Instance.Publish("AddTaskString", "generic/go_to_sleep");
                ShowDay3Dialogue();
                break;
            case 4:
                MessageBus.Instance.Publish("AddTaskString", "generic/task_breakfast");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_weather");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_work");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_survey_beach");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_bury_bodies");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_fish");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_dinner");
                MessageBus.Instance.Publish("AddTaskString", "generic/go_to_sleep");
                break;
            case 5:
                // Her first, then the routine. "Do Your Job" is swapped for her flowers once he's
                // outside (OutdoorsMiscLogic), and the fish are for her today.
                MessageBus.Instance.Publish("AddTaskString", "generic/task_tend_her");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_breakfast");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_weather");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_work");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_fish_her");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_dinner");
                MessageBus.Instance.Publish("AddTaskString", "generic/go_to_sleep");
                // The dream of her rotting beside him, then waking to find her still there. After a
                // Continue the dream's already been seen, so it's straight to waking.
                System.Action wake = () =>
                {
                    PlayWakeupSound();
                    DialogueManager.ShowDialogueFromText(new string[]
                    {
                        "Just a dream.",
                        "She's still there.## Still breathing.",
                        "She's not rotting.## She isn't.",
                        "I won't let her.",
                        "Something is wriggling out of her legs."
                    });
                };
                if (GameState.Get<bool>(Day5.DreamSeenKey, false))
                    wake();
                else
                    Day5Dream.Play(mermaidOnBedNight, day5DreamCorpse, wake);
                break;
            case 6:
                // Same shape as Day 5: her first, flowers instead of the lighthouse (swapped in by
                // OutdoorsMiscLogic), the fish are for her
                MessageBus.Instance.Publish("AddTaskString", "generic/task_tend_her");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_breakfast");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_weather");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_work");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_fish_her");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_dinner");
                MessageBus.Instance.Publish("AddTaskString", "generic/go_to_sleep");
                // Kept no flowers for her yesterday (FlowerMinigame.KeptKey on Day 5): nothing to cover
                // the smell, and he knows whose fault that is
                bool noFlowers = Day5.NoFlowersOnHer;
                List<string> wakeLines = new List<string>
                {
                    "The smell wakes you.",
                    "It's her."
                };
                if (noFlowers)
                {
                    wakeLines.Add("No flowers to cover it.");
                    wakeLines.Add("Should've picked some.");
                }
                wakeLines.Add("She needs me.");
                Dialogue wakeDialogue = DialogueManager.ShowDialogueFromText(wakeLines.ToArray());
                // Only charged once: Continue replays the morning from the day-start save
                if (noFlowers && !GameState.Get<bool>(Day6NoFlowersChargedKey, false))
                {
                    wakeDialogue.onDialogueEnd.AddListener(() =>
                    {
                        GameState.Set(Day6NoFlowersChargedKey, true);
                        SaveManager.SaveFlag(Day6NoFlowersChargedKey, true);
                        MessageBus.Instance.Publish("FloatText", 0f, 0.3f, "-SANITY", "purple");
                        MessageBus.Instance.Publish("PlusSanity", -1);
                    });
                }
                break;
            case 7:
                break;
        }
    }

    private void ShowDay3Dialogue()
    {
        // Phase 1: Plain dialogue with no choices
        Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
        dialogue.text = new List<string>(new string[] 
        { 
            "You take a look at your hand.", 
            "Feels like bugs are crawling underneath it." 
        });
        dialogue.choices = new List<string>();
        dialogue.consequences = new List<UnityEngine.Events.UnityEvent>();
        dialogue.onDialogueEnd = new UnityEngine.Events.UnityEvent();
        dialogue.onDialogueEndImmediate = new UnityEngine.Events.UnityEvent();

        // When player clicks through the last line, show a second dialogue with choices
        dialogue.onDialogueEnd.AddListener(() =>
        {
            ShowDay3Choices();
        });

        DialogueManager.ShowDialogue(dialogue);
    }

    private void ShowDay3Choices()
    {
        // Phase 2: Same last line, now with choice buttons
        Dialogue choiceDialogue = ScriptableObject.CreateInstance<Dialogue>();
        choiceDialogue.text = new List<string>(new string[] 
        { 
            "Scratch your hand?" 
        });
        choiceDialogue.choices = new List<string>(new string[] { "SCRATCH", "LEAVE IT ALONE" });
        choiceDialogue.consequences = new List<UnityEngine.Events.UnityEvent>();
        choiceDialogue.onDialogueEnd = new UnityEngine.Events.UnityEvent();
        choiceDialogue.onDialogueEndImmediate = new UnityEngine.Events.UnityEvent();

        UnityEngine.Events.UnityEvent scratchEvent = new UnityEngine.Events.UnityEvent();
        scratchEvent.AddListener(() =>
        {
            // This choice only comes up once (Day 3 morning); Day 4's scratching goes through HandScratch.
            handlogic hand = FindFirstObjectByType<handlogic>(FindObjectsInactive.Include);
            if (hand != null)
            {
                hand.Scratch();
            }
            MessageBus.Instance.Publish("CompleteTask", "task_wash_hand");
        });

        UnityEngine.Events.UnityEvent leaveEvent = new UnityEngine.Events.UnityEvent();
        leaveEvent.AddListener(() =>
        {
            DialogueManager.ShowDialogueFromText(new string[] { "You endure the intense itchiness." });
            MessageBus.Instance.Publish("PlusSanity", -1);
            MessageBus.Instance.Publish("CompleteTask", "task_wash_hand");
        });

        choiceDialogue.consequences.Add(scratchEvent);
        choiceDialogue.consequences.Add(leaveEvent);

        DialogueManager.ShowDialogue(choiceDialogue);
    }

    private void UpdateTrack(Ambience ambience, AudioClip newClip, float volume, int channel)
    {
        // Check if the new clip is different from the current clip
        AudioClip currentClip = ambience.GetCurrentClip(channel);
        if (currentClip != newClip)
        {
            // Play new clip if it's different
            ambience.PlayTrack(newClip, volume, channel);
        }
        else
        {
            // Update volume if the clip is the same
            ambience.SetVolume(channel, volume);
        }
    }

    private void PlayWakeupSound()
    {
        if (wakeupSound != null)
        {
            MessageBus.Instance.Publish("PlaySound", wakeupSound);
            return;
        }

        string clip = UnityEngine.Random.value < 0.5f ? "wakeup" : "wakeup2";
        MessageBus.Instance.Publish("PlaySound", clip);
    }
}

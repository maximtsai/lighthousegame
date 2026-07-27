using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BedroomMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    void Start()
    {
        if (Ambience.Instance != null) {
            Ambience ambience = Ambience.Instance;
    
            // Update track 1
            UpdateTrack(ambience, bgLoop1, 0.6f, 1);
            // Update track 2
            UpdateTrack(ambience, bgLoop2, 0.21f, 2);
        }
        Debug.Log("bedroom misc logic");

        BeginDay();

        if (GameState.Get<bool>("hand_cut") && !GameState.Get<bool>("hand_cleaned"))
        {
            if (GameState.Get<int>("day") != 3)
            {
                DialogueManager.ShowDialogueFromText(new string[] { "I should bandage my hand." });
            }
        }
    }

    private void BeginDay()
    {
        if (GameState.Get<bool>("day_began"))
            return;

        GameState.Set("day_began", true);

        int day = GameState.Get<int>("day");
        Debug.Log("Begin day " + day);
        switch (day)
        {
            case 1:
                MessageBus.Instance.Publish("AddTaskString", "generic/task_wash_up");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_breakfast");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_lighthouse");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_fish");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_dinner");
                break;
            case 2:
                MessageBus.Instance.Publish("AddTaskString", "generic/task_breakfast");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_work");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_fish");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_dinner");
                MessageBus.Instance.Publish("AddTaskString", "generic/go_to_sleep");
                break;
            case 3:
                MessageBus.Instance.Publish("AddTaskString", "generic/task_wash_hand");
                MessageBus.Instance.Publish("AddTaskString", "generic/task_work");
                ShowDay3Dialogue();
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
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
            "Feels like bugs are crawling underneath it." 
        });
        choiceDialogue.choices = new List<string>(new string[] { "SCRATCH", "LEAVE IT ALONE" });
        choiceDialogue.consequences = new List<UnityEngine.Events.UnityEvent>();
        choiceDialogue.onDialogueEnd = new UnityEngine.Events.UnityEvent();
        choiceDialogue.onDialogueEndImmediate = new UnityEngine.Events.UnityEvent();

        UnityEngine.Events.UnityEvent scratchEvent = new UnityEngine.Events.UnityEvent();
        scratchEvent.AddListener(() =>
        {
            MessageBus.Instance.Publish("FloatText", 0f, 0.3f, "-SANITY", "purple");
            MessageBus.Instance.Publish("PlusSanity", -1);
        });

        UnityEngine.Events.UnityEvent leaveEvent = new UnityEngine.Events.UnityEvent();

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


}

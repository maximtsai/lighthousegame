using UnityEngine;
using System;
using System.Collections.Generic;

public class LighthouseMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    void Start()
    {
        Ambience ambience = Ambience.Instance;

        // Update track 1
        UpdateTrack(ambience, bgLoop1, 0.3f, 1);
        // Update track 2
        UpdateTrack(ambience, bgLoop2, 0.17f, 2);

        // Day 3: if task is "Go back upstairs" and scissors disappeared, show scissors dialogue
        if (GameState.Get<int>("day") == 3 && IsTaskActive("task_finish_maintenance") && GameState.Get<bool>("ScissorsDisappeared", false))
        {
            GameState.Set("scissorsDrop", false);
            GameState.Set("scissors_found", true);
            GameState.Set("lighthouseFixForgotten", true);
            ShowScissorsDialogue();
        }
    }

    private bool IsTaskActive(string taskId)
    {
        if (TaskManager.instance == null) return false;
        var tasks = TaskManager.instance.GetCurrentTasks();
        foreach (var task in tasks)
        {
            if (task.id == taskId)
                return true;
        }
        return false;
    }

    private void ShowScissorsDialogue()
    {
        Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
        dialogue.text = new List<string>(new string[]
        {
            "You find the scissors besides the stairs.",
            "Did you drop the scissors or not?"
        });
        dialogue.choices = new List<string>();
        dialogue.consequences = new List<UnityEngine.Events.UnityEvent>();
        dialogue.onDialogueEnd = new UnityEngine.Events.UnityEvent();
        dialogue.onDialogueEndImmediate = new UnityEngine.Events.UnityEvent();

        dialogue.onDialogueEnd.AddListener(() =>
        {
            MessageBus.Instance.Publish("ShowChoiceDialog", "Did you drop the scissors or not?");
            MessageBus.Instance.Publish(
                "ShowTwoChoice",
                "I swear I did",
                "I don't know",
                (Action)(() =>
                {
                    MessageBus.Instance.Publish("PlusSanity", 1);
                    MessageBus.Instance.Publish("CompleteTask", "task_finish_maintenance");
                    MessageBus.Instance.Publish("AddTaskBefore", "generic/task_lighthouse", "task_fish");
                }),
                (Action)(() =>
                {
                    MessageBus.Instance.Publish("PlusSanity", -1);
                    MessageBus.Instance.Publish("CompleteTask", "task_finish_maintenance");
                    MessageBus.Instance.Publish("AddTaskBefore", "generic/task_lighthouse", "task_fish");
                })
            );
        });

        DialogueManager.ShowDialogue(dialogue);
    }

    private void UpdateTrack(Ambience ambience, AudioClip newClip, float volume, int channel)
    {
        // Null when Play Mode is entered straight from this scene instead of MainScene.
        if (ambience == null)
            return;

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

    public void UpdateTrackPublic(AudioClip newClip, float volume, int channel)
    {
        Ambience ambience = Ambience.Instance;
        if (ambience == null)
            return;

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

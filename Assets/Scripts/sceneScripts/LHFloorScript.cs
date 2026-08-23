using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LHFloorScript : MonoBehaviour
{

    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    [SerializeField] private AudioClip finishLoop;
    [SerializeField] private MiscObjectClick miscObjectClick;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Ambience ambience = Ambience.Instance;

        // Update track 1
        UpdateTrack(ambience, bgLoop1, 0.5f, 1);
        // Update track 2
        UpdateTrack(ambience, bgLoop2, 0.23f, 2);
        if (GameState.Get<bool>("lighthouse_fixed"))
        {
            StartCoroutine(PlaySoundDelayedRoutine(finishLoop, 0.2f, true, 0.01f));
        }

        // Day 3: if scissors dropped, show search dialogue
        if (GameState.Get<int>("day") == 3 && GameState.Get<bool>("scissorsDrop", false))
        {
            Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
            dialogue.text = new List<string>(new string[] 
            { 
                "You look around for the dropped scissors.", 
                "It's dim and you grope damp stones in the dark.", 
                "You don't find anything." 
            });
            dialogue.choices = new List<string>();
            dialogue.consequences = new List<UnityEngine.Events.UnityEvent>();
            dialogue.onDialogueEnd = new UnityEngine.Events.UnityEvent();
            dialogue.onDialogueEndImmediate = new UnityEngine.Events.UnityEvent();
            dialogue.onDialogueEnd.AddListener(() =>
            {
                GameState.Set("ScissorsDisappeared", true);
                MessageBus.Instance.Publish("CompleteTask", "task_find_scissors");
            });
            DialogueManager.ShowDialogue(dialogue);
        }
    }
    
    private IEnumerator PlaySoundDelayedRoutine(AudioClip sfx, float volume, bool loop, float delay)
    {
        yield return new WaitForSeconds(delay);
        miscObjectClick.PlaySound(sfx, volume, loop);
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
    
    
}

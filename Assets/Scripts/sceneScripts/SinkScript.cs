using UnityEngine;
using System;

public class SinkScript : MonoBehaviour
{
    public GameObject cleaningItems;
    [SerializeField] private AudioClip cleanSound;
    [SerializeField] private MiscObjectClick miscObjectClick;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameState.Get<string>("is_clean") == "true") {
            Destroy(cleaningItems);
        }
    }

    public void CleanSink()
    {
        int day = GameState.Get<int>("day");

        if (day == 2)
        {
            if (GameState.Get<bool>("hand_cut") && !GameState.Get<bool>("hand_cleaned"))
            {
                CleanAndBandageHand();
                return;
            }
        }

        if (day == 1 && GameState.Get<string>("is_clean") != "true") 
        {
            Destroy(cleaningItems);
            // miscObjectClick.PlaySound(cleanSound);
            MessageBus.Instance.Publish("CompleteTask", "task_wash_up");
            MessageBus.Instance.Publish("FloatText", 0, -1.1f, "+SANITY", "green");
            MessageBus.Instance.Publish("PlusSanity", 1);

            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("sink/sink_wash_up"));
            GameState.Set("is_clean", "true");
            return;
        }

        DialogueManager.ShowDialogueFromText(new string[] { "I'm clean enough." });
    }

    private void CleanAndBandageHand()
    {
        string[] cleanDialog = new string[] 
        { 
            "I rinse my hand under the grimy tap.",
            "No medical supplies here. I tear a strip off a spare rag.",
            "I wrap it tight.##.##. ##It'll do for now."
        };
        
        Dialogue d = DialogueManager.ShowDialogueFromText(cleanDialog);
        d.onDialogueEnd.AddListener(() =>
        {
            MessageBus.Instance.Publish("ShowChoiceDialog", "Are you going to get an infection and die?");
            
            MessageBus.Instance.Publish(
                "ShowTwoChoice",
                "YES",
                "NO",
                (Action)(() =>
                {
                    MessageBus.Instance.Publish("FloatText", 0f, 0.3f, "-SANITY", "purple");
                    MessageBus.Instance.Publish("PlusSanity", -2);
                    CompleteHandClean();
                }),
                (Action)(() =>
                {
                    MessageBus.Instance.Publish("FloatText", 0f, 0.3f, "+SANITY", "green");
                    MessageBus.Instance.Publish("PlusSanity", 1);
                    CompleteHandClean();
                })
            );
        });
    }

    private void CompleteHandClean()
    {
        GameState.Set("hand_cleaned", true);
        MessageBus.Instance.Publish("RemoveTaskString", "generic/task_wash_hand");

        if (GameState.Get<int>("day") == 2 && GameState.Get<bool>("lighthouse_fixed", false))
        {
            GameState.Set("ready_to_sleep", true);
        }
    }
}

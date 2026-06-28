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
            "I turn on the tap and hold my bleeding palm under the freezing water. The sting is sharp and sudden, making me gasp.",
            "I search around for something to wrap it with. There are no proper medical supplies here.",
            "I tear a strip of cloth from a clean rag and wrap it clumsily around the cut, tying it off with a tight knot.",
            "It looks amateurish, but the pressure seems to have stopped the bleeding for now."
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
        MessageBus.Instance.Publish("CompleteTask", "task_wash_hand");
    }
}

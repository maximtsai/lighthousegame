using UnityEngine;

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
        if (GameState.Get<string>("is_clean") == "true") {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("sink/already_washed"));
        } else 
        {
            Destroy(cleaningItems);
            // miscObjectClick.PlaySound(cleanSound);
            MessageBus.Instance.Publish("CompleteTask", "task_wash_up");
            MessageBus.Instance.Publish("FloatText", 0, -1.1f, "+SANITY", "green");

            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("sink/sink_wash_up"));
            
        }
    }
}

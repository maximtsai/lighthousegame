using UnityEngine;

public class JournalMisc : MonoBehaviour
{
    [SerializeField] private MiscObjectClick miscObjectClick;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameState.Get<int>("day") == 1 && !GameState.Get<bool>("introduced_journal", false))
        {
            GameState.Set("introduced_journal", true);
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("Bedroom/journal_not_yet_day1"));
        }
        else
        {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("Bedroom/journal_not_yet"));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

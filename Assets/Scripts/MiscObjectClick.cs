using UnityEngine;

public class MiscObjectClick : MonoBehaviour
{
    private string path = "ScriptableObjects/Dialogues/";

    private Dialogue getDialogue(string name)
    {
        string fullPath = path + name;
        Dialogue dialogue = Resources.Load<Dialogue>(fullPath);
        if (dialogue == null)
        {
            Debug.LogWarning("Dialogue not found: " + path);
        }
        return dialogue;
    }
    public void ClickPartnerBed()
    {
        if (GameState.Get<int>("day") == 1)
        {
            if (GameState.Get<bool>("choresdone"))
            {
                DialogueManager.ShowDialogue(getDialogue("co_bed_2"));
            }
            else
            {
                DialogueManager.ShowDialogue(getDialogue("co_bed_1"));
            }
        }
        else
        {
            DialogueManager.ShowDialogue(getDialogue("co_bed_3"));
        }
    }
}

using System.Data.Common;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public GameObject prefab_dialoguebox;
    public static DialogueManager instance;

    private static bool dialogue_is_open = false;

    private static GameObject activeDialogueBox = null;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public static void ShowDialogue(Dialogue d)
    {
        if (activeDialogueBox != null) {
            CloseDialogue();
        }
        activeDialogueBox = Instantiate(instance.prefab_dialoguebox);
        activeDialogueBox.GetComponent<UIDialogueBox>().SetDialogue(d);
        dialogue_is_open = true;
    }
    public static void CloseDialogue()
    {
        if (activeDialogueBox != null)
        {
            Destroy(activeDialogueBox);
            activeDialogueBox = null;
        }
        dialogue_is_open = false;
    }
    public static bool DialogueIsOpen()
    {
        return dialogue_is_open;
    }
}

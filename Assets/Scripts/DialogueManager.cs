using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public GameObject prefab_dialoguebox;
    public static DialogueManager instance;
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
        SubscribeToMessages();
    }

    private void SubscribeToMessages()
    {
        MessageBus.Instance.Subscribe("ShowDialogInstant", ShowDialogueInstant, this);
        MessageBus.Instance.Subscribe("CloseDialogue", CloseDialogue, this);
    }

    public static void ShowDialogue(Dialogue d)
    {
        if (activeDialogueBox != null)
        {
            CloseDialogue();
        }
        activeDialogueBox = Instantiate(instance.prefab_dialoguebox);
        activeDialogueBox.GetComponent<UIDialogueBox>().SetDialogue(d);

        GameState.Set("dialogue_is_open", true);
    }

    public static void ShowDialogueInstant(object[] args)
    {
        string text = args[0] as string;
        if (activeDialogueBox != null)
        {
            CloseDialogue();
        }
        activeDialogueBox = Instantiate(instance.prefab_dialoguebox);
        activeDialogueBox.GetComponent<UIDialogueBox>().SetDialogueInstant(text);

        GameState.Set("dialogue_is_open", true);
    }

    private static void CloseDialogue(object[] args)
    {
        CloseDialogue();
    }

    public static void CloseDialogue()
    {
        if (activeDialogueBox != null)
        {
            Destroy(activeDialogueBox);
            activeDialogueBox = null;
        }
        GameState.Set("dialogue_is_open", false);
        Debug.Log("dialogue closed");
    }
    public static bool DialogueIsOpen()
    {
        return GameState.Get<bool>("dialogue_is_open", false);
    }
}

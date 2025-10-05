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
    }
    public static void ShowDialogue(Dialogue d)
    {
        Debug.Log("showing dialogue");
        if (activeDialogueBox != null)
        {
            CloseDialogue();
        }
        activeDialogueBox = Instantiate(instance.prefab_dialoguebox);
        activeDialogueBox.GetComponent<UIDialogueBox>().SetDialogue(d);

        GameState.Set("dialogue_is_open", true);
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

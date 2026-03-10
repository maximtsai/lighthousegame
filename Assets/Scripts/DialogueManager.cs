using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public GameObject prefab_dialoguebox;
    public static DialogueManager instance;
    private static GameObject activeDialogueBox = null;

    private MessageBus.SubscriptionHandle handle;
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
        WarmUpFonts();
    }

    private void WarmUpFonts()
    {
        // Warming up fonts on the main thread to avoid "GetName can only be called from the main thread" 
        // in TextMeshPro/TextCore/UITK background jobs.
        var fonts = Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>();
        foreach (var font in fonts)
        {
            if (font == null) continue;
            var _ = font.name;
            var __ = font.faceInfo.familyName;
            font.HasCharacter(' ');
        }
    }


    private void SubscribeToMessages()
    {
        handle = MessageBus.Instance.Subscribe("CloseDialogue", CloseDialogue, this);
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

    public static void ShowDialogueFromText(string[] textArray)
    {
        Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
        dialogue.text = new List<string>(textArray);
        ShowDialogue(dialogue);
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

    void OnDestroy()
    {
        handle?.Unsubscribe();
    }
}

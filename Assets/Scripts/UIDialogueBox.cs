using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDialogueBox : MonoBehaviour, IPointerClickHandler
{
    // public static Texture2D pointerCursor;
    
    private Dialogue dialogue;
    private int current_line = -1;
    private int total_lines = -1;
    [SerializeField] private TMP_Text textmesh;
    [SerializeField] private Texture2D pointerCursor;

    [SerializeField] private float typeSpeed = 0.016f; // seconds per character
    private Coroutine typingCoroutine;
    private IEnumerator TypeText(string line)
    {
        // Types in text like a typewriter
        textmesh.text = "";
        int index = 0;
        while (index < line.Length)
        {
            // Take the next 1 - 3 characters
            int charsToAdd = Mathf.Min(3, line.Length - index);
            textmesh.text += line.Substring(index, charsToAdd);
            index += charsToAdd;

            yield return new WaitForSeconds(typeSpeed);
        }
        typingCoroutine = null;
    }
    
    public void OnDestroy()
    {
        DialogueManager.CloseDialogue();
    }
    public void SetDialogue(Dialogue d)
    {
        if (1 > d.text.Count) return;

        current_line = 0;
        dialogue = d;
        total_lines = dialogue.text.Count;
        
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(dialogue.text[current_line]));
        
        
        Cursor.SetCursor(pointerCursor, Vector2.zero, CursorMode.Auto);
    }
    public void OnPointerClick(PointerEventData e)
    {
        // Now handled by DialogClickable
        // AdvanceDialogue();
    }

    public void AdvanceDialogue()
    {
        if (typingCoroutine != null)
        {
            // Finish typing instantly
            StopCoroutine(typingCoroutine);
            textmesh.text = dialogue.text[current_line];
            typingCoroutine = null;
            return;
        }
        
        current_line++;
        if (total_lines <= current_line)
        {
            dialogue.onDialogueEnd?.Invoke();
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // revert cursor to default
            Destroy(gameObject);
        }
        else
        {
            typingCoroutine = StartCoroutine(TypeText(dialogue.text[current_line]));
        }
    }
}

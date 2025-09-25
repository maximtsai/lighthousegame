using TMPro;
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
        textmesh.text = dialogue.text[current_line];
        Cursor.SetCursor(pointerCursor, Vector2.zero, CursorMode.Auto);
    }
    public void OnPointerClick(PointerEventData e)
    {
        current_line++;
        if (total_lines <= current_line)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // revert to default
            Destroy(gameObject);
        }
        else
        {
            textmesh.text = dialogue.text[current_line];
        }
    }
}

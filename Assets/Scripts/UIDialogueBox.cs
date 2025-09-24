using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDialogueBox : MonoBehaviour, IPointerClickHandler
{
    private Dialogue dialogue;
    private int current_line = -1;
    private int total_lines = -1;
    [SerializeField] private TMP_Text textmesh;
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
    }
    public void OnPointerClick(PointerEventData e)
    {
        current_line++;
        if (total_lines <= current_line)
        {
            Destroy(gameObject);
        }
        else
        {
            textmesh.text = dialogue.text[current_line];
        }
    }
}

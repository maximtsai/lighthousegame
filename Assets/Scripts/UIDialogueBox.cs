using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class UIDialogueBox : MonoBehaviour
{
    private Dialogue dialogue;
    private int current_line = -1;
    private int total_lines = -1;
    [SerializeField] private TMP_Text textmesh;

    [SerializeField] private float typeSpeed = 0.016f; // seconds per character
    [SerializeField] private Button button0;
    [SerializeField] private Button button1;
    private Coroutine typingCoroutine;
    public void Start()
    {
        button0.gameObject.SetActive(false);
        button1.gameObject.SetActive(false);

        if (dialogue.startSound != null)
        {
            if (AudioManager.Instance)
            {
                AudioSource audioSource = AudioManager.Instance.AudioSource;
                audioSource.clip = dialogue.startSound;
                audioSource.PlayOneShot(dialogue.startSound);
            }
            else
            {
                Debug.LogWarning("No AudioSource found in the scene!");
            }
        }
    }
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
        
        // Show dialog choices if we have any
        if (dialogue.choices.Count == 2 && dialogue.choices.Count == dialogue.consequences.Count)
        {
            ShowDialogChoices();
        }
    }
    
    public void OnDestroy()
    {
        GameState.Set("picking_choice", false);
        GameState.Set("dialogue_is_open", false);
    }
    public void SetDialogue(Dialogue d)
    {
        if (1 > d.text.Count) return;

        current_line = 0;
        dialogue = d;

        total_lines = dialogue.text.Count;
        CustomCursor.SetCursorToDialog();

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(dialogue.text[current_line]));

    }
    public void EndDialogue(int choice = -1)
    {
        Debug.Log(choice);
        if (-1 < choice && choice < dialogue.consequences.Count)
        {
            dialogue.consequences[choice]?.Invoke();
        }
        else
        {
            dialogue.onDialogueEnd?.Invoke();
        }
        CustomCursor.SetCursorToNormal(); // revert cursor to default
        Destroy(gameObject);
    }

    public void AdvanceDialogue()
    {
        if (typingCoroutine != null)
        {
            // Finish typing instantly
            StopCoroutine(typingCoroutine);
            textmesh.text = dialogue.text[current_line];
            typingCoroutine = null;

            if (dialogue.choices.Count == 2 && dialogue.choices.Count == dialogue.consequences.Count)
            {
                ShowDialogChoices();
            }
            
            return;
        }

        current_line++;
        if (total_lines <= current_line)
        {
            if (null == dialogue.choices || 2 > dialogue.choices.Count)
            {
                EndDialogue();
            }
            else if (2 == dialogue.choices.Count && dialogue.choices.Count == dialogue.consequences.Count)
            {
                // Moving this to when the typewriter dialog finishes typing
                // button0.gameObject.SetActive(true);
                // button0.GetComponentInChildren<TMP_Text>().text = dialogue.choices[0];
                // button1.gameObject.SetActive(true);
                // button1.GetComponentInChildren<TMP_Text>().text = dialogue.choices[1];
                // CustomCursor.SetCursorToNormal(); // revert cursor to default
            }
            else
            {
                Debug.Log("malformed dialogue prompt: number of choices and consequences are mismatching and/or not 2 total");
                EndDialogue();
            }
        }
        else
        {
            typingCoroutine = StartCoroutine(TypeText(dialogue.text[current_line]));
        }
    }

    private void ShowDialogChoices()
    {
        GameState.Set("picking_choice", true);
        button0.gameObject.SetActive(true);
        button0.GetComponentInChildren<TMP_Text>().text = dialogue.choices[0];
        button1.gameObject.SetActive(true);
        button1.GetComponentInChildren<TMP_Text>().text = dialogue.choices[1];
        CustomCursor.SetCursorToNormal(); // revert cursor to default
    }
}

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
    [SerializeField] private AudioClip typeSound1;
    [SerializeField] private AudioClip typeSound2;
    [SerializeField] private AudioClip typeSound3;

    [SerializeField] private float typeSpeed = 0.016f; // seconds per character
    [SerializeField] private Button button0;
    [SerializeField] private Button button1;
    private AudioSource audioSrc;
    private Coroutine typingCoroutine;

    public void Start()
    {
        button0.gameObject.SetActive(false);
        button1.gameObject.SetActive(false);
        if (!typeSound1) {
            Debug.Log("Missing type sound");
        }
        if (AudioManager.Instance)
        {
            audioSrc = AudioManager.Instance.AudioSource;
        }
        else
        {
            Debug.LogWarning("No AudioSource found in the scene!");
        }

        if (dialogue != null && dialogue.startSound != null)
        {
            if (audioSrc)
            {
                audioSrc.clip = dialogue.startSound;
                audioSrc.PlayOneShot(dialogue.startSound);
            }
        }
    }
    private IEnumerator TypeText(string line, bool isFinalLine)
    {
        textmesh.text = "";
        int index = 0;

        while (index < line.Length)
        {
            // Handle control character #
            if (line[index] == '#')
            {
                index++; // skip the symbol, do not print
                yield return new WaitForSeconds(0.25f);
                continue;
            }

            // Take the next 1 - 3 characters (but stop if we hit #)
            int charsToAdd = 0;
            for (int i = 0; i < 3 && index + i < line.Length; i++)
            {
                if (line[index + i] == '#')
                    break;

                charsToAdd++;
            }

            // Append characters
            textmesh.text += line.Substring(index, charsToAdd);
            index += charsToAdd;

            // Play typing sound
            if (audioSrc && typeSound1)
            {
                int n = Random.Range(0, 4);
                switch (n)
                {
                    case 0:
                        audioSrc.PlayOneShot(typeSound1);
                        break;
                    case 1:
                    case 2:
                        audioSrc.PlayOneShot(typeSound2);
                        break;
                    case 3:
                        audioSrc.PlayOneShot(typeSound3);
                        break;
                }
            }

            yield return new WaitForSeconds(typeSpeed);
        }

        typingCoroutine = null;

        if (isFinalLine)
        {
            bool hasImmediateEnd = dialogue.onDialogueEndImmediate.GetPersistentEventCount() > 0;
            dialogue.onDialogueEndImmediate?.Invoke();

            if (hasImmediateEnd)
            {
                yield return new WaitForSeconds(0.2f);
                CustomCursor.SetCursorToNormal();
                Destroy(gameObject);
                yield break;
            }
        }

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
        bool isFinalLine = current_line == total_lines - 1;
        CustomCursor.SetCursorToDialog();

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(dialogue.text[current_line], isFinalLine));
    }

    public void SetDialogueInstant(string text)
    {
        if (1 > text.Length) return;

        current_line = 0;
        CustomCursor.SetCursorToDialog();

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = null;
        textmesh.text = text;

    }
    public void EndDialogue(int choice = -1)
    {
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
        if (dialogue == null)
        {
            return;
        }
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
            bool isFinalLine = current_line == total_lines - 1;
            typingCoroutine = StartCoroutine(TypeText(dialogue.text[current_line], isFinalLine));
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

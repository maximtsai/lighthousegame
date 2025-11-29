using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UITaskTracker : MonoBehaviour
{
    [SerializeField] TMP_Text text_tasklist;
    [SerializeField] Image tasklist_bg;
    [SerializeField] Canvas canvas_full_list;
    [SerializeField] AudioClip whoosh_in;
    [SerializeField] AudioClip typewriter;

    private string textToDisplay = "LALA hello world";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AnimateInNewTask();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowTaskList()
    {
        canvas_full_list.gameObject.SetActive(true);
        GameState.Set("task_list_open", true);
    }

    public void HideTaskList()
    {
        canvas_full_list.gameObject.SetActive(false);
        GameState.Set("task_list_open", false);
    }

    private void SetTextDisplay(string text)
    {
        textToDisplay = text;
    }

    public void AnimateInNewTask()
    {
        tasklist_bg.gameObject.SetActive(true);
        // Make tasklist animate in here
        text_tasklist.text = "";
        StartCoroutine(AnimateRevealThenType());
    }

    private IEnumerator AnimateRevealThenType()
    {
        // --- First: Reveal Animation ---
        yield return StartCoroutine(AnimateReveal());

        // --- Then: Typewriter Animation ---
        yield return StartCoroutine(Typewriter());
    }


    private IEnumerator AnimateReveal()
    {
        RectTransform bg = tasklist_bg.rectTransform;
        RectTransform mask = tasklist_bg.transform.parent.GetComponent<RectTransform>();

        playSoundClip(whoosh_in);

        float duration = 0.6f;
        float t = 0f;

        float targetWidth = bg.sizeDelta.x;
        mask.sizeDelta = new Vector2(0, bg.sizeDelta.y);

        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duration);

            // Ease-out smoothing
            float eased = Mathf.SmoothStep(0f, 1f, normalized);

            float w = Mathf.Lerp(0f, targetWidth, eased);
            mask.sizeDelta = new Vector2(w, bg.sizeDelta.y);

            yield return null;
        }

        // Ensure perfect final state
        mask.sizeDelta = new Vector2(targetWidth, bg.sizeDelta.y);
    }


    private IEnumerator Typewriter()
    {
        float delay = 0.03f; // time between letters (adjust to taste)
        text_tasklist.text = "";
        yield return new WaitForSeconds(0.06f);

        for (int i = 0; i <= textToDisplay.Length; i++)
        {
            text_tasklist.text = textToDisplay.Substring(0, i);
            playSoundClip(typewriter);
            yield return new WaitForSeconds(delay);
        }
    }


    private void playSoundClip(AudioClip clip)
    {
        if (clip)
        {
            // Play sound if it's available
            if (AudioManager.Instance)
            {
                AudioSource audioSource = AudioManager.Instance.AudioSource;
                audioSource.clip = clip;
                audioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning("No AudioSource found in the scene!");
            }
        }
    }

}

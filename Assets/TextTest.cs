using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshPro textToShow;
    public float displayTime = 0f;
    public float fadeDuration = 0.3f;
    Coroutine currentCoroutine;

    void Start()
    {
        Debug.Log("text setup");
        //textToShow.SetActive(false);
        Color c = textToShow.color;
        c.a = 0;
        textToShow.color = c;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnMouseDown()
    {
        Debug.Log("text appear");

        // Stop any previous fading coroutine
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        //textToShow.SetActive(true);
        Color c = textToShow.color;
        c.a = 1f;
        textToShow.color = c;

        //optional
        RectTransform rect = textToShow.GetComponent<RectTransform>();
        rect.anchoredPosition = rect.anchoredPosition; // optional, already default, but ensures fresh start
        
        currentCoroutine = StartCoroutine(FadeOutText());
    }

    System.Collections.IEnumerator FadeOutText()
    {
        //wait for display time;
        //yield return new WaitForSeconds(displayTime);

        float elapsed = 0f;
        Color c = textToShow.color;
        RectTransform rect = textToShow.GetComponent<RectTransform>();
        Vector2 startPos = rect.anchoredPosition; // store original position
        float floatDistance = 1f; // how far up it floats
        float slow = 1f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            //fade out
            c.a = Mathf.Lerp(1f, 0f, t);
            textToShow.color = c;


            //float upward
            rect.anchoredPosition = startPos + Vector2.up * floatDistance * t * slow;

            //modify slow speed
            slow = 1-t/2;

            yield return null;
        }

        // Fade out finished, reset to fully transparent and original position
        c.a = 0f;
        textToShow.color = c;
        rect.anchoredPosition = startPos;
    }
}

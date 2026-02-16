using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayHandler : MonoBehaviour
{
    public GameObject ClickBlocker;
    public GameObject Blackout;
    public TextMeshProUGUI dayText;
    public AudioClip dayTransitionSound;

    void Start()
    {
        int currentDay = GameState.Get<int>("day");
        if (currentDay > 1)
        {
            dayText.text = "DAY " + currentDay;
            
            Image blackoutImg = Blackout.GetComponent<Image>();
            blackoutImg.color = new Color(0f, 0f, 0f, 1f);
            dayText.color = new Color(1f, 1f, 1f, 1f);
            
            if (dayTransitionSound != null && AudioManager.Instance)
            {
                AudioManager.Instance.AudioSource.PlayOneShot(dayTransitionSound);
            }
            
            ClickBlocker.SetActive(true);
            Blackout.SetActive(true);
            
            StartCoroutine(DaySequence());
        }
    }

    private System.Collections.IEnumerator DaySequence()
    {
        yield return new WaitForSeconds(1.6f);
        
        float duration = 0.4f;
        float elapsed = 0f;
        
        Image blackoutImg = Blackout.GetComponent<Image>();
        Color startColor = blackoutImg.color;
        Color targetColor = startColor;
        targetColor.a = 0f;
        
        Color textStartColor = dayText.color;
        Color textTargetColor = textStartColor;
        textTargetColor.a = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            blackoutImg.color = Color.Lerp(startColor, targetColor, t);
            dayText.color = Color.Lerp(textStartColor, textTargetColor, t);
            
            yield return null;
        }
        
        blackoutImg.color = targetColor;
        dayText.color = textTargetColor;
        
        Blackout.SetActive(false);
        ClickBlocker.SetActive(false);
    }
}

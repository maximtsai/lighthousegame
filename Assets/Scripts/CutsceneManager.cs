using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private Image displayImage;   // The main image showing screenshots
    [SerializeField] private Image fadeOverlay;    // A black Image that covers the screen for fading
    [SerializeField] private GameObject clickAgainImage;   // A prompt asking you if you want to skip
    [SerializeField] private GameObject clickBlocker;    // A full-screen button that detects clicks
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private float fadeDuration = 0.65f; // seconds to fade in/out
    private const float targetWidth = 773f;

    private bool isPlaying = false;
    private string path = "ScriptableObjects/Cutscenes/";
    private const float referenceAspect = 16f / 9f; // 1.777...

    // --- Added for skip handling ---
    private bool waitingForSecondClick = false;
    private float doubleClickTimeout = 2f;
    private Coroutine resetClickRoutine;
    private System.Action cutsceneOnComplete;
    private bool skipFirstClickBlockerClick = true;

    void Start() { }

    private void SetImageSize(Sprite sprite)
    {
        if (sprite == null) return;
    
        RectTransform rt = displayImage.rectTransform;
    
        float aspect = sprite.rect.width / sprite.rect.height;
        float targetHeight = targetWidth / aspect;
    
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
    }

    public Cutscene getCutsceneScriptable(string name)
    {
        string fullPath = path + name;
        Cutscene cutscene = Resources.Load<Cutscene>(fullPath);
        if (cutscene == null)
        {
            Debug.LogWarning("Cutscene not found: " + fullPath);
        }
        return cutscene;
    }

    public void PlayCutscene(string cutsceneName, bool skipFadeout = false, System.Action onComplete = null)
    {
        if (isPlaying) return;
        Cutscene cutscene = getCutsceneScriptable(cutsceneName);

        if (cutscene == null)
        {
            Debug.LogError($"Cutscene '{cutsceneName}' not found! Make sure it’s in a Resources folder.");
            return;
        }

        cutsceneOnComplete = onComplete; // store for SkipCutscene()
        StartCoroutine(PlayCutsceneRoutine(cutscene, skipFadeout, onComplete));
    }

    private IEnumerator PlayCutsceneRoutine(Cutscene cutscene, bool skipFadeout, System.Action onComplete)
    {
        isPlaying = true;
        skipFirstClickBlockerClick = true;
        clickBlocker.SetActive(true);
        clickAgainImage.SetActive(false);

        if (cutscene.backgroundMusic)
        {
            audioSource.clip = cutscene.backgroundMusic;
            audioSource.Play();
        }

        // Start fully transparent
        Color overlayColor = fadeOverlay.color;
        overlayColor.a = 0f;
        Color fullColor = displayImage.color;
        Color emptyColor = displayImage.color;
        fullColor.a = 1f;
        emptyColor.a = 0f;
        displayImage.color = emptyColor;

        for (int i = 0; i < cutscene.images.Count; i++)
        {
            // --- Fade to black ---
            yield return StartCoroutine(FadeOverlay(1f));

            // --- Change image ---
            displayImage.sprite = cutscene.images[i];
            fullColor.a = 1f;
            displayImage.color = fullColor;
            SetImageSize(displayImage.sprite);
            SetImageStartPosition(displayImage.sprite);

            // --- Hold for the image duration ---
            float duration = (i < cutscene.durations.Count) ? cutscene.durations[i] : 4f;
            yield return StartCoroutine(ShowAndPanImage(displayImage, duration));
        }

        // Fade to black at end
        yield return StartCoroutine(FadeOverlay(1f));

        // Clear image & stop music
        displayImage.sprite = null;
        displayImage.color = emptyColor;
        audioSource.Stop();

        // Fade back to transparent for next scene
        if (!skipFadeout) {
            yield return StartCoroutine(FadeOverlay(0f));
        }

        isPlaying = false;
        skipFirstClickBlockerClick = false;
        clickBlocker.SetActive(false);
        clickAgainImage.SetActive(false);
        onComplete?.Invoke();
    }

    // -----------------------------
    // SKIP HANDLING (added section)
    // -----------------------------
    public void OnClickBlocker()
    {
        if (skipFirstClickBlockerClick) {
            skipFirstClickBlockerClick = false;
            return;
        }
        Debug.Log("Click blocker clickesdfsdfsdfd");
        if (!isPlaying) return;

        if (!waitingForSecondClick)
        {
            waitingForSecondClick = true;
            clickAgainImage.SetActive(true);

            if (resetClickRoutine != null)
                StopCoroutine(resetClickRoutine);

            resetClickRoutine = StartCoroutine(ResetSkipPrompt());
        }
        else
        {
            // Skip NOW
            SkipCutscene();
        }
    }

    private IEnumerator ResetSkipPrompt()
    {
        yield return new WaitForSeconds(doubleClickTimeout);

        waitingForSecondClick = false;
        clickAgainImage.SetActive(false);
    }

    private void SkipCutscene()
    {
        if (!isPlaying) return;
    
        StopAllCoroutines();
        StartCoroutine(SkipCutsceneRoutine());
    }

    private IEnumerator SkipCutsceneRoutine()
    {
        // 1. Fade to black
        yield return StartCoroutine(FadeOverlay(1f, 0.25f));

        // Reset UI
        displayImage.sprite = null;
        displayImage.color = new Color(1,1,1,0);
        clickBlocker.SetActive(false);
        clickAgainImage.SetActive(false);

        // Stop audio
        audioSource.Stop();

        isPlaying = false;
        skipFirstClickBlockerClick = false;

        // Call the final callback if provided
        cutsceneOnComplete?.Invoke();
    }

    private IEnumerator FadeOverlay(float targetAlpha, float usedDuration = 0f)
    {
        Color color = fadeOverlay.color;
        float startAlpha = color.a;
        float elapsed = 0f;

        float actualDuration = fadeDuration;
        if (usedDuration > 0f) {
            actualDuration = usedDuration;
        }

        while (elapsed < actualDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / actualDuration);
            fadeOverlay.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        fadeOverlay.color = color;
    }

    private IEnumerator ShowAndPanImage(Image img, float duration)
    {
        RectTransform rt = img.rectTransform;
        Sprite sprite = img.sprite;
        if (sprite == null) yield break;
    
        float spriteAspect = sprite.rect.width / sprite.rect.height;
        float targetHeight = targetWidth / spriteAspect;
        float refAspect = 16f / 9f;
        float spriteHeightRatio = sprite.rect.height / sprite.rect.width;
    
        rt.anchoredPosition = Vector2.zero;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
    
        if (spriteHeightRatio > (9f / 16f))
        {
            float extraHeight = targetHeight - (targetWidth / refAspect);
            Vector2 startPos = new Vector2(0f, extraHeight / 2f);
            Vector2 endPos = new Vector2(0f, -extraHeight / 2f);
    
            float slowDownStart = 0.9f;
            float slowDownFactor = 0.1f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (t > slowDownStart)
                {
                    float normalized = (t - slowDownStart) / (1f - slowDownStart);
                    float speedMult = Mathf.Lerp(1f, slowDownFactor, normalized);
                    t = slowDownStart + 0.4f * (t - slowDownStart);
                }
    
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t / 0.94f);

                float halfFadeDur = fadeDuration * 0.5f;
                if (elapsed < halfFadeDur)
                {
                    float fadeT = (halfFadeDur - elapsed) / halfFadeDur;
                    Color color = fadeOverlay.color;
                    color.a = Mathf.Lerp(0f, 1f, fadeT);
                    fadeOverlay.color = color;
                } 
                else 
                {
                    Color color = fadeOverlay.color;
                    color.a = 0f;
                    fadeOverlay.color = color;
                }

                yield return null;
            }
            yield return new WaitForSeconds(1f);
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }
    }

    private void SetImageStartPosition(Sprite sprite)
    {
        RectTransform rt = displayImage.rectTransform;
        float aspect = sprite.rect.width / sprite.rect.height;

        rt.anchoredPosition = Vector2.zero;

        if (aspect < referenceAspect)
        {
            float targetHeight = targetWidth / aspect;
            float visibleHeight = targetWidth / referenceAspect;
            float offset = (targetHeight - visibleHeight) / 2f;
            rt.anchoredPosition = new Vector2(0f, offset);
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private GameObject clickAgainImage;
    [SerializeField] private GameObject clickBlocker;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject animatorObj;   // used for animation cutscenes

    private Animator animator;   // used for animation cutscenes

    [SerializeField] private float fadeDuration = 0.65f;

    private bool isPlaying = false;
    private string path = "ScriptableObjects/Cutscenes/";

    // Skip system
    private bool waitingForSecondClick = false;
    private float doubleClickTimeout = 2f;
    private Coroutine resetClickRoutine;
    private System.Action cutsceneOnComplete;
    private bool skipFirstClickBlockerClick = true;

    void Start() {
        if (animatorObj) {
            animator = animatorObj.GetComponent<Animator>();     
            ResizeAnimatorObj();
        }
    }

    void ResizeAnimatorObj() {
        // Adjust height to maintain aspect ratio
        RectTransform rt = animatorObj.GetComponent<RectTransform>();
        if (rt != null) {
            // Assuming the animatorObj has an Image component for original sprite
            Image img = animatorObj.GetComponent<Image>();
            if (img != null && img.sprite != null) {
                float originalWidth = img.sprite.rect.width;
                float originalHeight = img.sprite.rect.height;

                float currentWidth = rt.rect.width;
                float newHeight = currentWidth * (originalHeight / originalWidth);

                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
            }
        }

    }

    // ---------------------------------------------------------
    // Load ScriptableObject by name
    // ---------------------------------------------------------
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

    // ---------------------------------------------------------
    // Play cutscene
    // ---------------------------------------------------------
    public void PlayCutscene(string cutsceneName, bool skipFadeout = false, System.Action onComplete = null)
    {
        if (isPlaying) return;
        Cutscene cutscene = getCutsceneScriptable(cutsceneName);

        if (cutscene == null)
        {
            Debug.LogError($"Cutscene '{cutsceneName}' not found! Make sure it’s in a Resources folder.");
            return;
        }

        cutsceneOnComplete = onComplete;
        StartCoroutine(PlayCutsceneRoutine(cutscene, skipFadeout, onComplete));
    }

    private IEnumerator PlayCutsceneRoutine(Cutscene cutscene, bool skipFadeout, System.Action onComplete)
    {
        isPlaying = true;
        skipFirstClickBlockerClick = true;

        clickBlocker.SetActive(true);
        clickAgainImage.SetActive(false);

        // -------------------------------------------------
        // PLAY MUSIC
        // -------------------------------------------------
        if (cutscene.backgroundMusic)
        {
            audioSource.clip = cutscene.backgroundMusic;
            audioSource.Play();
        }

        // Start transparent
        Color overlayColor = fadeOverlay.color;
        overlayColor.a = 0f;
        fadeOverlay.color = overlayColor;

        // FADE IN
        yield return StartCoroutine(FadeOverlay(1f, fadeDuration));

        // -------------------------------------------------
        // PLAY ANIMATION
        // -------------------------------------------------
        if (cutscene.animation != null && animator != null)
        {
            animatorObj.SetActive(true);
            ResizeAnimatorObj();
            animator.Play(cutscene.animation.name, 0, 0f);
        } else if (cutscene.animation == null)
        {
            Debug.LogWarning("Missing cutscene.animation");
        } else if (animator == null)
        {
            Debug.LogWarning("Missing animator");
        }

        // FADE IN
        yield return StartCoroutine(FadeOverlay(0f, fadeDuration));

        // SCROLL UP LINEARLY UNTIL PLAYER CAN SEE TOP OF animatorObj
        yield return StartCoroutine(ScrollUp(cutscene.duration));

        // -------------------------------------------------
        // FADE OUT
        // -------------------------------------------------
        yield return StartCoroutine(FadeOverlay(1f, fadeDuration, true));

        audioSource.Stop();

        // Optional fade back to transparent
        if (!skipFadeout)
        {
            yield return StartCoroutine(FadeOverlay(0f, 1f));
        }

        isPlaying = false;
        skipFirstClickBlockerClick = false;

        clickBlocker.SetActive(false);
        clickAgainImage.SetActive(false);
        animatorObj.SetActive(false);

        onComplete?.Invoke();
    }

    // ---------------------------------------------------------
    // Skip Handling
    // ---------------------------------------------------------
    public void OnClickBlocker()
    {
        if (skipFirstClickBlockerClick)
        {
            skipFirstClickBlockerClick = false;
            return;
        }

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
        // fade to black quickly
        yield return StartCoroutine(FadeOverlay(1f, 0.25f));

        // stop audio
        audioSource.Stop();

        clickBlocker.SetActive(false);
        clickAgainImage.SetActive(false);
        isPlaying = false;

        cutsceneOnComplete?.Invoke();
    }

    // ---------------------------------------------------------
    // Fade helper
    // ---------------------------------------------------------
    private IEnumerator FadeOverlay(float targetAlpha, float usedDuration = 0f, bool fadeAudio = false)
    {
        Color color = fadeOverlay.color;
        float startAlpha = color.a;
        float startVolume = audioSource.volume;
        float elapsed = 0f;
    
        float actualDuration = (usedDuration > 0f) ? usedDuration : fadeDuration;
    
        while (elapsed < actualDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / actualDuration;
    
            // Fade overlay
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            fadeOverlay.color = color;
    
            // Fade audio if requested
            if (fadeAudio)
            {
                audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            }
    
            yield return null;
        }
    
        // Ensure final values are exact
        color.a = targetAlpha;
        fadeOverlay.color = color;
    
        if (fadeAudio)
            audioSource.volume = 1f;
    }
    // ---------------------------------------------------------
    // Scroll up helper
    // ---------------------------------------------------------
    private IEnumerator ScrollUp(float usedDuration = 1f)
    {
        RectTransform rt = animatorObj.GetComponent<RectTransform>();
        if (rt == null) yield break;
    
        // Starting position (current anchored position)
        Vector2 startPos = rt.anchoredPosition;
    
        // Calculate target position
        // We want the top of the RectTransform to align with the top of its parent
        RectTransform parentRT = rt.parent.GetComponent<RectTransform>();
        if (parentRT == null) yield break;
    
        float parentHeight = parentRT.rect.height;
        float targetY = startPos.y - (rt.rect.height - parentHeight);
    
        // Only scroll if the content is taller than the parent
        if (rt.rect.height <= parentHeight)
            yield break;
    
        float elapsed = 0f;
    
        while (elapsed < usedDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / usedDuration);
            float newY = Mathf.Lerp(startPos.y, targetY, t);
            rt.anchoredPosition = new Vector2(startPos.x, newY);
            yield return null;
        }
    
        // Ensure final position is exact
        rt.anchoredPosition = new Vector2(startPos.x, targetY);
    }
}

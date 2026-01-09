using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private GameObject clickAgainImage;
    [SerializeField] private GameObject clickBlocker;
    [SerializeField] private GameObject animatorObj;   // used for animation cutscenes

    private Animator animator;   // used for animation cutscenes

    [SerializeField] private float fadeDuration = 0.6f;

    private bool isPlaying = false;
    private string path = "ScriptableObjects/Cutscenes/";

    // Skip system
    private bool waitingForSecondClick = false;
    private float doubleClickTimeout = 4f;
    private Coroutine resetClickRoutine;
    private System.Action cutsceneOnComplete;
    private bool skipFirstClickBlockerClick = true;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Prevent duplicate subscriptions
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 🔥 This runs EVERY time a scene loads
        //animatorObj.SetActive(false);
        // Your code here
        //StartCoroutine(FadeOverlay(0f, 1f));

    }

    private void Start() {
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

    void SetAnimation(AnimationClip clip)
    {
        if (clip == null) return;
        if (animator == null)
        {
            Debug.LogError("Animator component missing on animatorObj");
            return;
        }
        // If no controller is assigned, this is setup error
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("Animator has no RuntimeAnimatorController assigned");
            return;
        }
        
        // Create or reuse an override controller
        AnimatorOverrideController overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;

        if (overrideController == null)
        {
            overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        }

        // Override the first (default) animation clip
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);

        if (overrides.Count == 0)
        {
            Debug.LogError("Animator controller has no clips to override");
            return;
        }
        Debug.Log("-0-0-0");
        Debug.Log(overrides[0].Key);
        Debug.Log(clip.name);
        overrides[0] = new KeyValuePair<AnimationClip, AnimationClip>(
            overrides[0].Key,
            clip
        );

        overrideController.ApplyOverrides(overrides);
        animator.runtimeAnimatorController = overrideController;

        // (Optional but recommended)
        // animator.Rebind();
        // animator.Update(0f);
    }

    public void ChangeAnimatorController(
        Animator animatorToChange,
        RuntimeAnimatorController controller,
        string stateName = "idle",
        int layer = 0,
        float normalizedTime = 0f
    )
    {
        if (controller != null)
        {
            animatorToChange.runtimeAnimatorController = controller;
        }
        animator.Rebind();
        animator.Update(0f);
    }
    
    // ---------------------------------------------------------
    // Load ScriptableObject by name
    // ---------------------------------------------------------
    public Cutscene getCutsceneScriptable(string name)
    {
        string fullPath = path + name;
        Debug.Log(fullPath);
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
        if (cutscene.backgroundMusic && AudioManager.Instance)
        {
            AudioManager.Instance.AudioSource.clip = cutscene.backgroundMusic;
            AudioManager.Instance.AudioSource.Play();
        }

        // Start transparent
        Color transparentColor = fadeOverlay.color;
        transparentColor.a = 0f;
        fadeOverlay.color = transparentColor;
        Debug.Log("playing cutscene routine did audio");

        // FADE IN
        yield return StartCoroutine(FadeOverlay(1f, fadeDuration));
        Debug.Log("playing cutscene routine fade overlay");

        // -------------------------------------------------
        // PLAY ANIMATION
        // -------------------------------------------------
        if (cutscene.controller != null && animator != null)
        {
            animatorObj.SetActive(true);
            // SetAnimation(cutscene.animation);
            // ChangeAnimatorController(animator, cutscene.controller);
            ResizeAnimatorObj();
            animator.Play(0, 0, 0f); // layer 0, default state, time = 0
        } else if (cutscene.animation == null)
        {
            Debug.LogWarning("Missing cutscene.animation");
        } else if (animator == null)
        {
            Debug.LogWarning("Missing animator");
        }
        Debug.Log("playing cutscene routine fadein");

        // FADE IN
        yield return StartCoroutine(FadeOverlay(0f, fadeDuration));

        // SCROLL UP LINEARLY UNTIL PLAYER CAN SEE TOP OF animatorObj
        yield return StartCoroutine(ScrollUp(cutscene.duration));

        // -------------------------------------------------
        // FADE OUT
        // -------------------------------------------------
        yield return StartCoroutine(FadeOverlay(1f, fadeDuration, true));

        if (AudioManager.Instance)
        {
            AudioManager.Instance.AudioSource.Stop();
        }
        onComplete?.Invoke();

        yield return new WaitForSeconds(0.02f);
        // Optional fade back to transparent
        if (skipFadeout)
        {
            Debug.Log("skipped fadeout, quick fade");
            yield return StartCoroutine(FadeOverlay(0f, 0.05f));
        }
        else
        {
            Debug.Log("slow fade");
            yield return StartCoroutine(FadeOverlay(0f, 0.5f));
        }
        CleanUp();
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
        if (AudioManager.Instance)
        {
            AudioManager.Instance.AudioSource.Stop();
        }
        cutsceneOnComplete?.Invoke();
        yield return new WaitForSeconds(0.02f);

        CleanUp();

    }

    private void CleanUp()
    {
        skipFirstClickBlockerClick = true;
        waitingForSecondClick = false;
        Color transparentColor = fadeOverlay.color;
        transparentColor.a = 0f;
        fadeOverlay.color = transparentColor;
        fadeOverlay.color = transparentColor;

        isPlaying = false;
        skipFirstClickBlockerClick = false;

        clickBlocker.SetActive(false);
        clickAgainImage.SetActive(false);
        ResetAnimatorObj();
        animatorObj.SetActive(false);
        
    }

    // ---------------------------------------------------------
    // Fade helper
    // ---------------------------------------------------------
    private IEnumerator FadeOverlay(float targetAlpha, float usedDuration = 0f, bool fadeAudio = false)
    {
        Color color = fadeOverlay.color;
        float startAlpha = color.a;
        float startVolume = 1f;
        if (AudioManager.Instance)
        {
            startVolume = AudioManager.Instance.AudioSource.volume;
        }
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
            if (fadeAudio && AudioManager.Instance)
            {
                AudioManager.Instance.AudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            }
    
            yield return null;
        }
        Debug.Log("finishing fade overlay");

        // Ensure final values are exact
        color.a = targetAlpha;
        fadeOverlay.color = color;

        if (fadeAudio && AudioManager.Instance)
        {
            AudioManager.Instance.AudioSource.volume = 1f;
        }
        Debug.Log("finishing fade overlay2");
    }
    // ---------------------------------------------------------
    // Scroll up helper
    // ---------------------------------------------------------
    private IEnumerator ScrollUp(float usedDuration = 1f)
    {
        RectTransform rt = animatorObj.GetComponent<RectTransform>();
        if (rt == null) yield break;
    
        // Starting position (current anchored position)
        Vector2 startPos = new Vector2(0, 0);
    
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

    private void ResetAnimatorObj()
    {
        if (animatorObj == null) return;
        RectTransform rt = animatorObj.GetComponent<RectTransform>();
        if (rt == null) return;
    
        rt.anchoredPosition = new Vector2(0, 0);
    }
}

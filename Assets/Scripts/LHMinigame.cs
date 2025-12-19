using UnityEngine;
using UnityEngine.UI; // Needed for Image
using System.Collections;

public class LHMinigame : MonoBehaviour
{
    [SerializeField] private GameObject tools;
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private SpriteRenderer glowEffect;
    [SerializeField] private SpriteRenderer mercuryPool;
    [SerializeField] private Sprite lighthouseRoomCloseSprite;
    [SerializeField] private Sprite lighthouseRoomOpenSprite;
    [SerializeField] private Sprite glowScissorsSprite;
    [SerializeField] private Sprite glowOilSprite;
    [SerializeField] private Sprite glowMercurySprite;
    [SerializeField] private playlighthouseanim lighthouseAnimator;
    

    [SerializeField] private Image wrenchRenderer;
    [SerializeField] private Image oilRenderer;
    [SerializeField] private Image scissorsRenderer;
    [SerializeField] private Image mercuryRenderer;

    [SerializeField] private AudioClip lockSound;
    [SerializeField] private AudioClip wrenchSound;
    [SerializeField] private AudioClip oilSound;
    [SerializeField] private AudioClip scissorsSound;
    [SerializeField] private AudioClip mercurySound;
    [SerializeField] private AudioClip finishClick;
    [SerializeField] private AudioClip finishLoop;
    [SerializeField] private ParallaxScroll shadowScrollScript;

    [SerializeField] private MiscObjectClick miscObjectClick;

	private Coroutine fadeCoroutine;

    // Hacky variable to keep us from double clicking other objects if we click off the dialog
    private bool delayNextMisclick = false; 
    
    void Start()
    {
		if (GameState.Get<bool>("lighthouse_fixed"))
		{
			shadowScrollScript.startScale = 1.05f;
	        StartCoroutine(PlaySoundDelayedRoutine(finishLoop, 0.9f, true, 0.01f));
		}
    }

	private IEnumerator PlaySoundDelayedRoutine(AudioClip sfx, float volume, bool loop, float delay)
	{
		yield return new WaitForSeconds(delay);
        miscObjectClick.PlaySound(sfx, volume, loop);
	}

    public void StartMinigame()
    {
        if (GameState.Get<bool>("lighthouse_fixed"))
        {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("Lighthouse/already_fixed"));
            return;
        }

        GameState.Set("minigame_open", true);
        tools.SetActive(true);
    }

    public void StopMinigame()
    {
        GameState.Set("minigame_open", false);
        tools.SetActive(false);
    }
    public void ClickWrench()
    {
        if (GameState.Get<bool>("lighthouse_fixed"))
        {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("Lighthouse/already_fixed"));
            return;
        }

        if (background != null && lighthouseRoomOpenSprite != null)
        {
            background.sprite = lighthouseRoomOpenSprite;
            GameState.Set("lighthouse_opened", true);
        }

        if (wrenchRenderer != null)
        {
            wrenchRenderer.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Make it appear used
        }

        if (GameState.Get<bool>("wrench_used"))
        {
            // We've already used mercury
            return;
        }
        GameState.Set("wrench_used", true);
        miscObjectClick.PlaySound(wrenchSound);
    }
    
    public void ClickOil()
    {
        if (!GameState.Get<bool>("lighthouse_opened"))
        {
            // Need to open lighthouse door first
            if (delayNextMisclick)
            {
                delayNextMisclick = false;
            }
            else
            {
                DialogueManager.ShowDialogue(miscObjectClick.getDialogue("Lighthouse/open_lighthouse_first"));
                delayNextMisclick = true;
            }
            return;
        }
        if (oilRenderer != null)
        {
            oilRenderer.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Make it appear used
        }
        
        if (GameState.Get<bool>("oil_used"))
        {
            // We've already used mercury
            return;
        }
        GameState.Set("oil_used", true);
        glowEffect.sprite = glowOilSprite;
        glowEffect.color = new Color(1f, 1f, 1f, 1f);
		StartFadeOutDelay(glowEffect, 0.05f, 1.5f);
        miscObjectClick.PlaySound(oilSound);
        testIfLighthouseFininshed();
    }
    
    public void ClickScissors()
    {
        if (!GameState.Get<bool>("lighthouse_opened"))
        {
            // Need to open lighthouse door first
            if (delayNextMisclick)
            {
                delayNextMisclick = false;
            }
            else
            {
                DialogueManager.ShowDialogue(miscObjectClick.getDialogue("Lighthouse/open_lighthouse_first"));
                delayNextMisclick = true;
            }
            return;
        }
        if (scissorsRenderer != null)
        {
            scissorsRenderer.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Make it appear used
        }
        if (GameState.Get<bool>("scissors_used"))
        {
            // We've already used mercury
            return;
        }
        GameState.Set("scissors_used", true);
        miscObjectClick.PlaySound(scissorsSound);
        
        glowEffect.sprite = glowScissorsSprite;
        glowEffect.color = new Color(1f, 1f, 1f, 1f);
		StartFadeOutDelay(glowEffect, 0.05f, 1.5f);
        testIfLighthouseFininshed();
    }

    public void ClickMercury()
    {
        if (!GameState.Get<bool>("lighthouse_opened"))
        {
            // Need to open lighthouse door first
            if (delayNextMisclick)
            {
                delayNextMisclick = false;
            }
            else
            {
                DialogueManager.ShowDialogue(miscObjectClick.getDialogue("Lighthouse/open_lighthouse_first"));
                delayNextMisclick = true;
            }
            return;
        }
        if (mercuryRenderer != null)
        {
            mercuryRenderer.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Make it appear used
        }

        if (GameState.Get<bool>("mercury_used"))
        {
            // We've already used mercury
            return;
        }
        GameState.Set("mercury_used", true);
        glowEffect.sprite = glowMercurySprite;
        glowEffect.color = new Color(1f, 1f, 1f, 1f);
		StartFadeOutDelay(glowEffect, 0.05f, 1.5f);
        miscObjectClick.PlaySound(mercurySound);
        testIfLighthouseFininshed();
        
        if (mercuryPool != null)
        {
            // Make mercury visible immediately
            mercuryPool.color = new Color(1f, 1f, 1f, 1f);

            // Start fade coroutine

            StartCoroutine(FadeOutAfterDelay(mercuryPool, 2f, 2f));
        }
    }
    
	// Call this function. This will cancel any existing instances of a fadeout
	private void StartFadeOutDelay(SpriteRenderer sprite, float delay, float duration)
	{
    	// If there's already a fade running, stop it first
    	if (fadeCoroutine != null)
    	{
        	StopCoroutine(fadeCoroutine);
    	}

    	// Start a new one and keep the reference
    	fadeCoroutine = StartCoroutine(FadeOutAfterDelay(sprite, delay, duration));
	}

    private IEnumerator FadeOutAfterDelay(SpriteRenderer sprite, float delay, float duration)
    {
        // Wait before fading
        yield return new WaitForSeconds(delay);

        Color startColor = sprite.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            sprite.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
        }

        // Ensure final alpha is 0
        sprite.color = endColor;
    	fadeCoroutine = null; // clear reference when done
    }

    private void testIfLighthouseFininshed()
    {
        if (GameState.Get<bool>("scissors_used") && GameState.Get<bool>("mercury_used") &&
            GameState.Get<bool>("oil_used"))
        {
            // We've finished!
            Debug.Log("Lighthouse now fixed");
        	GameState.Set("lighthouse_fixed", true);
            MessageBus.Instance.Publish("CompleteTask", "task_lighthouse");

            StartCoroutine(CloseAfterDelay(2.4f));

            // if (!isAnimating)
            //     StartCoroutine(PlayLighthouseAnimation());
        }
    }
    private IEnumerator CloseAfterDelay(float delay)
    {
        // Wait before close
        yield return new WaitForSeconds(delay * 0.55f);
        if (background != null && lighthouseRoomCloseSprite != null)
        {
            background.sprite = lighthouseRoomCloseSprite;
        }
        miscObjectClick.PlaySound(lockSound);
        yield return new WaitForSeconds(delay * 0.45f);

        GameState.Set("lighthouse_opened", false);
        lighthouseAnimator.gameObject.SetActive(true);
		shadowScrollScript.startScale = 1.05f;

        miscObjectClick.PlaySound(finishClick);
        miscObjectClick.PlaySound(finishLoop, 1f, true);

        StopMinigame();
        DialogueManager.ShowDialogue(miscObjectClick.getDialogue("Lighthouse/work_done"));
    }

}

using UnityEngine;
using System.Collections;

public class StoveScript : MonoBehaviour
{
    [SerializeField] private GameObject doneAnim;   // used for animation 
    [SerializeField] private SpriteRenderer potGlowAnim;   // used for animation 
    [SerializeField] private GameObject eatButton;
    [SerializeField] private GameObject fish;
    [SerializeField] private GameObject fishAnim;
    [SerializeField] private GameObject soupCover;
    [SerializeField] private MiscObjectClick miscObjectClick;

    [SerializeField] private Sprite choppedFishSprite;
    [SerializeField] private Sprite choppedFishSpriteTwitch;
    [SerializeField] private Sprite choppedFishSpriteAlive;
    [SerializeField] private Sprite choppedFishSpriteTransition;
    [SerializeField] private Sprite choppedFishSpriteHover;

    [SerializeField] private AudioClip cornSound;
    [SerializeField] private AudioClip pepperSound;
    [SerializeField] private AudioClip alcoholSound;
    [SerializeField] private AudioClip fishChopSound;
    [SerializeField] private AudioClip eatSound;
    [SerializeField] private GameObject returnButton;
    [SerializeField] private GameObject holdingKnife;
    [SerializeField] private GameObject blackScreenFlash;

    private Coroutine glowRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bool isDay1 = GameState.Get<int>("day") == 1;
        bool isDay2 = GameState.Get<int>("day") == 2;
        bool gatheredFish = GameState.Get<bool>("gathered_fish");
        bool isBreakfastTime = !GameState.Get<bool>("ate_breakfast");

        // Fish is active only on Day 1 dinner (if gathered) or Day 2 breakfast
        bool shouldShowFish = (isDay1 && gatheredFish) || (isDay2 && isBreakfastTime);

        if (shouldShowFish)
        {
            fish.SetActive(true);
            if (GameState.Get<bool>("fish_clicked", false))
            {
                // Change gameObject "fish"'s sprite to a new sprite
                SpriteRenderer sr = fish.GetComponent<SpriteRenderer>();
                sr.sprite = choppedFishSprite;
                InteractableObject interactable = fish.GetComponent<InteractableObject>();
                interactable.hover_sprite = choppedFishSpriteHover;
                interactable.default_sprite = choppedFishSprite;

            }
        }
        if (IsDoneCooking())
        {
            EnableEating();
        }
        if (GameState.Get<bool>("corn_clicked") || GameState.Get<bool>("pepper_clicked") || GameState.Get<bool>("alcohol_clicked"))
        {
            // If you've prepared some food, but haven't eaten it and therefore are still hungry
            // Hide the soupcover to show the soup in progress
            if (GameState.Get<bool>("hungry"))
            {
                soupCover.SetActive(false);
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickCorn()
    {
        if (!GameState.Get<bool>("hungry"))
        {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/not_hungry"));
            return;
        }

        if (GameState.Get<bool>("corn_clicked"))
        {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/corn_already_added"));
        }
        else
        {
            GameState.Set("corn_clicked", true);
            MessageBus.Instance.Publish("FloatText", -1.2f, -0.1f, "+CORN");
            GlowPot();
            if (IsDoneCooking())
            {
                EnableEating();
            }
            PlaySound(cornSound);
        }
    }

    public void ClickPepper()
    {
        if (!GameState.Get<bool>("hungry"))
        {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/not_hungry"));
            return;
        }
        if (GameState.Get<bool>("pepper_clicked"))
        {
            // Already clicked
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/pepper_already_added"));
        } else {
            GameState.Set("pepper_clicked", true);
            MessageBus.Instance.Publish("FloatText", -1.35f, -0.1f, "+PEPPER");
            GlowPot();
            if (IsDoneCooking())
            {
                EnableEating();
            }
            PlaySound(pepperSound);
        }
    }

    public void ClickAlcohol()
    {
        if (!GameState.Get<bool>("hungry"))
        {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/not_hungry"));
            return;
        }
        if (GameState.Get<bool>("alcohol_clicked"))
        {
            // Already clicked
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/alcohol_already_added"));
        } else {
            GameState.Set("alcohol_clicked", true);
            MessageBus.Instance.Publish("FloatText", -1.25f, 0, "+RUM");
            GlowPot();
            if (IsDoneCooking())
            {
                EnableEating();
            }
            PlaySound(alcoholSound);
        }
    }

    public void ClickFish()
    {
        if (!GameState.Get<bool>("hungry"))
        {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/not_hungry"));
            return;
        }
        if (GameState.Get<bool>("fish_clicked"))
        {
            // Already clicked
            bool isDay1 = GameState.Get<int>("day") == 1;
            if (isDay1) {
                DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/fish_already_added"));
            } else {
                DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/fish_not_want"));
            }
        }
        else
        {
            StartCoroutine(FishChoppingSequence());
        }
    }

    private IEnumerator FishChoppingSequence()
    {
        // Block interaction
        GameState.Set("navigationBlocked", true);
        if (returnButton != null)
        {
            returnButton.SetActive(false);
        }

        Vector3 initialKnifePos = Vector3.zero;
        if (holdingKnife != null)
        {
            initialKnifePos = holdingKnife.transform.localPosition;
            // move it to y position -3 and x position 2
            holdingKnife.transform.localPosition = new Vector3(2f, -3f, initialKnifePos.z);
            holdingKnife.SetActive(true);
        }

        // quickly animate it going to y position 0, x position 2 using cubic.easeOut
        float durationUp = 0.35f;
        float elapsed = 0f;
        Vector3 upStart = new Vector3(2f, -3f, initialKnifePos.z);
        Vector3 upEnd = new Vector3(2f, 0f, initialKnifePos.z);
        while (elapsed < durationUp)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / durationUp);
            float tEased = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease-out
            if (holdingKnife != null)
            {
                holdingKnife.transform.localPosition = Vector3.Lerp(upStart, upEnd, tEased);
            }
            yield return null;
        }
        if (holdingKnife != null) holdingKnife.transform.localPosition = upEnd;

        // then swing down cubic.easeIn by moving to y position -0.8, x position 1.65
        float durationDown = 0.25f;
        elapsed = 0f;
        Vector3 downStart = upEnd;
        Vector3 downEnd = new Vector3(1.65f, -0.8f, initialKnifePos.z);
        while (elapsed < durationDown)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / durationDown);
            float tEased = t * t * t; // Cubic ease-in
            if (holdingKnife != null)
            {
                holdingKnife.transform.localPosition = Vector3.Lerp(downStart, downEnd, tEased);
            }
            yield return null;
        }
        if (holdingKnife != null) holdingKnife.transform.localPosition = downEnd;

        // black screen covering flash appears
        if (blackScreenFlash != null)
        {
            blackScreenFlash.SetActive(true);
        }

        yield return new WaitForSeconds(0.05f);

        // holding knife game object is deactivated and position reset
        if (holdingKnife != null)
        {
            holdingKnife.SetActive(false);
            holdingKnife.transform.localPosition = initialKnifePos;
        }

        // fish appears in its chopped state
        GameState.Set("fish_clicked", true);
        MessageBus.Instance.Publish("FloatText", -1.2f, -0.1f, "+FISH");
        PlaySound(fishChopSound);
        
        bool isDay2 = GameState.Get<int>("day") == 2;
        TwitchFish(isDay2);
        
        SpriteRenderer sr = fish.GetComponent<SpriteRenderer>();
        sr.sprite = choppedFishSprite;
        InteractableObject interactable = fish.GetComponent<InteractableObject>();
        if (interactable != null)
        {
            interactable.hover_sprite = choppedFishSpriteHover;
            interactable.default_sprite = choppedFishSprite;
        }
        GlowPot();

        if (IsDoneCooking())
        {
            StartCoroutine(EnableEatingDelayed(0.5f));
        }

        yield return new WaitForSeconds(0.15f);

        if (blackScreenFlash != null)
        {
            blackScreenFlash.SetActive(false);
        }

        if (!isDay2)
        {
            GameState.Set("navigationBlocked", false);
            if (returnButton != null)
            {
                returnButton.SetActive(true);
            }
        }
    }

    public void ClickPot()
    {
        if (!GameState.Get<bool>("hungry"))
        {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/not_hungry"));
            return;
        }
        
        if (IsDoneCooking())
        {
            PlaySound(eatSound);
            MessageBus.Instance.Publish("FloatText", 0, -0.4f, "+SANITY", "green");
            MessageBus.Instance.Publish("PlusSanity", 1);

            if (GameState.Get<bool>("ate_breakfast"))
            {
                MessageBus.Instance.Publish("CompleteTask", "task_dinner");
                GameState.Set("ate_dinner", true);
                if (GameState.Get<int>("day") >= 2)
                {
                    GameState.Set("ready_to_sleep", true);
                }
            }
            else
            {
                MessageBus.Instance.Publish("CompleteTask", "task_breakfast");
            }

            GameState.Set("corn_clicked", false);
            GameState.Set("pepper_clicked", false);
            GameState.Set("alcohol_clicked", false);
            GameState.Set("fish_clicked", false);
            
            GameState.Set("ate_breakfast", true);
            GameState.Set("hungry", false);
            doneAnim.SetActive(false);
            soupCover.SetActive(true);
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/eating"));
        }
        else
        {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/not_ready"));
        }

    }
    
    private bool IsDoneCooking() {
        if (GameState.Get<bool>("ate_breakfast")) {
            return IsDoneCookingDinner();
        } else {
            return IsDoneCookingBreakfast();
        }
    }

    private bool IsDoneCookingBreakfast() {
        if (!GameState.Get<bool>("hungry"))
        {
            return true;
        }
        if (GameState.Get<bool>("corn_clicked") && GameState.Get<bool>("pepper_clicked") && GameState.Get<bool>("alcohol_clicked")) {
            return true;
        }

        return false;
    }

    private bool IsDoneCookingDinner() {
        if (!GameState.Get<bool>("hungry"))
        {
            return true;
        }
        if (GameState.Get<bool>("corn_clicked") && GameState.Get<bool>("pepper_clicked") && GameState.Get<bool>("alcohol_clicked")) {
            if (GameState.Get<bool>("fish_clicked")) {
                return true;
            }
        }

        return false;
    }


    private void EnableEating() {
        if (!doneAnim.activeInHierarchy)
        {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/ready_to_eat"));
        }
        doneAnim.SetActive(true);
    }

    private IEnumerator EnableEatingDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnableEating();
    }


    private void GlowPot()
    {
        if (glowRoutine != null)
            StopCoroutine(glowRoutine);
        soupCover.SetActive(false);
        glowRoutine = StartCoroutine(GlowRoutine());
    }

    private IEnumerator GlowRoutine()
    {
        // snap to 100% alpha
        Color c = potGlowAnim.color;
        c.a = 1f;
        potGlowAnim.color = c;

        float duration = 1.2f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / duration);
            potGlowAnim.color = c;
            yield return null;
        }

        c.a = 0f;
        potGlowAnim.color = c;
        glowRoutine = null;
    }


    
    private void PlaySound(AudioClip clip)
    {
        if (!clip) return;
        if (!AudioManager.Instance) return;

        var src = AudioManager.Instance.AudioSource;
        src.PlayOneShot(clip);
    }

    private void TwitchFish(bool chainJumpscare = false)
    {
        if (chainJumpscare)
        {
            GameState.Set("navigationBlocked", true);
            if (returnButton != null)
            {
                returnButton.SetActive(false);
            }
        }
        fish.SetActive(false);
        fishAnim.SetActive(true);
        SpriteRenderer sr = fishAnim.GetComponent<SpriteRenderer>();
        sr.sprite = choppedFishSpriteTwitch;
        StartCoroutine(TwitchFishCoroutine(chainJumpscare));
    }

    private IEnumerator TwitchFishCoroutine(bool chainJumpscare)
    {
        yield return new WaitForSeconds(0.12f);
        SpriteRenderer sr = fishAnim.GetComponent<SpriteRenderer>();
        sr.sprite = choppedFishSprite;
        yield return new WaitForSeconds(1f);

        if (chainJumpscare)
        {
            StartCoroutine(JumpscareFishCoroutine());
        } else {
            fish.SetActive(true);
            fishAnim.SetActive(false);
        }
    }

    private IEnumerator JumpscareFishCoroutine()
    {
        SpriteRenderer sr = fishAnim.GetComponent<SpriteRenderer>();

        // choppedFishSprite for 0.75s
        sr.sprite = choppedFishSprite;
        yield return new WaitForSeconds(0.75f);

        // choppedFishSpriteTwitch for 0.1s
        sr.sprite = choppedFishSpriteTwitch;
        yield return new WaitForSeconds(0.1f);

        // choppedFishSprite for 2s
        sr.sprite = choppedFishSprite;
        yield return new WaitForSeconds(1.2f);

        // choppedFishSpriteTwitch for 0.2s
        sr.sprite = choppedFishSpriteTwitch;
        yield return new WaitForSeconds(0.4f);
        MessageBus.Instance.Publish("PlaySound", "squish");

        // choppedFishSpriteTransition for 0.1s
        sr.sprite = choppedFishSpriteTransition;
        yield return new WaitForSeconds(0.05f);

        // choppedFishSpriteAlive (fish_alive) for 0.25s
        sr.sprite = choppedFishSpriteAlive;
        yield return new WaitForSeconds(0.55f);

        // choppedFishSpriteTwitch for 0.1s
        sr.sprite = choppedFishSpriteTwitch;
        yield return new WaitForSeconds(0.05f);

        // Finish at choppedFishSprite
        sr.sprite = choppedFishSprite;
        yield return new WaitForSeconds(0.8f);
        // Play dialog stove/fish_ask
        DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/fish_ask"));
        // Re-enable interaction
        GameState.Set("navigationBlocked", false);
        if (returnButton != null)
        {
            returnButton.SetActive(true);
        }
        fish.SetActive(true);
        fishAnim.SetActive(false);
    }
}

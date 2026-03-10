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

    private Coroutine glowRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bool isDay2 = GameState.Get<int>("day") == 2;
        bool gatheredFish = GameState.Get<bool>("gathered_fish");
        if ((isDay2 || GameState.Get<bool>("ate_breakfast")) && !GameState.Get<bool>("ate_dinner") && (isDay2 || gatheredFish))
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
        } else {
            Debug.Log("fish not active");
            Debug.Log(isDay2);
            Debug.Log(GameState.Get<bool>("ate_breakfast"));
            Debug.Log(GameState.Get<bool>("ate_dinner"));
            Debug.Log(GameState.Get<bool>("gathered_fish"));
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
            GameState.Set("fish_clicked", true);
            MessageBus.Instance.Publish("FloatText", -1.2f, -0.1f, "+FISH");
            PlaySound(fishChopSound);
            // Change gameObject "fish"'s sprite to a new sprite
            bool isDay2 = GameState.Get<int>("day") == 2;
            TwitchFish(isDay2);
            SpriteRenderer sr = fish.GetComponent<SpriteRenderer>();
            sr.sprite = choppedFishSprite;
            InteractableObject interactable = fish.GetComponent<InteractableObject>();
            interactable.hover_sprite = choppedFishSpriteHover;
            interactable.default_sprite = choppedFishSprite;
            GlowPot();
            if (IsDoneCooking())
            {
                EnableEating();
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
        fish.SetActive(true);
        fishAnim.SetActive(false);
    }
}

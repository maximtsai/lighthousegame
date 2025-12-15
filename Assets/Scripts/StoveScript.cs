using UnityEngine;
using System.Collections;

public class StoveScript : MonoBehaviour
{
    [SerializeField] private GameObject doneAnim;   // used for animation 
    [SerializeField] private SpriteRenderer potGlowAnim;   // used for animation 
    [SerializeField] private GameObject eatButton;
    [SerializeField] private GameObject fish;
    [SerializeField] private MiscObjectClick miscObjectClick;

    [SerializeField] private AudioClip cornSound;
    [SerializeField] private AudioClip pepperSound;
    [SerializeField] private AudioClip alcoholSound;
    [SerializeField] private AudioClip fishChopSound;
    [SerializeField] private AudioClip eatSound;

    private Coroutine glowRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameState.Get<bool>("ate_breakfast") && !GameState.Get<bool>("ate_dinner") && GameState.Get<bool>("gathered_fish"))
        {
            fish.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickCorn()
    {
        if (GameState.Get<bool>("corn_clicked"))
        {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/corn_already_added"));
        }
        else
        {
            GlowPot();
            CheckIfDoneCooking();
            PlaySound(cornSound);
        }
        GameState.Set("corn_clicked", true);
    }

    public void ClickPepper()
    {
        if (GameState.Get<bool>("alcohol_clicked"))
        {
            // Already clicked
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/pepper_already_added"));
        } else {
            GlowPot();
            CheckIfDoneCooking();
            PlaySound(pepperSound);
        }
        GameState.Set("pepper_clicked", true);
    }

    public void ClickAlcohol()
    {
        if (GameState.Get<bool>("alcohol_clicked"))
        {
            // Already clicked
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/alcohol_already_added"));
        } else {
            GlowPot();
            CheckIfDoneCooking();
            PlaySound(alcoholSound);
        }
        GameState.Set("alcohol_clicked", true);
    }

    public void ClickFish()
    {
        if (GameState.Get<bool>("fish_clicked"))
        {
            // Already clicked
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/fish_already_added"));
        }
        else
        {
            GlowPot();
            CheckIfDoneCooking();
            PlaySound(fishChopSound);
        }
        GameState.Set("fish_clicked", true);
    }

    public void ClickPot()
    {
        if (!GameState.Get<bool>("hungry"))
        {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/not_hungry"));
            return;
        }
        if (doneAnim.activeInHierarchy)
        {
            PlaySound(eatSound);
            GameState.Set("ate_breakfast", true);
            GameState.Set("hungry", false);
        } else {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/not_ready"));
        }
    }
    
    private void CheckIfDoneCooking() {
        if (GameState.Get<bool>("ate_breakfast")) {
            CheckIfDoneCookingDinner();
        } else {
            CheckIfDoneCookingBreakfast();
        }
    }

    private void CheckIfDoneCookingBreakfast() {
        Debug.Log(GameState.Get<bool>("corn_clicked"));
        Debug.Log(GameState.Get<bool>("pepper_clicked"));
        Debug.Log(GameState.Get<bool>("alcohol_clicked"));
        Debug.Log("=====");
        if (GameState.Get<bool>("corn_clicked") && GameState.Get<bool>("pepper_clicked") && GameState.Get<bool>("alcohol_clicked")) {
            EnableEating();
        }
    }

    private void CheckIfDoneCookingDinner() {
        if (GameState.Get<bool>("corn_clicked") && GameState.Get<bool>("pepper_clicked") && GameState.Get<bool>("alcohol_clicked")) {
            if (GameState.Get<bool>("fish_clicked")) {
                EnableEating();
            }
        }
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

        glowRoutine = StartCoroutine(GlowRoutine());
    }

    private IEnumerator GlowRoutine()
    {
        // snap to 100% alpha
        Color c = potGlowAnim.color;
        c.a = 1f;
        potGlowAnim.color = c;

        float duration = 1.5f;
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
}

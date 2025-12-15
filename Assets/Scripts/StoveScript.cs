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
        if (IsDoneCooking())
        {
            EnableEating();
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
            GameState.Set("corn_clicked", true);
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
        if (GameState.Get<bool>("pepper_clicked"))
        {
            // Already clicked
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/pepper_already_added"));
        } else {
            GameState.Set("pepper_clicked", true);
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
        if (GameState.Get<bool>("alcohol_clicked"))
        {
            // Already clicked
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/alcohol_already_added"));
        } else {
            GameState.Set("alcohol_clicked", true);
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
        if (GameState.Get<bool>("fish_clicked"))
        {
            // Already clicked
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/fish_already_added"));
        }
        else
        {
            GameState.Set("fish_clicked", true);
            GlowPot();
            if (IsDoneCooking())
            {
                EnableEating();
            }
            
            PlaySound(fishChopSound);
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
            if (GameState.Get<bool>("ate_breakfast"))
            {
                MessageBus.Instance.Publish("CompleteTask", "task_dinner");
            }
            else
            {
                MessageBus.Instance.Publish("CompleteTask", "task_breakfast");
            }

            
            GameState.Set("ate_breakfast", true);
            GameState.Set("hungry", false);
            doneAnim.SetActive(false);
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
        if (GameState.Get<bool>("corn_clicked") && GameState.Get<bool>("pepper_clicked") && GameState.Get<bool>("alcohol_clicked")) {
            // EnableEating();
            return true;
        }

        return false;
    }

    private bool IsDoneCookingDinner() {
        if (GameState.Get<bool>("corn_clicked") && GameState.Get<bool>("pepper_clicked") && GameState.Get<bool>("alcohol_clicked")) {
            if (GameState.Get<bool>("fish_clicked")) {
                //EnableEating();
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
}

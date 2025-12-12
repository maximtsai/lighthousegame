using UnityEngine;

public class StoveScript : MonoBehaviour
{
    [SerializeField] private GameObject doneAnim;   // used for animation 
    [SerializeField] private GameObject eatButton;
    [SerializeField] private MiscObjectClick miscObjectClick;

    [SerializeField] private AudioClip cornSound;
    [SerializeField] private AudioClip pepperSound;
    [SerializeField] private AudioClip alcoholSound;
    [SerializeField] private AudioClip fishChopSound;
    [SerializeField] private AudioClip eatSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
            PlaySound(fishChopSound);
        }
        GameState.Set("fish_clicked", true);
    }

    public void ClickPot()
    {
        PlaySound(eatSound);
    }
    
    private void CheckIfDoneCooking() {
        if (GameState.Get<bool>("ate_breakfast")) {
            CheckIfDoneCookingDinner();
        } else {
            CheckIfDoneCookingBreakfast();
        }
    }

    private void CheckIfDoneCookingBreakfast() {
        if (GameState.Get<bool>("corn_clicked") && GameState.Get<bool>("pepper_clicked") && GameState.Get<bool>("alcohol_clicked")) {
            EnableEating();
        } else {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/not_ready"));
        }
    }

    private void CheckIfDoneCookingDinner() {
        if (GameState.Get<bool>("corn_clicked") && GameState.Get<bool>("pepper_clicked") && GameState.Get<bool>("alcohol_clicked")) {
            if (GameState.Get<bool>("fish_clicked")) {
                EnableEating();
            }
        } else {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("stove/not_ready"));
        }
    }

    private void EnableEating() {
        doneAnim.SetActive(true);
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (!clip) return;
        if (!AudioManager.Instance) return;

        var src = AudioManager.Instance.AudioSource;
        src.PlayOneShot(clip);
    }
}

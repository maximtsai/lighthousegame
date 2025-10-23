using UnityEngine;
using System.Collections;

public class BurialScript : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    [SerializeField] private GameObject shovel;
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Sprite backgroundDugSprite;
    [SerializeField] private Sprite backgroundCoveredSprite;
    [SerializeField] private Sprite backgroundRevealedSprite;
    
    [SerializeField] private MiscObjectClick miscObjectClick;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Ambience ambience = Ambience.Instance;

        // Update track 1
        UpdateTrack(ambience, bgLoop1, 0.65f, 1);
        // Update track 2
        UpdateTrack(ambience, bgLoop2, 0.3f, 2);
        if (GameState.Get<bool>("has_dug"))
        {
            Destroy(shovel);
            if (GameState.Get<bool>("has_buried"))
            {
                background.sprite = backgroundCoveredSprite;
            }
            else
            {
                background.sprite = backgroundDugSprite;
            }

        }
    }
    
    private IEnumerator PlaySoundDelayedRoutine(AudioClip sfx, float volume, bool loop, float delay)
    {
        yield return new WaitForSeconds(delay);
        miscObjectClick.PlaySound(sfx, volume, loop);
    }

    private void UpdateTrack(Ambience ambience, AudioClip newClip, float volume, int channel)
    {
        if (ambience == null)
        {
            return;
        }
        // Check if the new clip is different from the current clip
        AudioClip currentClip = ambience.GetCurrentClip(channel);
        if (currentClip != newClip)
        {
            // Play new clip if it's different
            ambience.PlayTrack(newClip, volume, channel);
        }
        else
        {
            // Update volume if the clip is the same
            ambience.SetVolume(channel, volume);
        }
    }
    
    public void ClickMound()
    {
        if (!GameState.Get<bool>("has_dug"))
        {
            // Haven't dug up the grave yet, do that.
            GameState.Set("has_dug", true);
            Destroy(shovel);
            if (background != null && backgroundDugSprite != null)
            {
                background.sprite = backgroundDugSprite;
                DialogueManager.ShowDialogue(miscObjectClick.getDialogue("burial/finished_digging"));
            }
            return;
        }
        
        if (!GameState.Get<bool>("has_buried"))
        {
            // There's a hole now
            GameState.Set("has_buried", true);
            if (background != null && backgroundCoveredSprite != null)
            {
                background.sprite = backgroundCoveredSprite;
            }
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("burial/finished_burying"));

            return;
        }
        DialogueManager.ShowDialogue(miscObjectClick.getDialogue("burial/already_buried"));

    }
}

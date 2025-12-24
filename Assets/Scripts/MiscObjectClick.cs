using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // required for SceneManager

public class MiscObjectClick : MonoBehaviour
{
    private string path = "ScriptableObjects/Dialogues/";
    AudioSource audioSource;

    private void Awake()
    {
        if (AudioManager.Instance)
        {
            audioSource = AudioManager.Instance.AudioSource;
        }
    }

    public Dialogue getDialogue(string name)
    {
        string fullPath = path + name;
        Dialogue dialogue = Resources.Load<Dialogue>(fullPath);
        if (dialogue == null)
        {
            Debug.LogWarning("Dialogue not found: " + fullPath);
        }
        return dialogue;
    }
    public void ClickPartnerBed()
    {
        if (GameState.Get<int>("day") == 1)
        {
            if (GameState.Get<bool>("choresdone"))
            {
                DialogueManager.ShowDialogue(getDialogue("Bedroom/co_bed_2"));
            }
            else
            {
                DialogueManager.ShowDialogue(getDialogue("Bedroom/co_bed_1"));
            }
        }
        else
        {
            DialogueManager.ShowDialogue(getDialogue("Bedroom/co_bed_3"));
        }
    }
    public void ClickSelfBed()
    {
        if (GameState.Get<bool>("checked_weather") && GameState.Get<bool>("lighthouse_fixed") && GameState.Get<bool>("ate_dinner"))
        {
            Debug.Log("Go to next day");
        }
        else
        {
            if (!GameState.Get<bool>("ate_breakfast"))
            {
                // Just woke up!
                DialogueManager.ShowDialogue(getDialogue("Bedroom/my_bed_1"));
            } else if (GameState.Get<int>("day") == 1)
            {
                // Check up on coworker first
                DialogueManager.ShowDialogue(getDialogue("Bedroom/my_bed_3"));
            }
            else
            {
                if (GameState.Get<bool>("checked_weather") || GameState.Get<bool>("lighthouse_fixed") || GameState.Get<bool>("gathered_fish"))
                {
                    // I still have work to do!
                    DialogueManager.ShowDialogue(getDialogue("Bedroom/my_bed_2a"));
                }
                else
                {
                    // I have work to do!
                    DialogueManager.ShowDialogue(getDialogue("Bedroom/my_bed_2"));
                }
            }
        }
    }
    
    public void ClickLighthouseLight()
    {
        if (GameState.Get<bool>("lighthouse_fixed"))
        {
            DialogueManager.ShowDialogue(getDialogue("Lighthouse/fix_complete"));
        }
        else
        {
            if (GameState.Get<bool>("dropped_tool"))
            {
                DialogueManager.ShowDialogue(getDialogue("Lighthouse/missing_tool"));
            }
            else
            {
                // This starts the minigame too
                DialogueManager.ShowDialogue(getDialogue("Lighthouse/open_default"));
            }
        }
    }

    public void ShowDialog(string str)
    {
        DialogueManager.ShowDialogue(getDialogue(str));

    }
    
	public void GatherFish()
    {
		if (GameState.Get<bool>("gathered_fish", false))
        {
        	DialogueManager.ShowDialogue(getDialogue("dock/gather_fish_done"));

		} else if (GameState.Get<bool>("lighthouse_fixed", false) == false)
        {
            DialogueManager.ShowDialogue(getDialogue("dock/gather_fish_not_yet"));
        } else
        {
            // Successfully got fish
			GameState.Set("gathered_fish", true);
            GameState.Set("hungry", true);
            GameState.Set("is_nighttime", true);
            
            MessageBus.Instance.Publish("CompleteTask", "task_fish");
            DialogueManager.ShowDialogue(getDialogue("dock/gather_fish"));
		}
	}

	public void GoOutdoors() 
	{
        DialogueManager.ShowDialogue(getDialogue("go_outside"));
	}

	public void PlaySoundDelayed(AudioClip sfx, float volume = 1f, bool loop = false, float delay = 0.1f)
	{
   		StartCoroutine(InvokeAfterDelay(() => PlaySound(sfx, volume, loop), delay));
	}

	private IEnumerator InvokeAfterDelay(System.Action action, float delay)
	{
    	yield return new WaitForSeconds(delay);
    	action?.Invoke();
	}

    public void PlaySound(AudioClip sfx, float volume = 1f, bool loop = false, AudioSource sourceOverride = null)
    {
        if (sfx == null)
        {
            Debug.LogWarning("No AudioClip provided to PlaySound!");
            return;
        }

        AudioSource src = sourceOverride;

        if (src == null)
        {
            if (audioSource == null && AudioManager.Instance != null)
                audioSource = AudioManager.Instance.AudioSource;

            src = audioSource;
        }

        if (src == null)
        {
            Debug.LogWarning("No AudioSource available to play sound!");
            return;
        }
        if (loop)
    	{
        	// Configure and play looping sound
        	audioSource.clip = sfx;
        	audioSource.loop = true;
        	audioSource.volume = volume;
        	audioSource.Play();
            // audiorandomcontainer
            // pitch by range randomness
    	}
    	else
    	{
        	// Play one-shot sound
        	audioSource.PlayOneShot(sfx, volume);
    	}
    }

    public void StopLoop()
    {
        if (audioSource != null && audioSource.loop)
            audioSource.Stop();
    }

    public void PublishMessage(string message)
    {
        MessageBus.Instance.Publish(message);
    }
}

using UnityEngine;
using System.Collections;

public class MiscObjectClick : MonoBehaviour
{
    private string path = "ScriptableObjects/Dialogues/";
    AudioSource audioSource;

    public Dialogue getDialogue(string name)
    {
        string fullPath = path + name;
        Dialogue dialogue = Resources.Load<Dialogue>(fullPath);
        if (dialogue == null)
        {
            Debug.LogWarning("Dialogue not found: " + path);
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
        if (GameState.Get<bool>("checked_weather") && GameState.Get<bool>("fixed_lighthouse"))
        {
            Debug.Log("Go to next day");
        }
        else
        {
            if (!GameState.Get<bool>("eaten_breakfast"))
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
                // I have work to do!
                DialogueManager.ShowDialogue(getDialogue("Bedroom/my_bed_2"));
            }
        }
    }
    
    public void ClickLighthouseLight()
    {
        if (GameState.Get<bool>("fixed_lighthouse"))
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
		if (GameState.Get<bool>("gathered_fish")) {
        	DialogueManager.ShowDialogue(getDialogue("gather_fish_done"));

		} else 
		{
			GameState.Set("gathered_fish", true);
        	DialogueManager.ShowDialogue(getDialogue("gather_fish"));
		}
	}

    public void ClickSink()
    {
        if (GameState.Get<string>("is_clean") == "true") {
            DialogueManager.ShowDialogue(getDialogue("sink/already_washed"));
        } else 
        {
            DialogueManager.ShowDialogue(getDialogue("sink/prompt_wash_up"));
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

    public void PlaySound(AudioClip sfx, float volume = 1f, bool loop = false)
    {
        if (AudioManager.Instance)
        {
            audioSource = AudioManager.Instance.AudioSource;
        } else {
            Debug.LogWarning("No AudioManager Instance ready yet!");
		}
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found in the scene!");
        }
    	if (sfx == null)
    	{
        	Debug.LogWarning("No AudioClip provided to PlaySound!");
        	return;
    	}
    	if (loop)
    	{
        	// Configure and play looping sound
        	audioSource.clip = sfx;
        	audioSource.loop = true;
        	audioSource.volume = volume;
        	audioSource.Play();
    	}
    	else
    	{
        	// Play one-shot sound
        	audioSource.loop = false;
        	audioSource.PlayOneShot(sfx, volume);
    	}
    }
}

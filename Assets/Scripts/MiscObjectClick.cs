using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // required for SceneManager
using System;

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
            if (GameState.Get<bool>("ready_to_sleep", false))
            {
                DialogueManager.ShowDialogue(getDialogue("Bedroom/co_bed_3"));
            } else if (GameState.Get<bool>("ate_dinner"))
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

        if (GameState.Get<bool>("ready_to_sleep", false))
        {
            DialogueManager.ShowDialogue(getDialogue("Bedroom/uneasy_sleep"));
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

    public void GotoNextDay()
    {
        MessageBus.Instance.Publish("PlayCutscene", "Lighthouse", true, (Action)(() =>
        {
            MessageBus.Instance.Publish("goto_next_day");
        }), true);
        
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
            GameState.Set("near_nighttime", true);
            // GameState.Set("is_nighttime", true);
            
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

    public void ShowDeadCamborneChoices()
    {
        Debug.Log("ShowDeadCamborneChoices");
        MessageBus.Instance.Publish(
            "ShowThreeChoice",
            "LOOK AWAY",
            "FEEL",
            "BURY HIM",
            (Action)(() =>
            {
                // Option 1 Panic
                DialogueManager.ShowDialogue(getDialogue("kitchen/camb_choice1a"));
            }),
            (Action)(() =>
            {
                // Option 2
                DialogueManager.ShowDialogue(getDialogue("kitchen/camb_choice2"));
            }),
            (Action)(() =>
            {
                // Option 3
                CamborneChoicesBuryClick();
            })
        );
    }

    public void CamborneChoicesLookAwayClick()
    {
        Debug.Log("CamborneChoicesLookAwayClick");
        MessageBus.Instance.Publish(
            "ShowTwoChoice",
            "TOUCH",
            "BURY HIM",
            (Action)(() =>
            {
                // Option 2
                DialogueManager.ShowDialogue(getDialogue("kitchen/camb_choice2x"));
            }),
            (Action)(() =>
            {
                // Option 3
                CamborneChoicesBuryClick();

            })
        );
    }

    public void CamborneChoicesTouchClick()
    {
        Debug.Log("CamborneChoicesTouchClick");

        MessageBus.Instance.Publish(
            "ShowTwoChoice",
            "LOOK AWAY",
            "BURY HIM",
            (Action)(() =>
            {
                // Option 2
                DialogueManager.ShowDialogue(getDialogue("kitchen/camb_choice1ax"));
            }),
            (Action)(() =>
            {
                // Option 3
                CamborneChoicesBuryClick();
            })
        );
    }

    public void CamborneChoicesLastClick()
    {
        Debug.Log("last choice");
        MessageBus.Instance.Publish(
            "ShowOneChoice",
            "BURY HIM",
            (Action)(() =>
            {
                // Option 3
                Debug.Log("last choice clicked");
                CamborneChoicesBuryClick();
            })
        );
    }
    
    public void CamborneChoicesBuryClick()
    {
        Debug.Log("bury click pressed");
        DialogueManager.ShowDialogue(getDialogue("kitchen/camb_choice3"));
    }
    
}

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
        if (GameState.Get<bool>("day_transition_started", false))
        {
            return;
        }

        if (GameState.Get<bool>("hand_cut") && !GameState.Get<bool>("hand_cleaned"))
        {
            DialogueManager.ShowDialogueFromText(new string[] { "I should clean my hand first." });
            return;
        }

        int day = GameState.Get<int>("day");
        if (GameState.Get<bool>("ready_to_sleep", false))
        {
            if (day == 1)
            {
                DialogueManager.ShowDialogue(getDialogue("Bedroom/uneasy_sleep"));
            } else if (day == 2)
            {
                DialogueManager.ShowDialogue(getDialogue("Bedroom/sleep_day2"));
            }
        }
        else
        {
            if (!GameState.Get<bool>("ate_breakfast"))
            {
                // Just woke up!
                if (day == 1)
                {
                    DialogueManager.ShowDialogue(getDialogue("Bedroom/my_bed_1"));
                }
                else
                {
                    DialogueManager.ShowDialogue(getDialogue("Bedroom/my_bed_1a"));
                }
            } else if (day == 1)
            {
                // Check up on coworker first
                DialogueManager.ShowDialogue(getDialogue("Bedroom/my_bed_3"));
            }
            else
            {
                if (GameState.Get<bool>("lighthouse_fixed") || GameState.Get<bool>("gathered_fish"))
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
        if (GameState.Get<bool>("day_transition_started", false))
        {
            return;
        }
        GameState.Set("day_transition_started", true);
        GameState.Set("navigationBlocked", true);

        MessageBus.Instance.Publish("ClearAllTasks");
        Debug.Log("current day: " + GameState.Get<int>("day"));
        int newDay = GameState.Get<int>("day") + 1;
        GameState.Set<int>("day", newDay);
        string cutsceneToPlay = "Day2";
        switch (newDay)
        {
            case 2:
                cutsceneToPlay = "Day2";
                break;
            case 3:
                cutsceneToPlay = "Day3";
                break;
            default:
                cutsceneToPlay = "Day2";
                break;
        }
        MessageBus.Instance.Publish("PlayCutscene", cutsceneToPlay, true, (Action)(() =>
        {
            GameState.StartNewDay();
            SaveManager.Save(); // Save progress at the start of the new day
            SceneManager.LoadScene(GameConsts.BEDROOMSCENE);
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

    public void ClickMirror()
    {
        int sanity = GameState.Get<int>("sanity");
        string str = "sink/sanity_low";
        if (sanity > 3)
        {
            str = "sink/sanity_high";
        } else if (sanity > 0)
        {
            str = "sink/sanity_medium";
        } else if (sanity > -3)
        {
            str = "sink/sanity_low";
        }
        else
        {
            str = "sink/sanity_lowest";
        }
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
            if (GameState.Get<int>("day") == 2)
            {
                DialogueManager.ShowDialogueFromText(new string[] { "You don't feel like fish for dinner tonight.", "You let the caught fishes go." });
            }
            else
            {
                DialogueManager.ShowDialogue(getDialogue("dock/gather_fish"));
            }
		}
	}

    public void ClickDock()
    {
        if (GameState.Get<bool>("is_nighttime")) {
            DialogueManager.ShowDialogue(getDialogue("dock/dark_dock"));
        } else {
            DialogueManager.ShowDialogue(getDialogue("dock/empty_dock"));
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

    public void QuestionFishClick()
    {
        MessageBus.Instance.Publish(
            "ShowTwoChoice",
            "YES",
            "NO",
            (Action)(() =>
            {
                // Make sanity go up
                IncreaseSanity();
            }),
            (Action)(() =>
            {
                // Make sanity go down
                DecreaseSanity();
            })
        );
    }

    public void IncreaseSanity()
    {
        Debug.Log("Sanity went up");
        MessageBus.Instance.Publish("FloatText", 0, 0.3f, "+SANITY", "green");
        MessageBus.Instance.Publish("PlusSanity", 1);
    }

    public void DecreaseSanity()
    {
        Debug.Log("Sanity went down");
        MessageBus.Instance.Publish("FloatText", 0, 0.3f, "-SANITY", "purple");
        MessageBus.Instance.Publish("PlusSanity", -1);
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

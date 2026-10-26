using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
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
        // Day 4: she's brought up from the dock and tended to in Camborne's bed
        if (Day4Beach.IsToday && Day4Beach.MermaidRescued)
        {
            if (GameState.Get<bool>(Day4Beach.MermaidTreatedKey, false))
            {
                DialogueManager.ShowDialogueFromText(new string[] { "She's resting." });
                return;
            }

            // First click lays her down (MermaidOnBed shows her there); the next starts tending to her
            if (!GameState.Get<bool>(Day4Beach.MermaidInBedKey, false))
            {
                GameState.Set(Day4Beach.MermaidInBedKey, true);
                MessageBus.Instance.Publish("CompleteTask", "task_mermaid_bed");
                MessageBus.Instance.Publish("AddTaskStringImportant", "generic/task_tend_her");
                DialogueManager.ShowDialogueFromText(new string[]
                {
                    "You lay her down in Camborne's bed.",
                    "She's covered in barnacles.## I have to clean her up."
                });
                return;
            }

            Navigation.GoToScene(GameConsts.MERMAIDSCENE);
            return;
        }

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

        if (Day4Beach.CarryingHer)
        {
            DialogueManager.ShowDialogueFromText(new string[] { "Not my bed.## Camborne's." });
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
            } else if (day == 3)
            {
                Dialogue sleepDialogue = ScriptableObject.CreateInstance<Dialogue>();
                sleepDialogue.text = new List<string>(new string[]
                {
                    "It's been a long day.",
                    "The rain doesn't stop. You don't notice yourself falling asleep."
                });
                sleepDialogue.choices = new List<string>();
                sleepDialogue.consequences = new List<UnityEngine.Events.UnityEvent>();
                sleepDialogue.onDialogueEnd = new UnityEngine.Events.UnityEvent();
                sleepDialogue.onDialogueEndImmediate = new UnityEngine.Events.UnityEvent();

                sleepDialogue.onLineStart = new List<UnityEngine.Events.UnityEvent>();
                UnityEngine.Events.UnityEvent line0Event = new UnityEngine.Events.UnityEvent();
                UnityEngine.Events.UnityEvent line1Event = new UnityEngine.Events.UnityEvent();
                line1Event.AddListener(() =>
                {
                    FadeToBlackAndTransitionDay(4);
                });
                sleepDialogue.onLineStart.Add(line0Event);
                sleepDialogue.onLineStart.Add(line1Event);

                DialogueManager.ShowDialogue(sleepDialogue);
            } else if (day == 4)
            {
                Dialogue sleepDialogue = DialogueManager.ShowDialogueFromText(new string[]
                {
                    "Your arms ache from the digging.",
                    "You can still smell the sea on your hands as you drift off..."
                });
                sleepDialogue.onLineStart = new List<UnityEngine.Events.UnityEvent>
                {
                    new UnityEngine.Events.UnityEvent(),
                    new UnityEngine.Events.UnityEvent()
                };
                sleepDialogue.onLineStart[1].AddListener(() => FadeToBlackAndTransitionDay(5));
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

    private void FadeToBlackAndTransitionDay(int newDay)
    {
        if (GameState.Get<bool>("day_transition_started", false))
        {
            return;
        }
        GameState.Set("day_transition_started", true);
        GameState.Set("navigationBlocked", true);

        if (DialogueManager.instance != null && DialogueManager.instance.gameObject.activeInHierarchy)
        {
            DialogueManager.instance.StartCoroutine(FadeToBlackDayRoutine(newDay));
        }
        else if (Navigation.Instance != null && Navigation.Instance.gameObject.activeInHierarchy)
        {
            Navigation.Instance.StartCoroutine(FadeToBlackDayRoutine(newDay));
        }
        else if (gameObject.activeInHierarchy)
        {
            StartCoroutine(FadeToBlackDayRoutine(newDay));
        }
    }

    private IEnumerator FadeToBlackDayRoutine(int newDay)
    {
        // Fade using Navigation's blackout overlay so dialogue text remains visible on top
        Image blackoutImage = null;
        if (Navigation.Instance != null)
        {
            blackoutImage = Navigation.Instance.GetComponentInChildren<Image>(true);
        }

        float duration = 2.5f;
        float elapsed = 0f;

        if (blackoutImage != null)
        {
            Color c = Color.black;
            c.a = 0f;
            blackoutImage.color = c;
            blackoutImage.gameObject.SetActive(true);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Clamp01(elapsed / duration);
                blackoutImage.color = c;
                yield return null;
            }

            c.a = 1f;
            blackoutImage.color = c;
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }

        yield return new WaitForSeconds(0.4f);

        // Close dialogue box before loading new scene
        DialogueManager.CloseDialogue();

        MessageBus.Instance.Publish("ClearAllTasks");
        GameState.Set<int>("day", newDay);
        GameState.StartNewDay();
        SaveManager.Save();
        SceneManager.LoadScene(GameConsts.BEDROOMSCENE);

        // Wait a frame for scene load
        yield return null;

        // Fade in to reveal the bedroom on the new morning
        float fadeInDuration = 1.0f;
        elapsed = 0f;
        if (blackoutImage != null)
        {
            Color c = Color.black;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Clamp01(1f - (elapsed / fadeInDuration));
                blackoutImage.color = c;
                yield return null;
            }
            c.a = 0f;
            blackoutImage.color = c;
        }

        GameState.Set("navigationBlocked", false);
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

        Action startNewDay = () =>
        {
            GameState.StartNewDay();
            SaveManager.Save(); // Save progress at the start of the new day
            SceneManager.LoadScene(GameConsts.BEDROOMSCENE);
        };

        if (newDay == 4)
        {
            startNewDay();
            return;
        }

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
        // The day has already been bumped, so if the cutscene can't play (no CutsceneManager in
        // this play session, or one already running) go straight to the new day instead of hanging.
        if (CutsceneManager.Instance == null || !CutsceneManager.Instance.PlayCutscene(cutsceneToPlay, true, startNewDay, true))
        {
            startNewDay();
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
		if (Day4Beach.CarryingHer)
		{
			DialogueManager.ShowDialogueFromText(new string[] { "Not with her in my arms." });
			return;
		}
		if (GameState.Get<bool>("gathered_fish", false))
        {
        	DialogueManager.ShowDialogue(getDialogue("dock/gather_fish_done"));

		} else if (!Day4Beach.WorkDone)
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
        MessageBus.Instance.Publish("PlaySound", "wood_creak");
        if (GameState.Get<bool>("is_nighttime")) {
            DialogueManager.ShowDialogue(getDialogue("dock/dark_dock"));
        } else if (GameState.Get<int>("day") >= 4) {
            // The supply ship has wrecked by Day 4
            DialogueManager.ShowDialogueFromText(new string[] { "No more ships are coming." });
        } else if (GameState.Get<int>("day") == 3) {
            DialogueManager.ShowDialogueFromText(new string[] { "Still no boats." });
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

        if (loop && sourceOverride == null)
        {
            // Loops live on AudioManager's own loop source, never the shared one-shot source
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayLoop(sfx, volume);
            else
                Debug.LogWarning("No AudioManager available to play loop!");
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
        	// Configure and play looping sound on the caller's own source
        	src.clip = sfx;
        	src.loop = true;
        	src.volume = volume;
        	src.Play();
    	}
    	else
    	{
        	// Play one-shot sound
        	src.PlayOneShot(sfx, volume);
    	}
    }

    public void StopLoop()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopLoop();
    }

    public void PublishMessage(string message)
    {
        MessageBus.Instance.Publish(message);
    }

    public void ShowDeadCamborneChoices()
    {
        Debug.Log("ShowDeadCamborneChoices");
        MessageBus.Instance.Publish("ShowChoiceDialog", "What do I do now?");
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
        MessageBus.Instance.Publish("ShowChoiceDialog", "What do I do now?");
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
        MessageBus.Instance.Publish("ShowChoiceDialog", "Did the fish just look at me?");
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

        MessageBus.Instance.Publish("ShowChoiceDialog", "What do I do now?");
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
        MessageBus.Instance.Publish("ShowChoiceDialog", "What do I do now?");
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

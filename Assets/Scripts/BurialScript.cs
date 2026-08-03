using UnityEngine;
using System;
using System.Collections;

public class BurialScript : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    [SerializeField] private AudioClip shovelClip;
    [SerializeField] private GameObject shovel;
    [SerializeField] private GameObject black;
    public Dialogue dialogue; // The ScriptableObject

    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Sprite backgroundDugSprite;
    [SerializeField] private Sprite backgroundCoveredSprite;

    [SerializeField] private Sprite backgroundRevealedSpriteDay;
    [SerializeField] private Sprite backgroundCoveredSpriteDay;
    
    [SerializeField] private MiscObjectClick miscObjectClick;
    [SerializeField] private GameObject handBleed;
    private Vector3 initialHandBleedPosition;
    private bool hasSavedHandPos = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    private void OnEnable()
    {
        // Hacky way to change background
        dialogue.onDialogueEnd.AddListener(CoverUpHole);
    }

    private void OnDisable()
    {
        dialogue.onDialogueEnd.RemoveListener(CoverUpHole);
    }
    
    void Start()
    {
        Ambience ambience = Ambience.Instance;

        // Update track 1
        UpdateTrack(ambience, bgLoop1, 0.65f, 1);
        // Update track 2
        UpdateTrack(ambience, bgLoop2, 0.3f, 2);

        if (handBleed != null)
        {
            initialHandBleedPosition = handBleed.transform.position;
            hasSavedHandPos = true;
        }

        if (handBleed != null)
        {
            handBleed.SetActive(GameState.Get<bool>("hand_cut"));
        }

        bool isDay2 = GameState.Get<int>("day") == 2;

        // Day 2: Rain has uncovered the grave
        if (isDay2 && GameState.Get<bool>("has_buried") && !GameState.Get<bool>("grave_inspected"))
        {
            Destroy(shovel);
            background.sprite = backgroundRevealedSpriteDay;
            GameState.Set("grave_revealed", true);
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("burial/grave_uncovered"));
            MessageBus.Instance.Publish("AddTaskStringImportant", "generic/fix_grave");
            return;
        }

        if (GameState.Get<bool>("has_dug"))
        {
            Destroy(shovel);
            if (GameState.Get<bool>("has_buried"))
            {
                background.sprite = backgroundCoveredSprite;
                MessageBus.Instance.Publish("BurialMoundCovered");
            }
            else
            {
                background.sprite = backgroundDugSprite;
            }
        }
    }

    void LateUpdate()
    {
        if (handBleed != null && handBleed.activeSelf && Camera.main != null)
        {
            if (!hasSavedHandPos)
            {
                initialHandBleedPosition = handBleed.transform.position;
                hasSavedHandPos = true;
            }
            Vector3 camPos = Camera.main.transform.position;
            handBleed.transform.position = initialHandBleedPosition + new Vector3(camPos.x * 0.6f, camPos.y * 0.6f, 0f);
        }
    }
    
    private IEnumerator PlaySoundDelayedRoutine(AudioClip sfx, float volume, bool loop, float delay)
    {
        yield return new WaitForSeconds(delay);
        miscObjectClick.PlaySound(sfx, volume, loop);
    }

    private IEnumerator InjuryFlash()
    {
        if (black == null) yield break;
        SpriteRenderer sr = black.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        black.SetActive(true);
        sr.color = new Color(1f, 0f, 0f, 0f);

        // Fade in red
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            Color c = sr.color;
            c.a = Mathf.Lerp(0f, 0.6f, t / 0.15f);
            sr.color = c;
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        // Fade out
        t = 0f;
        while (t < 0.35f)
        {
            t += Time.deltaTime;
            Color c = sr.color;
            c.a = Mathf.Lerp(0.6f, 0f, t / 0.35f);
            sr.color = c;
            yield return null;
        }

        sr.color = new Color(1f, 1f, 1f, 0f);
        black.SetActive(false);
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
        // Day 2: Re-bury after rain uncovered the grave
        if (GameState.Get<bool>("grave_revealed") && !GameState.Get<bool>("grave_inspected"))
        {
            GameState.Set("grave_revealed", false);
            GameState.Set("grave_inspected", true);
            GameState.Set("near_nighttime", true);

            black.SetActive(true);
            StartCoroutine(PlaySoundDelayedRoutine(shovelClip, 0.6f, false, 0.5f));
            FadeTo(black, 1, 1.2f, () =>
            {
                if (background != null && backgroundCoveredSprite != null)
                {
                    background.sprite = backgroundCoveredSprite;
                }
                FadeTo(black, 1, 1.75f, () =>
                {
                    FadeTo(black, 0, 0.85f, () =>
                    {
                        black.SetActive(false);
                        Dialogue original = miscObjectClick.getDialogue("burial/rebury");
                        Dialogue d = Instantiate(original);
                        d.onDialogueEnd.AddListener(() =>
                        {
                            GameState.Set("hand_cut", true);
                            MessageBus.Instance.Publish("FloatText", 0f, 0.3f, "-SANITY", "purple");
                            MessageBus.Instance.Publish("PlusSanity", -1);
                            MessageBus.Instance.Publish("AddTaskString", "generic/task_wash_hand");
                            MessageBus.Instance.Publish("CompleteTask", "generic/fix_grave");
                            if (handBleed != null)
                            {
                                handBleed.SetActive(true);
                            }
                            MessageBus.Instance.Publish("PlaySound", "injury");
                            StartCoroutine(InjuryFlash());
                        });
                        DialogueManager.ShowDialogue(d);
                    });
                });
            });
            return;
        }

        if (!GameState.Get<bool>("has_dug"))
        {
            // Haven't dug up the grave yet, do that.
            GameState.Set("has_dug", true);

            black.SetActive(true);
			StartCoroutine(PlaySoundDelayedRoutine(shovelClip, 0.6f, false, 0.5f));
			FadeTo(black, 1, 1.2f, () => {
            	Destroy(shovel);
            	if (background != null && backgroundDugSprite != null)
            	{
                	background.sprite = backgroundDugSprite;
            	}
				FadeTo(black, 1, 1.75f, () => {
					FadeTo(black, 0, 0.85f, () => {
						black.SetActive(false);
					});

				});
			});
            return;
        }
        
        if (!GameState.Get<bool>("has_buried"))
        {
            // There's a hole now
            GameState.Set("has_buried", true);
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("burial/finished_burying"));

            return;
        }
        if (GameState.Get<int>("day") == 2)
        {
            DialogueManager.ShowDialogueFromText(new string[] { "Giving me trouble even after death." });
        }
        else
        {
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue("burial/already_buried"));
        }
    }

    public void CoverUpHole()
    {
        if (background != null && backgroundCoveredSprite != null)
        {
            background.sprite = backgroundCoveredSprite;
            MessageBus.Instance.Publish("CompleteTask", "bury_body");
            // May need to be changed with multibury
            GameState.Set("ready_to_sleep", true);
            GameState.Set("do_burial", false);
            MessageBus.Instance.Publish("BurialMoundCovered");
        }
    }

	private void FadeTo(GameObject target, float alpha = 1, float duration = 2, Action onComplete = null) {
		StartCoroutine(FadeInRoutine(target, alpha, duration, onComplete));
	}

	private IEnumerator FadeInRoutine(GameObject target, float endAlpha, float duration, Action onComplete) {
		SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
		if (sr == null)
		{
			Debug.LogWarning("Missing sprite renderer");
			yield break;
		}

		float elapsed = 0f;
		Color color = sr.color;
		float startAlpha = color.a;
		while (elapsed < duration) {
			elapsed += Time.deltaTime;
			float t = elapsed / duration;
			color.a = Mathf.Lerp(startAlpha, endAlpha, t);
			sr.color = color;
			yield return null;
		}
		// Ensure ends at full alpha
		color.a = endAlpha;
		sr.color = color;

        onComplete?.Invoke();
	}

}

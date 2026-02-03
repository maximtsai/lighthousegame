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
    [SerializeField] private Sprite backgroundRevealedSprite;
    
    [SerializeField] private MiscObjectClick miscObjectClick;
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
            MessageBus.Instance.Publish("AddTaskString", "generic/go_to_bed");
            MessageBus.Instance.Publish("CompleteTask", "bury_body");

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
        DialogueManager.ShowDialogue(miscObjectClick.getDialogue("burial/already_buried"));
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

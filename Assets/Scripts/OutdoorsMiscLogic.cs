using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OutdoorsMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    // [SerializeField] CutsceneManager CsManager;
    [SerializeField] private MiscObjectClick miscObjectClick;

    [Header("Thunderstorm")]
    [SerializeField] private AudioClip rainLoop;

    [Header("Weather UI (Optional)")]
    [SerializeField] private Button weatherActionButton;

    [Header("Day 3 Lighthouse Flicker")]
    [SerializeField] private SpriteRenderer background;
    // Night background with the lights out, lined up 1:1 with the night background frames.
    [SerializeField] private Sprite blackoutSprite;

    private const string Day3FlickerSeenKey = "day3_flicker_seen";

    [Header("Day 4 Beach")]
    // Placeholder wreckage until the washed-up pile art is in: fishing junk sprites.
    [SerializeField] private Sprite[] washedUpPileSprites;

    void Start()
    {
        UpdateAmbience();

        Dialogue nearlyDark = null;
        if (GameState.Get<bool>("near_nighttime"))
        {
            GameState.Set("near_nighttime", false);
            if (!GameState.Get<bool>("is_nighttime"))
            {
                GameState.Set("is_nighttime", true);
                nearlyDark = Instantiate(miscObjectClick.getDialogue("nearly_dark"));
                DialogueManager.ShowDialogue(nearlyDark);
            }
        }

        if (Day4Beach.IsToday)
        {
            gameObject.AddComponent<Day4Beach>().Init(washedUpPileSprites);
        }

        // Day 3: the lighthouse hasn't been right since the scissors broke.
        if (GameState.Get<int>("day") == 3 && GameState.Get<bool>("is_nighttime"))
        {
            StartLighthouseFlicker();
            if (!GameState.Get<bool>(Day3FlickerSeenKey, false))
            {
                if (nearlyDark != null)
                    nearlyDark.onDialogueEnd.AddListener(ShowFlickerDialogue);
                else
                    ShowFlickerDialogue();
            }
        }

        // Initialize Weather Button
        // if (weatherActionButton != null)
        // {
        //     weatherActionButton.onClick.AddListener(OnWeatherButtonClicked);
        // }
    }



    private void ShowFlickerDialogue()
    {
        GameState.Set(Day3FlickerSeenKey, true);
        DialogueManager.ShowDialogueFromText(new string[]
        {
            "The lighthouse is flickering.",
            "Eh.## Good enough."
        });
    }

    private void StartLighthouseFlicker()
    {
        if (background == null || blackoutSprite == null)
        {
            Debug.LogWarning("OutdoorsMiscLogic: background or blackoutSprite not assigned for the day 3 flicker");
            return;
        }

        // Child of the background so it pans and scales with it. Same sorting order, just in
        // front of it, so the rain and everything else in front of the background stays in front.
        GameObject blackoutObject = new GameObject("day3_blackout");
        blackoutObject.transform.SetParent(background.transform, false);
        blackoutObject.transform.localPosition = new Vector3(0f, 0f, -0.05f);
        SpriteRenderer blackout = blackoutObject.AddComponent<SpriteRenderer>();
        blackout.sprite = blackoutSprite;
        blackout.sharedMaterial = background.sharedMaterial;
        blackout.sortingLayerID = background.sortingLayerID;
        blackout.sortingOrder = background.sortingOrder;
        blackout.enabled = false;

        StartCoroutine(FlickerRoutine(blackout));
    }

    private IEnumerator FlickerRoutine(SpriteRenderer blackout)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1.5f, 3f));

            // Stutter off, back on for a moment, then out for a bit longer.
            blackout.enabled = true;
            yield return new WaitForSeconds(0.08f);
            blackout.enabled = false;
            yield return new WaitForSeconds(0.06f);
            blackout.enabled = true;
            yield return new WaitForSeconds(Random.Range(0.4f, 0.9f));
            blackout.enabled = false;
        }
    }

    public void UpdateAmbience()
    {
        Ambience ambience = Ambience.Instance;
        bool raining = GameState.Get<bool>("thunderstorm", false);

        // Update track 1
        UpdateTrack(ambience, bgLoop1, 0.9f, 1);
        // Update track 2
        UpdateTrack(ambience, raining && rainLoop != null ? rainLoop : bgLoop2, 0.35f, 2);
    }

    private void UpdateTrack(Ambience ambience, AudioClip newClip, float volume, int channel)
    {
        // Check if the new clip is different from the current clip
        if (ambience == null)
        {
            return;
        }
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
}

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

    [Header("Lighthouse Light")]
    [SerializeField] private SpriteRenderer background;
    // The light, separate from the day background and lined up 1:1 with its frames. Only on once
    // the lighthouse has been fixed. The night background still has the light painted in.
    [SerializeField] private Sprite[] lighthouseLightSprites;
    [SerializeField] private float lighthouseLightFrameRate = 4f;

    [Header("Day 3 Lighthouse Flicker")]
    // Night background with the lights out, lined up 1:1 with the night background frames. Only
    // needed at night: by day the separate light is just switched off instead.
    [SerializeField] private Sprite blackoutSprite;

    [Header("Day 5 Flower Field")]
    // Click area over the meadow that leads into the flower minigame, in world units. It sits
    // behind the scene's own clickables (z -1) and the treasure, so it only catches open grass.
    [SerializeField] private Vector2 day5FieldCenter = new Vector2(-0.6f, -1.0f);
    [SerializeField] private Vector2 day5FieldSize = new Vector2(6f, 1.3f);

    private SpriteRenderer lighthouseLight;

    private const string Day3FlickerSeenKey = "day3_flicker_seen";

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

        bool lit = GameState.Get<bool>("lighthouse_fixed", false);
        if (lit && !GameState.Get<bool>("is_nighttime"))
            CreateLighthouseLight();

        // Day 3: the lighthouse hasn't been right since the scissors broke.
        if (GameState.Get<int>("day") == 3 && lit && lighthouseLight != null)
        {
            StartCoroutine(FlickerRoutine(off => lighthouseLight.enabled = !off));
        }
        else if (GameState.Get<int>("day") == 3 && GameState.Get<bool>("is_nighttime"))
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

        if (Day5.IsToday)
        {
            SpawnFlowerField();
            if (!GameState.Get<bool>(Day5.WorkReplacedKey, false))
                ReplaceWorkWithFlowers(nearlyDark);
        }

        // Initialize Weather Button
        // if (weatherActionButton != null)
        // {
        //     weatherActionButton.onClick.AddListener(OnWeatherButtonClicked);
        // }
    }



    // Day 5: the first time he steps out, the day's work is crossed off for something better
    private void ReplaceWorkWithFlowers(Dialogue showAfter)
    {
        GameState.Set(Day5.WorkReplacedKey, true);
        MessageBus.Instance.Publish("CompleteTask", "task_work");
        MessageBus.Instance.Publish("AddTaskBefore", "generic/task_flowers", "task_fish");

        System.Action line = () => DialogueManager.ShowDialogueFromText(new string[]
        {
            "The lighthouse can wait.",
            "She'd like some flowers."
        });
        if (showAfter != null)
            showAfter.onDialogueEnd.AddListener(() => line());
        else
            line();
    }

    private void SpawnFlowerField()
    {
        GameObject field = new GameObject("day5_flower_field");
        field.transform.position = new Vector3(day5FieldCenter.x, day5FieldCenter.y, -0.1f);
        BoxCollider2D box = field.AddComponent<BoxCollider2D>();
        box.size = day5FieldSize;
        field.AddComponent<ClickTarget>().onClick = ClickFlowerField;
    }

    private void ClickFlowerField()
    {
        if (Day5.FlowersPicked)
            DialogueManager.ShowDialogueFromText(new string[] { "That's enough flowers for today." });
        else if (!Day5.Treated)
            DialogueManager.ShowDialogueFromText(new string[] { Day5.TendHerFirstLine });
        else
            Navigation.GoToScene(GameConsts.FLOWERSCENE);
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

    private void CreateLighthouseLight()
    {
        if (background == null || lighthouseLightSprites == null || lighthouseLightSprites.Length == 0)
        {
            Debug.LogWarning("OutdoorsMiscLogic: background or lighthouseLightSprites not assigned");
            return;
        }

        // Child of the background so it pans and scales with it, just in front of it like the blackout.
        GameObject lightObject = new GameObject("lighthouse_light");
        lightObject.transform.SetParent(background.transform, false);
        lightObject.transform.localPosition = new Vector3(0f, 0f, -0.05f);
        lighthouseLight = lightObject.AddComponent<SpriteRenderer>();
        lighthouseLight.sprite = lighthouseLightSprites[0];
        lighthouseLight.sharedMaterial = background.sharedMaterial;
        lighthouseLight.sortingLayerID = background.sortingLayerID;
        lighthouseLight.sortingOrder = background.sortingOrder;

        if (lighthouseLightSprites.Length > 1)
            StartCoroutine(AnimateLighthouseLight());
    }

    private IEnumerator AnimateLighthouseLight()
    {
        float frameTime = 1f / Mathf.Max(1f, lighthouseLightFrameRate);
        int frame = 0;
        while (lighthouseLight != null)
        {
            yield return new WaitForSeconds(frameTime);
            frame = (frame + 1) % lighthouseLightSprites.Length;
            lighthouseLight.sprite = lighthouseLightSprites[frame];
        }
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

        StartCoroutine(FlickerRoutine(off => blackout.enabled = off));
    }

    // setLightOff(true) puts the light out: hides the separate light by day, shows the blackout at night.
    private IEnumerator FlickerRoutine(System.Action<bool> setLightOff)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1.5f, 3f));

            // Stutter off, back on for a moment, then out for a bit longer.
            setLightOff(true);
            yield return new WaitForSeconds(0.08f);
            setLightOff(false);
            yield return new WaitForSeconds(0.06f);
            setLightOff(true);
            yield return new WaitForSeconds(Random.Range(0.4f, 0.9f));
            setLightOff(false);
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

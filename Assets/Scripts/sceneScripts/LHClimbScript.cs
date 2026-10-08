using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LHClimbScript : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    [SerializeField] private AudioClip finishLoop;
    [SerializeField] private MiscObjectClick miscObjectClick;
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Sprite[] upSprites;
    [SerializeField] private Sprite[] downSprites;
    [SerializeField] private AudioClip brickFallSound;
    [SerializeField] private AudioClip brickPlaceSound;
    [SerializeField] private AudioClip brickFootstepSound;
    private readonly Dictionary<InteractableObject, AudioClip> defaultFootstepSounds = new Dictionary<InteractableObject, AudioClip>();
    private AudioClip activeFootstepsSound;
    // Day 3: the view into the hole on floor 4 where the missing scissors are stuck.
    [SerializeField] private GameObject scissorsCloseUp;
    // Edit Mode only, for laying out a screen. Play Mode uses the live climb state instead.
    [SerializeField, Range(1, 4)] private int previewFloor = 1;
    [SerializeField] private bool previewGoingUp = true;
    [SerializeField] private bool previewBrickOnFloor = true;
    [SerializeField] private bool previewScissorsStuck = true;
    [SerializeField] private bool previewScissorsCloseUp = false;

    void Start()
    {
        Ambience ambience = Ambience.Instance;

        UpdateTrack(ambience, bgLoop1, 0.4f, 1);
        UpdateTrack(ambience, bgLoop2, 0.2f, 2);
        if (GameState.Get<bool>("lighthouse_fixed"))
        {
            StartCoroutine(PlaySoundDelayedRoutine(finishLoop, 0.4f, true, 0.01f));
        }

        // Nothing leaves the close-up but its own fade, so a fresh load never starts in it.
        LighthouseScissors.SetCloseUpOpen(false);

        WireBricks();
        WireScissors();
        ShowFloor(LighthouseClimb.Floor, LighthouseClimb.GoingUp);
    }

    // Every brick outline does the same thing, so they share one listener here rather than
    // each carrying its own on_click.
    private void WireBricks()
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in transforms)
        {
            if (t.gameObject.scene != gameObject.scene || !t.name.EndsWith("_BrickHighlight"))
                continue;

            InteractableObject interactable = t.GetComponent<InteractableObject>();
            if (interactable != null)
                interactable.AddClickListener(Navigation.PlaceBrickInWall);
        }
    }

    private void WireScissors()
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in transforms)
        {
            if (t.gameObject.scene != gameObject.scene || !t.name.EndsWith("_ScissorsSparkle"))
                continue;

            InteractableObject interactable = t.GetComponent<InteractableObject>();
            if (interactable != null)
                interactable.AddClickListener(Navigation.OpenScissorsCloseUp);
        }
    }

    public float FootstepsDuration => ClipLength(activeFootstepsSound);
    public float BrickDropDuration => ClipLength(brickFallSound);

    public void PlayBrickDrop()
    {
        PlayBrickSound(brickFallSound, 1f);
    }

    private static float ClipLength(AudioClip clip)
    {
        return clip != null ? clip.length : 0f;
    }

    public void PlayBrickPlace()
    {
        PlayBrickSound(brickPlaceSound, 0.6f);
    }

    public float BrickPlaceFadeOutDuration()
    {
        return 0.5f;
    }

    public float BrickPlaceHoldDuration()
    {
        return Mathf.Max(0f, ClipLength(brickPlaceSound) - BrickPlaceFadeOutDuration());
    }

    private void PlayBrickSound(AudioClip clip, float volume)
    {
        if (clip == null || miscObjectClick == null)
            return;

        miscObjectClick.PlaySound(clip, volume);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying)
            return;

        UnityEditor.EditorApplication.delayCall -= ApplyPreview;
        UnityEditor.EditorApplication.delayCall += ApplyPreview;
    }

    void ApplyPreview()
    {
        if (this == null || Application.isPlaying)
            return;

        ShowFloor(previewFloor, previewGoingUp);
    }
#endif

    public void ShowFloor(int floor, bool goingUp)
    {
        if (background == null)
        {
            GameObject bgObject = GameObject.Find("background");
            if (bgObject != null)
                background = bgObject.GetComponent<SpriteRenderer>();
        }

        int index = Mathf.Clamp(floor, 1, LighthouseClimb.FloorCount) - 1;
        Sprite[] sprites = goingUp ? upSprites : downSprites;
        if (background != null && sprites != null && index < sprites.Length && sprites[index] != null)
            background.sprite = sprites[index];

        string screen = (goingUp ? "up" : "down") + floor + "_";
        bool brickInWall = BrickInWall();
        bool scissorsStuck = ScissorsStuck();

        // The close-up covers the whole screen, so the screen's own objects go away while it's
        // up and can't be clicked through it.
        bool closeUp = ScissorsCloseUpOpen() && floor == LighthouseScissors.Floor && goingUp;
        if (scissorsCloseUp != null)
            scissorsCloseUp.SetActive(closeUp);

        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in transforms)
        {
            if (t.gameObject.scene != gameObject.scene)
                continue;

            if (!IsScreenObject(t.name))
                continue;

            bool onThisScreen = t.name.StartsWith(screen) && !closeUp;

            // Either view of floor 3 can lead up to the brick. Use one step only when it will fall.
            if (t.name == "up3_UpArrow" || t.name == "down3_UpArrow")
                UpdateBrickFootstepSound(t.GetComponent<InteractableObject>(), onThisScreen);

            // Only one brick shows at a time. The outline is a child of the loose one and
            // follows it, so it needs no case of its own.
            if (t.name.EndsWith("_BrickInWall"))
                t.gameObject.SetActive(onThisScreen && brickInWall);
            else if (t.name.EndsWith("_Brick"))
                t.gameObject.SetActive(onThisScreen && !brickInWall);
            // The sparkle sits in the hole the brick leaves.
            else if (t.name.EndsWith("_ScissorsSparkle"))
                t.gameObject.SetActive(onThisScreen && scissorsStuck && !brickInWall);
            else
                t.gameObject.SetActive(onThisScreen);
        }
    }

    private void UpdateBrickFootstepSound(InteractableObject arrow, bool onThisScreen)
    {
#if UNITY_EDITOR
        // Scene previews must keep the saved default sounds intact.
        if (!Application.isPlaying)
            return;
#endif
        if (arrow == null)
            return;

        if (!defaultFootstepSounds.TryGetValue(arrow, out AudioClip defaultSound))
        {
            defaultSound = arrow.clickSound;
            defaultFootstepSounds.Add(arrow, defaultSound);
        }

        arrow.clickSound = brickFootstepSound != null && LighthouseBrick.WillFallOnArrival(LighthouseBrick.Floor, true)
            ? brickFootstepSound : defaultSound;
        if (onThisScreen)
            activeFootstepsSound = arrow.clickSound;
    }

    private bool BrickInWall()
    {
#if UNITY_EDITOR
        // No game state in Edit Mode, and the loose brick has to show to be positioned.
        if (!Application.isPlaying)
            return !previewBrickOnFloor;
#endif
        return LighthouseBrick.InWall;
    }

    private bool ScissorsStuck()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return previewScissorsStuck;
#endif
        return LighthouseScissors.Stuck;
    }

    private bool ScissorsCloseUpOpen()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return previewScissorsCloseUp;
#endif
        return LighthouseScissors.CloseUpOpen;
    }

    // Anything named "up3_DownArrow", "down4_Brick" and so on belongs to a single climb screen.
    private static bool IsScreenObject(string name)
    {
        for (int f = 1; f <= LighthouseClimb.FloorCount; f++)
        {
            if (name.StartsWith("up" + f + "_") || name.StartsWith("down" + f + "_"))
                return true;
        }

        return false;
    }

    private IEnumerator PlaySoundDelayedRoutine(AudioClip sfx, float volume, bool loop, float delay)
    {
        yield return new WaitForSeconds(delay);
        miscObjectClick.PlaySound(sfx, volume, loop);
    }

    private void UpdateTrack(Ambience ambience, AudioClip newClip, float volume, int channel)
    {
        // Null when Play Mode is entered straight from this scene instead of MainScene.
        if (ambience == null)
            return;

        AudioClip currentClip = ambience.GetCurrentClip(channel);
        if (currentClip != newClip)
        {
            ambience.PlayTrack(newClip, volume, channel);
        }
        else
        {
            ambience.SetVolume(channel, volume);
        }
    }
}

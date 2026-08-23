using UnityEngine;
using System.Collections;

public class LHClimbScript : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    [SerializeField] private AudioClip finishLoop;
    [SerializeField] private MiscObjectClick miscObjectClick;
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Sprite[] upSprites;
    [SerializeField] private Sprite[] downSprites;
    // Not recorded yet, so the brick stays silent until these are filled in.
    [SerializeField] private AudioClip brickFallSound;
    [SerializeField] private AudioClip brickSettleSound;
    [SerializeField] private AudioClip brickPlaceSound;
    // Edit Mode only, for laying out a screen. Play Mode uses the live climb state instead.
    [SerializeField, Range(1, 4)] private int previewFloor = 1;
    [SerializeField] private bool previewGoingUp = true;
    [SerializeField] private bool previewBrickOnFloor = true;

    void Start()
    {
        Ambience ambience = Ambience.Instance;

        UpdateTrack(ambience, bgLoop1, 0.4f, 1);
        UpdateTrack(ambience, bgLoop2, 0.2f, 2);
        if (GameState.Get<bool>("lighthouse_fixed"))
        {
            StartCoroutine(PlaySoundDelayedRoutine(finishLoop, 0.4f, true, 0.01f));
        }

        WireBricks();
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

    public void PlayBrickArrival(LighthouseBrick.Arrival arrival)
    {
        PlayBrickSound(arrival == LighthouseBrick.Arrival.Fell ? brickFallSound : brickSettleSound);
    }

    public void PlayBrickPlace()
    {
        PlayBrickSound(brickPlaceSound);
    }

    private void PlayBrickSound(AudioClip clip)
    {
        if (clip == null || miscObjectClick == null)
            return;

        miscObjectClick.PlaySound(clip, 0.6f);
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

        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in transforms)
        {
            if (t.gameObject.scene != gameObject.scene)
                continue;

            if (!IsScreenObject(t.name))
                continue;

            bool onThisScreen = t.name.StartsWith(screen);

            // Only one brick shows at a time. The outline is a child of the loose one and
            // follows it, so it needs no case of its own.
            if (t.name.EndsWith("_BrickInWall"))
                t.gameObject.SetActive(onThisScreen && brickInWall);
            else if (t.name.EndsWith("_Brick"))
                t.gameObject.SetActive(onThisScreen && !brickInWall);
            else
                t.gameObject.SetActive(onThisScreen);
        }
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

using System.Collections;
using UnityEngine;

// A sea worm squirming on the body. The gloves pop it: the worm is swapped for a splat that plays
// once from where it was, sits on the finished mess for a moment, then fades away. As it bursts,
// a big copy of the splat hits the screen too.
//
// To feel alive, each worm wriggles at its own speed, now and then going still or into a sudden
// frenzy. It thrashes while you hover it with the gloves, and flinches when another worm bursts.
//
// The squirm frames are registered 640x1500 layers like the rest of the body art, so the worm
// sits at local (0,0). The splat frames are small, so the splat renderer is its own child object
// placed over the worm.
public class MermaidWorm : MermaidTarget
{
    [Header("Squirm")]
    [SerializeField] private Sprite[] squirmFrames;
    [Tooltip("Each worm has its own speed; the small ones are quicker.")]
    [SerializeField] private float squirmFps = 10f;
    [Tooltip("Frame to start on, so the worms don't all wriggle in step.")]
    [SerializeField] private int startFrame;
    [Tooltip("Seconds of ordinary wriggling between going still and a frenzy.")]
    [SerializeField] private Vector2 calmTime = new Vector2(1.5f, 4f);
    [Tooltip("Seconds it goes still for.")]
    [SerializeField] private Vector2 stillTime = new Vector2(0.3f, 1f);
    [Tooltip("Seconds a frenzy lasts.")]
    [SerializeField] private Vector2 frenzyTime = new Vector2(0.4f, 0.8f);
    [Tooltip("How much faster it wriggles in a frenzy.")]
    [SerializeField] private float frenzySpeed = 2f;

    [Header("Reactions")]
    [Tooltip("How much faster it wriggles while you hover it with the gloves.")]
    [SerializeField] private float thrashSpeed = 2.5f;
    [Tooltip("How much it shrinks back when another worm bursts, as a fraction of its size.")]
    [SerializeField] private float flinchAmount = 0.08f;
    [SerializeField] private float flinchDuration = 0.35f;

    [Header("Splat")]
    [SerializeField] private SpriteRenderer splatRenderer;
    [SerializeField] private Sprite[] splatFrames;
    [SerializeField] private float splatFps = 14f;
    [Tooltip("How long the finished splat stays before it fades.")]
    [SerializeField] private float splatHold = 0.6f;
    [SerializeField] private float splatFade = 0.4f;

    [Header("Screen Splat")]
    [Tooltip("Optional. Where the burst gets sprayed across the screen.")]
    [SerializeField] private MermaidScreenSplat screenSplat;
    [Tooltip("The splat frame where the worm bursts. The screen gets the frames from here on, and " +
             "the other worms flinch.")]
    [SerializeField] private int sprayFrame = 6;

    [Header("Progress")]
    [SerializeField] private bool popped;

    // Every worm hears about a burst, so the others can flinch.
    private static event System.Action<MermaidWorm> Burst;

    private enum Mood { Calm, Still, Frenzy }

    private Mood mood;
    private float moodLeft;
    private float framePosition;
    private bool thrashing;
    private float flinchDelay;
    private float flinchLeft;
    private bool splatting;
    private Vector3 restPosition;
    private Vector2 middle;

    public override bool IsBusy => splatting;

    void Awake()
    {
        restPosition = transform.localPosition;
        middle = OutlineMiddle();

        // Out of step with the others from the very first frame.
        moodLeft = Random.Range(calmTime.x, calmTime.y);
    }

    void OnEnable()
    {
        Burst += OnOtherBurst;
    }

    void OnDisable()
    {
        Burst -= OnOtherBurst;
    }

    void Update()
    {
        if (popped || spriteRenderer == null || squirmFrames == null || squirmFrames.Length == 0) return;

        UpdateMood();
        float speed = thrashing ? thrashSpeed : mood == Mood.Still ? 0f : mood == Mood.Frenzy ? frenzySpeed : 1f;
        framePosition += Time.deltaTime * squirmFps * speed;
        spriteRenderer.sprite = squirmFrames[(startFrame + (int)framePosition) % squirmFrames.Length];

        float size = 1f;
        if (flinchDelay > 0f)
        {
            flinchDelay -= Time.deltaTime;
        }
        else if (flinchLeft > 0f)
        {
            flinchLeft -= Time.deltaTime;
            float t = 1f - Mathf.Clamp01(flinchLeft / Mathf.Max(0.01f, flinchDuration));
            size = 1f - flinchAmount * Mathf.Sin(t * Mathf.PI);
        }

        Pose(size);
    }

    // Ordinary wriggling, broken up now and then by going still or a sudden frenzy.
    private void UpdateMood()
    {
        moodLeft -= Time.deltaTime;
        if (moodLeft > 0f) return;

        if (mood != Mood.Calm)
        {
            mood = Mood.Calm;
            moodLeft = Random.Range(calmTime.x, calmTime.y);
        }
        else if (Random.value < 0.5f)
        {
            mood = Mood.Still;
            moodLeft = Random.Range(stillTime.x, stillTime.y);
        }
        else
        {
            mood = Mood.Frenzy;
            moodLeft = Random.Range(frenzyTime.x, frenzyTime.y);
        }
    }

    // A hair apart from each other, the rest recoil and then wriggle harder for a moment.
    private void OnOtherBurst(MermaidWorm other)
    {
        if (other == this || popped) return;

        flinchDelay = Random.Range(0f, 0.1f);
        flinchLeft = flinchDuration;
        mood = Mood.Frenzy;
        moodLeft = Random.Range(frenzyTime.x, frenzyTime.y);
    }

    // The worm art is a full 640x1500 layer, so its transform sits at the middle of the body. To
    // flinch in place it scales about the middle of its own outline, which means shifting as it scales.
    private void Pose(float size)
    {
        transform.localScale = new Vector3(size, size, 1f);
        transform.localPosition = restPosition + (Vector3)(middle * (1f - size));
    }

    // Hovering it with the gloves only sets it thrashing, like it knows what's coming. There's no
    // tint or shake, so the art stays exactly as drawn and the frantic wriggling is the only sign.
    protected override void SetHover(bool on)
    {
        thrashing = on;
    }

    public override bool CanApply(MermaidTool tool)
    {
        return tool == MermaidTool.Gloves && !popped;
    }

    public override void Apply(MermaidTool tool)
    {
        popped = true;
        Pose(1f);
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        StartCoroutine(Splat());
    }

    public override int RemainingSteps(MermaidTool tool)
    {
        return tool == MermaidTool.Gloves && !popped ? 1 : 0;
    }

    private IEnumerator Splat()
    {
        if (splatRenderer == null || splatFrames == null || splatFrames.Length == 0) yield break;

        splatting = true;
        splatRenderer.color = Color.white;
        splatRenderer.enabled = true;

        float frameTime = 1f / Mathf.Max(1f, splatFps);
        for (int i = 0; i < splatFrames.Length; i++)
        {
            splatRenderer.sprite = splatFrames[i];
            if (i == sprayFrame)
            {
                if (screenSplat != null) screenSplat.Spray(splatFrames, sprayFrame);
                Burst?.Invoke(this);
            }
            yield return new WaitForSeconds(frameTime);
        }

        // The pop itself is done, so the phase can move on while the mess lingers a little.
        splatting = false;

        yield return new WaitForSeconds(splatHold);

        float elapsed = 0f;
        while (elapsed < splatFade)
        {
            elapsed += Time.deltaTime;
            splatRenderer.color = new Color(1f, 1f, 1f, 1f - Mathf.Clamp01(elapsed / splatFade));
            yield return null;
        }

        splatRenderer.enabled = false;
    }
}

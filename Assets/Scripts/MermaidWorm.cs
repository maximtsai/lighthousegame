using System.Collections;
using System.Collections.Generic;
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
    [Tooltip("Optional second stage for a large worm. It takes another click to burst.")]
    [SerializeField] private Sprite[] halfFrames;
    [Tooltip("Saved, inactive worms released by the final burst.")]
    [SerializeField] private MermaidWorm[] releasedWorms;
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
    [Tooltip("Use a small body splat when this worm releases smaller worms.")]
    [SerializeField] private bool softReleaseSplat;
    [SerializeField, HideInInspector] private Vector2[] landingSplatCenters;
    [SerializeField, HideInInspector] private Vector2[] squirmCenters;
    [Tooltip("Seconds the little landing splat stays visible before fading.")]
    [SerializeField] private float landingSplatHold = 0.35f;
    [SerializeField] private float landingSplatFade = 0.35f;

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
    private bool scattering;
    private Vector3 restPosition;
    private Vector2 middle;
    private Vector2 originalMiddle;
    private bool half;
    private Vector3 splatRestPosition;
    private Vector3 splatRestScale;
    private bool landingSplat;
    private int landingSplatFrame;
    private Sprite shownSquirmFrame;
    private float poseSize = -1f;

    public override bool IsBusy => splatting || scattering;

    void Awake()
    {
        restPosition = transform.localPosition;
        middle = OutlineMiddle();
        originalMiddle = middle;
        if (splatRenderer != null)
        {
            splatRestPosition = splatRenderer.transform.localPosition;
            splatRestScale = splatRenderer.transform.localScale;
        }

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
        landingSplat = false;
    }

    void LateUpdate()
    {
        if (landingSplat) AlignLandingSplat();
    }

    private void AlignLandingSplat()
    {
        if (splatRenderer == null || spriteRenderer == null ||
            landingSplatCenters == null || landingSplatCenters.Length != splatFrames.Length) return;

        int wormFrame = System.Array.IndexOf(squirmFrames, spriteRenderer.sprite);
        Vector2 centre = squirmCenters != null && wormFrame >= 0 && wormFrame < squirmCenters.Length
            ? squirmCenters[wormFrame] : middle;
        Vector2 splatCentre = landingSplatCenters[landingSplatFrame];
        if (spriteRenderer.flipX) centre.x = -centre.x;
        if (spriteRenderer.flipY) centre.y = -centre.y;
        if (splatRenderer.flipX) splatCentre.x = -splatCentre.x;
        if (splatRenderer.flipY) splatCentre.y = -splatCentre.y;

        // Transparent margins and the visible centre change between animation frames.
        Vector3 offset = splatRenderer.transform.localRotation *
            Vector3.Scale(splatCentre, splatRenderer.transform.localScale);
        splatRenderer.transform.localPosition = new Vector3(centre.x, centre.y, splatRestPosition.z) - offset;
    }

    void Update()
    {
        if (popped || splatting || scattering || spriteRenderer == null || squirmFrames == null || squirmFrames.Length == 0) return;

        UpdateMood();
        float speed = thrashing ? thrashSpeed : mood == Mood.Still ? 0f : mood == Mood.Frenzy ? frenzySpeed : 1f;
        framePosition += Time.deltaTime * squirmFps * speed;
        Sprite[] frames = half ? halfFrames : squirmFrames;
        Sprite frame = frames[(startFrame + (int)framePosition) % frames.Length];
        if (frame != shownSquirmFrame)
        {
            spriteRenderer.sprite = frame;
            shownSquirmFrame = frame;
        }

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
        if (other == this || popped || scattering) return;

        flinchDelay = Random.Range(0f, 0.1f);
        flinchLeft = flinchDuration;
        mood = Mood.Frenzy;
        moodLeft = Random.Range(frenzyTime.x, frenzyTime.y);
    }

    // The worm art is a full 640x1500 layer, so its transform sits at the middle of the body. To
    // flinch in place it scales about the middle of its own outline, which means shifting as it scales.
    private void Pose(float size)
    {
        if (poseSize == size) return;
        poseSize = size;
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
        StopAllCoroutines();
        landingSplat = false;
        // A landing splat may still be fading when the player squishes this worm.
        if (splatRenderer != null)
        {
            splatRenderer.transform.localPosition = splatRestPosition;
            splatRenderer.transform.localScale = splatRestScale;
        }
        if (!half && halfFrames != null && halfFrames.Length > 0) half = true;
        else popped = true;
        if (popped && half && splatRenderer != null)
            splatRenderer.transform.localPosition += (Vector3)(middle - originalMiddle);
        poseSize = -1f;
        Pose(1f);
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = !popped;
            if (!popped) spriteRenderer.sprite = halfFrames[0];
        }
        StartCoroutine(Splat());
    }

    public override int RemainingSteps(MermaidTool tool)
    {
        return tool == MermaidTool.Gloves && !popped
            ? (!half && halfFrames != null && halfFrames.Length > 0 ? 2 : 1) : 0;
    }

    private IEnumerator Splat(bool landing = false)
    {
        if (splatRenderer == null || splatFrames == null || splatFrames.Length == 0) yield break;

        if (!landing) splatting = true;
        bool softBurst = landing || (softReleaseSplat && popped && releasedWorms != null && releasedWorms.Length > 0);
        Vector3 splatScale = splatRenderer.transform.localScale;
        Vector3 splatPosition = splatRenderer.transform.localPosition;
        if (softBurst)
        {
            // Keep the visible splat centred as its full sprite canvas shrinks.
            Vector3 centre = (Vector3)middle;
            centre.z = splatPosition.z;
            splatRenderer.transform.localPosition = centre + (splatPosition - centre) * 0.65f;
            splatRenderer.transform.localScale = splatScale * 0.65f;
        }
        splatRenderer.color = Color.white;
        splatRenderer.enabled = true;
        landingSplat = landing;

        float frameTime = 1f / Mathf.Max(1f, landing ? 20f : softBurst ? 28f : splatFps);
        var frameWait = new WaitForSeconds(frameTime);
        // Landing needs the spray, rather than the worm-shaped squish windup.
        int firstFrame = landing ? Mathf.Clamp(sprayFrame, 0, splatFrames.Length - 1) : 0;
        for (int i = firstFrame; i < splatFrames.Length; i++)
        {
            splatRenderer.sprite = splatFrames[i];
            if (landing)
            {
                landingSplatFrame = i;
                AlignLandingSplat();
            }
            if (!landing && i == (softBurst ? 0 : sprayFrame))
            {
                if (screenSplat != null) screenSplat.Spray(splatFrames, sprayFrame);
                Burst?.Invoke(this);
                if (popped && releasedWorms != null)
                    for (int index = 0; index < releasedWorms.Length; index++)
                    {
                        MermaidWorm worm = releasedWorms[index];
                        if (worm != null) worm.Reveal(transform.TransformPoint(middle), index);
                    }
            }
            yield return frameWait;
        }

        // The pop itself is done, so the phase can move on while the mess lingers a little.
        if (!landing && !popped && spriteRenderer != null)
        {
            spriteRenderer.sprite = half ? halfFrames[0] : squirmFrames[0];
            spriteRenderer.enabled = true;
            if (half && bodyCollider != null)
            {
                Sprite frame = halfFrames[0];
                int count = frame.GetPhysicsShapeCount();
                if (count > 0)
                {
                    bodyCollider.pathCount = count;
                    var points = new List<Vector2>();
                    for (int path = 0; path < count; path++)
                    {
                        frame.GetPhysicsShape(path, points);
                        bodyCollider.SetPath(path, points);
                    }
                    middle = OutlineMiddle();
                }
            }
        }
        if (!landing) splatting = false;

        if (landing) yield return new WaitForSeconds(landingSplatHold);
        else if (!softBurst) yield return new WaitForSeconds(splatHold);

        float fadeDuration = landing ? landingSplatFade : softBurst ? 0.22f : splatFade;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            splatRenderer.color = new Color(1f, 1f, 1f, 1f - Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }

        splatRenderer.enabled = false;
        landingSplat = false;
        splatRenderer.transform.localScale = splatScale;
        splatRenderer.transform.localPosition = splatPosition;
    }

    private void Reveal(Vector3 origin, int index)
    {
        gameObject.SetActive(true);
        scattering = true;
        if (bodyCollider != null) bodyCollider.enabled = false;
        if (splatRenderer != null) splatRenderer.enabled = false;
        StartCoroutine(Scatter(origin, index));
    }

    private IEnumerator Scatter(Vector3 origin, int index)
    {
        // Full-body sprite canvases need their visible centre moved, not their pivot.
        Vector3 start = transform.parent != null ? transform.parent.InverseTransformPoint(origin) : origin;
        Vector3 end = restPosition + (Vector3)middle;
        start.z = end.z;
        int order = spriteRenderer != null ? spriteRenderer.sortingOrder : 0;
        transform.localScale = Vector3.one;
        transform.localPosition = start - (Vector3)middle;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sortingOrder = 175 + index;
        }

        const float duration = 0.09f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // A simultaneous, straight burst: no stagger, growth, or drifting arc.
            Vector3 centre = Vector3.Lerp(start, end, t);
            transform.localPosition = centre - (Vector3)middle;
            elapsed += Time.deltaTime;
            yield return null;
        }

        poseSize = -1f;
        Pose(1f);
        if (spriteRenderer != null) spriteRenderer.sortingOrder = order;
        scattering = false;
        if (bodyCollider != null) bodyCollider.enabled = true;
        StartCoroutine(Splat(true));
    }
}

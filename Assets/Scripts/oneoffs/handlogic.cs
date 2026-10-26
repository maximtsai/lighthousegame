using System.Collections;
using UnityEngine;

public class handlogic : MonoBehaviour
{
    [Header("Cleaned Sprites")]
    [SerializeField] private Sprite handGash;
    [SerializeField] private Sprite nightGash;

    [Header("Bleeding Sprites")]
    [SerializeField] private Sprite nightHandBleed;

    [Header("Bandaged Sprites")]
    [SerializeField] private Sprite handBandaged;
    [SerializeField] private Sprite nightHandBandaged;
    [SerializeField] private Sprite handBandagedBloody;
    [SerializeField] private Sprite nightHandBandagedBloody;

    [Header("Fester Sprites")]
    [SerializeField] private Sprite handFester1;
    [SerializeField] private Sprite nightFester1;
    [SerializeField] private Sprite handFester2;
    [SerializeField] private Sprite nightFester2;

    [Header("Flash Sprite")]
    [SerializeField] private Sprite hand_flash;

    [Header("Scratch")]
    [SerializeField] private GameObject handScratch;
    [SerializeField] private AudioClip scratchSound;

    [Header("Screen Flash")]
    [SerializeField] private GameObject blackScreenFlash;

    private Vector3 initialPosition;
    private bool hasSavedPos = false;
    private Sprite originalSprite;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // Set up before the early return: the hand can be cut mid-scene (Day 2 burial) and
        // switched back on, and Awake won't run again then
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }

        // If the hand is not cut, deactivate this game object immediately
        if (!GameState.Get<bool>("hand_cut", false))
        {
            gameObject.SetActive(false);
            return;
        }

        initialPosition = transform.position;
        hasSavedPos = true;
    }

    void Update()
    {
        UpdateSprite();
        UpdateCarryingHer();
    }

    // Day 4: his arms are full carrying her up from the dock, so the hand stays off screen until
    // she's laid down. Only undoes its own hiding, so others (Day4Dock's carries) can hide it too.
    private bool hiddenForCarry;

    private void UpdateCarryingHer()
    {
        if (spriteRenderer == null) return;

        if (Day4Beach.CarryingHer)
        {
            spriteRenderer.enabled = false;
            hiddenForCarry = true;
        }
        else if (hiddenForCarry)
        {
            spriteRenderer.enabled = true;
            hiddenForCarry = false;
        }
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            if (!hasSavedPos)
            {
                initialPosition = transform.position;
                hasSavedPos = true;
            }
            Vector3 camPos = Camera.main.transform.position;
            transform.position = initialPosition + new Vector3(camPos.x * 0.6f, camPos.y * 0.6f, 0f);

            // The parallax lifts the hand when the camera pans down. The wrist runs off the sprite's
            // bottom edge, so keep that edge at or below the bottom of the screen or the hand floats.
            if (spriteRenderer != null && Camera.main.orthographic)
            {
                float screenBottom = camPos.y - Camera.main.orthographicSize;
                float gap = spriteRenderer.bounds.min.y - screenBottom;
                if (gap > 0f)
                {
                    transform.position -= new Vector3(0f, gap, 0f);
                }
            }
        }
    }

    public void Scratch()
    {
        if (handScratch == null) return;
        PlayScratchSound();
        StartCoroutine(ScratchRoutine());
    }

    public void ScratchInterrupted()
    {
        if (handScratch == null) return;
        PlayScratchSound();
        StartCoroutine(ScratchInterruptedRoutine());
    }

    private void PlayScratchSound()
    {
        if (scratchSound == null) return;
        MessageBus.Instance.Publish("PlaySound", scratchSound);
    }

    private IEnumerator ScratchInterruptedRoutine()
    {
        if (handScratch == null) yield break;

        SpriteRenderer scratchSpriteRenderer = handScratch.GetComponent<SpriteRenderer>();
        Sprite originalScratchSprite = scratchSpriteRenderer != null ? scratchSpriteRenderer.sprite : null;

        // Enable and move to position x=0.6, y=0, z=0.1 over 0.3s cubic ease-out
        handScratch.SetActive(true);
        Transform scratchTransform = handScratch.transform;
        Vector3 startPos = scratchTransform.localPosition;
        Vector3 targetPos = new Vector3(0.6f, 0f, 0.1f);
        yield return MoveLocal(scratchTransform, startPos, targetPos, 0.3f, true);

        // Scratch down once
        Vector3 downPos = new Vector3(targetPos.x, -0.7f, targetPos.z);
        yield return MoveLocal(scratchTransform, targetPos, downPos, 0.2f, true);

        // Hand sprite briefly turns red
        if (scratchSpriteRenderer != null)
        {
            scratchSpriteRenderer.color = Color.red;
        }
        yield return new WaitForSeconds(0.1f);

        // Hand turns to hand_flash sprite (no longer red)
        if (scratchSpriteRenderer != null)
        {
            scratchSpriteRenderer.color = Color.white;
            scratchSpriteRenderer.sprite = hand_flash;
        }
        yield return new WaitForSeconds(0.1f);

        // Screen turns black briefly
        if (blackScreenFlash != null)
        {
            blackScreenFlash.SetActive(true);
        }
        yield return new WaitForSeconds(0.15f);
        if (blackScreenFlash != null)
        {
            blackScreenFlash.SetActive(false);
        }

        // Return hand to starting spot and hide
        yield return MoveLocal(scratchTransform, downPos, startPos, 0.2f, false);
        handScratch.SetActive(false);

        // Reset scratch sprite back to original
        if (scratchSpriteRenderer != null)
        {
            scratchSpriteRenderer.sprite = originalScratchSprite;
        }

        // Hand sprite turns to fester sprite
        if (spriteRenderer != null)
        {
            Sprite festerSprite = GameState.Get<bool>("is_nighttime", false) ? nightFester1 : handFester1;
            if (festerSprite != null)
            {
                spriteRenderer.sprite = festerSprite;
            }
        }
    }

    private IEnumerator ScratchRoutine()
    {
        if (handScratch == null) yield break;

        // Enable and move to position x=0.6, y=0, z=0.1 over 0.3s cubic ease-out
        handScratch.SetActive(true);
        Transform scratchTransform = handScratch.transform;
        Vector3 startPos = scratchTransform.localPosition;
        Vector3 targetPos = new Vector3(0.6f, 0f, 0.1f);
        yield return MoveLocal(scratchTransform, startPos, targetPos, 0.3f, true);

        // Scratch animation: down to y=-0.7, back to y=0, then repeat
        int repeatCount = 3;
        Vector3 downPos = new Vector3(targetPos.x, -0.7f, targetPos.z);
        for (int i = 0; i < repeatCount; i++)
        {
            yield return MoveLocal(scratchTransform, targetPos, downPos, 0.2f, true);
            yield return MoveLocal(scratchTransform, downPos, targetPos, 0.2f, false);
        }

        // Move away and disable
        Vector3 awayPos = new Vector3(-1.7f, -1.9f, targetPos.z);
        yield return MoveLocal(scratchTransform, targetPos, awayPos, 0.3f, true);
        handScratch.SetActive(false);
    }

    private IEnumerator MoveLocal(Transform target, Vector3 from, Vector3 to, float duration, bool easeOut)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float tEased = easeOut ? 1f - Mathf.Pow(1f - t, 3f) : t * t * t;
            target.localPosition = Vector3.Lerp(from, to, tEased);
            yield return null;
        }
        target.localPosition = to;
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        bool isNight = GameState.Get<bool>("is_nighttime", false);

        if (GameState.Get<bool>("hand_oozing", false))
        {
            // Day 4 onward: pus oozing out
            Sprite targetOozeSprite = isNight ? nightFester2 : handFester2;
            if (targetOozeSprite != null && spriteRenderer.sprite != targetOozeSprite)
            {
                spriteRenderer.sprite = targetOozeSprite;
            }
        }
        else if (GameState.Get<bool>("handInfested", false))
        {
            Sprite targetFesterSprite = isNight ? nightFester1 : handFester1;
            if (targetFesterSprite != null && spriteRenderer.sprite != targetFesterSprite)
            {
                spriteRenderer.sprite = targetFesterSprite;
            }
        }
        else if (GameState.Get<bool>("hand_cleaned", false))
        {
            Sprite targetCleanSprite = isNight ? nightHandBandaged : handBandaged;
            // Fallback to gash sprites if bandaged sprites are not assigned
            if (targetCleanSprite == null)
            {
                targetCleanSprite = isNight ? nightGash : handGash;
            }

            if (targetCleanSprite != null && spriteRenderer.sprite != targetCleanSprite)
            {
                spriteRenderer.sprite = targetCleanSprite;
            }
        }
        else
        {
            Sprite targetBleedSprite = isNight ? nightHandBleed : originalSprite;
            if (targetBleedSprite != null && spriteRenderer.sprite != targetBleedSprite)
            {
                spriteRenderer.sprite = targetBleedSprite;
            }
        }
    }
}

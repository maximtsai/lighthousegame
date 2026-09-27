using System.Collections;
using UnityEngine;

// Worm goo on the screen. When a worm bursts, a big copy of its splat lands somewhere across the
// view, hangs there for a moment, then slides down and fades away.
//
// This sits in front of everything and follows the camera, so a splat stays put on screen while
// the view pans underneath it. It has no colliders, so clicks go straight through to the body.
public class MermaidScreenSplat : MonoBehaviour
{
    [Tooltip("Used in turn, so a few worms popped in a row can all be on screen at once.")]
    [SerializeField] private SpriteRenderer[] slots;
    [Tooltip("Whole numbers keep the pixel art crisp.")]
    [SerializeField] private float scale = 3f;
    [Tooltip("How far from the middle of the view a splat can land, in world units.")]
    [SerializeField] private Vector2 spread = new Vector2(1.6f, 0.7f);
    [SerializeField] private float framesPerSecond = 14f;
    [Tooltip("How long it stays on screen once the burst has finished.")]
    [SerializeField] private float hold = 0.8f;
    [SerializeField] private float fadeDuration = 0.8f;
    [Tooltip("How far it slides down while it fades.")]
    [SerializeField] private float drip = 0.35f;

    private Coroutine[] playing;
    private int next;
    private Camera cam;

    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        Vector3 c = cam.transform.position;
        transform.position = new Vector3(c.x, c.y, transform.position.z);
    }

    // Plays the frames from fromFrame on. If every slot is busy, the oldest splat makes way.
    public void Spray(Sprite[] frames, int fromFrame)
    {
        if (slots == null || slots.Length == 0 || frames == null || fromFrame >= frames.Length) return;
        if (playing == null || playing.Length != slots.Length) playing = new Coroutine[slots.Length];

        int slot = next;
        next = (next + 1) % slots.Length;
        if (playing[slot] != null) StopCoroutine(playing[slot]);
        playing[slot] = StartCoroutine(Play(slots[slot], frames, Mathf.Max(0, fromFrame)));
    }

    private IEnumerator Play(SpriteRenderer slot, Sprite[] frames, int fromFrame)
    {
        Transform t = slot.transform;
        Vector3 landing = new Vector3(Random.Range(-spread.x, spread.x), Random.Range(-spread.y, spread.y), 0f);
        t.localPosition = landing;
        t.localScale = new Vector3(scale, scale, 1f);
        slot.flipX = Random.value < 0.5f;
        slot.color = Color.white;
        slot.enabled = true;

        float frameTime = 1f / Mathf.Max(1f, framesPerSecond);
        for (int i = fromFrame; i < frames.Length; i++)
        {
            slot.sprite = frames[i];
            yield return new WaitForSeconds(frameTime);
        }

        yield return new WaitForSeconds(hold);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / fadeDuration);
            t.localPosition = landing + Vector3.down * (drip * k * k);
            slot.color = new Color(1f, 1f, 1f, 1f - k);
            yield return null;
        }

        slot.enabled = false;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Plays the mermaid_alcohol frames with the stream landing where you clicked. The pour is pinned to
// that spot on her body, so if the view scrolls while it plays, it scrolls with her instead of
// following the cursor.
//
// The bottle tips in from the right and pours down to the left; when there isn't room for it on the
// right it comes in from the left instead. It never turns upside down, so near the top of the view
// the top of the bottle runs off the screen, but the stream still lands on the spot.
public class MermaidAlcoholPour : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("The bottle tipping in and starting to pour: the first ten frames of the sheet.")]
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 14f;
    [Tooltip("Where the stream ends on the last frame, relative to the frame centre. This lands on the spot you clicked.")]
    [SerializeField] private Vector2 pourHit = new Vector2(-0.27f, 0.56f);
    [Tooltip("Left and right edges of the tipped bottle, relative to the frame centre, before flipping. " +
             "Used to pick the side that has room for it.")]
    [SerializeField] private float bottleLeft = -0.67f;
    [SerializeField] private float bottleRight = 1.87f;
    [SerializeField] private float screenPadding = 0.12f;
    [Tooltip("The last frame fades out instead of vanishing.")]
    [SerializeField] private float fadeOutDuration = 0.15f;

    private Coroutine playRoutine;
    private Camera cam;
    private bool flipX;

    void Awake()
    {
        cam = Camera.main;
        Hide();
    }

    public void Play()
    {
        if (frames == null || frames.Length == 0 || spriteRenderer == null) return;

        if (playRoutine != null) StopCoroutine(playRoutine);

        // Placed once, where the click landed, and left there in the world. It's snapped to the
        // body's pixel grid so its pixels sit exactly on hers instead of between them.
        Vector3 spot = CursorWorld();
        flipX = ChooseFlip(spot);
        spriteRenderer.flipX = flipX;
        float hitX = flipX ? -pourHit.x : pourHit.x;
        float ppu = frames[0] != null ? frames[0].pixelsPerUnit : 100f;
        transform.position = new Vector3(SnapToPixel(spot.x - hitX, ppu), SnapToPixel(spot.y - pourHit.y, ppu), -0.4f);

        playRoutine = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        spriteRenderer.color = Color.white;
        spriteRenderer.enabled = true;

        float frameTime = 1f / Mathf.Max(1f, framesPerSecond);
        for (int i = 0; i < frames.Length; i++)
        {
            spriteRenderer.sprite = frames[i];
            yield return new WaitForSeconds(frameTime);
        }

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f - Mathf.Clamp01(elapsed / fadeOutDuration));
            yield return null;
        }

        Hide();
        playRoutine = null;
    }

    // The authored side, unless the bottle would run off the edge there and has more room on the other.
    private bool ChooseFlip(Vector3 spot)
    {
        float authored = SideOverflow(spot, false);
        return authored > 0f && SideOverflow(spot, true) < authored;
    }

    private float SideOverflow(Vector3 spot, bool useFlip)
    {
        float originX = spot.x - (useFlip ? -pourHit.x : pourHit.x);
        GetBottleSpan(useFlip, out float left, out float right);
        Rect view = CameraWorldRect();

        return Mathf.Max(0f, view.xMin - (originX + left)) + Mathf.Max(0f, (originX + right) - view.xMax);
    }

    private static float SnapToPixel(float value, float pixelsPerUnit)
    {
        return Mathf.Round(value * pixelsPerUnit) / pixelsPerUnit;
    }

    private void GetBottleSpan(bool useFlip, out float left, out float right)
    {
        left = useFlip ? -bottleRight : bottleLeft;
        right = useFlip ? -bottleLeft : bottleRight;
    }

    private Vector3 CursorWorld()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return transform.position;

        Vector2 screen = Pointer.current != null
            ? Pointer.current.position.ReadValue()
            : (Vector2)Input.mousePosition;

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
        world.z = -0.4f;
        return world;
    }

    private Rect CameraWorldRect()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return new Rect(-3.2f, -1.8f, 6.4f, 3.6f);

        Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0f, 0f, -cam.transform.position.z));
        Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1f, 1f, -cam.transform.position.z));
        return Rect.MinMaxRect(
            Mathf.Min(bl.x, tr.x) + screenPadding,
            Mathf.Min(bl.y, tr.y) + screenPadding,
            Mathf.Max(bl.x, tr.x) - screenPadding,
            Mathf.Max(bl.y, tr.y) - screenPadding);
    }

    private void Hide()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
            spriteRenderer.flipX = false;
            spriteRenderer.color = Color.white;
        }
    }
}

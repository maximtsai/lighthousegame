using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Plays the mermaid_alcohol sheet so the stream lands on the cursor and the bottle
// stays on screen. The unflipped art sits to the right and above the pour; we flip
// X and/or Y when that would send the bottle off the camera.
public class MermaidAlcoholPour : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 14f;
    [Tooltip("Sprite-local point the liquid hits, relative to the frame center.")]
    [SerializeField] private Vector2 pourHit = new Vector2(-0.85f, 0.15f);
    [Tooltip("Unflipped bottle AABB in sprite-local space, relative to the frame center.")]
    [SerializeField] private Vector2 bottleMin = new Vector2(-0.2f, -1.2f);
    [SerializeField] private Vector2 bottleMax = new Vector2(2.4f, 2.0f);
    [SerializeField] private float screenPadding = 0.12f;

    private Coroutine playRoutine;
    private Camera cam;
    private bool flipX;
    private bool flipY;

    void Awake()
    {
        cam = Camera.main;
        Hide();
    }

    public void Play()
    {
        if (frames == null || frames.Length == 0 || spriteRenderer == null) return;

        if (playRoutine != null) StopCoroutine(playRoutine);
        ChooseOrientation(CursorWorld());
        ApplyFlip();
        playRoutine = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        spriteRenderer.enabled = true;
        FollowCursor();

        float frameTime = 1f / Mathf.Max(1f, framesPerSecond);
        for (int i = 0; i < frames.Length; i++)
        {
            spriteRenderer.sprite = frames[i];
            FollowCursor();
            yield return new WaitForSeconds(frameTime);
        }

        Hide();
        playRoutine = null;
    }

    void LateUpdate()
    {
        if (playRoutine != null) FollowCursor();
    }

    private void ChooseOrientation(Vector3 cursor)
    {
        // Prefer the authored angle, then horizontal flip, then vertical, then both.
        bool[] xs = { false, true, false, true };
        bool[] ys = { false, false, true, true };

        int best = 0;
        float bestOverflow = float.MaxValue;
        for (int i = 0; i < 4; i++)
        {
            float overflow = BottleOverflow(cursor, xs[i], ys[i]);
            if (overflow < bestOverflow - 0.0001f)
            {
                bestOverflow = overflow;
                best = i;
                if (overflow <= 0f) break;
            }
        }

        flipX = xs[best];
        flipY = ys[best];
    }

    private void ApplyFlip()
    {
        spriteRenderer.flipX = flipX;
        spriteRenderer.flipY = flipY;
    }

    private void FollowCursor()
    {
        Vector3 cursor = CursorWorld();
        Vector2 hit = Flip(pourHit, flipX, flipY);
        Vector3 pos = cursor - (Vector3)hit;
        pos.z = -0.4f;
        transform.position = ClampBottleOnScreen(pos);
    }

    private Vector3 ClampBottleOnScreen(Vector3 pos)
    {
        Rect view = CameraWorldRect();
        GetFlippedBottle(flipX, flipY, out Vector2 localMin, out Vector2 localMax);

        float minX = view.xMin - localMin.x;
        float maxX = view.xMax - localMax.x;
        float minY = view.yMin - localMin.y;
        float maxY = view.yMax - localMax.y;

        if (minX > maxX) pos.x = (minX + maxX) * 0.5f;
        else pos.x = Mathf.Clamp(pos.x, minX, maxX);

        if (minY > maxY) pos.y = (minY + maxY) * 0.5f;
        else pos.y = Mathf.Clamp(pos.y, minY, maxY);

        return pos;
    }

    private float BottleOverflow(Vector3 cursor, bool useFlipX, bool useFlipY)
    {
        Vector2 hit = Flip(pourHit, useFlipX, useFlipY);
        Vector3 origin = cursor - (Vector3)hit;
        GetFlippedBottle(useFlipX, useFlipY, out Vector2 localMin, out Vector2 localMax);
        Rect view = CameraWorldRect();

        float overflow = 0f;
        overflow += Mathf.Max(0f, view.xMin - (origin.x + localMin.x));
        overflow += Mathf.Max(0f, (origin.x + localMax.x) - view.xMax);
        overflow += Mathf.Max(0f, view.yMin - (origin.y + localMin.y));
        overflow += Mathf.Max(0f, (origin.y + localMax.y) - view.yMax);
        return overflow;
    }

    private static void GetFlippedBottle(bool useFlipX, bool useFlipY, Vector2 min, Vector2 max, out Vector2 localMin, out Vector2 localMax)
    {
        float x0 = useFlipX ? -max.x : min.x;
        float x1 = useFlipX ? -min.x : max.x;
        float y0 = useFlipY ? -max.y : min.y;
        float y1 = useFlipY ? -min.y : max.y;
        localMin = new Vector2(Mathf.Min(x0, x1), Mathf.Min(y0, y1));
        localMax = new Vector2(Mathf.Max(x0, x1), Mathf.Max(y0, y1));
    }

    private void GetFlippedBottle(bool useFlipX, bool useFlipY, out Vector2 localMin, out Vector2 localMax)
    {
        GetFlippedBottle(useFlipX, useFlipY, bottleMin, bottleMax, out localMin, out localMax);
    }

    private static Vector2 Flip(Vector2 value, bool useFlipX, bool useFlipY)
    {
        if (useFlipX) value.x = -value.x;
        if (useFlipY) value.y = -value.y;
        return value;
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
            spriteRenderer.flipY = false;
        }
    }
}

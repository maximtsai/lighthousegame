using UnityEngine;

/// <summary>
/// One of the dock's "float dock" sheets (bodies, the mermaid, driftwood) playing on top of the
/// dock background. Each frame is the full 660x400 dock frame with only the floating thing
/// painted in, so a layer sits at the background's origin and lines up with it exactly, bobbing
/// along with the water.
/// </summary>
public class FloatingLayer : MonoBehaviour
{
    private Sprite[] frames;
    private float frameTime;
    private float timer;
    private int frame;
    private SpriteRenderer spriteRenderer;

    public SpriteRenderer Renderer => spriteRenderer;

    // Child of the background so it pans and scales with it, just in front of it.
    public static FloatingLayer Spawn(string name, SpriteRenderer background, Sprite[] frames, float fps = 12f)
    {
        if (background == null || frames == null || frames.Length == 0)
        {
            Debug.LogWarning("FloatingLayer: background or frames missing for " + name);
            return null;
        }

        GameObject layerObject = new GameObject(name);
        layerObject.transform.SetParent(background.transform, false);
        layerObject.transform.localPosition = new Vector3(0f, 0f, -0.05f);

        FloatingLayer layer = layerObject.AddComponent<FloatingLayer>();
        layer.frames = frames;
        layer.frameTime = 1f / Mathf.Max(1f, fps);
        layer.spriteRenderer = layerObject.AddComponent<SpriteRenderer>();
        layer.spriteRenderer.sprite = frames[0];
        layer.spriteRenderer.sharedMaterial = background.sharedMaterial;
        layer.spriteRenderer.sortingLayerID = background.sortingLayerID;
        layer.spriteRenderer.sortingOrder = background.sortingOrder;
        return layer;
    }

    // A click box around the floating thing, given in pixels from the frame's top-left corner.
    public BoxCollider2D AddClickBox(RectInt pixels)
    {
        Sprite first = frames[0];
        float ppu = first.pixelsPerUnit;
        Vector2 frameSize = first.rect.size;

        BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
        box.size = new Vector2(pixels.width, pixels.height) / ppu;
        box.offset = new Vector2(
            (pixels.x + pixels.width * 0.5f - frameSize.x * 0.5f) / ppu,
            (frameSize.y * 0.5f - (pixels.y + pixels.height * 0.5f)) / ppu);
        return box;
    }

    void Update()
    {
        if (frames.Length < 2) return;

        timer += Time.deltaTime;
        while (timer >= frameTime)
        {
            timer -= frameTime;
            frame = (frame + 1) % frames.Length;
            spriteRenderer.sprite = frames[frame];
        }
    }
}

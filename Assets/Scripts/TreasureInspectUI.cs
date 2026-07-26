using UnityEngine;

/// <summary>
/// Dimmed inspect view that shows a treasure at its real SpriteRenderer size
/// (scale 1,1,1 — same as dragging the sprite into a scene). No UI size box.
/// Dim + item track the main camera every frame so pans stay covered.
/// Works with Single-mode sprites (fileID 21300000).
/// </summary>
public class TreasureInspectUI : MonoBehaviour
{
    private const int DimSortOrder = 9000;
    private const int ItemSortOrder = 9001;
    // How far in front of the camera (local +Z) the overlay sits.
    private const float CameraForward = 1f;

    private static TreasureInspectUI instance;

    private SpriteRenderer dimRenderer;
    private SpriteRenderer itemRenderer;
    private Transform dimTransform;
    private Transform itemTransform;

    public static void Show(Sprite sprite)
    {
        // Drop any leftover UI-canvas version from earlier iterations.
        if (instance != null && instance.itemRenderer == null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }

        EnsureInstance();

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("TreasureInspectUI: no Main Camera.");
            return;
        }

        if (sprite == null)
            Debug.LogWarning("TreasureInspectUI: treasure sprite is null (check Single sprite reference).");

        instance.itemRenderer.sprite = sprite;
        instance.itemRenderer.color = Color.white;
        instance.itemTransform.localScale = Vector3.one;
        instance.itemTransform.gameObject.SetActive(sprite != null);

        instance.gameObject.SetActive(true);
        instance.SyncToCamera(cam);
    }

    public static void Hide()
    {
        if (instance != null)
            instance.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        SyncToCamera(cam);
    }

    private void SyncToCamera(Camera cam)
    {
        Vector3 camPos = cam.transform.position;
        // Closer to camera than the scene (camera looks +Z).
        float itemZ = camPos.z + CameraForward;
        float dimZ = itemZ + 0.1f; // slightly behind the item

        dimTransform.position = new Vector3(camPos.x, camPos.y, dimZ);
        if (cam.orthographic)
        {
            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;
            dimTransform.localScale = new Vector3(width * 1.05f, height * 1.05f, 1f);
        }
        else
        {
            dimTransform.localScale = new Vector3(40f, 40f, 1f);
        }

        if (!itemTransform.gameObject.activeSelf || itemRenderer.sprite == null)
            return;

        // Center the sprite on the camera (works for Single sprites with center pivot).
        itemTransform.position = new Vector3(camPos.x, camPos.y, itemZ);
        itemTransform.localScale = Vector3.one;

        Bounds bounds = itemRenderer.bounds;
        Vector3 delta = new Vector3(camPos.x, camPos.y, bounds.center.z) - bounds.center;
        delta.z = 0f;
        itemTransform.position += delta;
    }

    private static void EnsureInstance()
    {
        if (instance != null) return;

        GameObject root = new GameObject("TreasureInspectUI");
        DontDestroyOnLoad(root);
        instance = root.AddComponent<TreasureInspectUI>();
        instance.Build(root);
        root.SetActive(false);
    }

    private void Build(GameObject root)
    {
        GameObject dimGo = new GameObject("Dim");
        dimGo.transform.SetParent(root.transform, false);
        dimTransform = dimGo.transform;
        dimRenderer = dimGo.AddComponent<SpriteRenderer>();
        dimRenderer.sprite = CreateUnitWhiteSprite();
        dimRenderer.color = new Color(0f, 0f, 0f, 0.75f);
        dimRenderer.sortingOrder = DimSortOrder;

        GameObject itemGo = new GameObject("Item");
        itemGo.transform.SetParent(root.transform, false);
        itemTransform = itemGo.transform;
        itemRenderer = itemGo.AddComponent<SpriteRenderer>();
        itemRenderer.sortingOrder = ItemSortOrder;
    }

    private static Sprite CreateUnitWhiteSprite()
    {
        Texture2D tex = Texture2D.whiteTexture;
        return Sprite.Create(
            tex,
            new Rect(0f, 0f, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            tex.width
        );
    }
}

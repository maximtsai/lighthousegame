using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Dimmed inspect view at real SpriteRenderer size (scale 1,1,1).
/// Supports single sprites and two-part reveals (closed → hover outline → click → fade → open).
/// </summary>
public class TreasureInspectUI : MonoBehaviour
{
    private const int DimSortOrder = 9000;
    private const int ItemSortOrder = 9001;
    private const int OutlineSortOrder = 9002;
    private const int FadeSortOrder = 9100;
    private const float CameraForward = 1f;
    private const float RevealFadeDuration = 0.15f;

    private static TreasureInspectUI instance;

    private SpriteRenderer dimRenderer;
    private SpriteRenderer itemRenderer;
    private SpriteRenderer outlineRenderer;
    private SpriteRenderer fadeRenderer;
    private Transform dimTransform;
    private Transform itemTransform;
    private Transform outlineTransform;
    private Transform fadeTransform;

    private Sprite closedSprite;
    private Sprite hoverOutlineSprite;
    private Sprite revealSprite;
    private Action onRevealed;
    private bool awaitingReveal;
    private bool revealing;
    private bool outlineHovered;
    private float itemYOffset;

    public static void Show(Sprite sprite, float yOffset = 0f)
    {
        ShowInternal(sprite, null, null, yOffset, null);
    }

    /// <summary>
    /// Two-part inspect: show closed sprite; hover shows outline; click fades, swaps to open, then onRevealed.
    /// </summary>
    public static void ShowTwoPart(
        Sprite closedSprite,
        Sprite openSprite,
        Action onRevealed,
        float yOffset = 0f,
        Sprite hoverOutline = null)
    {
        ShowInternal(closedSprite, openSprite, onRevealed, yOffset, hoverOutline);
    }

    public static void Hide()
    {
        if (instance == null) return;

        instance.StopAllCoroutines();
        instance.awaitingReveal = false;
        instance.revealing = false;
        instance.outlineHovered = false;
        instance.closedSprite = null;
        instance.hoverOutlineSprite = null;
        instance.revealSprite = null;
        instance.onRevealed = null;
        if (instance.outlineTransform != null)
            instance.outlineTransform.gameObject.SetActive(false);
        if (instance.fadeRenderer != null)
        {
            Color c = instance.fadeRenderer.color;
            c.a = 0f;
            instance.fadeRenderer.color = c;
            instance.fadeTransform.gameObject.SetActive(false);
        }

        GameState.Set("treasure_inspect_open", false);
        instance.itemYOffset = 0f;
        CustomCursor.SetCursorToNormal();
        instance.gameObject.SetActive(false);
    }

    private static void ShowInternal(
        Sprite sprite,
        Sprite openSprite,
        Action onRevealed,
        float yOffset,
        Sprite hoverOutline)
    {
        if (instance != null && (instance.itemRenderer == null || instance.outlineRenderer == null))
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

        instance.StopAllCoroutines();
        instance.closedSprite = sprite;
        instance.hoverOutlineSprite = hoverOutline;
        instance.revealSprite = openSprite;
        instance.onRevealed = onRevealed;
        instance.awaitingReveal = openSprite != null;
        instance.revealing = false;
        instance.outlineHovered = false;
        instance.itemYOffset = yOffset;

        instance.itemRenderer.sprite = sprite;
        instance.itemRenderer.color = Color.white;
        instance.itemTransform.localScale = Vector3.one;
        instance.itemTransform.gameObject.SetActive(sprite != null);

        instance.outlineRenderer.sprite = hoverOutline;
        instance.outlineRenderer.color = Color.white;
        instance.outlineTransform.localPosition = Vector3.zero;
        instance.outlineTransform.localScale = Vector3.one;
        instance.outlineTransform.gameObject.SetActive(false);

        if (instance.fadeRenderer != null)
        {
            Color c = instance.fadeRenderer.color;
            c.a = 0f;
            instance.fadeRenderer.color = c;
            instance.fadeTransform.gameObject.SetActive(false);
        }

        GameState.Set("treasure_inspect_open", true);
        instance.gameObject.SetActive(true);
        instance.SyncToCamera(cam);

        if (instance.awaitingReveal)
            instance.StartCoroutine(instance.WaitForRevealClick());
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        SyncToCamera(cam);
        UpdateHoverOutline(cam);
    }

    private void SyncToCamera(Camera cam)
    {
        Vector3 camPos = cam.transform.position;
        float itemZ = camPos.z + CameraForward;
        float dimZ = itemZ + 0.1f;
        float fadeZ = itemZ - 0.05f;

        dimTransform.position = new Vector3(camPos.x, camPos.y, dimZ);
        if (cam.orthographic)
        {
            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;
            Vector3 cover = new Vector3(width * 1.05f, height * 1.05f, 1f);
            dimTransform.localScale = cover;
            if (fadeTransform != null)
            {
                fadeTransform.position = new Vector3(camPos.x, camPos.y, fadeZ);
                fadeTransform.localScale = cover;
            }
        }
        else
        {
            dimTransform.localScale = new Vector3(40f, 40f, 1f);
            if (fadeTransform != null)
            {
                fadeTransform.position = new Vector3(camPos.x, camPos.y, fadeZ);
                fadeTransform.localScale = new Vector3(40f, 40f, 1f);
            }
        }

        if (!itemTransform.gameObject.activeSelf || itemRenderer.sprite == null)
            return;

        itemTransform.position = new Vector3(camPos.x, camPos.y + itemYOffset, itemZ);
        itemTransform.localScale = Vector3.one;

        Bounds bounds = itemRenderer.bounds;
        Vector3 delta = new Vector3(camPos.x, camPos.y + itemYOffset, bounds.center.z) - bounds.center;
        delta.z = 0f;
        itemTransform.position += delta;
    }

    private void UpdateHoverOutline(Camera cam)
    {
        if (!awaitingReveal || revealing || hoverOutlineSprite == null || closedSprite == null)
        {
            if (outlineTransform != null && outlineTransform.gameObject.activeSelf)
                outlineTransform.gameObject.SetActive(false);
            return;
        }

        bool hovered = IsPointerOverItem(cam);
        if (hovered == outlineHovered)
            return;

        outlineHovered = hovered;
        // Keep the closed art; toggle outline overlay only.
        outlineRenderer.sprite = hoverOutlineSprite;
        outlineTransform.gameObject.SetActive(hovered);
        if (hovered)
            CustomCursor.SetCursorToPointer();
        else
            CustomCursor.SetCursorToNormal();
    }

    private bool IsPointerOverItem(Camera cam)
    {
        if (itemRenderer == null || itemRenderer.sprite == null)
            return false;

        Vector2 screenPos;
        if (Mouse.current != null)
            screenPos = Mouse.current.position.ReadValue();
        else
            screenPos = Input.mousePosition;

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z - itemTransform.position.z)));
        world.z = itemRenderer.bounds.center.z;
        return itemRenderer.bounds.Contains(world);
    }

    private IEnumerator WaitForRevealClick()
    {
        // Ignore the sparkle click that opened this inspect.
        yield return null;
        yield return new WaitUntil(() => !IsPrimaryPressed());

        while (awaitingReveal && !revealing)
        {
            if (IsPrimaryDownThisFrame())
            {
                OnRevealClicked();
                yield break;
            }
            yield return null;
        }
    }

    private static bool IsPrimaryPressed()
    {
        if (Mouse.current != null)
            return Mouse.current.leftButton.isPressed;
        return Input.GetMouseButton(0);
    }

    private static bool IsPrimaryDownThisFrame()
    {
        if (Mouse.current != null)
            return Mouse.current.leftButton.wasPressedThisFrame;
        return Input.GetMouseButtonDown(0);
    }

    private void OnRevealClicked()
    {
        if (!awaitingReveal || revealing || revealSprite == null) return;
        revealing = true;
        outlineHovered = false;
        if (outlineTransform != null)
            outlineTransform.gameObject.SetActive(false);
        CustomCursor.SetCursorToNormal();
        StartCoroutine(RevealRoutine());
    }

    private IEnumerator RevealRoutine()
    {
        yield return StartCoroutine(FadeReveal(1f));

        itemRenderer.sprite = revealSprite;
        hoverOutlineSprite = null;
        closedSprite = revealSprite;
        if (outlineTransform != null)
            outlineTransform.gameObject.SetActive(false);
        Camera cam = Camera.main;
        if (cam != null)
            SyncToCamera(cam);

        yield return StartCoroutine(FadeReveal(0f));

        awaitingReveal = false;
        revealing = false;

        Action callback = onRevealed;
        onRevealed = null;
        callback?.Invoke();
    }

    private IEnumerator FadeReveal(float targetAlpha)
    {
        if (fadeRenderer == null)
            yield break;

        fadeTransform.gameObject.SetActive(true);
        Color c = fadeRenderer.color;
        float startAlpha = c.a;
        float elapsed = 0f;

        while (elapsed < RevealFadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / RevealFadeDuration);
            fadeRenderer.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        fadeRenderer.color = c;

        if (Mathf.Approximately(targetAlpha, 0f))
            fadeTransform.gameObject.SetActive(false);
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

        // Outline shares the item transform so same-size art (e.g. 660x400) lines up exactly.
        GameObject outlineGo = new GameObject("HoverOutline");
        outlineGo.transform.SetParent(itemTransform, false);
        outlineTransform = outlineGo.transform;
        outlineRenderer = outlineGo.AddComponent<SpriteRenderer>();
        outlineRenderer.sortingOrder = OutlineSortOrder;
        outlineGo.SetActive(false);

        GameObject fadeGo = new GameObject("RevealFade");
        fadeGo.transform.SetParent(root.transform, false);
        fadeTransform = fadeGo.transform;
        fadeRenderer = fadeGo.AddComponent<SpriteRenderer>();
        fadeRenderer.sprite = CreateUnitWhiteSprite();
        fadeRenderer.color = new Color(0f, 0f, 0f, 0f);
        fadeRenderer.sortingOrder = FadeSortOrder;
        fadeGo.SetActive(false);
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

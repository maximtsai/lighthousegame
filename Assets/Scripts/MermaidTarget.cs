using UnityEngine;
using UnityEngine.EventSystems;

// The four tools on the medical tray, in the order you use them.
public enum MermaidTool
{
    None = 0,
    Gloves = 1,
    Alcohol = 2,
    FishBones = 3,
    Bandages = 4,
}

// Base for anything on the mermaid's body you can treat with a tool.
//
// All the body art is drawn on one shared 640x1500 canvas, so every target sits at local
// (0,0,0) and lines up with the body by itself. Only sorting order separates them, plus a
// sliver of local z so two overlapping colliders always resolve the same way.
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public abstract class MermaidTarget : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected MermaidMinigame minigame;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected PolygonCollider2D bodyCollider;

    [Header("Hover")]
    [SerializeField] protected Color hoverTint = new Color(1f, 0.82f, 0.82f, 1f);

    private bool hovering;

    public PolygonCollider2D BodyCollider => bodyCollider;

    // Is there still work here for this tool?
    public abstract bool CanApply(MermaidTool tool);

    // Do one step of that work. Only called when CanApply said yes.
    public abstract void Apply(MermaidTool tool);

    // How many clicks this target still needs. Counting clicks rather than targets is what
    // makes a three-stage suture chain read as 3 in the task bar counter.
    public abstract int RemainingSteps(MermaidTool tool);

    // World colliders don't know about UI sitting on top of them, so without this a click on
    // the tray while it's still sliding would fall straight through onto the body.
    private static bool PointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    protected virtual void OnMouseUpAsButton()
    {
        if (PointerOverUI()) return;
        if (minigame != null) minigame.OnTargetClicked(this);
    }

    protected virtual void OnMouseEnter()
    {
        if (PointerOverUI()) return;
        if (minigame == null || !minigame.IsTargetLive(this)) return;

        hovering = true;
        SetHover(true);
        CustomCursor.SetCursorToPointer();
    }

    protected virtual void OnMouseExit()
    {
        ClearHover();
    }

    // Drop the hover without waiting for the mouse to leave, for when a phase ends underneath it.
    public void ClearHover()
    {
        if (!hovering) return;

        hovering = false;
        SetHover(false);
        CustomCursor.SetCursorToNormal();
    }

    protected virtual void SetHover(bool on)
    {
        if (spriteRenderer == null) return;

        // Keep the alpha we already have; bandages fade in and must not be snapped to opaque.
        Color tint = on ? hoverTint : Color.white;
        spriteRenderer.color = new Color(tint.r, tint.g, tint.b, spriteRenderer.color.a);
    }
}

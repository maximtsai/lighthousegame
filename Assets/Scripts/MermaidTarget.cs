using UnityEngine;
using UnityEngine.EventSystems;

// The tools on the medical tray. These values are saved in the scene, so new ones go at the end.
public enum MermaidTool
{
    None = 0,
    Gloves = 1,
    Alcohol = 2,
    FishBones = 3,
    Bandages = 4,
    Fish = 5,
    FishSkin = 6,
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

    // True while this target is mid-animation. Clicks wait until it settles, and a phase waits
    // for every target to settle before moving on, so the last worm pop or fish peel gets to finish.
    public virtual bool IsBusy => false;

    // Called when the player picks up this phase's tool, and again once the phase is over. Most
    // targets don't need them; the fish and the fish skin use them to come on screen and leave again.
    public virtual void OnToolReady() { }
    public virtual void OnPhaseFinished() { }

    // The middle of this target's outline, in its own local space. The art is drawn on full-size
    // canvases, so this, rather than the object's position, is where the thing you see actually is.
    protected Vector2 OutlineMiddle()
    {
        if (bodyCollider == null) return Vector2.zero;

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        for (int i = 0; i < bodyCollider.pathCount; i++)
        {
            foreach (Vector2 p in bodyCollider.GetPath(i))
            {
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
            }
        }

        if (min.x > max.x) return Vector2.zero;
        return (min + max) * 0.5f + bodyCollider.offset;
    }

    // World colliders don't know about UI sitting on top of them, so without this a click on
    // the tray while it's still sliding would fall straight through onto the body.
    protected static bool PointerOverUI()
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

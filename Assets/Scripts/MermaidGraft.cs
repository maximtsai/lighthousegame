using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// The peeled fish skin, rubbed onto the mermaid as a graft (Day 5).
//
// Picking the fish skin off the tray puts it in your hand: it follows the cursor, and a faint
// preview of the graft shows on her torso. Hold the mouse down and rub over the wound, and each
// piece of the graft fills in where you rub, in whatever order you like. Letting go just pauses
// it. Once all seven pieces are on, the skin is used up and fades away.
//
// This object is the skin in your hand. The graft itself is the seven scale layers on the body,
// registered 640x1500 art like everything else, each with its own area traced around it.
public class MermaidGraft : MermaidTarget
{
    [Serializable]
    public class Piece
    {
        public SpriteRenderer art;
        [Tooltip("Rubbing over this area puts this piece on; where pieces overlap, the one on top goes " +
                 "first. Switched on while the skin is in hand.")]
        public PolygonCollider2D area;
    }

    [Header("Graft")]
    [Tooltip("The graft layers on the body. They can go on in any order.")]
    [SerializeField] private Piece[] pieces;
    [Tooltip("How faint the pieces still to go on look while the skin is in hand.")]
    [SerializeField] private float previewAlpha = 0.4f;
    [Tooltip("How far you rub over a piece to put it on, in world units. About one pass across it.")]
    [SerializeField] private float rubPerPiece = 0.65f;

    [Header("Timing")]
    [SerializeField] private float showFadeDuration = 0.25f;
    [Tooltip("How long the used-up skin takes to fade once the last piece is on.")]
    [SerializeField] private float finishFadeDuration = 0.2f;

    [Header("Progress")]
    [SerializeField] private int piecesOn;

    private float[] progress;
    private bool[] done;
    private int finishingPiece = -1;
    private bool inHand;
    private bool finishing;
    private bool rubbing;
    private Vector3 lastCursor;
    private Vector2 artMiddle;
    private Camera cam;

    public override bool IsBusy => finishing;

    void Awake()
    {
        artMiddle = OutlineMiddle();
        progress = new float[pieces.Length];
        done = new bool[pieces.Length];
    }

    public override bool CanApply(MermaidTool tool)
    {
        return tool == MermaidTool.FishSkin && piecesOn < pieces.Length;
    }

    // One step per piece, so the task bar counts the graft going on.
    public override int RemainingSteps(MermaidTool tool)
    {
        return tool == MermaidTool.FishSkin ? pieces.Length - piecesOn : 0;
    }

    public override void OnToolReady()
    {
        inHand = true;
        rubbing = false;
        SetAreas(true);
        transform.position = OnPixelGrid(CursorWorld() - (Vector3)artMiddle);
        ShowPieces();
        StartCoroutine(Fade(spriteRenderer, 0f, 1f, showFadeDuration));
    }

    public override void OnPhaseFinished()
    {
        inHand = false;
        SetAreas(false);
    }

    // Pieces go on by rubbing, so a plain click does nothing.
    protected override void OnMouseUpAsButton() { }

    // The skin sits under the cursor the whole time, so hovering it means nothing. No tint either.
    protected override void SetHover(bool on) { }

    // After the camera has moved for the frame, so the skin never lags a frame behind the view.
    void LateUpdate()
    {
        if (!inHand) return;

        Vector3 cursor = CursorWorld();
        transform.position = OnPixelGrid(cursor - (Vector3)artMiddle);
        if (finishing) return;

        if (!MouseHeld() || PointerOverUI())
        {
            rubbing = false;
            return;
        }

        // Only movement from here on counts, not the jump from wherever the last rub ended.
        if (!rubbing)
        {
            rubbing = true;
            lastCursor = cursor;
            return;
        }

        // Scrolling the view counts too, since the cursor moves across her either way.
        float moved = Vector2.Distance(cursor, lastCursor);
        lastCursor = cursor;

        int under = PieceUnder(cursor);
        if (under >= 0) Rub(under, moved);
    }

    // The topmost piece still to go on under the cursor. The pieces overlap like real scales, so
    // rubbing one spot keeps working down through them until everything there is on.
    private int PieceUnder(Vector3 cursor)
    {
        int top = -1;
        for (int i = 0; i < pieces.Length; i++)
        {
            Piece p = pieces[i];
            if (done[i] || p.area == null || p.art == null || !p.area.OverlapPoint(cursor)) continue;
            if (top < 0 || p.art.sortingOrder > pieces[top].art.sortingOrder) top = i;
        }
        return top;
    }

    private void Rub(int piece, float distance)
    {
        progress[piece] = Mathf.Min(1f, progress[piece] + distance / Mathf.Max(0.01f, rubPerPiece));
        ShowPieces();

        // A full piece is one step of the task: the minigame plays its sound, ticks the counter,
        // and calls Apply to finish this piece.
        if (progress[piece] >= 1f && minigame != null)
        {
            finishingPiece = piece;
            minigame.OnTargetClicked(this);
        }
    }

    public override void Apply(MermaidTool tool)
    {
        int piece = finishingPiece;
        finishingPiece = -1;
        if (piece < 0 || done[piece]) return;

        done[piece] = true;
        piecesOn++;
        ShowPieces();

        if (piecesOn >= pieces.Length) StartCoroutine(UseUp());
    }

    private IEnumerator UseUp()
    {
        finishing = true;
        yield return Fade(spriteRenderer, spriteRenderer.color.a, 0f, finishFadeDuration);
        finishing = false;
    }

    // Pieces already on are solid, one being rubbed fills in as you go, and the rest wait faintly
    // so you can see where the graft is going.
    private void ShowPieces()
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i].art == null) continue;
            SetAlpha(pieces[i].art, done[i] ? 1f : Mathf.Lerp(previewAlpha, 1f, progress[i]));
        }
    }

    private void SetAreas(bool on)
    {
        foreach (Piece piece in pieces)
        {
            if (piece.area != null) piece.area.enabled = on;
        }
    }

    private static bool MouseHeld()
    {
        return Pointer.current != null ? Pointer.current.press.isPressed : Input.GetMouseButton(0);
    }

    // Moved in whole pixels of the body art, so the skin's pixels always sit on hers. The skin art
    // is an odd size, so its centre pivot is half a pixel in, and the grid shifts by that much.
    private Vector3 OnPixelGrid(Vector3 p)
    {
        Sprite sprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        if (sprite == null) return p;

        float ppu = sprite.pixelsPerUnit;
        Vector2 shift = new Vector2(sprite.pivot.x % 1f, sprite.pivot.y % 1f) / ppu;
        return new Vector3(Mathf.Round((p.x - shift.x) * ppu) / ppu + shift.x,
                           Mathf.Round((p.y - shift.y) * ppu) / ppu + shift.y, p.z);
    }

    private Vector3 CursorWorld()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return transform.position;

        Vector2 screen = Pointer.current != null
            ? Pointer.current.position.ReadValue()
            : (Vector2)Input.mousePosition;

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
        world.z = transform.position.z;
        return world;
    }

    private static IEnumerator Fade(SpriteRenderer r, float from, float to, float duration)
    {
        if (r == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(r, Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        SetAlpha(r, to);
    }

    private static void SetAlpha(SpriteRenderer r, float a)
    {
        Color c = r.color;
        r.color = new Color(c.r, c.g, c.b, a);
    }
}

using UnityEngine;

/// <summary>
/// Day 4: the mermaid lying in Camborne's bed once she's been laid down there (see
/// MiscObjectClick.ClickPartnerBed). While she's there, hovering the bed shows her yellow outline
/// in place of the bed's own highlight. Added by BedroomMiscLogic.
/// </summary>
public class MermaidOnBed : MonoBehaviour
{
    private Sprite daySprite;
    private Sprite nightSprite;
    private Sprite outlineSprite;
    private Vector3 position;

    private SpriteRenderer body;
    private SpriteRenderer outline;
    private InteractableObject bed;

    public static bool ShouldShow =>
        Day4Beach.IsToday && GameState.Get<bool>(Day4Beach.MermaidInBedKey, false);

    public void Init(Sprite daySprite, Sprite nightSprite, Sprite outlineSprite, Vector3 position)
    {
        this.daySprite = daySprite;
        this.nightSprite = nightSprite;
        this.outlineSprite = outlineSprite;
        this.position = position;
    }

    // Checked every frame so she turns up the moment she's laid down, not just on scene load.
    void Update()
    {
        if (body == null && ShouldShow)
            Show();
    }

    private void Show()
    {
        bool night = GameState.Get<bool>("is_nighttime", false);
        Sprite sprite = night && nightSprite != null ? nightSprite : daySprite;
        if (sprite == null)
        {
            Debug.LogWarning("MermaidOnBed: no mermaid sprite assigned");
            enabled = false;
            return;
        }

        // Above the bed in the background, below the treasure sparkles.
        body = CreateRenderer("mermaid_on_bed", sprite, 1);
        if (outlineSprite != null)
        {
            outline = CreateRenderer("mermaid_on_bed_outline", outlineSprite, 2);
            outline.enabled = false;
        }

        GameObject bedObject = GameObject.Find("PartnerBed");
        bed = bedObject != null ? bedObject.GetComponent<InteractableObject>() : null;
        if (bed != null)
        {
            // Her outline is the highlight now, so the bed's own one would only get in the way
            bed.hover_sprite = null;
            bed.HoverChanged += OnBedHover;
        }
    }

    private SpriteRenderer CreateRenderer(string name, Sprite sprite, int sortingOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.position = position;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
        return sr;
    }

    private void OnBedHover(bool hovering)
    {
        if (outline != null)
            outline.enabled = hovering;
    }

    void OnDestroy()
    {
        if (bed != null)
            bed.HoverChanged -= OnBedHover;
    }
}

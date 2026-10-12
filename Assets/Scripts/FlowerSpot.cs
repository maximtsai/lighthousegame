using UnityEngine;
using UnityEngine.EventSystems;

// Each spot, its art, outline and Button callback are saved in FlowerScene.
public class FlowerSpot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private FlowerMinigame minigame;
    [SerializeField] private GameObject hoverOutline;
    [SerializeField] private Sprite pickedSprite;
    [SerializeField] private Vector2 inspectionSize;
    [SerializeField] private Vector2 inspectionPosition;
    [SerializeField] private bool healthy;
    [SerializeField, Range(1, 2)] private int flowerCount = 1;

    private FlowerSpot revealSource;
    public Sprite PickedSprite => revealSource != null ? revealSource.pickedSprite : pickedSprite;
    public Vector2 InspectionSize => revealSource != null ? revealSource.inspectionSize : inspectionSize;
    public Vector2 InspectionPosition => revealSource != null ? revealSource.inspectionPosition : inspectionPosition;
    public bool Healthy => revealSource != null ? revealSource.healthy : healthy;
    public int FlowerCount => flowerCount;

    internal void SetRevealSource(FlowerSpot source)
    {
        // Only the inspection result moves; the saved background art and hit area stay put.
        revealSource = flowerCount == 1 && source.flowerCount == 1 ? source : null;
    }

    public void Pick()
    {
        if (minigame != null)
            minigame.Inspect(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (minigame != null && minigame.CanInspect)
        {
            hoverOutline.SetActive(true);
            CustomCursor.SetCursorToPointer();
        }
    }

    public void OnPointerExit(PointerEventData eventData) => ClearHover();

    public void ClearHover()
    {
        if (hoverOutline != null)
            hoverOutline.SetActive(false);
        CustomCursor.SetCursorToNormal();
    }

    private void OnDisable() => ClearHover();
}

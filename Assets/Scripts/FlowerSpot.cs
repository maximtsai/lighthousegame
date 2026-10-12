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

    public Sprite PickedSprite => pickedSprite;
    public Vector2 InspectionSize => inspectionSize;
    public Vector2 InspectionPosition => inspectionPosition;
    public bool Healthy => healthy;
    public int FlowerCount => flowerCount;

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

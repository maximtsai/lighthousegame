using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Shows a glow object while the pointer is over this UI element (e.g. the
// KEEP / DISCARD buttons). Mirrors the hover_sprite swap that InteractableObject
// does for world sprites, but works for UI / while minigame_open is true.
[RequireComponent(typeof(RectTransform))]
public class UIButtonGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject glow;
    [Tooltip("Optional. If set, only glow while this can be clicked, so greyed-out tools stay dark.")]
    [SerializeField] private Selectable onlyWhenInteractable;

    void Awake()
    {
        if (glow != null)
            glow.SetActive(false);
    }

    void OnDisable()
    {
        if (glow != null)
            glow.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onlyWhenInteractable != null && !onlyWhenInteractable.IsInteractable())
            return;

        if (glow != null)
            glow.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (glow != null)
            glow.SetActive(false);
    }
}

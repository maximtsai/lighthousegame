using System;
using UnityEngine;

/// <summary>
/// A click area made in code (the Day 4 scraps, bodies and mermaid at the dock, the Day 5 flower field),
/// with the same guards as InteractableObject. Set onClick after adding it.
/// </summary>
public class ClickTarget : MonoBehaviour
{
    public Action onClick;

    private static bool Blocked => InputGate.WorldBlocked;

    void OnMouseOver()
    {
        if (!Blocked)
            CustomCursor.SetCursorToPointer();
    }

    void OnMouseExit()
    {
        CustomCursor.SetCursorToNormal();
    }

    void OnMouseUp()
    {
        if (Blocked) return;
        CustomCursor.SetCursorToNormal();
        onClick?.Invoke();
    }
}

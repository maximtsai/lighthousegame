using System;
using UnityEngine;

/// <summary>
/// A click area made in code (the Day 4 scraps, bodies and mermaid at the dock, the Day 5 flower field),
/// with the same guards as InteractableObject. Set onClick after adding it.
/// </summary>
public class ClickTarget : MonoBehaviour
{
    public Action onClick;
    // Told when the cursor comes onto it (true) and leaves it or gets blocked (false), for a highlight
    public Action<bool> onHover;

    private bool hovering;

    private static bool Blocked => InputGate.WorldBlocked;

    void OnMouseOver()
    {
        if (!Blocked)
            CustomCursor.SetCursorToPointer();
        SetHovering(!Blocked);
    }

    void OnMouseExit()
    {
        CustomCursor.SetCursorToNormal();
        SetHovering(false);
    }

    private void SetHovering(bool value)
    {
        if (hovering == value) return;
        hovering = value;
        onHover?.Invoke(value);
    }

    void OnMouseUp()
    {
        if (Blocked) return;
        CustomCursor.SetCursorToNormal();
        onClick?.Invoke();
    }
}

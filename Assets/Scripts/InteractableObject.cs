using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] string object_id = "null";
    public Sprite default_sprite;
    public Sprite hover_sprite;
    [SerializeField] UnityEvent on_click;

    private AudioSource audioSource;
    public AudioClip clickSound;
    private bool hasSound;

    // Raised when the hover highlight turns on (true) or off (false), for extra highlights that
    // live on other objects (e.g. the mermaid's outline on Camborne's bed).
    public event Action<bool> HoverChanged;
    private bool hovering;

    void Awake()
    {
        if (on_click == null)
            on_click = new UnityEvent();
    }

    public void AddClickListener(UnityAction action)
    {
        if (on_click == null)
            on_click = new UnityEvent();
        on_click.AddListener(action);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpriteRenderer srender = transform.GetComponent<SpriteRenderer>();
        srender.sprite = default_sprite;
        srender.color = Color.white;
        // Automatically find the AudioSource in the scene
        if (AudioManager.Instance)
        {
            audioSource = AudioManager.Instance.AudioSource;
        }
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found in the scene!");
        }
        else if (clickSound != null)
        {
            audioSource.clip = clickSound;
            hasSound = true;
        }
    }

    void OnMouseOver()
    {
        if (InputGate.WorldBlocked)
        {
            // Something opened on top while we were hovered; drop the highlight until it closes.
            // Whatever opened owns the cursor now (the dialogue box sets its own), so leave it.
            if (hovering) ClearHover(false);
            return;
        }
        transform.GetComponent<SpriteRenderer>().sprite = hover_sprite;
        CustomCursor.SetCursorToPointer();
        SetHovering(true);
    }

    void OnMouseExit()
    {
        if (hovering) ClearHover(true);
    }

    // OnMouseExit never comes if the collider or object is switched off under the cursor
    // (e.g. a treasure spot hiding once found), so the pointer and hover sprite would stick.
    void OnDisable()
    {
        if (hovering) ClearHover(!InputGate.WorldBlocked);
    }

    private void ClearHover(bool resetCursor)
    {
        SpriteRenderer srender = GetComponent<SpriteRenderer>();
        if (srender != null) srender.sprite = default_sprite;
        if (resetCursor) CustomCursor.SetCursorToNormal();
        SetHovering(false);
    }

    private void SetHovering(bool value)
    {
        if (hovering == value) return;
        hovering = value;
        HoverChanged?.Invoke(value);
    }

    void OnMouseUp()
    {
        if (InputGate.WorldBlocked) return;
        if (hasSound)
        {
            audioSource.PlayOneShot(clickSound);
        }
        on_click.Invoke();
    }



    public string GetObjectId()
    {
        return this.object_id;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] string object_id = "null";
    [SerializeField] Sprite default_sprite;
    [SerializeField] Sprite hover_sprite;
    [SerializeField] UnityEvent on_click;

    private AudioSource audioSource;
    public AudioClip clickSound;
    private bool hasSound;

    void Awake()
    {
        if (on_click == null)
            on_click = new UnityEvent();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpriteRenderer srender = transform.GetComponent<SpriteRenderer>();
        srender.sprite = default_sprite;
        srender.color = Color.white;
        Debug.Log("activate");
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
        else
        {
            Debug.Log("has audio source but no clickSound");
        }
    }

    void OnMouseOver()
    {
        if (GameState.Get<bool>("task_list_open", false)) return;
        if (GameState.Get<bool>("minigame_open", false)) return;
        transform.GetComponent<SpriteRenderer>().sprite = hover_sprite;
        CustomCursor.SetCursorToPointer();
    }

    void OnMouseExit()
    {
        transform.GetComponent<SpriteRenderer>().sprite = default_sprite;
        CustomCursor.SetCursorToNormal();
    }

    void OnMouseUp()
    {
        if (GameState.Get<bool>("task_list_open", false)) return;
        if (GameState.Get<bool>("minigame_open", false)) return;
        Debug.Log("mouse up");
        if (hasSound)
        {
            Debug.Log("has sound");
            audioSource.PlayOneShot(clickSound);
        }
        on_click.Invoke();
    }

    public string GetObjectId()
    {
        return this.object_id;
    }
}

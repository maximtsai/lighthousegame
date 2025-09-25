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
    }

    void OnMouseOver()
    {
        transform.GetComponent<SpriteRenderer>().sprite = hover_sprite;
    }

    void OnMouseExit()
    {
        transform.GetComponent<SpriteRenderer>().sprite = default_sprite;
    }

    void OnMouseDown()
    {
        on_click.Invoke();
    }

    public string GetObjectId()
    {
        return this.object_id;
    }
}

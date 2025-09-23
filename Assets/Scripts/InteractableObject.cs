using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    public Sprite default_sprite;
    public Sprite hover_sprite;
    public UnityEvent on_click;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpriteRenderer srender = transform.GetComponent<SpriteRenderer>();
        srender.sprite = default_sprite;
        srender.color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        
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
}

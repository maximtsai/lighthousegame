using UnityEngine;

public class TableSeatScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Sprite camborne_dark;
    public Sprite mermaid;
    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (GameState.Get<int>("day", 1) > 1 || GameState.Get<bool>("has_buried", false))
        {
            gameObject.SetActive(false);
        } else if (GameState.Get<bool>("is_nighttime", false))
        {
            spriteRenderer.sprite = camborne_dark;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

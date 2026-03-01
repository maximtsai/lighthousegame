using UnityEngine;

public class kitchenBGScript : MonoBehaviour
{
    [SerializeField] private Sprite stainedSprite;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameState.Get<int>("day") >= 2)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && stainedSprite != null)
            {
                sr.sprite = stainedSprite;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

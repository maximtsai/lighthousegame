using UnityEngine;

public class SetBackgroundWhenDark : MonoBehaviour
{
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Sprite dark_bg;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameState.Get<bool>("is_nighttime", false))
        {
            background.sprite = dark_bg;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

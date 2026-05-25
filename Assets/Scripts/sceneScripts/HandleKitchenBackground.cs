using UnityEngine;

public class HandleKitchenBackground : MonoBehaviour
{
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Sprite dark_bg;
    [SerializeField] private Sprite stained_bg;
    [SerializeField] private Sprite stained_dark_bg;

    // Start is called once before the first execution of Update after the MonoBehaviour  is created
    void Start()
    {
        if (GameState.Get<bool>("is_nighttime", false))
        {
            if (GameState.Get<int>("day") >= 2) {
                background.sprite = stained_dark_bg;
            } else {
                background.sprite = stained_dark_bg;
            }
        }
        else if (GameState.Get<int>("day") >= 2)
        {
            background.sprite = stained_bg;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

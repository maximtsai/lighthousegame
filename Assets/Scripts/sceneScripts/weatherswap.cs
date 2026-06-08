using UnityEngine;

public class weatherswap : MonoBehaviour
{
    [Header("Weather Sprites")]
    [SerializeField] private Sprite altocumulusSprite;
    [SerializeField] private Sprite stratusSprite;
    [SerializeField] private Sprite cirrocumulusSprite;
    [SerializeField] private Sprite cirrostratusSprite;
    [SerializeField] private Sprite cirrusSprite;
    [SerializeField] private Sprite cumulonimbusSprite;

    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("weatherswap: No SpriteRenderer component found on this GameObject.");
            return;
        }

        int day = GameState.Get<int>("day");
        Sprite targetSprite = null;

        switch (day)
        {
            case 1:
                targetSprite = altocumulusSprite;
                break;
            case 2:
                targetSprite = stratusSprite;
                break;
            case 3:
                targetSprite = cirrocumulusSprite;
                break;
            case 4:
                targetSprite = cirrostratusSprite;
                break;
            case 5:
                targetSprite = cirrusSprite;
                break;
            case 6:
                targetSprite = cumulonimbusSprite;
                break;
            case 7:
                targetSprite = stratusSprite;
                break;
            default:
                targetSprite = altocumulusSprite;
                break;
        }

        if (targetSprite != null)
        {
            spriteRenderer.sprite = targetSprite;
            Debug.Log($"weatherswap: Swapped weather sprite to {targetSprite.name} for Day {day}");
        }
        else
        {
            Debug.LogWarning($"weatherswap: Target sprite is null for Day {day}. Make sure it is assigned in the Inspector.");
        }
    }
}

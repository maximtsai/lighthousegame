using UnityEngine;

public class handlogic : MonoBehaviour
{
    [Header("Cleaned Sprites")]
    [SerializeField] private Sprite handGash;
    [SerializeField] private Sprite nightGash;

    [Header("Bleeding Sprites")]
    [SerializeField] private Sprite nightHandBleed;

    private Vector3 initialPosition;
    private bool hasSavedPos = false;
    private Sprite originalSprite;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // If the hand is not cut, deactivate this game object immediately
        if (!GameState.Get<bool>("hand_cut", false))
        {
            gameObject.SetActive(false);
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }

        initialPosition = transform.position;
        hasSavedPos = true;
    }

    void Update()
    {
        UpdateSprite();
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            if (!hasSavedPos)
            {
                initialPosition = transform.position;
                hasSavedPos = true;
            }
            Vector3 camPos = Camera.main.transform.position;
            transform.position = initialPosition + new Vector3(camPos.x * 0.6f, camPos.y * 0.6f, 0f);
        }
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        bool isNight = GameState.Get<bool>("is_nighttime", false);

        if (GameState.Get<bool>("hand_cleaned", false))
        {
            Sprite targetCleanSprite = isNight ? nightGash : handGash;
            if (targetCleanSprite != null && spriteRenderer.sprite != targetCleanSprite)
            {
                spriteRenderer.sprite = targetCleanSprite;
            }
        }
        else
        {
            Sprite targetBleedSprite = isNight ? nightHandBleed : originalSprite;
            if (targetBleedSprite != null && spriteRenderer.sprite != targetBleedSprite)
            {
                spriteRenderer.sprite = targetBleedSprite;
            }
        }
    }
}

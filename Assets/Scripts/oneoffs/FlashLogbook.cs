using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlashLogbook : MonoBehaviour
{
    [SerializeField] private Sprite btn_normal;
    [SerializeField] private Sprite btn_hover;
    
    private SpriteRenderer spriteRenderer;
    private Image uiImage;
    private Coroutine flashCoroutine;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiImage = GetComponent<Image>();

        if (!GameState.Get<bool>("is_recording_weather", false) && !GameState.Get<bool>("recorded_weather", false))
        {
            flashCoroutine = StartCoroutine(FlashSequence());
        }
    }

    private IEnumerator FlashSequence()
    {
        while (!GameState.Get<bool>("is_recording_weather", false) && !GameState.Get<bool>("recorded_weather", false))
        {
            // Wait 2 seconds before the next blink cycle
            yield return new WaitForSeconds(2f);

            if (GameState.Get<bool>("is_recording_weather", false) || GameState.Get<bool>("recorded_weather", false))
                break;

            // Flash 1
            SetSprite(btn_hover);
            yield return new WaitForSeconds(0.2f);
            SetSprite(btn_normal);
            yield return new WaitForSeconds(0.2f);

            if (GameState.Get<bool>("is_recording_weather", false) || GameState.Get<bool>("recorded_weather", false))
                break;

            // Flash 2
            SetSprite(btn_hover);
            yield return new WaitForSeconds(0.2f);
            SetSprite(btn_normal);
        }
        
        // Restore to normal at the end
        SetSprite(btn_normal);
    }

    private void SetSprite(Sprite sprite)
    {
        if (sprite == null) return;
        
        if (uiImage != null)
        {
            uiImage.sprite = sprite;
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    void OnDisable()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        SetSprite(btn_normal);
    }
}

using System.Collections;
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

    [Header("Thunderstorm")]
    [SerializeField] private AudioClip thunderSound;
    [SerializeField] private GameObject whiteScreenFlash;
    [SerializeField] private float flashOnDuration = 0.1f;
    [SerializeField] private float flashGapDuration = 0.15f;

    public void TriggerStorm(bool firstTime = false)
    {
        EnableStormRain();

        if (firstTime)
        {
            StartCoroutine(ThunderFlashRoutine());
        }

        OutdoorsMiscLogic outdoors = FindFirstObjectByType<OutdoorsMiscLogic>();
        if (outdoors != null)
        {
            outdoors.UpdateAmbience();
        }

        if (firstTime && thunderSound != null)
        {
            MessageBus.Instance.Publish("PlaySound", thunderSound);
        }
    }

    private IEnumerator ThunderFlashRoutine()
    {
        for (int i = 0; i < 2; i++)
        {
            if (whiteScreenFlash != null)
            {
                whiteScreenFlash.SetActive(true);
            }
            yield return new WaitForSeconds(flashOnDuration);
            if (whiteScreenFlash != null)
            {
                whiteScreenFlash.SetActive(false);
            }
            if (i < 1)
            {
                yield return new WaitForSeconds(flashGapDuration);
            }
        }
    }

    private void EnableStormRain()
    {
        foreach (Animator anim in FindObjectsByType<Animator>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (anim.gameObject.name == "rain_0")
            {
                anim.gameObject.SetActive(true);
                return;
            }
        }
        Debug.LogWarning("weatherswap: rain_0 rain overlay not found in scene");
    }

    void Start()
    {
        if (GameState.Get<bool>("thunderstorm", false))
        {
            TriggerStorm();
        }

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
                targetSprite = cirrocumulusSprite;
                break;
            case 3:
                targetSprite = stratusSprite;
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

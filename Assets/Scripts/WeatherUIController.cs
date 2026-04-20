using UnityEngine;
using UnityEngine.UI;

public class WeatherUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("The centered visual image.")]
    [SerializeField] private Image weatherImage;
    
    [Tooltip("The button at the bottom right.")]
    [SerializeField] private Button actionButton;

    [Header("Animation")]
    [Tooltip("The animator component on the weather image.")]
    [SerializeField] private Animator weatherAnimator;
    
    [Tooltip("The name of the state or trigger to play the 'openbook' animation.")]
    [SerializeField] private string animationName = "openbook";

    void Start()
    {
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(OnActionButtonClicked);
        }
    }

    private void OnActionButtonClicked()
    {
        // 1. Play the animation
        if (weatherAnimator != null)
        {
            Debug.Log("play anim, " + animationName);
            weatherAnimator.Play(animationName);
        }
        else
        {
            Debug.LogWarning("WeatherUIController: No Animator assigned!");
        }

        GameState.Set("recorded_weather", true);

        // 2. Make the button no longer clickable
        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(false);
        }
    }
}

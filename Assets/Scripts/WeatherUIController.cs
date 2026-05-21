using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeatherUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("The centered visual image.")]
    [SerializeField] private Image weatherImage;
    
    [Tooltip("The button at the bottom right.")]
    [SerializeField] private Button actionButton;

    [Tooltip("The click blocker GameObject.")]
    [SerializeField] private GameObject clickBlocker;

    [Tooltip("The parent object for cloud buttons.")]
    [SerializeField] private GameObject cloudButtons;

    [Tooltip("The image where clouds will be drawn.")]
    [SerializeField] private Image drawImage;

    [Header("Animation")]
    [Tooltip("The animator component on the weather image.")]
    [SerializeField] private Animator weatherAnimator;
    
    [Tooltip("The name of the state or trigger to play the 'openbook' animation.")]
    [SerializeField] private string animationName = "openbook";

    [Tooltip("The GameObject for the flashing effect.")]
    [SerializeField] private GameObject flasherObject;
    
    [Tooltip("The button to confirm the selection.")]
    [SerializeField] private Button confirmButton;

    void Start()
    {
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(OnActionButtonClicked);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(confirmCloudSelection);
        }

        SetupCloudButtonHoverSounds();
    }

    private void SetupCloudButtonHoverSounds()
    {
        if (cloudButtons != null)
        {
            Button[] buttons = cloudButtons.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                if (btn.gameObject.GetComponent<HoverSound>() == null)
                {
                    btn.gameObject.AddComponent<HoverSound>();
                }
            }
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
        GameState.Set("is_recording_weather", true);

        // 2. Make the button no longer clickable
        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(false);
        }

        if (clickBlocker != null)
        {
            clickBlocker.SetActive(true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySoundByName("paper_flap");
        }

        if (cloudButtons != null)
        {
            StartCoroutine(ActivateCloudButtonsDelayed(0.8f));
        }

        if (drawImage != null)
        {
            drawImage.gameObject.SetActive(true);
        }

        if (flasherObject != null)
        {
            flasherObject.SetActive(false);
        }

        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(false);
        }
    }

    private IEnumerator ActivateCloudButtonsDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (cloudButtons != null)
        {
            cloudButtons.SetActive(true);
        }
    }
    public void SelectCloudSprite(Sprite sprite)
    {
        if (drawImage != null && sprite != null)
        {
            drawImage.sprite = sprite;
            drawImage.gameObject.SetActive(true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySoundWithRandomPitch("quill_short", 0.9f, 1.1f);
        }

        if (flasherObject != null)
        {
            flasherObject.SetActive(true);
        }

        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(true);
        }
    }

    public void confirmCloudSelection()
    {
        if (cloudButtons != null)
        {
            cloudButtons.SetActive(false);
        }

        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }

        if (flasherObject != null)
        {
            flasherObject.SetActive(false);
        }

        StartCoroutine(PostConfirmSequence());
    }

    private IEnumerator PostConfirmSequence()
    {
        yield return new WaitForSeconds(0.25f);
        postConfirmJudgment();
    }

    private void postConfirmJudgment()
    {
        bool isPartialCorrect = GameState.Get<int>("day") == 2;
        bool isCorrect = false;

        if (drawImage != null && drawImage.sprite != null)
        {
            if (drawImage.sprite.name == "weather_altocumulus_left")
            {
                isCorrect = true;
            }
        }

        string spriteName = drawImage.sprite != null ? drawImage.sprite.name : "None";
        Debug.Log($"Judgment Result - Correct: {isCorrect}, Partial: {isPartialCorrect}, Selected Sprite: {spriteName}");

        string dialoguePath;
        if (GameState.Get<int>("day") == 2)
        {
            dialoguePath = "ScriptableObjects/Dialogues/outdoors/weather_neutral";
        }
        else
        {
            dialoguePath = isCorrect ? "ScriptableObjects/Dialogues/outdoors/weather_correct" : "ScriptableObjects/Dialogues/outdoors/weather_incorrect";
        }

        Dialogue dialogue = Resources.Load<Dialogue>(dialoguePath);
        if (dialogue != null)
        {
            DialogueManager.ShowDialogue(dialogue);
        }
        else
        {
            Debug.LogWarning("Dialogue not found at path: " + dialoguePath);
        }
    }
}

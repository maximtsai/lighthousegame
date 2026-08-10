using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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
        if (GameState.Get<bool>("recorded_weather", false))
        {
            this.gameObject.SetActive(false);
            return;
        }

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

        // Day 3: don't set recorded_weather until after "Are you sure?" -> YES
        if (GameState.Get<int>("day") != 3)
        {
            GameState.Set("recorded_weather", true);
        }
        StartCoroutine(PostConfirmSequence());
    }

    private IEnumerator PostConfirmSequence()
    {
        yield return new WaitForSeconds(0.1f);
        postConfirmJudgment();
    }

    private void postConfirmJudgment()
    {
        int day = GameState.Get<int>("day");

        // Day 3 has special flow: no sanity change, "Are you sure?" -> thunderstorm
        if (day == 3)
        {
            ShowDay3WeatherFlow();
            return;
        }

        bool isCorrect = false;

        string targetSpriteName = "weather_altocumulus_left";
        switch (day)
        {
            case 1:
                targetSpriteName = "weather_altocumulus_left";
                break;
            case 2:
                targetSpriteName = "weather_cirrocumulus_left";
                break;
            case 4:
                targetSpriteName = "weather_cirrostratus_left";
                break;
            case 5:
                targetSpriteName = "weather_cirrus_left";
                break;
            case 6:
                targetSpriteName = "weather_cumulonimbus_left";
                break;
            case 7:
                targetSpriteName = "weather_stratus_left";
                break;
            default:
                targetSpriteName = "weather_altocumulus_left";
                break;
        }

        if (drawImage != null && drawImage.sprite != null)
        {
            string selectedName = drawImage.sprite.name;
            if (selectedName == targetSpriteName || selectedName.EndsWith(targetSpriteName))
            {
                isCorrect = true;
            }
        }

        string spriteName = drawImage.sprite != null ? drawImage.sprite.name : "None";
        Debug.Log($"Judgment Result - Day: {day}, Correct: {isCorrect}, Target: {targetSpriteName}, Selected Sprite: {spriteName}");

        string dialoguePath = isCorrect ? "ScriptableObjects/Dialogues/outdoors/weather_correct" : "ScriptableObjects/Dialogues/outdoors/weather_incorrect";

        Dialogue originalDialogue = Resources.Load<Dialogue>(dialoguePath);
        if (originalDialogue != null)
        {
            Dialogue dialogue = Instantiate(originalDialogue);
            dialogue.onDialogueEnd.AddListener(OnDialogueFinished);
            DialogueManager.ShowDialogue(dialogue);
        }
        else
        {
            Debug.LogWarning("Dialogue not found at path: " + dialoguePath);
        }
    }

    private void ShowDay3WeatherFlow()
    {
        Dialogue areYouSure = ScriptableObject.CreateInstance<Dialogue>();
        areYouSure.text = new List<string>(new string[] { "Are you sure?" });
        areYouSure.choices = new List<string>(new string[] { "YES", "NO" });
        areYouSure.consequences = new List<UnityEngine.Events.UnityEvent>();
        areYouSure.onDialogueEnd = new UnityEngine.Events.UnityEvent();
        areYouSure.onDialogueEndImmediate = new UnityEngine.Events.UnityEvent();

        // YES: continue to next question
        UnityEngine.Events.UnityEvent yesEvent = new UnityEngine.Events.UnityEvent();
        yesEvent.AddListener(() =>
        {
            ShowDay3ThunderstormPrompt();
        });

        // NO: go back to weather UI
        UnityEngine.Events.UnityEvent noEvent = new UnityEngine.Events.UnityEvent();
        noEvent.AddListener(() =>
        {
            // Don't set recorded_weather, keep is_recording_weather true
            // Re-open cloud buttons for selection
            if (cloudButtons != null)
            {
                cloudButtons.SetActive(true);
            }
            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(false);
                confirmButton.interactable = true;
            }
            if (flasherObject != null)
            {
                flasherObject.SetActive(false);
            }
            if (drawImage != null)
            {
                drawImage.sprite = null;
                drawImage.gameObject.SetActive(false);
            }
        });

        areYouSure.consequences.Add(yesEvent);
        areYouSure.consequences.Add(noEvent);

        DialogueManager.ShowDialogue(areYouSure);
    }

    private void ShowDay3ThunderstormPrompt()
    {
        Dialogue thunderstormDialogue = ScriptableObject.CreateInstance<Dialogue>();
        thunderstormDialogue.text = new List<string>(new string[] { "Are you certain it's not raining?" });
        thunderstormDialogue.choices = new List<string>(new string[] { "...Huh?" });
        thunderstormDialogue.consequences = new List<UnityEngine.Events.UnityEvent>();
        thunderstormDialogue.onDialogueEnd = new UnityEngine.Events.UnityEvent();
        thunderstormDialogue.onDialogueEndImmediate = new UnityEngine.Events.UnityEvent();

        UnityEngine.Events.UnityEvent huhEvent = new UnityEngine.Events.UnityEvent();
        huhEvent.AddListener(() =>
        {
            changeToThunderstorm();
        });

        thunderstormDialogue.consequences.Add(huhEvent);

        DialogueManager.ShowDialogue(thunderstormDialogue);
    }

    public void changeToThunderstorm()
    {
        // TODO: Implement thunderstorm logic
        Debug.Log("changeToThunderstorm called");
        GameState.Set("recorded_weather", true);
        MessageBus.Instance.Publish("CompleteTask", "task_weather");
        StartCoroutine(CloseWeatherUIDelayed());
    }

    private void OnDialogueFinished()
    {
        if (this.gameObject.activeInHierarchy)
        {
            StartCoroutine(CloseWeatherUIDelayed());
        }
    }

    private IEnumerator CloseWeatherUIDelayed()
    {
        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(false);
            confirmButton.interactable = true; // reset interactable for next use
        }

        Vector3 startWeatherImagePos = weatherImage != null ? weatherImage.transform.localPosition : Vector3.zero;
        Vector3 startDrawImagePos = drawImage != null ? drawImage.transform.localPosition : Vector3.zero;
        Vector3 startCloudButtonsPos = cloudButtons != null ? cloudButtons.transform.localPosition : Vector3.zero;

        Vector3 endWeatherImagePos = startWeatherImagePos + new Vector3(0, -1200f, 0);
        Vector3 endDrawImagePos = startDrawImagePos + new Vector3(0, -1200f, 0);
        Vector3 endCloudButtonsPos = startCloudButtonsPos + new Vector3(0, -1200f, 0);

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Smooth step interpolation for a premium animation feel
            t = t * t * (3f - 2f * t);

            if (weatherImage != null) weatherImage.transform.localPosition = Vector3.Lerp(startWeatherImagePos, endWeatherImagePos, t);
            if (drawImage != null) drawImage.transform.localPosition = Vector3.Lerp(startDrawImagePos, endDrawImagePos, t);
            if (cloudButtons != null) cloudButtons.transform.localPosition = Vector3.Lerp(startCloudButtonsPos, endCloudButtonsPos, t);

            yield return null;
        }

        if (weatherImage != null) weatherImage.transform.localPosition = endWeatherImagePos;
        if (drawImage != null) drawImage.transform.localPosition = endDrawImagePos;
        if (cloudButtons != null) cloudButtons.transform.localPosition = endCloudButtonsPos;

        if (clickBlocker != null)
        {
            clickBlocker.SetActive(false);
        }

        // 2. Set GameState and deactivate the entire game object
        GameState.Set("is_recording_weather", false);
        this.gameObject.SetActive(false);

        // Restore original positions so they are ready for the next opening
        if (weatherImage != null) weatherImage.transform.localPosition = startWeatherImagePos;
        if (drawImage != null) drawImage.transform.localPosition = startDrawImagePos;
        if (cloudButtons != null) cloudButtons.transform.localPosition = startCloudButtonsPos;
    }
}

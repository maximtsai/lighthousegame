using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FlowerMinigame : MonoBehaviour
{
    public const string ResolvedKey = "flowers_day5_resolved";
    public const string KeptKey = "flowers_day5_kept";
    public const string CompleteKey = "gathered_flowers";

    [Header("Saved scene objects")]
    [SerializeField] private FlowerSpot[] flowers;
    [SerializeField] private RectTransform field;
    [SerializeField] private RectTransform fieldViewport;
    [SerializeField] private GameObject inspectionPanel;
    [SerializeField] private Image pickedFlower;
    [SerializeField] private Button keepButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private TextMeshProUGUI progressLabel;
    [SerializeField] private TextMeshProUGUI feedbackLabel;
    [Header("Cursor movement")]
    [SerializeField, Range(0f, 0.49f)] private float edgeThreshold = 0.25f;
    [SerializeField, Range(0.01f, 1f)] private float panSmoothing = 0.0335f;
    [Header("Preview movement")]
    [SerializeField] private float bobAmplitude = 5f;
    [SerializeField] private float bobSpeed = 1.4f;
    [Header("Sanity popup")]
    [SerializeField] private float popupYOffset = 80f;
    [SerializeField] private float popupRise = 40f;
    [SerializeField] private float popupDuration = 1f;
    [Header("Sound and timing")]
    [SerializeField] private AudioSource choiceAudio;
    [SerializeField] private AudioClip keepSound;
    [SerializeField] private AudioClip discardSound;
    [SerializeField] private float feedbackDuration = 0.65f;

    private FlowerSpot current;
    private bool resolving;
    private bool fieldWasAvailable;
    private bool complete;
    private float bobPhase;

    private bool InputBlocked => GameState.Get<bool>("navigationBlocked")
        || GameState.Get<bool>("pause_open") || GameState.Get<bool>("task_list_open")
        || DialogueManager.DialogueIsOpen();
    public bool CanInspect => !resolving && current == null && !InputBlocked;

    private void Start()
    {
        GameState.Set("minigame_open", true);
        inspectionPanel.SetActive(false);
        feedbackLabel.gameObject.SetActive(false);
        RefreshField();
    }

    private void Update()
    {
        if (current != null && !InputBlocked)
        {
            bobPhase += Time.deltaTime * bobSpeed;
            pickedFlower.rectTransform.anchoredPosition = current.InspectionPosition
                + Vector2.up * (Mathf.Sin(bobPhase) * bobAmplitude);
        }
        bool allowField = CanInspect && !complete;
        if (fieldWasAvailable && !allowField)
        {
            foreach (FlowerSpot flower in flowers) flower.ClearHover();
        }
        fieldWasAvailable = allowField;
    }

    private void LateUpdate()
    {
        if (Pointer.current == null || Screen.width <= 0 || Screen.height <= 0) return;
        Vector2 cursor = Pointer.current.position.ReadValue();
        PanField(new Vector2(cursor.x / Screen.width, cursor.y / Screen.height), Time.unscaledDeltaTime);
    }

    private void PanField(Vector2 cursor, float deltaTime)
    {
        if (!CanInspect || complete) return;

        // Match MouseCameraPan's cursor mapping, moving the saved field beneath the viewport.
        float x = Mathf.InverseLerp(edgeThreshold, 1f - edgeThreshold, cursor.x);
        float y = Mathf.InverseLerp(edgeThreshold, 1f - edgeThreshold, cursor.y);
        Vector2 travel = new Vector2(
            Mathf.Max(0f, field.rect.width - fieldViewport.rect.width),
            Mathf.Max(0f, field.rect.height - fieldViewport.rect.height));
        // With a top-anchored field, zero vertical offset shows the top of the map.
        Vector2 target = new Vector2(-travel.x * x, travel.y * (1f - y));
        // Preserve the mermaid's smoothing at 60 fps without changing speed with frame rate.
        float blend = 1f - Mathf.Pow(1f - panSmoothing, Mathf.Max(0f, deltaTime) * 60f);
        Vector2 position = Vector2.Lerp(field.anchoredPosition, target, blend);
        field.anchoredPosition = new Vector2(
            Mathf.Clamp(position.x, -travel.x, 0f), Mathf.Clamp(position.y, 0f, travel.y));
    }

    public void Inspect(FlowerSpot flower)
    {
        int index = Array.IndexOf(flowers, flower);
        if (!CanInspect || index < 0 || IsResolved(index) || flower.PickedSprite == null)
            return;

        current = flower;
        bobPhase = 0f;
        foreach (FlowerSpot spot in flowers) spot.ClearHover();
        pickedFlower.sprite = flower.PickedSprite;
        pickedFlower.rectTransform.sizeDelta = flower.InspectionSize;
        pickedFlower.rectTransform.anchoredPosition = flower.InspectionPosition;
        inspectionPanel.SetActive(true);
        keepButton.interactable = true;
        discardButton.interactable = true;
        ClearSelection();
    }

    public void Keep() => Resolve(true);
    public void Discard() => Resolve(false);

    private void Resolve(bool keep)
    {
        if (current == null || resolving || InputBlocked)
            return;

        int index = Array.IndexOf(flowers, current);
        if (index < 0 || IsResolved(index))
            return;

        resolving = true;
        keepButton.interactable = false;
        discardButton.interactable = false;
        GameState.Set(ResolvedKey, GameState.Get<int>(ResolvedKey) | (1 << index));
        if (keep)
            GameState.Increment(KeptKey, current.FlowerCount);

        int sanity = ChoiceSanity(current.Healthy, keep);
        if (sanity != 0)
        {
            // Apply normal scoring, but play this minigame's sanity feedback at half volume.
            MessageBus.Instance.Publish("PlusSanity", sanity, true);
            AudioManager.Instance.PlaySoundWithRandomPitch(
                sanity > 0 ? "sanity_gain" : "sanity_loss", 1f, 1f, 0.5f);
        }
        AudioClip sound = keep ? keepSound : discardSound;
        if (sound != null) choiceAudio.PlayOneShot(sound);
        StartCoroutine(FinishChoice(sanity, (RectTransform)(keep ? keepButton : discardButton).transform));
    }

    public static int ChoiceSanity(bool healthy, bool keep)
    {
        return keep ? (healthy ? 1 : -1) : (healthy ? -1 : 0);
    }

    private IEnumerator FinishChoice(int sanity, RectTransform anchor)
    {
        if (sanity != 0)
            yield return SanityPopupAnimation.Play(feedbackLabel,
                sanity > 0 ? "+SANITY" : "-SANITY", sanity > 0 ? Color.green : Color.purple,
                anchor, popupYOffset, popupRise, popupDuration);
        else
            yield return new WaitForSeconds(feedbackDuration);
        // Don't uncover the field behind a paused menu.
        while (InputBlocked) yield return null;
        current = null;
        resolving = false;
        inspectionPanel.SetActive(false);
        feedbackLabel.gameObject.SetActive(false);
        ClearSelection();
        RefreshField();
    }

    private bool IsResolved(int index) => (GameState.Get<int>(ResolvedKey) & (1 << index)) != 0;

    private void RefreshField()
    {
        int sorted = 0;
        for (int i = 0; i < flowers.Length; i++)
        {
            bool resolved = IsResolved(i);
            flowers[i].gameObject.SetActive(!resolved);
            if (resolved) sorted++;
        }
        progressLabel.text = $"{sorted} / {flowers.Length} picked";
        complete = flowers.Length > 0 && sorted == flowers.Length;
        if (complete) GameState.Set(CompleteKey, true);
    }

    private static void ClearSelection()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnDestroy()
    {
        GameState.Set("minigame_open", false);
        CustomCursor.SetCursorToNormal();
    }
}

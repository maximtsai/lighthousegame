using System.Collections;
using UnityEngine;

/// <summary>
/// Day-gated map treasure hotspot. Shows a sparkle in the world; on click,
/// opens an item inspect popup and plays dialogue via DialogueManager.
/// Optional revealSprite enables two-part inspect (glow click + fade).
/// </summary>
[RequireComponent(typeof(InteractableObject))]
public class TreasureSpot : MonoBehaviour
{
    [Header("Treasure")]
    [SerializeField] private string treasureId = "boot";
    [SerializeField] private Sprite treasureSprite;
    [Tooltip("If set, inspect is two-part: closed sprite, click to fade, then this open sprite + dialogue.")]
    [SerializeField] private Sprite revealSprite;
    [Tooltip("Path under Resources/ScriptableObjects/Dialogues/ (e.g. outdoors/boot)")]
    [SerializeField] private string dialoguePath = "outdoors/boot";
    [SerializeField] private Dialogue dialogueOverride;

    [Header("Day window (inclusive)")]
    [SerializeField] private int startDay = 1;
    [SerializeField] private int endDay = 1;

    [Header("Behavior")]
    [SerializeField] private bool hideAfterFound = true;
    [SerializeField] private bool markFoundOnInspect = true;
    [Tooltip("Only show after the burial mound is covered (hill on top).")]
    [SerializeField] private bool requireMoundCovered = false;
    [Tooltip("World-space Y offset for the inspect popup (e.g. 0.2 = 20px at PPU 100).")]
    [SerializeField] private float inspectYOffset = 0f;

    private bool inspecting;
    private MessageBus.SubscriptionHandle burialHandle;

    private string FoundKey => $"treasure_{treasureId}_found";

    void Awake()
    {
        InteractableObject interactable = GetComponent<InteractableObject>();
        if (interactable != null)
            interactable.AddClickListener(Inspect);

        if (requireMoundCovered)
            burialHandle = MessageBus.Instance.Subscribe("BurialMoundCovered", OnBurialMoundCovered, this);
    }

    void Start()
    {
        ApplyVisibility();
    }

    void OnDestroy()
    {
        burialHandle?.Unsubscribe();
    }

    private void OnBurialMoundCovered(object[] args)
    {
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        bool shouldShow = !IsFound() && IsAvailableToday() && IsMoundRequirementMet();
        SetSpotVisible(shouldShow);
    }

    private bool IsMoundRequirementMet()
    {
        if (!requireMoundCovered)
            return true;

        return GameState.Get<bool>("has_buried", false)
            && !GameState.Get<bool>("do_burial", false)
            && !GameState.Get<bool>("grave_revealed", false);
    }

    private void SetSpotVisible(bool visible)
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = visible;

        InteractableObject interactable = GetComponent<InteractableObject>();
        if (interactable != null)
            interactable.enabled = visible;

        foreach (Transform child in transform)
            child.gameObject.SetActive(visible);

        if (!visible && IsFound() && hideAfterFound)
            gameObject.SetActive(false);
    }

    private bool IsAvailableToday()
    {
        int day = GameState.Get<int>("day", 1);
        return day >= startDay && day <= endDay;
    }

    private bool IsFound()
    {
        return GameState.Get<bool>(FoundKey, false);
    }

    public void Inspect()
    {
        if (inspecting) return;
        if (!IsAvailableToday()) return;
        if (!IsMoundRequirementMet()) return;
        if (IsFound() && hideAfterFound) return;
        if (DialogueManager.DialogueIsOpen()) return;
        if (GameState.Get<bool>("minigame_open", false)) return;
        if (GameState.Get<bool>("treasure_inspect_open", false)) return;

        inspecting = true;

        if (revealSprite != null)
        {
            TreasureInspectUI.ShowTwoPart(treasureSprite, revealSprite, BeginDialogue, inspectYOffset);
        }
        else
        {
            TreasureInspectUI.Show(treasureSprite, inspectYOffset);
            BeginDialogue();
        }
    }

    private void BeginDialogue()
    {
        Dialogue dialogue = ResolveDialogue();
        if (dialogue != null)
            DialogueManager.ShowDialogue(dialogue);
        else
            DialogueManager.ShowDialogueFromText(new string[] { "..." });

        StartCoroutine(WaitForDialogueThenFinish());
    }

    private Dialogue ResolveDialogue()
    {
        if (dialogueOverride != null)
            return dialogueOverride;

        if (string.IsNullOrEmpty(dialoguePath))
            return null;

        return Resources.Load<Dialogue>("ScriptableObjects/Dialogues/" + dialoguePath);
    }

    private IEnumerator WaitForDialogueThenFinish()
    {
        yield return null;
        while (DialogueManager.DialogueIsOpen())
            yield return null;

        TreasureInspectUI.Hide();

        if (markFoundOnInspect)
            GameState.Set(FoundKey, true);

        inspecting = false;

        if (hideAfterFound)
            gameObject.SetActive(false);
    }
}

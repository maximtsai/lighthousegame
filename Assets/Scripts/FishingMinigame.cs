using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishingMinigame : MonoBehaviour
{
    [System.Serializable]
    public class FishItem
    {
        public string itemName;
        public Sprite sprite;
        public bool keepIsCorrect; // true = must keep (fish), false = must discard (coral)
        public string wrongActionDialogue;
    }

    [Header("Sequence")]
    // Day 1: coral (discard), then fish (keep). Can vary by day later.
    [SerializeField] private List<FishItem> sequence = new List<FishItem>();

    [Header("UI References")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private Image itemImage;

    [Header("Audio / Helpers")]
    [SerializeField] private MiscObjectClick miscObjectClick;
    [SerializeField] private AudioClip keepSound;
    [SerializeField] private AudioClip discardSound;

    [Header("Testing")]
    // Editor-only: tick this to skip the lighthouse_fixed / gathered_fish gate
    // so you can open the minigame straight from PierScene. Leave OFF for builds.
    [SerializeField] private bool testModeSkipRequirements = false;

    private int currentIndex = 0;

    void Awake()
    {
        // On scene load the minigame is never running; clear the flag so a
        // stuck "minigame_open" (e.g. from disabled domain reload, or exiting
        // play mid-minigame) can't block clicks and navigation.
        GameState.Set("minigame_open", false);

        if (minigamePanel != null)
            minigamePanel.SetActive(false);
    }

    // Called by the net's on_click in place of MiscObjectClick.GatherFish
    public void TryStartFishing()
    {
        if (!testModeSkipRequirements)
        {
            if (GameState.Get<bool>("gathered_fish", false))
            {
                DialogueManager.ShowDialogue(miscObjectClick.getDialogue("dock/gather_fish_done"));
                return;
            }

            if (!GameState.Get<bool>("lighthouse_fixed", false))
            {
                DialogueManager.ShowDialogue(miscObjectClick.getDialogue("dock/gather_fish_not_yet"));
                return;
            }
        }

        currentIndex = 0;
        GameState.Set("minigame_open", true);
        minigamePanel.SetActive(true);
        ShowCurrentItem();
    }

    private void ShowCurrentItem()
    {
        if (currentIndex >= sequence.Count)
        {
            FinishMinigame();
            return;
        }

        Sprite sprite = sequence[currentIndex].sprite;
        itemImage.sprite = sprite;
        itemImage.enabled = sprite != null;
    }

    public void OnKeep()
    {
        if (currentIndex >= sequence.Count) return;

        FishItem item = sequence[currentIndex];
        if (item.keepIsCorrect)
        {
            if (keepSound != null) miscObjectClick.PlaySound(keepSound);
            currentIndex++;
            ShowCurrentItem();
        }
        else
        {
            // can't keep this one (coral)
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue(item.wrongActionDialogue));
        }
    }

    public void OnDiscard()
    {
        if (currentIndex >= sequence.Count) return;

        FishItem item = sequence[currentIndex];
        if (!item.keepIsCorrect)
        {
            if (discardSound != null) miscObjectClick.PlaySound(discardSound);
            currentIndex++;
            ShowCurrentItem();
        }
        else
        {
            // need this one, can't throw it back (fish)
            DialogueManager.ShowDialogue(miscObjectClick.getDialogue(item.wrongActionDialogue));
        }
    }

    private void FinishMinigame()
    {
        GameState.Set("minigame_open", false);
        minigamePanel.SetActive(false);

        // same payoff as MiscObjectClick.GatherFish
        GameState.Set("gathered_fish", true);
        GameState.Set("hungry", true);
        GameState.Set("near_nighttime", true);

        MessageBus.Instance.Publish("CompleteTask", "task_fish");
        DialogueManager.ShowDialogue(miscObjectClick.getDialogue("dock/gather_fish"));
    }
}

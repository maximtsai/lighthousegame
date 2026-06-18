using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishingMinigame : MonoBehaviour
{
    [System.Serializable]
    public class CatchItem
    {
        public string itemName;
        public Sprite sprite;
        public bool isFish;
    }

    [Header("Catch pool")]
    [SerializeField] private List<CatchItem> junkPool = new List<CatchItem>();
    [SerializeField] private CatchItem fish;
    [SerializeField] private int openingJunkCount = 4;

    [Header("UI References")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private Image itemImage;

    [Header("Sanity popup")]
    [SerializeField] private TextMeshProUGUI sanityPopup;
    [SerializeField] private RectTransform keepButtonRect;
    [SerializeField] private RectTransform discardButtonRect;
    [SerializeField] private float popupYOffset = 80f;
    [SerializeField] private float popupRise = 60f;
    [SerializeField] private float popupDuration = 1f;

    [Header("Audio / Helpers")]
    [SerializeField] private MiscObjectClick miscObjectClick;
    [SerializeField] private AudioClip keepSound;
    [SerializeField] private AudioClip discardSound;

    [Header("Testing")]
    [SerializeField] private bool testModeSkipRequirements = false;

    private CatchItem currentItem;
    private int openingJunkRemaining;
    private bool loopPhase;
    private bool finishing;
    private Coroutine popupRoutine;

    void Awake()
    {
        GameState.Set("minigame_open", false);

        if (minigamePanel != null)
            minigamePanel.SetActive(false);

        if (itemImage != null)
            itemImage.preserveAspect = true;

        if (sanityPopup != null)
            sanityPopup.gameObject.SetActive(false);
    }

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

        openingJunkRemaining = openingJunkCount;
        loopPhase = false;
        finishing = false;
        GameState.Set("minigame_open", true);
        minigamePanel.SetActive(true);
        ShowRandomJunk();
    }

    private void ShowItem(CatchItem item)
    {
        currentItem = item;
        itemImage.sprite = item.sprite;
        itemImage.enabled = item.sprite != null;
    }

    private void ShowRandomJunk()
    {
        if (junkPool.Count == 0)
        {
            Debug.LogWarning("FishingMinigame: junk pool is empty.");
            return;
        }

        ShowItem(junkPool[Random.Range(0, junkPool.Count)]);
    }

    private void ShowFish()
    {
        ShowItem(fish);
    }

    // After the opening junk, or after discarding a fish in the loop.
    private void ShowRandomLoopItem()
    {
        int pick = Random.Range(0, junkPool.Count + 1);
        if (pick == junkPool.Count)
            ShowFish();
        else
            ShowItem(junkPool[pick]);
    }

    public void OnKeep()
    {
        if (finishing || currentItem == null) return;

        if (currentItem.isFish)
        {
            if (keepSound != null) miscObjectClick.PlaySound(keepSound);
            ChangeSanity(1, keepButtonRect);
            finishing = true;
            StartCoroutine(FinishAfter(popupDuration));
            return;
        }

        // Keeping junk costs sanity; stay on the same item.
        ChangeSanity(-1, keepButtonRect);
    }

    public void OnDiscard()
    {
        if (finishing || currentItem == null) return;

        if (!loopPhase)
        {
            if (currentItem.isFish) return;

            if (discardSound != null) miscObjectClick.PlaySound(discardSound);
            openingJunkRemaining--;

            if (openingJunkRemaining > 0)
                ShowRandomJunk();
            else
            {
                loopPhase = true;
                ShowFish();
            }
            return;
        }

        // Loop phase: only ends when the player keeps a fish.
        if (currentItem.isFish)
        {
            ChangeSanity(-1, discardButtonRect);
            ShowRandomLoopItem();
        }
        else
        {
            if (discardSound != null) miscObjectClick.PlaySound(discardSound);
            ShowRandomLoopItem();
        }
    }

    // Applies the sanity change through the same MessageBus path the rest of the
    // game uses, and shows a self-contained popup above the given button.
    private void ChangeSanity(int amount, RectTransform anchor)
    {
        MessageBus.Instance.Publish("PlusSanity", amount);

        string label = amount >= 0 ? "+SANITY" : "-SANITY";
        Color color = amount >= 0 ? Color.green : Color.purple;
        ShowSanityPopup(label, color, anchor);
    }

    private void ShowSanityPopup(string text, Color color, RectTransform anchor)
    {
        if (sanityPopup == null) return;

        if (popupRoutine != null) StopCoroutine(popupRoutine);
        popupRoutine = StartCoroutine(SanityPopupRoutine(text, color, anchor));
    }

    private IEnumerator SanityPopupRoutine(string text, Color color, RectTransform anchor)
    {
        sanityPopup.gameObject.SetActive(true);
        sanityPopup.text = text;
        color.a = 1f;
        sanityPopup.color = color;

        RectTransform rt = sanityPopup.rectTransform;
        // Use world position so the popup lines up over the button regardless of
        // differing anchor/pivot settings between the popup and the buttons.
        Vector3 basePos = anchor != null ? anchor.position : rt.position;
        Vector3 start = basePos + new Vector3(0f, popupYOffset, 0f);
        Vector3 end = start + new Vector3(0f, popupRise, 0f);

        float elapsed = 0f;
        while (elapsed < popupDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popupDuration;
            rt.position = Vector3.Lerp(start, end, t);
            color.a = Mathf.Lerp(1f, 0f, t);
            sanityPopup.color = color;
            yield return null;
        }

        sanityPopup.gameObject.SetActive(false);
        popupRoutine = null;
    }

    private IEnumerator FinishAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        FinishMinigame();
    }

    private void FinishMinigame()
    {
        GameState.Set("minigame_open", false);
        minigamePanel.SetActive(false);

        GameState.Set("gathered_fish", true);
        GameState.Set("hungry", true);
        GameState.Set("near_nighttime", true);

        MessageBus.Instance.Publish("CompleteTask", "task_fish");
        DialogueManager.ShowDialogue(miscObjectClick.getDialogue("dock/gather_fish"));
    }
}

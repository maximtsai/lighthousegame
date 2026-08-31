using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FishingMinigame : MonoBehaviour
{
    [System.Serializable]
    public class CatchItem
    {
        public string itemName;
        public Sprite sprite;         // frame 1 / default
        public Sprite revealSprite;   // frame 2; if set, must click reveal before keep/discard
        public bool isGoodFish;
        [Tooltip("Optional yellow outline shown on hover while waiting to reveal frame 2.")]
        public Sprite hoverOutlineSprite;
        [Tooltip("UI scale for this catch (1 = normal). Use 2 for small items like Lone Worm.")]
        public float displayScale = 1f;

        public bool IsTwoFrame => revealSprite != null;
    }

    [System.Serializable]
    public class DayCatch
    {
        public int day;
        // Day's catch pool. Opening pass: all junk first, then good fish last.
        // After discarding the good fish: random reshuffles until Keep.
        public List<CatchItem> items = new List<CatchItem>();
    }

    [Header("Per-day catch pools")]
    [SerializeField] private List<DayCatch> dayCatches = new List<DayCatch>();
    [SerializeField] private DayCatch fallbackCatch;

    [Header("UI References")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private Image itemImage;

    [Header("Two-frame reveal")]
    // Outline overlay on frame 1. Click → fade → frame 2 → keep/discard.
    [SerializeField] private Button revealButton;
    [SerializeField] private Image revealFadeOverlay;
    [SerializeField] private float revealFadeDuration = 0.15f;

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
    [SerializeField] private float keepSoundVolume = 1.2f;
    [SerializeField] private float sanityGainVolume = 1.35f;
    [SerializeField] private float discardPitchMin = 0.88f;
    [SerializeField] private float discardPitchMax = 1.12f;
    [SerializeField] private int discardAudioPoolSize = 4;
    [SerializeField] private AudioClip pierAmbientLoop;
    [SerializeField] private AudioClip pierRainAmbientLoop;
    [SerializeField] private float pierAmbientVolume = 0.12f;
    [SerializeField] private float pierAmbientFadeDuration = 0.6f;

    [Header("Button pop")]
    [SerializeField] private float buttonPopScale = 0.92f;
    [SerializeField] private float buttonPopDuration = 0.1f;

    [Header("Item bob")]
    [SerializeField] private float bobAmplitude = 5f;
    [SerializeField] private float bobSpeed = 1.4f;

    [Header("Testing")]
    [SerializeField] private bool testModeSkipRequirements = false;
    [SerializeField] private int testModeForceDay = 0;

    private CatchItem currentItem;
    private bool finishing;
    private bool awaitingReveal;
    private bool revealing;
    private bool choiceLocked;
    private Coroutine popupRoutine;
    private Coroutine bobRoutine;
    private Image hoverOutlineImage;
    private AudioSource keepAudioSource;
    private AudioSource[] discardAudioPool;
    private int discardPoolIndex;
    private AudioSource ambientAudioSource;
    private Coroutine ambientFadeRoutine;
    private Coroutine keepPopRoutine;
    private Coroutine discardPopRoutine;
    private Vector2 itemBaseAnchoredPos;
    private Vector3 keepButtonBaseScale = Vector3.one;
    private Vector3 discardButtonBaseScale = Vector3.one;
    private AudioClip sanityGainClip;

    private List<CatchItem> activeItems;
    // Current presentation bag (opening: junk→fish, or random loop after fish discarded).
    private readonly List<CatchItem> playBag = new List<CatchItem>();
    private int playIndex;
    // False = opening pass (junk first, good fish last). True = random loop until Keep.
    private bool randomLoopPhase;

    void Awake()
    {
        GameState.Set("minigame_open", false);

        if (minigamePanel != null)
            minigamePanel.SetActive(false);

        if (itemImage != null)
        {
            itemImage.preserveAspect = true;
            // Catch art must not steal hover/clicks from Keep/Discard.
            itemImage.raycastTarget = false;
            itemBaseAnchoredPos = itemImage.rectTransform.anchoredPosition;
        }

        keepAudioSource = gameObject.AddComponent<AudioSource>();
        keepAudioSource.playOnAwake = false;
        sanityGainClip = Resources.Load<AudioClip>("Audio/sanity_gain");

        int poolSize = Mathf.Max(1, discardAudioPoolSize);
        discardAudioPool = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            discardAudioPool[i] = gameObject.AddComponent<AudioSource>();
            discardAudioPool[i].playOnAwake = false;
        }

        EnsureHoverOutlineImage();

        if (sanityPopup != null)
            sanityPopup.gameObject.SetActive(false);

        if (revealButton != null)
        {
            revealButton.onClick.AddListener(OnRevealClicked);
            revealButton.gameObject.SetActive(false);
            WireRevealHover(revealButton.gameObject);
        }

        if (revealFadeOverlay != null)
            revealFadeOverlay.gameObject.SetActive(false);

        ConfigureChoiceButton(keepButtonRect);
        ConfigureChoiceButton(discardButtonRect);

        if (keepButtonRect != null)
            keepButtonBaseScale = keepButtonRect.localScale;
        if (discardButtonRect != null)
            discardButtonBaseScale = discardButtonRect.localScale;

        ambientAudioSource = gameObject.AddComponent<AudioSource>();
        ambientAudioSource.playOnAwake = false;
        ambientAudioSource.loop = true;

        // Ensure Keep/Discard start visible (scene defaults).
        SetChoiceButtonsActive(true);
    }

    // Avoid sticky UI selection after a click — that was killing Discard's yellow hover
    // on the next item while Keep still looked fine.
    private static void ConfigureChoiceButton(RectTransform buttonRect)
    {
        if (buttonRect == null) return;
        Button button = buttonRect.GetComponent<Button>();
        if (button == null) return;

        // Fully qualify — this project also has a scene Navigation singleton.
        UnityEngine.UI.Navigation nav = button.navigation;
        nav.mode = UnityEngine.UI.Navigation.Mode.None;
        button.navigation = nav;

        ColorBlock colors = button.colors;
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
    }

    private static void ClearUiSelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
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

        activeItems = GetItemsForDay(GetCurrentDay());
        if (activeItems == null || activeItems.Count == 0)
        {
            Debug.LogWarning($"FishingMinigame: no catch pool configured for day {GetCurrentDay()}.");
            return;
        }

        finishing = false;
        choiceLocked = false;
        BuildOpeningBag();
        GameState.Set("minigame_open", true);
        minigamePanel.SetActive(true);
        StartPierAmbient();
        ShowNextItem();
    }

    private int GetCurrentDay()
    {
        if (testModeSkipRequirements && testModeForceDay > 0)
            return testModeForceDay;
        return GameState.Get<int>("day", 1);
    }

    private List<CatchItem> GetItemsForDay(int day)
    {
        foreach (DayCatch dc in dayCatches)
        {
            if (dc != null && dc.day == day)
                return dc.items;
        }
        return fallbackCatch != null ? fallbackCatch.items : null;
    }

    // Opening bag: every junk item first (Inspector order), good fish last.
    private void BuildOpeningBag()
    {
        playBag.Clear();
        playIndex = 0;
        randomLoopPhase = false;

        if (activeItems == null)
            return;

        foreach (CatchItem item in activeItems)
        {
            if (item != null && !item.isGoodFish)
                playBag.Add(item);
        }
        foreach (CatchItem item in activeItems)
        {
            if (item != null && item.isGoodFish)
                playBag.Add(item);
        }
    }

    // After discarding the good fish: same day's objects in a new random order.
    private void BuildRandomLoopBag()
    {
        playBag.Clear();
        playIndex = 0;
        randomLoopPhase = true;

        if (activeItems == null || activeItems.Count == 0)
            return;

        playBag.AddRange(activeItems);
        for (int i = playBag.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (playBag[i], playBag[j]) = (playBag[j], playBag[i]);
        }
    }

    private void ShowNextItem()
    {
        if (activeItems == null || activeItems.Count == 0)
            return;

        if (playBag.Count == 0 || playIndex >= playBag.Count)
        {
            // Opening finished without a Keep, or random bag emptied — keep looping randomly.
            BuildRandomLoopBag();
        }

        if (playBag.Count == 0)
            return;

        ShowItem(playBag[playIndex]);
        playIndex++;
    }

    private void ShowItem(CatchItem item)
    {
        currentItem = item;
        choiceLocked = false;
        awaitingReveal = false;
        revealing = false;

        itemImage.sprite = item.sprite;
        itemImage.enabled = item.sprite != null;
        float scale = item.displayScale > 0f ? item.displayScale : 1f;
        itemImage.rectTransform.localScale = Vector3.one * scale;
        SetHoverOutlineVisible(false);
        ClearUiSelection();
        StartItemBob();

        if (item.IsTwoFrame)
        {
            // Frame 1: hide Keep/Discard, show reveal hit area, wait for click.
            awaitingReveal = true;
            SetChoiceButtonsActive(false);
            if (revealButton != null)
            {
                // Invisible hit target — yellow outline overlays the item image on hover.
                Image revealImage = revealButton.GetComponent<Image>();
                if (revealImage != null)
                {
                    Color c = revealImage.color;
                    c.a = 0f;
                    revealImage.color = c;
                }
                revealButton.transition = Selectable.Transition.None;
                revealButton.gameObject.SetActive(true);
            }
        }
        else
        {
            // Normal items: Keep/Discard available immediately.
            if (revealButton != null)
                revealButton.gameObject.SetActive(false);
            SetChoiceButtonsActive(true);
        }
    }

    private void EnsureHoverOutlineImage()
    {
        if (hoverOutlineImage != null || itemImage == null)
            return;

        GameObject go = new GameObject("HoverOutline", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(itemImage.rectTransform, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;

        hoverOutlineImage = go.GetComponent<Image>();
        hoverOutlineImage.raycastTarget = false;
        hoverOutlineImage.preserveAspect = true;
        hoverOutlineImage.color = Color.white;
        go.SetActive(false);
    }

    private void SetHoverOutlineVisible(bool visible)
    {
        EnsureHoverOutlineImage();
        if (hoverOutlineImage == null)
            return;

        if (visible && currentItem != null && currentItem.hoverOutlineSprite != null)
        {
            hoverOutlineImage.sprite = currentItem.hoverOutlineSprite;
            hoverOutlineImage.enabled = true;
            hoverOutlineImage.gameObject.SetActive(true);
        }
        else
        {
            hoverOutlineImage.gameObject.SetActive(false);
        }
    }

    private void WireRevealHover(GameObject revealGo)
    {
        EventTrigger trigger = revealGo.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = revealGo.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => OnRevealHoverEnter());
        trigger.triggers.Add(enter);

        EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => OnRevealHoverExit());
        trigger.triggers.Add(exit);
    }

    private void OnRevealHoverEnter()
    {
        if (!awaitingReveal || revealing || currentItem == null) return;
        if (currentItem.hoverOutlineSprite == null) return;
        SetHoverOutlineVisible(true);
    }

    private void OnRevealHoverExit()
    {
        SetHoverOutlineVisible(false);
    }

    private void SetChoiceButtonsActive(bool active)
    {
        if (keepButtonRect != null)
            keepButtonRect.gameObject.SetActive(active);
        if (discardButtonRect != null)
            discardButtonRect.gameObject.SetActive(active);

        if (active)
        {
            SetChoiceButtonsInteractable(true);
            ClearUiSelection();
        }
    }

    private void SetChoiceButtonsInteractable(bool interactable)
    {
        if (keepButtonRect != null)
        {
            Button keep = keepButtonRect.GetComponent<Button>();
            if (keep != null) keep.interactable = interactable;
        }
        if (discardButtonRect != null)
        {
            Button discard = discardButtonRect.GetComponent<Button>();
            if (discard != null) discard.interactable = interactable;
        }
        if (revealButton != null)
            revealButton.interactable = interactable;
    }

    private void StartItemBob()
    {
        StopItemBob();
        if (itemImage == null)
            return;

        itemBaseAnchoredPos = itemImage.rectTransform.anchoredPosition;
        bobRoutine = StartCoroutine(ItemBobRoutine());
    }

    private void StopItemBob()
    {
        if (bobRoutine != null)
        {
            StopCoroutine(bobRoutine);
            bobRoutine = null;
        }

        if (itemImage != null)
            itemImage.rectTransform.anchoredPosition = itemBaseAnchoredPos;
    }

    private IEnumerator ItemBobRoutine()
    {
        float phase = Random.Range(0f, Mathf.PI * 2f);
        while (itemImage != null)
        {
            phase += Time.deltaTime * bobSpeed * Mathf.PI * 2f;
            float offset = Mathf.Sin(phase) * bobAmplitude;
            itemImage.rectTransform.anchoredPosition = itemBaseAnchoredPos + new Vector2(0f, offset);
            yield return null;
        }
    }

    private void PlayDiscardSound()
    {
        if (discardSound == null || discardAudioPool == null || discardAudioPool.Length == 0)
            return;

        AudioSource src = discardAudioPool[discardPoolIndex];
        discardPoolIndex = (discardPoolIndex + 1) % discardAudioPool.Length;
        src.pitch = Random.Range(discardPitchMin, discardPitchMax);
        src.PlayOneShot(discardSound);
    }

    private float PlayKeepSound()
    {
        if (keepSound == null || keepAudioSource == null)
            return 0f;

        keepAudioSource.PlayOneShot(keepSound, keepSoundVolume);
        return keepSound.length;
    }

    private void PlaySanityGainSound()
    {
        if (sanityGainClip == null || keepAudioSource == null)
            return;

        keepAudioSource.PlayOneShot(sanityGainClip, sanityGainVolume);
    }

    private void PlayKeepButtonPop()
    {
        if (keepPopRoutine != null)
            StopCoroutine(keepPopRoutine);
        keepPopRoutine = StartCoroutine(ButtonPopRoutine(keepButtonRect, keepButtonBaseScale));
    }

    private void PlayDiscardButtonPop()
    {
        if (discardPopRoutine != null)
            StopCoroutine(discardPopRoutine);
        discardPopRoutine = StartCoroutine(ButtonPopRoutine(discardButtonRect, discardButtonBaseScale));
    }

    private IEnumerator ButtonPopRoutine(RectTransform buttonRect, Vector3 baseScale)
    {
        if (buttonRect == null)
            yield break;

        Vector3 squashed = baseScale * buttonPopScale;
        float half = buttonPopDuration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            buttonRect.localScale = Vector3.Lerp(baseScale, squashed, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            float eased = 1f - (1f - t) * (1f - t);
            buttonRect.localScale = Vector3.Lerp(squashed, baseScale, eased);
            yield return null;
        }

        buttonRect.localScale = baseScale;
    }

    private void StartPierAmbient()
    {
        if (ambientAudioSource == null)
            return;

        bool isRaining = GameState.Get<bool>("thunderstorm", false);
        AudioClip clip = (isRaining && pierRainAmbientLoop != null) ? pierRainAmbientLoop : pierAmbientLoop;
        if (clip == null)
            return;

        if (ambientFadeRoutine != null)
            StopCoroutine(ambientFadeRoutine);

        ambientAudioSource.clip = clip;
        ambientAudioSource.volume = 0f;
        ambientAudioSource.Play();
        ambientFadeRoutine = StartCoroutine(FadePierAmbient(pierAmbientVolume, pierAmbientFadeDuration, stopWhenDone: false));
    }

    private void StopPierAmbient()
    {
        if (ambientAudioSource == null || !ambientAudioSource.isPlaying)
            return;

        if (ambientFadeRoutine != null)
            StopCoroutine(ambientFadeRoutine);

        ambientFadeRoutine = StartCoroutine(FadePierAmbient(0f, pierAmbientFadeDuration, stopWhenDone: true));
    }

    private IEnumerator FadePierAmbient(float targetVolume, float duration, bool stopWhenDone)
    {
        if (ambientAudioSource == null)
            yield break;

        float startVolume = ambientAudioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            ambientAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        ambientAudioSource.volume = targetVolume;

        if (stopWhenDone)
            ambientAudioSource.Stop();

        ambientFadeRoutine = null;
    }

    public void OnRevealClicked()
    {
        if (!awaitingReveal || revealing || currentItem == null) return;

        revealing = true;
        if (revealButton != null)
            revealButton.gameObject.SetActive(false);
        SetHoverOutlineVisible(false);
        StartCoroutine(RevealRoutine());
    }

    private IEnumerator RevealRoutine()
    {
        yield return StartCoroutine(FadeReveal(1f));

        if (currentItem != null && currentItem.revealSprite != null)
        {
            itemImage.sprite = currentItem.revealSprite;
            itemImage.enabled = true;
        }

        yield return StartCoroutine(FadeReveal(0f));

        awaitingReveal = false;
        revealing = false;
        SetChoiceButtonsActive(true);
    }

    private IEnumerator FadeReveal(float targetAlpha)
    {
        if (revealFadeOverlay == null)
            yield break;

        revealFadeOverlay.gameObject.SetActive(true);

        Color c = revealFadeOverlay.color;
        float startAlpha = c.a;
        float elapsed = 0f;

        while (elapsed < revealFadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / revealFadeDuration);
            revealFadeOverlay.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        revealFadeOverlay.color = c;

        if (Mathf.Approximately(targetAlpha, 0f))
            revealFadeOverlay.gameObject.SetActive(false);
    }

    public void OnKeep()
    {
        if (finishing || choiceLocked || awaitingReveal || revealing || currentItem == null) return;

        PlayKeepButtonPop();
        choiceLocked = true;
        StartCoroutine(KeepRoutine());
    }

    private IEnumerator KeepRoutine()
    {
        SetChoiceButtonsInteractable(false);

        float keepDuration = PlayKeepSound();
        if (keepDuration > 0f)
            yield return new WaitForSeconds(keepDuration * 0.7f);

        if (currentItem == null)
            yield break;

        if (currentItem.isGoodFish)
        {
            ChangeSanity(1, keepButtonRect);
            finishing = true;
            yield return new WaitForSeconds(popupDuration);
            FinishMinigame();
            yield break;
        }

        ChangeSanity(-1, keepButtonRect);
        yield return new WaitForSeconds(popupDuration);

        if (!finishing)
        {
            choiceLocked = false;
            SetChoiceButtonsInteractable(true);
            ShowNextItem();
        }
    }

    public void OnDiscard()
    {
        if (finishing || choiceLocked || awaitingReveal || revealing || currentItem == null) return;

        PlayDiscardButtonPop();
        PlayDiscardSound();

        if (currentItem.isGoodFish)
        {
            // Discarded the good fish (end of opening bag, or later): random loop
            // of this day's objects until they Keep it.
            choiceLocked = true;
            ChangeSanity(-1, discardButtonRect);
            BuildRandomLoopBag();
            StartCoroutine(AdvanceAfterSanity());
            return;
        }

        ShowNextItem();
    }

    private IEnumerator AdvanceAfterSanity()
    {
        yield return new WaitForSeconds(popupDuration);
        if (!finishing)
        {
            choiceLocked = false;
            SetChoiceButtonsInteractable(true);
            ShowNextItem();
        }
    }

    private void ChangeSanity(int amount, RectTransform anchor)
    {
        if (amount > 0)
        {
            // Louder gain SFX for fishing only; suppress the default PlusSanity sound.
            MessageBus.Instance.Publish("PlusSanity", amount, true);
            PlaySanityGainSound();
        }
        else
        {
            MessageBus.Instance.Publish("PlusSanity", amount);
        }

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

    private void FinishMinigame()
    {
        StopItemBob();
        StopPierAmbient();
        GameState.Set("minigame_open", false);
        if (revealButton != null)
            revealButton.gameObject.SetActive(false);
        SetHoverOutlineVisible(false);
        SetChoiceButtonsActive(true);
        minigamePanel.SetActive(false);

        GameState.Set("gathered_fish", true);
        GameState.Set("hungry", true);
        GameState.Set("near_nighttime", true);

        MessageBus.Instance.Publish("CompleteTask", "task_fish");
        DialogueManager.ShowDialogue(miscObjectClick.getDialogue("dock/gather_fish"));
    }
}

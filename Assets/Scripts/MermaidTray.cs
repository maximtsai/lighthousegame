using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The medical supply tray that slides up from the bottom between tasks.
//
// The tray and all four tools are drawn on the same shared canvas as each other, so every Image
// shares one anchored position and the tools land on the tray on their own. Only the little
// invisible hit buttons over each tool are placed individually.
public class MermaidTray : MonoBehaviour
{
    [Serializable]
    public class ToolButton
    {
        public MermaidTool tool;
        [Tooltip("The full-frame tool art.")]
        public Image art;
        [Tooltip("The small invisible hit rect sitting over it.")]
        public Button button;
    }

    [Header("Slide")]
    [SerializeField] private RectTransform trayRoot;
    [Tooltip("Y that puts the tray near the bottom of the screen.")]
    [SerializeField] private float shownY = -314f;
    [Tooltip("Y that parks it fully below the screen.")]
    [SerializeField] private float hiddenY = -480f;
    [SerializeField] private float slideDuration = 0.55f;

    [Header("Tools")]
    [Tooltip("Lit and greyed states come from each Button's own Color Tint block. This list only " +
             "decides which one is interactable.")]
    [SerializeField] private List<ToolButton> tools = new List<ToolButton>();

    [Header("Audio")]
    [SerializeField] private MiscObjectClick miscObjectClick;
    [SerializeField] private AudioClip traySound;

    [Header("State")]
    [SerializeField] private bool shown;

    private Coroutine slideRoutine;

    public bool IsShown => shown;

    // Raise the tray with only this tool lit and clickable.
    public void Show(MermaidTool activeTool)
    {
        SetToolStates(activeTool, false);
        shown = true;

        if (miscObjectClick != null && traySound != null) miscObjectClick.PlaySound(traySound);

        // Panning slows to a fifth while a minigame is open, which is a nice beat while you're
        // choosing a tool, but it has to be off again the moment the tray drops.
        GameState.Set("minigame_open", true);

        StartSlide(shownY, () => SetToolStates(activeTool, true));
    }

    // Drop the tray and hand control back to the body.
    public void Hide()
    {
        shown = false;
        SetToolStates(MermaidTool.None, false);
        ClearUiSelection();

        if (miscObjectClick != null && traySound != null) miscObjectClick.PlaySound(traySound);

        StartSlide(hiddenY, () => GameState.Set("minigame_open", false));
    }

    private void SetToolStates(MermaidTool activeTool, bool interactable)
    {
        foreach (ToolButton entry in tools)
        {
            if (entry == null || entry.button == null) continue;

            entry.button.interactable = entry.tool == activeTool && interactable;

            // Fully qualify, this project also has a scene Navigation singleton.
            UnityEngine.UI.Navigation nav = entry.button.navigation;
            nav.mode = UnityEngine.UI.Navigation.Mode.None;
            entry.button.navigation = nav;

            // Otherwise sticky selection leaves a tool looking hovered after you click it.
            ColorBlock colors = entry.button.colors;
            colors.selectedColor = colors.highlightedColor;
            entry.button.colors = colors;
        }
    }

    private static void ClearUiSelection()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    private void StartSlide(float targetY, Action onComplete)
    {
        if (slideRoutine != null) StopCoroutine(slideRoutine);
        slideRoutine = StartCoroutine(SlideTo(targetY, onComplete));
    }

    private IEnumerator SlideTo(float targetY, Action onComplete)
    {
        if (trayRoot == null) yield break;

        Vector2 start = trayRoot.anchoredPosition;
        Vector2 end = new Vector2(start.x, targetY);

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            t = t * t * (3f - 2f * t);
            trayRoot.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        trayRoot.anchoredPosition = end;
        slideRoutine = null;
        onComplete?.Invoke();
    }
}

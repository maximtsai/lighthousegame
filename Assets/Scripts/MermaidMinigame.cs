using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Runs the mermaid cleaning minigame.
//
// Four phases go in order, each announced by the task bar:
//
//     1. Clean Barnacles    gloves       14 sites (the stomach gash has no barnacle)
//     2. Disinfect Wounds   alcohol      15 sites
//     3. Stitch Wounds      fish bones    4 sites, 10 clicks (chains of 2 and 3)
//     4. Bandage Wounds     bandages     10 patches
//
// Each phase raises the tray with only its own tool lit. Pick it up and the tray drops, that
// phase's targets become clickable, and once they're all treated the task completes and the
// next phase begins.
//
// This is the only script that knows phases exist. The targets just know their own sprites.
public class MermaidMinigame : MonoBehaviour
{
    [System.Serializable]
    public class Phase
    {
        [Tooltip("Path under Resources/ScriptableObjects/Tasks/, e.g. mermaid/clean_barnacles")]
        public string taskResource;
        [Tooltip("The id field inside that Task asset, used to complete it.")]
        public string taskId;
        public MermaidTool tool;
        [Tooltip("Plays each time a target is treated in this phase.")]
        public AudioClip sound;
        [Tooltip("If set, these rotate instead of sound. Used for the barnacle rip variants.")]
        public AudioClip[] sounds;
        [Tooltip("Everything this phase's tool works on.")]
        public List<MermaidTarget> targets = new List<MermaidTarget>();
    }

    [Header("Phases (run in order)")]
    [SerializeField] private List<Phase> phases = new List<Phase>();

    [Header("Every target in the scene")]
    [Tooltip("Used to switch colliders on and off per phase so only the current one is clickable.")]
    [SerializeField] private List<MermaidTarget> allTargets = new List<MermaidTarget>();

    [Header("References")]
    [SerializeField] private MermaidTray tray;
    [SerializeField] private MiscObjectClick miscObjectClick;
    [SerializeField] private MermaidAlcoholPour alcoholPour;

    [Header("Timing")]
    [Tooltip("Lets the task bar animate in before the tray comes up.")]
    [SerializeField] private float startDelay = 0.9f;
    [Tooltip("Breath between finishing a phase and the next task appearing.")]
    [SerializeField] private float phaseGap = 0.9f;

    [Header("Finish")]
    [Tooltip("Optional path under Resources/ScriptableObjects/Dialogues/, e.g. mermaid/patched_up")]
    [SerializeField] private string finishDialogue = "";

    // Live state while the minigame runs. stepsDone/stepsTotal drive the counter after the
    // task text.
    private int phaseIndex = -1;
    private bool toolInHand;
    private bool finished;
    private int stepsDone;
    private int stepsTotal;
    private int soundRotate;
    private AudioClip[] shuffledSounds;

    private Phase CurrentPhase =>
        phaseIndex >= 0 && phaseIndex < phases.Count ? phases[phaseIndex] : null;

    private MermaidTool CurrentTool => CurrentPhase != null ? CurrentPhase.tool : MermaidTool.None;

    void Start()
    {
        GameState.Set("minigame_open", false);

        // Queue all four up front. Only the top one shows, and completing it reveals the next.
        foreach (Phase phase in phases)
        {
            if (!string.IsNullOrEmpty(phase.taskResource))
                MessageBus.Instance.Publish("AddTaskString", phase.taskResource);
        }

        StartCoroutine(BeginAfterDelay(0, startDelay));
    }

    // Hooked up to the tool buttons on the tray.
    public void ClickGloves() => ChooseTool(MermaidTool.Gloves);
    public void ClickAlcohol() => ChooseTool(MermaidTool.Alcohol);
    public void ClickFishBones() => ChooseTool(MermaidTool.FishBones);
    public void ClickBandages() => ChooseTool(MermaidTool.Bandages);

    private void ChooseTool(MermaidTool tool)
    {
        if (finished || toolInHand) return;
        if (tray == null || !tray.IsShown) return;

        // The other three are greyed out and not interactable, so this shouldn't ever trip. It
        // just stops a stray click from arming the wrong phase.
        if (tool != CurrentTool) return;

        toolInHand = true;
        tray.Hide();
    }

    // Whether this target can be acted on right now. Gates the click and the hover tint both,
    // which is why a target with no work left feels inert rather than broken.
    public bool IsTargetLive(MermaidTarget target)
    {
        if (finished || !toolInHand || target == null) return false;
        if (tray != null && tray.IsShown) return false;

        Phase phase = CurrentPhase;
        if (phase == null || !phase.targets.Contains(target)) return false;

        return target.CanApply(phase.tool);
    }

    public void OnTargetClicked(MermaidTarget target)
    {
        if (!IsTargetLive(target)) return;

        Phase phase = CurrentPhase;
        target.Apply(phase.tool);

        if (phase.tool == MermaidTool.Alcohol && alcoholPour != null) alcoholPour.Play();

        AudioClip clip = NextPhaseSound(phase);
        if (miscObjectClick != null && clip != null) miscObjectClick.PlaySound(clip);

        stepsDone++;
        MessageBus.Instance.Publish("SetTaskProgress", stepsDone, stepsTotal);

        if (!target.CanApply(phase.tool)) target.ClearHover();

        if (IsPhaseComplete(phase)) CompletePhase(phase);
    }

    private bool IsPhaseComplete(Phase phase)
    {
        foreach (MermaidTarget target in phase.targets)
        {
            if (target != null && target.CanApply(phase.tool)) return false;
        }
        return true;
    }

    private IEnumerator BeginAfterDelay(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        BeginPhase(index);
    }

    private void BeginPhase(int index)
    {
        phaseIndex = index;
        toolInHand = false;

        Phase phase = CurrentPhase;
        if (phase == null)
        {
            Finish();
            return;
        }

        // Switch colliders rather than filtering clicks later, so the bandages can never steal
        // a click from the wound sitting underneath them.
        foreach (MermaidTarget target in allTargets)
        {
            if (target == null) continue;
            target.ClearHover();
            if (target.BodyCollider != null)
                target.BodyCollider.enabled = phase.targets.Contains(target);
        }

        ShufflePhaseSounds(phase);

        // The counter counts clicks, so a three-stage suture chain contributes three.
        stepsDone = 0;
        stepsTotal = 0;
        foreach (MermaidTarget target in phase.targets)
        {
            if (target != null) stepsTotal += target.RemainingSteps(phase.tool);
        }
        MessageBus.Instance.Publish("SetTaskProgress", stepsDone, stepsTotal);

        if (tray != null) tray.Show(phase.tool);
    }

    private void CompletePhase(Phase phase)
    {
        toolInHand = false;

        foreach (MermaidTarget target in allTargets)
        {
            if (target == null) continue;
            target.ClearHover();
            if (target.BodyCollider != null) target.BodyCollider.enabled = false;
        }

        if (!string.IsNullOrEmpty(phase.taskId))
            MessageBus.Instance.Publish("CompleteTask", phase.taskId);

        int next = phaseIndex + 1;
        if (next < phases.Count) StartCoroutine(BeginAfterDelay(next, phaseGap));
        else StartCoroutine(FinishAfterDelay(phaseGap));
    }

    private IEnumerator FinishAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Finish();
    }

    private void Finish()
    {
        if (finished) return;
        finished = true;
        phaseIndex = phases.Count;

        GameState.Set("minigame_open", false);
        GameState.Set("mermaid_treated", true);
        MessageBus.Instance.Publish("ClearTaskProgress");

        if (!string.IsNullOrEmpty(finishDialogue) && miscObjectClick != null)
        {
            Dialogue dialogue = miscObjectClick.getDialogue(finishDialogue);
            if (dialogue != null) DialogueManager.ShowDialogue(dialogue);
        }
    }

    // Cycle through a shuffled copy of the phase's variant clips so the four barnacle rips
    // don't play in the same order twice in a row. Falls back to the single sound field.
    private void ShufflePhaseSounds(Phase phase)
    {
        soundRotate = 0;
        shuffledSounds = null;
        if (phase == null || phase.sounds == null || phase.sounds.Length == 0) return;

        shuffledSounds = (AudioClip[])phase.sounds.Clone();
        for (int i = shuffledSounds.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            AudioClip swap = shuffledSounds[i];
            shuffledSounds[i] = shuffledSounds[j];
            shuffledSounds[j] = swap;
        }
    }

    private AudioClip NextPhaseSound(Phase phase)
    {
        if (shuffledSounds != null && shuffledSounds.Length > 0)
        {
            AudioClip clip = shuffledSounds[soundRotate % shuffledSounds.Length];
            soundRotate++;
            if (soundRotate % shuffledSounds.Length == 0) ShufflePhaseSounds(phase);
            return clip;
        }

        return phase != null ? phase.sound : null;
    }
}

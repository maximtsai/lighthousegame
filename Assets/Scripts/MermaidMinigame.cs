using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Runs one day's mermaid cleaning minigame as a list of phases, each announced by the task bar.
// There's one of these per day in MermaidScene, and MermaidDaySelector switches on the right one.
//
// Day 4:  clean barnacles (gloves), disinfect (alcohol), stitch (fish bones), bandage.
// Day 5:  pop worms (gloves), remove dirty bandages (gloves), clean pus (alcohol), peel the fish,
//         drag the fish skin on as a graft, stitch the graft (fish bones), bandage.
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
        [Tooltip("Plays each time a target is treated in this phase, climbing in pitch while you keep going.")]
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

    [Header("Repeated Sounds")]
    [Tooltip("A phase with a single sound goes up this many semitones each time you treat something " +
             "in quick succession, like a combo. Phases with variant sounds just take turns instead.")]
    [SerializeField] private float comboSemitones = 0.5f;
    [Tooltip("How many steps it climbs before walking back down.")]
    [SerializeField] private int comboSteps = 3;
    [Tooltip("A pause longer than this, in seconds, starts the climb again from the bottom.")]
    [SerializeField] private float comboWindow = 1.2f;

    [Header("Opening")]
    [Tooltip("Optional. Plays when the minigame starts, i.e. as you first look at her.")]
    [SerializeField] private AudioClip openingSound;
    [Tooltip("Keep the opening sound looping until the minigame is finished.")]
    [SerializeField] private bool openingSoundLoops;
    [Range(0f, 1f)] [SerializeField] private float openingSoundVolume = 1f;
    [Tooltip("Seconds a looping opening sound takes to fade out once she's patched up.")]
    [SerializeField] private float openingFadeOut = 1.5f;

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

    // The treat sounds get a few voices of their own, so changing one sound's pitch never bends
    // another that's still ringing out, which is what would happen on the shared AudioManager source.
    private const int VoiceCount = 4;
    private AudioSource[] voices;
    private int nextVoice;
    private int comboCount;
    private float lastSoundTime = float.NegativeInfinity;
    private AudioSource openingSource;

    private Phase CurrentPhase =>
        phaseIndex >= 0 && phaseIndex < phases.Count ? phases[phaseIndex] : null;

    private MermaidTool CurrentTool => CurrentPhase != null ? CurrentPhase.tool : MermaidTool.None;

    void Awake()
    {
        voices = new AudioSource[VoiceCount];
        for (int i = 0; i < VoiceCount; i++)
        {
            voices[i] = gameObject.AddComponent<AudioSource>();
            voices[i].playOnAwake = false;
        }
    }

    void Start()
    {
        GameState.Set("minigame_open", false);

        // Queue every task up front. Only the top one shows, and completing it reveals the next.
        foreach (Phase phase in phases)
        {
            if (!string.IsNullOrEmpty(phase.taskResource))
                MessageBus.Instance.Publish("AddTaskString", phase.taskResource);
        }

        if (openingSound != null)
        {
            openingSource = gameObject.AddComponent<AudioSource>();
            openingSource.playOnAwake = false;
            openingSource.clip = openingSound;
            openingSource.loop = openingSoundLoops;
            openingSource.volume = openingSoundVolume;
            openingSource.Play();
        }

        StartCoroutine(BeginAfterDelay(0, startDelay));
    }

    // Hooked up to the tool buttons on the tray.
    public void ClickGloves() => ChooseTool(MermaidTool.Gloves);
    public void ClickAlcohol() => ChooseTool(MermaidTool.Alcohol);
    public void ClickFishBones() => ChooseTool(MermaidTool.FishBones);
    public void ClickBandages() => ChooseTool(MermaidTool.Bandages);
    public void ClickFish() => ChooseTool(MermaidTool.Fish);
    public void ClickFishSkin() => ChooseTool(MermaidTool.FishSkin);

    private void ChooseTool(MermaidTool tool)
    {
        if (finished || toolInHand) return;
        if (tray == null || !tray.IsShown) return;

        // The other tools are greyed out and not interactable, so this shouldn't ever trip. It
        // just stops a stray click from arming the wrong phase.
        if (tool != CurrentTool) return;

        toolInHand = true;
        tray.Hide();

        foreach (MermaidTarget target in CurrentPhase.targets)
        {
            if (target != null) target.OnToolReady();
        }
    }

    // Whether this target can be acted on right now. Gates the click and the hover tint both,
    // which is why a target with no work left feels inert rather than broken.
    public bool IsTargetLive(MermaidTarget target)
    {
        if (finished || !toolInHand || target == null) return false;
        if (tray != null && tray.IsShown) return false;
        if (target.IsBusy) return false;

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

        PlayPhaseSound(phase);

        stepsDone++;
        PublishProgress();

        if (!target.CanApply(phase.tool)) target.ClearHover();

        if (IsPhaseComplete(phase)) StartCoroutine(CompletePhase(phase));
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
        comboCount = 0;

        // The counter counts clicks, so a three-stage suture chain contributes three.
        stepsDone = 0;
        stepsTotal = 0;
        foreach (MermaidTarget target in phase.targets)
        {
            if (target != null) stepsTotal += target.RemainingSteps(phase.tool);
        }
        PublishProgress();

        if (tray != null) tray.Show(phase.tool);
    }

    private IEnumerator CompletePhase(Phase phase)
    {
        // Dropping the tool straight away also stops any further clicks landing on this phase.
        toolInHand = false;

        foreach (MermaidTarget target in allTargets)
        {
            if (target == null) continue;
            target.ClearHover();
            if (target.BodyCollider != null) target.BodyCollider.enabled = false;
        }

        // Let the last worm pop or fish peel play out before the task ticks over.
        while (AnyBusy(phase)) yield return null;

        foreach (MermaidTarget target in phase.targets)
        {
            if (target != null) target.OnPhaseFinished();
        }

        if (!string.IsNullOrEmpty(phase.taskId))
            MessageBus.Instance.Publish("CompleteTask", phase.taskId);

        yield return new WaitForSeconds(phaseGap);

        int next = phaseIndex + 1;
        if (next < phases.Count) BeginPhase(next);
        else Finish();
    }

    private static bool AnyBusy(Phase phase)
    {
        foreach (MermaidTarget target in phase.targets)
        {
            if (target != null && target.IsBusy) return true;
        }
        return false;
    }

    private void Finish()
    {
        if (finished) return;
        finished = true;
        phaseIndex = phases.Count;

        GameState.Set("minigame_open", false);
        GameState.Set("mermaid_treated", true);
        MessageBus.Instance.Publish("ClearTaskProgress");

        if (openingSource != null && openingSource.loop) StartCoroutine(FadeOut(openingSource, openingFadeOut));

        if (!string.IsNullOrEmpty(finishDialogue) && miscObjectClick != null)
        {
            Dialogue dialogue = miscObjectClick.getDialogue(finishDialogue);
            if (dialogue != null) DialogueManager.ShowDialogue(dialogue);
        }
    }

    private static IEnumerator FadeOut(AudioSource source, float duration)
    {
        float start = source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(start, 0f, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        source.Stop();
    }

    // A task that's done in one go doesn't need a 0/1 counter after it.
    private void PublishProgress()
    {
        MessageBus.Instance.Publish("SetTaskProgress", stepsDone, stepsTotal > 1 ? stepsTotal : 0);
    }

    private void PlayPhaseSound(Phase phase)
    {
        bool hasVariants = phase.sounds != null && phase.sounds.Length > 0;
        AudioClip clip = NextPhaseSound(phase);
        if (clip == null || voices == null) return;

        AudioSource voice = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;
        voice.pitch = hasVariants ? 1f : NextComboPitch();
        voice.PlayOneShot(clip);
    }

    // Up a step for each quick repeat, back down once it reaches the top, and from the bottom again
    // after a pause, so seven worm pops in a row sound like a run of notes instead of one sound
    // seven times.
    private float NextComboPitch()
    {
        if (Time.time - lastSoundTime > comboWindow) comboCount = 0;
        lastSoundTime = Time.time;

        int step = comboSteps > 0 ? Mathf.RoundToInt(Mathf.PingPong(comboCount, comboSteps)) : 0;
        comboCount++;
        return Mathf.Pow(2f, step * comboSemitones / 12f);
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

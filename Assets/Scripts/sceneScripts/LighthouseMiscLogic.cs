using UnityEngine;

public class LighthouseMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    void Start()
    {
        Ambience ambience = Ambience.Instance;

        // Update track 1
        UpdateTrack(ambience, bgLoop1, 0.3f, 1);
        // Update track 2
        UpdateTrack(ambience, bgLoop2, 0.17f, 2);

        // Day 3: back upstairs with the scissors pulled out of the wall, so the lighthouse
        // can be fixed now.
        if (GameState.Get<int>("day") == 3 && IsTaskActive("task_finish_maintenance") && GameState.Get<bool>("scissors_found", false))
        {
            GameState.Set("lighthouseFixForgotten", true);
            MessageBus.Instance.Publish("CompleteTask", "task_finish_maintenance");
            MessageBus.Instance.Publish("AddTaskBefore", "generic/task_lighthouse", "task_fish");
        }
    }

    private bool IsTaskActive(string taskId)
    {
        if (TaskManager.instance == null) return false;
        var tasks = TaskManager.instance.GetCurrentTasks();
        foreach (var task in tasks)
        {
            if (task.id == taskId)
                return true;
        }
        return false;
    }

    private void UpdateTrack(Ambience ambience, AudioClip newClip, float volume, int channel)
    {
        // Null when Play Mode is entered straight from this scene instead of MainScene.
        if (ambience == null)
            return;

        // Check if the new clip is different from the current clip
        AudioClip currentClip = ambience.GetCurrentClip(channel);
        if (currentClip != newClip)
        {
            // Play new clip if it's different
            ambience.PlayTrack(newClip, volume, channel);
        }
        else
        {
            // Update volume if the clip is the same
            ambience.SetVolume(channel, volume);
        }
    }

    public void UpdateTrackPublic(AudioClip newClip, float volume, int channel)
    {
        Ambience ambience = Ambience.Instance;
        if (ambience == null)
            return;

        AudioClip currentClip = ambience.GetCurrentClip(channel);
        if (currentClip != newClip)
        {
            // Play new clip if it's different
            ambience.PlayTrack(newClip, volume, channel);
        }
        else
        {
            // Update volume if the clip is the same
            ambience.SetVolume(channel, volume);
        }
    }
}

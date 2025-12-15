using UnityEngine;
using System.Collections;

public class BedroomMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    void Start()
    {
        if (Ambience.Instance != null) {
            Ambience ambience = Ambience.Instance;
    
            // Update track 1
            UpdateTrack(ambience, bgLoop1, 0.6f, 1);
            // Update track 2
            UpdateTrack(ambience, bgLoop2, 0.21f, 2);
        }

        int day = GameState.Get<int>("day");
        switch (day) {
            case 1:
                if (GameState.Get<string>("is_clean") != "true") {
                    MessageBus.Instance.Publish("AddTaskString", "generic/task_wash_up");
                }
                if (!GameState.Get<bool>("ate_breakfast")) {
                    MessageBus.Instance.Publish("AddTaskString", "generic/task_breakfast");
                }

                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
            case 7:
                break;
        }
    }

    private void UpdateTrack(Ambience ambience, AudioClip newClip, float volume, int channel)
    {
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

    public void NewDay() {
        GameState.Set("corn_clicked", false);
        GameState.Set("pepper_clicked", false);
        GameState.Set("alcohol_clicked", false);
        GameState.Set("fish_clicked", false);
        GameState.Set("ate_breakfast", false);
        GameState.Set("ate_dinner", false);
        GameState.Set("hungry", true);
        

        GameState.Set("lighthouse_opened", false);
        GameState.Set("lighthouse_fixed", false);
        GameState.Set("wrench_used", false);
        GameState.Set("oil_used", false);
        GameState.Set("scissors_used", false);
        GameState.Set("mercury_used", false);
        GameState.Set("gathered_fish", false);
        GameState.Set("is_clean", "false");

    }
}

using UnityEngine;

public class LighthouseMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    void Start()
    {
        Ambience ambience = Ambience.Instance;

        // Update track 1
        UpdateTrack(ambience, bgLoop1, 0.15f, 1);
        // Update track 2
        UpdateTrack(ambience, bgLoop2, 0.13f, 2);
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
}

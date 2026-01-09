using UnityEngine;

public class OutdoorsMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    [SerializeField] CutsceneManager CsManager;

    void Start()
    {
        Ambience ambience = Ambience.Instance;

        // Update track 1
        UpdateTrack(ambience, bgLoop1, 0.9f, 1);
        // Update track 2
        UpdateTrack(ambience, bgLoop2, 0.35f, 2);

        bool cutscenePlayed = GameState.Get<bool>("cutscene_outdoors_played", false) || GameState.Get<int>("day", 1) > 1;
        if (!cutscenePlayed)
        {
            Debug.Log("play lighthouse cutscene");
            GameState.Set("cutscene_outdoors_played", true);
            CsManager.PlayCutscene("Intro", true, () => {
                Debug.Log("lighthouse scene done");
                // SceneManager.LoadScene("BedroomScene");
            });
        }

        
    }

    private void UpdateTrack(Ambience ambience, AudioClip newClip, float volume, int channel)
    {
        // Check if the new clip is different from the current clip
        if (ambience == null)
        {
            return;
        }
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

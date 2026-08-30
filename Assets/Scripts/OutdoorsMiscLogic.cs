using UnityEngine;
using UnityEngine.UI;

public class OutdoorsMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    // [SerializeField] CutsceneManager CsManager;
    [SerializeField] private MiscObjectClick miscObjectClick;

    [Header("Thunderstorm")]
    [SerializeField] private AudioClip rainLoop;

    [Header("Weather UI (Optional)")]
    [SerializeField] private Button weatherActionButton;

    void Start()
    {
        UpdateAmbience();

        if (GameState.Get<bool>("near_nighttime"))
        {
            GameState.Set("near_nighttime", false);
            if (!GameState.Get<bool>("is_nighttime"))
            {
                GameState.Set("is_nighttime", true);
                DialogueManager.ShowDialogue(miscObjectClick.getDialogue("nearly_dark"));
            }
        }

        // Initialize Weather Button
        // if (weatherActionButton != null)
        // {
        //     weatherActionButton.onClick.AddListener(OnWeatherButtonClicked);
        // }
    }



    public void UpdateAmbience()
    {
        Ambience ambience = Ambience.Instance;
        bool raining = GameState.Get<bool>("thunderstorm", false);

        // Update track 1
        UpdateTrack(ambience, bgLoop1, 0.9f, 1);
        // Update track 2
        UpdateTrack(ambience, raining && rainLoop != null ? rainLoop : bgLoop2, 0.35f, 2);
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

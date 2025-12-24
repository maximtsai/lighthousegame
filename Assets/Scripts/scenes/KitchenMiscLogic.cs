using UnityEngine;
using UnityEngine.SceneManagement; // required for SceneManager

public class KitchenMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    [SerializeField] private MiscObjectClick miscObjectClick;
    void Start()
    {
        Ambience ambience = Ambience.Instance;

        // Update track 1
        UpdateTrack(ambience, bgLoop1, 0.75f, 1);
        // Update track 2
        UpdateTrack(ambience, bgLoop2, 0.24f, 2);

        if (GameState.Get<bool>("is_nighttime", false))
        {

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

	public void ClickTable() {
        // miscObjectClick.PlaySound(cleanSound);
		if (GameState.Get<int>("day") == 1) 
		{
            if (GameState.Get<bool>("lighthouse_fixed"))
			{
                if (GameState.Get<bool>("ate_dinner"))
                {
                    DialogueManager.ShowDialogue(miscObjectClick.getDialogue("kitchen/check_up_camborne"));
                } else
                {
                    DialogueManager.ShowDialogue(miscObjectClick.getDialogue("kitchen/sleep_all_day"));
                }
            }
			else 
			{
                if (GameState.Get<bool>("ate_breakfast", false) && !GameState.Get<bool>("gathered_fish", false))
                {
                    DialogueManager.ShowDialogue(miscObjectClick.getDialogue("kitchen/camborne_breakfast"));
                } else
                {
                    DialogueManager.ShowDialogue(miscObjectClick.getDialogue("kitchen/long_night"));
                }
            }
		}
	}

    public void checkupCamborne()
    {
        Debug.Log("Checkup camborne called");
        SceneManager.LoadScene("TableScene");
    }
}

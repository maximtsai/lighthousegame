using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // required for SceneManager

public class TableMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject tableButton;
    [SerializeField] private Sprite bgScare;
    [SerializeField] private MiscObjectClick miscObjectClick;
    [SerializeField] private MouseCameraPan mouseCamPanScript;

    [SerializeField] private AudioClip jumpScare;
    [SerializeField] private AudioClip horrorLoop;

    void Start()
    {
        Ambience ambience = Ambience.Instance;
        MessageBus.Instance.Subscribe("camborne_jumpscare", (str) =>
        {
            showCamborneJumpscare();
        });

        // Update track 1
        UpdateTrack(ambience, bgLoop1, 0.6f, 1);
        // Update track 2
        UpdateTrack(ambience, bgLoop2, 0.2f, 2);
        MessageBus.Instance.Publish("HideTask");
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

    public void clickCamborne()
    {
        DialogueManager.ShowDialogue(miscObjectClick.getDialogue("kitchen/wake_camborne"));
    }

    public void showCamborneJumpscare()
    {
        DialogueManager.CloseDialogue();
        tableButton.SetActive(false);

        background.GetComponent<SpriteRenderer>().sprite = bgScare;
        mouseCamPanScript.RecalculateDimensions();

        miscObjectClick.PlaySound(jumpScare, 0.8f, false);
        Ambience ambience = Ambience.Instance;
        UpdateTrack(ambience, horrorLoop, 0.8f, 1);

        StartCoroutine(ShowCamborneDialogAfterDelay());
    }

    private IEnumerator ShowCamborneDialogAfterDelay()
    {
        Debug.Log("Show Camborne Jumpscare");
        yield return new WaitForSeconds(1.5f);
        MessageBus.Instance.Publish("ChangeQuestText", "PANIC");
        yield return new WaitForSeconds(0.33f);
        MessageBus.Instance.Publish("ChangeQuestText", "HOLD BREATH");
        yield return new WaitForSeconds(0.33f);
        MessageBus.Instance.Publish("ChangeQuestText", "DON'T CHOKE");
        yield return new WaitForSeconds(0.33f);
        MessageBus.Instance.Publish("ChangeQuestText", "IGNORE MAGGOTS");
        yield return new WaitForSeconds(0.33f);
        MessageBus.Instance.Publish("ChangeQuestText", "AVERT GAZE");
        yield return new WaitForSeconds(0.33f);
        MessageBus.Instance.Publish("ChangeQuestText", "SWALLOW VOMIT");
        yield return new WaitForSeconds(0.33f);
        MessageBus.Instance.Publish("ChangeQuestText", "BREATHE OUT");
        yield return new WaitForSeconds(0.66f);
        MessageBus.Instance.Publish("ChangeQuestText", "MAKE DECISION");
        yield return new WaitForSeconds(1f);
        DialogueManager.ShowDialogue(miscObjectClick.getDialogue("kitchen/reveal_camborne"));

        MessageBus.Instance.Publish(
            "ShowThreeChoice",
            "PANIC",
            "STARE",
            "BURY HIM",
            (Action)(() =>
            {
                // Option 1 Panic
                Debug.Log("Panic!");
            }),
            (Action)(() =>
            {
                // Option 2
                Debug.Log("stare");
            }),
            (Action)(() =>
            {
                // Option 3
                Debug.Log("Bury him");
            })
        );

    }
}

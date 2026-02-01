using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // required for SceneManager

public class TableMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject background_fried;
    [SerializeField] private GameObject tableButton;
    [SerializeField] private Sprite bgScare;
    [SerializeField] private Sprite bgScareFried;
    [SerializeField] private MiscObjectClick miscObjectClick;
    [SerializeField] private MouseCameraPan mouseCamPanScript;

    [SerializeField] private AudioClip jumpScare;
    [SerializeField] private AudioClip horrorLoop;

    private MessageBus.SubscriptionHandle handle;
    private float pingPongSpeed = 0.5f;
    private float maxAlpha = 0.95f;
    float pingPongTime = 0f;

    SpriteRenderer sr_fried;
    private bool showingScare = false;
    void Start()
    {
        Ambience ambience = Ambience.Instance;
        handle = MessageBus.Instance.Subscribe("camborne_jumpscare", (str) =>
        {
            showCamborneJumpscare();
        });

        // Update track 1
        UpdateTrack(ambience, bgLoop1, 0.6f, 1);
        // Update track 2
        UpdateTrack(ambience, bgLoop2, 0.2f, 2);
        MessageBus.Instance.Publish("HideTask");
        sr_fried = background_fried.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!showingScare) return;

        pingPongTime += Time.deltaTime * pingPongSpeed;
        float a = Mathf.PingPong(pingPongTime, maxAlpha);

        Color c = sr_fried.color;
        c.a = a;
        sr_fried.color = c;
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
        sr_fried.sprite = bgScareFried;
        Color c = sr_fried.color;
        c.a = 1f;
        sr_fried.color = c;
        StartCoroutine(FadeToZero(sr_fried));

        mouseCamPanScript.RecalculateDimensions();

        miscObjectClick.PlaySound(jumpScare, 0.8f, false);
        Ambience ambience = Ambience.Instance;
        UpdateTrack(ambience, horrorLoop, 0.8f, 1);

        StartCoroutine(ShowCamborneDialogAfterDelay());
    }

    IEnumerator FadeToZero(SpriteRenderer sr_fried)
    {
        yield return new WaitForSeconds(0.25f);
        float startAlpha = sr_fried.color.a;
        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, 0f, t / 1);

            Color c = sr_fried.color;
            c.a = a;
            sr_fried.color = c;

            yield return null;
        }

        // Ensure EXACTLY 0 alpha
        Color final = sr_fried.color;
        final.a = 0f;
        sr_fried.color = final;
        
        pingPongTime = 0f;
        showingScare = true;
        
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
    }
    
    void OnDestroy()
    {
        // Always unsubscribe when this object is destroyed
        handle?.Unsubscribe();
    }

    private IEnumerator ShakeBG(SpriteRenderer bg)
    {
        yield return null;
    }
}

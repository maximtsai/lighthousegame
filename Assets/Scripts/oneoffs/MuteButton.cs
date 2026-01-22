using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool muted = false;
    public GameObject muteButton;
    public Sprite mutedSprite;
    public Sprite soundOnSprite;

    private Image muteButtonImage;
    void Start()
    {
        muteButtonImage = muteButton.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickMuteButton()
    {
        muted = !muted;
        if (muted)
        {
            muteButtonImage.sprite = mutedSprite;
            AudioManager.Instance.SetMuted(true);
        }
        else
        {
            muteButtonImage.sprite = soundOnSprite;
            AudioManager.Instance.SetMuted(false);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class UILogic : Singleton<UILogic>
{
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	private bool muted = false;
	public GameObject muteButton;
	public GameObject popup;
	public GameObject clickBlocker;
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

	public void OpenPopup()
	{
		clickBlocker.SetActive(true);
		popup.SetActive(true);
	}

	public void ClosePopup()
	{
		clickBlocker.SetActive(false);
		popup.SetActive(false);
	}
}
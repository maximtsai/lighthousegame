using UnityEngine;

public class StoveScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickCorn(SceneTransition holder)
    {
        if (GameState.Get<bool>("corn_clicked"))
        {
            PlaySound(holder.travelSound);
        }
        else
        {
            PlaySound(holder.travelSound);
        }
        GameState.Set("corn_clicked", true);
    }

    public void ClickPepper(SceneTransition holder)
    {
        GameState.Set("pepper_clicked", true);
    }

    public void ClickAlcohol(SceneTransition holder)
    {
        GameState.Set("alcohol_clicked", true);
    }

    public void ClickFish(SceneTransition holder)
    {
        if (GameState.Get<bool>("fish_clicked"))
        {
        }
        else
        {
            PlaySound(holder.travelSound);
        }
        GameState.Set("fish_clicked", true);
    }

    public void ClickPot(SceneTransition holder)
    {
        PlaySound(holder.travelSound);
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (!clip) return;
        if (!AudioManager.Instance) return;

        var src = AudioManager.Instance.AudioSource;
        src.PlayOneShot(clip);
    }
}

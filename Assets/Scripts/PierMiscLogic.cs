using UnityEngine;
using System.Collections;

public class PierMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip finishLoop;
    [SerializeField] private AudioClip rainLoop;
    [SerializeField] private GameObject rainOverlay;
    [SerializeField] private MiscObjectClick miscObjectClick;

    void Start()
    {
        bool isRaining = GameState.Get<bool>("thunderstorm", false);
        if (rainOverlay != null)
        {
            rainOverlay.SetActive(isRaining);
        }
        else
        {
            foreach (Animator anim in FindObjectsByType<Animator>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (anim.gameObject.name == "rain_0")
                {
                    anim.gameObject.SetActive(isRaining);
                    break;
                }
            }
        }

        AudioClip clipToPlay = (isRaining && rainLoop != null) ? rainLoop : finishLoop;
        StartCoroutine(PlaySoundDelayedRoutine(clipToPlay, 0.35f, true, 0.001f));
    }
    
    private IEnumerator PlaySoundDelayedRoutine(AudioClip sfx, float volume, bool loop, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (sfx != null && miscObjectClick != null)
        {
            miscObjectClick.PlaySound(sfx, volume, loop);
        }
    }
}

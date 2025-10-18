using UnityEngine;
using System.Collections;

public class PierMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip finishLoop;
    [SerializeField] private MiscObjectClick miscObjectClick;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(PlaySoundDelayedRoutine(finishLoop, 0.2f, true, 0.001f));
    }
    
    private IEnumerator PlaySoundDelayedRoutine(AudioClip sfx, float volume, bool loop, float delay)
    {
        yield return new WaitForSeconds(delay);
        miscObjectClick.PlaySound(sfx, volume, loop);
    }
}

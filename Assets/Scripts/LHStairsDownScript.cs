using UnityEngine;
using System.Collections;

public class LHStairsDownScript : MonoBehaviour
{
    [SerializeField] private AudioClip finishLoop;
    [SerializeField] private MiscObjectClick miscObjectClick;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameState.Get<bool>("lighthouse_fixed"))
        {
            StartCoroutine(PlaySoundDelayedRoutine(finishLoop, 0.4f, true, 0.01f));
        }
    }
    
    private IEnumerator PlaySoundDelayedRoutine(AudioClip sfx, float volume, bool loop, float delay)
    {
        yield return new WaitForSeconds(delay);
        miscObjectClick.PlaySound(sfx, volume, loop);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;
using System.Collections;

public class BedroomMiscLogic : MonoBehaviour
{
    [SerializeField] private AudioClip bgLoop1;
    [SerializeField] private AudioClip bgLoop2;
    void Start()
    {
        Ambience ambience = Ambience.Instance;

        // Play background ambience
        ambience.PlayTrack(bgLoop1, 0.7f, 1);
        ambience.PlayTrack(bgLoop2, 0.2f, 2);
    }
}

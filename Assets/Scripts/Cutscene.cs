using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "Cutscene", menuName = "Scriptable Objects/Cutscene")]
public class Cutscene : ScriptableObject
{
    public List<Sprite> images;
    public List<float> durations;
    public AudioClip backgroundMusic;
}
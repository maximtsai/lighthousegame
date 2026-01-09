using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "Cutscene", menuName = "Scriptable Objects/Cutscene")]
public class Cutscene : ScriptableObject
{
    public AnimationClip animation;
    public RuntimeAnimatorController controller;
    public float duration;
    public AudioClip backgroundMusic;
}
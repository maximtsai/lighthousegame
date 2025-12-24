using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "Dialogue", menuName = "Scriptable Objects/Dialogue")]
public class Dialogue : ScriptableObject
{
    public List<string> text;
    public UnityEvent onDialogueEnd;
    public UnityEvent onDialogueEndImmediate;
    public List<string> choices;
    public List<UnityEvent> consequences;
    public AudioClip startSound;
}

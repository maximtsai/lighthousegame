using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "Scriptable Objects/Task")]
public class Task : ScriptableObject
{
    public string id = "null";
    public string description = "null";
    public Dialogue on_completion;
}

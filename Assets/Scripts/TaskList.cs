using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TaskList", menuName = "Scriptable Objects/TaskList")]
public class TaskList : ScriptableObject
{
    public List<Task> tasks;
}

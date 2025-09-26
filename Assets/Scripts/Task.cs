using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "Scriptable Objects/Task")]
public class Task : ScriptableObject
{
    public string id = "null";
    public string description = "null";
    public string completion_object_id = "null"; // internal id assigned to each interactable object; when it matches current task, complete the task and load next
    public Dialogue on_completion;
    public List<Task> new_tasks_on_completion = new List<Task>();
}

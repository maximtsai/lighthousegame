using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "Scriptable Objects/Task")]
public class Task : ScriptableObject
{
    public string description = "null";
    public string completion_object_id = "null"; // internal id assigned to each interactable object; when it matches current task, complete the task and load next
    public Dialogue on_completion;
    public Task next_task;
    public Dialogue on_wrong;
}

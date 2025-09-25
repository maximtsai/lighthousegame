using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    private Task current_task;
    public static TaskManager instance;
    public Task default_task;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        instance.current_task = default_task;
        DontDestroyOnLoad(gameObject);
    }

    public static void AttemptTaskCompletion(InteractableObject o)
    {
        if (null == o || DialogueManager.DialogueIsOpen())
            return;

        if (o.GetObjectId() == instance.current_task.completion_object_id)
        {
            Debug.Log("task completed");
            if (instance.current_task.on_completion != null)
                DialogueManager.ShowDialogue(instance.current_task.on_completion);
            instance.current_task = instance.current_task.next_task;
        }
        else
        {
            Debug.Log("wrong object");
            if (instance.current_task.on_wrong != null)
                DialogueManager.ShowDialogue(instance.current_task.on_wrong);
        }
    }

    public static Task GetCurrentTask()
    {
        return instance.current_task;
    }

    void LoadTasksFromFile()
    {
        // deserialize json file or something
    }
}

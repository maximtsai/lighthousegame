using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Unity.VisualScripting;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    private List<Task> tasklist;
    [SerializeField] private Dialogue dialogue_no_matching_task;
    [SerializeField] private Dialogue dialogue_all_tasks_done;
    [SerializeField] private TaskList default_task_list;
    public static TaskManager instance;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        tasklist = new List<Task>();
        if (null != default_task_list) LoadTasks(default_task_list);
        DontDestroyOnLoad(gameObject);
    }

    public static void AttemptTaskCompletion(InteractableObject o)
    {
        if (null == o || DialogueManager.DialogueIsOpen())
            return;

        string object_id = o.GetObjectId();
        int matching_task_index = -1;
        for (int i = 0; i < instance.tasklist.Count; i++)
        {
            if (instance.tasklist[i].completion_object_id == object_id)
            {
                matching_task_index = i;
                break;
            }
        }

        if (-1 < matching_task_index)
        {
            Debug.Log("task completed");
            DialogueManager.ShowDialogue(instance.tasklist[matching_task_index].on_completion);
            instance.tasklist.RemoveAt(matching_task_index);
        }
        else if (0 == instance.tasklist.Count)
        {
            DialogueManager.ShowDialogue(instance.dialogue_all_tasks_done);
        }
        else
        {
            Debug.Log("wrong object");
            DialogueManager.ShowDialogue(instance.dialogue_no_matching_task);
        }
    }

    public static List<Task> GetCurrentTasks()
    {
        return instance.tasklist;
    }

    static void LoadTasks(TaskList l)
    {
        instance.tasklist = new List<Task>(l.tasks);
    }
}

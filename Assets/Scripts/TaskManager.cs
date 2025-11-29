using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager instance;
    private List<Task> taskList = new List<Task>();
    private string taskPath = "ScriptableObjects/Tasks/";

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        MessageBus.SubscriptionHandle addTaskHandle = MessageBus.Instance.Subscribe("AddTaskString", (args) =>
            {
                string message = args[0] as string;
                AddTaskString(message);
            });

        MessageBus.SubscriptionHandle addTaskImportantHandle = MessageBus.Instance.Subscribe("AddTaskStringImportant", (args) =>
        {
            string message = args[0] as string;
            AddTaskString(message);
        });

        MessageBus.SubscriptionHandle completeTaskHandle = MessageBus.Instance.Subscribe("CompleteTask", (args) =>
        {
            string message = args[0] as string;
            CompleteTask(message);
        });


        // We're implementing a new always-on task list, no longer needed.
        // GameState.Set("task_list_open", true);

        DontDestroyOnLoad(gameObject);
    }

    // Adds to back of tasklist
    // Usage in other scripts:
        // TaskManager.instance.AddTask(someTask);
    public void AddTask(Task task)
    {
        if (!taskList.Contains(task))
        {
            taskList.Add(task);
        }
        else
        {
            Debug.LogWarning("Task already added");
        }
    }
    public void AddTaskString(string id)
    {
        Task task = getTaskFromString(id);
        AddTask(task);
    }
    
    // Add task to the front (priority)
    public void AddTaskImportant(Task task)
    {
        if (!taskList.Contains(task))
        {
            taskList.Insert(0, task);
        }
        else
        {
            Debug.LogWarning("Task already added");
        }
    }
    public void AddTaskImportantString(string id)
    {
        Task task = getTaskFromString(id);
        AddTaskImportant(task);
    }
    
    public Task getTaskFromString(string name)
    {
        string fullPath = taskPath + name;
        Task task = Resources.Load<Task>(fullPath);
        if (task == null)
        {
            Debug.LogWarning("Task not found: " + fullPath);
        }
        return task;
    }
    

    // Complete a task by ID (recommended)
    public void CompleteTask(string id)
    {
        for (int i = 0; i < taskList.Count; i++)
        {
            if (taskList[i].id == id)
            {
                Task t = taskList[i];
                taskList.RemoveAt(i);

                // trigger the on completion dialog if any
                if (t.on_completion)
                {
                    DialogueManager.ShowDialogue(t.on_completion);
                }
                return;
            }
        }
    }
    // private static void AttemptTaskCompletion(string o, Dialogue all_tasks_done, Dialogue no_matching_task)
    // {
    //     string object_id = o;
    //     int matching_task_index = -1;
    //     for (int i = 0; i < instance.tasklist.Count; i++)
    //     {
    //         if (instance.tasklist[i].completion_object_id == object_id)
    //         {
    //             matching_task_index = i;
    //             break;
    //         }
    //     }
    //
    //     if (-1 < matching_task_index)
    //     {
    //         Debug.Log("task completed");
    //         Task task = instance.tasklist[matching_task_index];
    //         if (null != task.on_completion)
    //         {
    //             DialogueManager.ShowDialogue(task.on_completion);
    //         }
    //         instance.tasklist.RemoveAt(matching_task_index);
    //         if (0 < task.new_tasks_on_completion.Count)
    //         {
    //             foreach (Task t in task.new_tasks_on_completion)
    //             {
    //                 if (!instance.tasklist.Find((Task p) => { return p.id == t.id; }))
    //                     instance.tasklist.Add(t);
    //             }
    //         }
    //     }
    //     else if (0 == instance.tasklist.Count && null != all_tasks_done)
    //     {
    //         DialogueManager.ShowDialogue(all_tasks_done);
    //     }
    //     else if (null != no_matching_task)
    //     {
    //         DialogueManager.ShowDialogue(no_matching_task);
    //     }
    // }
    // public static void AttemptTaskCompletion(string o)
    // {
    //     AttemptTaskCompletion(o, instance.dialogue_all_tasks_done, instance.dialogue_no_matching_task);
    // }
    // public static void AttemptTaskCompletion(InteractableObject o)
    // {
    //     if (null == o) return;
    //     AttemptTaskCompletion(o.GetObjectId());
    // }
    // public static void AttemptTaskCompletionSilent(string o)
    // {
    //     AttemptTaskCompletion(o, null, null);
    // }
    // public static void AttemptTaskCompletionSilent(InteractableObject o)
    // {
    //     if (null == o) return;
    //     AttemptTaskCompletion(o.GetObjectId(), null, null);
    // }
    
    
    // Returns a shallow copy of all active tasks
    public List<Task> GetCurrentTasks()
    {
        return new List<Task>(taskList);
    }
    
    // Logs each task's id and description
    public void LogAllActiveTasks()
    {
        foreach (Task t in taskList)
        {
            Debug.Log($"Task ID: {t.id}\nDescription: {t.description}");
        }
    }
}

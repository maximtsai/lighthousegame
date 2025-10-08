using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    private List<Task> tasklist;
    [SerializeField] private Dialogue dialogue_no_matching_task;
    [SerializeField] private Dialogue dialogue_all_tasks_done;
    [SerializeField] private TaskList default_task_list;
    [SerializeField] private UITaskTracker prefab_task_tracker;
    public static TaskManager instance;
    void Awake()
    {
        Instantiate(prefab_task_tracker);
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        tasklist = new List<Task>();
        GameState.Set("task_list_open", false);
        if (null != default_task_list) LoadTasks(default_task_list);
        DontDestroyOnLoad(gameObject);
    }
    private static void AttemptTaskCompletion(string o, Dialogue all_tasks_done, Dialogue no_matching_task)
    {
        string object_id = o;
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
            Task task = instance.tasklist[matching_task_index];
            if (null != task.on_completion)
            {
                DialogueManager.ShowDialogue(task.on_completion);
            }
            instance.tasklist.RemoveAt(matching_task_index);
            if (0 < task.new_tasks_on_completion.Count)
            {
                foreach (Task t in task.new_tasks_on_completion)
                {
                    if (!instance.tasklist.Find((Task p) => { return p.id == t.id; }))
                        instance.tasklist.Add(t);
                }
            }
        }
        else if (0 == instance.tasklist.Count && null != all_tasks_done)
        {
            DialogueManager.ShowDialogue(all_tasks_done);
        }
        else if (null != no_matching_task)
        {
            DialogueManager.ShowDialogue(no_matching_task);
        }
    }
    public static void AttemptTaskCompletion(string o)
    {
        AttemptTaskCompletion(o, instance.dialogue_all_tasks_done, instance.dialogue_no_matching_task);
    }
    public static void AttemptTaskCompletion(InteractableObject o)
    {
        if (null == o) return;
        AttemptTaskCompletion(o.GetObjectId());
    }
    public static void AttemptTaskCompletionSilent(string o)
    {
        AttemptTaskCompletion(o, null, null);
    }
    public static void AttemptTaskCompletionSilent(InteractableObject o)
    {
        if (null == o) return;
        AttemptTaskCompletion(o.GetObjectId(), null, null);
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

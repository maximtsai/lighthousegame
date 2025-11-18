using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UITaskTracker : MonoBehaviour
{
    [SerializeField] TMP_Text text_tasklist;
    [SerializeField] Canvas canvas_full_list;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (null == canvas_full_list) return;
        canvas_full_list.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // if (null == text_tasklist) return;
        // List<Task> tasks = TaskManager.GetCurrentTasks();
        // if (0 < tasks.Count)
        // {
        //     text_tasklist.text = "";
        //     for (int i = 0; i < tasks.Count; i++)
        //     {
        //         text_tasklist.text += string.Format("{0:d}. {1}\n", i + 1, tasks[i].description);
        //     }
        // }
        // else
        // {
        //     text_tasklist.text = "No tasks.";
        // }
    }

    public void ShowTaskList()
    {
        canvas_full_list.gameObject.SetActive(true);
        GameState.Set("task_list_open", true);
    }

    public void HideTaskList()
    {
        canvas_full_list.gameObject.SetActive(false);
        GameState.Set("task_list_open", false);
    }
}

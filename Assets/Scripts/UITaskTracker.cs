using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDTask : MonoBehaviour
{
    [SerializeField] TMP_Text t;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        t.text = TaskManager.GetCurrentTask().description;
    }
}

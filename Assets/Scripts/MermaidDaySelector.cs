using UnityEngine;

// Selects the saved treatment hierarchy for the current day, including the Test Day tool.
//
// The Day 5 objects are saved switched off, so nothing from the wrong day ever starts up.
public class MermaidDaySelector : MonoBehaviour
{
    [Tooltip("Shown before the day below.")]
    [SerializeField] private GameObject[] day4Objects;
    [Tooltip("Shown from day5From until day6From.")]
    [SerializeField] private GameObject[] day5Objects;
    [SerializeField] private int day5From = 5;
    [SerializeField] private GameObject[] day6Objects;
    [SerializeField] private int day6From = 6;
    [SerializeField] private GameObject[] day7Objects;
    [SerializeField] private int day7From = 7;
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private Sprite day6Body;
    [SerializeField] private Sprite day7Body;

    void Awake()
    {
        int day = GameState.Get<int>("day", 1);
        bool daySeven = day >= day7From && day7Objects != null && day7Objects.Length > 0;
        bool daySix = !daySeven && day >= day6From && day6Objects != null && day6Objects.Length > 0;
        bool dayFive = day >= day5From && !daySix && !daySeven;

        foreach (GameObject go in day4Objects)
        {
            if (go != null) go.SetActive(!dayFive && !daySix && !daySeven);
        }
        foreach (GameObject go in day5Objects)
        {
            if (go != null) go.SetActive(dayFive);
        }
        if (day6Objects != null)
            foreach (GameObject go in day6Objects)
                if (go != null) go.SetActive(daySix);
        if (daySix && body != null && day6Body != null) body.sprite = day6Body;
        if (day7Objects != null)
            foreach (GameObject go in day7Objects)
                if (go != null) go.SetActive(daySeven);
        if (daySeven && body != null && day7Body != null) body.sprite = day7Body;
    }
}

using UnityEngine;

// Picks which mermaid to show. Day 4 is the barnacle clean-up; from day 5 on she's the festered
// version with worms, pus and the fish-skin graft. It reads the current day, so the Test Day
// tool drives it too.
//
// The Day 5 objects are saved switched off, so nothing from the wrong day ever starts up.
public class MermaidDaySelector : MonoBehaviour
{
    [Tooltip("Shown before the day below.")]
    [SerializeField] private GameObject[] day4Objects;
    [Tooltip("Shown on the day below and after.")]
    [SerializeField] private GameObject[] day5Objects;
    [SerializeField] private int day5From = 5;

    void Awake()
    {
        bool dayFive = GameState.Get<int>("day", 1) >= day5From;

        foreach (GameObject go in day4Objects)
        {
            if (go != null) go.SetActive(!dayFive);
        }
        foreach (GameObject go in day5Objects)
        {
            if (go != null) go.SetActive(dayFive);
        }
    }
}

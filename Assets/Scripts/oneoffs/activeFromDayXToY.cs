using UnityEngine;

public class activeFromDayXToY : MonoBehaviour
{
    [SerializeField] private int startDay;
    [SerializeField] private int endDay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int currentDay = GameState.Get<int>("day");
        if (currentDay >= startDay && currentDay <= endDay)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}

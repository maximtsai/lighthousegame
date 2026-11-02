using UnityEngine;

public class BurialActivate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Day 4: the way up to the mass grave stays open until the wreck's dead are all in it
        if (Day4Beach.MassGraveOpen)
            return;

        if (!GameState.Get<bool>("do_burial", false))
        {
            if (GameState.Get<int>("day", 1) == 2)
            {
                bool finishedFish = GameState.Get<bool>("gathered_fish", false);
                if (!finishedFish || GameState.Get<bool>("grave_inspected", false)) {
                    gameObject.SetActive(false);
                }
            } else {
                gameObject.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

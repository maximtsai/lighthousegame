using UnityEngine;

public class BurialActivate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!GameState.Get<bool>("do_burial", false))
        {
            if (GameState.Get<int>("day", 1) == 2)
            {
                if (GameState.Get<bool>("finished_burial", false)) {
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

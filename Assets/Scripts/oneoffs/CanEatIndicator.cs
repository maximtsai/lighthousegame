using UnityEngine;

public class CanEatIndicator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameState.Get<bool>("hungry") && GameState.Get<bool>("corn_clicked") && GameState.Get<bool>("pepper_clicked") && GameState.Get<bool>("alcohol_clicked"))
        {
            if (!GameState.Get<bool>("ate_breakfast") || GameState.Get<bool>("fish_clicked"))
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

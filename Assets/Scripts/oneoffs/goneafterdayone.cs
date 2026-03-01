using UnityEngine;

public class goneafterdayone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameState.Get<int>("day") > 1)
        {
            gameObject.SetActive(false);
        }
    }
}

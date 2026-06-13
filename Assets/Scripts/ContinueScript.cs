using UnityEngine;

// Logic for the Continue button.
public class ContinueScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!SaveManager.HasSave())
        {
            gameObject.SetActive(false);
        }
    }
}

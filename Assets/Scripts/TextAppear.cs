using UnityEngine;

public class TextAppear : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject textToShow;
    void Start()
    {
        Debug.Log("Whatever");
        textToShow.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetMouseButtonDown(0))
        {
            textToShow.SetActive(true);
        }*/
    }
    void OnMouseDown()
    {
        textToShow.SetActive(true);
    }
}

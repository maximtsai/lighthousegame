using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class FloatingText : MonoBehaviour
{
    private const float VELOCITY = 1.0f;
    private const float TOTAL_DURATION = 1.0f;
    private float current_duration = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        current_duration += Time.deltaTime;
        gameObject.transform.Translate(new Vector3(0, VELOCITY * Time.deltaTime, 0));
        gameObject.GetComponent<TextMeshPro>().alpha = 1.0f - (current_duration / TOTAL_DURATION);

        if (current_duration >= TOTAL_DURATION) Destroy(gameObject);
    }

    public void Instantiate(string text)
    {
        gameObject.GetComponent<TextMeshPro>().SetText(text);
    }
}

using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class FloatingText : MonoBehaviour
{
    private const float VELOCITY = 0.5f;
    private const float TOTAL_DURATION = 2.0f;
    private float current_duration = 0.0f;
    TextMeshPro _textMeshPro;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _textMeshPro = gameObject.GetComponent<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        current_duration += Time.deltaTime;
        gameObject.transform.Translate(new Vector3(0, VELOCITY * Time.deltaTime, 0));
        _textMeshPro.alpha = 1.5f - (current_duration / TOTAL_DURATION) * 1.5f;

        if (current_duration >= TOTAL_DURATION) Destroy(gameObject);
    }

    public void Instantiate(string text)
    {
        if (_textMeshPro != null)
        {
            _textMeshPro.SetText(text);
        }
        else
        {
            _textMeshPro = gameObject.GetComponent<TextMeshPro>();
            _textMeshPro.SetText(text);
        }
    }

    public void SetColor(Color c)
    {
        _textMeshPro.color = c;
    }
    
}

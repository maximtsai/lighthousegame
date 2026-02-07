using UnityEngine;

public class FloatingTextFactory : MonoBehaviour
{
    public GameObject prefab_floating_text;

    public void SpawnText(float x, float y, string text)
    {
        GameObject floating_text = Instantiate(prefab_floating_text, new Vector3(x, y, -1.0f), Quaternion.identity);

        FloatingText component = floating_text.GetComponent<FloatingText>();
        if (component != null) component.Instantiate(text);
    }
}

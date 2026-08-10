using UnityEngine;

public class FloatingTextFactory : MonoBehaviour
{
    public GameObject prefab_floating_text;
    private static FloatingTextFactory instance;
    private MessageBus.SubscriptionHandle addTaskHandle;

    // Only MainScene has one of these in it, so playing straight from any other scene used to
    // leave nothing listening for FloatText and the popups just never appeared. Same fallback
    // Navigation and AudioManager use.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureExists()
    {
        if (instance != null) return;

        GameObject prefab = Resources.Load<GameObject>("FloatingTextFactory");
        if (prefab == null)
        {
            Debug.LogError("FloatingTextFactory prefab not found in Resources folder!");
            return;
        }

        Instantiate(prefab).name = "FloatingTextFactory";
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
        addTaskHandle = MessageBus.Instance.Subscribe("FloatText", (args) =>
        {
            float x = System.Convert.ToSingle(args[0]);
            float y = System.Convert.ToSingle(args[1]);
            string text = args[2].ToString();
            string color = "yellow";
            if (args.Length > 3 && args[3] != null)
                color = args[3].ToString();
            
            SpawnText(x, y, text, color);
        });
    }

    public void SpawnText(float x, float y, string text, string color = "yellow")
    {
        GameObject floating_text = Instantiate(prefab_floating_text, new Vector3(x, y, -1.0f), Quaternion.identity);

        FloatingText component = floating_text.GetComponent<FloatingText>();
        if (component == null) return;

        component.Instantiate(text);
        component.SetColor(ParseColor(color));
    }

    private Color ParseColor(string color)
    {
        switch (color.ToLower())
        {
            case "red": return Color.red;
            case "green": return Color.green;
            case "blue": return Color.blue;
            case "white": return Color.white;
            case "black": return Color.black;
            case "purple": return Color.purple;
            case "yellow":
            default: return Color.yellow;
        }
    }
    
    private void OnDestroy()
    {
        addTaskHandle?.Unsubscribe();
    }
}

using UnityEngine;

// this class contains functions modifying specific GameState values

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;
    private MessageBus.SubscriptionHandle plusSanityHandle;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        plusSanityHandle = MessageBus.Instance.Subscribe("PlusSanity", (args) =>
        {
            int amount = (int)args[0];
            GameState.Increment("sanity", amount);
        }, this);
    }

    public void SetClean(bool is_clean)
    {
		if (is_clean) {
	        GameState.Set("is_clean", "true");
		} else {
	        GameState.Set("is_clean", "false");
		}
    }

    void OnDestroy()
    {
        plusSanityHandle?.Unsubscribe();
    }
}

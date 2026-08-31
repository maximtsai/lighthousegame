using UnityEngine;

// this class contains functions modifying specific GameState values

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;
    private MessageBus.SubscriptionHandle plusSanityHandle;

    // Pier and other scenes may be played without visiting Bedroom/Sink first.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureExists()
    {
        if (instance != null)
            return;

        GameObject go = new GameObject("GameStateManager");
        go.AddComponent<GameStateManager>();
    }

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SubscribePlusSanity();
    }

    private void SubscribePlusSanity()
    {
        if (plusSanityHandle != null)
            return;

        plusSanityHandle = MessageBus.Instance.Subscribe("PlusSanity", (args) =>
        {
            int amount = (int)args[0];
            GameState.Increment("sanity", amount);

            bool suppressSound = args.Length > 1 && args[1] is bool skip && skip;
            if (suppressSound)
                return;

            if (amount > 0)
                MessageBus.Instance.Publish("PlaySound", "sanity_gain");
            else if (amount < 0)
                MessageBus.Instance.Publish("PlaySound", "sanity_loss");
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

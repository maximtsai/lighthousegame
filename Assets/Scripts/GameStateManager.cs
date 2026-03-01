using UnityEngine;

// this class contains functions modifying specific GameState values

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;
    [SerializeField] private AudioClip sanityGainSound;
    [SerializeField] private AudioClip sanityLossSound;
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
            if (amount > 0) {
                MessageBus.Instance.Publish("PlaySound", sanityGainSound);
            } else {
                MessageBus.Instance.Publish("PlaySound", sanityLossSound);
            }
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

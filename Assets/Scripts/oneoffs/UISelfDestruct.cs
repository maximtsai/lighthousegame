using UnityEngine;

public class UISelfDestruct : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private MessageBus.SubscriptionHandle destroyHandle;

    void Start()
    {
        destroyHandle = MessageBus.Instance.Subscribe("DestroyAllUI", (args) =>
        {
            Destroy(gameObject);
        }, this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnDestroy()
    {
        // Always unsubscribe when this object is destroyed
        destroyHandle?.Unsubscribe();
    }
}

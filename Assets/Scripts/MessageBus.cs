using System;
using System.Collections.Generic;
using UnityEngine;

/*
Example usage:

// Subscribe to a message
MessageBus.SubscriptionHandle handle =
    MessageBus.Instance.Subscribe("GameStarted", (args) =>
{
    Debug.Log("Game started!");
});

// Publish a message with optional arguments
MessageBus.Instance.Publish("GameStarted");
MessageBus.Instance.Publish("DialogueShown", "intro_1", true);

// Unsubscribe when no longer needed
handle.Unsubscribe();
*/

public class MessageBus
{
    // ------------------------------------------------------------
    // Singleton
    // ------------------------------------------------------------

    private static MessageBus _instance;

    public static MessageBus Instance
    {
        get
        {
            if (_instance == null)
                _instance = new MessageBus();
            return _instance;
        }
    }

    // ------------------------------------------------------------
    // Internals
    // ------------------------------------------------------------

    private const int INDEX_THRESHOLD = 32767;

    // Topics -> (Subscriber ID -> Subscription info)
    private readonly Dictionary<string, Dictionary<int, Subscription>> topics =
        new Dictionary<string, Dictionary<int, Subscription>>();

    private int index = 0;

    private class Subscription
    {
        public Action<object[]> Callback;
        public object Target;
    }

    public class SubscriptionHandle
    {
        public string Topic { get; private set; }

        private readonly int id;
        private readonly MessageBus bus;

        internal SubscriptionHandle(string topic, int id, MessageBus bus)
        {
            Topic = topic;
            this.id = id;
            this.bus = bus;
        }

        public void Unsubscribe()
        {
            bus.UnsubscribeInternal(Topic, id);
        }
    }

    // ------------------------------------------------------------
    // Internally remove a subscription
    // ------------------------------------------------------------

    private void UnsubscribeInternal(string topic, int id)
    {
        if (topics.TryGetValue(topic, out var subscribers))
        {
            subscribers.Remove(id);
        }
    }

    // ------------------------------------------------------------
    // Subscribe
    // ------------------------------------------------------------

    public SubscriptionHandle Subscribe(string topic, Action<object[]> callback, object target = null)
    {
        if (string.IsNullOrEmpty(topic))
        {
            Debug.LogError($"Undefined subscription topic for callback: {callback}");
            return null;
        }

        if (callback == null)
        {
            Debug.LogError("Cannot subscribe with a null callback.");
            return null;
        }

        if (!topics.ContainsKey(topic))
        {
            topics[topic] = new Dictionary<int, Subscription>();
        }

        int currentId = index++;

        topics[topic][currentId] = new Subscription
        {
            Callback = callback,
            Target = target
        };

        if (index > INDEX_THRESHOLD)
        {
            Debug.LogWarning($"WARNING: Subscriber threshold reached for topic {topic}!");
        }

        return new SubscriptionHandle(topic, currentId, this);
    }

    // ------------------------------------------------------------
    // Publish
    // ------------------------------------------------------------
    public void Publish(string topic, params object[] args)
    {
        if (!topics.TryGetValue(topic, out var subscribers))
            return;

        List<Exception> errors = new();

        foreach (var pair in subscribers)
        {
            try
            {
                pair.Value.Callback?.Invoke(args);
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        }

        if (errors.Count > 0)
        {
            Debug.LogError($"Errors occurred during MessageBus.Publish({topic}):");
            foreach (var e in errors)
                Debug.LogException(e);
        }
    }
}
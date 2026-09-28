using System;
using UnityEngine;

/// <summary>
/// The day 4 "Scratch?" prompt that comes up during the routine. Scratching costs sanity and
/// plays the hand scratch if the hand is on screen; leaving it alone costs nothing.
/// </summary>
public static class HandScratch
{
    public const int SanityCost = 2;

    public static void Prompt(string question, Action onDone = null, AudioClip fallbackSound = null)
    {
        MessageBus.Instance.Publish("ShowChoiceDialog", question);
        MessageBus.Instance.Publish(
            "ShowTwoChoice",
            "YES",
            "NO",
            (Action)(() =>
            {
                handlogic hand = UnityEngine.Object.FindFirstObjectByType<handlogic>();
                if (hand != null)
                    hand.Scratch();
                else if (fallbackSound != null)
                    MessageBus.Instance.Publish("PlaySound", fallbackSound);

                MessageBus.Instance.Publish("FloatText", 0f, 0.3f, "-SANITY", "purple");
                MessageBus.Instance.Publish("PlusSanity", -SanityCost);
                onDone?.Invoke();
            }),
            (Action)(() =>
            {
                onDone?.Invoke();
            })
        );
    }
}

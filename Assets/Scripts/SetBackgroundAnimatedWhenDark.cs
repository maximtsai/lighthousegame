using UnityEngine;

public class SetBackgroundAnimatedWhenDark : MonoBehaviour
{
    [SerializeField] private Animator background;
    [SerializeField] private AnimationClip clipDark;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameState.Get<bool>("is_nighttime", false) || true)
        {
            if (background != null && clipDark != null)
            {
                background.Play(clipDark.name, 0, 0f); // layer 0, start at time 0
                MessageBus.Instance.Publish("refreshCamera");
            }
            else
            {
                Debug.LogWarning("Animator or clipDark not assigned.");
            }
        }
    }
}

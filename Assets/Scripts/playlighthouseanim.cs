using UnityEngine;

public class playlighthouseanim : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Animator animator;

    void Awake()
    {
        if (!GameState.Get<bool>("lighthouse_fixed"))
        {
            gameObject.SetActive(false);
        }
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        if (GameState.Get<bool>("lighthouse_fixed"))
        {
            PlayLighthouseAnimation();
        }
    }
    

    public void PlayLighthouseAnimation()
    {
        if (animator != null)
        {
            // animator.SetTrigger("lighthouse_anim");
        }
        else
        {
            Debug.LogWarning("Animator not found on background!");
        }
    }
    
}

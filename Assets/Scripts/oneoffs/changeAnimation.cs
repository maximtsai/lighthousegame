using UnityEngine;

public class changeAnimation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public AnimationClip clip;
    public RuntimeAnimatorController controller;
    void Start()
    {
        Animator animator = GameObject.FindFirstObjectByType<Animator>();
        animator.runtimeAnimatorController = controller;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

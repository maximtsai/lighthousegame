using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickBlocker : MonoBehaviour
{
    void OnMouseDown()
    {
        UIDialogueBox parentScript = transform.parent.GetComponent<UIDialogueBox>();
        
        // Check if the script exists to avoid null reference errors
        if (parentScript != null)
        {
            // Call the function in the parent script
            parentScript.AdvanceDialogue();
        }
        else
        {
            Debug.LogWarning("ParentScript not found on parent GameObject!");
        }
    }
}

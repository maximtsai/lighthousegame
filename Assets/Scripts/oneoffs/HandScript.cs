using UnityEngine;

public class HandScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private RectTransform handRect;
    [SerializeField] private float parallaxStrength = 20f;

    private Vector2 initialHandPos;
    private RectTransform canvasRect;
    private Camera mainCam;
    
    void Awake()
    {
        if (handRect == null)
        {
            Debug.LogError("HandParallaxUI: handRect not assigned.");
            enabled = false;
            return;
        }

        mainCam = Camera.main;
        canvasRect = handRect.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        initialHandPos = handRect.anchoredPosition;
        initialHandPos.x += 100f;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 camDelta = mainCam.transform.position;
        Vector2 parallaxOffset = new Vector2(
            camDelta.x * parallaxStrength,
            camDelta.y * parallaxStrength * 0.1f
        );
        Vector2 targetPos = initialHandPos + parallaxOffset;

        targetPos.y = Mathf.Min(targetPos.y, 0f);

        handRect.anchoredPosition = targetPos;
    }
}

using UnityEngine;
using UnityEngine.InputSystem; // required for new Input System
using UnityEngine.SceneManagement;

public class MouseCameraPan : MonoBehaviour
{
    public Transform background; // assign your background GameObject in Inspector
    private Camera cam;
    public float edgeThreshold = 0.25f; // 20% by default

    private Vector2 bounds; // half-size of how far camera can move

    void Start()
    {
        cam = Camera.main;

        // Calculate how much bigger the background is compared to the camera view
        float bgWidth = background.GetComponent<SpriteRenderer>().bounds.size.x;
        float bgHeight = background.GetComponent<SpriteRenderer>().bounds.size.y;

        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        bounds = new Vector2(
            Mathf.Max(0, (bgWidth - camWidth) / 2f),
            Mathf.Max(0, (bgHeight - camHeight) / 2f)
        );
		UpdatePosition(0.9f);
    }

    void Update()
    {
		UpdatePosition(0.05f);
    }

	void UpdatePosition(float shiftRatio)
	{
        if (Mouse.current == null) 
		{
			Debug.Log("Missing Mouse");
			return;
		}; // safety check

        // Get mouse position in pixels
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Normalize (0–1 range)
        mousePos = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);
        // Apply edge threshold
        mousePos.x = ApplyEdgeClamp(mousePos.x);
        mousePos.y = ApplyEdgeClamp(mousePos.y);
        
        // Convert to -1 to 1 range
        mousePos = (mousePos - Vector2.one * 0.5f) * 2f;

        // Move camera relative to mouse position
        Vector3 targetPos = new Vector3(
            mousePos.x * bounds.x,
            mousePos.y * bounds.y,
            transform.position.z
        );

        transform.position = targetPos * shiftRatio + transform.position * (1-shiftRatio);
	}


    private float ApplyEdgeClamp(float value)
    {
        if (value < edgeThreshold) return 0f;       // snap to min
        if (value > 1f - edgeThreshold) return 1f;  // snap to max
        // normalize the middle region back to 0–1
        return (value - edgeThreshold) / (1f - 2f * edgeThreshold);
    }
    
}

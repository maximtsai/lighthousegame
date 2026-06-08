using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel; // required for new Input System
using UnityEngine.SceneManagement;

public class MouseCameraPan : MonoBehaviour
{
    public Transform background; // assign your background GameObject in Inspector
    private Camera cam;
    public float edgeThreshold = 0.25f; // 25% by default
    public float scrollSpeed = 0.05f;

    private Vector2 bounds; // half-size of how far camera can move
    private Vector2 lastMousePos;
    private MessageBus.SubscriptionHandle handle;

    void Start()
    {
        cam = Camera.main;
        if (Pointer.current != null)
        {
            lastMousePos = Pointer.current.position.ReadValue();
        }
        else
        {
            lastMousePos = new Vector2(Screen.width / 2f, Screen.height / 2f); // Start centered fallback
        }
        RecalculateDimensions();
        handle = MessageBus.Instance.Subscribe("refreshCamera", RecalculateDimensionsMessage, this);
        
    }

    private void RecalculateDimensionsMessage(object[] args)
    {
	    RecalculateDimensions();
    }
    public void RecalculateDimensions()
    {
        // Calculate how much bigger the background is compared to the camera view
        float bgWidth = background.GetComponent<SpriteRenderer>().bounds.size.x;
        float bgHeight = background.GetComponent<SpriteRenderer>().bounds.size.y;

        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        bounds = new Vector2(
            Mathf.Max(0, (bgWidth - camWidth) / 2f),
            Mathf.Max(0, (bgHeight - camHeight) / 2f)
        );
        UpdatePosition(1f);
    }

    void Update()
    {
		UpdatePosition(scrollSpeed);
    }

	void UpdatePosition(float shiftRatio)
	{
        if (Pointer.current == null) 
		{
			return;
		}; // safety check

		if (GameState.Get<bool>("picking_choice"))
		{
			// we are currently picking choice, stop moving.
			return;
		}

		if (GameState.Get<bool>("dialogue_is_open") || GameState.Get<bool>("minigame_open") || GameState.Get<bool>("is_recording_weather"))
		{
			// Make scrolling much slower when dialog open
			shiftRatio *= 0.25f;
		} else if (GameState.Get<bool>("pause_open"))
		{
			shiftRatio = 0;
		}

		
        // Get pointer position (mouse or touch)
        Vector2 currentPointerPos = Pointer.current.position.ReadValue();

        // Update the tracked position if the pointer is moving or being pressed (important for mobile)
        if (Pointer.current.delta.ReadValue().sqrMagnitude > 0.01f || Pointer.current.press.isPressed)
        {
            lastMousePos = currentPointerPos;
        }

        // Normalize (0–1 range) based on the last known position
        Vector2 mousePos = new Vector2(lastMousePos.x / Screen.width, lastMousePos.y / Screen.height);
        // Apply edge threshold
        mousePos.x = ApplyEdgeClamp(mousePos.x);
        mousePos.y = ApplyEdgeClamp(mousePos.y);
        
        // Convert to -1 to 1 range
        mousePos = (mousePos - Vector2.one * 0.5f) * 2f;

        if (GameState.Get<bool>("minigame_open") || GameState.Get<bool>("is_recording_weather"))
        {
	        // Center the camera more when minigame is open
	        mousePos *= 0.2f;
        }
        
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

    void OnDestroy()
    {
	    // Always unsubscribe when this object is destroyed
	    handle?.Unsubscribe();
    }
    // private void ShakeScreen()
    // {
	   //  
    // }
}

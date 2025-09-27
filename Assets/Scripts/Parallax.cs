using UnityEngine;
using System.Collections;

public class ParallaxScroll : MonoBehaviour
{
    // The camera that this object will scroll relative to
    public Camera mainCamera;
    
    // The parallax scrolling speed, where 1 is same speed as camera
    // and 0.5 is 50% of the camera's speed.
    public float parallaxSpeed = 0.5f;
    public float startScale = 1f;
    public float scaleSpread = 0.05f;

    // Stores the camera's position from the previous frame.
    private Vector3 lastCameraPosition;

    void Start()
    {
        // Find the main camera if it's not set.
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Store the initial position of the camera.
        lastCameraPosition = mainCamera.transform.position;
        StartCoroutine(ChangeScaleRoutine());
    }

    void LateUpdate()
    {
        // Calculate the distance the camera has moved since the last frame.
        Vector3 cameraMovementDelta = mainCamera.transform.position - lastCameraPosition;

        // Move the object by a fraction of the camera's movement.
        transform.position += cameraMovementDelta * parallaxSpeed;

        // Update the last camera position for the next frame.
        lastCameraPosition = mainCamera.transform.position;
    }
    // Coroutine to change the scale every 0.5 seconds
    private IEnumerator ChangeScaleRoutine()
    {
        while (true)
        {
            // Set a random scale between 1 and 1.05 for x and y
            float randomScale = Random.Range(startScale, startScale + scaleSpread);
            transform.localScale = new Vector3(randomScale, 0.5f + randomScale * 0.5f, 1f);

            // Wait for 0.5 seconds before the next scale change
            yield return new WaitForSeconds(0.5f);
        }
    }
}
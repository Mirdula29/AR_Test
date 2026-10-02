using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ARPlacementManager : MonoBehaviour
{
    [Header("Prefabs & References")]
    [SerializeField] private GameObject gameBasePrefab;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;

    private GameObject spawnedBase = null;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        // Auto-assign managers if not dragged into the Inspector
        if (raycastManager == null) raycastManager = GetComponent<ARRaycastManager>();
        if (planeManager == null) planeManager = GetComponent<ARPlaneManager>();
    }

    void Update()
    {
        // Stop if the base is already placed
        if (spawnedBase != null) return;

        Vector2 touchPosition = Vector2.zero;
        bool hasTouch = false;

        // 1. Check New Input System (Unity 6 default)
#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            hasTouch = true;
        }
#endif

        // 2. Check Legacy Input System fallback
        if (!hasTouch && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                touchPosition = touch.position;
                hasTouch = true;
            }
        }

        // 3. Process Raycast against detected AR Planes
        if (hasTouch)
        {
            if (raycastManager != null && raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds))
            {
                Pose hitPose = hits[0].pose;

                // Spawn and align to surface normal
                spawnedBase = Instantiate(gameBasePrefab, hitPose.position, hitPose.rotation);

                // Hide planes and disable plane manager
                if (planeManager != null)
                {
                    foreach (var plane in planeManager.trackables)
                    {
                        plane.gameObject.SetActive(false);
                    }
                    planeManager.enabled = false;
                }
            }
        }
    }
}
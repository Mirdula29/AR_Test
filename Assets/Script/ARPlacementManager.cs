using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementManager : MonoBehaviour
{
    [SerializeField] private GameObject gameBasePrefab;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;

    private GameObject spawnedBase = null;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool isPlacementAllowed = false;

    void Awake()
    {
        if (raycastManager == null) raycastManager = GetComponent<ARRaycastManager>();
        if (planeManager == null) planeManager = GetComponent<ARPlaneManager>();

        // Keep planes hidden until Start is pressed
        if (planeManager != null)
        {
            planeManager.enabled = false;
        }
    }

    // Called when Start button is pressed on How To Play card
    public void EnablePlacement()
    {
        if (planeManager != null)
        {
            planeManager.enabled = true;
        }

        StartCoroutine(ActivateTouchNextFrame());
    }

    private IEnumerator ActivateTouchNextFrame()
    {
        yield return null;
        isPlacementAllowed = true;
    }

    void Update()
    {
        if (!isPlacementAllowed) return;
        if (spawnedBase != null) return;

        Vector2 touchPosition = Vector2.zero;
        bool hasTouch = false;

        // 1. Unity 6 New Input System (Hardware Touchscreen)
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Touchscreen.current != null)
        {
            var primaryTouch = UnityEngine.InputSystem.Touchscreen.current.primaryTouch;
            if (primaryTouch.press.wasPressedThisFrame)
            {
                touchPosition = primaryTouch.position.ReadValue();
                hasTouch = true;
            }
        }
#endif

        // 2. Legacy Input System fallback
        if (!hasTouch && Input.touchCount > 0)
        {
            UnityEngine.Touch touch = Input.GetTouch(0);
            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                touchPosition = touch.position;
                hasTouch = true;
            }
        }

        // 3. Process Raycast against detected AR planes
        if (hasTouch)
        {
            if (raycastManager != null && raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds))
            {
                Pose hitPose = hits[0].pose;

                if (gameBasePrefab != null)
                {
                    // Spawn base aligned to surface normal
                    spawnedBase = Instantiate(gameBasePrefab, hitPose.position, hitPose.rotation);

                    // Notify GameManager to show Fire Button and wire HUD
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.OnBasePlaced(spawnedBase);
                    }

                    // Turn off planes once placed
                    if (planeManager != null)
                    {
                        foreach (var plane in planeManager.trackables)
                        {
                            plane.gameObject.SetActive(false);
                        }
                        planeManager.enabled = false;
                    }
                }
                else
                {
                    Debug.LogError("Game Base Prefab is unassigned on ARPlacementManager!");
                }
            }
        }
    }
}
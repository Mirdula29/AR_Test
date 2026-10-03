using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform mainCam;

    void Start()
    {
        if (Camera.main != null) mainCam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (mainCam != null)
        {
            // Face the camera directly
            transform.forward = mainCam.forward;
        }
    }
}
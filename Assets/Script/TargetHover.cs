using UnityEngine;

public class TargetHover : MonoBehaviour
{
    [SerializeField] private float bobSpeed = 2.0f;
    [SerializeField] private float bobHeight = 0.03f; // 3cm bobbing range
    [SerializeField] private float rotateSpeed = 25f;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        // Gentle vertical hover
        float newY = initialPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = new Vector3(initialPosition.x, newY, initialPosition.z);

        // Slow volatile rotation
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }
}
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 spawnPosition;
    private const float MaxDistance = 5.0f; // Brief requirement: Miss > 5m -> Destroy
    [SerializeField] private GameObject impactVfxPrefab;

    void Start()
    {
        spawnPosition = transform.position;
    }

    void Update()
    {
        if (Vector3.Distance(spawnPosition, transform.position) >= MaxDistance)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnProjectileMiss();
            }
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            if (impactVfxPrefab != null)
            {
                Instantiate(impactVfxPrefab, other.transform.position, Quaternion.identity);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTargetHit();
            }

            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
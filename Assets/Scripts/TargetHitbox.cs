using UnityEngine;
using System.Collections;

public class TargetHitbox : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject explosionPrefab;

    [Header("Respawn Settings")]
    public float respawnDelay = 3f;

    [Header("System Links")]
    public GameManager gameManager;

    // Changed from single to array to handle child models
    private MeshRenderer[] allRenderers;
    private Collider[] allColliders;

    void Start()
    {
        // Get all renderers and colliders attached to this object AND its children
        allRenderers = GetComponentsInChildren<MeshRenderer>();
        allColliders = GetComponentsInChildren<Collider>();
    }

    void OnCollisionEnter(Collision collision)
    {
        ArtilleryProjectile projectile = collision.gameObject.GetComponentInParent<ArtilleryProjectile>();

        if (projectile != null)
        {
            if (gameManager != null) gameManager.AddScore();

            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, transform.rotation);
            }

            Destroy(collision.gameObject);
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        // Toggle visibility for all child components
        foreach (var rend in allRenderers)
        {
            rend.enabled = false;
        }

        // Toggle collision for all child components
        foreach (var col in allColliders)
        {
            col.enabled = false;
        }

        yield return new WaitForSeconds(respawnDelay);

        // Re-enable everything
        foreach (var rend in allRenderers)
        {
            rend.enabled = true;
        }
        foreach (var col in allColliders)
        {
            col.enabled = true;
        }
    }
}
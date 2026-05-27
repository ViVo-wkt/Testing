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

    private MeshRenderer meshRenderer;
    private Collider col;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // 1. Identify if it's our artillery shell
        ArtilleryProjectile projectile = collision.gameObject.GetComponentInParent<ArtilleryProjectile>();

        if (projectile != null)
        {
            // Report the hit
            if (gameManager != null) gameManager.AddScore();

            // 2. Trigger the explosion visuals at the target location
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, transform.rotation);
            }

            // 3. FORCE DESTRUCTION OF THE SHELL IMMEDIATELY
            // Destroying the shell here stops it from "floating"
            Destroy(collision.gameObject);

            // 4. Trigger the Respawn routine for this target
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        // Hide the target visuals and disable interaction
        meshRenderer.enabled = false;
        col.enabled = false;

        // Wait for the delay
        yield return new WaitForSeconds(respawnDelay);

        // Re-enable
        meshRenderer.enabled = true;
        col.enabled = true;
    }
}
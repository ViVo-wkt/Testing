using UnityEngine;

public class TargetHitbox : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject explosionPrefab;

    void OnCollisionEnter(Collision collision)
    {
        // Check if the object hitting us has the ArtilleryProjectile script
        if (collision.gameObject.GetComponentInParent<ArtilleryProjectile>() != null)
        {
            Debug.Log($"<color=green>DIRECT HIT on {gameObject.name}!</color>");

            // 1. Spawn the explosion exactly where the target is
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, transform.rotation);
            }

            // 2. Destroy the artillery shell
            Destroy(collision.gameObject);

            // 3. Destroy the target cube itself
            Destroy(gameObject);
        }
    }
}
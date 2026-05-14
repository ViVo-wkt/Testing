using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArtilleryProjectile : MonoBehaviour
{
    [Header("Flight Physics")]
    [Tooltip("1 = Normal Gravity. 0.5 = Half Gravity (Flies further). 0 = Laser beam.")]
    [Range(0f, 2f)]
    public float gravityMultiplier = 0.5f;

    [Header("Visuals")]
    [Tooltip("If true, the shell will constantly rotate its nose to face the direction it's falling.")]
    public bool alignToTrajectory = true;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 1. Turn off Unity's default gravity so we can manually control it!
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        // 2. Apply our custom, tweaked gravity every physics frame
        // Physics.gravity is usually (0, -9.81, 0)
        rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);

        // 3. Make the nose gracefully point in the exact direction of travel
        if (alignToTrajectory && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            // This assumes your prefab wrapper is aligned so the Blue Z-Arrow is the nose!
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }
}
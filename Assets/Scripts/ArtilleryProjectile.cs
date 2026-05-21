using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArtilleryProjectile : MonoBehaviour
{
    [Header("Flight Physics")]
    [Tooltip("1 = Normal Gravity. 0.5 = Half Gravity (Flies further).")]
    [Range(0f, 2f)]
    public float gravityMultiplier = 0.5f;

    [Header("Impact Physics")]
    [Tooltip("How much drag to apply the moment it hits the ground to stop it rolling.")]
    public float groundStoppingPower = 5f;
    private bool hasHitGround = false;

    [Header("Visuals")]
    public bool alignToTrajectory = true;

    [Header("Smart Aim Assist")]
    public bool enableSmartAim = true;
    [Tooltip("How wide the 'forgiveness' cone is in degrees. (e.g. 15 means it will correct near-misses).")]
    public float assistConeAngle = 15f;
    [Tooltip("How strongly the shell curves towards the target in mid-air.")]
    public float homingStrength = 15f;

    private Rigidbody rb;
    private Transform lockedTarget;
    private bool hasLockedOn = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        GetComponent<TrailRenderer>().enabled = true;
    }

    void FixedUpdate()
    {
        if (hasHitGround) return; // Stop applying flight/homing physics if it's in the dirt

        // 1. Wait until the firing script actually shoots the shell to check our trajectory
        if (!hasLockedOn && rb.linearVelocity.sqrMagnitude > 10f)
        {
            LockOnToTarget();
            hasLockedOn = true;
        }

        // 2. Apply Custom Gravity
        rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);

        // 3. Smart Steering (The Homing Math)
        if (lockedTarget != null && rb.linearVelocity.sqrMagnitude > 1f)
        {
            Vector3 dirToTarget = (lockedTarget.position - transform.position).normalized;

            // Gently rotate the velocity vector towards the target over time
            Vector3 newVelocity = Vector3.RotateTowards(
                rb.linearVelocity,
                dirToTarget * rb.linearVelocity.magnitude,
                homingStrength * Mathf.Deg2Rad * Time.fixedDeltaTime,
                0f
            );

            rb.linearVelocity = newVelocity;
        }

        // 4. Align the 3D model's nose to the flight path
        if (alignToTrajectory && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }

    private void LockOnToTarget()
    {
        if (!enableSmartAim) return;

        // Find every object in the scene with the "Target" tag
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Target");
        float bestAngle = assistConeAngle;

        foreach (GameObject t in targets)
        {
            Vector3 dirToTarget = (t.transform.position - transform.position).normalized;

            // Compare the exact direction the shell is flying against the direction to the target
            float angle = Vector3.Angle(rb.linearVelocity, dirToTarget);

            // If the target is inside our "Cone of Forgiveness", lock onto it!
            if (angle < bestAngle)
            {
                bestAngle = angle;
                lockedTarget = t.transform;
            }
        }

        if (lockedTarget != null)
        {
            Debug.Log($"Target locked! Correcting a {bestAngle:F1} degree aiming error.");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // The exact millisecond the shell hits ANYTHING, shut down the flight math
        if (!hasHitGround)
        {
            hasHitGround = true;

            // Massively increase Damping to simulate burying into the dirt
            rb.linearDamping = groundStoppingPower;
            rb.angularDamping = groundStoppingPower;
        }
    }
}
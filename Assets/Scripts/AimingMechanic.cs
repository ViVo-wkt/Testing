using UnityEngine;

[ExecuteAlways]
public class AimingMechanic : MonoBehaviour
{
    [Header("Gun Hierarchy")]
    public Transform horizontalCarriage;
    public Transform verticalBarrel;

    [Header("The Cranks")]
    public Transform horizontalCrank;
    public Transform verticalCrank;

    [Header("Mechanical Settings")]
    public float gearingRatio = 0.05f;

    [Header("Aiming Limits")]
    public float maxElevation = 25f;
    public float minElevation = -5f;
    public float maxTraverse = 30f;
    public float minTraverse = -30f;

    [Header("VR Simulation (Draggable)")]
    [Tooltip("Drag this to spin the horizontal crank infinitely. This perfectly mimics the XR Knob in VR!")]
    public float horizontalCrankDegrees = 0f;
    [Tooltip("Drag this to spin the vertical crank infinitely.")]
    public float verticalCrankDegrees = 0f;

    [Header("Live Data (Read Only)")]
    public float currentYaw = 0f;
    public float currentPitch = 0f;

    [Header("Actions")]
    [Tooltip("Check this box to cleanly zero out the gun and cranks.")]
    public bool resetAimToZero = false;
    [Tooltip("Check this box if you manually altered the 3D cranks in the scene view and want the script to learn their new angled mountings.")]
    public bool saveMountingAngles = false;

    // We cache the angled mountings (like Y=7.011) here so they are never lost to Gimbal Lock
    [HideInInspector] public Quaternion horizBaseRot = Quaternion.identity;
    [HideInInspector] public Quaternion vertBaseRot = Quaternion.identity;

    private void OnEnable()
    {
        // Safe-catch to initialize the base mounting rotations
        if (horizBaseRot == Quaternion.identity && horizontalCrank != null) horizBaseRot = horizontalCrank.localRotation;
        if (vertBaseRot == Quaternion.identity && verticalCrank != null) vertBaseRot = verticalCrank.localRotation;
    }

    void Update()
    {
        if (resetAimToZero)
        {
            horizontalCrankDegrees = 0f;
            verticalCrankDegrees = 0f;
            resetAimToZero = false;
        }

        if (saveMountingAngles)
        {
            if (horizontalCrank != null) horizBaseRot = horizontalCrank.localRotation;
            if (verticalCrank != null) vertBaseRot = verticalCrank.localRotation;
            horizontalCrankDegrees = 0f;
            verticalCrankDegrees = 0f;
            saveMountingAngles = false;
            Debug.Log("New crank mounting angles successfully saved!");
        }

        if (horizontalCrank != null && horizontalCarriage != null) HandleHorizontalAiming();
        if (verticalCrank != null && verticalBarrel != null) HandleVerticalAiming();
    }

    private void HandleHorizontalAiming()
    {
        // 1. Where does the gun want to go based on the total crank spin?
        float proposedYaw = horizontalCrankDegrees * gearingRatio;

        // 2. Is that legal?
        if (proposedYaw > maxTraverse || proposedYaw < minTraverse)
        {
            currentYaw = Mathf.Clamp(proposedYaw, minTraverse, maxTraverse);

            // 3. HARD LOCK: Force the Inspector slider to stop moving!
            horizontalCrankDegrees = currentYaw / gearingRatio;
        }
        else
        {
            currentYaw = proposedYaw;
        }

        // 4. Apply Left/Right rotation to the gun
        horizontalCarriage.localEulerAngles = new Vector3(0f, currentYaw, 0f);

        // 5. Apply the spin to the crank ON TOP of its saved mounting angle. 
        // This makes Y-axis drift mathematically impossible!
        Quaternion spin = Quaternion.AngleAxis(horizontalCrankDegrees, Vector3.forward);
        horizontalCrank.localRotation = horizBaseRot * spin;
    }

    private void HandleVerticalAiming()
    {
        float proposedPitch = verticalCrankDegrees * gearingRatio;

        if (proposedPitch > maxElevation || proposedPitch < minElevation)
        {
            currentPitch = Mathf.Clamp(proposedPitch, minElevation, maxElevation);
            verticalCrankDegrees = currentPitch / gearingRatio;
        }
        else
        {
            currentPitch = proposedPitch;
        }

        verticalBarrel.localEulerAngles = new Vector3(currentPitch, 0f, 0f);

        Quaternion spin = Quaternion.AngleAxis(verticalCrankDegrees, Vector3.forward);
        verticalCrank.localRotation = vertBaseRot * spin;
    }
}
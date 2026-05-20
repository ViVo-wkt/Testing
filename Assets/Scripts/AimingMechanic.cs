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

    [HideInInspector] public Quaternion horizBaseRot = Quaternion.identity;
    [HideInInspector] public Quaternion vertBaseRot = Quaternion.identity;

    private void OnEnable()
    {
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
        float proposedYaw = horizontalCrankDegrees * gearingRatio;

        if (proposedYaw > maxTraverse || proposedYaw < minTraverse)
        {
            currentYaw = Mathf.Clamp(proposedYaw, minTraverse, maxTraverse);
            horizontalCrankDegrees = currentYaw / gearingRatio;
        }
        else
        {
            currentYaw = proposedYaw;
        }

        horizontalCarriage.localEulerAngles = new Vector3(0f, currentYaw, 0f);

        // ONLY force-overwrite the crank transform if we are NOT in Play Mode.
        // In VR Play Mode, let the XRKnob script smoothly handle its own object rotation tracking.
        if (!Application.isPlaying)
        {
            Quaternion spin = Quaternion.AngleAxis(horizontalCrankDegrees, Vector3.forward);
            horizontalCrank.localRotation = horizBaseRot * spin;
        }
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

        // ONLY force-overwrite the crank transform if we are NOT in Play Mode.
        if (!Application.isPlaying)
        {
            Quaternion spin = Quaternion.AngleAxis(verticalCrankDegrees, Vector3.forward);
            verticalCrank.localRotation = vertBaseRot * spin;
        }
    }

    // VR Bridges for the Cranks
    public void SetHorizontalCrankFromVR(float knobValue)
    {
        horizontalCrankDegrees = knobValue * 360f;
    }

    public void SetVerticalCrankFromVR(float knobValue)
    {
        verticalCrankDegrees = knobValue * 360f;
    }

    [ContextMenu("Reset Aim To Zero")]
    public void ResetAim()
    {
        if (horizontalCrank != null)
        {
            float totalHorizontalSpin = currentYaw / gearingRatio;
            horizontalCrank.Rotate(0f, 0f, -totalHorizontalSpin, Space.Self);
            horizBaseRot = horizontalCrank.localRotation;
        }

        if (verticalCrank != null)
        {
            float totalVerticalSpin = currentPitch / gearingRatio;
            verticalCrank.Rotate(0f, 0f, -totalVerticalSpin, Space.Self);
            vertBaseRot = verticalCrank.localRotation;
        }

        currentYaw = 0f;
        currentPitch = 0f;
        horizontalCrankDegrees = 0f;
        verticalCrankDegrees = 0f;

        if (horizontalCarriage != null) horizontalCarriage.localEulerAngles = Vector3.zero;
        if (verticalBarrel != null) verticalBarrel.localEulerAngles = Vector3.zero;
    }
}
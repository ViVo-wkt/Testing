using UnityEngine;

[ExecuteAlways]
public class AimingMechanic : MonoBehaviour
{
    [Header("Gun Hierarchy")]
    public Transform horizontalCarriage;
    public Transform verticalBarrel;

    [Header("The Cranks (_local containers)")]
    [Tooltip("Assign your '*_local' correction objects here (e.g., horizontal_aiming_handle_local).")]
    public Transform horizontalCrankLocal;
    [Tooltip("Assign your vertical '*_local' correction object here.")]
    public Transform verticalCrankLocal;

    [Header("Mechanical Settings")]
    public float gearingRatio = 0.05f;

    [Header("Aiming Limits")]
    public float maxElevation = 25f;
    public float minElevation = -5f;
    public float maxTraverse = 30f;
    public float minTraverse = -30f;

    [Header("VR Tuning")]
    [Tooltip("If the gun rotates too slowly when twisting in VR, increase this multiplier.")]
    public float vrSpeedMultiplier = 1f;

    [Header("Live Data (Read Only)")]
    public float currentYaw = 0f;
    public float currentPitch = 0f;

    [Header("Actions")]
    public bool resetAimToZero = false;
    public bool saveMountingAngles = false;

    [HideInInspector] public Quaternion horizBaseRot = Quaternion.identity;
    [HideInInspector] public Quaternion vertBaseRot = Quaternion.identity;

    // Track historical tracking values to isolate clean mathematical deltas
    private float lastVRHorizDegrees;
    private float lastVRVertDegrees;
    private bool isFirstFrameHoriz = true;
    private bool isFirstFrameVert = true;

    private void OnEnable()
    {
        if (horizBaseRot == Quaternion.identity && horizontalCrankLocal != null) horizBaseRot = horizontalCrankLocal.localRotation;
        if (vertBaseRot == Quaternion.identity && verticalCrankLocal != null) vertBaseRot = verticalCrankLocal.localRotation;
    }

    void Update()
    {
        if (resetAimToZero)
        {
            currentYaw = 0f;
            currentPitch = 0f;
            resetAimToZero = false;
        }

        if (saveMountingAngles)
        {
            if (horizontalCrankLocal != null) horizBaseRot = horizontalCrankLocal.localRotation;
            if (verticalCrankLocal != null) vertBaseRot = verticalCrankLocal.localRotation;
            saveMountingAngles = false;
            Debug.Log("New local container mounting angles successfully saved!");
        }

        // Apply core logic structures
        currentYaw = Mathf.Clamp(currentYaw, minTraverse, maxTraverse);
        currentPitch = Mathf.Clamp(currentPitch, minElevation, maxElevation);

        if (horizontalCarriage != null) horizontalCarriage.localEulerAngles = new Vector3(0f, currentYaw, 0f);
        if (verticalBarrel != null) verticalBarrel.localEulerAngles = new Vector3(currentPitch, 0f, 0f);

        // In the Editor view, drive visual followers safely from variables
        if (!Application.isPlaying)
        {
            if (horizontalCrankLocal != null)
            {
                float targetCrankDegrees = currentYaw / gearingRatio;
                horizontalCrankLocal.localRotation = horizBaseRot * Quaternion.AngleAxis(targetCrankDegrees, Vector3.forward);
            }
            if (verticalCrankLocal != null)
            {
                float targetCrankDegrees = currentPitch / gearingRatio;
                verticalCrankLocal.localRotation = vertBaseRot * Quaternion.AngleAxis(targetCrankDegrees, Vector3.forward);
            }
        }
    }

    // --- REWORKED DELTA VR BRIDGES ---
    // Safely captures how much your hand turned, applies it to the gun,
    // and completely stops feedback looping by ignoring absolute positions.
    public void SetHorizontalCrankFromVR(float knobValue)
    {
        if (!Application.isPlaying) return;

        float currentDegrees = knobValue * 360f;

        if (isFirstFrameHoriz)
        {
            lastVRHorizDegrees = currentDegrees;
            isFirstFrameHoriz = false;
            return;
        }

        // Isolate frame-to-frame movement delta safely wrapping around 360 boundaries
        float deltaDegrees = Mathf.DeltaAngle(lastVRHorizDegrees, currentDegrees);

        // Suppress massive tracking initialization jumps on fresh selections
        if (Mathf.Abs(deltaDegrees) < 180f)
        {
            currentYaw += deltaDegrees * gearingRatio * vrSpeedMultiplier;
        }

        lastVRHorizDegrees = currentDegrees;
    }

    public void SetVerticalCrankFromVR(float knobValue)
    {
        if (!Application.isPlaying) return;

        float currentDegrees = knobValue * 360f;

        if (isFirstFrameVert)
        {
            lastVRVertDegrees = currentDegrees;
            isFirstFrameVert = false;
            return;
        }

        float deltaDegrees = Mathf.DeltaAngle(lastVRVertDegrees, currentDegrees);

        if (Mathf.Abs(deltaDegrees) < 180f)
        {
            currentPitch += deltaDegrees * gearingRatio * vrSpeedMultiplier;
        }

        lastVRVertDegrees = currentDegrees;
    }

    [ContextMenu("Reset Aim To Zero")]
    public void ResetAim()
    {
        currentYaw = 0f;
        currentPitch = 0f;
        isFirstFrameHoriz = true;
        isFirstFrameVert = true;
        if (horizontalCarriage != null) horizontalCarriage.localEulerAngles = Vector3.zero;
        if (verticalBarrel != null) verticalBarrel.localEulerAngles = Vector3.zero;
    }
}
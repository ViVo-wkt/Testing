using UnityEngine;

[ExecuteAlways]
public class BreechMechanic : MonoBehaviour
{
    [Header("Mechanical Parts")]
    public Transform breechBlock;
    public Transform breechHandle;

    [Header("Breech Block Limits")]
    public Vector3 blockClosedPosition;
    public Vector3 blockOpenPosition;

    [Header("Handle Limits")]
    public Vector3 handleClosedRotation;
    public Vector3 handleOpenRotation;

    [Header("Testing Controls")]
    [Range(0f, 1f)]
    public float openProgress = 0f;

    [Header("System Links")]
    [Tooltip("Link the Firing Mechanic script here so the breech can tell it when to eject!")]
    public FiringMechanic firingSystem;

    // Tracks where the breech was a millisecond ago
    private float lastProgress = 0f;

    private void OnValidate()
    {
        UpdateMechanicalParts();
    }

    private void Update()
    {
        // This ensures the slider works in Play Mode too!
        if (Application.isPlaying)
        {
            UpdateMechanicalParts();
        }
    }

    public void UpdateMechanicalParts()
    {
        if (breechBlock != null)
            breechBlock.localPosition = Vector3.Lerp(blockClosedPosition, blockOpenPosition, openProgress);

        if (breechHandle != null)
        {
            Quaternion closedRot = Quaternion.Euler(handleClosedRotation);
            Quaternion openRot = Quaternion.Euler(handleOpenRotation);
            breechHandle.localRotation = Quaternion.Slerp(closedRot, openRot, openProgress);
        }

        // --- EJECTION TRIGGER LOGIC ---
        // If the breech just passed the 80% open mark, trigger the ejection!
        if (openProgress > 0.8f && lastProgress <= 0.8f)
        {
            if (firingSystem != null)
            {
                firingSystem.Eject();
            }
        }

        lastProgress = openProgress;
    }
}
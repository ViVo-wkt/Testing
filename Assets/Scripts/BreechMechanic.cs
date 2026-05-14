using UnityEngine;

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
    [Tooltip("Drag this slider to test the animation! 0 is closed, 1 is open.")]
    public float openProgress = 0f;

    // OnValidate runs instantly in the Editor whenever you change a value in the Inspector.
    // This allows us to preview the animation without hitting Play!
    private void OnValidate()
    {
        UpdateMechanicalParts();
    }

    public void UpdateMechanicalParts()
    {
        if (breechBlock != null)
        {
            // Lerp smoothly slides the block between point A and point B based on our 0-1 progress
            breechBlock.localPosition = Vector3.Lerp(blockClosedPosition, blockOpenPosition, openProgress);
        }

        if (breechHandle != null)
        {
            // Slerp smoothly rotates the handle between angle A and angle B
            Quaternion closedRot = Quaternion.Euler(handleClosedRotation);
            Quaternion openRot = Quaternion.Euler(handleOpenRotation);
            breechHandle.localRotation = Quaternion.Slerp(closedRot, openRot, openProgress);
        }
    }
}
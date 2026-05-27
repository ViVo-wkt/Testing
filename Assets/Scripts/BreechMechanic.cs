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

    [Header("Animation Settings")]
    [Tooltip("How fast the breech automatically opens/closes when clicked.")]
    public float animationSpeed = 4f;

    [Header("System Links")]
    [Tooltip("Link the Firing Mechanic script here so the breech can tell it when to eject!")]
    public FiringMechanic firingSystem;

    private float lastProgress = 0f;

    // Tracks what state the breech is trying to reach automatically (0 = closed, 1 = open)
    private float targetProgress = 0f;

    private void OnValidate()
    {
        UpdateMechanicalParts();

        // If you drag the slider manually in the Editor, update the target so it doesn't fight you
        if (!Application.isPlaying)
        {
            targetProgress = openProgress;
        }
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            // Smoothly animate the progress towards the target over time
            if (openProgress != targetProgress)
            {
                openProgress = Mathf.MoveTowards(openProgress, targetProgress, animationSpeed * Time.deltaTime);
                UpdateMechanicalParts();
            }
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

    // --- NEW VR BRIDGE ---
    // Call this from the XR Simple Interactable's "Select Entered" event
    public void ToggleBreechFromVR()
    {
        if (targetProgress > 0.5f)
        {
            // It is mostly open, so tell the script to close it
            targetProgress = 0f;
        }
        else
        {
            // It is mostly closed, so tell the script to open it
            targetProgress = 1f;
        }
    }
}
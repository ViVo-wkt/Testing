using UnityEngine;
using UnityEngine.InputSystem;
// NEW: We need to include the VR namespace so the script can talk to the Socket!
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class FiringMechanic : MonoBehaviour
{
    public enum ChamberState { Empty, Loaded, Spent }

    [Header("Current State")]
    public ChamberState currentState = ChamberState.Empty;
    public BreechMechanic breech;

    [Header("VR Integration")]
    [Tooltip("Drag your Chamber_Socket here so the gun can consume the loaded shell!")]
    public XRSocketInteractor ammoSocket;

    [Header("Static Chamber Meshes")]
    public GameObject staticFullShellMesh;
    public GameObject staticEmptyCasingMesh;

    [Header("Firing Physics")]
    public GameObject projectilePrefab;
    public Transform barrelTip;
    public float firingForce = 150f;
    public Vector3 projectileRotationOffset = new Vector3(-90f, 0f, 0f);

    [Header("Ejection Physics")]
    public GameObject ejectedCasingPrefab;
    public Transform ejectionPoint;
    public float ejectionForce = 15f;
    public float ejectionSpin = 10f;

    [Header("Visual Effects (Game Juice)")]
    public ParticleSystem muzzleFlash;
    public ParticleSystem breechSmoke;

    [Tooltip("Assign the visual root of the gun or carriage here to roll the entire gun backward.")]
    public Transform recoilObject;
    public float recoilDistance = 0.5f;
    public float recoilReturnSpeed = 2f;

    private Vector3 originalLocalPos;

    void Start()
    {
        UpdateChamberVisuals();

        if (recoilObject != null)
        {
            originalLocalPos = recoilObject.localPosition;
        }
    }

    void Update()
    {
        if (recoilObject != null)
        {
            recoilObject.localPosition = Vector3.Lerp(
                recoilObject.localPosition,
                originalLocalPos,
                Time.deltaTime * recoilReturnSpeed
            );
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame) LoadGun();
            if (Keyboard.current.spaceKey.wasPressedThisFrame) FireGun();
        }
    }

    [ContextMenu("Load Gun")]
    public void LoadGun()
    {
        // Only load if the chamber is empty AND the breech is open
        if (currentState == ChamberState.Empty && breech.openProgress > 0.9f)
        {
            currentState = ChamberState.Loaded;
            UpdateChamberVisuals();

            // --- THE FIX: CONSUME THE PHYSICAL SHELL ---
            if (ammoSocket != null && ammoSocket.hasSelection)
            {
                // Grab the physical shell prefab that the socket is currently holding
                GameObject physicalShell = ammoSocket.firstInteractableSelected.transform.gameObject;

                // Destroy it out of existence! 
                // (The XR Toolkit is smart enough to cleanly release it when it gets destroyed)
                Destroy(physicalShell);
            }
        }
    }

    [ContextMenu("Fire Gun")]
    public void FireGun()
    {
        if (currentState == ChamberState.Loaded && breech.openProgress < 0.1f)
        {
            currentState = ChamberState.Spent;
            UpdateChamberVisuals();

            if (projectilePrefab != null && barrelTip != null)
            {
                Quaternion spawnRotation = barrelTip.rotation * Quaternion.Euler(projectileRotationOffset);
                GameObject proj = Instantiate(projectilePrefab, barrelTip.position, spawnRotation);

                Rigidbody rb = proj.GetComponent<Rigidbody>();
                if (rb != null) rb.AddForce(barrelTip.forward * firingForce, ForceMode.Impulse);
            }

            if (muzzleFlash != null) muzzleFlash.Play();

            if (recoilObject != null)
            {
                recoilObject.localPosition = originalLocalPos + new Vector3(0, 0, -recoilDistance);
            }
        }
    }

    public void Eject()
    {
        if (currentState == ChamberState.Spent)
        {
            currentState = ChamberState.Empty;
            UpdateChamberVisuals();

            if (ejectedCasingPrefab != null && ejectionPoint != null)
            {
                GameObject casing = Instantiate(ejectedCasingPrefab, ejectionPoint.position, ejectionPoint.rotation);
                Rigidbody rb = casing.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(ejectionPoint.forward * ejectionForce, ForceMode.Impulse);
                    rb.AddTorque(Random.insideUnitSphere * ejectionSpin, ForceMode.Impulse);
                }
            }

            if (breechSmoke != null) breechSmoke.Play();
        }
    }

    private void UpdateChamberVisuals()
    {
        if (staticFullShellMesh) staticFullShellMesh.SetActive(currentState == ChamberState.Loaded);
        if (staticEmptyCasingMesh) staticEmptyCasingMesh.SetActive(currentState == ChamberState.Spent);
    }
}
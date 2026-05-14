using UnityEngine;
using UnityEngine.InputSystem; // Added the modern Input System namespace!

public class FiringMechanic : MonoBehaviour
{
    public enum ChamberState { Empty, Loaded, Spent }

    [Header("Current State")]
    public ChamberState currentState = ChamberState.Empty;
    public BreechMechanic breech;

    [Header("Static Chamber Meshes")]
    public GameObject staticFullShellMesh;
    public GameObject staticEmptyCasingMesh;

    [Header("Firing Physics")]
    public GameObject projectilePrefab;
    public Transform barrelTip;
    public float firingForce = 150f;

    [Header("Ejection Physics")]
    public GameObject ejectedCasingPrefab;
    public Transform ejectionPoint;
    public float ejectionForce = 5f;
    public float ejectionSpin = 10f;

    void Start()
    {
        UpdateChamberVisuals();
    }

    void Update()
    {
        // --- FIXED: Modern Keyboard controls for rapid testing in Play Mode ---
        if (Keyboard.current != null)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame) LoadGun();
            if (Keyboard.current.spaceKey.wasPressedThisFrame) FireGun();
        }
    }

    [ContextMenu("1. Load Gun")]
    public void LoadGun()
    {
        if (currentState == ChamberState.Empty && breech.openProgress > 0.9f)
        {
            currentState = ChamberState.Loaded;
            UpdateChamberVisuals();
            Debug.Log("Gun Loaded!");
        }
        else
        {
            Debug.LogWarning("Cannot load: Gun is not empty or breech is closed.");
        }
    }

    [ContextMenu("2. Fire Gun")]
    public void FireGun()
    {
        if (currentState == ChamberState.Loaded && breech.openProgress < 0.1f)
        {
            currentState = ChamberState.Spent;
            UpdateChamberVisuals();

            if (projectilePrefab != null && barrelTip != null)
            {
                GameObject proj = Instantiate(projectilePrefab, barrelTip.position, barrelTip.rotation);
                Rigidbody rb = proj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(barrelTip.forward * firingForce, ForceMode.Impulse);
                }
            }

            Debug.Log("BOOM!");
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

            Debug.Log("Casing Ejected!");
        }
    }

    private void UpdateChamberVisuals()
    {
        if (staticFullShellMesh) staticFullShellMesh.SetActive(currentState == ChamberState.Loaded);
        if (staticEmptyCasingMesh) staticEmptyCasingMesh.SetActive(currentState == ChamberState.Spent);
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

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
    public Vector3 projectileRotationOffset = new Vector3(-90f, 0f, 0f);

    [Header("Ejection Physics")]
    public GameObject ejectedCasingPrefab;
    public Transform ejectionPoint;
    public float ejectionForce = 15f;
    public float ejectionSpin = 10f;

    void Start()
    {
        UpdateChamberVisuals();
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame) LoadGun();
            if (Keyboard.current.spaceKey.wasPressedThisFrame) FireGun();
        }
    }

    [ContextMenu("Load Gun")]
    public void LoadGun()
    {
        if (currentState == ChamberState.Empty && breech.openProgress > 0.9f)
        {
            currentState = ChamberState.Loaded;
            UpdateChamberVisuals();
            Debug.Log("Gun Loaded!");
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
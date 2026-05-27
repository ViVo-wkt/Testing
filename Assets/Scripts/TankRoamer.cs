using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TankRoamer : MonoBehaviour
{
    [Header("Roaming Settings")]
    [Tooltip("How far from its starting point the tank is allowed to wander.")]
    public float roamRadius = 30f;
    [Tooltip("Time in seconds before the tank decides to pick a new destination.")]
    public float changeDestinationInterval = 5f;

    [Header("Movement Settings")]
    [Tooltip("Forward movement speed.")]
    public float moveSpeed = 5f;
    [Tooltip("How fast the tank rotates (degrees per second).")]
    public float turnSpeed = 120f;
    [Tooltip("How quickly the tank reaches its max speed.")]
    public float acceleration = 8f;

    [Header("Tank Behavior")]
    [Tooltip("If the turn is sharper than this angle, the tank stops to turn in place.")]
    public float maxTurnAngle = 15f;
    [Tooltip("How many points to sample when picking a destination to ensure a mild turn.")]
    public int destinationSamples = 5;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        timer = changeDestinationInterval;

        // Disable automatic rotation so we can handle it manually for that heavy tank feel
        agent.updateRotation = false;

        agent.speed = moveSpeed;
        agent.acceleration = acceleration;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Pick a new destination if the timer runs out OR if the tank has reached its current goal
        if (timer >= changeDestinationInterval || (!agent.pathPending && agent.remainingDistance < 0.5f))
        {
            SetNewRandomDestination();
            timer = 0f;
        }

        // Run our custom tank movement logic every frame
        HandleTankMovement();
    }

    void HandleTankMovement()
    {
        // agent.steeringTarget is the immediate next corner on the path
        Vector3 directionToTarget = agent.steeringTarget - transform.position;
        directionToTarget.y = 0f; // Keep the direction strictly horizontal

        // Only rotate if we actually have somewhere to go
        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            // --- ROTATION LOGIC ---
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            // --- MOVEMENT LOGIC ---
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            if (angle > maxTurnAngle)
            {
                // If the turn is too sharp, cut the engine speed to 0 so we pivot in place
                agent.speed = 0f;
            }
            else
            {
                // Once we are facing the right way, hit the gas
                agent.speed = moveSpeed;
            }
        }
        else
        {
            // Stop moving if we have arrived
            agent.speed = 0f;
        }
    }

    void SetNewRandomDestination()
    {
        Vector3 bestDestination = transform.position;
        float smallestAngle = 360f;
        bool foundValidPoint = false;

        // Generate a few potential destinations and score them to avoid constant 180-degree turns
        for (int i = 0; i < destinationSamples; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
            randomDirection += startPosition;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, NavMesh.AllAreas))
            {
                // Calculate the direction from the tank's current position to this new point
                Vector3 directionToHit = hit.position - transform.position;
                directionToHit.y = 0f;

                // Find out how many degrees the tank would have to turn to face this point
                float angleToTurn = Vector3.Angle(transform.forward, directionToHit);

                // If this is the mildest turn we've found so far, save it as the new best option
                if (angleToTurn < smallestAngle)
                {
                    smallestAngle = angleToTurn;
                    bestDestination = hit.position;
                    foundValidPoint = true;
                }
            }
        }

        // Apply the best destination we found
        if (foundValidPoint)
        {
            agent.SetDestination(bestDestination);
        }
    }
}
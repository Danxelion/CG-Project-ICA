using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 8f;
    public float acceleration = 40f;
    public float friction = 25f;
    public float playerRadius = 0.5f;
    public bool isometricScreenAligned = true;

    private Vector3 currentVelocity = Vector3.zero;
    private Wall[] arenaWalls;

    void Start()
    {
        // Find all custom walls in the scene
        arenaWalls = Object.FindObjectsByType<Wall>(FindObjectsSortMode.None);
    }

    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(inputX, 0f, inputZ).normalized;

        if (isometricScreenAligned && inputDir.sqrMagnitude > 0.001f)
        {
            inputDir = Quaternion.Euler(0f, 45f, 0f) * inputDir;
        }

        // Time Integration (Replaces Rigidbody Velocity)
        float dt = Time.deltaTime;
        if (inputDir.sqrMagnitude > 0.001f)
        {
            Vector3 desiredVelocity = inputDir * maxSpeed;
            currentVelocity = Vector3.MoveTowards(currentVelocity, desiredVelocity, acceleration * dt);
        }
        else
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, friction * dt);
        }

        // Next position
        Vector3 projectedPosition = transform.position + currentVelocity * dt;

        // Collision Solver (Replaces Unity Physics Colliders)
        projectedPosition = ResolveCollisions(projectedPosition);

        // Final position
        transform.position = projectedPosition;
    }

    private Vector3 ResolveCollisions(Vector3 targetPos)
    {
        Vector2 circleCenter = new Vector2(targetPos.x, targetPos.z);

        foreach (Wall wall in arenaWalls)
        {
            if (wall == null) continue;

            // Find closest point on the wall to the player
            float clampedX = Mathf.Clamp(circleCenter.x, wall.minBound.x, wall.maxBound.x);
            float clampedZ = Mathf.Clamp(circleCenter.y, wall.minBound.y, wall.maxBound.y);
            Vector2 closestPoint = new Vector2(clampedX, clampedZ);

            Vector2 delta = circleCenter - closestPoint;
            float sqrDistance = delta.sqrMagnitude;

            // Check if distance is less than radius squared
            if (sqrDistance < (playerRadius * playerRadius))
            {
                float distance = Mathf.Sqrt(sqrDistance);
                Vector2 normal = (distance < 0.0001f) ? Vector2.up : delta / distance;

                // Push player out of the wall
                circleCenter = closestPoint + normal * playerRadius;

                // Cancel velocity going into the wall 
                Vector3 normal3D = new Vector3(normal.x, 0f, normal.y);
                float velocityAlongNormal = Vector3.Dot(currentVelocity, normal3D);
                if (velocityAlongNormal < 0f)
                {
                    currentVelocity -= normal3D * velocityAlongNormal;
                }
            }
        }
        return new Vector3(circleCenter.x, targetPos.y, circleCenter.y);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, playerRadius);
    }
}
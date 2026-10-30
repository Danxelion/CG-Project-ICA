using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 8f;
    public float acceleration = 40f;
    public float friction = 25f;
    public float playerRadius = 0.5f;
    public bool isometricScreenAligned = true;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    private float dashTimer = 0f;
    private bool isDashing = false;

    [Header("Attack Movement Settings")]
    [SerializeField, Range(0.1f, 1f)] public float attackSpeedMultiplier = 0.4f; 
    private PlayerMeleeAttack meleeAttack;

    private Vector3 currentVelocity = Vector3.zero;
    private Wall[] arenaWalls;
    private Animator animator;

    void Start()
    {
        arenaWalls = Object.FindObjectsByType<Wall>(FindObjectsSortMode.None); // Find all custom walls in the scene
        animator = GetComponentInChildren<Animator>();
        meleeAttack = GetComponent<PlayerMeleeAttack>();
    }

    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(inputX, 0f, inputZ).normalized;

        bool hasInput = inputDir.sqrMagnitude > 0.001f;

        if (isometricScreenAligned && hasInput)
        {
            inputDir = Quaternion.Euler(0f, 45f, 0f) * inputDir;
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isDashing)
        {
            isDashing = true;
            dashTimer = dashDuration;
            
            if (animator != null) animator.SetTrigger("Dash");

            // If moving, dash in movement direction. If standing still, dash in facing direction.
            Vector3 dashDirection = hasInput ? inputDir : transform.forward;
            currentVelocity = dashDirection * dashSpeed;
        }

        // Time Integration (Replaces Rigidbody Velocity)
        float dt = Time.deltaTime;

        if (isDashing)
        {
            dashTimer -= dt;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
            // Slide friction during dash
            currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, (friction * 0.5f) * dt);
        }
        else 
        {
            if (animator != null) animator.SetBool("isMoving", hasInput);

            if (hasInput)
            {
                // Calculate effective max speed
                float effectiveSpeed = maxSpeed;
                // Check if currently attacking -> apply penalty
                if (meleeAttack != null && meleeAttack.IsAttacking)
                {
                    effectiveSpeed *= attackSpeedMultiplier;
                }
                Vector3 desiredVelocity = inputDir * effectiveSpeed;
                currentVelocity = Vector3.MoveTowards(currentVelocity, desiredVelocity, acceleration * dt);
            }
            else
            {
                currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, friction * dt);
            }
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
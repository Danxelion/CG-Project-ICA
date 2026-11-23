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
    private CharacterController controller;
    private Animator animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        meleeAttack = GetComponentInChildren<PlayerMeleeAttack>();
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

            if (meleeAttack != null) meleeAttack.CancelAttack();
            
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

        // Lock vertical velocity to stay strictly on the ground plane
        currentVelocity.y = 0f;

        // Unity handles wall collision and smooth surface sliding automatically
        controller.Move(currentVelocity * dt);
    }
}
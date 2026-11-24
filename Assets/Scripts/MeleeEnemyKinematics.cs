using UnityEngine;
using System;

public class MeleeEnemyKinematics : MonoBehaviour
{
    public enum EnemyState
    {
        Spawning,
        Idle,
        Chasing,
        Attacking,
        Dead
    }
    
    public Transform playerTarget;
    public float moveSpeed = 3.0f;
    public float attackRange = 1.5f;
    [SerializeField, Range(10f, 180f)] public float attackAngleTotal = 75f;
    public float attackDamage = 15f;
    public float attackCooldown = 1.0f;
    private float nextAttackTime = 0f;
    public float enemyRadius = 0.5f;
    private float stateTimer = 0f;
    public float maxAttackDuration = 1.0f; // Fallback if AttackFinished event is interrupted 
    public float spawnDuration = 1.0f; // Fallback if Spawn animation event is interrupted or skipped
    
    private CharacterController controller;
    private EnemyHealth health;
    private EnemyState currentState = EnemyState.Spawning;

    // Events for the Child Animator Bridge (View)
    public event Action OnAttackStarted;
    public event Action<bool> OnMovingChanged;

    private bool wasMoving = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<EnemyHealth>();
    }

    void OnEnable()
    {
        health.OnDeath += HandleDeath;
    }

    void OnDisable()
    {
        health.OnDeath -= HandleDeath;
    }

    void Start()
    {
        currentState = EnemyState.Spawning;
    }

    void Update()
    {
        if (currentState == EnemyState.Dead) return;

        // Spawning State
        if (currentState == EnemyState.Spawning)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                OnSpawnAnimationFinished();
            }
            return;
        }

        // Attacking State
        if (currentState == EnemyState.Attacking)
        {
            SetMoving(false);
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                OnAttackAnimationFinished();
            }
            return;
        }

        if (playerTarget == null)
        {
            currentState = EnemyState.Idle;
            SetMoving(false);
            return;
        }

        // Calculate the displacement vector (d = player position - enemy position)
        float dx = playerTarget.position.x - transform.position.x;
        float dz = playerTarget.position.z - transform.position.z;

        // Calculate the magnitude / distance (||d||)
        float distance = Mathf.Sqrt(dx * dx + dz * dz);

        // Check if the character is outside the range => Chase
        if (distance > attackRange)
        {
            currentState = EnemyState.Chasing;
            SetMoving(true);
            // Get the unit vector to get the direction (v_u = d / ||d||)
            float dirX = dx / distance;
            float dirZ = dz / distance;

            /// Displacement step (v_u * speed * dt) passed to Unity's built-in CharacterController
            Vector3 displacement = new Vector3(dirX, 0f, dirZ) * (moveSpeed * Time.deltaTime);
            controller.Move(displacement);

            // Rotate to face player
            float angle = Mathf.Atan2(dx, dz) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        else // Inside attack range => Attack
        {
            SetMoving(false);

            if (Time.time >= nextAttackTime)
            {
                StartAttack();
            }
            else
            {
                currentState = EnemyState.Idle;
            }
        }
    }

    private void StartAttack()
    {
        currentState = EnemyState.Attacking;
        stateTimer = maxAttackDuration;
        nextAttackTime = Time.time + attackCooldown;
        OnAttackStarted?.Invoke();
    }

    // Called by the child Animator Bridge at the exact impact frame of the swing animation
    // Evaluates range and the dot-product attack cone (f . d >= cos(theta / 2))
    public void ExecuteAttackHit()
    {
        if (currentState == EnemyState.Dead || playerTarget == null) return;

        float dx = playerTarget.position.x - transform.position.x;
        float dz = playerTarget.position.z - transform.position.z;
        float distance = Mathf.Sqrt(dx * dx + dz * dz);

        // Check slightly generous hit range (+0.2f) so player backing away slightly feels fair
        if (distance <= attackRange + 0.2f && distance > 0.0001f)
        {
            Vector3 enemyForward = transform.forward;
            enemyForward.y = 0f;
            enemyForward.Normalize();

            Vector3 dirToPlayer = new Vector3(dx / distance, 0f, dz / distance);

            // Dot product test: f . d = ||f|| ||d|| cos(alpha)
            float dotProduct = Vector3.Dot(enemyForward, dirToPlayer);
            float cosHalfAngle = Mathf.Cos((attackAngleTotal * 0.5f) * Mathf.Deg2Rad);

            if (dotProduct >= cosHalfAngle)
            {
                IDamageable target = playerTarget.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(attackDamage);
                }
            }
        }
    }

    public void OnSpawnAnimationFinished()
    {
        if (currentState == EnemyState.Spawning)
        {
            currentState = EnemyState.Idle;
        }
    }

    public void OnAttackAnimationFinished()
    {
        if (currentState == EnemyState.Attacking)
        {
            currentState = EnemyState.Idle;
        }
    }

    private void HandleDeath()
    {
        currentState = EnemyState.Dead;
        SetMoving(false);
    }

    private void SetMoving(bool moving)
    {
        if (wasMoving != moving)
        {
            wasMoving = moving;
            OnMovingChanged?.Invoke(moving);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 pos = transform.position;
        Vector3 fwd = transform.forward;
        fwd.y = 0f;
        fwd.Normalize();

        float halfAngle = attackAngleTotal * 0.5f;
        Vector3 leftRay = Quaternion.Euler(0f, -halfAngle, 0f) * fwd * attackRange;
        Vector3 rightRay = Quaternion.Euler(0f, halfAngle, 0f) * fwd * attackRange;

        Gizmos.DrawRay(pos, leftRay);
        Gizmos.DrawRay(pos, rightRay);
        Gizmos.DrawWireSphere(pos, attackRange);
    }
}
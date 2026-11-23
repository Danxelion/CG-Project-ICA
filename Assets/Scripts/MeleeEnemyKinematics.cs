using UnityEngine;

public class MeleeEnemyKinematics : MonoBehaviour
{
    public Transform playerTarget;
    public float moveSpeed = 3.0f;
    public float attackRange = 1.5f;
    public float attackAngleTotal = 75f;
    public float attackDamage = 15f;
    public float attackCooldown = 1.0f;
    private float nextAttackTime = 0f;
    public float enemyRadius = 0.5f;
    
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (playerTarget == null) return;

        // Calculate the displacement vector (d = player position - enemy position)
        float dx = playerTarget.position.x - transform.position.x;
        float dz = playerTarget.position.z - transform.position.z;

        // Calculate the magnitude / distance (||d||)
        float distance = Mathf.Sqrt(dx * dx + dz * dz);

        // Check if the character is outside the range => Chase
        if (distance > attackRange)
        {
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
            if (Time.time >= nextAttackTime)
            {
                Vector3 enemyForward = transform.forward;
                enemyForward.y = 0f;
                enemyForward.Normalize();

                Vector3 dirToPlayer = new Vector3(dx / distance, 0f, dz / distance);
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

                nextAttackTime = Time.time + attackCooldown;
            }
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
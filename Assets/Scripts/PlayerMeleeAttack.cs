using UnityEngine;
using System;

public class PlayerMeleeAttack : MonoBehaviour
{
    [Header("Attack Parameters")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField, Range(10f, 180f)] private float attackAngleTotal = 90f; 
    [SerializeField] private float attackDamage = 35f;
    [SerializeField] private float attackCooldown = 0.3f;

    private float nextAttackTime = 0f;
    private Animator animator;
    // Decoupled event hooks for visual effects / audio / UI
    public event Action OnAttackFired;
    public event Action<Vector3> OnTargetHit;

    void Start()
    {
        animator = GetComponentInChildren<Animator>(); // Grab animator from child model
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            ExecuteMeleeAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void ExecuteMeleeAttack()
    {
        if (animator != null)
        {
            // Clears any leftover input queue so clicks don't double-trigger
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }
        
        OnAttackFired?.Invoke();

        // Find all damageable entities in the scene
        MonoBehaviour[] candidates = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        Vector3 playerPos = transform.position;
        Vector3 playerForward = transform.forward;
        playerForward.y = 0f;
        playerForward.Normalize();

        float cosHalfAngle = Mathf.Cos((attackAngleTotal * 0.5f) * Mathf.Deg2Rad);
        float sqrRange = attackRange * attackRange;

        foreach (MonoBehaviour mb in candidates)
        {
            if (mb.gameObject == this.gameObject) continue;

            if (mb is IDamageable target)
            {
                Vector3 toTarget = mb.transform.position - playerPos;
                toTarget.y = 0f; 

                // Radial Distance Test 
                float sqrDist = toTarget.sqrMagnitude;
                if (sqrDist > sqrRange || sqrDist < 0.0001f)
                {
                    continue;
                }

                // Angular Alignment Test 
                Vector3 dirToTarget = toTarget.normalized;
                float dotProduct = Vector3.Dot(playerForward, dirToTarget);

                if (dotProduct >= cosHalfAngle)
                {
                    // Target is confirmed inside the attack cone
                    target.TakeDamage(attackDamage);
                    OnTargetHit?.Invoke(mb.transform.position);
                }
            }
        }
    }

    // Visualize the mathematical cone directly in the Unity Editor for debugging and defense
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        float halfAngle = attackAngleTotal * 0.5f;
        Quaternion leftRot = Quaternion.Euler(0, -halfAngle, 0);
        Quaternion rightRot = Quaternion.Euler(0, halfAngle, 0);

        Vector3 leftRay = leftRot * forward * attackRange;
        Vector3 rightRay = rightRot * forward * attackRange;

        Gizmos.DrawRay(pos, leftRay);
        Gizmos.DrawRay(pos, rightRay);
        Gizmos.DrawWireSphere(pos, attackRange);
    }
}
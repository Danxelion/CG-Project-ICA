using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerMeleeAttack : MonoBehaviour
{
    [Header("Attack Parameters")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField, Range(10f, 180f)] private float attackAngleTotal = 90f; 
    [SerializeField] private float attackDamage = 35f;
    [SerializeField] private float attackCooldown = 0.3f;

    [Header("Attack Movement Penalty")]
    [SerializeField] private float attackDuration = 0.4f;

    private float attackEndTime = 0f;
    private float nextAttackTime = 0f;
    private Animator animator;
    // Decoupled event hooks for visual effects / audio / UI
    public event Action OnAttackFired;
    public event Action<Vector3> OnTargetHit;

    private bool isCancelled = false; // Explicit flag for dash interruptions
    public bool IsAttacking => Time.time < attackEndTime;
    
    void Start()
    {
        animator = GetComponent<Animator>();
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
        isCancelled = false; // Reset flag for new attack
        attackEndTime = Time.time + attackDuration;
        
        if (animator != null)
        {
            // Clears any leftover input queue so clicks don't double-trigger
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }
        
        OnAttackFired?.Invoke();
    }

    public void CancelAttack()
    {
        isCancelled = true;
        attackEndTime = 0f;
    }

   // Triggered by the Unity Animation Event on the swing's impact frame
    public void ApplyMeleeDamage()
    {   
        if (isCancelled) return;

        Transform owner = transform.parent != null ? transform.parent : transform;
        Vector3 playerPos = owner.position;
        Vector3 playerForward = owner.forward;
        playerForward.y = 0f;
        playerForward.Normalize();

        float cosHalfAngle = Mathf.Cos((attackAngleTotal * 0.5f) * Mathf.Deg2Rad);
        float sqrRange = attackRange * attackRange;

        MonoBehaviour[] candidates = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (MonoBehaviour mb in candidates)
        {
            // Skip both the child model and the parent Player object
            if (mb.gameObject == this.gameObject || mb.transform == owner) continue;

            if (mb is IDamageable target)
            {
                Vector3 toTarget = mb.transform.position - playerPos;
                toTarget.y = 0f;

                // Radial Distance Test
                float sqrDist = toTarget.sqrMagnitude;
                if (sqrDist > sqrRange || sqrDist < 0.0001f) continue;

                // Angular Alignment Test (Dot Product Cone)
                float dotProduct = Vector3.Dot(playerForward, toTarget.normalized);
                if (dotProduct >= cosHalfAngle)
                {
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
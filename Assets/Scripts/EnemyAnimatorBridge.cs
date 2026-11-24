using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimatorBridge : MonoBehaviour
{
    private Animator animator;
    private MeleeEnemyKinematics rootKinematics;
    private EnemyHealth rootHealth;

    // Animator Parameter Hashes (faster and avoids string typos)
    private static readonly int IsWalkingHash = Animator.StringToHash("IsMoving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int TakeDamageHash = Animator.StringToHash("TakeDamage");
    private static readonly int DieHash = Animator.StringToHash("Die");

    void Awake()
    {
        animator = GetComponent<Animator>();
        rootKinematics = GetComponentInParent<MeleeEnemyKinematics>();
        rootHealth = GetComponentInParent<EnemyHealth>();
    }

    void OnEnable()
    {
        if (rootKinematics != null)
        {
            rootKinematics.OnMovingChanged += HandleMovingChanged;
            rootKinematics.OnAttackStarted += HandleAttackStarted;
        }

        if (rootHealth != null)
        {
            rootHealth.OnDamaged += HandleDamaged;
            rootHealth.OnDeath += HandleDeath;
        }
    }

    void OnDisable()
    {
        if (rootKinematics != null)
        {
            rootKinematics.OnMovingChanged -= HandleMovingChanged;
            rootKinematics.OnAttackStarted -= HandleAttackStarted;
        }

        if (rootHealth != null)
        {
            rootHealth.OnDamaged -= HandleDamaged;
            rootHealth.OnDeath -= HandleDeath;
        }
    }

    private void HandleMovingChanged(bool isMoving)
    {
        animator.SetBool(IsWalkingHash, isMoving);
    }

    private void HandleAttackStarted()
    {
        animator.SetTrigger(AttackHash);
    }

    private void HandleDamaged(float damageAmount)
    {
        animator.SetTrigger(TakeDamageHash);
    }

    private void HandleDeath()
    {
        animator.SetTrigger(DieHash);
    }

    public void AnimEvent_DealDamage()
    {
        if (rootKinematics != null)
        {
            rootKinematics.ExecuteAttackHit();
        }
    }

    public void AnimEvent_AttackFinished()
    {
        if (rootKinematics != null)
        {
            rootKinematics.OnAttackAnimationFinished();
        }
    }

    public void AnimEvent_SpawnFinished()
    {
        if (rootKinematics != null)
        {
            rootKinematics.OnSpawnAnimationFinished();
        }
    }
}

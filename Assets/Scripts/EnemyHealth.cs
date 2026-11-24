using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float deathDestroyDelay = 2.0f; // Avoid the object being destroyed immediately 

    private float currentHealth;
    private bool isDead = false;
    
    public bool IsDead => isDead;
    public event Action<float> OnHealthChanged;
    public event Action<float> OnDamaged;
    public event Action OnDeath;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        } else {
            OnDamaged.Invoke(amount);
        }
    }

    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();
        
        // Disable CharacterController so dead enemies do not block player movement
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        Destroy(gameObject, deathDestroyDelay); 
    }
}
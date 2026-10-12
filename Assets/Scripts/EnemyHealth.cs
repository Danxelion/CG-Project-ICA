using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth);
        Debug.Log($"Enemy hit! HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Debug.Log("Enemy defeated.");
        Destroy(gameObject);
    }
}
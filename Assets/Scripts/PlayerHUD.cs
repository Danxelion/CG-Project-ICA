using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public Slider hpSlider;
    public PlayerHealth playerHealth;

    void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (playerHealth != null && hpSlider != null)
        {
            hpSlider.minValue = 0f;
            hpSlider.maxValue = playerHealth.GetMaxHealth();
            hpSlider.value = playerHealth.GetCurrentHealth();

            playerHealth.OnHealthChanged += UpdateHealthBar;
        }
    }

    private void UpdateHealthBar(float current, float max)
    {
        if (hpSlider != null)
        {
            hpSlider.value = current;
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }
}
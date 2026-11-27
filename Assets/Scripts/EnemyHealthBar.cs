using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private EnemyHealth targetHealth;
    private Slider healthSlider;
    private Transform mainCameraTransform;

    void Awake()
    {
        healthSlider = GetComponent<Slider>();

        if (targetHealth == null)
        {
            targetHealth = GetComponentInParent<EnemyHealth>();
        }
    }

    void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }

        // Sync slider bounds with EnemyHealth after Awake() has initialized maxHealth
        if (targetHealth != null && healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = targetHealth.GetMaxHealth();
            healthSlider.value = targetHealth.GetCurrentHealth();
        }
    }

    void OnEnable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged += UpdateSlider;
        }
    }

    void OnDisable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged -= UpdateSlider;
        }
    }

    // Billboarding: Ensure the health bar always faces the camera in axonometric view
    void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            transform.forward = mainCameraTransform.forward;
        }
    }

    private void UpdateSlider(float newHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = newHealth;
        }
    }
}
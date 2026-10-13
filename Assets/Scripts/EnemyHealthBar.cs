using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private EnemyHealth targetHealth;
    private Slider healthSlider;

    void Awake()
    {
        healthSlider = GetComponent<Slider>();

        if (targetHealth == null)
        {
            targetHealth = GetComponentInParent<EnemyHealth>();
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
        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }
    }

    private void UpdateSlider(float newHealth)
    {
        healthSlider.value = newHealth;
    }
}
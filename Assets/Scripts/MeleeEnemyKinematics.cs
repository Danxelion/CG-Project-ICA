using UnityEngine;

public class MeleeEnemyKinematics : MonoBehaviour
{
    public Transform playerTarget;
    public float moveSpeed = 3.0f;
    public float attackRange = 1.5f;

    void Update()
    {
        if (playerTarget == null) return;

        // Calculate the displacement vector (d = player position - enemy position)
        float dx = playerTarget.position.x - transform.position.x;
        float dy = 0f; // Lock movement to the flat ground plane
        float dz = playerTarget.position.z - transform.position.z;

        // Calculate the magnitude / distance (||d||)
        float distance = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);

        // Check if the character is outside the range => Chase
        if (distance > attackRange)
        {
            // Get the unit vector to get the direction (v_u = d / ||d||)
            float dirX = dx / distance;
            float dirZ = dz / distance;

            // Time Integration (P_new = P_old + v_u * speed * dt)
            float newX = transform.position.x + (dirX * moveSpeed * Time.deltaTime);
            float newZ = transform.position.z + (dirZ * moveSpeed * Time.deltaTime);

            transform.position = new Vector3(newX, transform.position.y, newZ);

            // Rotate to face player
            float angle = Mathf.Atan2(dx, dz) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        else
        {
            // Enemy is within attack range
            Debug.Log("Enemy is in range to strike!");
        }
    }
}
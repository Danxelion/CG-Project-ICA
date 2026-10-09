using UnityEngine;

public class Wall : MonoBehaviour
{
    public Vector2 minBound;
    public Vector2 maxBound;

    void Awake()
    {
        CalculateBounds();
    }

    public void CalculateBounds()
    {
        // Calculates the 2D bounding box limits based on the object's position and scale
        Vector3 pos = transform.position;
        Vector3 size = transform.localScale;

        minBound = new Vector2(pos.x - size.x * 0.5f, pos.z - size.z * 0.5f);
        maxBound = new Vector2(pos.x + size.x * 0.5f, pos.z + size.z * 0.5f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 center = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Gizmos.DrawWireCube(center, transform.localScale);
    }
}
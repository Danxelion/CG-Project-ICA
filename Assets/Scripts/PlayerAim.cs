using UnityEngine;

public class PlayerAimMath : MonoBehaviour
{
    void Update()
    {
        // Define the Ray (p and t) from the screen cursor
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 p = mouseRay.origin;           
        Vector3 t = mouseRay.direction.normalized; 

        // Define the Plane (y = 0)
        Vector3 planePoint = Vector3.zero;  // A point sitting exactly on the floor
        Vector3 n = Vector3.up; // Surface Normal of the floor                

        // Calculate 'pr' (Vector from ray origin to the plane point)
        Vector3 pr = planePoint - p;

        // Calculate the Denominator (t dot n)
        float denominator = Vector3.Dot(t, n);

        // Prevent division by zero (happens if the camera looks perfectly parallel to the floor)
        if (Mathf.Abs(denominator) > 0.0001f)
        {
            // Calculate the distance along the ray 
            float numerator = Vector3.Dot(pr, n);
            float distance = numerator / denominator;
            
            // Find the final 3D point 
            Vector3 q = p + (t * distance);

            // Calculate the look direction vector and rotate the player
            Vector3 lookDirection = q - transform.position;
            lookDirection.y = 0; // Keep the character perfectly upright

            if (lookDirection.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }
}
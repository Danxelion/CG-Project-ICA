using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CustomCameraMatrix : MonoBehaviour
{
    public bool useBuiltInCamera = true;
    [Header("Camera Settings")]
    public Transform target;
    public float distance = 15f;
    public float orthoSize = 5f;
    public float nearClip = 0.3f;
    public float farClip = 100f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (useBuiltInCamera)
        {
            // Use Unity's default rendering
            cam.ResetWorldToCameraMatrix();
            cam.ResetProjectionMatrix();
            
            // Standard Unity tracking
            transform.rotation = Quaternion.Euler(30f, 45f, 0f);
            if (target != null)
                transform.position = target.position - transform.forward * distance;
        }
        else
        {
            // Override the graphics pipeline with math matrices
            ApplyCustomMatrices();
        }
    }

    private void ApplyCustomMatrices()
    {
        if (target == null) return;
        
        float pitch = 30f * Mathf.Deg2Rad;
        float yaw = 45f * Mathf.Deg2Rad;

        // Look down (-sin(pitch)) and angle horizontally using sine/cosine of yaw
        Vector3 F = new Vector3(
            Mathf.Sin(yaw) * Mathf.Cos(pitch),
            -Mathf.Sin(pitch),
            Mathf.Cos(yaw) * Mathf.Cos(pitch)
        ).normalized;

        // Calculate Right R and Up U vectors using cross products
        Vector3 worldUp = Vector3.up;
        Vector3 R = Vector3.Cross(worldUp, F).normalized;
        Vector3 U = Vector3.Cross(F, R).normalized;

        // Push the camera backward 
        Vector3 P = target.position - (F * distance);
        transform.position = P; 

        // This matrix translates and rotates the entire world 
        Matrix4x4 viewMatrix = new Matrix4x4();
        viewMatrix.SetRow(0, new Vector4(R.x, R.y, R.z, -Vector3.Dot(R, P)));
        viewMatrix.SetRow(1, new Vector4(U.x, U.y, U.z, -Vector3.Dot(U, P)));
        viewMatrix.SetRow(2, new Vector4(-F.x, -F.y, -F.z, Vector3.Dot(F, P))); 
        viewMatrix.SetRow(3, new Vector4(0, 0, 0, 1));

        cam.worldToCameraMatrix = viewMatrix;


        // Orthographic projection
        float aspect = (float)Screen.width / Screen.height;
        
        // Define the boundaries of the viewing box
        float right = orthoSize * aspect;
        float left = -right;
        float top = orthoSize;
        float bottom = -top;

        // Projection Matrix
        Matrix4x4 projMatrix = new Matrix4x4();
        projMatrix.SetRow(0, new Vector4(2f / (right - left), 0, 0, -(right + left) / (right - left)));
        projMatrix.SetRow(1, new Vector4(0, 2f / (top - bottom), 0, -(top + bottom) / (top - bottom)));
        projMatrix.SetRow(2, new Vector4(0, 0, -2f / (farClip - nearClip), -(farClip + nearClip) / (farClip - nearClip)));
        projMatrix.SetRow(3, new Vector4(0, 0, 0, 1));

        cam.projectionMatrix = projMatrix;
    }
}
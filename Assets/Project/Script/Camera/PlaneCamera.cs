using UnityEngine;

public class PlaneCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // Pesawat
    
    [Header("Camera Position")]
    [SerializeField] private Vector3 offsetPosition = new Vector3(0f, 3f, -12f); // Posisi relatif ke pesawat
    [SerializeField] private bool useLocalOffset = true; // Offset relatif ke rotasi pesawat
    
    [Header("Look Settings")]
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 0f, 5f); // Look ahead dari pesawat
    [SerializeField] private bool lookAhead = true; // Look di depan pesawat
    
    [Header("Smoothing")]
    [SerializeField] private float positionSmoothTime = 0.2f;
    [SerializeField] private float rotationSmoothTime = 0.15f;
    
    [Header("FOV Settings")]
    [SerializeField] private bool dynamicFOV = true;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float boostFOV = 75f;
    [SerializeField] private float fovSmoothTime = 0.3f;
    
    [Header("Camera Shake")]
    [SerializeField] private bool enableShake = true;
    [SerializeField] private float shakeAmount = 0.1f;
    [SerializeField] private float shakeFrequency = 1f;
    
    // Components
    private Camera cam;
    private ArcadePlaneController planeController;
    
    // Smooth damping
    private Vector3 positionVelocity;
    private Vector3 rotationVelocity;
    private float fovVelocity;
    
    // Camera shake
    private float shakeTime;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        
        // Auto-find plane if not assigned
        if (target == null)
        {
            planeController = FindFirstObjectByType<ArcadePlaneController>();
            if (planeController != null)
            {
                target = planeController.transform;
            }
            else
            {
                Debug.LogError("❌ PlaneCamera: No target assigned and no ArcadePlaneController found!");
                enabled = false;
                return;
            }
        }
        else
        {
            planeController = target.GetComponent<ArcadePlaneController>();
        }
        
        // Set initial FOV
        if (cam != null && dynamicFOV)
        {
            cam.fieldOfView = normalFOV;
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        UpdateCameraPosition();
        UpdateCameraRotation();
        UpdateCameraFOV();
        UpdateCameraShake();
    }
    
    void UpdateCameraPosition()
    {
        Vector3 targetPosition;
        
        if (useLocalOffset)
        {
            // Offset relatif ke rotasi pesawat (ikut miring saat roll)
            targetPosition = target.position + target.TransformDirection(offsetPosition);
        }
        else
        {
            // Offset fixed (tidak ikut rotasi pesawat)
            targetPosition = target.position + offsetPosition;
        }
        
        // Smooth follow
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref positionVelocity,
            positionSmoothTime
        );
    }
    
    void UpdateCameraRotation()
    {
        Vector3 lookAtPoint;
        
        if (lookAhead)
        {
            // Look di depan pesawat (lebih cinematic)
            lookAtPoint = target.position + target.TransformDirection(lookAtOffset);
        }
        else
        {
            // Look tepat ke pesawat
            lookAtPoint = target.position + target.TransformDirection(lookAtOffset);
        }
        
        // Calculate target rotation
        Vector3 direction = lookAtPoint - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        
        // Smooth rotation using SmoothDamp
        Vector3 currentEuler = transform.eulerAngles;
        Vector3 targetEuler = targetRotation.eulerAngles;
        
        // Normalize angles to -180 to 180
        for (int i = 0; i < 3; i++)
        {
            if (currentEuler[i] > 180f) currentEuler[i] -= 360f;
            if (targetEuler[i] > 180f) targetEuler[i] -= 360f;
        }
        
        Vector3 smoothEuler = new Vector3(
            Mathf.SmoothDampAngle(currentEuler.x, targetEuler.x, ref rotationVelocity.x, rotationSmoothTime),
            Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref rotationVelocity.y, rotationSmoothTime),
            Mathf.SmoothDampAngle(currentEuler.z, targetEuler.z, ref rotationVelocity.z, rotationSmoothTime)
        );
        
        transform.eulerAngles = smoothEuler;
    }
    
    void UpdateCameraFOV()
    {
        if (cam == null || !dynamicFOV || planeController == null) return;
        
        // Change FOV based on boost
        float targetFOV = planeController.IsBoosting() ? boostFOV : normalFOV;
        
        // Smooth FOV transition
        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFOV, ref fovVelocity, fovSmoothTime);
    }
    
    void UpdateCameraShake()
    {
        if (!enableShake || planeController == null) return;
        
        // Only shake when moving fast or boosting
        float speed = planeController.GetCurrentSpeed();
        bool shouldShake = speed > 50f || planeController.IsBoosting();
        
        if (shouldShake)
        {
            shakeTime += Time.deltaTime;
            
            // Perlin noise for smooth shake
            float shakeX = (Mathf.PerlinNoise(shakeTime * shakeFrequency, 0f) - 0.5f) * shakeAmount;
            float shakeY = (Mathf.PerlinNoise(0f, shakeTime * shakeFrequency) - 0.5f) * shakeAmount;
            
            // Apply shake multiplier based on speed
            float shakeMultiplier = Mathf.Clamp01((speed - 50f) / 50f);
            if (planeController.IsBoosting())
            {
                shakeMultiplier = 1.5f;
            }
            
            transform.localPosition += new Vector3(shakeX, shakeY, 0f) * shakeMultiplier;
        }
    }
    
    // Public methods for external control
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        planeController = target.GetComponent<ArcadePlaneController>();
    }
    
    public void SetOffset(Vector3 position, Vector3 lookAt)
    {
        offsetPosition = position;
        lookAtOffset = lookAt;
    }
    
    public void SetFOV(float fov)
    {
        if (cam != null)
        {
            normalFOV = fov;
            cam.fieldOfView = fov;
        }
    }
    
    public void EnableShake(bool enable)
    {
        enableShake = enable;
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (target == null) return;
        
        // Draw camera position
        Gizmos.color = Color.cyan;
        Vector3 targetPos = useLocalOffset 
            ? target.position + target.TransformDirection(offsetPosition)
            : target.position + offsetPosition;
        Gizmos.DrawWireSphere(targetPos, 0.5f);
        
        // Draw look at point
        Gizmos.color = Color.yellow;
        Vector3 lookAtPoint = target.position + target.TransformDirection(lookAtOffset);
        Gizmos.DrawWireSphere(lookAtPoint, 0.3f);
        
        // Draw line from camera to look point
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, lookAtPoint);
        
        // Draw line from camera to plane
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.position);
    }
}

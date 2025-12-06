using UnityEngine;

/// <summary>
/// Visual indicator that appears under locked target
/// Billboard sprite or quad that follows target position
/// </summary>
public class TargetIndicator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float yOffset = 2f; // Di atas kepala target
    [SerializeField] private bool billboard = true;
    [SerializeField] private float rotationSpeed = 45f;
    [SerializeField] private bool autoRotate = true;
    
    [Header("Scale Animation")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.1f;
    
    private Transform targetTransform;
    private Vector3 originalScale;
    private Camera mainCamera;
    private bool isActive = false;
    
    void Awake()
    {
        mainCamera = Camera.main;
        originalScale = transform.localScale;
    }
    
    void Update()
    {
        if (!isActive || targetTransform == null) return;
        
        // Follow target position
        UpdatePosition();
        
        // Billboard effect - face camera horizontally (not tilted)
        if (billboard && mainCamera != null)
        {
            // Only rotate on Y axis to keep indicator horizontal
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;
            directionToCamera.y = 0; // Ignore vertical difference
            
            if (directionToCamera != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
                transform.rotation = targetRotation;
            }
        }
        
        // Auto-rotate around Y axis
        if (autoRotate)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }
        
        // Pulse animation
        if (enablePulse)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = originalScale * pulse;
        }
    }
    
    /// <summary>
    /// Update indicator position to follow target
    /// </summary>
    void UpdatePosition()
    {
        if (targetTransform != null)
        {
            transform.position = targetTransform.position + Vector3.up * yOffset;
        }
    }
    
    /// <summary>
    /// Set target to follow
    /// </summary>
    public void SetTarget(Transform target)
    {
        targetTransform = target;
        isActive = target != null;
        
        if (isActive)
        {
            UpdatePosition();
            gameObject.SetActive(true);
            
            Debug.Log($"<color=lime>[Indicator] SetTarget: {target.name}</color>");
            Debug.Log($"<color=lime>  → Indicator GameObject: {gameObject.name}</color>");
            Debug.Log($"<color=lime>  → Position: {transform.position}</color>");
            Debug.Log($"<color=lime>  → Active: {gameObject.activeSelf}</color>");
            Debug.Log($"<color=lime>  → Y Offset: {yOffset}</color>");
        }
    }
    
    /// <summary>
    /// Clear target and hide indicator
    /// </summary>
    public void ClearTarget()
    {
        Debug.Log($"<color=orange>[Indicator] ClearTarget: {(targetTransform != null ? targetTransform.name : "null")}</color>");
        targetTransform = null;
        isActive = false;
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Check if indicator is currently active
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }
    
    /// <summary>
    /// Get current target
    /// </summary>
    public Transform GetTarget()
    {
        return targetTransform;
    }
    
    /// <summary>
    /// Set Y offset from ground
    /// </summary>
    public void SetYOffset(float offset)
    {
        yOffset = offset;
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World space health bar untuk enemy yang selalu menghadap kamera
/// </summary>
public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Text healthText;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);
    [SerializeField] private float hideDelay = 2f;
    [SerializeField] private bool alwaysShow = false;
    [SerializeField] private bool showHealthText = true;
    
    [Header("Colors")]
    [SerializeField] private Color healthyColor = new Color(0f, 1f, 0f, 1f); // Green
    [SerializeField] private Color damagedColor = new Color(1f, 0.5f, 0f, 1f); // Orange
    [SerializeField] private Color criticalColor = new Color(1f, 0f, 0f, 1f); // Red
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    
    [Header("Animation")]
    [SerializeField] private bool enableDamageFlash = true;
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private bool enableSmoothFill = true;
    [SerializeField] private float smoothSpeed = 5f;
    
    private Camera mainCamera;
    private Transform targetTransform;
    private float currentDisplayHealth;
    private float targetHealth;
    private float hideTimer;
    private bool isVisible;
    
    void Awake()
    {
        mainCamera = Camera.main;
        
        // Setup UI if not assigned
        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>();
            
        if (fillImage == null && healthSlider != null && healthSlider.fillRect != null)
        {
            fillImage = healthSlider.fillRect.GetComponent<Image>();
        }
            
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
            
        if (backgroundImage != null)
            backgroundImage.color = backgroundColor;
            
        // Validate setup
        if (healthSlider == null)
        {
            Debug.LogWarning("EnemyHealthBarUI: No Slider found! Disabling component.", gameObject);
            enabled = false;
            return;
        }
        
        if (healthSlider.fillRect == null)
        {
            Debug.LogWarning("EnemyHealthBarUI: Slider.fillRect not assigned! Please configure the Slider component.", gameObject);
        }
    }
    
    void Start()
    {
        targetTransform = transform.parent;
        
        if (!alwaysShow)
        {
            Hide();
        }
    }
    
    void LateUpdate()
    {
        // Always face camera
        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }
        
        // Position above enemy
        if (targetTransform != null)
        {
            transform.position = targetTransform.position + offset;
        }
        
        // Smooth fill animation
        if (enableSmoothFill && currentDisplayHealth != targetHealth)
        {
            currentDisplayHealth = Mathf.Lerp(currentDisplayHealth, targetHealth, smoothSpeed * Time.deltaTime);
            healthSlider.value = currentDisplayHealth;
            UpdateHealthColor(currentDisplayHealth);
        }
        
        // Auto hide timer
        if (!alwaysShow && isVisible)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
            {
                Hide();
            }
        }
    }
    
    public void SetHealth(float current, float max)
    {
        if (healthSlider == null) return;
        
        healthSlider.maxValue = max;
        targetHealth = current;
        
        if (!enableSmoothFill)
        {
            currentDisplayHealth = current;
            healthSlider.value = current;
            UpdateHealthColor(current / max);
        }
        
        // Update text
        if (healthText != null && showHealthText)
        {
            healthText.text = $"{Mathf.Ceil(current)}/{Mathf.Ceil(max)}";
        }
        
        // Show when damaged
        if (!alwaysShow)
        {
            Show();
            hideTimer = hideDelay;
        }
        
        // Damage flash effect
        if (enableDamageFlash && current < currentDisplayHealth)
        {
            StopAllCoroutines();
            StartCoroutine(DamageFlashEffect());
        }
    }
    
    void UpdateHealthColor(float healthPercent)
    {
        if (fillImage == null) return;
        
        Color targetColor;
        
        if (healthPercent > 0.6f)
        {
            // Healthy - Green
            targetColor = healthyColor;
        }
        else if (healthPercent > 0.3f)
        {
            // Damaged - Orange
            targetColor = damagedColor;
        }
        else
        {
            // Critical - Red
            targetColor = criticalColor;
        }
        
        fillImage.color = targetColor;
    }
    
    System.Collections.IEnumerator DamageFlashEffect()
    {
        if (fillImage == null) yield break;
        
        Color originalColor = fillImage.color;
        fillImage.color = Color.white;
        
        yield return new WaitForSeconds(damageFlashDuration);
        
        fillImage.color = originalColor;
    }
    
    public void Show()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            gameObject.SetActive(true);
        }
        isVisible = true;
    }
    
    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        else
        {
            gameObject.SetActive(false);
        }
        isVisible = false;
    }
    
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
}

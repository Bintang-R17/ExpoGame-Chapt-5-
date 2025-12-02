using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Health bar untuk Player menggunakan Slider
/// Menampilkan HP bar di layar (Screen Space)
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI healthText;
    
    [Header("Player Reference")]
    [SerializeField] private PlayerHealth playerHealth;
    
    [Header("Colors")]
    [SerializeField] private Color healthyColor = new Color(0f, 1f, 0f, 1f); // Green
    [SerializeField] private Color damagedColor = new Color(1f, 0.92f, 0.016f, 1f); // Yellow
    [SerializeField] private Color criticalColor = new Color(1f, 0f, 0f, 1f); // Red
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    
    [Header("Animation")]
    [SerializeField] private bool enableSmoothFill = true;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool enableDamageFlash = true;
    [SerializeField] private float damageFlashDuration = 0.2f;
    
    [Header("Display Settings")]
    [SerializeField] private bool showHealthText = true;
    [SerializeField] private bool showMaxHealth = true;
    [SerializeField] private bool showPercentage = false;
    
    private float currentDisplayHealth;
    private float targetHealth;
    private float lastHealth;
    
    void Start()
    {
        // Find player health if not assigned
        if (playerHealth == null)
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            
            if (playerHealth == null)
            {
                Debug.LogError("❌ PlayerHealth not found! Please assign it in the inspector.");
                return;
            }
        }
        
        // Setup UI references
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }
        
        if (fillImage == null && healthSlider != null && healthSlider.fillRect != null)
        {
            fillImage = healthSlider.fillRect.GetComponent<Image>();
        }
        
        if (backgroundImage == null)
        {
            // Try to find background image
            Transform bgTransform = transform.Find("Background");
            if (bgTransform != null)
            {
                backgroundImage = bgTransform.GetComponent<Image>();
            }
        }
        
        // Set background color
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
        
        // Subscribe to health change event
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(OnPlayerHealthChanged);
        }
        
        // Initialize health bar
        InitializeHealthBar();
        
        Debug.Log("✅ PlayerHealthBar initialized successfully!");
    }
    
    void InitializeHealthBar()
    {
        if (playerHealth == null || healthSlider == null) return;
        
        healthSlider.maxValue = playerHealth.MaxHealth;
        healthSlider.minValue = 0;
        
        currentDisplayHealth = playerHealth.CurrentHealth;
        targetHealth = playerHealth.CurrentHealth;
        lastHealth = playerHealth.CurrentHealth;
        
        healthSlider.value = currentDisplayHealth;
        
        UpdateHealthDisplay();
    }
    
    void Update()
    {
        if (playerHealth == null) return;
        
        // Smooth fill animation
        if (enableSmoothFill && Mathf.Abs(currentDisplayHealth - targetHealth) > 0.01f)
        {
            currentDisplayHealth = Mathf.Lerp(currentDisplayHealth, targetHealth, smoothSpeed * Time.deltaTime);
            healthSlider.value = currentDisplayHealth;
            UpdateHealthDisplay();
        }
    }
    
    void OnPlayerHealthChanged(float healthPercentage)
    {
        if (playerHealth == null || healthSlider == null) return;
        
        float newHealth = playerHealth.CurrentHealth;
        targetHealth = newHealth;
        
        if (!enableSmoothFill)
        {
            currentDisplayHealth = newHealth;
            healthSlider.value = newHealth;
        }
        
        UpdateHealthDisplay();
        
        // Damage flash effect
        if (enableDamageFlash && newHealth < lastHealth)
        {
            StopAllCoroutines();
            StartCoroutine(DamageFlashEffect());
        }
        
        lastHealth = newHealth;
    }
    
    void UpdateHealthDisplay()
    {
        if (playerHealth == null) return;
        
        // Update health color based on percentage
        float healthPercent = currentDisplayHealth / playerHealth.MaxHealth;
        UpdateHealthColor(healthPercent);
        
        // Update health text
        if (healthText != null && showHealthText)
        {
            if (showPercentage)
            {
                healthText.text = $"{Mathf.RoundToInt(healthPercent * 100)}%";
            }
            else if (showMaxHealth)
            {
                healthText.text = $"{Mathf.CeilToInt(currentDisplayHealth)}/{Mathf.CeilToInt(playerHealth.MaxHealth)}";
            }
            else
            {
                healthText.text = $"{Mathf.CeilToInt(currentDisplayHealth)}";
            }
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
            // Damaged - Yellow/Orange
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
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(OnPlayerHealthChanged);
        }
    }
    
    // Public methods untuk customization
    public void SetColors(Color healthy, Color damaged, Color critical)
    {
        healthyColor = healthy;
        damagedColor = damaged;
        criticalColor = critical;
        UpdateHealthDisplay();
    }
    
    public void SetSmoothFill(bool enabled, float speed = 5f)
    {
        enableSmoothFill = enabled;
        smoothSpeed = speed;
    }
    
    public void SetShowPercentage(bool show)
    {
        showPercentage = show;
        showMaxHealth = !show;
        UpdateHealthDisplay();
    }
}

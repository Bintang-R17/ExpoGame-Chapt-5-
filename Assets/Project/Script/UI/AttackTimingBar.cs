using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Bar untuk menampilkan timing window attack ke enemy
/// Bar bergerak dari atas ke bawah dengan zona timing yang optimal
/// </summary>
public class AttackTimingBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider timingSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private RectTransform sweetSpotIndicator;
    [SerializeField] private TextMeshProUGUI statusText;
    
    [Header("Bar Settings")]
    [SerializeField] private float barSpeed = 1f;
    [SerializeField] private bool autoReverse = true;
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 1f;
    
    [Header("Sweet Spot Settings")]
    [SerializeField] private float sweetSpotStart = 0.4f;
    [SerializeField] private float sweetSpotEnd = 0.6f;
    [SerializeField] private Color sweetSpotColor = Color.yellow;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.3f, 0.8f, 1f, 1f); // Cyan
    [SerializeField] private Color perfectColor = new Color(0f, 1f, 0f, 1f); // Green
    [SerializeField] private Color missColor = new Color(1f, 0f, 0f, 1f); // Red
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    
    [Header("Animation")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.1f;
    
    [Header("Display Settings")]
    [SerializeField] private bool showStatusText = true;
    [SerializeField] private float feedbackDuration = 0.5f;
    
    // State
    private float currentValue;
    private bool isMovingUp = false;
    private bool isActive = false;
    private string currentStatus = "";
    private float statusDisplayTime = 0f;
    private Vector3 originalScale;
    
    void Start()
    {
        Initialize();
    }
    
    void Update()
    {
        if (isActive)
        {
            UpdateBarMovement();
            UpdateVisuals();
        }
        
        UpdateStatusDisplay();
    }
    
    /// <summary>
    /// Initialize bar
    /// </summary>
    void Initialize()
    {
        if (timingSlider != null)
        {
            timingSlider.minValue = minValue;
            timingSlider.maxValue = maxValue;
            currentValue = minValue;
            timingSlider.value = currentValue;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
        
        if (fillImage != null)
        {
            fillImage.color = normalColor;
        }
        
        if (sweetSpotIndicator != null)
        {
            originalScale = sweetSpotIndicator.localScale;
            UpdateSweetSpotPosition();
        }
        
        // Hide status text initially
        if (statusText != null && showStatusText)
        {
            statusText.text = "";
        }
    }
    
    /// <summary>
    /// Update sweet spot indicator position
    /// </summary>
    void UpdateSweetSpotPosition()
    {
        if (sweetSpotIndicator == null || timingSlider == null) return;
        
        RectTransform sliderRect = timingSlider.GetComponent<RectTransform>();
        if (sliderRect == null) return;
        
        // Calculate sweet spot center position (normalized)
        float sweetSpotCenter = (sweetSpotStart + sweetSpotEnd) / 2f;
        
        // Set position along slider (vertical for top-to-bottom bar)
        float height = sliderRect.rect.height;
        float yPos = height * sweetSpotCenter - (height / 2f);
        
        sweetSpotIndicator.anchoredPosition = new Vector2(0f, yPos);
        
        // Set height based on sweet spot range
        float sweetSpotHeight = height * (sweetSpotEnd - sweetSpotStart);
        sweetSpotIndicator.sizeDelta = new Vector2(sweetSpotIndicator.sizeDelta.x, sweetSpotHeight);
        
        // Set color
        Image sweetSpotImage = sweetSpotIndicator.GetComponent<Image>();
        if (sweetSpotImage != null)
        {
            sweetSpotImage.color = sweetSpotColor;
        }
    }
    
    /// <summary>
    /// Update bar movement
    /// </summary>
    void UpdateBarMovement()
    {
        float direction = isMovingUp ? 1f : -1f;
        currentValue += direction * barSpeed * Time.deltaTime;
        
        // Clamp value
        currentValue = Mathf.Clamp(currentValue, minValue, maxValue);
        
        // Auto reverse at ends
        if (autoReverse)
        {
            if (currentValue >= maxValue)
            {
                isMovingUp = false;
            }
            else if (currentValue <= minValue)
            {
                isMovingUp = true;
            }
        }
        
        // Update slider
        if (timingSlider != null)
        {
            timingSlider.value = currentValue;
        }
    }
    
    /// <summary>
    /// Update visual effects
    /// </summary>
    void UpdateVisuals()
    {
        // Update fill color based on position
        if (fillImage != null)
        {
            if (IsInSweetSpot())
            {
                fillImage.color = perfectColor;
            }
            else
            {
                fillImage.color = normalColor;
            }
        }
        
        // Pulse animation on sweet spot
        if (enablePulse && sweetSpotIndicator != null && IsInSweetSpot())
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            sweetSpotIndicator.localScale = originalScale * pulse;
        }
        else if (sweetSpotIndicator != null)
        {
            sweetSpotIndicator.localScale = originalScale;
        }
    }
    
    /// <summary>
    /// Update status text display
    /// </summary>
    void UpdateStatusDisplay()
    {
        if (!showStatusText || statusText == null) return;
        
        if (statusDisplayTime > 0f)
        {
            statusDisplayTime -= Time.deltaTime;
            
            if (statusDisplayTime <= 0f)
            {
                statusText.text = "";
            }
        }
    }
    
    /// <summary>
    /// Check if current value is in sweet spot
    /// </summary>
    public bool IsInSweetSpot()
    {
        return currentValue >= sweetSpotStart && currentValue <= sweetSpotEnd;
    }
    
    /// <summary>
    /// Get timing accuracy (0-1, where 1 is perfect center)
    /// </summary>
    public float GetAccuracy()
    {
        if (!IsInSweetSpot())
        {
            return 0f;
        }
        
        float sweetSpotCenter = (sweetSpotStart + sweetSpotEnd) / 2f;
        float sweetSpotRadius = (sweetSpotEnd - sweetSpotStart) / 2f;
        float distance = Mathf.Abs(currentValue - sweetSpotCenter);
        
        return 1f - (distance / sweetSpotRadius);
    }
    
    /// <summary>
    /// Start/Activate the timing bar
    /// </summary>
    public void Activate()
    {
        isActive = true;
        currentValue = minValue;
        isMovingUp = true;
    }
    
    /// <summary>
    /// Stop/Deactivate the timing bar
    /// </summary>
    public void Deactivate()
    {
        isActive = false;
    }
    
    /// <summary>
    /// Check timing and show feedback
    /// </summary>
    public TimingResult CheckTiming()
    {
        TimingResult result = new TimingResult();
        
        if (IsInSweetSpot())
        {
            float accuracy = GetAccuracy();
            
            if (accuracy > 0.8f)
            {
                result.quality = TimingQuality.Perfect;
                result.damageMultiplier = 2f;
                ShowFeedback("PERFECT!", perfectColor);
            }
            else if (accuracy > 0.5f)
            {
                result.quality = TimingQuality.Great;
                result.damageMultiplier = 1.5f;
                ShowFeedback("GREAT!", Color.yellow);
            }
            else
            {
                result.quality = TimingQuality.Good;
                result.damageMultiplier = 1.2f;
                ShowFeedback("GOOD", Color.green);
            }
        }
        else
        {
            result.quality = TimingQuality.Miss;
            result.damageMultiplier = 0.5f;
            ShowFeedback("MISS", missColor);
        }
        
        result.accuracy = GetAccuracy();
        return result;
    }
    
    /// <summary>
    /// Show feedback text
    /// </summary>
    void ShowFeedback(string message, Color color)
    {
        if (!showStatusText || statusText == null) return;
        
        statusText.text = message;
        statusText.color = color;
        statusDisplayTime = feedbackDuration;
    }
    
    /// <summary>
    /// Get current bar value
    /// </summary>
    public float GetCurrentValue()
    {
        return currentValue;
    }
    
    /// <summary>
    /// Set bar speed
    /// </summary>
    public void SetSpeed(float speed)
    {
        barSpeed = speed;
    }
    
    /// <summary>
    /// Toggle bar visibility
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}

/// <summary>
/// Timing quality levels
/// </summary>
public enum TimingQuality
{
    Miss,
    Good,
    Great,
    Perfect
}

/// <summary>
/// Result from timing check
/// </summary>
public class TimingResult
{
    public TimingQuality quality;
    public float damageMultiplier;
    public float accuracy;
}

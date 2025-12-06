using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Contextual timing window UI for shield/vulnerable enemy phases
/// Shows timing bar with perfect/good/miss zones
/// </summary>
public class TimingBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider timingSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private RectTransform perfectZoneIndicator;
    [SerializeField] private TextMeshProUGUI feedbackText;
    
    [Header("Height Thresholds")]
    [SerializeField] [Range(0f, 1f)] private float perfectThreshold = 0.8f; // ≥80% = Perfect
    [SerializeField] [Range(0f, 1f)] private float goodThreshold = 0.4f; // ≥40% = Good
    [SerializeField] private int goodHitsRequired = 2; // Hits needed for Good timing
    
    [Header("Visual Colors")]
    [SerializeField] private Color missColor = new Color(1f, 0f, 0f, 0.8f); // Red <40%
    [SerializeField] private Color goodColor = new Color(1f, 1f, 0f, 0.8f); // Yellow 40-79%
    [SerializeField] private Color perfectColor = new Color(0f, 1f, 0f, 0.8f); // Green ≥80%
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    
    [Header("Animation")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseAmount = 0.15f;
    
    [Header("Feedback")]
    [SerializeField] private float feedbackDuration = 0.8f;
    
    // State
    private Transform currentTarget;
    private float windowDuration;
    private float perfectZoneStart; // Deprecated (kept for compatibility)
    private float perfectZoneEnd; // Deprecated (kept for compatibility)
    private float startTime;
    private bool isActive = false;
    private Vector3 originalScale;
    private float feedbackTimer = 0f;
    private string currentFeedback = "";
    private int currentGoodHits = 0; // Track consecutive good hits
    
    // Singleton for easy access
    private static TimingBarUI instance;
    public static TimingBarUI Instance => instance;
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        if (timingSlider != null)
        {
            originalScale = timingSlider.transform.localScale;
        }
        else
        {
            Debug.LogWarning("[TimingBarUI] Timing Slider reference is missing! Please assign in Inspector or UI will not display.");
        }
        
        Hide();
    }
    
    void Update()
    {
        if (isActive)
        {
            UpdateTimingBar();
            UpdateVisuals();
        }
        
        UpdateFeedback();
    }
    
    /// <summary>
    /// Show timing bar for specific target with configurable zones
    /// </summary>
    public void Show(Transform target, float duration, float perfectStart, float perfectEnd)
    {
        if (target == null)
        {
            Debug.LogWarning("[TimingBarUI] Cannot show - target is null");
            return;
        }
        
        currentTarget = target;
        windowDuration = duration;
        perfectZoneStart = perfectStart;
        perfectZoneEnd = perfectEnd;
        startTime = Time.time;
        isActive = true;
        
        // Setup slider
        if (timingSlider != null)
        {
            timingSlider.minValue = 0f;
            timingSlider.maxValue = 1f;
            timingSlider.value = 0f;
        }
        
        // Setup perfect zone indicator (now shows perfect threshold line)
        if (perfectZoneIndicator != null)
        {
            // Position indicator at perfect threshold height (80%)
            perfectZoneIndicator.anchorMin = new Vector2(0f, perfectThreshold);
            perfectZoneIndicator.anchorMax = new Vector2(1f, perfectThreshold);
            perfectZoneIndicator.offsetMin = new Vector2(0f, -2f); // Thin line
            perfectZoneIndicator.offsetMax = new Vector2(0f, 2f);
        }
        
        gameObject.SetActive(true);
        
        Debug.Log($"<color=cyan>[TimingBarUI] SHOW for target: {target.name}</color>");
        Debug.Log($"<color=cyan>  → Duration: {duration}s</color>");
        Debug.Log($"<color=cyan>  → Perfect: ≥{perfectThreshold * 100f:F0}% | Good: {goodThreshold * 100f:F0}-{perfectThreshold * 100f:F0}% ({goodHitsRequired}x) | Miss: <{goodThreshold * 100f:F0}%</color>");
    }
    
    /// <summary>
    /// Show persistent timing bar (stays visible while locked)
    /// </summary>
    public void ShowPersistent(Transform target, float duration, float perfectStart, float perfectEnd)
    {
        Show(target, duration, perfectStart, perfectEnd);
    }
    
    /// <summary>
    /// Hide timing bar
    /// </summary>
    public void Hide()
    {
        isActive = false;
        currentTarget = null;
        gameObject.SetActive(false);
        
        Debug.Log($"<color=yellow>[TimingBarUI] HIDE</color>");
    }
    
    /// <summary>
    /// Update timing bar animation
    /// </summary>
    void UpdateTimingBar()
    {
        if (timingSlider == null) return;
        
        float elapsed = Time.time - startTime;
        float progress = Mathf.Clamp01(elapsed / windowDuration);
        
        timingSlider.value = progress;
        
        // Loop animation when window expires (don't auto-hide)
        if (progress >= 1f)
        {
            startTime = Time.time; // Restart animation
        }
    }
    
    /// <summary>
    /// Update visual feedback colors (height-based)
    /// </summary>
    void UpdateVisuals()
    {
        if (fillImage == null) return;
        
        float currentHeight = timingSlider.value;
        
        // Color based on height threshold
        if (currentHeight >= perfectThreshold) // ≥80%
        {
            fillImage.color = perfectColor;
        }
        else if (currentHeight >= goodThreshold) // 40-79%
        {
            fillImage.color = goodColor;
        }
        else // <40%
        {
            fillImage.color = missColor;
        }
        
        // Pulse animation
        if (enablePulse && timingSlider != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            timingSlider.transform.localScale = originalScale * pulse;
        }
    }
    
    /// <summary>
    /// Update feedback text display
    /// </summary>
    void UpdateFeedback()
    {
        if (feedbackText == null) return;
        
        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.deltaTime;
            feedbackText.text = currentFeedback;
            feedbackText.gameObject.SetActive(true);
        }
        else
        {
            feedbackText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Check current timing result (height-based)
    /// </summary>
    public ShieldTimingResult CheckTiming()
    {
        if (!isActive || timingSlider == null)
        {
            return ShieldTimingResult.Miss;
        }
        
        float currentHeight = timingSlider.value;
        
        // Height-based timing check
        if (currentHeight >= perfectThreshold) // ≥80%
        {
            ShowFeedback("⚡ PERFECT!", perfectColor);
            currentGoodHits = 0; // Reset good hits counter
            Debug.Log($"<color=lime>[TimingBarUI] PERFECT HIT at {currentHeight * 100f:F0}% height!</color>");
            return ShieldTimingResult.Perfect;
        }
        else if (currentHeight >= goodThreshold) // 40-79%
        {
            currentGoodHits++;
            ShowFeedback($"✓ GOOD ({currentGoodHits}/{goodHitsRequired})", goodColor);
            Debug.Log($"<color=yellow>[TimingBarUI] GOOD HIT at {currentHeight * 100f:F0}% height! ({currentGoodHits}/{goodHitsRequired})</color>");
            
            // Check if enough good hits
            if (currentGoodHits >= goodHitsRequired)
            {
                currentGoodHits = 0; // Reset counter
                return ShieldTimingResult.Good;
            }
            else
            {
                return ShieldTimingResult.Miss; // Not enough hits yet
            }
        }
        else // <40%
        {
            ShowFeedback("✗ BLOCKED!", missColor);
            currentGoodHits = 0; // Reset on miss
            Debug.Log($"<color=red>[TimingBarUI] BLOCKED at {currentHeight * 100f:F0}% height!</color>");
            return ShieldTimingResult.Miss;
        }
    }
    
    /// <summary>
    /// Show feedback message
    /// </summary>
    void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null) return;
        
        currentFeedback = message;
        feedbackTimer = feedbackDuration;
        feedbackText.color = color;
        
        Debug.Log($"<color=lime>[TimingBarUI] Feedback: {message}</color>");
    }
    
    /// <summary>
    /// Check if timing window is currently active
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }
    
    /// <summary>
    /// Get current target
    /// </summary>
    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }
    
    /// <summary>
    /// Get normalized progress (0-1)
    /// </summary>
    public float GetProgress()
    {
        if (!isActive || timingSlider == null) return 0f;
        return timingSlider.value;
    }
    
    /// <summary>
    /// Get current good hits count
    /// </summary>
    public int GetGoodHitsCount()
    {
        return currentGoodHits;
    }
    
    /// <summary>
    /// Reset good hits counter
    /// </summary>
    public void ResetGoodHits()
    {
        currentGoodHits = 0;
        Debug.Log("<color=orange>[TimingBarUI] Good hits counter reset</color>");
    }
}

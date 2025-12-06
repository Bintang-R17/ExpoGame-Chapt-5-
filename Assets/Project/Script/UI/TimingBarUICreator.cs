using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Runtime creator for TimingBarUI if not found in scene
/// Attach this to any GameObject or it will auto-create on first ShieldCycle.Expose()
/// </summary>
public class TimingBarUICreator : MonoBehaviour
{
    [Header("Auto Create Settings")]
    [SerializeField] private bool createOnAwake = true;
    [SerializeField] private Vector2 anchoredPosition = new Vector2(750f, 0f); // Right side
    [SerializeField] private Vector2 sizeDelta = new Vector2(60f, 300f); // Vertical bar
    
    void Awake()
    {
        if (createOnAwake)
        {
            CreateTimingBarUI();
        }
    }
    
    /// <summary>
    /// Create TimingBarUI GameObject in scene
    /// </summary>
    [ContextMenu("Create Timing Bar UI")]
    public void CreateTimingBarUI()
    {
        // Check if already exists
        if (TimingBarUI.Instance != null)
        {
            Debug.Log("[TimingBarUICreator] TimingBarUI already exists in scene.");
            return;
        }
        
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[TimingBarUICreator] No Canvas found in scene! Create a Canvas first.");
            return;
        }
        
        // Create TimingBarUI GameObject
        GameObject timingBarObj = new GameObject("TimingBarUI");
        timingBarObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rectTransform = timingBarObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 0.5f); // Right center
        rectTransform.anchorMax = new Vector2(1f, 0.5f);
        rectTransform.pivot = new Vector2(1f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
        
        // Create Slider
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(timingBarObj.transform, false);
        
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.direction = Slider.Direction.BottomToTop; // Vertical
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.interactable = false;
        
        // Create Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        bgImage.raycastTarget = false;
        
        // Create Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(5f, 5f);
        fillAreaRect.offsetMax = new Vector2(-5f, -5f);
        
        // Create Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.green;
        fillImage.raycastTarget = false;
        
        slider.fillRect = fillRect;
        
        // Create Perfect Zone Indicator
        GameObject perfectZoneObj = new GameObject("PerfectZone");
        perfectZoneObj.transform.SetParent(fillAreaObj.transform, false);
        
        RectTransform perfectZoneRect = perfectZoneObj.AddComponent<RectTransform>();
        perfectZoneRect.anchorMin = new Vector2(0f, 0.4f); // 40%
        perfectZoneRect.anchorMax = new Vector2(1f, 0.6f); // 60%
        perfectZoneRect.offsetMin = Vector2.zero;
        perfectZoneRect.offsetMax = Vector2.zero;
        
        Image perfectZoneImage = perfectZoneObj.AddComponent<Image>();
        perfectZoneImage.color = new Color(0f, 1f, 0f, 0.3f); // Transparent green
        perfectZoneImage.raycastTarget = false;
        
        // Create Feedback Text
        GameObject textObj = new GameObject("FeedbackText");
        textObj.transform.SetParent(timingBarObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 0f);
        textRect.anchoredPosition = new Vector2(0f, 10f);
        textRect.sizeDelta = new Vector2(0f, 50f);
        
        TextMeshProUGUI feedbackText = textObj.AddComponent<TextMeshProUGUI>();
        feedbackText.text = "";
        feedbackText.fontSize = 24f;
        feedbackText.alignment = TextAlignmentOptions.Center;
        feedbackText.color = Color.white;
        feedbackText.raycastTarget = false;
        
        // Add TimingBarUI Component
        TimingBarUI timingBarUI = timingBarObj.AddComponent<TimingBarUI>();
        
        // Set references using reflection (private fields)
        var sliderField = typeof(TimingBarUI).GetField("timingSlider", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var fillField = typeof(TimingBarUI).GetField("fillImage", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var bgField = typeof(TimingBarUI).GetField("backgroundImage", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var perfectZoneField = typeof(TimingBarUI).GetField("perfectZoneIndicator", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var textField = typeof(TimingBarUI).GetField("feedbackText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (sliderField != null) sliderField.SetValue(timingBarUI, slider);
        if (fillField != null) fillField.SetValue(timingBarUI, fillImage);
        if (bgField != null) bgField.SetValue(timingBarUI, bgImage);
        if (perfectZoneField != null) perfectZoneField.SetValue(timingBarUI, perfectZoneRect);
        if (textField != null) textField.SetValue(timingBarUI, feedbackText);
        
        // Initially hide
        timingBarObj.SetActive(false);
        
        Debug.Log($"<color=lime>✅ [TimingBarUICreator] Successfully created TimingBarUI at {canvas.name}/TimingBarUI</color>");
        Debug.Log($"<color=lime>   Position: Right side ({anchoredPosition.x}, {anchoredPosition.y})</color>");
        Debug.Log($"<color=lime>   Size: {sizeDelta.x}x{sizeDelta.y}</color>");
    }
}

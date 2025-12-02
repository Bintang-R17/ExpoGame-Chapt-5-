using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script untuk setup komponen Sniper secara otomatis
/// Attach ini ke Player GameObject dan jalankan sekali
/// </summary>
public class SniperSetupHelper : MonoBehaviour
{
    [Header("Setup Options")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createTargetIndicator = true;
    [SerializeField] private LayerMask enemyLayer = -1; // Default all layers
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 50f;
    [SerializeField] private float targetingAngle = 45f;
    
    [Header("Visual Effect Settings")]
    [SerializeField] private int orbCount = 5;
    [SerializeField] private float orbitRadius = 1.5f;
    [SerializeField] private Color sniperBlue = new Color(0.3f, 0.7f, 1f);
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupSniperComponents();
        }
    }
    
    [ContextMenu("Setup Sniper Components")]
    public void SetupSniperComponents()
    {
        Debug.Log("🎯 Starting Sniper Setup...");
        
        // 1. Setup SniperAutoTargeting
        SetupAutoTargeting();
        
        // 2. Setup SniperVisualEffect
        SetupVisualEffect();
        
        // 3. Setup Target Indicator UI
        if (createTargetIndicator)
        {
            SetupTargetIndicatorUI();
        }
        
        // 4. Setup SniperCombat references
        SetupCombatReferences();
        
        Debug.Log("✅ Sniper Setup Complete!");
    }
    
    void SetupAutoTargeting()
    {
        SniperAutoTargeting autoTarget = GetComponent<SniperAutoTargeting>();
        if (autoTarget == null)
        {
            autoTarget = gameObject.AddComponent<SniperAutoTargeting>();
            Debug.Log("   ✓ SniperAutoTargeting added");
        }
        
        // Set via reflection since fields are private
        var type = typeof(SniperAutoTargeting);
        
        // Detection settings
        SetPrivateField(autoTarget, "detectionRange", detectionRange);
        SetPrivateField(autoTarget, "targetingAngle", targetingAngle);
        SetPrivateField(autoTarget, "enemyLayer", enemyLayer);
        
        Debug.Log($"   ✓ Detection Range: {detectionRange}m, Angle: {targetingAngle}°");
    }
    
    void SetupVisualEffect()
    {
        SniperVisualEffect visualEffect = GetComponent<SniperVisualEffect>();
        if (visualEffect == null)
        {
            visualEffect = gameObject.AddComponent<SniperVisualEffect>();
            Debug.Log("   ✓ SniperVisualEffect added");
        }
        
        // Configure via public methods if available
        if (visualEffect != null)
        {
            visualEffect.SetOrbCount(orbCount);
            visualEffect.SetOrbitRadius(orbitRadius);
            visualEffect.SetOrbColor(sniperBlue);
            Debug.Log($"   ✓ Visual Effect: {orbCount} orbs, radius {orbitRadius}m");
        }
    }
    
    void SetupTargetIndicatorUI()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("SniperUI_Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("   ✓ Canvas created");
        }
        
        // Create Target Indicator Prefab in Canvas
        GameObject indicatorObj = new GameObject("SniperTargetIndicator");
        indicatorObj.transform.SetParent(canvas.transform, false);
        
        // Add RectTransform
        RectTransform rect = indicatorObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 100);
        
        // Add Image for the ring
        Image ringImage = indicatorObj.AddComponent<Image>();
        ringImage.color = Color.red;
        ringImage.raycastTarget = false;
        
        // Try to use a circle sprite or create a simple ring
        // For now, we'll use a simple filled circle (you can replace with ring sprite later)
        ringImage.sprite = CreateCircleSprite();
        
        // Add SniperTargetIndicator component
        SniperTargetIndicator indicator = indicatorObj.AddComponent<SniperTargetIndicator>();
        
        // Set reference in SniperAutoTargeting
        SniperAutoTargeting autoTarget = GetComponent<SniperAutoTargeting>();
        if (autoTarget != null)
        {
            SetPrivateField(autoTarget, "indicatorPrefab", indicator);
            SetPrivateField(autoTarget, "indicatorParent", canvas.transform);
            Debug.Log("   ✓ Target Indicator UI created");
        }
        
        // Hide initially
        indicatorObj.SetActive(false);
    }
    
    void SetupCombatReferences()
    {
        SniperCombat combat = GetComponent<SniperCombat>();
        if (combat == null)
        {
            Debug.LogWarning("   ⚠ SniperCombat component not found");
            return;
        }
        
        SniperAutoTargeting autoTarget = GetComponent<SniperAutoTargeting>();
        if (autoTarget != null)
        {
            SetPrivateField(combat, "autoTargeting", autoTarget);
            SetPrivateField(combat, "enemyLayer", enemyLayer);
            Debug.Log("   ✓ SniperCombat references set");
        }
    }
    
    // Helper method to set private fields using reflection
    void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(obj, value);
        }
        else
        {
            Debug.LogWarning($"   ⚠ Field '{fieldName}' not found in {obj.GetType().Name}");
        }
    }
    
    // Create a simple circle sprite
    Sprite CreateCircleSprite()
    {
        int size = 128;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        float innerRadius = radius * 0.7f; // Ring thickness
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                // Create ring shape
                if (distance <= radius && distance >= innerRadius)
                {
                    float alpha = 1f - Mathf.Abs(distance - (innerRadius + radius) / 2f) / (radius - innerRadius);
                    pixels[y * size + x] = new Color(1, 1, 1, alpha);
                }
                else
                {
                    pixels[y * size + x] = new Color(1, 1, 1, 0);
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}

using UnityEngine;
using UnityEngine.UI;

public class AutoSetupPlayerHUD : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool autoCreate = true;
    
    void Start()
    {
        if (!autoCreate) return;
        
        Canvas canvas = FindFirstObjectByType<Canvas>();
        
        if (canvas == null)
        {
            CreateCanvas();
            canvas = FindFirstObjectByType<Canvas>();
        }
        
        if (canvas != null)
        {
            CreateSimpleHUD(canvas);
        }
    }
    
    void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("PlayerHUD");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
    }
    
    void CreateSimpleHUD(Canvas canvas)
    {
        // Health Bar (Top-Left)
        GameObject healthPanel = CreatePanel(canvas.transform, "HealthPanel", new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(300, 80));
        
        GameObject healthSlider = CreateSlider(healthPanel.transform, "HealthSlider", 100, Color.green);
        
        GameObject healthText = CreateText(healthPanel.transform, "HealthText", "100 / 100", 24);
        RectTransform healthTextRect = healthText.GetComponent<RectTransform>();
        healthTextRect.anchorMin = new Vector2(0, 0.6f);
        healthTextRect.anchorMax = new Vector2(1, 1);
        
        // Ammo Counter (Bottom-Right)
        GameObject ammoPanel = CreatePanel(canvas.transform, "AmmoPanel", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20, 20), new Vector2(200, 80));
        
        GameObject ammoText = CreateText(ammoPanel.transform, "AmmoText", "8 / 8", 32);
        
        // Combat Indicator (Top-Center)
        GameObject combatText = CreateText(canvas.transform, "CombatMode", "COMBAT MODE", 28);
        RectTransform combatRect = combatText.GetComponent<RectTransform>();
        combatRect.anchorMin = new Vector2(0.5f, 1);
        combatRect.anchorMax = new Vector2(0.5f, 1);
        combatRect.anchoredPosition = new Vector2(0, -20);
        combatRect.sizeDelta = new Vector2(300, 50);
        combatText.GetComponent<Text>().color = Color.red;
        combatText.SetActive(false);
        
        // Add ImprovedPlayerHUD component
        canvas.gameObject.AddComponent<ImprovedPlayerHUD>();
    }
    
    GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.7f);
        
        return panel;
    }
    
    GameObject CreateSlider(Transform parent, string name, float maxValue, Color fillColor)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent);
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = maxValue;
        slider.value = maxValue;
        
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.05f, 0.3f);
        sliderRect.anchorMax = new Vector2(0.95f, 0.6f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = fillColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        slider.fillRect = fillRect;
        
        return sliderObj;
    }
    
    GameObject CreateText(Transform parent, string name, string content, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        
        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        return textObj;
    }
}

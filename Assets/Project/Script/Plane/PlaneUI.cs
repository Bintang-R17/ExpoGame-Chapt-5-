using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlaneUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArcadePlaneController planeController;
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI altitudeText;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private Slider altitudeSlider;
    [SerializeField] private Image boostIndicator;
    [SerializeField] private TextMeshProUGUI boostCooldownText;
    
    [Header("Settings")]
    [SerializeField] private float maxDisplayAltitude = 200f;
    [SerializeField] private float maxDisplaySpeed = 100f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color boostColor = Color.cyan;
    [SerializeField] private Color cooldownColor = Color.gray;
    
    void Start()
    {
        // Auto-find plane if not assigned
        if (planeController == null)
        {
            planeController = FindFirstObjectByType<ArcadePlaneController>();
        }
        
        if (planeController == null)
        {
            Debug.LogWarning("⚠️ PlaneUI: ArcadePlaneController not found!");
            enabled = false;
        }
    }
    
    void Update()
    {
        if (planeController == null) return;
        
        UpdateSpeed();
        UpdateAltitude();
        UpdateBoost();
    }
    
    void UpdateSpeed()
    {
        float speed = planeController.GetCurrentSpeed();
        
        // Update text
        if (speedText != null)
        {
            speedText.text = $"{speed:F0} km/h";
        }
        
        // Update slider
        if (speedSlider != null)
        {
            speedSlider.value = speed / maxDisplaySpeed;
        }
    }
    
    void UpdateAltitude()
    {
        float altitude = planeController.GetAltitude();
        
        // Update text
        if (altitudeText != null)
        {
            altitudeText.text = $"{altitude:F0} m";
        }
        
        // Update slider
        if (altitudeSlider != null)
        {
            altitudeSlider.value = altitude / maxDisplayAltitude;
        }
    }
    
    void UpdateBoost()
    {
        bool isBoosting = planeController.IsBoosting();
        float cooldown = planeController.GetBoostCooldown();
        
        // Update boost indicator color
        if (boostIndicator != null)
        {
            if (isBoosting)
            {
                boostIndicator.color = boostColor;
            }
            else if (cooldown > 0)
            {
                boostIndicator.color = cooldownColor;
            }
            else
            {
                boostIndicator.color = normalColor;
            }
        }
        
        // Update cooldown text
        if (boostCooldownText != null)
        {
            if (isBoosting)
            {
                boostCooldownText.text = "BOOST!";
                boostCooldownText.color = boostColor;
            }
            else if (cooldown > 0)
            {
                boostCooldownText.text = $"{cooldown:F1}s";
                boostCooldownText.color = cooldownColor;
            }
            else
            {
                boostCooldownText.text = "READY";
                boostCooldownText.color = normalColor;
            }
        }
    }
}

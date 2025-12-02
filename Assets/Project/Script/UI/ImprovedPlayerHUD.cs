using UnityEngine;
using UnityEngine.UI;

public class ImprovedPlayerHUD : MonoBehaviour
{
    [Header("Health Display")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFill;
    [SerializeField] private Text healthText;
    
    [Header("Weapon/Ammo Display")]
    [SerializeField] private Text ammoText;
    [SerializeField] private Text reloadText;
    [SerializeField] private GameObject combatIndicator;
    
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private OrbitalWeapons orbitalWeapons;
    
    private float currentHealthDisplay;
    
    void Start()
    {
        // Auto-find references
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            
        if (orbitalWeapons == null)
            orbitalWeapons = FindFirstObjectByType<OrbitalWeapons>();
        
        // Auto-find UI elements if not assigned
        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>();
            
        if (healthFill == null && healthSlider != null)
            healthFill = healthSlider.fillRect.GetComponent<Image>();
            
        if (healthText == null)
        {
            Text[] texts = GetComponentsInChildren<Text>();
            foreach (Text t in texts)
            {
                if (t.name == "HealthText")
                    healthText = t;
                else if (t.name == "AmmoText")
                    ammoText = t;
                else if (t.name == "CombatMode")
                    combatIndicator = t.gameObject;
            }
        }
        
        if (playerHealth != null)
        {
            currentHealthDisplay = playerHealth.CurrentHealth;
            
            // Set slider max value to match max health
            if (healthSlider != null)
            {
                healthSlider.maxValue = playerHealth.MaxHealth;
                healthSlider.minValue = 0;
                healthSlider.value = currentHealthDisplay;
            }
            
            // Subscribe to health change event
            playerHealth.OnHealthChanged.AddListener(OnHealthChanged);
        }
        
        Debug.Log($"✅ ImprovedPlayerHUD initialized - Health: {playerHealth?.CurrentHealth}/{playerHealth?.MaxHealth}");
    }
    
    void OnHealthChanged(float healthPercentage)
    {
        // Update immediately when health changes
        UpdateHealthDisplay();
    }
    
    void Update()
    {
        UpdateHealthDisplay();
        UpdateAmmoDisplay();
        UpdateCombatInfo();
    }
    
    void UpdateHealthDisplay()
    {
        if (playerHealth == null || healthSlider == null) return;
        
        float currentHealth = playerHealth.CurrentHealth;
        float maxHealth = playerHealth.MaxHealth;
        float healthPercent = currentHealth / maxHealth;
        
        // Set slider max if not correct
        if (healthSlider.maxValue != maxHealth)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.minValue = 0;
        }
        
        // Smooth health animation
        currentHealthDisplay = Mathf.Lerp(currentHealthDisplay, currentHealth, Time.deltaTime * 10f);
        healthSlider.value = currentHealthDisplay;
        
        // Update color
        if (healthFill != null)
        {
            if (healthPercent > 0.6f)
                healthFill.color = Color.green;
            else if (healthPercent > 0.3f)
                healthFill.color = new Color(1f, 0.7f, 0f);
            else
                healthFill.color = Color.red;
        }
        
        // Update text
        if (healthText != null)
        {
            healthText.text = Mathf.Ceil(currentHealth) + " / " + Mathf.Ceil(maxHealth);
        }
    }
    
    void UpdateAmmoDisplay()
    {
        if (orbitalWeapons == null || ammoText == null) return;
        
        int currentAmmo = orbitalWeapons.GetAvailableWeapons();
        int maxAmmo = orbitalWeapons.GetMaxWeapons();
        bool isReloading = orbitalWeapons.IsReloading();
        
        if (isReloading)
        {
            ammoText.text = "RELOADING...";
            if (ammoText.color != Color.yellow)
                ammoText.color = Color.yellow;
        }
        else
        {
            ammoText.text = currentAmmo + " / " + maxAmmo;
            if (ammoText.color != Color.white)
                ammoText.color = Color.white;
        }
    }
    
    void UpdateCombatInfo()
    {
        if (orbitalWeapons == null || combatIndicator == null) return;
        
        // Combat indicator always active in new role system
        if (!combatIndicator.activeSelf)
        {
            combatIndicator.SetActive(true);
        }
    }
}

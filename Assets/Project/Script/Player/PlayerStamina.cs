using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina;

    [Header("Regeneration")]
    [SerializeField] private float regenRate = 20f; // Per detik
    [SerializeField] private float regenDelay = 1f; // Delay setelah pakai stamina
    private float timeSinceLastUse;

    [Header("Costs")]
    [SerializeField] private float sprintCostPerSecond = 15f;
    [SerializeField] private float dodgeCost = 25f;
    [SerializeField] private float jumpCost = 10f;

    [Header("UI References")]
    [SerializeField] private Slider staminaBar;
    [SerializeField] private Text staminaText;
    [SerializeField] private CanvasGroup staminaBarGroup; // For fade in/out

    [Header("Visual Settings")]
    [SerializeField] private bool autoHideStaminaBar = true;
    [SerializeField] private float hideDelay = 2f;
    private float hideTimer;

    // Properties
    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;
    public float StaminaPercentage => currentStamina / maxStamina;
    public bool IsEmpty => currentStamina <= 0;

    void Start()
    {
        currentStamina = maxStamina;
        timeSinceLastUse = regenDelay;
        hideTimer = hideDelay;

        UpdateStaminaUI();

        if (autoHideStaminaBar && staminaBarGroup != null)
        {
            staminaBarGroup.alpha = 0f;
        }
    }

    void Update()
    {
        // Stamina regeneration
        timeSinceLastUse += Time.deltaTime;

        if (timeSinceLastUse >= regenDelay && currentStamina < maxStamina)
        {
            RegenerateStamina(regenRate * Time.deltaTime);
        }

        // Auto-hide stamina bar
        if (autoHideStaminaBar && staminaBarGroup != null)
        {
            if (currentStamina >= maxStamina)
            {
                hideTimer -= Time.deltaTime;
                if (hideTimer <= 0)
                {
                    staminaBarGroup.alpha = Mathf.Lerp(staminaBarGroup.alpha, 0f, Time.deltaTime * 5f);
                }
            }
            else
            {
                hideTimer = hideDelay;
                staminaBarGroup.alpha = Mathf.Lerp(staminaBarGroup.alpha, 1f, Time.deltaTime * 10f);
            }
        }
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina < amount)
            return false;

        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        timeSinceLastUse = 0f;
        UpdateStaminaUI();

        return true;
    }

    public bool CanUseStamina(float amount)
    {
        return currentStamina >= amount;
    }

    public bool UseSprint(float deltaTime)
    {
        return UseStamina(sprintCostPerSecond * deltaTime);
    }

    public bool UseDodge()
    {
        return UseStamina(dodgeCost);
    }

    public bool UseJump()
    {
        return UseStamina(jumpCost);
    }

    void RegenerateStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        UpdateStaminaUI();
    }

    void UpdateStaminaUI()
    {
        if (staminaBar != null)
        {
            staminaBar.value = StaminaPercentage;
        }

        if (staminaText != null)
        {
            staminaText.text = $"{Mathf.Ceil(currentStamina)}";
        }
    }

    public void SetMaxStamina(float newMaxStamina)
    {
        maxStamina = newMaxStamina;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        UpdateStaminaUI();
    }

    public void FullRestore()
    {
        currentStamina = maxStamina;
        UpdateStaminaUI();
    }
}
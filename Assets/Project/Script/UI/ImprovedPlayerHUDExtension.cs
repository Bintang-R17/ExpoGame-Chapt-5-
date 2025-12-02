using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component untuk menambahkan UnityEvent listener
/// Gunakan ini jika tidak ingin mengubah script existing
/// </summary>
public class ImprovedPlayerHUDExtension : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private ImprovedPlayerHUD hud;
    
    void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        hud = GetComponent<ImprovedPlayerHUD>();
        
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(OnHealthChanged);
        }
    }
    
    void OnHealthChanged(float percentage)
    {
        // Force update HUD
        if (hud != null)
        {
            hud.SendMessage("UpdateHealthDisplay", SendMessageOptions.DontRequireReceiver);
        }
    }
    
    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(OnHealthChanged);
        }
    }
}

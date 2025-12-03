using UnityEngine;

/// <summary>
/// Contoh implementasi enemy yang bisa menerima damage
/// </summary>
public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Visual Feedback")]
    [SerializeField] private Renderer meshRenderer;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    
    private Color originalColor;
    private bool isFlashing = false;
    
    private void Start()
    {
        currentHealth = maxHealth;
        
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<Renderer>();
        }
        
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }
    
    /// <summary>
    /// Implementasi interface IDamageable
    /// </summary>
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        // Visual feedback
        if (meshRenderer != null && !isFlashing)
        {
            StartCoroutine(FlashDamage());
        }
        
        // Check jika mati
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Flash merah saat kena damage
    /// </summary>
    private System.Collections.IEnumerator FlashDamage()
    {
        isFlashing = true;
        meshRenderer.material.color = damageColor;
        
        yield return new WaitForSeconds(flashDuration);
        
        meshRenderer.material.color = originalColor;
        isFlashing = false;
    }
    
    /// <summary>
    /// Dipanggil saat health habis
    /// </summary>
    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        
        // Tambahkan efek death di sini (particle, sound, dll)
        
        // Destroy enemy
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Heal enemy
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{gameObject.name} healed {amount}. Health: {currentHealth}/{maxHealth}");
    }
}

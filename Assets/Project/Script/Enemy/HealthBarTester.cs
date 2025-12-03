using UnityEngine;

/// <summary>
/// Test script untuk health bar - attach ke enemy untuk test manual
/// Tekan tombol untuk test damage
/// </summary>
public class HealthBarTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private KeyCode damageKey = KeyCode.T;
    [SerializeField] private float testDamage = 10f;
    [SerializeField] private KeyCode healKey = KeyCode.Y;
    [SerializeField] private float testHeal = 10f;
    
    private EnemyHealth enemyHealth;
    
    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            Debug.LogError("❌ No EnemyHealth component found!");
            enabled = false;
            return;
        }
        
        Debug.Log($"✅ HealthBarTester ready on {gameObject.name}");
        Debug.Log($"   Press [{damageKey}] to deal {testDamage} damage");
        Debug.Log($"   Press [{healKey}] to heal {testHeal} HP");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(damageKey))
        {
            Debug.Log($"🔥 TEST DAMAGE: {testDamage} on {gameObject.name}");
            enemyHealth.TakeDamage(testDamage);
        }
        
        if (Input.GetKeyDown(healKey))
        {
            Debug.Log($"💚 TEST HEAL: {testHeal} on {gameObject.name}");
            // Heal by negative damage (if supported)
            // Or directly access health field via reflection
            var currentHealthField = typeof(EnemyHealth).GetField("currentHealth", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var maxHealthField = typeof(EnemyHealth).GetField("maxHealth", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            if (currentHealthField != null && maxHealthField != null)
            {
                float current = (float)currentHealthField.GetValue(enemyHealth);
                float max = (float)maxHealthField.GetValue(enemyHealth);
                float newHealth = Mathf.Min(current + testHeal, max);
                currentHealthField.SetValue(enemyHealth, newHealth);
                
                // Update health bar
                var healthBarUI = enemyHealth.GetComponentInChildren<EnemyHealthBarUI>();
                if (healthBarUI != null)
                {
                    healthBarUI.SetHealth(newHealth, max);
                }
                
                Debug.Log($"   New HP: {newHealth}/{max}");
            }
        }
    }
    
    void OnGUI()
    {
        if (enemyHealth == null) return;
        
        // Show HP on screen
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f);
        if (screenPos.z > 0)
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(screenPos.x - 100, Screen.height - screenPos.y, 200, 30), 
                $"{gameObject.name}: {enemyHealth.GetHealthPercentage() * 100f:F0}% HP");
        }
    }
}

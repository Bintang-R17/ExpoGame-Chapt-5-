using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controller untuk auto-switch mode berdasarkan kondisi player
/// Attach ke GameObject Player yang sama dengan OrbitalWeapons
/// </summary>
public class OrbitalWeaponsController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private OrbitalWeapons orbitalWeapons;

    [Header("Auto Mode Switch")]
    [SerializeField] private bool autoSwitchOnNearbyEnemy = true;
    [SerializeField] private float enemyDetectionRadius = 10f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float switchCooldown = 1f;

    [Header("Manual Control")]
    [SerializeField] private bool allowManualToggle = true;

    [Header("Visual Feedback")]
    [SerializeField] private bool showModeIndicator = true;
    [SerializeField] private Color defenseColor = Color.cyan;
    [SerializeField] private Color combatColor = Color.red;

    private float lastSwitchTime = -999f;
    private bool hasNearbyEnemy = false;

    void Start()
    {
        if (orbitalWeapons == null)
            orbitalWeapons = GetComponent<OrbitalWeapons>();

        if (orbitalWeapons == null)
        {
            Debug.LogError("OrbitalWeapons component tidak ditemukan!");
            enabled = false;
        }
    }

    void Update()
    {
        if (orbitalWeapons == null) return;

        // Role-based system - no mode switching needed
        if (autoSwitchOnNearbyEnemy)
        {
            CheckNearbyEnemies();
        }
    }

    void CheckNearbyEnemies()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, enemyDetectionRadius, enemyLayer);
        hasNearbyEnemy = enemies.Length > 0;
    }

    void OnDrawGizmosSelected()
    {
        if (!autoSwitchOnNearbyEnemy) return;

        Gizmos.color = hasNearbyEnemy ? combatColor : defenseColor;
        Gizmos.DrawWireSphere(transform.position, enemyDetectionRadius);
    }

    void OnGUI()
    {
        if (!showModeIndicator || orbitalWeapons == null) return;

        Color modeColor = combatColor;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = modeColor;
        style.alignment = TextAnchor.UpperRight;

        GUI.Label(new Rect(Screen.width - 250, 20, 230, 30), "⚔️ ORBITAL WEAPONS", style);

        if (allowManualToggle)
        {
            GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
            hintStyle.fontSize = 12;
            hintStyle.normal.textColor = Color.white;
            hintStyle.alignment = TextAnchor.UpperRight;

            string ammoText = $"Ammo: {orbitalWeapons.GetAvailableWeapons()}/{orbitalWeapons.GetMaxWeapons()}";
            if (orbitalWeapons.IsReloading())
            {
                float progress = orbitalWeapons.GetReloadProgress() * 100f;
                ammoText = $"Reloading... {progress:F0}%";
            }

            GUI.Label(new Rect(Screen.width - 250, 50, 230, 20), ammoText, hintStyle);
        }
    }
}
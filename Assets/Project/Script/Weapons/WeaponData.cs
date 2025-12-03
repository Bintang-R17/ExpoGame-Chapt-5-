using UnityEngine;

/// <summary>
/// ScriptableObject yang menyimpan data senjata
/// </summary>
[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon System/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;
    
    [Header("Combat Stats")]
    [Tooltip("Damage yang diberikan senjata ini")]
    public float damage;
    
    [Tooltip("Cooldown antar serangan dalam detik")]
    public float attackCooldown;
    
    [Tooltip("Jarak serangan maksimal")]
    public float attackRange;
}

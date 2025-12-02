using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Visual effect for Sniper loadout - creates floating orbs that orbit vertically around player
/// 5 orbs moving up and down along the Y axis at same X/Z position
/// </summary>
public class SniperVisualEffect : MonoBehaviour
{
    [Header("Orb Settings")]
    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private int orbCount = 5;
    [SerializeField] private float orbitRadius = 1.5f;
    [SerializeField] private float heightRange = 3f; // Total height from feet to head
    [SerializeField] private float baseHeight = 0.5f; // Starting height from ground
    
    [Header("Animation")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private bool rotateOrbs = true;
    
    [Header("Visual")]
    [SerializeField] private Color orbColor = new Color(0.3f, 0.7f, 1f); // Sniper blue
    [SerializeField] private float orbScale = 0.3f;
    [SerializeField] private Material orbMaterial;
    
    private List<GameObject> orbs = new List<GameObject>();
    private List<float> orbOffsets = new List<float>(); // Phase offset for each orb
    private bool isActive = false;
    
    void Start()
    {
        // Don't create orbs on start - wait for role activation
    }
    
    public void ActivateEffect()
    {
        if (isActive) return;
        
        CreateOrbs();
        isActive = true;
        
        Debug.Log($"🎯 Sniper visual effect activated - {orbCount} orbs created");
    }
    
    public void DeactivateEffect()
    {
        if (!isActive) return;
        
        DestroyOrbs();
        isActive = false;
        
        Debug.Log("🎯 Sniper visual effect deactivated");
    }
    
    void CreateOrbs()
    {
        for (int i = 0; i < orbCount; i++)
        {
            GameObject orb = CreateOrb(i);
            orbs.Add(orb);
            
            // Each orb starts at different phase so they're spread vertically
            float phaseOffset = (float)i / orbCount * Mathf.PI * 2f;
            orbOffsets.Add(phaseOffset);
        }
    }
    
    GameObject CreateOrb(int index)
    {
        GameObject orb;
        
        if (orbPrefab != null)
        {
            orb = Instantiate(orbPrefab, transform);
        }
        else
        {
            // Create default sphere if no prefab provided
            orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.transform.SetParent(transform);
            
            // Remove collider - this is visual only
            Collider col = orb.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }
        
        orb.name = $"SniperOrb_{index}";
        orb.transform.localScale = Vector3.one * orbScale;
        
        // Apply material and color
        Renderer renderer = orb.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (orbMaterial != null)
            {
                renderer.material = orbMaterial;
            }
            renderer.material.color = orbColor;
            
            // Make it glow
            if (renderer.material.HasProperty("_EmissionColor"))
            {
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", orbColor * 2f);
            }
        }
        
        return orb;
    }
    
    void Update()
    {
        if (!isActive || orbs.Count == 0) return;
        
        UpdateOrbPositions();
    }
    
    void UpdateOrbPositions()
    {
        float time = Time.time * moveSpeed;
        
        for (int i = 0; i < orbs.Count; i++)
        {
            if (orbs[i] == null) continue;
            
            // Calculate Y position using sine wave + phase offset
            // This makes orbs move up and down at same X position
            float verticalOffset = Mathf.Sin(time + orbOffsets[i]) * (heightRange / 2f);
            float yPos = baseHeight + (heightRange / 2f) + verticalOffset;
            
            // Keep X and Z constant (same as player), only Y changes
            Vector3 localPos = new Vector3(0f, yPos, 0f);
            
            // Add slight circular offset so they orbit around player
            float angle = (float)i / orbCount * Mathf.PI * 2f;
            localPos.x = Mathf.Cos(angle) * orbitRadius;
            localPos.z = Mathf.Sin(angle) * orbitRadius;
            
            orbs[i].transform.localPosition = localPos;
            
            // Rotate orbs for extra visual flair
            if (rotateOrbs)
            {
                orbs[i].transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    void DestroyOrbs()
    {
        foreach (GameObject orb in orbs)
        {
            if (orb != null)
            {
                Destroy(orb);
            }
        }
        
        orbs.Clear();
        orbOffsets.Clear();
    }
    
    void OnDestroy()
    {
        DeactivateEffect();
    }
    
    // Public setters for customization
    public void SetOrbCount(int count)
    {
        if (count != orbCount)
        {
            orbCount = Mathf.Max(1, count);
            if (isActive)
            {
                DeactivateEffect();
                ActivateEffect();
            }
        }
    }
    
    public void SetOrbitRadius(float radius)
    {
        orbitRadius = Mathf.Max(0.1f, radius);
    }
    
    public void SetHeightRange(float range)
    {
        heightRange = Mathf.Max(0.5f, range);
    }
    
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0.1f, speed);
    }
    
    public void SetOrbColor(Color color)
    {
        orbColor = color;
        
        // Update existing orbs
        foreach (GameObject orb in orbs)
        {
            if (orb != null)
            {
                Renderer renderer = orb.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = color;
                    if (renderer.material.HasProperty("_EmissionColor"))
                    {
                        renderer.material.SetColor("_EmissionColor", color * 2f);
                    }
                }
            }
        }
    }
    
    public bool IsActive() => isActive;
}

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object pool for target indicators
/// Avoids Instantiate/Destroy overhead when switching targets
/// </summary>
public class TargetIndicatorPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private int initialPoolSize = 3;
    [SerializeField] private int maxPoolSize = 10;
    [SerializeField] private bool autoExpand = true;
    
    [Header("Debug")]
    [SerializeField] private bool logPoolActivity = false;
    
    private Queue<TargetIndicator> availableIndicators = new Queue<TargetIndicator>();
    private List<TargetIndicator> activeIndicators = new List<TargetIndicator>();
    private Transform poolParent;
    
    void Awake()
    {
        // Check if prefab is assigned
        if (indicatorPrefab == null)
        {
            Debug.LogError("[TargetIndicatorPool] Indicator prefab not assigned! Indicator system will be disabled.");
            enabled = false; // Disable this component
            return;
        }
        
        // Create pool parent
        poolParent = new GameObject("TargetIndicatorPool").transform;
        poolParent.SetParent(transform);
        
        // Pre-warm pool
        InitializePool();
    }
    
    /// <summary>
    /// Initialize pool with initial size
    /// </summary>
    void InitializePool()
    {
        if (indicatorPrefab == null)
        {
            Debug.LogError("[TargetIndicatorPool] Indicator prefab not assigned!");
            return;
        }
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewIndicator();
        }
        
        if (logPoolActivity)
        {
            Debug.Log($"[TargetIndicatorPool] Initialized with {initialPoolSize} indicators");
        }
    }
    
    /// <summary>
    /// Create a new indicator instance
    /// </summary>
    TargetIndicator CreateNewIndicator()
    {
        if (indicatorPrefab == null)
        {
            Debug.LogError("[TargetIndicatorPool] Cannot create indicator - prefab is null!");
            return null;
        }
        
        GameObject obj = Instantiate(indicatorPrefab, poolParent);
        TargetIndicator indicator = obj.GetComponent<TargetIndicator>();
        
        if (indicator == null)
        {
            indicator = obj.AddComponent<TargetIndicator>();
        }
        
        obj.SetActive(false);
        availableIndicators.Enqueue(indicator);
        
        return indicator;
    }
    
    /// <summary>
    /// Get an indicator from the pool
    /// </summary>
    public TargetIndicator GetIndicator()
    {
        TargetIndicator indicator;
        
        Debug.Log($"<color=magenta>[Pool] GetIndicator called - Available: {availableIndicators.Count}, Active: {activeIndicators.Count}</color>");
        
        // Try to get from available pool
        if (availableIndicators.Count > 0)
        {
            indicator = availableIndicators.Dequeue();
            Debug.Log($"<color=magenta>[Pool] Retrieved from queue: {indicator.gameObject.name}</color>");
        }
        // Create new if pool exhausted and expansion allowed
        else if (autoExpand && (maxPoolSize <= 0 || activeIndicators.Count < maxPoolSize))
        {
            indicator = CreateNewIndicator();
            
            if (indicator == null)
            {
                Debug.LogError("[TargetIndicatorPool] Failed to create indicator - prefab is null!");
                return null;
            }
            
            Debug.Log($"<color=magenta>[Pool] Expanded pool - Created: {indicator.gameObject.name}</color>");
            
            if (logPoolActivity)
            {
                Debug.Log($"[TargetIndicatorPool] Expanded pool - Active: {activeIndicators.Count + 1}");
            }
        }
        else
        {
            Debug.LogWarning("[TargetIndicatorPool] Pool exhausted and expansion disabled!");
            return null;
        }
        
        if (indicator != null)
        {
            activeIndicators.Add(indicator);
            indicator.gameObject.SetActive(true);
        }
        
        if (logPoolActivity)
        {
            Debug.Log($"[TargetIndicatorPool] Get indicator - Active: {activeIndicators.Count}, Available: {availableIndicators.Count}");
        }
        
        return indicator;
    }
    
    /// <summary>
    /// Return an indicator to the pool
    /// </summary>
    public void ReturnIndicator(TargetIndicator indicator)
    {
        if (indicator == null) return;
        
        // Remove from active list
        if (activeIndicators.Contains(indicator))
        {
            activeIndicators.Remove(indicator);
        }
        
        // Clear target and hide
        indicator.ClearTarget();
        indicator.gameObject.SetActive(false);
        
        // Return to pool parent
        indicator.transform.SetParent(poolParent);
        
        // Add to available queue
        if (!availableIndicators.Contains(indicator))
        {
            availableIndicators.Enqueue(indicator);
        }
        
        if (logPoolActivity)
        {
            Debug.Log($"[TargetIndicatorPool] Return indicator - Active: {activeIndicators.Count}, Available: {availableIndicators.Count}");
        }
    }
    
    /// <summary>
    /// Return all active indicators to pool
    /// </summary>
    public void ReturnAll()
    {
        // Create copy to avoid modification during iteration
        List<TargetIndicator> toReturn = new List<TargetIndicator>(activeIndicators);
        
        foreach (var indicator in toReturn)
        {
            ReturnIndicator(indicator);
        }
        
        if (logPoolActivity)
        {
            Debug.Log($"[TargetIndicatorPool] Returned all indicators");
        }
    }
    
    /// <summary>
    /// Get pool statistics
    /// </summary>
    public void GetPoolStats(out int active, out int available, out int total)
    {
        active = activeIndicators.Count;
        available = availableIndicators.Count;
        total = active + available;
    }
    
    /// <summary>
    /// Set indicator prefab
    /// </summary>
    public void SetPrefab(GameObject prefab)
    {
        indicatorPrefab = prefab;
    }
    
    /// <summary>
    /// Clear and recreate pool
    /// </summary>
    public void ResetPool()
    {
        // Destroy all instances
        foreach (var indicator in activeIndicators)
        {
            if (indicator != null)
                Destroy(indicator.gameObject);
        }
        
        while (availableIndicators.Count > 0)
        {
            var indicator = availableIndicators.Dequeue();
            if (indicator != null)
                Destroy(indicator.gameObject);
        }
        
        activeIndicators.Clear();
        availableIndicators.Clear();
        
        // Reinitialize
        InitializePool();
    }
    
    void OnDestroy()
    {
        // Cleanup on destroy
        ReturnAll();
    }
}

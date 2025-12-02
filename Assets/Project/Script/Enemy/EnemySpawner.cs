using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPoint
    {
        public Transform location;
        public GameObject treasureObject; // Visual indicator (chest, item, etc)
        [Range(1, 10)] public int minEnemies = 1;
        [Range(1, 10)] public int maxEnemies = 3;
        public float spawnRadius = 5f;
        public bool hasSpawned = false;
    }

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs; // Drag 2+ enemy prefabs here
    
    [Header("Spawn Points")]
    [SerializeField] private SpawnPoint[] spawnPoints;
    
    [Header("Spawn Settings")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private float spawnDelay = 0.5f; // Delay between each enemy spawn
    [SerializeField] private float minDistanceFromTreasure = 2f;
    [SerializeField] private float maxDistanceFromTreasure = 8f;
    
    [Header("Spawn Behavior")]
    [SerializeField] private bool randomizeEnemyType = true;
    [SerializeField] private bool faceTowardsTreasure = true;
    [SerializeField] private bool spawnOnGround = true;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = Color.red;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnAllEnemies();
        }
    }

    public void SpawnAllEnemies()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("No enemy prefabs assigned to EnemySpawner!");
            return;
        }

        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.location != null && !spawnPoint.hasSpawned)
            {
                StartCoroutine(SpawnEnemiesAtPoint(spawnPoint));
            }
        }
    }

    System.Collections.IEnumerator SpawnEnemiesAtPoint(SpawnPoint spawnPoint)
    {
        int enemyCount = Random.Range(spawnPoint.minEnemies, spawnPoint.maxEnemies + 1);
        
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnSingleEnemy(spawnPoint);
            yield return new WaitForSeconds(spawnDelay);
        }
        
        spawnPoint.hasSpawned = true;
    }

    void SpawnSingleEnemy(SpawnPoint spawnPoint)
    {
        // Random enemy prefab
        GameObject enemyPrefab = randomizeEnemyType 
            ? enemyPrefabs[Random.Range(0, enemyPrefabs.Length)] 
            : enemyPrefabs[0];

        // Random position around treasure
        Vector3 spawnPosition = GetRandomSpawnPosition(spawnPoint);

        // Spawn enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.transform.SetParent(transform); // Organize in hierarchy
        
        // Face towards treasure
        if (faceTowardsTreasure && spawnPoint.location != null)
        {
            Vector3 directionToTreasure = (spawnPoint.location.position - enemy.transform.position).normalized;
            directionToTreasure.y = 0;
            if (directionToTreasure.sqrMagnitude > 0.01f)
            {
                enemy.transform.rotation = Quaternion.LookRotation(directionToTreasure);
            }
        }

        spawnedEnemies.Add(enemy);
    }

    Vector3 GetRandomSpawnPosition(SpawnPoint spawnPoint)
    {
        Vector3 treasurePos = spawnPoint.location.position;
        Vector3 randomPos = Vector3.zero;
        
        // Try to find valid position (max 10 attempts)
        for (int attempt = 0; attempt < 10; attempt++)
        {
            // Random angle around treasure
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minDistanceFromTreasure, maxDistanceFromTreasure);
            
            // Calculate position
            randomPos = treasurePos + new Vector3(
                Mathf.Cos(angle) * distance,
                0f,
                Mathf.Sin(angle) * distance
            );

            // Snap to ground if enabled
            if (spawnOnGround)
            {
                RaycastHit hit;
                if (Physics.Raycast(randomPos + Vector3.up * 10f, Vector3.down, out hit, 20f, groundLayer))
                {
                    randomPos = hit.point;
                    break;
                }
            }
            else
            {
                randomPos.y = treasurePos.y;
                break;
            }
        }

        return randomPos;
    }

    public void RespawnEnemies()
    {
        ClearAllEnemies();
        
        // Reset spawn flags
        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            spawnPoint.hasSpawned = false;
        }
        
        SpawnAllEnemies();
    }

    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
    }

    public int GetAliveEnemyCount()
    {
        int count = 0;
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                if (health != null && !health.IsDead())
                {
                    count++;
                }
            }
        }
        return count;
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || spawnPoints == null) return;

        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.location == null) continue;

            Vector3 treasurePos = spawnPoint.location.position;

            // Draw treasure marker
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(treasurePos, 0.5f);
            Gizmos.DrawLine(treasurePos, treasurePos + Vector3.up * 2f);

            // Draw spawn radius
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            DrawCircle(treasurePos, minDistanceFromTreasure, 32);
            DrawCircle(treasurePos, maxDistanceFromTreasure, 32);

            // Draw spawn range area
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.1f);
            for (float angle = 0; angle < 360; angle += 30)
            {
                float rad = angle * Mathf.Deg2Rad;
                Vector3 inner = treasurePos + new Vector3(
                    Mathf.Cos(rad) * minDistanceFromTreasure,
                    0.1f,
                    Mathf.Sin(rad) * minDistanceFromTreasure
                );
                Vector3 outer = treasurePos + new Vector3(
                    Mathf.Cos(rad) * maxDistanceFromTreasure,
                    0.1f,
                    Mathf.Sin(rad) * maxDistanceFromTreasure
                );
                Gizmos.DrawLine(inner, outer);
            }

            // Draw enemy count label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(
                treasurePos + Vector3.up * 2.5f,
                $"Enemies: {spawnPoint.minEnemies}-{spawnPoint.maxEnemies}\n" +
                $"Status: {(spawnPoint.hasSpawned ? "Spawned" : "Ready")}"
            );
            #endif
        }
    }

    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0.1f,
                Mathf.Sin(angle) * radius
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}

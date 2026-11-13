using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// -------------------------------------------------------
// EnemySpawner  —  MonoBehaviour
//
// CHANGES FROM PREVIOUS VERSION (spawn point section only):
//   - SpawnPoint now stores an assignedConfig (set during map scan)
//   - ScanForSpawnPoints loops through all configs and matches tiles
//     instead of checking a single enemySpawnTile
//   - SpawnEnemyAt uses sp.assignedConfig directly instead of PickPool()
//   - PickPool() still exists as fallback for untagged spawn points
//   - enemySpawnTile field removed (each config now has its own tile)
//
// REQUIRES in scene:
//   - A Tilemap assigned to the eventmap field
//   - EnemySpawnConfig assets — one per enemy type, each with a spawnTile set
//   - Enemy prefabs referenced by those configs must have Enemyv2 on them
//   - A GameObject tagged "Player" in the scene
//   - MapGeneration must call NotifyMapReady() after initMap() finishes
// -------------------------------------------------------

public class EnemySpawner : MonoBehaviour
{
    // -------------------------------------------------------
    // INSPECTOR SETTINGS

    [Header("Tilemap Reference")]
    [Tooltip("The eventmap Tilemap that MapGeneration writes spawn tiles onto.")]
    [SerializeField] private Tilemap eventmap;

    // NOTE: enemySpawnTile removed — each EnemySpawnConfig now holds its own tile.

    [Header("Enemy Configs")]
    [Tooltip("One EnemySpawnConfig asset per enemy type. " +
             "Each config has a spawnTile that links it to a specific tile on the map. " +
             "At least one config is required.")]
    [SerializeField] private List<EnemySpawnConfig> enemyConfigs = new List<EnemySpawnConfig>();

    [Header("Distance Settings")]
    [SerializeField] private float spawnDistanceMultiplier  = 1.5f;
    [SerializeField] private float despawnDistanceMultiplier = 2.5f;
    [SerializeField] private float safeDistanceMultiplier = 0.8f;
    [SerializeField] private float baseAggroRange = 10f;
    [SerializeField] private float comradeRange = 10f; // Spawns enemies grouped together

    [Header("Poll Rate")]
    [SerializeField] private float pollInterval = 0.3f;

    [Header("Respawn Settings")]
    [SerializeField] private bool allowRespawn = true;

    [Header("Testing")]
    [SerializeField] private bool autoStartWithoutMapGeneration = false;

    // -------------------------------------------------------
    // PRIVATE STATE

    // Represents one spawn point found on the map.
    // assignedConfig is set during ScanForSpawnPoints — it records
    // which enemy type this tile position should always spawn.
    private class SpawnPoint
    {
        public Vector3          worldPosition;   // world-space center of this tile
        public Enemyv2          activeEnemy;     // the live enemy here, or null
        public bool             hasEverSpawned;  // has this point spawned at least once?
        public EnemyPool        assignedPool;    // pool to return to on despawn
        public EnemySpawnConfig assignedConfig;  // which config/enemy type this tile maps to
        public List<SpawnPoint> comrades; // Which spawn points are nearby
    }

    private List<SpawnPoint>    spawnPoints = new List<SpawnPoint>();
    private List<EnemyPool>     pools       = new List<EnemyPool>(); // one pool per config

    // A dictionary for fast tile → config lookup during scanning
    private Dictionary<TileBase, EnemySpawnConfig> tileToConfig
        = new Dictionary<TileBase, EnemySpawnConfig>();

    // A dictionary for fast config → pool lookup during spawning
    private Dictionary<EnemySpawnConfig, EnemyPool> configToPool
        = new Dictionary<EnemySpawnConfig, EnemyPool>();

    private Transform playerTransform;
    private bool      mapReady = false;

    private float spawnDistanceSq;
    private float despawnDistanceSq;
    private float safeDistanceSq;

    // -------------------------------------------------------
    // AWAKE

    private void Awake()
    {
        float spawnDist   = baseAggroRange * spawnDistanceMultiplier;
        float despawnDist = baseAggroRange * despawnDistanceMultiplier;
        float safeDist = baseAggroRange * safeDistanceMultiplier;
        spawnDistanceSq   = spawnDist   * spawnDist;
        despawnDistanceSq = despawnDist * despawnDist;
        safeDistanceSq = safeDist * safeDist;
    }

    // -------------------------------------------------------
    // START

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogWarning("EnemySpawner: No GameObject tagged 'Player' found!");

        if (enemyConfigs.Count == 0)
        {
            Debug.LogError("EnemySpawner: No EnemySpawnConfigs assigned!");
            return;
        }

        // Build pool and lookup dictionaries from configs
        foreach (EnemySpawnConfig config in enemyConfigs)
        {
            EnemyPool pool = EnemyPool.CreatePool(config, this.transform);
            pools.Add(pool);
            configToPool[config] = pool;

            // Only add to tile lookup if a tile is assigned
            if (config.spawnTile != null)
            {
                if (!tileToConfig.ContainsKey(config.spawnTile))
                    tileToConfig[config.spawnTile] = config;
                else
                    Debug.LogWarning($"EnemySpawner: Two configs share the same spawnTile " +
                                     $"({config.spawnTile.name}). Only the first will be used.");
            }
        }

        if (autoStartWithoutMapGeneration)
            NotifyMapReady();
    }

    // -------------------------------------------------------
    // NOTIFY MAP READY — called by MapGeneration after initMap()

    public void NotifyMapReady()
    {
        ScanForSpawnPoints();
        StartCoroutine(PollRoutine());
    }

    // -------------------------------------------------------
    // SCAN FOR SPAWN POINTS
    //
    // CHANGED: instead of matching one enemySpawnTile, we check every tile
    // against the tileToConfig dictionary. Each matching cell gets a SpawnPoint
    // with assignedConfig set to whichever config owns that tile type.

    private void ScanForSpawnPoints()
    {
        spawnPoints.Clear();

        if (eventmap == null)
        {
            Debug.LogError("EnemySpawner: eventmap Tilemap not assigned!");
            return;
        }

        if (tileToConfig.Count == 0)
        {
            Debug.LogError("EnemySpawner: No configs have a spawnTile assigned! " +
                           "Set the spawnTile field on each EnemySpawnConfig asset.");
            return;
        }

        eventmap.CompressBounds();
        BoundsInt bounds = eventmap.cellBounds;

        int count = 0;

        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            TileBase tile = eventmap.GetTile(cellPos);
            if (tile == null) continue;

            // Check if this tile matches any config
            if (!tileToConfig.TryGetValue(tile, out EnemySpawnConfig matchedConfig))
                continue; // tile exists but isn't a recognised enemy spawn tile

            Vector3 worldPos = new Vector3(
                cellPos.x * eventmap.cellSize.x,
                cellPos.y * eventmap.cellSize.y,
                cellPos.z * eventmap.cellSize.z);

            spawnPoints.Add(new SpawnPoint
            {
                worldPosition  = worldPos,
                activeEnemy    = null,
                hasEverSpawned = false,
                assignedPool   = null,
                assignedConfig = matchedConfig  // tile determines enemy type
            });

            count++;
        }

        float checkRange = comradeRange*comradeRange;
        foreach (SpawnPoint baseSP in spawnPoints)
        {
            baseSP.comrades = new();
            foreach (SpawnPoint sp in spawnPoints)
            {
                if (sp==baseSP) continue;
                if (Vector2.SqrMagnitude(baseSP.worldPosition-sp.worldPosition)<=checkRange)
                {
                    baseSP.comrades.Add(sp);
                }
            }
        }

        Debug.Log($"EnemySpawner: Found {count} enemy spawn points on eventmap.");
    }

    // -------------------------------------------------------
    // POLL ROUTINE — unchanged logic, same two-pass approach

    private IEnumerator PollRoutine()
    {
        yield return null;

        while (true)
        {
            if (playerTransform != null)
            {
                Vector3 playerPos = playerTransform.position;

                // PASS 1: DESPAWN
                foreach (SpawnPoint sp in spawnPoints)
                {
                    if (sp.activeEnemy == null) continue;

                    float distSq = (sp.activeEnemy.transform.position - playerPos).sqrMagnitude;
                    if (distSq > despawnDistanceSq)
                        ReturnEnemyToPool(sp);
                }

                // PASS 2: SPAWN
                foreach (SpawnPoint sp in spawnPoints)
                {
                    if (sp.activeEnemy != null) continue;
                    if (!allowRespawn && sp.hasEverSpawned) continue;

                    float distSq = (sp.worldPosition - playerPos).sqrMagnitude;
                    if (distSq <= spawnDistanceSq)
                    {
                        if (distSq <= safeDistanceSq) continue;
                        SpawnEnemyAt(sp);
                        foreach (SpawnPoint comrade in sp.comrades)
                        {
                            float cdistSq = (comrade.worldPosition - playerPos).sqrMagnitude;
                            if (sp.activeEnemy == null && cdistSq>safeDistanceSq && cdistSq<despawnDistanceSq) SpawnEnemyAt(comrade);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(pollInterval);
        }
    }

    // -------------------------------------------------------
    // SPAWN ENEMY AT SPAWN POINT
    //
    // CHANGED: uses sp.assignedConfig directly to get the right pool,
    // instead of PickPool() (random). The tile already decided the type.

    private void SpawnEnemyAt(SpawnPoint sp)
    {
        // Get the pool that belongs to this spawn point's config
        EnemyPool pool = null;

        if (sp.assignedConfig != null && configToPool.TryGetValue(sp.assignedConfig, out EnemyPool configPool))
        {
            pool = configPool;
        }
        else
        {
            // Fallback: no config assigned (shouldn't happen after scan, but just in case)
            pool = PickPool();
        }

        if (pool == null) return;

        Enemyv2 enemy = pool.Get(sp.worldPosition);
        if (enemy == null) return;

        enemy.SetSpawnPoint(sp.worldPosition, this);

        sp.activeEnemy    = enemy;
        sp.hasEverSpawned = true;
        sp.assignedPool   = pool;
    }

    // -------------------------------------------------------
    // RETURN ENEMY TO POOL — unchanged

    private void ReturnEnemyToPool(SpawnPoint sp)
    {
        if (sp.assignedPool == null || sp.activeEnemy == null) return;

        sp.assignedPool.Return(sp.activeEnemy);
        sp.activeEnemy = null;
        sp.assignedPool = null;
        sp.hasEverSpawned = false;
    }

    // -------------------------------------------------------
    // NOTIFY ENEMY DIED — unchanged

    public void NotifyEnemyDied(Enemyv2 enemy)
    {
        foreach (SpawnPoint sp in spawnPoints)
        {
            if (sp.activeEnemy == enemy)
            {
                ReturnEnemyToPool(sp);
                return;
            }
        }
    }

    // -------------------------------------------------------
    // PICK POOL — fallback weighted random, unchanged
    // Only used when a spawn point has no assignedConfig.

    private EnemyPool PickPool()
    {
        if (pools.Count == 0) return null;
        if (pools.Count == 1) return pools[0];

        float totalWeight = 0f;
        foreach (EnemySpawnConfig cfg in enemyConfigs)
            totalWeight += cfg.spawnWeight;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < enemyConfigs.Count; i++)
        {
            cumulative += enemyConfigs[i].spawnWeight;
            if (roll < cumulative)
                return pools[i];
        }

        return pools[pools.Count - 1];
    }

    // -------------------------------------------------------
    // RESET — unchanged

    public void ResetSpawner()
    {
        foreach (SpawnPoint sp in spawnPoints)
        {
            if (sp.activeEnemy != null)
                ReturnEnemyToPool(sp);
        }

        spawnPoints.Clear();
        StopAllCoroutines();
        mapReady = false;
    }

    // -------------------------------------------------------
    // GIZMOS — unchanged

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (playerTransform == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(playerTransform.position, baseAggroRange * spawnDistanceMultiplier);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerTransform.position, baseAggroRange * despawnDistanceMultiplier);

        Gizmos.color = Color.cyan;
        foreach (SpawnPoint sp in spawnPoints)
            Gizmos.DrawSphere(sp.worldPosition, 0.2f);
    }
}

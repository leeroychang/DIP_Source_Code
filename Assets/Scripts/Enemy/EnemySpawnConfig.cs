using UnityEngine;
using UnityEngine.Tilemaps;

// -------------------------------------------------------
// EnemySpawnConfig  —  ScriptableObject
//
// PURPOSE:
//   Holds the data that defines one enemy TYPE for the spawner.
//   One asset per enemy type (e.g. "FireSlimeConfig", "IceSlimeConfig").
//   The EnemySpawner holds a List<EnemySpawnConfig> — one entry per type.
//
// HOW TO CREATE A NEW CONFIG ASSET:
//   Right-click in Project window → Create → Enemy → Spawn Config
//   Fill in the fields in the Inspector.
//
// TILE LINKING:
//   Set spawnTile to the specific tile asset placed on the eventmap
//   for this enemy type. The spawner reads which tile is at each
//   spawn cell and picks the matching config automatically.
//   Leave spawnTile null to use weight-based random selection instead.
// -------------------------------------------------------

[CreateAssetMenu(fileName = "NewEnemySpawnConfig", menuName = "Enemy/Spawn Config")]
public class EnemySpawnConfig : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("The EntityData asset for this enemy type. " +
             "Defines HP, ATK, DEF, SPD, XP and base skills.")]
    public EntityData entityData;
    public float attackRange;

    [Header("Prefab")]
    [Tooltip("The enemy prefab to instantiate/pool for this type. " +
             "Must have an Enemyv2 component on it.")]
    public GameObject prefab;

    [Header("Pool Settings")]
    [Tooltip("How many instances to pre-warm at startup. " +
             "Set to the max number of this type you expect on screen at once.")]
    public int poolSize = 10;

    [Header("Spawn Tile")]
    [Tooltip("The specific tile asset on the eventmap that exclusively spawns this enemy type. " +
             "Drag the tile asset (e.g. FireSlimeSpawn.asset) here. " +
             "When the spawner finds this tile on the map it will always use this config. " +
             "Leave empty to fall back to weight-based random selection.")]
    public TileBase spawnTile;

    [Header("Spawn Weighting (fallback)")]
    [Tooltip("Only used if spawnTile is empty. " +
             "Relative chance this type is chosen at an untagged spawn point. " +
             "Higher = spawns more often. Example: Fire=70, Ice=30.")]
    [Range(0f, 100f)]

    public float spawnWeight = 100f;
}

using System.Collections.Generic;
using UnityEngine;

// -------------------------------------------------------
// EnemyPool  —  MonoBehaviour (one per enemy TYPE)
//
// PURPOSE:
//   Manages a fixed pool of pre-created enemy GameObjects for one
//   enemy type. Instead of Instantiate/Destroy every spawn, we
//   reuse sleeping instances — avoids GC spikes and frame hitches.
//
// USAGE:
//   Created and owned by EnemySpawner at runtime — you do NOT
//   add this script manually in the Inspector.
//   EnemySpawner calls:
//       EnemyPool pool = EnemyPool.CreatePool(config, parent);
//       Enemyv2 enemy  = pool.Get(position);   // wake up an instance
//       pool.Return(enemy);                     // put it back to sleep
//
// HOW IT WORKS:
//   1. On creation, Instantiate poolSize copies, disable them, store in stack.
//   2. Get()   — pops from stack, moves to position, calls OnSpawn(), enables.
//   3. Return() — calls OnDespawn(), disables, pushes back onto stack.
//   4. If stack is empty on Get(), a new instance is created (pool auto-expands).
// -------------------------------------------------------

public class EnemyPool : MonoBehaviour
{
    // -------------------------------------------------------
    // PRIVATE STATE

    private EnemySpawnConfig config;                // which enemy type this pool serves
    private Stack<Enemyv2>   available = new Stack<Enemyv2>(); // sleeping instances
    private Transform        poolParent;            // scene hierarchy parent to keep things tidy

    // -------------------------------------------------------
    // FACTORY — called by EnemySpawner to create a pool
    //
    // Static factory so EnemySpawner doesn't need to AddComponent
    // and then configure separately — one call does both.

    public static EnemyPool CreatePool(EnemySpawnConfig spawnConfig, Transform parent)
    {
        // Create a dedicated GameObject for this pool's hierarchy
        GameObject poolObj = new GameObject("Pool_" + spawnConfig.entityData.name);
        poolObj.transform.SetParent(parent);

        EnemyPool pool   = poolObj.AddComponent<EnemyPool>();
        pool.config      = spawnConfig;
        pool.poolParent  = poolObj.transform;

        pool.Prewarm(spawnConfig.poolSize);

        return pool;
    }

    // -------------------------------------------------------
    // PREWARM — instantiate all instances up front at startup
    //
    // Done once so there are no Instantiate calls mid-gameplay.
    // All instances start disabled (sleeping in the pool).

    private void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Enemyv2 instance = CreateNewInstance();
            available.Push(instance); // push onto available stack
        }
    }

    // -------------------------------------------------------
    // CREATE NEW INSTANCE — used by Prewarm and auto-expand
    //
    // Instantiates, parents to pool hierarchy, disables immediately.
    // CHANGED: disable NavMeshAgent before activating so NavMesh
    // doesn't complain about being instantiated off the mesh at (0,0).

    private Enemyv2 CreateNewInstance()
    {
        // Instantiate at a far-off position so NavMeshAgent doesn't
        // complain about being outside the NavMesh during prewarm.
        // The real position is set in Get() before the agent is used.
        //Vector3 offMapPos = new Vector3(-9999f, -9999f, 0f);
        GameObject obj = Instantiate(config.prefab, Vector3.zero, Quaternion.identity, poolParent);

        obj.SetActive(false); // sleeping — invisible, no Update, no agent activity

        Enemyv2 enemy = obj.GetComponent<Enemyv2>();

        if (enemy == null)
            Debug.LogError("EnemyPool: Prefab '" + config.prefab.name +
                           "' has no Enemyv2 component!");

        enemy.attackRange = config.attackRange;

        return enemy;
    }

    // -------------------------------------------------------
    // GET — pull a sleeping enemy from the pool, place it, wake it up
    //
    // Parameters:
    //   position — world position to place the enemy at spawn
    //
    // Returns the Enemyv2 component of the activated enemy.
    // If the pool is empty, creates a new instance (auto-expand).

    public Enemyv2 Get(Vector3 position)
    {
        // If all instances are in use, auto-expand the pool
        if (available.Count == 0)
        {
            Debug.LogWarning("EnemyPool [" + config.entityData.name + "]: Pool empty — " +
                             "auto-expanding. Consider increasing poolSize.");
            available.Push(CreateNewInstance());
        }

        Enemyv2 enemy = available.Pop(); // take from stack

        enemy.transform.position = position; // place at spawn point before waking up
        enemy.gameObject.SetActive(true);    // wake up — enables Update, physics, etc.
        enemy.OnSpawn(config.entityData);    // reset HP, state, re-find player, etc.

        return enemy;
    }

    // -------------------------------------------------------
    // RETURN — put a used enemy back to sleep in the pool
    //
    // Called by EnemySpawner when despawn distance exceeded,
    // OR by Enemyv2 itself when the enemy dies.

    public void Return(Enemyv2 enemy)
    {
        if (enemy == null) return;

        enemy.OnDespawn();                    // clean up state before sleeping
        enemy.gameObject.SetActive(false);    // sleep — no Update, invisible
        enemy.transform.SetParent(poolParent); // re-parent in case it moved
        available.Push(enemy);               // back on the stack
    }

    // -------------------------------------------------------
    // HOW MANY ARE CURRENTLY IN USE (for debug/UI)

    public int ActiveCount => config.poolSize - available.Count;
}

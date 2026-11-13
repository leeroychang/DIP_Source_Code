using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;  // NavMesh

/// STATES:
///   PATROL  — enemy wanders randomly (NavMesh steers around walls)
///   CHASE   — player spotted via line-of-sight raycast, NavMesh paths toward them
///   ATTACK  — player in range, stands still and deals damage (melee) or fires (ranged)
///   STUN    — enemy is frozen: no movement, no attack, no state transitions
///   DEAD    — no HP left, gets destroyed
///
/// REQUIRES on this GameObject:
///   - Rigidbody2D  (Body Type: Dynamic, Gravity Scale: 0, Freeze Rotation Z)
///   - Collider2D   (BoxCollider2D or CircleCollider2D)
///   - NavMeshAgent component (added via NavMeshPlus package)
///     ^ Set: Steering > Speed = 0 (we drive it manually), Angular Speed = 0, Stopping Distance = 0
///     ^ Set: Agent Type = "Humanoid" or whichever you baked
///
/// REQUIRES in scene:
///   - NavMesh baked over the walkable floor (Window > AI > Navigation, bake with NavMeshPlus for 2D)
///   - A GameObject tagged "Player" with an Entity-derived component (e.g. PlayerController)
///   - Walls on their own Physics Layer so raycasts can filter them
///   - Enemies on their own Physics Layer (for peer separation), assigned to enemyLayer below

public class Enemyv2 : Entity
{
    // -------------------------------------------------------
    // STATES

    private enum EnemyState { Patrol, Chase, Attack, Stun, Dead }
    private EnemyState currentState = EnemyState.Patrol;

    // -------------------------------------------------------
    // POOL / SPAWNER REFERENCES

    private EnemySpawner ownerSpawner;
    private Vector3      mySpawnPoint;

    // -------------------------------------------------------
    // INSPECTOR SETTINGS

    [Header("Health")]
    public int maxHealth = 50;

    [Header("Movement")]
    public float patrolSpeed        = 1f;
    public float chaseSpeed         = 2.5f;
    public float patrolChangeTime   = 3f;
    public float patrolWanderRadius = 4f;

    [Header("Range")]
    public float aggroRange  = 5f;
    public float attackRange = 1f;

    [Header("Line of Sight")]
    public LayerMask wallLayer;

    [Header("Attack")]
    public float attackCooldown = 1f;

    [Header("Skills")]
    public float skillTriggerChance = 1f;

    [Header("Separation (crowd-around)")]
    // Set to whichever Physics Layer your enemies are on, so peers push
    // each other apart instead of stacking on top of the player.
    public LayerMask enemyLayer;
    public float separationRadius = 1.2f;
    public float separationForce  = 2f;

    [Header("Debug (Scene View)")]
    public bool showGizmos = true;

    // -------------------------------------------------------
    // PRIVATE VARIABLES

    private NavMeshAgent agent;
    private Transform    target;

    // Derived stopping distance used by NavMeshAgent.
    // Calculated once in Awake/OnSpawn so we don't redo the math every frame.
    private float effectiveStoppingDist = 0f;

    // Patrol
    private float patrolTimer     = 0f;
    private bool  hasPatrolTarget = false;

    // Attack
    private float attackTimer = 0f;

    // Stun
    private float stunTimer = 0f;

    private bool isAttacking = false;

    // -------------------------------------------------------
    // SET SPAWN POINT — called by EnemySpawner immediately after Get()

    public void SetSpawnPoint(Vector3 spawnPosition, EnemySpawner spawner)
    {
        mySpawnPoint = spawnPosition;
        ownerSpawner = spawner;
    }

    // -------------------------------------------------------
    // ON SPAWN — called by EnemyPool.Get() when this enemy is activated

    public void OnSpawn(EntityData entityData)
    {
        Setup(entityData);

        currentState = EnemyState.Patrol;
        wallLayer = LayerMask.NameToLayer("Wall");

        attackTimer     = 0f;
        patrolTimer     = 0f;
        stunTimer       = 0f;
        hasPatrolTarget = false;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            target = playerObj.transform;
        else
            Debug.LogWarning(gameObject.name + ": No 'Player' tag found on OnSpawn.");

        if (agent != null)
        {
            agent.isStopped = false;
            agent.ResetPath();

            effectiveStoppingDist  = Mathf.Max(0f, attackRange - 0.4f);
            agent.stoppingDistance = effectiveStoppingDist;
        }

        PickNewPatrolTarget();
    }

    // -------------------------------------------------------
    // ON DESPAWN — called by EnemyPool.Return() before this enemy is disabled

    public void OnDespawn()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        ownerSpawner = null;
    }

    // -------------------------------------------------------
    // AWAKE — grab components

    private void Awake()
    {
        rb    = GetComponent<Rigidbody2D>();
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis   = false;

            // Safe default — will be overwritten correctly in OnSpawn().
            // Needed in case the enemy is placed manually in scene (no OnSpawn call).
            effectiveStoppingDist  = Mathf.Max(0f, attackRange - 0.4f);
            agent.stoppingDistance = effectiveStoppingDist;
        }
        else
        {
            Debug.LogWarning(gameObject.name + ": No NavMeshAgent found! Add one via NavMeshPlus.");
        }
    }

    // -------------------------------------------------------
    // UPDATE — state decisions every frame

    void Update()
    {
        if (currentState == EnemyState.Dead) return;

        // STUN — do nothing, just count down then wake back up into patrol
        if (currentState == EnemyState.Stun)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
                currentState = EnemyState.Patrol;
            return;
        }

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (target == null) return;

        float dist = Vector2.Distance(transform.position, target.position);

        // --- STATE TRANSITIONS ---

        if (aniLock)
        {
            if (isAttacking) rb.linearVelocity = Vector2.zero;
            return;
        }

        if (dist <= attackRange && HasLineOfSight())
        {
            currentState = EnemyState.Attack;
        }
        else if (dist <= aggroRange)
        {
            if (currentState!=EnemyState.Chase && currentState!=EnemyState.Patrol) TriggerAnimation("Move");
            currentState = EnemyState.Chase;
        }
        else if (currentState == EnemyState.Chase && dist > aggroRange)
        {
            currentState = EnemyState.Patrol;
            PickNewPatrolTarget();
        }
        else if (currentState != EnemyState.Chase)
        {
            if (currentState!=EnemyState.Chase && currentState!=EnemyState.Patrol) TriggerAnimation("Move");
            currentState = EnemyState.Patrol;
        }

        // --- STATE BEHAVIOUR ---
        switch (currentState)
        {
            case EnemyState.Patrol: HandlePatrol(); break;
            case EnemyState.Chase:  HandleChase();  break;
            case EnemyState.Attack: HandleAttack(); break;
        }
    }

    // -------------------------------------------------------
    // FIXED UPDATE — keep Rigidbody from fighting NavMeshAgent

    private void FixedUpdate()
    {
        if (currentState == EnemyState.Dead || currentState == EnemyState.Stun)
        {
            if (agent != null) agent.isStopped = true;
            // rb.linearVelocity = Vector2.zero;
            TickUpdate();
            return;
        }

        if (currentState == EnemyState.Chase || currentState == EnemyState.Patrol)
        {
            rb.linearVelocity = Vector2.zero;
        }

        TickUpdate();
    }

    // -------------------------------------------------------
    // APPLY STUN — call this from a skill, status effect, or trap
    // Enemy freezes for duration seconds then returns to Patrol

    public void ApplyStun(float duration)
    {
        if (currentState == EnemyState.Dead) return;

        currentState = EnemyState.Stun;
        stunTimer    = duration;

        if (agent != null) agent.isStopped = true;
        // if (rb    != null) rb.linearVelocity = Vector2.zero;
    }

    // -------------------------------------------------------
    // LINE OF SIGHT CHECK

    private bool HasLineOfSight()
    {
        if (target == null) return false;

        Vector2 origin    = transform.position;
        Vector2 direction = ((Vector2)target.position - origin).normalized;
        float   distance  = Vector2.Distance(origin, target.position);

        RaycastHit2D hit = Physics2D.CircleCast(origin, 0.3f, direction, distance, LayerMask.GetMask("Wall"));
        if (hit.collider!=null) agent.stoppingDistance = 1f;
        else agent.stoppingDistance = effectiveStoppingDist;

        return hit.collider == null;
    }

    // -------------------------------------------------------
    // STATE HANDLERS

    // PATROL: pick a random walkable point nearby via NavMesh, walk there, repeat
    private void HandlePatrol()
    {
        patrolTimer -= Time.deltaTime;

        if (agent == null) return;

        bool arrived = !agent.pathPending &&
                       agent.remainingDistance <= agent.stoppingDistance + 0.1f;

        if (patrolTimer <= 0f || arrived || !hasPatrolTarget)
        {
            PickNewPatrolTarget();
        }

        agent.isStopped = false;
        agent.speed     = patrolSpeed;
        AnimateDirection(agent.velocity.normalized);
    }

    // CHASE: path toward player. Peer separation spreads enemies out.
    private void HandleChase()
    {
        if (agent == null) return;

        ApplySeparation();

        agent.isStopped        = false;
        agent.speed            = stats[EntityData.Stat.SPD]/2f;
        // agent.stoppingDistance = effectiveStoppingDist;
        agent.SetDestination(target.position);
        AnimateDirection(agent.velocity.normalized);
    }

    // ATTACK: stand still and use skills on cooldown.
    private void HandleAttack()
    {
        if (agent != null) agent.isStopped = true;
        rb.linearVelocity = Vector2.zero;

        ApplySeparation();

        if (attackTimer > 0f) return;

        attackTimer = attackCooldown;

        UseSkills();
    }

    // -------------------------------------------------------
    // SKILLS

    private void UseSkills()
    {
        if (target == null) return;
        foreach (var pair in skills)
        {
            if (Random.value <= skillTriggerChance)
                if (pair.Value.OnUse(this, target.position))
                    isAttacking = true;
        }
    }

    // -------------------------------------------------------
    // PEER SEPARATION
    // Finds nearby enemies on enemyLayer and gently pushes this one away so
    // they crowd around the player rather than stacking on top of each other.

    private void ApplySeparation()
    {
        Collider2D[] peers = Physics2D.OverlapCircleAll(transform.position, separationRadius, enemyLayer);

        Vector2 pushDir = Vector2.zero;
        int     count   = 0;

        foreach (Collider2D peer in peers)
        {
            if (peer.gameObject == gameObject) continue;

            Vector2 away = (Vector2)transform.position - (Vector2)peer.transform.position;
            float   dist = away.magnitude;

            if (dist < 0.01f) dist = 0.01f;

            pushDir += away.normalized * (1f - dist / separationRadius);
            count++;
        }

        if (count > 0 && rb != null)
        {
            Vector2 nudge = pushDir.normalized * separationForce * Time.deltaTime;
            rb.MovePosition(rb.position + nudge);
        }
    }

    // -------------------------------------------------------
    // PATROL HELPER

    private void PickNewPatrolTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolWanderRadius;
        Vector3 randomPoint  = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, patrolWanderRadius, NavMesh.AllAreas))
        {
            hasPatrolTarget = true;
            if (agent != null)
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
            }
        }

        patrolTimer = patrolChangeTime;
    }

    // -------------------------------------------------------
    // DAMAGE — routed through Entity.Damaged(), extended for pool/state handling

    public override int Damaged(int val, int ignoreDef=0, float penpercent=0f)
    {
        int dmg = base.Damaged(val,ignoreDef,penpercent);

        if (stats[EntityData.Stat.HP] <= 0)
        {
            currentState = EnemyState.Dead;
        }

        return dmg;
    }

    public override void Knockback(float strength, float duration, Vector2 dir, bool interuptable = true, bool immune = false, bool triggerAni=true)
    {
        base.Knockback(strength, duration, dir, interuptable, immune);
        ApplyStun(duration);
    }

    public override void StartAnimation(float duration, bool interuptable = true, bool immune = false, float moveSpeed = 0, Vector2? moveDir = null)
    {
        isAttacking = false;
        base.StartAnimation(duration, interuptable, immune, moveSpeed, moveDir);
    }

    public override void resetAnimation(bool resetAgent = true)
    {
        base.resetAnimation(resetAgent);
        isAttacking = false;
    }

    // -------------------------------------------------------
    // DIE — updated for pool compatibility
    //
    // NOTE: base Entity.Damaged() already starts OnDown() (in Entity.cs) when
    // HP hits 0, which disables the sprite/collider and Destroys the gameObject
    // after a short delay. If you want pooled enemies to return to the pool
    // instead of being Destroyed, override OnDown() here to call
    // ownerSpawner.NotifyEnemyDied(this) instead of Destroy(gameObject).

    // private void Die()
    // {
    //     currentState = EnemyState.Dead;
    //     if (agent != null) agent.isStopped = true;
    //     rb.linearVelocity = Vector2.zero;
    //
    //     if (ownerSpawner != null)
    //         ownerSpawner.NotifyEnemyDied(this);
    //     else
    //         Destroy(gameObject);
    // }

    // -------------------------------------------------------
    // GIZMOS — draw ranges in Scene view

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // Yellow = aggro/sight range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        // Red = attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Line to player: cyan = can see, gray = wall blocking
        if (target != null)
        {
            Gizmos.color = HasLineOfSight() ? Color.cyan : Color.gray;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}

using System.Collections; //for couroutines e.g. invincibility timer
using System.Collections.Generic;
using UnityEngine;

/// STATES:
///   PATROL  — enemy wander before noticing the player
///   CHASE   — detected player, walks toward them
///   ATTACK  — player in melee range, enemy deals damage
/// *adjust attack later for diff types enemies
///   DEAD    — no HP left, gets destroyed
///
/// REQUIRES on this GameObject:
///   - Rigidbody2D  (Body Type: Dynamic, Gravity Scale: 0, Freeze Rotation Z)
///   - Collider2D   (BoxCollider2D or CircleCollider2D)
///
/// REQUIRES in scene:
///   - A GameObject tagged "Player" with a PlayerHealth component
/// *edit after seeing player code


public class Enemy : MonoBehaviour
{
    // STATE 
    private enum EnemyState { Patrol, Chase, Attack, Dead }
    private EnemyState currentState = EnemyState.Patrol; //begin wth patrol
    
    // SETTINGS

    [Header("Health")]
    public int maxHealth = 50;

    [Header("Movement")]
    public float patrolSpeed = 1f;          // Speed while wandering
    public float chaseSpeed  = 2.5f;        // Speed while chasing player
    public float patrolChangeTime = 2f;     // Seconds before picking a new patrol direction

    [Header("Range")]
    public float aggroRange  = 5f;          // Distance at which enemy notices player
    public float attackRange = 1f;          // Distance at which enemy starts attacking

    [Header("Attack")]
    public int   attackDamage    = 1;       // Damage per hit
    public float attackCooldown  = 1f;      // Seconds between attacks

    [Header("Debug (Scene View)")]
    public bool showGizmos = true;

    private int            currentHealth;
    private Rigidbody2D    rb;
    private Transform      target;
    private Vector2        moveDirection;

    // Patrol
    private float patrolTimer = 0f;

    // Attack
    private float attackTimer = 0f;

    // -------------------------------------------------------
    // AWAKE
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); //search game object for a rigidbody2d component, stores in rb
    }

    // -------------------------------------------------------
    // START
    
    void Start()
    {
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); //look for player
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Enemy: No GameObject tagged 'Player' found in scene!");
        }

        // Begin patrolling in a random direction
        PickNewPatrolDirection();
    }
//*check if player tag is Player
    // -------------------------------------------------------
    // UPDATE — logic & state decisions every frame

    void Update()
    {
        // Don't do anything if dead
        if (currentState == EnemyState.Dead) return;

        // Tick attack cooldown
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (target == null) return;

        float dist = Vector2.Distance(transform.position, target.position); 
        //^ straight line distance this frame from enemy to player

        // --- STATE TRANSITIONS ---
        if (dist <= attackRange)
        {
            currentState = EnemyState.Attack;
        }
        else if (dist <= aggroRange)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Patrol;
        }

        // --- STATE BEHAVIOUR ---
        switch (currentState)
        {
            case EnemyState.Patrol:
                HandlePatrol();
                break;

            case EnemyState.Chase:
                HandleChase();
                break;

            case EnemyState.Attack:
                HandleAttack();
                break;
        }
    }

    // -------------------------------------------------------
    // FIXED UPDATE — physics movement
    
    private void FixedUpdate()
    {
        if (currentState == EnemyState.Dead)
        {
            rb.linearVelocity = Vector2.zero; //stop completely (vector 0, 0)
            return;
        }

        if (currentState == EnemyState.Attack)
        {
            rb.linearVelocity = Vector2.zero; // stop while attacking
        }
        else if (currentState == EnemyState.Chase)
        {
            rb.linearVelocity = moveDirection * chaseSpeed;
        }
        else // Patrol
        {
            rb.linearVelocity = moveDirection * patrolSpeed;
        }
    }

    // -------------------------------------------------------
    // STATE HANDLERS

    // PATROL: wander in a random direction, change direction every few seconds
    private void HandlePatrol()
    {
        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0f)
        {
            PickNewPatrolDirection();
        }
    }

    // CHASE: calculate direction toward player each frame
    private void HandleChase()
    {
        Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized; //vector from enemy to player
        moveDirection = dir;
    }

    // ATTACK: stand still and deal damage on cooldown
    private void HandleAttack()
    {
        moveDirection = Vector2.zero; // stop moving while attacking
        if (attackTimer <= 0f)
        {
            DealDamage();
            attackTimer = attackCooldown;
        }
    }

    // -------------------------------------------------------
    // DAMAGE — enemy hits the player
    //*assuming player health is PLayerHealth

    private void DealDamage()
    {
        // if (target == null) return;

        // PlayerHealth ph = target.GetComponent<PlayerHealth>();
        // if (ph != null)
        // {
        //     ph.TakeDamage(attackDamage);
        //     Debug.Log("Enemy attacked player for " + attackDamage + " damage.");
        // }
        // else
        // {
        //     Debug.LogWarning("Enemy: Player has no PlayerHealth component!");
        // }
    }

    // -------------------------------------------------------
    // TAKE DAMAGE — called by weapons/projectiles

    public void TakeDamage(int damage)
    {
        if (currentState == EnemyState.Dead) return;

        currentHealth -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage. HP: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // -------------------------------------------------------
    // DIE

    private void Die()
    {
        currentState = EnemyState.Dead;
        rb.linearVelocity = Vector2.zero;
        Debug.Log(gameObject.name + " died!");

        // TODO: play death animation / sound / drop loot here
        Destroy(gameObject); // Destroy(gameObject, 0.1f); if want slight delay so death animation can start
    }

    // -------------------------------------------------------
    // PATROL HELPER

    private void PickNewPatrolDirection()
    {
        // Pick a random angle and convert to a direction vector
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        patrolTimer = patrolChangeTime;
    }

    // -------------------------------------------------------
    // GIZMOS — draw ranges in Scene view (editor only) 
    //for our own displays in unity, not in actual game

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // Yellow = aggro range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        // Red = attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

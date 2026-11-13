using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// -------------------------------------------------------
// TentacleController
//
// REQUIRES on this GameObject:
//   - Rigidbody2D  (Body Type: Dynamic, Gravity Scale: 0, Freeze Rotation Z)
//   - CircleCollider2D or BoxCollider2D
//   - NavMeshAgent (NavMeshPlus, updateRotation: false, updateUpAxis: false)
//   - EntityData asset assigned to tentacleData in Inspector
//
// SETUP:
//   1. Create EntityData asset (Tentacle_Data) — HP 500, DEF 0, SPD moderate
//      Add SkillData for BossSwipe (skill slot 0) and BossSlam (skill slot 1)
//      to its baseSkills list
//   2. Assign tentacleData in Inspector
//   3. Assign boss reference in Inspector
//   4. Place this GameObject in the scene at its starting edge position
//   5. Tag player GameObject as "Player"
// -------------------------------------------------------

public class TentacleController : Entity
{
    // -------------------------------------------------------
    // INSPECTOR SETTINGS

    [Header("Data")]
    [SerializeField] EntityData tentacleData;
    [SerializeField] Sprite stunSprite;
    [SerializeField] Sprite defSprite;

    [Header("Boss Reference")]
    [SerializeField] BossController boss;
    [SerializeField] SpriteRenderer stateRender;

    [Header("Movement")]
    [SerializeField] float chaseSpeed  = 2.5f;
    [SerializeField] float aggroRange  = 8f;
    [SerializeField] float attackRange = 1.5f;

    [Header("Skill Timers")]
    [SerializeField] float swipeCooldown = 5f;    // skill slot 0 — BossSwipe
    [SerializeField] float slamCooldown  = 7f;    // skill slot 1 — BossSlam

    [Header("Stun")]
    [SerializeField] float stunDuration = 10f;
    [SerializeField] int   damageOnStun = 500;

    [Header("Debug")]
    [SerializeField] bool showGizmos = true;

    // -------------------------------------------------------
    // PRIVATE

    NavMeshAgent agent;
    Transform    player;

    float swipeTimer = 0f;
    float slamTimer  = 0f;

    bool isStunned = false;
    bool isEnraged = false;

    SpriteRenderer sr;
    Collider2D     coll2D;

    // -------------------------------------------------------
    // START

    void Start()
    {
        if (stateRender!=null) stateRender.enabled = false;
        Setup(tentacleData);

        sr   = GetComponentInChildren<SpriteRenderer>();
        coll2D = GetComponent<Collider2D>();

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis   = false;
            agent.speed          = chaseSpeed;
        }
        else
        {
            Debug.LogWarning("TentacleController: No NavMeshAgent found.");
        }

        if (rb != null)
        {
            rb.gravityScale = 0f;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("TentacleController: No GameObject tagged 'Player' found.");

        // Stagger timers so swipe and slam don't fire at the same time on start
        swipeTimer = swipeCooldown * 0.5f;
        slamTimer  = slamCooldown;

        interuptImmune = true;
    }

    // -------------------------------------------------------
    // UPDATE

    void Update()
    {
        if (isStunned || IsDown() || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // Movement — chase player if outside attack range
        if (dist > attackRange)
        {
            if (agent != null)
            {
                agent.isStopped = false;
                agent.speed     = chaseSpeed;
                agent.SetDestination(player.position);
            }
        }
        else
        {
            if (agent != null) agent.isStopped = true;
            if (rb    != null) rb.linearVelocity = Vector2.zero;
        }

        // Skill timers — fire independently of each other
        swipeTimer -= Time.deltaTime;
        slamTimer  -= Time.deltaTime;

        if (swipeTimer <= 0f)
        {
            FireSkill(0);
            swipeTimer = isEnraged ? swipeCooldown * 0.6f : swipeCooldown;
        }

        if (slamTimer <= 0f)
        {
            FireSkill(1);
            slamTimer = isEnraged ? slamCooldown * 0.6f : slamCooldown;
        }
    }

    // -------------------------------------------------------
    // FIXED UPDATE

    void FixedUpdate()
    {
        if (isStunned || IsDown())
        {
            if (agent != null) agent.isStopped = true;
            if (rb    != null) rb.linearVelocity = Vector2.zero;
        }

        TickUpdate();
    }

    public override void resetAnimation(bool resetAgent = true)
    {
        base.resetAnimation(resetAgent);
        interuptImmune = true;
    }

    // -------------------------------------------------------
    // FIRE SKILL

    void FireSkill(int slot)
    {
        if (!skills.ContainsKey(slot)) return;
        if (player == null) return;
        skills[slot].OnUse(this, player.position);
    }

    // -------------------------------------------------------
    // ENRAGE — called by BossController

    public void SetEnraged(bool enraged)
    {
        isEnraged = enraged;
        if (isEnraged && agent != null)
            agent.speed = chaseSpeed * 1.3f;
    }

    // -------------------------------------------------------
    // TAKE DAMAGE — at 0 HP, stun instead of dying

    public override int Damaged(int val, int ignoreDef=0, float penpercent=0f)
    {
        if (isStunned) return 0; // immune while stunned
        if (dmgImmune) return 0;

        if (stats[EntityData.Stat.HP]<=0) return 0;
        Hurt();
        int dmg = (int) (val*DMGReduction(ignoreDef,penpercent));
        if (dmg<=0) dmg = 1;
        stats[EntityData.Stat.HP] -= dmg;

        if (stats[EntityData.Stat.HP]<=0)
        {
            stats[EntityData.Stat.HP] = 0;
            //Insert on Stun method
            StartCoroutine(StunRoutine());

        }
        //Insert UpdateHealth method
        UpdateHealth();
        if (UIManager.Instance!=null) UIManager.Instance.DMG(dmg,transform.position,ally);
        //Insert DmgPopUp method
        return dmg;
    }

    // -------------------------------------------------------
    // STUN ROUTINE
    // Disables tentacle for stunDuration, then deals damage to boss and recovers

    IEnumerator StunRoutine()
    {
        isStunned = true;
        dmgImmune = true;

        // Stop movement
        if (agent != null) agent.isStopped = true;
        if (rb    != null) rb.linearVelocity = Vector2.zero;

        // Visual — gray out
        if (sr != null) sr.color = Color.gray;

        if (stateRender!=null)
        {
            stateRender.sprite = stunSprite;
            stateRender.enabled = true;
        }

        // Disable collision so player can't hit it while stunned
        if (coll2D != null) coll2D.enabled = false;

        // Deal damage to boss
        if (boss != null)
            boss.Damaged(damageOnStun,0,1f);
        else
            Debug.LogWarning("TentacleController: No boss reference assigned — skipping boss damage.");

        yield return new WaitForSeconds(stunDuration);

        // Recover — restore HP
        // stats[EntityData.Stat.HP]    = stats[EntityData.Stat.MaxHP];
        // stats[EntityData.Stat.MaxHP] = tentacleData.GetStats()[EntityData.Stat.MaxHP];
        Heal(stats[EntityData.Stat.MaxHP]);

        // Re-enable
        if (coll2D != null) coll2D.enabled = true;
        if (sr   != null) sr.color = Color.white;

        isStunned = false;

        // Restart stagger so skills don't all fire the instant it wakes
        swipeTimer = swipeCooldown * 0.3f;
        slamTimer  = slamCooldown  * 0.6f;

        if (stateRender!=null)
        {
            stateRender.sprite = defSprite;
            stateRender.enabled = true;
        }

        yield return new WaitForSeconds(5f);

        if (stateRender!=null) stateRender.enabled = false;
        dmgImmune = false;
    }

    // -------------------------------------------------------
    // GIZMOS

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public override void Flip()
    {
        isFlipped = !isFlipped;
    }
}

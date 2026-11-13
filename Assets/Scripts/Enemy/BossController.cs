using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -------------------------------------------------------
// BossController
//
// REQUIRES on this GameObject:
//   - Rigidbody2D  (Body Type: Dynamic, Gravity Scale: 0, Freeze Position X/Y, Freeze Rotation Z)
//   - CircleCollider2D or BoxCollider2D
//   - EntityData asset assigned to bossData in Inspector
//
// SETUP:
//   1. Create EntityData asset (BossMain_Data) — HP 5000, DEF 0, SPD 0
//      Add SkillData for the boss circle skill to its baseSkills list
//   2. Assign bossData in Inspector
//   3. Assign both TentacleControllers in Inspector
//   4. Tag player GameObject as "Player"
// -------------------------------------------------------

public class BossController : Entity
{
    // -------------------------------------------------------
    // INSPECTOR SETTINGS

    [Header("Data")]
    [SerializeField] EntityData bossData;
    [SerializeField] AudioClip enragedBGM;

    [Header("Tentacles")]
    [SerializeField] TentacleController leftTentacle;
    [SerializeField] TentacleController rightTentacle;
    [SerializeField] AudioSource BGMSource;

    [Header("Circle Attack")]
    [SerializeField] float circleAttackCooldown = 20f;

    [Header("Enrage")]
    [SerializeField] float enrageHPThreshold = 0.5f;     // 50% HP
    [SerializeField] float enrageCooldownMult = 0.6f;    // all cooldowns x0.6

    [Header("Debug")]
    [SerializeField] bool showGizmos = true;

    // -------------------------------------------------------
    // PRIVATE

    Transform player;
    float circleAttackTimer = 0f;
    bool  isEnraged         = false;

    // -------------------------------------------------------
    // START

    void Start()
    {
        Setup(bossData);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("BossController: No GameObject tagged 'Player' found.");

        // Freeze position — boss never moves
        if (rb != null)
        {
            rb.gravityScale  = 0f;
            rb.constraints   = RigidbodyConstraints2D.FreezeAll;
        }

        interuptImmune = true;
    }

    // -------------------------------------------------------
    // UPDATE

    void Update()
    {
        if (player == null || IsDown()) return;

        // Tick skill cooldowns (base Entity handles skill CDTick in TickUpdate via FixedUpdate)
        // We just manage our own fire timer here
        if (circleAttackTimer > 0f)
            circleAttackTimer -= Time.deltaTime;

        if (circleAttackTimer <= 0f)
        {
            // Debug.Log("Circle Attack");
            FireCircleAttack();
            circleAttackTimer = isEnraged
                ? circleAttackCooldown * enrageCooldownMult
                : circleAttackCooldown;
        }

        // Check enrage
        if (!isEnraged && stats[EntityData.Stat.HP] <= stats[EntityData.Stat.MaxHP] * enrageHPThreshold)
            TriggerEnrage();
    }

    public override void resetAnimation(bool resetAgent = true)
    {
        base.resetAnimation(resetAgent);
        interuptImmune = true;
    }

    // -------------------------------------------------------
    // FIRE CIRCLE ATTACK
    // Fires skill slot 0 — assign the BossCircle SkillData as first skill in bossData

    void FireCircleAttack()
    {
        if (!skills.ContainsKey(0)) return;
        skills[0].OnUse(this, player.position);
    }

    // -------------------------------------------------------
    // ENRAGE

    void TriggerEnrage()
    {
        isEnraged = true;
        BGMSource.clip = enragedBGM;
        BGMSource.Play();
        Debug.Log("BossController: ENRAGED");

        if (leftTentacle  != null) leftTentacle.SetEnraged(true);
        if (rightTentacle != null) rightTentacle.SetEnraged(true);
    }

    // -------------------------------------------------------
    // TAKE DAMAGE — override to skip enrage re-trigger

    public override int Damaged(int val, int ignoreDef=0, float penpercent=0f)
    {
        if (dmgImmune) return 0;
        if (stats[EntityData.Stat.HP]<=0) return 0;
        Hurt();
        int dmg = (int) (val*DMGReduction(ignoreDef,penpercent));
        if (dmg<=0) dmg = 1;
        stats[EntityData.Stat.HP] -= dmg;

        if (stats[EntityData.Stat.HP]<=0)
        {
            stats[EntityData.Stat.HP] = 0;
            //Insert on Down method
            BossDown();

        }
        //Insert UpdateHealth method
        UpdateHealth();
        if (UIManager.Instance!=null) UIManager.Instance.DMG(dmg,transform.position,ally);
        //Insert DmgPopUp method
        return dmg;
    }


    public void BossDown()
    {
        // Trigger Change Scene
        BossSceneScript.Instance.BossDown();
    }

    // -------------------------------------------------------
    // GIZMOS

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }

    public override void Flip()
    {
        isFlipped = !isFlipped;
    }

}

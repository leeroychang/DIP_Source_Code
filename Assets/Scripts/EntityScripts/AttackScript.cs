using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    protected Dictionary<SkillStat,float> skillStats;
    protected Skill skill;
    protected Entity caster;
    protected Vector2? dir;
    protected float speed;

    protected Coroutine endCoroutine;


    protected Dictionary<Entity,float> hits;
    protected float hitCD = 0;
    public bool knockbackCenter = false;
    public bool staticRotation = false;

    public virtual void SetUp(Skill skill, Entity caster, Vector2 targetDir, Vector2? moveDir = null, float hitCD=0)
    {
        staticRotation = false;
        skillStats = new Dictionary<SkillStat,float>(skill.skillStats);
        this.skill = skill;
        
        this.caster = caster;
        dir = moveDir;
        speed = skillStats.ContainsKey(SkillStat.ProjectileSpeed) ? skillStats[SkillStat.ProjectileSpeed] : 0;
        
        hits = new();
        this.hitCD = hitCD;
        endCoroutine = StartCoroutine(endAttack(skillStats[SkillStat.Duration]));

        transform.right = dir ?? targetDir;
        if (gameObject.GetComponent<Rigidbody2D>()==null) gameObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
    }

    void FixedUpdate()
    {
        TickUpdate();
    }

    void OnDisable()
    {
        if (endCoroutine!=null)
        {
            StopCoroutine(endCoroutine);
        }
    }

    public virtual void TickUpdate()
    {
        float tick = Time.fixedDeltaTime;
        foreach (Entity entity in hits.Keys.ToList())
        {
            if (hits[entity]==0) continue;
            hits[entity] -= tick;
            if (hits[entity]<=0) hits.Remove(entity);
        }

        if (dir==null) return;
        if (speed==0) return;

        if (!staticRotation)
        {
            transform.right = dir ?? Vector2.right;
            transform.Translate(Vector2.right*Time.fixedDeltaTime*speed);
        }
        else transform.Translate((dir ?? Vector2.right)*Time.fixedDeltaTime*speed);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Entity entity = other.gameObject.GetComponent<Entity>();
        EntityEnter(entity);
        if (other.tag=="Wall" && skillStats.ContainsKey(SkillStat.Pierce) && skillStats[SkillStat.Pierce]!=-1) attackDestroy();
    }

    public void EntityEnter(Entity entity)
    {
        if (entity==null || caster==entity || (caster.ally==entity.ally) || hits.ContainsKey(entity) || entity.IsDown()) return;

        if (!skillStats.ContainsKey(SkillStat.Damage)) return;
        OnHit(entity);
        if (!skillStats.ContainsKey(SkillStat.Pierce)) return;
        else
        {
            skillStats[SkillStat.Pierce]--;
            if (skillStats.ContainsKey(SkillStat.Damage) && skillStats.ContainsKey(SkillStat.Followthrough)) skillStats[SkillStat.Damage] = skillStats[SkillStat.Damage]*skillStats[SkillStat.Followthrough]/100; //Multiply first (int)
            if (skillStats[SkillStat.Pierce]==0) attackDestroy();
        }
    }

    protected virtual void OnHit(Entity entity)
    {
        DamageEvent dmgEvent;
        if (skillStats.ContainsKey(SkillStat.Knockback)) dmgEvent = new DamageEvent(caster,entity,skill,knockbackCenter ? (Vector2)(entity.transform.position-transform.position).normalized : transform.right);
        else dmgEvent = new DamageEvent(caster,entity,skill);
        EventManager.Queue(dmgEvent);
        if (skill.skillData.element!=Element.None)
        {
            if (Random.value<=skill.skillStats[SkillStat.StatusChance])
            {
                ElementEvent elementEvent = new ElementEvent(caster,entity,skill);
                EventManager.Queue(elementEvent);
            }
        }
        hits[entity] = hitCD;
    }

    public virtual IEnumerator endAttack(float duration)
    {
        yield return new WaitForSeconds(duration);
        attackDestroy();
    }

    protected virtual void attackDestroy()
    {
        if (endCoroutine!=null) StopCoroutine(endCoroutine);
        Destroy(gameObject);
    }

}

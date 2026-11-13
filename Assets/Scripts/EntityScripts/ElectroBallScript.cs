using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectroBallScript : MonoBehaviour
{
    protected Dictionary<SkillStat,float> skillStats;
    protected Skill skill;
    protected Entity caster;
    protected Vector2? dir;
    protected float speed;

    protected Coroutine endCoroutine;


    protected Dictionary<Entity,float> hits;
    protected float hitCD = 0;

    public virtual void SetUp(Skill skill, Entity caster, Vector2 targetDir, Vector2? moveDir = null, float hitCD=0)
    {
        skillStats = new Dictionary<SkillStat,float>(skill.skillStats);
        this.skill = skill;
        
        this.caster = caster;
        dir = moveDir;
        speed = skillStats.ContainsKey(SkillStat.ProjectileSpeed) ? skillStats[SkillStat.ProjectileSpeed] : 0;
        
        hits = new();
        this.hitCD = hitCD;
        endCoroutine = StartCoroutine(endAttack(skillStats[SkillStat.Duration]));

        transform.right = dir ?? targetDir;
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
        transform.right = dir ?? Vector2.right;
        transform.Translate(Vector2.right*Time.fixedDeltaTime*speed);
    }


    void OnTriggerStay2D(Collider2D collision)
    {
        Entity entity = collision.gameObject.GetComponent<Entity>();
        EntityEnter(entity);
    }

    public void EntityEnter(Entity entity)
    {
        if (entity==null || caster==entity || (caster.ally==entity.ally) || hits.ContainsKey(entity) || entity.IsDown()) return;

        if (!skillStats.ContainsKey(SkillStat.Damage)) return;
        OnHit(entity);
    }


    protected virtual void OnHit(Entity entity)
    {
        Link(entity,skillStats[SkillStat.Damage]);
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

    private void Link(Entity entity, float dmg)
    {
        LineRenderAnimation line = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ShockPrefab")).GetComponent<LineRenderAnimation>();
        line.obj1 = gameObject;
        line.obj2 = entity.gameObject;

        DamageEvent dmgEvent = new DamageEvent(entity,dmg);
        EventManager.Queue(dmgEvent);
    }








    public virtual IEnumerator endAttack(float duration)
    {
        yield return new WaitForSeconds(duration);
        attackDestroy();
    }

    protected void attackDestroy()
    {
        if (endCoroutine!=null) StopCoroutine(endCoroutine);
        Destroy(gameObject);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class AttackScript_OnHit : AttackScript
{
    Transform HitTransform;
    public GameObject OnHitPrefab;
    
    public StatusEffect OnHitStatus;
    public bool HitOnDestroy = false;

    protected override void OnHit(Entity entity)
    {
        DamageEvent dmgEvent;
        if (skillStats.ContainsKey(SkillStat.Knockback)) dmgEvent = new DamageEvent(caster,entity,skill,transform.right);
        else dmgEvent = new DamageEvent(caster,entity,skill);
        EventManager.Queue(dmgEvent);
        if (skill.skillData.element!=Element.None)
        {
            if (Random.value<=skillStats[SkillStat.StatusChance])
            {
                ElementEvent elementEvent = new ElementEvent(caster,entity,skill);
                EventManager.Queue(elementEvent);
            }
        }
        hits[entity] = hitCD;

        if (!HitOnDestroy && OnHitPrefab!=null)
        {
            if (HitTransform==null)
            {
                Collider2D coll =  GetComponent<Collider2D>();
                if (coll==null) coll = GetComponentInChildren<Collider2D>();
                if (coll!=null) HitTransform = coll.transform;
            }
            SpawnedAreaScript aoe = Instantiate(OnHitPrefab).AddComponent<SpawnedAreaScript>();
            aoe.transform.position = HitTransform.position;
            aoe.SetUp(skillStats[SkillStat.Damage],2,caster.ally,skill.skillData.element,skillStats[SkillStat.StatusChance]);
        }

        if (OnHitStatus!=null)
        {
            StatusEffect status = OnHitStatus.Clone();
            status.ChangeEntity(entity,false);
            StatusEvent statusEvent = new StatusEvent(caster,entity,skill,status);
            EventManager.Queue(statusEvent);
        }
        
    }

    protected override void attackDestroy()
    {
        if (HitOnDestroy && OnHitPrefab!=null)
        {
            if (HitTransform==null)
            {
                Collider2D coll =  GetComponent<Collider2D>();
                if (coll==null) coll = GetComponentInChildren<Collider2D>();
                if (coll!=null) HitTransform = coll.transform;
            }
            SpawnedAreaScript aoe = Instantiate(OnHitPrefab).AddComponent<SpawnedAreaScript>();
            aoe.transform.position = HitTransform.position;
            aoe.SetUp(skillStats[SkillStat.Damage],2,caster.ally,skill.skillData.element,skillStats[SkillStat.StatusChance]);
        }
        base.attackDestroy();
    }
}

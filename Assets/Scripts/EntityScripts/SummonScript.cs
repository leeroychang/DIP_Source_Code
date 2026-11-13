using System.Collections;
using UnityEngine;

public class SummonScript : Entity
{
    Skill skill;
    SkillSummon skillSummon;
    Entity caster;
    float duration;

    bool requireTarget = true;

    


    Coroutine CastCoroutine;

    public void SummonSetUp(Vector3 pos, Entity caster, Skill skill, SkillSummon skillSummon, bool requireTarget)
    {
        transform.position = pos;
        this.caster = caster;
        this.skill = skill;
        this.skillSummon = skillSummon;
        this.requireTarget = requireTarget;
        duration = skillSummon.GetStats()[SkillStat.Duration];

        Setup(skillSummon.summonEntityData,caster.ally);
        dmgImmune = true;
        defaultLayer = "EntityIgnore";

        CastCoroutine = StartCoroutine(Tick());
    }

    void FixedUpdate()
    {
        TickUpdate();
    }


    public IEnumerator Tick()
    {
        while (duration>0)
        {
            Cast();

            if (skillSummon.castInterval>duration)
            {
                yield return new WaitForSeconds(duration);
                duration = 0;
                break;
            }

            yield return new WaitForSeconds(skillSummon.castInterval);
            duration -= skillSummon.castInterval;
        }

        Unsummon();
    }

    public void Cast()
    {

        if (!requireTarget)
        {
            skillSummon.summonSkill.OnUse(this,transform.position,true);
            return;
        }


        Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position,skillSummon.targetRange);
        Entity target = null;
        bool isAlly = caster.ally;
        foreach (Collider2D coll in colls)
        {
            Entity e = coll.GetComponent<Entity>();
            if (e==null) e = coll.GetComponentInChildren<Entity>();
            if (e==null) continue;
            if (e.ally==isAlly) continue;

            if (target==null || Vector2.SqrMagnitude(transform.position-e.transform.position)<Vector2.SqrMagnitude(transform.position-target.transform.position)) target = e;
        }

        if (target==null) return;
        skillSummon.summonSkill.OnUse(this,target.transform.position,true);
    
    }

    public void Unsummon()
    {
        if (CastCoroutine!=null) StopCoroutine(CastCoroutine);

        StartCoroutine(OnDown());
        SkillSummon sumType = skill.skillData.skillType as SkillSummon;
        if (sumType==null) return;
        sumType.RemoveSummon(this);
    }

}

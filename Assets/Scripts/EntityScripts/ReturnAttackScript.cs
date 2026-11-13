using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ReturnAttackScript : AttackScript
{
    float baseSpeed;
    float duration;
    float maxDuration;

    public override void SetUp(Skill skill, Entity caster, Vector2 targetDir, Vector2? moveDir = null, float hitCD=0)
    {
        skillStats = new Dictionary<SkillStat,float>(skill.skillStats);
        this.skill = skill;
        this.caster = caster;
        dir = moveDir;
        baseSpeed = skillStats.ContainsKey(SkillStat.ProjectileSpeed) ? skillStats[SkillStat.ProjectileSpeed] : 1;
        speed = baseSpeed;
        maxDuration = skillStats[SkillStat.Duration];
        duration = maxDuration;

        hits = new();
        this.hitCD = hitCD;
        endCoroutine = StartCoroutine(maxDistReached(duration));

        transform.right = dir ?? targetDir;
    }

    void FixedUpdate()
    {
        speed = baseSpeed*math.abs(duration)/maxDuration;
        duration -= Time.fixedDeltaTime;
        TickUpdate();
    }

    public IEnumerator maxDistReached(float duration)
    {
        yield return new WaitForSeconds(duration);
        endCoroutine = StartCoroutine(UpdateTarget());
    }

    public IEnumerator UpdateTarget()
    {
        while (Vector2.Distance(transform.position,caster.transform.position)>0.5f)
        {
            dir = (caster.transform.position-transform.position).normalized;
            yield return new WaitForFixedUpdate();
        }
        attackDestroy();
    }
}

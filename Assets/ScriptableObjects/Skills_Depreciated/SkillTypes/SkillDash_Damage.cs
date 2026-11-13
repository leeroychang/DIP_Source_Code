using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillDash_Damage : SkillDash
{
    [SerializeField] GameObject areaPrefab;
    [SerializeField] float damage;
    [SerializeField] float hitCD = 0;
    [SerializeField] bool rotateArea = false;

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        Vector2 dir = (targetPos-entity.transform.position).normalized;
        Dictionary<SkillStat,float> skillStats = skill.skillStats;
        entity.Knockback(skillStats[SkillStat.MoveSpeed],skillStats[SkillStat.Duration],dir,false,true,false);
        FollowAreaScript followAreaScript = GameObject.Instantiate(areaPrefab,entity.transform).AddComponent<FollowAreaScript>();
        followAreaScript.SetUp(skillStats[SkillStat.Damage],skillStats[SkillStat.Duration],entity.ally,skill.skillData.element,skillStats[SkillStat.StatusChance],hitCD);
        followAreaScript.followTarget=entity;
        if (rotateArea) 
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            followAreaScript.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public override Dictionary<SkillStat, float> GetStats()
    {
        Dictionary<SkillStat,float> stats = base.GetStats();
        stats[SkillStat.Damage] = damage;
        return stats;
    }
}

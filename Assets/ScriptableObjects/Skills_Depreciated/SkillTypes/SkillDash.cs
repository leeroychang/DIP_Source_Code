using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillDash : SkillType
{
    [SerializeField] float moveSpeed;
    [SerializeField] float duration;

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        Dictionary<SkillStat,float> skillStats = skill.skillStats;
        entity.Knockback(skillStats[SkillStat.MoveSpeed],skillStats[SkillStat.Duration],(targetPos-entity.transform.position).normalized,false,true,false);
        // entity.StartAnimation(skillStats[SkillStat.Duration],skillStats[SkillStat.MoveSpeed],targetPos.normalized);
    }

    public override Dictionary<SkillStat,float> GetStats()
    {
        Dictionary<SkillStat,float> stats = base.GetStats();
        stats[SkillStat.MoveSpeed] = moveSpeed;
        stats[SkillStat.Duration] = duration;
        return stats;

    }

}

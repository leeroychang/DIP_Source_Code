using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class SkillBuff : SkillType
{
    [SerializeField] float duration;

    public abstract StatusEffect GetStatus(Skill skill, Entity entity);

    public override Dictionary<SkillStat, float> GetStats()
    {
        Dictionary<SkillStat,float> stats = base.GetStats();
        stats[SkillStat.Duration] = duration;
        return stats;
    }
}

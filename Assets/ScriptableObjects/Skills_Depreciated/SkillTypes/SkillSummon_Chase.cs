using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class SkillSummon_Chase : SkillSummon_Follow
{
    public override void OnSummon(Skill skill, Entity entity, Vector3 summonPos)
    {
        base.OnSummon(skill, entity, summonPos);
        if (summons.Last().TryGetComponent(out SummonScript_Chase script)) script.SetUp();
    }
}

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class SkillSummon_Follow : SkillSummon
{

    public override void OnSummon(Skill skill, Entity entity, Vector3 summonPos)
    {
        base.OnSummon(skill, entity, summonPos);
        if (summons.Last().TryGetComponent(out SummonScript_Follow script)) script.SetUp();
    }
    
}

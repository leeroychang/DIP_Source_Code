using System;
using UnityEngine;

[Serializable]
public class ModifierStat : Modifier
{
    [SerializeField] SkillStat stat;
    [SerializeField] float value;

    public override void OnEnable(Skill skill, int slot)
    {
        if (skill.statModifier.ContainsKey(stat)) skill.statModifier[stat] += value;
        if (stat==SkillStat.Cooldown && skill.cooldown>skill.skillStats[SkillStat.Cooldown]) skill.ResetCooldown();
    }

    public override void OnDisable(Skill skill, int slot)
    {
        if (skill.statModifier.ContainsKey(stat)) skill.statModifier[stat] -= value;
        if (stat==SkillStat.Cooldown && skill.cooldown>skill.skillStats[SkillStat.Cooldown]) skill.ResetCooldown();
    }
}

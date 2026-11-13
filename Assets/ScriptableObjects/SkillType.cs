using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class SkillType
{
    [SerializeField] protected float cooldown;
    [SerializeField] protected int charge;
    [SerializeField] public float castTime = 0;
    [SerializeField] public float aniTime = 1f;
    [SerializeField] public float statusChance = 0.5f;
    [SerializeField] public List<DamageType> damageTypes = new();
    public abstract void OnUse(Skill skill, Entity entity, Vector3 targetPos);

    public virtual void OnRelease(Entity entity, Vector2? dir=null) {}

    public virtual Dictionary<SkillStat,float> GetStats()
    {
        return new Dictionary<SkillStat,float>()
        {
            {SkillStat.Cooldown, cooldown},
            {SkillStat.Charge, charge},
            {SkillStat.StatusChance, statusChance}
        };
    }


    public enum DamageType
    {
        None,
        Melee,
        Projectile,
        Summon,
        AOE
    }

}

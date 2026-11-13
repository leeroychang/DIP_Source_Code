using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class SkillMelee : SkillType
{
    [SerializeField] int damage;
    [SerializeField] float duration;
    [SerializeField] protected GameObject attackPrefab;

    [SerializeField] protected float minDistance; //Min Spawn Distance
    [SerializeField] protected float maxDistance; //Max Spawn Distance
    [SerializeField] float knockback;
    

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        Vector2 targetDir = targetPos-entity.transform.position;
        SpawnProjectile(skill, targetDir, entity);
        
    }

    public virtual void SpawnProjectile(Skill skill, Vector2 targetDir, Entity entity)
    {
        GameObject attack = GameObject.Instantiate(attackPrefab);
        AttackScript attackScript = attack.AddComponent<AttackScript>();
        attack.transform.position = entity.transform.position+(Vector3)targetDir.normalized*math.clamp(targetDir.magnitude,minDistance,maxDistance);
        attackScript.SetUp(skill,entity,targetDir.normalized,targetDir.normalized);
    }

    public override Dictionary<SkillStat,float> GetStats()
    {
        if (damageTypes.Count==0) damageTypes.Add(DamageType.Melee);
        Dictionary<SkillStat,float> stats = base.GetStats();
        stats[SkillStat.Damage] = damage;
        stats[SkillStat.Duration] = duration;
        stats[SkillStat.Knockback] = knockback;
        return stats;

    }
}

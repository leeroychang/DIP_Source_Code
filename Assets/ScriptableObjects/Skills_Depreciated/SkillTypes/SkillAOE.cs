using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class SkillAOE : SkillType
{
    [SerializeField] int damage;
    [SerializeField] float projSpeed = 5;
    [SerializeField] float duration;
    [SerializeField] protected float hitCD;
    [SerializeField] protected GameObject attackPrefab;
    [SerializeField] protected float radius;
    [SerializeField] protected bool staticRotation;

    [SerializeField] protected float minDistance; //Min Spawn Distance
    [SerializeField] protected float maxDistance; //Max Spawn Distance
    [SerializeField] protected float knockback;


    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        Vector2 targetDir = targetPos-entity.transform.position;
        SpawnProjectile(skill, targetDir, entity);
        
    }

    public virtual void SpawnProjectile(Skill skill, Vector2 targetDir, Entity entity)
    {
        GameObject attack = GameObject.Instantiate(attackPrefab);
        AttackScriptStay attackScript = attack.AddComponent<AttackScriptStay>();
        attack.transform.position = entity.transform.position+(Vector3)targetDir.normalized*math.clamp(targetDir.magnitude,minDistance,maxDistance);
        attackScript.SetUp(skill,entity,targetDir.normalized,targetDir.normalized,hitCD);
        attackScript.knockbackCenter = true;
        if (staticRotation)
        {
            attackScript.staticRotation = true;
            attackScript.transform.rotation = Quaternion.identity;
        }
        if (attack.TryGetComponent(out CircleCollider2D coll))
        {
            coll.radius = radius;
        }
    }

    public override Dictionary<SkillStat,float> GetStats()
    {
        if (damageTypes.Count==0) damageTypes.Add(DamageType.AOE);
        Dictionary<SkillStat,float> stats = base.GetStats();
        stats[SkillStat.Damage] = damage;
        stats[SkillStat.ProjectileSpeed] = projSpeed;
        stats[SkillStat.Duration] = duration;
        stats[SkillStat.Pierce] = -1;
        stats[SkillStat.Followthrough] = 1;
        stats[SkillStat.Knockback] = knockback;
        return stats;

    }
}

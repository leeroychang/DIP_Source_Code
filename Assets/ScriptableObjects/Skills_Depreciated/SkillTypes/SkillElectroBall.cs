using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class SkillElectroBall : SkillType
{
    [SerializeField] int damage;
    [SerializeField] float projSpeed = 5;
    [SerializeField] float duration;
    [SerializeField] protected float hitCD;
    [SerializeField] protected GameObject attackPrefab;
    [SerializeField] protected float radius;

    [SerializeField] protected float minDistance; //Min Spawn Distance
    [SerializeField] protected float maxDistance; //Max Spawn Distance


    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        Vector2 targetDir = targetPos-entity.transform.position;
        SpawnProjectile(skill, targetDir, entity);
        
    }

    public void SpawnProjectile(Skill skill, Vector2 targetDir, Entity entity)
    {
        GameObject attack = GameObject.Instantiate(attackPrefab);
        ElectroBallScript attackScript = attack.AddComponent<ElectroBallScript>();
        attack.transform.position = entity.transform.position+(Vector3)targetDir.normalized*math.clamp(targetDir.magnitude,minDistance,maxDistance);
        attackScript.SetUp(skill,entity,targetDir.normalized,targetDir.normalized,hitCD);
        if (attack.TryGetComponent(out CircleCollider2D coll)) coll.radius = radius;
        
    }


    public override Dictionary<SkillStat,float> GetStats()
    {
        if (damageTypes.Count==0) damageTypes.Add(DamageType.AOE);
        Dictionary<SkillStat,float> stats = base.GetStats();
        stats[SkillStat.Damage] = damage;
        stats[SkillStat.ProjectileSpeed] = projSpeed;
        stats[SkillStat.Duration] = duration;
        return stats;

    }
}

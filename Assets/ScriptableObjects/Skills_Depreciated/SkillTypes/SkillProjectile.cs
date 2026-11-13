using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class SkillProjectile : SkillType
{
    [SerializeField] int damage;
    [SerializeField] float projSpeed = 15;
    [SerializeField] float duration = 1;
    [SerializeField] int pierce = 1; //Number of enemies hit b4 despawning (-1 for infinite)
    [SerializeField] float followthrough = 1; //Damage Multiplier after each hit (100 for consistent dmg)
    [SerializeField] protected float hitCD = 0;
    [SerializeField] protected GameObject attackPrefab;

    [SerializeField] protected float minDistance; //Min Spawn Distance
    [SerializeField] protected float maxDistance; //Max Spawn Distance
    [SerializeField] int projCount = 1;
    [SerializeField] float maxSpreadAngle = 90;
    [SerializeField] float baseAngleInterval = 15;
    [SerializeField] float knockback;
    

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        int count = GetProjCount(skill.skillStats[SkillStat.Count]);

        if (count<=0) return;
        
        if (count==1)
        {
            Vector2 targetDir = targetPos-entity.transform.position;
            SpawnProjectile(skill, targetDir, entity);
            return;
        }

        float angle = baseAngleInterval;
        if (angle*(count-1)>maxSpreadAngle)
        {
            if (maxSpreadAngle>=360) angle = maxSpreadAngle/count;
            else angle = maxSpreadAngle/(count-1);
        }

        Vector2 baseDir = targetPos-entity.transform.position;
        for (int i=0; i<count; i++)
        {
            float a = angle*((i+1)/2)*(i%2==0 ? 1 : -1);
            Vector2 targetDir =  Quaternion.Euler(0,0,a) * baseDir;
            SpawnProjectile(skill, targetDir, entity);
        }
        
        
    }

    private int GetProjCount(float count)
    {
        float frac = count - (int) count;
        if (frac>0) return (int) count + (UnityEngine.Random.value<=frac ? 1 : 0);
        return (int) count;
    }

    public virtual void SpawnProjectile(Skill skill, Vector2 targetDir, Entity entity)
    {
        GameObject attack = GameObject.Instantiate(attackPrefab);
        AttackScript attackScript = attack.AddComponent<AttackScript>();
        attack.transform.position = entity.transform.position+(Vector3)targetDir.normalized*math.clamp(targetDir.magnitude,minDistance,maxDistance);
        attackScript.SetUp(skill,entity,targetDir.normalized,targetDir.normalized,hitCD);
    }

    public override Dictionary<SkillStat,float> GetStats()
    {
        if (damageTypes.Count==0) damageTypes.Add(DamageType.Projectile);
        Dictionary<SkillStat,float> stats = base.GetStats();
        stats[SkillStat.Damage] = damage;
        stats[SkillStat.ProjectileSpeed] = projSpeed;
        stats[SkillStat.Pierce] = pierce;
        stats[SkillStat.Followthrough] = followthrough;
        stats[SkillStat.Duration] = duration;
        stats[SkillStat.Count] = projCount;
        stats[SkillStat.Knockback] = knockback;
        return stats;

    }
}

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class SkillGravity : SkillType
{
    [SerializeField] float duration;
    [SerializeField] protected GameObject aoePrefab;
    [SerializeField] protected float radius;
    [SerializeField] protected float minDistance; //Min Spawn Distance
    [SerializeField] protected float maxDistance; //Max Spawn Distance
    [SerializeField] protected float strength;

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        Dictionary<SkillStat,float> stats = skill.skillStats;
        Vector2 targetDir = targetPos-entity.transform.position;
        GameObject aoe = GameObject.Instantiate(aoePrefab);
        GravityScript gravity = aoe.AddComponent<GravityScript>();
        aoe.transform.position = entity.transform.position + (Vector3)targetDir.normalized*math.clamp(targetDir.magnitude,minDistance,maxDistance);
        gravity.SetUp(stats[SkillStat.Knockback],radius,entity.ally,stats[SkillStat.Duration]);
    }


    public override Dictionary<SkillStat,float> GetStats()
    {
        Dictionary<SkillStat,float> stats = base.GetStats();
        stats[SkillStat.Duration] = duration;
        stats[SkillStat.Knockback] = strength;
        return stats;

    }
}

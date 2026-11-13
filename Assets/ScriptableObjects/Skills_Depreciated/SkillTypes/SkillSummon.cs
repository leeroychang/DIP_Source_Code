using System;
using System.Collections.Generic;
using SerializeReferenceEditor;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class SkillSummon : SkillType
{

    [SerializeField] GameObject summonPrefab;
    public EntityData summonEntityData;

    [Header("Cast Modifiers")]
    [SerializeField] protected float minDistance; //Min Spawn Distance
    [SerializeField] protected float maxDistance; //Max Spawn Distance


    [Header("Summon Modifiers")]
    public float targetRange; //Range summon can target entities
    public float castInterval; //Time interval between summon skill cast
    public float summonDuration;
    public float summonCount = 1;
    public bool requireTarget;
    
    protected List<SummonScript> summons = new();

    [SerializeField] SkillData summonSkillData;
    public Skill summonSkill;

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        Vector3 targetDir = targetPos-entity.transform.position;
        float dist = math.clamp(targetDir.magnitude,minDistance,maxDistance);
        Vector3 summonPos = entity.transform.position + targetDir*dist;
        summonSkill = new Skill(summonSkillData)
        {
            statModifier = skill.statModifier,
            baseSkill = skill
        };

        float baseCount = skill.GetStat(SkillStat.Count,0.5f);
        int count = (int)baseCount;
        if (baseCount!=(int)baseCount) count = UnityEngine.Random.value<=baseCount-count? count+1 : count;

        for (int i=0; i<count; i++) OnSummon(skill,entity,summonPos);
        
    }

    public virtual void OnSummon(Skill skill, Entity entity, Vector3 basePos)
    {
        SummonScript summon = GameObject.Instantiate(summonPrefab).GetComponent<SummonScript>();
        Vector3 summonPos = basePos + new Vector3(UnityEngine.Random.value,UnityEngine.Random.value,0);
        summon.SummonSetUp(summonPos,entity,skill,this,requireTarget);
        summons.Add(summon);
    }

    public override Dictionary<SkillStat, float> GetStats()
    {
        if (damageTypes.Count==0) damageTypes.Add(DamageType.Summon);
        Dictionary<SkillStat,float> stats = base.GetStats();
        stats[SkillStat.Duration] = summonDuration;
        stats[SkillStat.Count] = summonCount;
        return stats;

    }

    public void RemoveSummon(SummonScript summon)
    {
        if (summons.Contains(summon)) summons.Remove(summon);
    }
}

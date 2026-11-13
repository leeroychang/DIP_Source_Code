using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillMirage : SkillType
{
    
    [SerializeField] float range;
    [SerializeField] float duration;
    
    [SerializeField] GameObject miragePrefab;
    [SerializeField] GameObject clonePrefab;
    [SerializeField] EntityData cloneData;

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        MirageScript mirage = GameObject.Instantiate(miragePrefab).AddComponent<MirageScript>();
        mirage.SetUp(range, skill, entity, cloneData, clonePrefab);
    }

    public override Dictionary<SkillStat, float> GetStats()
    {
        Dictionary<SkillStat,float> stats = base.GetStats();
        stats[SkillStat.Duration] = duration;
        return stats;
    }
}

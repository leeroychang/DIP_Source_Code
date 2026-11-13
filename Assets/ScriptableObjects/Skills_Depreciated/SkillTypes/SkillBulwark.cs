using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillBulwark : SkillType
{
    [SerializeField] GameObject summonPrefab;
    public EntityData summonEntityData;

    [SerializeField] float duration;

    [Header("Cast Modifiers")]
    [SerializeField] protected float minDistance; //Min Spawn Distance
    [SerializeField] protected float maxDistance; //Max Spawn Distance

    [SerializeField] float maxSpreadAngle = 90;
    [SerializeField] float baseAngleInterval = 15;

    [SerializeField] SkillData summonSkillData;
    public Skill summonSkill;


    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        Vector3 dir = targetPos-entity.transform.position;
        float dist = Mathf.Clamp(dir.magnitude,minDistance,maxDistance);
        Vector3 baseDir = dir.normalized*dist;

        summonSkill = new Skill(summonSkillData)
        {
            statModifier = skill.statModifier,
            baseSkill = skill
        };

        float baseCount = skill.GetStat(SkillStat.Count,0.5f);
        int count = (int)baseCount;
        if (baseCount!=(int)baseCount) count = Random.value<=baseCount-count? count+1 : count;

        if (count<=0) return;
        
        if (count==1)
        {
            OnSummon(skill, entity, baseDir);
            return;
        }

        float angle = baseAngleInterval;
        if (angle*(count-1)>maxSpreadAngle)
        {
            if (maxSpreadAngle>=360) angle = maxSpreadAngle/count;
            else angle = maxSpreadAngle/(count-1);
        }

        for (int i=0; i<count; i++)
        {
            float a = angle*((i+1)/2)*(i%2==0 ? 1 : -1);
            Vector2 targetDir = Quaternion.Euler(0,0,a) * baseDir;
            OnSummon(skill, entity, targetDir);
        }
    }



    public virtual void OnSummon(Skill skill, Entity entity, Vector3 baseDir)
    {
        Vector2 pos = entity.transform.position + baseDir;
        RaycastHit2D hit = Physics2D.Raycast(entity.transform.position,baseDir,baseDir.magnitude,LayerMask.GetMask("Wall","Default"));
        if (hit.collider!=null) pos = hit.point;
        BulwarkScript bulwark = GameObject.Instantiate(summonPrefab).GetComponent<BulwarkScript>();
        bulwark.Setup(summonEntityData,entity.ally);
        bulwark.BulwarkSetUp(pos,duration,summonSkill);
    }


    public override Dictionary<SkillStat, float> GetStats()
    {
        if (damageTypes.Count==0) damageTypes.Add(DamageType.Summon);
        Dictionary<SkillStat,float> stats = base.GetStats();
        stats[SkillStat.Duration] = duration;
        stats[SkillStat.Count] = 1;
        return stats;

    }









}

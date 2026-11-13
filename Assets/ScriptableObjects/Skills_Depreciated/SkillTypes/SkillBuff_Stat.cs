using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillBuff_Stat : SkillBuff
{
    [SerializeField] EntityData.Stat stat;
    [SerializeField] float modifier;
    [SerializeField] Sprite statusIcon;

    public override StatusEffect GetStatus(Skill skill, Entity entity)
    {
        Dictionary<SkillStat,float> stats = skill.skillStats;
        StatStatus status = new StatStatus(entity,skill,stats[SkillStat.Duration]);
        status.Set(stat,modifier,statusIcon);
        return status;
    }

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        StatusEvent statusEvent = new StatusEvent(entity, entity, skill, GetStatus(skill, entity));
        EventManager.Queue(statusEvent);
    }



    public class StatStatus : StatusEffect
    {
        EntityData.Stat stat;
        float modifier;
        Sprite statusIcon;


        public StatStatus(Entity entity, Skill skill, float duration = -1, int stacks = 1) : base(entity, skill, duration, stacks)
        {
            
        }

        public void Set(EntityData.Stat stat, float modifier, Sprite statusIcon)
        {
            this.stat = stat;
            this.modifier = modifier;
            this.statusIcon = statusIcon;
        }

        public override void AddStatus()
        {
            base.AddStatus();
            entity.UpdateStat(stat, modifier);
        }

        public override void RemoveStatus()
        {
            base.RemoveStatus();
            entity.UpdateStat(stat, -modifier);
        }

        public override StatusEffect Clone()
        {
            return new StatStatus(entity,skill,duration,stacks);
        }

        public override Sprite icon => statusIcon;
    }
}

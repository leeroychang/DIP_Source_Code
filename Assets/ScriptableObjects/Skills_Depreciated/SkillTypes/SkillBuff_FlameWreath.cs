using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SkillBuff_FlameWreath : SkillBuff
{
    [SerializeField] SkillData triggerSkilldata;

    public override StatusEffect GetStatus(Skill skill, Entity entity)
    {
        Dictionary<SkillStat,float> stats = skill.skillStats;
        FlameWreath status = new FlameWreath(entity, skill, stats[SkillStat.Duration]);
        Skill triggerSkill = new Skill(triggerSkilldata);
        status.SetTriggerSkill(triggerSkill,skill.statModifier);
        triggerSkill.baseSkill = skill;
        return status;
    }

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        StatusEvent statusEvent = new StatusEvent(entity, entity, skill, GetStatus(skill, entity));
        EventManager.Queue(statusEvent);
    }


    [Serializable]
    public class FlameWreath : StatusEffect
    {
        FlameWreathReaction reaction;
        GameObject effectPrefab;

        public FlameWreath(Entity entity, Skill skill, float duration = -1, int stacks = 1) : base(entity, skill, duration, stacks)
        {
        }

        public void SetTriggerSkill(Skill skill, Dictionary<SkillStat,float> modifiers)
        {
            Skill triggerSkill = skill;
            triggerSkill.statModifier = modifiers;

            reaction = new FlameWreathReaction(entity, skill, modifiers);
        }

        public override void AddStatus()
        {
            base.AddStatus();
            EventManager.Subscribe(typeof(CastEvent), reaction, Reaction.ReactionTiming.POST);
            if (effectPrefab!=null) GameObject.Destroy(effectPrefab);
            effectPrefab = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/FlameWreathPrefab"),entity.transform);
        }

        public override void RemoveStatus()
        {
            base.RemoveStatus();
            EventManager.Unsubscribe(typeof(CastEvent), reaction, Reaction.ReactionTiming.POST);
            if (effectPrefab!=null) GameObject.Destroy(effectPrefab);
        }

        public override StatusEffect Clone()
        {
            return new FlameWreath(entity,skill,duration,stacks);
        }

        [Serializable]
        public class FlameWreathReaction : Reaction
        {
            public Entity entity;
            Skill triggerSkill;

            public FlameWreathReaction(Entity entity, Skill skill, Dictionary<SkillStat,float> modifiers)
            {
                this.entity = entity;
                triggerSkill = skill;
                foreach (SkillStat stat in triggerSkill.statModifier.Keys.ToList()) if (modifiers.ContainsKey(stat)) triggerSkill.statModifier[stat] = modifiers[stat];
            }

            public override void Call(GameEvent gameEvent)
            {
                CastEvent cast = gameEvent as CastEvent;
                if (cast==null || cast.caster!=entity || cast.targetPos==entity.transform.position) return;
                if (cast.skill==null || cast.skill.skillData.displayName=="Basic Attack" || cast.skill.skillData.displayName=="Dash") return;
                triggerSkill.OnUse(entity,cast.targetPos,true);

            }
        }
    }
}

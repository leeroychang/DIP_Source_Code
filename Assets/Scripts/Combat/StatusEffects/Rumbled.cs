using UnityEngine;

public class Rumbled : StatusEffect
{

    Reaction reaction;

    public Rumbled(Entity entity, float duration=-1, int stacks = 1) : base(entity, duration, stacks)
    {
        maxStacks = 1;
        maxDuration = 5;

        if (duration==-1) this.duration = maxDuration;
    }

    public override void AddStatus()
    {
        base.AddStatus();
        reaction = new RumbledReaction(this);
        EventManager.Subscribe(typeof(DamageEvent), reaction, Reaction.ReactionTiming.PRE);
        //entity.UpdateStat(EntityData.Stat.HP,-0.25f);
    }

    public override void RemoveStatus()
    {
        base.RemoveStatus();
        EventManager.Unsubscribe(typeof(DamageEvent), reaction, Reaction.ReactionTiming.PRE);
        //entity.UpdateStat(EntityData.Stat.HP,0.25f);
    }

    public override StatusEffect Clone()
    {
        return new Rumbled(entity,duration,stacks);
    }


    public class RumbledReaction : Reaction
    {
        Rumbled status;
        public RumbledReaction(Rumbled status)
        {
            this.status = status;
        }

        public override void Call(GameEvent gameEvent)
        {
            DamageEvent dmgEvent = gameEvent as DamageEvent;
            Entity entity = gameEvent.target;
            if (dmgEvent==null || entity!=status.entity) return;
            if (dmgEvent.skill!=null && dmgEvent.skill.skillData.skillType.damageTypes.Contains(SkillType.DamageType.Melee))
                dmgEvent.dmg *= 1.5f; //Temp Formula
            
        }
    }
}

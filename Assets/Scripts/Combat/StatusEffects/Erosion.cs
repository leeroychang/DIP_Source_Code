using UnityEngine;

public class Erosion : StatusEffect
{
    Reaction reaction;

    public Erosion(Entity entity, float duration = -1, int stacks = 1) : base(entity, duration, stacks)
    {
        maxStacks = 1;
        maxDuration = 5;

        if (duration==-1) this.duration = maxDuration;
    }



    public override void AddStatus()
    {
        base.AddStatus();
        reaction = new ErosionReaction(this);
        EventManager.Subscribe(typeof(DamageEvent), reaction, Reaction.ReactionTiming.PRE);
    }

    public override void RemoveStatus()
    {
        base.RemoveStatus();
        EventManager.Unsubscribe(typeof(DamageEvent), reaction, Reaction.ReactionTiming.PRE);
    }

    public override StatusEffect Clone()
    {
        return new Erosion(entity,duration,stacks);
    }


    public class ErosionReaction : Reaction
    {
        Erosion status;
        public ErosionReaction(Erosion status)
        {
            this.status = status;
        }

        public override void Call(GameEvent gameEvent)
        {
            DamageEvent dmgEvent = gameEvent as DamageEvent;
            Entity entity = gameEvent.caster;
            if (dmgEvent==null || entity!=status.entity) return;
            dmgEvent.dmg *= 0.75f;
            
        }
    }
}

using UnityEngine;

public class Stun : StatusEffect
{
    public Stun(Entity entity, float duration=-1, int stacks = 1) : base(entity, duration, stacks)
    {
        maxStacks = 1;
        maxDuration = 2;

        if (duration==-1) this.duration = maxDuration;
    }

    public override void AddStatus()
    {
        base.AddStatus();
        entity.StartAnimation(duration);
    }

    public override StatusEffect Clone()
    {
        return new Stun(entity,duration,stacks);
    }
}

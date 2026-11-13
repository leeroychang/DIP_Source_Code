using UnityEngine;

public class Frost : StatusEffect
{
    public Frost(Entity entity, float duration=-1, int stacks = 1) : base(entity, duration, stacks)
    {
        maxStacks = 1;
        maxDuration = 5;

        if (duration==-1) this.duration = maxDuration;
    }

    public override void AddStatus()
    {
        base.AddStatus();
        entity.UpdateStat(EntityData.Stat.SPD,-0.5f);
    }

    public override void RemoveStatus()
    {
        base.RemoveStatus();
        entity.UpdateStat(EntityData.Stat.SPD,0.5f);
    }

    public override StatusEffect Clone()
    {
        return new Frost(entity,duration,stacks);
    }

}

using UnityEngine;

public class Burn : StatusEffect
{
    
    float tickCD = 0.5f;
    float currentCD;
    int baseDMG = 3;


    public Burn(Entity entity, float duration=-1, int stacks = 1) : base(entity, duration, stacks)
    {
        maxStacks = 9;
        maxDuration = 3;
        if (duration==-1) this.duration = maxDuration;
        currentCD = tickCD;
    }



    public override bool TickUpdate(float time)
    {
        currentCD -= time;
        if (currentCD<=0) Tick();

        return base.TickUpdate(time);
    }

    public void Tick()
    {
        float dmg = baseDMG*stacks; //Temporary DMG formula
        DamageEvent dmgEvent = new DamageEvent(entity,dmg);
        EventManager.Queue(dmgEvent);
        currentCD = tickCD;
    }

    public override StatusEffect Clone()
    {
        return new Burn(entity,duration,stacks);
    }
}

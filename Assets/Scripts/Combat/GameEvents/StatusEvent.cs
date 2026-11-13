using UnityEngine;

public class StatusEvent : GameEvent
{

    //Status Effect
    StatusEffect status;


    public StatusEvent(Entity caster, Entity target, Skill skill, StatusEffect status)
    {
        this.caster = caster;
        this.target = target;
        this.skill = skill;
        this.status = status;
    }

    public override void Call()
    {
        if (target==null || skill==null || status==null) return;
        status.AddStatus();
    }
}

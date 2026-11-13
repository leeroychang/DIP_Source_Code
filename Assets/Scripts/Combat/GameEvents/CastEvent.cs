using UnityEngine;

public class CastEvent : GameEvent
{

    public Vector3 targetPos;

    public CastEvent(Entity caster, Entity target, Skill skill)
    {
        this.caster = caster;
        this.target = target;
        targetPos = target.transform.position;
        this.skill = skill;
    }

    public CastEvent(Entity caster, Vector3 targetPos, Skill skill)
    {
        this.caster = caster;
        this.targetPos = targetPos;
        this.skill = skill;
    }

    public override void Call()
    {
        skill.skillData.skillType.OnUse(skill, caster, targetPos);
    }
}

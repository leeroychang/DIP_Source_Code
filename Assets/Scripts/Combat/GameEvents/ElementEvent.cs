using UnityEngine;

public class ElementEvent : GameEvent
{
    Element element1;
    Element element2;

    public ElementEvent(Entity caster, Entity target, Skill skill)
    {
        this.caster = caster;
        this.target = target;
        this.skill = skill;
        element1 = skill.skillData.element;
        element2 = skill.skillData.elementMerge;
    }

    public ElementEvent(Entity target, Element element, Element elementMerge=Element.None)
    {
        this.target = target;
        element1 = element;
        element2 = elementMerge;
    }

    public override void Call()
    {
        if (target==null || target.dmgImmune) return;
        if (element2==Element.None) ElementHandler.ApplyElement(element1,target);
        else
        {
            StatusEffect status = ElementHandler.GenStatus(element1,element2,1,target);
            status.AddStatus();
        }
    }
}

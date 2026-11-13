using System;
using UnityEngine;

public class ElementHandler : MonoBehaviour
{
    public static Type GetStatus(Element element)
    {
        switch (element)
        {
            case Element.Fire: return typeof(Burn);
            case Element.Earth: return typeof(Rumbled);
            case Element.Ice: return typeof(Frost);
            case Element.Electric: return typeof(Shock);
        }
        return null;
    }

    public static Element GetElement(StatusEffect status)
    {
        switch (status)
        {
            case Burn: return Element.Fire;
            case Rumbled: return Element.Earth;
            case Frost: return Element.Ice;
            case Shock: return Element.Electric;
        }
        return Element.None;
    }

    public static StatusEffect GenStatus(Element e1, Element e2, int stacks, Entity entity)
    {
        switch (e1)
        {
            case Element.Fire:
                switch (e2)
                {
                    case Element.Fire: return new Burn(entity);
                    case Element.Earth: return new Shatter(entity);
                    case Element.Ice: return new Steam(entity,-1,stacks);
                    case Element.Electric: return new Combust(entity,-1,stacks);
                    case Element.None: return new Burn(entity);
                }
                break;

            case Element.Earth:
                switch (e2)
                {
                    case Element.Fire: return new Shatter(entity);
                    case Element.Earth: return new Rumbled(entity);
                    case Element.Ice: return new Erosion(entity);
                    case Element.Electric: return new Stun(entity);
                    case Element.None: return new Rumbled(entity);
                }
                break;

            case Element.Ice:
                switch (e2)
                {
                    case Element.Fire: return new Steam(entity,-1,stacks);
                    case Element.Earth: return new Erosion(entity);
                    case Element.Ice: return new Frost(entity);
                    case Element.Electric: return new Crystallise(entity);
                    case Element.None: return new Frost(entity);
                }
                break;
                
            case Element.Electric:
                switch (e2)
                {
                    case Element.Fire: return new Combust(entity,-1,stacks);
                    case Element.Earth: return new Stun(entity);
                    case Element.Ice: return new Crystallise(entity);
                    case Element.Electric: return new Shock(entity);
                    case Element.None: return new Shock(entity);
                }
                break;
        }
        return null;
    }


    public static void ApplyElement(Element element, Entity entity, int stacks=1)
    {
        if (element==Element.None) return;
        Element activeElement = Element.None;

        foreach (Element ele in Enum.GetValues(typeof(Element)))
        {
            StatusEffect active = entity.FindStatus(GetStatus(ele));
            if (active!=null)
            {
                if (element==ele)
                {
                    active.AddStack(stacks);
                    active.RefreshDuration();
                    entity.UpdateStatus(active);
                    return;
                }
                activeElement = ele;
                stacks += active.stacks;
                active.RemoveStatus();
                break;
            }
        }

        StatusEffect status = GenStatus(element,activeElement,stacks,entity);

        if (status!=null) status.AddStatus();
    }





}

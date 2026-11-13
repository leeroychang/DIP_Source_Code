using System;
using UnityEngine;

[Serializable]
public class Mark : StatusEffect
{
    Reaction reaction;
    float dmg;
    GameObject markObject;

    public Mark() {}

    public Mark(Entity entity, float duration = 0, int stacks = 1) : base(entity, duration, stacks)
    {
        maxStacks = 1;
        maxDuration = 5;

        if (duration==0) this.duration = maxDuration;
    }

    public override void AddStatus()
    {
        base.AddStatus();
        reaction = new MarkReaction(this);
        EventManager.Subscribe(typeof(DamageEvent), reaction, Reaction.ReactionTiming.POST);
        markObject = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/MarkPrefab"),entity.transform);
    }

    public override void RemoveStatus()
    {
        if (dmg!=0)
        {
            DamageEvent dmgEvent = new DamageEvent(entity,dmg);
            EventManager.Queue(dmgEvent);
        }

        GameObject.Destroy(markObject);

        base.RemoveStatus();
        EventManager.Unsubscribe(typeof(DamageEvent), reaction, Reaction.ReactionTiming.POST);
    }

    public override StatusEffect Clone()
    {
        return new Mark(entity,duration,stacks);
    }


    public class MarkReaction : Reaction
    {
        Mark status;
        public MarkReaction(Mark status)
        {
            this.status = status;
        }

        public override void Call(GameEvent gameEvent)
        {
            DamageEvent dmgEvent = gameEvent as DamageEvent;
            Entity entity = gameEvent.target;
            if (dmgEvent==null || entity!=status.entity) return;
            if (dmgEvent.skill!=null && dmgEvent.skill.skillData.skillType.damageTypes.Contains(SkillType.DamageType.Summon))
                status.dmg += dmgEvent.dmg*0.5f; //Temp Formula
                if (status.duration>0.5f) status.duration = 0.5f;
            
        }
    }

}

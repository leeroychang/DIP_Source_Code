using UnityEngine;

public class Crystallise : StatusEffect
{
    Reaction reaction;
    public Crystallise(Entity entity, float duration = -1, int stacks = 1) : base(entity, duration, stacks)
    {
        maxStacks = 1;
        maxDuration = 5;
        if (duration==-1) this.duration = maxDuration;
        if (stacks>maxStacks) this.stacks = maxStacks;
    }

    public override void AddStatus()
    {
        base.AddStatus();
        reaction = new CrystalliseReaction(this);
        EventManager.Subscribe(typeof(DamageEvent), reaction, Reaction.ReactionTiming.POST);
    }

    public override void RemoveStatus()
    {
        base.RemoveStatus();
        EventManager.Unsubscribe(typeof(DamageEvent), reaction, Reaction.ReactionTiming.POST);
        reaction = null;
    }

    public override StatusEffect Clone()
    {
        return new Crystallise(entity,duration,stacks);
    }




    public class CrystalliseReaction : Reaction
    {
        Crystallise status;
        public CrystalliseReaction(Crystallise status)
        {
            this.status = status;
        }

        public override void Call(GameEvent gameEvent)
        {
            DamageEvent dmgEvent = gameEvent as DamageEvent;
            Entity entity = gameEvent.target;
            if (dmgEvent==null || entity!=status.entity) return;

            Debug.Log("Crystal Hit");

            for (int i=0; i<3; i++)
            {
                CrystalScript crystal = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/CrystallisePrefab")).GetComponent<CrystalScript>();
                crystal.transform.position = entity.transform.position;
                crystal.SetUp(!entity.ally);
            }
            status.RemoveStatus();
        }
    }


}

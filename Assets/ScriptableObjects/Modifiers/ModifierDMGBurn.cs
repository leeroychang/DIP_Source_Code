using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ModifierDMGBurn : Modifier
{

    [SerializeField] protected DMGOperation operation;
    [SerializeField] protected float val;

    public override void OnDisable(Skill skill, int slot)
    {
        if (!reactions.ContainsKey(skill) || !reactions[skill].ContainsKey(slot)) return;
        Reaction reaction = reactions[skill][slot];
        reactions[skill].Remove(slot);
        if (reactions[skill].Count==0) reactions.Remove(skill);
        EventManager.Unsubscribe(typeof(DamageEvent),reaction,Reaction.ReactionTiming.POST);
    }

    public override void OnEnable(Skill skill, int slot)
    {
        Reaction reaction = GetReaction(skill);
        if (!reactions.ContainsKey(skill)) reactions[skill] = new();
        reactions[skill][slot] = reaction;
        EventManager.Subscribe(typeof(DamageEvent),reaction,Reaction.ReactionTiming.POST);
    }

    Dictionary<Skill,Dictionary<int,Reaction>> reactions = new();

    public Reaction GetReaction(Skill skill)
    {
        return new ModifierDMGBurnReaction(skill,this);
    }


    public class ModifierDMGBurnReaction : Reaction
    {
        protected Skill skill;
        protected ModifierDMGBurn modifier;

        public ModifierDMGBurnReaction(Skill skill, ModifierDMGBurn modifier)
        {
            this.skill = skill;
            this.modifier = modifier;
        }

        public override void Call(GameEvent gameEvent)
        {
            DamageEvent dmgEvent = gameEvent as DamageEvent;
            if (dmgEvent==null || dmgEvent.skill==null || !(dmgEvent.skill==skill || dmgEvent.skill.baseSkill==skill)) return;

            DamageEvent newEvent = new DamageEvent(dmgEvent.target,DamageCalculation(dmgEvent, modifier.val,modifier.operation),0,1);
            EventManager.Queue(newEvent);

        }

        protected float DamageCalculation(DamageEvent dmgEvent, float val, DMGOperation operation)
        {
            int stacks = dmgEvent.target.GetStatusStacks();
            val *= stacks;
            switch (operation)
            {
                case DMGOperation.Add:
                    return val;
                case DMGOperation.Multi:
                    return dmgEvent.dmg*(1+val);
            }
            return 0;
        }
    }


    public enum DMGOperation
    {
        Add,
        Multi
    }
}

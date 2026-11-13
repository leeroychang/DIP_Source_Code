using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ModifierPEN : Modifier
{
    
    [SerializeField] DMGOperation operation;
    [SerializeField] float val;

    Dictionary<Skill,Dictionary<int,Reaction>> reactions = new();
    public override void OnDisable(Skill skill, int slot)
    {
        if (!reactions.ContainsKey(skill) || !reactions[skill].ContainsKey(slot)) return;
        Reaction reaction = reactions[skill][slot];
        reactions[skill].Remove(slot);
        if (reactions[skill].Count==0) reactions.Remove(skill);
        EventManager.Unsubscribe(typeof(DamageEvent),reaction,Reaction.ReactionTiming.PRE);
    }

    public override void OnEnable(Skill skill, int slot)
    {
        Reaction reaction = GetReaction(skill);
        if (!reactions.ContainsKey(skill)) reactions[skill] = new();
        reactions[skill][slot] = reaction;
        EventManager.Subscribe(typeof(DamageEvent),reaction,Reaction.ReactionTiming.PRE);
    }

    public virtual Reaction GetReaction(Skill skill)
    {
        return new ModifierPENReaction(skill,this);
    }


    [Serializable]
    public class ModifierPENReaction : Reaction
    {
        Skill skill;
        ModifierPEN modifier;

        public ModifierPENReaction(Skill skill, ModifierPEN modifier)
        {
            this.skill = skill;
            this.modifier = modifier;
        }

        public override void Call(GameEvent gameEvent)
        {
            DamageEvent dmgEvent = gameEvent as DamageEvent;
            if (dmgEvent==null || dmgEvent.skill==null || !(dmgEvent.skill==skill || dmgEvent.skill.baseSkill==skill)) return;

            Debug.Log("ARMOR PEN");

            if (modifier.operation==DMGOperation.Add) dmgEvent.ignoreDef += modifier.val;
            else dmgEvent.penpercent += modifier.val;

        }

    }





    public enum DMGOperation
    {
        Add,
        Multi
    }
}

using System;
using UnityEngine;

[Serializable]
public abstract class StatusEffect
{
    public Entity entity { get; protected set; }
    public float duration { get; protected set; }
    public float maxDuration { get; protected set; }
    public int stacks { get; protected set; }
    public int maxStacks { get; protected set; }
    public Skill skill { get; protected set; }

    public virtual Sprite icon { get { Debug.Log(GetType()); return Resources.Load<Sprite>("StatusIcons/"+GetType()); } }

    public StatusEffect() {}

    public StatusEffect(Entity entity, float duration=-1, int stacks=1)
    {
        this.entity = entity;
        this.duration = duration;
        maxDuration = duration;
        this.stacks = stacks;
        maxStacks = 1;
    }

    public StatusEffect(Entity entity, Skill skill, float duration=-1, int stacks=1)
    {
        this.entity = entity;
        this.duration = duration;
        maxDuration = duration;
        this.stacks = stacks;
        maxStacks = 1;
        this.skill = skill;
    }

    public bool Stackable() { return maxStacks!=1; }
    
    public virtual void AddStatus()
    {
        if (entity==null || entity.IsDown()) return;
        StatusEffect active = entity.FindStatus(GetType());
        if (active!=null)
        {
            if (Stackable())
            {
                active.AddStack(stacks);
                if (active.duration<duration) active.duration = duration;
            }
            else
            {
                entity.RemoveStatus(active);
                entity.AddStatus(this);
            }
        }
        else
        {
            entity.AddStatus(this);
        }
    }

    public virtual void RemoveStatus()
    {
        entity.RemoveStatus(this);
    }

    public virtual void AddStack(int val)
    {
        stacks += val;
        if (stacks>maxStacks) stacks = maxStacks;
        if (stacks<=0)
        {
            stacks = 0;
            RemoveStatus();
        }
    }

    public virtual void RefreshDuration()
    {
        duration = maxDuration;
    }

    public virtual bool TickUpdate(float time)
    {
        //Returns whether status should be removed
        duration -= time;
        if (duration>0) return false;
        RemoveStatus();
        return true;
    }

    public abstract StatusEffect Clone();

    public void ChangeEntity(Entity e, bool reapply)
    {
        if (!reapply)
        {
            entity = e;
            return;
        }

        if (entity!=null) RemoveStatus();
        entity = e;
        AddStatus();
    }

}

using UnityEngine;

public abstract class Reaction
{
    public abstract void Call(GameEvent gameEvent);

    public virtual void Unsubscribe() {}
    public virtual void Subscribe() {}

    public enum ReactionTiming
    {
        PRE,
        POST
    }
    
}

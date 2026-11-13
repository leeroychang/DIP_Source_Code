using UnityEngine;

public abstract class GameEvent
{
    public Entity caster;
    public Entity target;
    public Skill skill;

    public abstract void Call();

    
}

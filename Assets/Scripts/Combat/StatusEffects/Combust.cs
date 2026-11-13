using UnityEngine;

public class Combust : StatusEffect
{
    float radius = 5f;
    int baseDMG = 10;
    public Combust(Entity entity, float duration = -1, int stacks = 1) : base(entity, duration, stacks)
    {
        maxStacks = 5;
        maxDuration = 3;
        if (duration==-1) this.duration = maxDuration;
        if (stacks>maxStacks) this.stacks = maxStacks;
    }

    public override void RemoveStatus()
    {
        int dmg = baseDMG*stacks; //Temporary Calc

        Collider2D[] colls = Physics2D.OverlapCircleAll(entity.transform.position,radius);
        foreach (Collider2D coll in colls)
        {
            Entity e = coll.GetComponent<Entity>();
            if (e==null) e = coll.GetComponentInChildren<Entity>();
            if (e==null) continue;
            if (e.ally!=entity.ally) continue;

            DamageEvent dmgEvent = new DamageEvent(e,dmg);
            EventManager.Queue(dmgEvent);
        }

        GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/CombustPrefab")).transform.position = entity.transform.position;

        base.RemoveStatus();
    }

    public override StatusEffect Clone()
    {
        return new Combust(entity,duration,stacks);
    }

}

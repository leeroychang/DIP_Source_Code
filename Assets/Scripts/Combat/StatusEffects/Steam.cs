using UnityEngine;

public class Steam : StatusEffect
{

    float tickCD = 1f;
    float radius = 5f;
    float currentCD;
    int baseDMG = 10;

    public Steam(Entity entity, float duration = -1, int stacks = 1) : base(entity, duration, stacks)
    {
        maxStacks = 9;
        maxDuration = 5;

        if (duration==-1) this.duration = maxDuration;
    }

    public override bool TickUpdate(float time)
    {
        currentCD -= time;
        if (currentCD<=0) Tick();

        return base.TickUpdate(time);
    }

    public void Tick()
    {
        float dmg = baseDMG*stacks; //Temporary DMG formula
        Collider2D[] colls = Physics2D.OverlapCircleAll(entity.transform.position,radius);
        foreach (Collider2D coll in colls)
        {
            Entity e = coll.GetComponent<Entity>();
            if (e==null) e = coll.GetComponentInChildren<Entity>();
            if (e==null) continue;
            if (e.ally!=entity.ally) continue;

            DamageEvent dmgEvent = new DamageEvent(entity,dmg);
            EventManager.Queue(dmgEvent);
            if (Random.value<0.1f)
            {
                ElementEvent eleEvent = new ElementEvent(entity,Element.Fire);
                EventManager.Queue(eleEvent);
            }
            
        }

        GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/SteamPrefab")).transform.position = entity.transform.position;
        currentCD = tickCD;
    }

    public override StatusEffect Clone()
    {
        return new Steam(entity,duration,stacks);
    }
}

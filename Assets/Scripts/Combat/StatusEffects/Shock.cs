using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shock : StatusEffect
{

    float tickCD = 1f;
    float radius = 5f;
    float currentCD;
    int baseDMG = 10;

    public Shock(Entity entity, float duration=-1, int stacks = 1) : base(entity, duration, stacks)
    {
        maxStacks = 3;
        maxDuration = 5;

        if (duration==-1) this.duration = maxDuration;

        currentCD = tickCD;

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
        List<Entity> prevHits = new() {entity};
        Entity current = entity;

        DamageEvent dmgEvent = new DamageEvent(entity,dmg);
        EventManager.Queue(dmgEvent);

        for (int i=0; i<stacks; i++)
        {
            Vector2 currentPos = current.transform.position;
            Entity target = null;
            float targetDist = 0;
            Collider2D[] colls = Physics2D.OverlapCircleAll(entity.transform.position,radius);
            foreach (Collider2D coll in colls)
            {
                Entity e = coll.GetComponent<Entity>();
                if (e==null) e = coll.GetComponentInChildren<Entity>();
                if (e==null) continue;
                if (e.ally!=entity.ally || prevHits.Contains(e)) continue;

                float dist = Vector2.SqrMagnitude(currentPos-(Vector2)e.transform.position);
                if (target!=null && dist>=targetDist) continue;
                
                target = e;
                targetDist = dist;
            }
            if (target==null) break;
            Link(current,target,dmg);
            prevHits.Add(target);
            current = target;
        }
        

        Vector2 pos = entity.transform.position;
        
        currentCD = tickCD;
    }

    private void Link(Entity e1, Entity e2, float dmg)
    {
        Debug.Log($"Linking {e1.name} to {e2.name}");
        LineRenderAnimation line = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ShockPrefab")).GetComponent<LineRenderAnimation>();
        line.obj1 = e1.gameObject;
        line.obj2 = e2.gameObject;

        DamageEvent dmgEvent = new DamageEvent(e2,dmg);
        EventManager.Queue(dmgEvent);
    }

    public override StatusEffect Clone()
    {
        return new Shock(entity,duration,stacks);
    }


}

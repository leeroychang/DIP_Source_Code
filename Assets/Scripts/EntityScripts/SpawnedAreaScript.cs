using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnedAreaScript : MonoBehaviour
{
    float damage;
    Element element;
    float statusChance;
    float hitCD;
    Dictionary<Entity,float> hits = new();
    bool ally;

    Coroutine endCoroutine;


    public void SetUp(float damage, float duration, bool ally, Element element=Element.None, float statusChance=0, float hitCD=0)
    {
        this.damage = damage;
        this.hitCD = hitCD;
        this.ally = ally;
        this.element = element;
        this.statusChance = statusChance;

        endCoroutine = StartCoroutine(EndAttack(duration));
    }

    void FixedUpdate()
    {
        TickUpdate();
    }

    public virtual void TickUpdate()
    {
        float tick = Time.fixedDeltaTime;
        if (hitCD==0) return;
        foreach (Entity entity in hits.Keys.ToList())
        {
            if (hits[entity]==0) continue;
            hits[entity] -= tick;
            if (hits[entity]<=0) hits.Remove(entity);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Entity entity = other.gameObject.GetComponent<Entity>();
        EntityEnter(entity);
    }

    public void EntityEnter(Entity entity)
    {
        if (entity==null || ally==entity.ally || hits.ContainsKey(entity) || entity.IsDown()) return;
        OnHit(entity);
    }

    protected virtual void OnHit(Entity entity)
    {
        DamageEvent dmgEvent;
        dmgEvent = new DamageEvent(entity,damage);
        EventManager.Queue(dmgEvent);
        if (element!=Element.None)
        {
            if (Random.value<=statusChance)
            {
                ElementEvent elementEvent = new ElementEvent(entity,element);
                EventManager.Queue(elementEvent);
            }
        }
        hits[entity] = hitCD;
    }

    public IEnumerator EndAttack(float duration)
    {
        yield return new WaitForSeconds(duration);
        OnDestroy();
    }

    protected void OnDestroy()
    {
        if (endCoroutine!=null) StopCoroutine(endCoroutine);
        Destroy(gameObject);
    }

}

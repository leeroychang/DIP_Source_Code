using System.Collections.Generic;
using UnityEngine;

public class MirageScript : MonoBehaviour
{
    public Entity entity { get; private set; }
    bool inRange;
    public Entity clone { get; private set; }
    float timer;
    Reaction reaction;

    public void SetUp(float range, Skill skill, Entity entity, EntityData cloneData, GameObject clonePrefab)
    {
        transform.position = entity.transform.position;
        this.entity = entity;
        clone = Instantiate(clonePrefab,transform).GetComponent<Entity>();
        clone.Setup(cloneData,entity.ally);
        clone.dmgImmune = true;
        clone.GetComponent<CircleCollider2D>().enabled = false;
        inRange = true;
        timer = skill.skillStats[SkillStat.Duration];
        CircleCollider2D coll = GetComponent<CircleCollider2D>();
        coll.radius = range;

        reaction = new MirageReaction(this);
        EventManager.Subscribe(typeof(CastEvent), reaction, Reaction.ReactionTiming.POST);
    }

    public void FixedUpdate()
    {
        if (inRange) SetClonePos();
        timer -= Time.fixedDeltaTime;
        if (timer<=0) OnEnd();
    }

    private void SetClonePos()
    {
        Vector3 offset = entity.transform.position - transform.position;
        clone.transform.position = transform.position - offset;
    }

    private void OnEnd()
    {
        EventManager.Unsubscribe(typeof(CastEvent), reaction, Reaction.ReactionTiming.POST);
        Destroy(gameObject);
    }




    void OnTriggerEnter2D(Collider2D collision)
    {
        Entity e = collision.GetComponent<Entity>();
        if (e == null || entity!=e) return;
        inRange = true;
        clone.gameObject.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Entity e = collision.GetComponent<Entity>();
        if (e == null || entity!=e) return;
        inRange = false;
        clone.gameObject.SetActive(false);
    }


    public class MirageReaction : Reaction
    {
        MirageScript mirage;

        public MirageReaction(MirageScript mirage)
        {
            this.mirage = mirage;
        }

        public override void Call(GameEvent gameEvent)
        {
            CastEvent cast = gameEvent as CastEvent;
            if (!mirage.inRange || cast==null || cast.caster!=mirage.entity || cast.targetPos==mirage.entity.transform.position || !(cast.skill.skillData.skillType is SkillProjectile || cast.skill.skillData.skillType is SkillMelee || cast.skill.skillData.skillType is SkillAOE)) return;
            
            CastEvent newCast = new CastEvent(mirage.clone,cast.targetPos,cast.skill);
            EventManager.Queue(newCast);
        }
    }


}

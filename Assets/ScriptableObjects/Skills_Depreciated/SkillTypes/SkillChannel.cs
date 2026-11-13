using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class SkillChannel : SkillType
{

    [SerializeField] SkillData castData;
    [SerializeField] protected float duration;
    [SerializeField] protected float interval; // Interval between each cast (0 for only casting on release)

    protected Entity caster;
    protected Coroutine channelCoroutine;
    protected Skill cast;
    protected Vector3 targetPos;

    protected float timer;

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        caster = entity;
        if (castData!=null) cast = new Skill(castData)
        {
            baseSkill = skill
        };
        cast.statModifier = skill.statModifier;
        this.targetPos = targetPos;
        channelCoroutine = caster.StartCoroutine(Channel());
    }

    public virtual IEnumerator Channel()
    {
        caster.StartAnimation(duration);
        timer = duration;
        if (interval!=0)
        {
            do
            {
                if (cast!=null) Cast();
                yield return new WaitForSeconds(interval);
                timer -= interval;
            } while (timer>interval);
        }
        yield return new WaitForSeconds(timer);
        OnEnd();
    }

    

    public virtual void Cast()
    {
        cast.OnUse(caster,targetPos,true);
    }

    public virtual void OnEnd()
    {
        if (channelCoroutine!=null) caster.StopCoroutine(channelCoroutine);
        if (interval==0) Cast();
        channelCoroutine = null;
        caster.resetAnimation();
    }

    public override void OnRelease(Entity entity, Vector2? dir=null)
    {
        if (channelCoroutine!=null) OnEnd();
    }


}

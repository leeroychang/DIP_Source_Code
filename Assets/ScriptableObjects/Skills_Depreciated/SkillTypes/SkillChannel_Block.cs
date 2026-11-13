using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class SkillChannel_Block : SkillChannel
{
    [SerializeField] float blockRatio;
    [SerializeField] float parryDuration;
    [SerializeField] float intervalMulti; // Bonus multiplier per second
    [SerializeField] float maxMulti = 0; // Max multiplier
    [SerializeField] GameObject BlockPrefab;

    protected float multi; // Current multiplier
    BlockReaction reaction;



    public bool parry { get; private set; }

    public override IEnumerator Channel()
    {
        caster.StartAnimation(duration);
        timer = duration;
        parry = true;
        multi = 0;
        reaction = new BlockReaction(this,caster);
        EventManager.Subscribe(typeof(DamageEvent),reaction,Reaction.ReactionTiming.PRE);
        if (interval!=0)
        {
            if (parryDuration<interval)
            {
                yield return new WaitForSeconds(parryDuration);
                parry = false;
                yield return new WaitForSeconds(interval-parryDuration);
                timer -= interval;
                multi += intervalMulti;
                if (multi>maxMulti) multi = maxMulti;
            }
            while (timer>interval)
            {
                yield return new WaitForSeconds(interval);
                timer -= interval;
                multi += intervalMulti;
                if (multi>maxMulti) multi = maxMulti;
                if (parry && parryDuration-(duration-timer) <= interval)
                {
                    float timeLeft = parryDuration-(duration-timer);
                    yield return new WaitForSeconds(timeLeft);
                    parry = false;
                    yield return new WaitForSeconds(interval-timeLeft);
                }
            }
        }
        else
        {
            yield return new WaitForSeconds(parryDuration);
            parry = false;
            timer -= parryDuration;
        }
        yield return new WaitForSeconds(timer);
        OnEnd();
    }

    public override void OnEnd()
    {
        if (channelCoroutine!=null) caster.StopCoroutine(channelCoroutine);
        channelCoroutine = null;
        EventManager.Unsubscribe(typeof(DamageEvent),reaction,Reaction.ReactionTiming.PRE);
        Cast();
        caster.resetAnimation();
    }

    public override void Cast()
    {
        float val = cast.statModifier[SkillStat.Damage];
        cast.statModifier[SkillStat.Damage] = (val+1f)*multi;
        cast.OnUse(caster,targetPos);
        caster.StartCoroutine(RemoveMultiplier(val));
    }

    public IEnumerator RemoveMultiplier(float baseValue)
    {
        float m = multi;
        yield return new WaitForSeconds(0.1f);
        float t = cast.statModifier[SkillStat.Damage];
        cast.statModifier[SkillStat.Damage] = baseValue;
    }


    public class BlockReaction : Reaction
    {
        SkillChannel_Block skillType;
        Entity entity;
        GameObject blockPrefab;

        public BlockReaction(SkillChannel_Block skillType, Entity entity)
        {
            this.skillType = skillType;
            this.entity = entity;
        }

        public override void Call(GameEvent gameEvent)
        {
            DamageEvent dmgEvent = gameEvent as DamageEvent;
            if (dmgEvent==null|| dmgEvent.target!=entity) return;
            if (skillType.parry)
            {
                skillType.multi = skillType.maxMulti;
                dmgEvent.dmg = 0;
            }
            else dmgEvent.dmg *= 1f-skillType.blockRatio;

        }

        public override void Subscribe()
        {
            if (blockPrefab!=null) GameObject.Destroy(blockPrefab);
            if (skillType.BlockPrefab!=null) blockPrefab = GameObject.Instantiate(skillType.BlockPrefab,entity.transform);
        }

        public override void Unsubscribe()
        {
            if (blockPrefab!=null) GameObject.Destroy(blockPrefab);
        }
    }


}

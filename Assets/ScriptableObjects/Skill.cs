using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public SkillData skillData;

    private Dictionary<SkillStat,float> _skillStats; //Allows modifiable stats
    public Dictionary<SkillStat,float> statModifier;
    public Dictionary<SkillStat,float> skillStats
    {
        get
        {
            Dictionary<SkillStat,float> stats = new();
            foreach (KeyValuePair<SkillStat,float> stat in _skillStats) stats[stat.Key] = stat.Value*(1+statModifier[stat.Key]);
            if (statModifier[SkillStat.Cooldown]<-0.8f) stats[SkillStat.Cooldown] = _skillStats[SkillStat.Cooldown]*0.2f;
            return stats;
        }

        set { return; }
    }

    public float GetStat(SkillStat stat, float modifierRatio=1)
    {
        if (_skillStats.ContainsKey(stat)) return _skillStats[stat]*(1+statModifier[stat]*modifierRatio);
        return 0;
    }

    public int charges { get; private set; } //Current Skill Charges
    public float cooldown { get; private set; } //Current Skill Cooldown

    public Upgrade[] upgradeSlots;

    public Skill baseSkill;
    
    public Skill(SkillData skillData)
    {
        this.skillData = skillData;
        _skillStats = skillData.skillType.GetStats();
        statModifier = new();
        foreach (SkillStat stat in Enum.GetValues(typeof(SkillStat))) statModifier[stat] = 0;
        charges = (int)skillStats[SkillStat.Charge];
        cooldown = 0;
        upgradeSlots = new Upgrade[3];
    }

    public virtual bool OnUse(Entity entity, Vector3 targetPos, bool bypass=false)
    {
        if (bypass)
        {
            skillData.skillType.OnUse(this, entity, targetPos);
            return true;
        }
        if (charges==0) return false;
        if (cooldown==0) cooldown = skillStats[SkillStat.Cooldown];
        charges -= 1;

        entity.TriggerAnimation(skillData.altAtk ? "AttackAlt" : "Attack",skillData.skillType.aniTime, targetPos-entity.transform.position);
        entity.StartAnimation(skillData.skillType.aniTime);
        if (entity.skillCoroutine!=null)
        {
            entity.StopCoroutine(entity.skillCoroutine);
            entity.skillCoroutine = null;
        }
        entity.skillCoroutine = entity.StartCoroutine(castCoroutine(entity,targetPos));
        return true;
    }

    public IEnumerator castCoroutine(Entity entity, Vector3 targetPos)
    {

        yield return new WaitForSeconds(skillData.skillType.castTime);

        CastEvent cast = new CastEvent(entity,targetPos,this);
        EventManager.Queue(cast);
        yield break;
    }

    public virtual void OnRelease(Entity entity, Vector2? dir=null) { skillData.skillType.OnRelease(entity,dir); }

    public void CDTick(float time)
    {
        if (cooldown==0) return; //CD won't be 0 for non-max charge
        cooldown -= time;
        if (cooldown<=0)
        {
            charges++;
            if (charges==skillStats[SkillStat.Charge]) cooldown = 0;
            else cooldown = skillStats[SkillStat.Cooldown];
        }
    }

    public void ReplaceUpgrade(int slot, Upgrade upgrade)
    {
        if (slot<0 || slot>=3) return;

        if (upgradeSlots[slot]!=null)
        {
            //Remove old upgrade modifiers
            upgradeSlots[slot].OnDisable(this,slot);


        }
        
        if (upgrade!=null)
        {
            //Add new upgrade modifiers
            upgrade.OnEnable(this,slot);

        }

        upgradeSlots[slot] = upgrade;
    }

    public void InheritUpgrade(List<Upgrade> upgrades)
    {
        if (upgrades==null || upgrades.Count==0) return;
        foreach (Upgrade upgrade in upgrades) upgrade.OnEnable(this,-1);
    }


    public void ResetCooldown() { cooldown = skillStats[SkillStat.Cooldown]; }


}

using System.Collections.Generic;
using UnityEngine;

public class DamageEvent : GameEvent
{

    public float dmg;
    public float ignoreDef;
    public float penpercent;
    public float knockback;
    public Vector2 dir;

    public DamageEvent(Entity caster, Entity target, Skill skill)
    {
        this.caster = caster;
        this.target = target;
        this.skill = skill;
        dmg = skill.skillStats[SkillStat.Damage];
    }

    public DamageEvent(Entity target, float dmg, float ignoreDef=0, float penpercent=0f)
    {
        this.target = target;
        this.dmg = dmg;
        this.ignoreDef = ignoreDef;
        this.penpercent = penpercent;
    }

    public DamageEvent(Entity caster, Entity target, Skill skill, Vector2 dir)
    {
        this.skill = skill;
        Dictionary<SkillStat,float> stats = skill.skillStats;
        this.caster = caster;
        this.target = target;
        dmg = stats[SkillStat.Damage];
        knockback = stats[SkillStat.Knockback];
        this.dir = dir;
    }

    // public DamageEvent(Entity caster, Entity target, Dictionary<SkillStat,float> skillStats)
    // {
    //     this.caster = caster;
    //     this.target = target;
    //     dmg = skillStats[SkillStat.Damage];
    // }

    public override void Call()
    {
        target.Damaged((int)dmg,(int)ignoreDef,penpercent);
        if (knockback!=0 && dir!=Vector2.zero) target.Knockback(knockback*10,0.1f,dir,true);
    }
}

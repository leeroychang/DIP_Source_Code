using System.Collections.Generic;
using UnityEngine;

public class FollowAttackScript : AttackScript
{
    public Entity target;

    public override void SetUp(Skill skill, Entity caster, Vector2 targetDir, Vector2? moveDir = null, float hitCD=0)
    {
        skillStats = new Dictionary<SkillStat,float>(skill.skillStats);
        this.skill = skill;
        
        this.caster = caster;
        dir = moveDir;
        speed = skillStats.ContainsKey(SkillStat.ProjectileSpeed) ? skillStats[SkillStat.ProjectileSpeed] : 0;
        
        hits = new();
        this.hitCD = hitCD;
        endCoroutine = StartCoroutine(endAttack(skillStats[SkillStat.Duration]));

        transform.right = Vector2.right;
        dir = null;
    }

    void FixedUpdate()
    {
        transform.position = target.transform.position;
        TickUpdate();
    }



    private void OnTriggerStay2D(Collider2D other)
    {
        Entity entity = other.gameObject.GetComponent<Entity>();
        EntityEnter(entity);
    }





}

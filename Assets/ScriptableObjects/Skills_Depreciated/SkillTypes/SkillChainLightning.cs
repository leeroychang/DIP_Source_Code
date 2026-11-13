using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class SkillChainLightning : SkillAOE_Cone
{
    public override void SpawnProjectile(Skill skill, Vector2 targetDir, Entity entity)
    {
        Dictionary<SkillStat,float> stats = skill.skillStats;
        targetDir = targetDir.normalized;
        Vector3 basePos = entity.transform.position+(Vector3)targetDir*math.clamp(targetDir.magnitude,minDistance,maxDistance);

        List<Entity> entities = new();
        Collider2D[] colls = Physics2D.OverlapCircleAll(basePos, radius);

        foreach (Collider2D coll in colls) if (coll.TryGetComponent(out Entity e) && e.ally!=entity.ally && CheckAngle(targetDir,angle,e.transform.position,basePos)) entities.Add(e);
        
        entities.OrderBy(e => e.transform.position-basePos);

        Entity currentE = entity;
        foreach (Entity e in entities)
        {
            Link(currentE,e,stats[SkillStat.Damage]);
            currentE = e;
        }
    }

    public static bool CheckAngle(Vector2 dir, float angle, Vector2 target, Vector2 root)
    {
        Vector2 targetDir = target-root;
        return Vector2.Angle(targetDir,dir)<=angle;
    }

    private void Link(Entity e1, Entity e2, float dmg)
    {
        LineRenderAnimation line = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ShockPrefab")).GetComponent<LineRenderAnimation>();
        line.obj1 = e1.gameObject;
        line.obj2 = e2.gameObject;

        DamageEvent dmgEvent = new DamageEvent(e2,dmg);
        EventManager.Queue(dmgEvent);
    }
}

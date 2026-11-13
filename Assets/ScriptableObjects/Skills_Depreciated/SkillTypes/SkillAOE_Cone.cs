using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class SkillAOE_Cone : SkillAOE
{
    [SerializeField] protected float angle;

    public override void SpawnProjectile(Skill skill, Vector2 targetDir, Entity entity)
    {
        GameObject attack = GameObject.Instantiate(attackPrefab);
        AttackScript attackScript = attack.AddComponent<AttackScript>();
        ConeCollider2D cone = attack.AddComponent<ConeCollider2D>();
        cone.SetUp(attackScript,angle,radius);
        
        
        attack.transform.position = entity.transform.position+(Vector3)targetDir.normalized*math.clamp(targetDir.magnitude,minDistance,maxDistance);
        attackScript.SetUp(skill,entity,targetDir.normalized,targetDir.normalized,hitCD);
        attackScript.knockbackCenter = true;
    }
}

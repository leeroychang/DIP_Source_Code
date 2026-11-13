using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class SkillAOE_Follow : SkillAOE
{
    public override void SpawnProjectile(Skill skill, Vector2 targetDir, Entity entity)
    {
        GameObject attack = GameObject.Instantiate(attackPrefab);
        FollowAttackScript attackScript = attack.AddComponent<FollowAttackScript>();
        attack.transform.position = entity.transform.position+(Vector3)targetDir.normalized*math.clamp(targetDir.magnitude,minDistance,maxDistance);
        attackScript.SetUp(skill,entity,targetDir.normalized,targetDir.normalized,hitCD);
        attackScript.knockbackCenter = true;
        if (attack.TryGetComponent(out CircleCollider2D coll))
        {
            coll.radius = radius;
            Debug.Log($"Set radius to {radius}");
        }
        attackScript.target = entity;
    }
}

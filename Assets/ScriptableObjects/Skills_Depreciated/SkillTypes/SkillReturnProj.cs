using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class SkillReturnProj : SkillProjectile
{

    public override void SpawnProjectile(Skill skill, Vector2 targetDir, Entity entity)
    {
        GameObject attack = GameObject.Instantiate(attackPrefab);
        ReturnAttackScript attackScript = attack.AddComponent<ReturnAttackScript>();
        attack.transform.position = entity.transform.position+(Vector3)targetDir.normalized*math.clamp(targetDir.magnitude,minDistance,maxDistance);
        attackScript.SetUp(skill,entity,targetDir.normalized,targetDir.normalized,hitCD);
    }

}

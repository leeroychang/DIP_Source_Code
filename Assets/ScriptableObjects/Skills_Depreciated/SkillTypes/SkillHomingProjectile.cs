using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class SkillHomingProjectile : SkillProjectile
{
    [SerializeField] float homingRange = 5;
    [SerializeField] float homingStrength = 1;

    public override void SpawnProjectile(Skill skill, Vector2 targetDir, Entity entity)
    {
        GameObject attack = GameObject.Instantiate(attackPrefab);
        HomingAttackScript attackScript = attack.AddComponent<HomingAttackScript>();
        attack.transform.position = entity.transform.position+(Vector3)targetDir.normalized*math.clamp(targetDir.magnitude,minDistance,maxDistance);
        attackScript.SetUp(skill,entity,targetDir.normalized,targetDir.normalized);
        attackScript.HomingSetUp(homingRange,homingStrength);
    }
}

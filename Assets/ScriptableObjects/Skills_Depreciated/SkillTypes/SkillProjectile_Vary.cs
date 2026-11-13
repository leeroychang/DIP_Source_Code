using System;
using UnityEngine;

[Serializable]
public class SkillProjectile_Vary : SkillProjectile
{
    [SerializeField] float angleVariance;

    public override void SpawnProjectile(Skill skill, Vector2 targetDir, Entity entity)
    {
        float a = angleVariance/2;
        targetDir = Quaternion.Euler(0,0,UnityEngine.Random.Range(-a,a))*targetDir;
        base.SpawnProjectile(skill, targetDir, entity);
    }
}

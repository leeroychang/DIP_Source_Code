using System;
using SerializeReferenceEditor;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class SkillProjectile_OnHit : SkillProjectile
{
    [SerializeField] GameObject OnHitPrefab;
    [SerializeReference][SR] StatusEffect status;
    [SerializeField] bool HitOnDestroy;

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        base.OnUse(skill, entity, targetPos);
    }

    public override void SpawnProjectile(Skill skill, Vector2 targetDir, Entity entity)
    {
        GameObject attack = GameObject.Instantiate(attackPrefab);
        AttackScript_OnHit attackScript = attack.AddComponent<AttackScript_OnHit>();
        attack.transform.position = entity.transform.position+(Vector3)targetDir.normalized*math.clamp(targetDir.magnitude,minDistance,maxDistance);
        attackScript.SetUp(skill,entity,targetDir.normalized,targetDir.normalized,hitCD);
        attackScript.OnHitPrefab = OnHitPrefab;
        attackScript.OnHitStatus = status;
        attackScript.HitOnDestroy = HitOnDestroy;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnchorSkillType : SkillType
{
    [Header("Anchor Settings")]
    [SerializeField] private GameObject anchorPrefab;
    [SerializeField] private AnimationClip throwAnimationClip;
    [SerializeField] private float flySpeed = 14f;
    [SerializeField] private float pullSpeed = 16f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float spawnOffset = 0.8f;
    [SerializeField] float damage;

    [Header("Aim Settings")]
    [SerializeField] private bool aimAtTarget = true;
    [SerializeField] private bool useUpAsFacing = false;

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        if (entity == null || anchorPrefab == null) return;

        Dictionary<SkillStat, float> stats = skill.skillStats;

        // 1. Animation
        // Animator anim = entity.GetComponentInChildren<Animator>();
        // if (anim != null)
        // {
        //     if (throwAnimationClip != null)
        //         anim.Play(throwAnimationClip.name);
        //     else
        //         anim.SetTrigger("Throw");
        // }

        // 2. Trajectory Direction
        Vector2 origin = entity.transform.position;
        Vector2 direction;

        if (aimAtTarget && targetPos != Vector3.zero)
        {
            direction = ((Vector2)targetPos - origin).normalized;
        }
        else
        {
            direction = useUpAsFacing ? (Vector2)entity.transform.up : (Vector2)entity.transform.right;
        }

        if (direction == Vector2.zero) direction = Vector2.right;

        // 3. Spawn Projectile
        Vector2 spawnPos = origin + (direction * spawnOffset);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject anchorObj = UnityEngine.Object.Instantiate(anchorPrefab, spawnPos, rotation);

        // float animDuration = throwAnimationClip != null ? throwAnimationClip.length : 0.2f;
        entity.resetAnimation(true);
        entity.StartAnimation(1.5f, false, false, 0f, Vector2.zero);

        if (anchorObj.TryGetComponent<AnchorProjectile>(out var anchor))
        {
            anchor.Initialize(entity, direction, flySpeed, pullSpeed, maxDistance, (int)stats[SkillStat.Damage]);
        }
        else
        {
            UnityEngine.Object.Destroy(anchorObj, maxDistance / flySpeed);
        }
    }

    public override Dictionary<SkillStat, float> GetStats()
    {
        Dictionary<SkillStat, float> stats = base.GetStats();
        stats[SkillStat.Damage] = damage;
        return stats;
    }
}
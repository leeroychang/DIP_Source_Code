using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChakramThrowSkillType : SkillType
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject chakramPrefab;
    [SerializeField] private AnimationClip throwAnimationClip;
    [SerializeField] private float flySpeed = 12f;
    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private float spawnOffset = 0.8f;

    [Header("Aim Settings")]
    [SerializeField] private bool aimAtTarget = true;
    [SerializeField] private bool useUpAsFacing = false;

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        if (entity == null || chakramPrefab == null) return;

        Dictionary<SkillStat, float> stats = skill.skillStats;

        Animator anim = entity.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            if (throwAnimationClip != null)
                anim.Play(throwAnimationClip.name);
            else
                anim.SetTrigger("Throw");
        }

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

        Vector2 spawnPos = origin + (direction * spawnOffset);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject projectileObj = UnityEngine.Object.Instantiate(chakramPrefab, spawnPos, rotation);

        float animDuration = throwAnimationClip != null ? throwAnimationClip.length : 0.2f;
        entity.StartAnimation(animDuration, true, false, 0f, Vector2.zero);

        int attackDamage = 10;
        if (stats != null)
        {
            foreach (var stat in stats)
            {
                if (stat.Key.ToString().Contains("ATK") || stat.Key.ToString().Contains("Damage"))
                {
                    attackDamage = Mathf.RoundToInt(stat.Value);
                    break;
                }
            }
        }

        if (projectileObj.TryGetComponent<ChakramProjectile>(out var projectile))
        {
            projectile.Initialize(entity, direction, flySpeed, maxDistance, attackDamage);
        }
        else
        {
            float fallbackLifetime = maxDistance / (flySpeed > 0 ? flySpeed : 1f);
            UnityEngine.Object.Destroy(projectileObj, fallbackLifetime);
        }
    }
}
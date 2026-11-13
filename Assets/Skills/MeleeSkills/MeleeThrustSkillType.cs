using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MeleeThrustSkillType : SkillType
{
    [Header("Thrust Settings")]
    [SerializeField] private AnimationClip thrustAnimationClip;
    [SerializeField] private float thrustRange = 2.0f;
    [SerializeField] private float thrustWidth = 1.0f;
    [SerializeField] private float animLockDuration = 0.3f;

    [Header("Aim Settings")]
    [SerializeField] private bool aimAtTarget = true;
    [SerializeField] private bool useUpAsFacing = false;

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        if (entity == null) return;

        Dictionary<SkillStat, float> stats = skill.skillStats;

        Animator anim = entity.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            if (thrustAnimationClip != null)
                anim.Play(thrustAnimationClip.name);
            else
                anim.SetTrigger("Thrust");
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

        float duration = thrustAnimationClip != null ? thrustAnimationClip.length : animLockDuration;
        entity.StartAnimation(duration, true, false, 0f, Vector2.zero);

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

        Vector2 boxCenter = origin + (direction * (thrustRange * 0.5f));
        Vector2 boxSize = new Vector2(thrustRange, thrustWidth);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, angle);

        foreach (var hit in hits)
        {
            if (hit.gameObject == entity.gameObject) continue;

            if (hit.TryGetComponent<Entity>(out var targetEntity))
            {
                targetEntity.Damaged(attackDamage);
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class SkillHitbox : MonoBehaviour
{
    private Entity owner;
    private int damage;
    private int pierceCount;
    private readonly HashSet<Entity> hitEntities = new HashSet<Entity>();

    public void Initialize(Entity owner, int damage, int pierce)
    {
        this.owner = owner;
        this.damage = damage;
        this.pierceCount = pierce;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Avoid self-damage or re-hitting targets in the same thrust
        if (!other.TryGetComponent<Entity>(out var targetEntity))
        {
            targetEntity = other.GetComponentInParent<Entity>();
        }

        if (targetEntity == null || targetEntity == owner || hitEntities.Contains(targetEntity)) return;

        // Apply damage & log hit
        hitEntities.Add(targetEntity);
        targetEntity.Damaged(damage);

        // Check pierce limit
        pierceCount--;
        if (pierceCount <= 0)
        {
            Destroy(gameObject);
        }
    }

    // Called via Unity Animation Event on the final frame of your clip
    public void OnAnimationFinished()
    {
        Destroy(gameObject);
    }
}
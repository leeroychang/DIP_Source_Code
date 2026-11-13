using System;
using UnityEngine;

[Serializable]
public class BossSwipe : SkillAOE
{
    [SerializeField] float indicatorTime = 2f;
    [SerializeField] Vector2 swipePos;

        public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
        {
            GameObject attack = GameObject.Instantiate(attackPrefab);
            attack.transform.position = swipePos;

            // Fix rigidbody for 2D top-down
            if (attack.TryGetComponent(out Rigidbody2D rb))
                rb.gravityScale = 0f;

            // Kick off cast time on the indicator — enables collider + AttackScript after castTime
            if (attack.TryGetComponent(out BossAOEIndicator indicator))
                indicator.Activate(indicatorTime, () =>
                {
                    if (attack == null) return;
                    AttackScript attackScript = attack.AddComponent<AttackScript>();
                    attackScript.SetUp(skill, entity, Vector2.zero, null, hitCD);
                    if (attack.TryGetComponent(out Collider2D c)) c.enabled = true;
                });
            else
                Debug.LogWarning("SkillType_BossCircle: BossAOEIndicator not found on attackPrefab!");
        }

}

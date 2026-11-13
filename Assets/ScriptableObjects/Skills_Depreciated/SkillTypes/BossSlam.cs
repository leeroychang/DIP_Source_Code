using System;
using UnityEngine;

[Serializable]
public class BossSlam : SkillAOE
{
      [SerializeField] float startX;
      [SerializeField] float fixedY;

      float currentX;
      bool moveLeft;

      public void StartAttack(bool moveLeft)
      {
          currentX = startX;
          this.moveLeft = moveLeft;
      }

      public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
      {
          entity.transform.position = new Vector3(currentX, fixedY,
  entity.transform.position.z);
          base.OnUse(skill, entity, targetPos);

          currentX += (moveLeft ? -1 : 1) * GetPrefabWidth();
      }

      float GetPrefabWidth()
      {
          if (attackPrefab.TryGetComponent(out SpriteRenderer sr) && sr.sprite != null)
              return sr.sprite.bounds.size.x;
          if (attackPrefab.TryGetComponent(out BoxCollider2D box))
              return box.size.x;
          return 1f;
      }

}

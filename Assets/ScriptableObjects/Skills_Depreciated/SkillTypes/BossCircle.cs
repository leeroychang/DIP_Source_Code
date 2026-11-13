using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class SkillType_BossCircle : SkillAOE
{
    [Header("Boss Circle")]
    [SerializeField] float indicatorTime = 1f;
    [SerializeField] float minOffset = 1f;
    [SerializeField] float maxOffset = 4f;
    [SerializeField] int casts = 10;
    [SerializeField] float interval = 0.1f;
    [SerializeField] Tilemap mapBoundsTilemap;

    Dictionary<Entity,Coroutine> castCoroutines = new();

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        // Grab map bounds from assigned tilemap, fallback to default 50x50
        // Bounds mapBounds = mapBoundsTilemap != null
        //     ? mapBoundsTilemap.localBounds
        //     : new Bounds(new Vector3(-15f,-9f,0f), new Vector3(15f, 0f, 0f));

        Bounds mapBounds = new Bounds(new Vector3(0f,-4f,0f), new Vector3(30f, 9f, 0f));
        if (castCoroutines.ContainsKey(entity))
        {
            entity.StopCoroutine(castCoroutines[entity]);
            castCoroutines.Remove(entity);
        }
        castCoroutines[entity] = entity.StartCoroutine(Cast(skill,entity,targetPos,mapBounds));
    }

    private Bounds GetLocalBounds(Bounds mapBounds, Vector2 targetPos)
    {
        float minX = targetPos.x-maxOffset;
        float maxX = targetPos.x+maxOffset;
        float minY = targetPos.y-maxOffset;
        float maxY = targetPos.y+maxOffset;

        minX = minX<mapBounds.min.x ? mapBounds.min.x : minX;
        maxX = maxX>mapBounds.max.x ? mapBounds.max.x : maxX;
        minY = minY<mapBounds.min.y ? mapBounds.min.y : minY;
        maxY = maxY>mapBounds.max.y ? mapBounds.max.y : maxY;

        return new Bounds (
            new Vector2((minX+maxX)/2f,(minY+maxY)/2f),
            new Vector2(Mathf.Abs(maxX-minX)/2f,Mathf.Abs(maxY-minY)/2f)
        );
    }


    private IEnumerator Cast(Skill skill, Entity entity, Vector3 targetPos, Bounds mapBounds)
    {
        Bounds bounds = GetLocalBounds(mapBounds,targetPos);
        
        for (int i=0; i<casts; i++)
        {
            Vector3 spawnPos = new Vector2(Random.Range(bounds.min.x,bounds.max.x), Random.Range(bounds.min.y,bounds.max.y));

            GameObject attack = GameObject.Instantiate(attackPrefab);
            attack.transform.position = spawnPos;

            // Set collider radius
            if (attack.TryGetComponent(out CircleCollider2D coll))
            {
                coll.radius    = radius;
                coll.isTrigger = true;
                coll.enabled   = false; // disabled during cast time
            }

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
                    if (attack.TryGetComponent(out CircleCollider2D c)) c.enabled = true;
                });
            else
                Debug.LogWarning("SkillType_BossCircle: BossAOEIndicator not found on attackPrefab!");

            yield return new WaitForSeconds(interval);
        }
    }
}

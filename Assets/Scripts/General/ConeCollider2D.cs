using System.Collections.Generic;
using UnityEngine;

public class ConeCollider2D : MonoBehaviour
{
    public AttackScript attackScript;
    float angle;
    float range;
    
    public static bool CheckAngle(Vector2 dir, float angle, Vector2 target, Vector2 root)
    {
        Vector2 targetDir = target-root;
        return Vector2.Angle(targetDir,dir)<=angle;
    }

    public static List<Entity> GetEntities(Vector2 dir, float angle, float dist, Vector2 root)
    {
        List<Entity> entities = new();
        Collider2D[] colls = Physics2D.OverlapCircleAll(root, dist);

        foreach (Collider2D coll in colls) if (coll.TryGetComponent(out Entity e) && CheckAngle(dir,angle,e.transform.position,root)) entities.Add(e);
        return entities;
    }

    public void SetUp(AttackScript attack, float angle, float range)
    {
        attackScript = attack;
        this.angle = angle;
        this.range = range;
    }

    void FixedUpdate()
    {
        List<Entity> entities = GetEntities(transform.right,angle,range, transform.position);
        foreach(Entity e in entities) attackScript.EntityEnter(e);
    }



}

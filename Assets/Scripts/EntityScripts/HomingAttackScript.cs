using System.Linq;
using UnityEngine;

public class HomingAttackScript : AttackScript
{

    Entity target;
    float homingRange = 5;
    float homingStrength = 1;
    float baseAngle = 15;
    bool isAlly;

    void FixedUpdate()
    {
        UpdateTargetDir();
        if (dir==null) return;
        transform.right = dir ?? Vector2.right;
        transform.Translate(Vector2.right*Time.fixedDeltaTime*speed);
    }

    public void HomingSetUp(float homingRange, float homingStrength)
    {
        this.homingRange = homingRange;
        this.homingStrength = homingStrength;
    }

    private void UpdateTargetDir()
    {
        if (target!=null)
            if (Vector2.Angle(dir ?? target.transform.position-transform.position, target.transform.position-transform.position)>baseAngle*homingStrength*homingRange*speed)
                target = null;
        
        if (target==null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position,homingRange);
            if (hits.Length==0) return;
            Vector3 pos = transform.position;
            hits = hits.OrderBy(hit => (pos-hit.transform.position).sqrMagnitude).ToArray();
            foreach (Collider2D hit in hits)
            {
                Entity e = hit.GetComponent<Entity>();
                if (e==null) e = hit.GetComponentInChildren<Entity>();
                if (e==null || e.ally==caster.ally) continue;

                float angle = Vector2.Angle(dir ?? e.transform.position-pos, e.transform.position-pos);
                if (angle>baseAngle*homingStrength*homingRange/speed) continue;

                target = e;
                break;
            }
            if (target==null) return;
        }

        Vector2 targetDir = (target.transform.position - transform.position).normalized;
        if (dir==null)
        {
            dir = targetDir;
            return;
        }

        Vector2 current = dir ?? targetDir;

        float signedAngle = Vector2.SignedAngle(dir ?? targetDir,targetDir) * Mathf.Deg2Rad;
        float stepAngle = Mathf.MoveTowardsAngle(0,signedAngle,baseAngle*homingStrength*Time.fixedDeltaTime*Mathf.Deg2Rad);

        float cos = Mathf.Cos(stepAngle);
        float sin = Mathf.Sin(stepAngle);
        
        dir = new Vector2(
            current.x * cos - current.y * sin,
            current.x * sin + current.y * cos
        );


    }
    
}

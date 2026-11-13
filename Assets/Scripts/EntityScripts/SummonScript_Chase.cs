using System.Linq;
using UnityEngine;

public class SummonScript_Chase : SummonScript_Follow
{
    public Entity chaseEntity;

    void FixedUpdate()
    {
        if (rb.linearVelocityX<0 && !isFlipped || rb.linearVelocityX>0 && isFlipped) Flip();
        if (agent.desiredVelocity.x<0 && !isFlipped || agent.desiredVelocity.x>0 && isFlipped) Flip();

        TickUpdate();
        if (aniLock==true || agent==null || followEntity==null) return;
        Vector3 targetPos = followEntity.transform.position+followOffset;
        float dist = Vector2.SqrMagnitude(transform.position-targetPos);

        if (dist>100)
        {
            transform.position = targetPos;
            chaseEntity = null;
            return;
        }

        if (dist>400)
        {
            chaseEntity = null;
            agent.SetDestination(targetPos);
        }
        else
        {
            Vector3 pos = transform.position;
            if (chaseEntity==null || chaseEntity.IsDown() || (pos-chaseEntity.transform.position).sqrMagnitude>400)
            {
                chaseEntity=null;
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position,20);
                if (hits.Length==0) return;
                hits = hits.OrderBy(hit => (pos-hit.transform.position).sqrMagnitude).ToArray();
                foreach (Collider2D hit in hits)
                {
                    Entity e = hit.GetComponent<Entity>();
                    if (e==null) e = hit.GetComponentInChildren<Entity>();
                    if (e==null || e.ally==ally) continue;

                    chaseEntity = e;
                    break;
                }
            }
            if (chaseEntity==null) agent.SetDestination(targetPos);
            else agent.SetDestination(chaseEntity.transform.position);
        }
        agent.speed = (3+dist)*Time.fixedDeltaTime*20;
    }

    public override void Flip()
    {
        isFlipped = !isFlipped;
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3 (scale.x*-1f,scale.y,scale.z);
    }


}

using UnityEngine;
using UnityEngine.AI;

public class SummonScript_Follow : SummonScript
{
    protected NavMeshAgent agent;
    public Entity followEntity;
    protected Vector3 followOffset;

    public void SetUp()
    {
        agent = GetComponent<NavMeshAgent>();
        followEntity = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.stoppingDistance = 2;
        if (followEntity!=null) followOffset = new Vector3(UnityEngine.Random.value*2-1,UnityEngine.Random.value*2-1,UnityEngine.Random.value*2-1);
    }

    private void FixedUpdate()
    {
        TickUpdate();
        if (aniLock==true || agent==null || followEntity==null) return;
        Vector3 targetPos = followEntity.transform.position+followOffset;
        float dist = Vector2.SqrMagnitude(transform.position-targetPos);

        if (dist>100)
        {
            transform.position = targetPos;
            return;
        }
        agent.SetDestination(targetPos);
        agent.speed = dist*Time.fixedDeltaTime*5;
    }
}

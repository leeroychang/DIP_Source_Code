using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GravityScript : MonoBehaviour
{
    float strength;
    float speed;
    float spdSqr;
    bool ally;

    Coroutine pullCoroutine;

    List<Rigidbody2D> rbs = new();
    List<NavMeshAgent> agents = new();

    public void SetUp(float force, float radius, bool ally, float duration)
    {
        strength = force;
        speed = force/10f;
        spdSqr = speed*speed;
        if (TryGetComponent(out CircleCollider2D coll)) coll.radius = radius;
        this.ally = ally;

        pullCoroutine = StartCoroutine(DurationTimer(duration));
    }

    void OnDisable()
    {
        if (pullCoroutine!=null)
        {
            StopCoroutine(pullCoroutine);
            pullCoroutine = null;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Entity entity = collision.gameObject.GetComponent<Entity>();

        if (entity==null || entity.ally==ally || entity.interuptImmune) return;

        // if (collision.TryGetComponent(out NavMeshAgent agent)) agents.Add(agent);
        if (collision.TryGetComponent(out Rigidbody2D rb)) rbs.Add(rb);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // if (collision.TryGetComponent(out NavMeshAgent agent)) if (agents.Contains(agent)) agents.Add(agent);
        if (TryGetComponent(out Rigidbody2D rb)) if (rbs.Contains(rb)) rbs.Remove(rb);
    }

    IEnumerator DurationTimer(float duration)
    {
        List<Rigidbody2D> removal = new();
        while (duration>0)
        {
            yield return new WaitForFixedUpdate();
            removal.Clear();
            foreach (Rigidbody2D rb in rbs)
            {
                if (rb==null)
                {
                    removal.Add(rb);
                    continue;
                }
                rb.AddForce((Vector2) (transform.position-rb.transform.position).normalized*strength);
                if (rb.linearVelocity.sqrMagnitude>spdSqr) rb.linearVelocity = rb.linearVelocity.normalized * speed;
            }
            foreach (Rigidbody2D rb in removal) rbs.Remove(rb);
            // foreach (Rigidbody2D rb in rbs) rb.linearVelocity += (Vector2) (transform.position-rb.transform.position).normalized*strength;
            // foreach (NavMeshAgent agent in agents) agent.velocity += (transform.position-agent.transform.position).normalized*strength;
            duration -= Time.deltaTime;
            
        }
        Destroy(gameObject);
    }
}

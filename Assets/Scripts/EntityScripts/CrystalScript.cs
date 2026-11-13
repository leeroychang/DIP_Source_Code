using Unity.Mathematics;
using UnityEngine;

public class CrystalScript : MonoBehaviour
{
    bool ally;
    bool moving = true;
    float timer;
    Rigidbody2D rb;

    [SerializeField] GameObject area;
    [SerializeField] GameObject motionAnim;
    [SerializeField] GameObject groundedAnim;

    public void SetUp(bool ally)
    {
        this.ally = ally;
        timer = UnityEngine.Random.Range(0.2f,0.5f);
        rb = GetComponent<Rigidbody2D>();
        transform.rotation = Quaternion.Euler(0,0,UnityEngine.Random.Range(0f,360f));
        rb.linearVelocity = transform.right*10f;
        motionAnim.SetActive(true);
        groundedAnim.SetActive(false);
    }

    void FixedUpdate()
    {
        timer -= Time.fixedDeltaTime;
        if (timer>0) return;

        if (moving)
        {
            moving=false;
            if (rb.linearVelocityX<0)
            {
                Vector3 scale = transform.localScale;
                transform.localScale = new Vector3 (scale.x*-1f,scale.y,scale.z);
            }
            rb.linearVelocity = Vector2.zero;
            transform.rotation = quaternion.identity;
            timer = 5f;
            motionAnim.SetActive(false);
            groundedAnim.SetActive(true);
        }
        else
        {
            Explode();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (moving) return;

        Entity entity = collision.GetComponent<Entity>();
        if (entity==null) entity = collision.GetComponentInChildren<Entity>();
        if (entity==null || entity.ally==ally) return;

        Explode();
    }

    void Explode()
    {
        Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position,5f);
        foreach (Collider2D coll in colls)
        {
            Entity e = coll.GetComponent<Entity>();
            if (e==null) e = coll.GetComponentInChildren<Entity>();
            if (e==null) continue;
            if (e.ally==ally) continue;

            DamageEvent dmgEvent = new DamageEvent(e,20);
            EventManager.Queue(dmgEvent);
        }

        area.transform.SetParent(null);
        area.transform.rotation = Quaternion.identity;
        area.transform.position = transform.position;
        area.SetActive(true);
        Destroy(gameObject);
    }


}

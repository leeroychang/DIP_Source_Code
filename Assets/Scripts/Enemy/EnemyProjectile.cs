using UnityEngine;

// Attach this to your projectile prefab.
// The prefab needs: Rigidbody2D (Gravity Scale 0), Collider2D (Is Trigger ON).
// Call Launch() right after Instantiate().
public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed    = 6f;
    public float lifetime = 3f;   // auto-destroy after this many seconds
    public int   damage   = 1;    // raw damage before target's DEF

    private Vector2 direction;

    // Called by Enemyv2 right after Instantiate to give it heading and damage.
    public void Launch(Vector2 dir, int dmg)
    {
        direction = dir.normalized;
        damage    = dmg;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    // Hits anything with an Entity component (Player, other entities).
    // Make sure your player's collider is a Trigger, or swap this for OnCollisionEnter2D.
    private void OnTriggerEnter2D(Collider2D other)
    {
        Entity entity = other.GetComponent<Entity>();
        if (entity != null)
        {
            entity.Damaged(damage);
            Destroy(gameObject);
        }
    }
}
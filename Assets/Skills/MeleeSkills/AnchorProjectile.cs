using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AnchorProjectile : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private float spinSpeed = 0f;
    [SerializeField] private LineRenderer chainLineRenderer;

    [Header("Pull Settings")]
    [SerializeField] private float catchDistance = 0.8f;

    private enum ProjectileState { Outbound, Inbound }
    private ProjectileState currentState = ProjectileState.Outbound;

    private Entity owner;
    private Entity snaggedEntity;
    private Rigidbody2D snaggedRb;
    private RigidbodyType2D originalBodyType;

    private Vector2 moveDirection;
    private float flySpeed;
    private float pullSpeed;
    private float maxDistance;
    private int damage;

    private Vector2 startPos;

    public void Initialize(Entity ownerEntity, Vector2 direction, float flySpd, float pullSpd, float maxDist, int damageVal)
    {
        owner = ownerEntity;
        moveDirection = direction.normalized;
        flySpeed = flySpd;
        pullSpeed = pullSpd;
        maxDistance = maxDist;
        damage = damageVal;
        startPos = transform.position;
        currentState = ProjectileState.Outbound;

        if (chainLineRenderer != null)
        {
            chainLineRenderer.positionCount = 2;
        }
    }

    private void Update()
    {
        // 1. Update Chain Line positions
        if (chainLineRenderer != null && owner != null)
        {
            chainLineRenderer.SetPosition(0, owner.transform.position);
            chainLineRenderer.SetPosition(1, transform.position);
        }

        // 2. Sprite Spin
        if (spriteTransform != null && spinSpeed > 0)
        {
            spriteTransform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        }

        // 3. Flight Logic
        if (currentState == ProjectileState.Outbound)
        {
            transform.position += (Vector3)(moveDirection * flySpeed * Time.deltaTime);

            if (Vector2.Distance(startPos, transform.position) >= maxDistance)
            {
                currentState = ProjectileState.Inbound;
            }
        }
        else if (currentState == ProjectileState.Inbound)
        {
            if (owner == null)
            {
                ReleaseAndDestroy();
                return;
            }

            Vector2 returnDir = ((Vector2)owner.transform.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(returnDir * pullSpeed * Time.deltaTime);

            // Drag snagged target smoothly
            if (snaggedEntity != null)
            {
                snaggedEntity.transform.position = transform.position;
            }

            if (Vector2.Distance(transform.position, owner.transform.position) <= catchDistance)
            {
                ReleaseAndDestroy();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (owner != null && collision.gameObject == owner.gameObject) return;

        if (currentState == ProjectileState.Outbound && snaggedEntity == null)
        {
            if (collision.TryGetComponent<Entity>(out var targetEntity))
            {
                snaggedEntity = targetEntity;
                DamageEvent dmgEvent = new DamageEvent(snaggedEntity,damage);
                ElementEvent eleEvent = new ElementEvent(snaggedEntity,Element.Earth);
                EventManager.Queue(dmgEvent);
                EventManager.Queue(eleEvent);

                if (targetEntity.interuptImmune)
                {
                    ReleaseAndDestroy();
                    return;
                }

                // Disable physics forces while snagged
                if (snaggedEntity.TryGetComponent<Rigidbody2D>(out snaggedRb))
                {
                    originalBodyType = snaggedRb.bodyType;
                    snaggedRb.bodyType = RigidbodyType2D.Kinematic;
                    snaggedRb.linearVelocity = Vector2.zero;
                }

                snaggedEntity.Knockback(10,0.5f,moveDirection*-1);

                currentState = ProjectileState.Inbound;
            }
        }
    }

    private void ReleaseAndDestroy()
    {
        // Restore enemy physics back to normal
        if (snaggedRb != null)
        {
            snaggedRb.bodyType = originalBodyType;
            snaggedRb.linearVelocity = Vector2.zero;
        }

        snaggedEntity = null;
        snaggedRb = null;
        if (owner!=null) owner.resetAnimation(true);
        Destroy(gameObject);
    }
}
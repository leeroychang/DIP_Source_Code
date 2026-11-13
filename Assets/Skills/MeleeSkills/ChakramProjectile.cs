using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ChakramProjectile : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private Transform spriteTransform; // Drag the child Sprite object here to spin it
    [SerializeField] private float spinSpeed = 720f;   // Rotation speed in degrees per second

    [Header("Return Settings")]
    [Tooltip("Distance threshold to catch/destroy the chakram when returning to the player.")]
    [SerializeField] private float catchDistance = 0.6f;

    private enum ProjectileState { Outbound, Inbound }
    private ProjectileState currentState = ProjectileState.Outbound;

    private Entity owner;
    private Vector2 moveDirection;
    private float moveSpeed;
    private float maxDistance;
    private int damage;

    private Vector2 startPos;

    public void Initialize(Entity ownerEntity, Vector2 direction, float speed, float maxDist, int damageVal)
    {
        owner = ownerEntity;
        moveDirection = direction.normalized;
        moveSpeed = speed;
        maxDistance = maxDist;
        damage = damageVal;
        startPos = transform.position;
        currentState = ProjectileState.Outbound;
    }

    private void Update()
    {
        // 1. Visual Spinning Effect
        if (spriteTransform != null)
        {
            spriteTransform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        }
        else
        {
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        }

        // 2. Trajectory Movement State Machine
        if (currentState == ProjectileState.Outbound)
        {
            // Fly forward in initial direction
            transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);

            // Reached max distance -> Turn around and head back
            if (Vector2.Distance(startPos, transform.position) >= maxDistance)
            {
                currentState = ProjectileState.Inbound;
            }
        }
        else if (currentState == ProjectileState.Inbound)
        {
            // If player no longer exists, despawn
            if (owner == null)
            {
                Destroy(gameObject);
                return;
            }

            // Homing vector tracking player's current position
            Vector2 returnDir = ((Vector2)owner.transform.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(returnDir * moveSpeed * Time.deltaTime);

            // Catch check based on proximity to player center
            if (Vector2.Distance(transform.position, owner.transform.position) <= catchDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Handle owner collision (Catching the returning chakram)
        if (owner != null && collision.gameObject == owner.gameObject)
        {
            if (currentState == ProjectileState.Inbound)
            {
                Destroy(gameObject);
            }
            return;
        }

        // Damage enemies on contact (both outbound and return legs)
        if (collision.TryGetComponent<Entity>(out var targetEntity))
        {
            targetEntity.Damaged(damage);
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class SkateScript : MonoBehaviour
{
    GameObject trail;
    SkillSkate skill;
    Entity entity;
    Rigidbody2D rb;
    Vector2 currentDir;
    Vector2 targetDir;
    InputAction moveAction;
    float turnSpeed = 180;
    float moveSpeed = 8;

    public void SetUp(SkillSkate skill, Entity entity, Vector2 dir, Rigidbody2D rb)
    {
        Debug.Log("Test");
        
        this.skill = skill;
        this.entity = entity;
        this.rb = rb;
        dir = dir.normalized;
        currentDir = dir;
        targetDir = dir;
        
        if (!TryGetComponent(out PlayerController player)) return;
        
        player.allowInput = false;
        moveAction = player.GetMoveAction();
        moveAction.performed += UpdateTarget;
        moveAction.canceled += UpdateTarget;

        trail = Instantiate(Resources.Load<GameObject>("Prefabs/SkateTrailPrefab"),entity.transform);
        
    }

    void OnDisable()
    {
        if (!TryGetComponent(out PlayerController player)) return;
        player.allowInput = true;
        player.aniAllowedInput = true;
        moveAction.performed -= UpdateTarget;
        moveAction.canceled -= UpdateTarget;
        rb.linearVelocity = Vector2.zero;
        Destroy(trail);
    }

    void UpdateTarget(InputAction.CallbackContext context)
    {
        Vector2 dir = context.ReadValue<Vector2>();
        targetDir = dir.normalized;
    }

    void FixedUpdate()
    {
        float maxAngle = turnSpeed*Time.fixedDeltaTime;
        float angle = Vector2.SignedAngle(currentDir,targetDir);
        currentDir = (Quaternion.Euler(0,0,Mathf.Clamp(angle,-maxAngle,maxAngle))*currentDir).normalized;

        rb.linearVelocity = currentDir*moveSpeed;
    }

    public void EndSkate()
    {
        skill.EndSkate(entity);
    }

}

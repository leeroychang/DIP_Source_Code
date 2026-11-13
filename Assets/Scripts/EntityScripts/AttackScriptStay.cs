using UnityEngine;

public class AttackScriptStay : AttackScript
{
    void OnTriggerStay2D(Collider2D collision)
    {
        Entity entity = collision.gameObject.GetComponent<Entity>();
        EntityEnter(entity);
    }
}

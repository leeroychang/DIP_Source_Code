using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class SkillSkate : SkillType
{
    Dictionary<Entity,SkateScript> scripts = new();

    public override void OnUse(Skill skill, Entity entity, Vector3 targetPos)
    {
        Debug.Log("Testt");
        if (scripts.ContainsKey(entity)) EndSkate(entity);
        else
        {
            SkateScript script = entity.AddComponent<SkateScript>();
            script.SetUp(this,entity,targetPos-entity.transform.position,entity.GetComponent<Rigidbody2D>());
            scripts[entity] = script;
        }
    }

    public void EndSkate(Entity entity)
    {
        SkateScript script = scripts[entity];
        scripts.Remove(entity);
        GameObject.Destroy(script);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulwarkScript : Entity
{


    [SerializeField] List<Sprite> sprites;

    float maxDuration;
    float duration;
    Skill downSkill;
    Coroutine endCoroutine;

    public void BulwarkSetUp(Vector3 pos, float duration, Skill skill)
    {
        transform.position = pos;
        this.duration = duration;
        maxDuration = duration;
        healthbar.maxValue = duration;
        downSkill = skill;
        interuptImmune = true;
    }

    void OnDisable()
    {
        if (endCoroutine!=null)
        {
            StopCoroutine(endCoroutine);
            endCoroutine = null;
        }
    }

    public override int Damaged(int val, int ignoreDef = 0, float penpercent = 0)
    {
        duration -= val/20f;
        UpdateHealth();
        return val;
    }


    protected override void Down()
    {
        DownSkill();
        Destroy(gameObject);
    }

    void DownSkill()
    {
        downSkill.OnUse(this,transform.position,true);
    }

    void FixedUpdate()
    {
        duration -= Time.fixedDeltaTime;
        UpdateHealth();
        if (duration<=0) Down();
    }

    protected override void UpdateHealth()
    {
        if (spriteRenderer!=null)
        {
            if (duration==maxDuration) spriteRenderer.sprite = sprites[0];
            spriteRenderer.sprite = sprites[sprites.Count - (int) (duration/maxDuration*sprites.Count) - 1];
        }
        if (healthbar!=null) healthbar.value = duration;
    }
    
}

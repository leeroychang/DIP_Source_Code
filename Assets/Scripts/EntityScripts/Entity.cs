using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Entity : MonoBehaviour
{
    [Header("References")]

    [SerializeField] Transform shadow;
    [SerializeField] CircleCollider2D coll;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Slider healthbar;
    [SerializeField] Transform statusHolder;
    [SerializeField] GameObject statusPrefab;
    [SerializeField] protected StateAnimator animator;
    [SerializeField] protected Animator anim;

    [SerializeField] EntityData prefixEntity;

    protected Dictionary<int,Skill> skills;
    protected Dictionary<StatusEffect,StatusDisplayScript> statusEffects;

    public bool ally { get; private set; }
    public string defaultLayer;

    public Dictionary<int,SkillData> GetSkillDatas()
    {
        Dictionary<int,SkillData> datas = new Dictionary<int, SkillData>();
        foreach (KeyValuePair<int,Skill> pair in skills) datas[pair.Key] = pair.Value.skillData;
        return datas;
    }

    public Dictionary<int,Skill> GetSkills()
    {
        return new Dictionary<int, Skill>(skills);
    }

    public void ResetSkills(Dictionary<int,Skill> newSkills)
    {
        if (skills!=null) skills.Clear();
        else skills = new Dictionary<int, Skill>();
        foreach (KeyValuePair<int,Skill> pair in newSkills) skills[pair.Key] = pair.Value;
    }

    public void UpdateSkill(int slot, Skill skill)
    {
        if (skill!=null) skills[slot] = skill;
        else if (skills.ContainsKey(slot)) skills.Remove(slot);
    }

    public void UpdateSkillUpgrade(int slot, int upgradeSlot, Upgrade upgrade)
    {
        skills[slot].ReplaceUpgrade(upgradeSlot,upgrade);
    }
    

    protected Rigidbody2D rb;

    //Insert HealthBar Reference


    EntityData _entityData;
    public EntityData entityData
    {
        get { return _entityData; }
        private set { _entityData = value; }
    }

    protected Dictionary<EntityData.Stat,int> stats;
    protected Dictionary<EntityData.Stat,float> statModifier;

    public bool aniLock = false;
    float aniDuration = 0f;
    float aniMoveSpeed = 0f;
    Vector2? aniMoveDir = null;
    public bool interuptImmune = false;
    public bool dmgImmune = false;
    protected bool isFlipped = false;
    public Coroutine skillCoroutine;
    public Coroutine hurtCoroutine;

    public virtual void Setup(EntityData entityData, bool isAlly=false)
    {
        defaultLayer = "Entity";
        this.entityData = entityData;
        stats = entityData.GetStats();
        statModifier = new();
        foreach (EntityData.Stat stat in Enum.GetValues(typeof(EntityData.Stat))) statModifier[stat] = 1;
        coll.radius = entityData.size/2;
        if (shadow!=null) shadow.localScale = (Vector2.one+Vector2.right) * entityData.size/2f;
        skills = new Dictionary<int, Skill>();
        List<SkillData> datas = entityData.GetSkills();
        for (int i=0; i<datas.Count; i++) skills[i] = new Skill(datas[i]);
        statusEffects = new();
        ally = isAlly;

        if (anim!=null)
        {
            if (entityData.animator!=null)
            {
                anim.runtimeAnimatorController = entityData.animator;
                anim.transform.localPosition = Vector2.up*0.5f + entityData.offset;
                animator=null;
            }
            else if (this is PlayerController)
            {
                anim.transform.localPosition = Vector2.up*0.5f + entityData.offset;
                animator=null;
            }
            else anim=null;
        }
        if (animator!=null)
        {
  
            animator.Init(entityData.Sprites());
            animator.SetState(StateAnimator.State.Idle,entityData.framesPerSec,true);
        }
    }

    void Awake()
    {
        if (rb==null)
        {
            TryGetComponent(out rb);
        }
        if (prefixEntity==null) return;
        Setup(prefixEntity);
    }


    void FixedUpdate()
    {
        TickUpdate();
    }

    public virtual void TickUpdate()
    {
        if (anim!=null)
        {
            float dura = anim.GetFloat("duration");
            if (dura>=0) anim.SetFloat("duration",dura-Time.fixedDeltaTime);
        }
        if (aniLock)
        {
            aniDuration -= Time.fixedDeltaTime;
            
            if (aniMoveSpeed!=0 && aniMoveDir!=null) rb.MovePosition((Vector2)transform.position+(aniMoveDir ?? Vector2.zero)*Time.fixedDeltaTime*aniMoveSpeed);
            if (aniDuration<=0) resetAnimation();
            
        }

        float tick = Time.fixedDeltaTime;
        foreach (Skill skill in skills.Values) skill.CDTick(tick);
        foreach ((StatusEffect status, StatusDisplayScript display) in new Dictionary<StatusEffect,StatusDisplayScript>(statusEffects))
        {
            if (display!=null) display.UpdateDisplay(status.duration);
            status.TickUpdate(tick);
        }
    }

    public void AddStatus(StatusEffect status)
    {
        //DO NOT CALL DIRECTLY! CALL FROM STATUSEFFECT!
        if (statusHolder==null || statusPrefab==null)
        {
            statusEffects[status] = null;
            return;
        }
        StatusDisplayScript display = Instantiate(statusPrefab,statusHolder).GetComponent<StatusDisplayScript>();
        display.SetUp(status);
        statusEffects[status] = display;
    }

    public void RemoveStatus(StatusEffect status)
    {
        //DO NOT CALL DIRECTLY! CALL FROM STATUSEFFECT!
        if (statusHolder==null || statusPrefab==null)
        {
            statusEffects.Remove(status);
            return;
        }
        StatusDisplayScript display = statusEffects[status];
        statusEffects.Remove(status);
        Destroy(display.gameObject);
    }

    public void ClearStatus(Type type = null)
    {
        if (type==null)
        {
            foreach (StatusEffect status in statusEffects.Keys.ToList()) status.RemoveStatus();
            return;
        }
        StatusEffect effect = statusEffects.Keys.ToList().Find(x => x.GetType()==type);
        if (effect==null) return;
        effect.RemoveStatus();
    }

    public void UpdateStatus(StatusEffect status)
    {
        if (!statusEffects.ContainsKey(status) || statusEffects[status]==null) return;
        statusEffects[status].UpdateDisplay(status.duration,status.stacks,status.maxDuration);
    }

    public bool CheckStatus(Type type)
    {
        return FindStatus(type)!=null;
    }

    public StatusEffect FindStatus(Type type)
    {
        return statusEffects.Keys.ToList().Find(x => x.GetType()==type);
    }

    public int GetStatusStacks(Type type=null)
    {
        int stacks = 0;
        if (type==null) { foreach (StatusEffect status in statusEffects.Keys.ToList()) stacks += status.stacks; }
        else { foreach (StatusEffect status in statusEffects.Keys.ToList().FindAll(x => x.GetType()==type)) stacks += status.stacks; }
        return stacks;
    }

    public void UpdateStat(EntityData.Stat stat, float modifier)
    {
        if (stat==EntityData.Stat.HP || stat==EntityData.Stat.MaxHP)
        {
            float ratio = (statModifier[EntityData.Stat.MaxHP]+modifier)/statModifier[EntityData.Stat.MaxHP];
            statModifier[EntityData.Stat.MaxHP] += modifier;
            stats[EntityData.Stat.MaxHP] = (int) (entityData.GetStats()[EntityData.Stat.MaxHP]*statModifier[EntityData.Stat.MaxHP]);
            stats[EntityData.Stat.HP] = (int) (stats[EntityData.Stat.HP]*ratio);
            if (stats[EntityData.Stat.MaxHP]<=0) stats[EntityData.Stat.MaxHP]=1;
            if (stats[EntityData.Stat.HP]<=0) stats[EntityData.Stat.HP]=1;
            UpdateHealth();
            return;
        }
        statModifier[stat] += modifier;
        stats[stat] = (int) (entityData.GetStats()[stat]*statModifier[stat]);
        if (stat==EntityData.Stat.SPD)
        {
            Enemyv2 e = this as Enemyv2;
            if (e==null) return;
            e.chaseSpeed = stats[stat];
        }
    }

    public Dictionary<EntityData.Stat,float> GetStatModifiers() { return statModifier; }
    public float GetStatValue(EntityData.Stat stat) { return stats[stat]; }
    public int HP { get { return stats[EntityData.Stat.HP]; } }

    public virtual float DMGReduction(int ignoreDef, float penpercent)
    {
        float def = stats[EntityData.Stat.DEF]*(1f-penpercent)-ignoreDef;
        if (def<=0) return 1f;
        return 1f - (def/(def+30f));
    }

    public virtual int Damaged(int val, int ignoreDef=0, float penpercent=0f)
    {
        if (dmgImmune) return 0;
        if (stats[EntityData.Stat.HP]<=0) return 0;
        Hurt();
        int dmg = (int) (val*DMGReduction(ignoreDef,penpercent));
        if (dmg<=0) dmg = 1;
        stats[EntityData.Stat.HP] -= dmg;

        if (stats[EntityData.Stat.HP]<=0)
        {
            stats[EntityData.Stat.HP] = 0;
            //Insert on Down method
            Down();

        }
        //Insert UpdateHealth method
        UpdateHealth();
        if (UIManager.Instance!=null) UIManager.Instance.DMG(dmg,transform.position,ally);
        //Insert DmgPopUp method
        return dmg;
    }

    public void Hurt(float duration = 0.2f)
    {
        if (hurtCoroutine!=null)
        {
            StopCoroutine(hurtCoroutine);
            hurtCoroutine = null;
        }
        hurtCoroutine = StartCoroutine(Hurting(duration));
    }

    public virtual IEnumerator Hurting(float duration = 0.2f)
    {

        spriteRenderer.color = Color.red;
        float time = 0;
        while (time<duration)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(spriteRenderer.color,Color.wheat,time/duration);
        }

        spriteRenderer.color = Color.white;
    }

    public virtual int Heal(int val)
    {
        int hp = stats[EntityData.Stat.HP];
        int maxhp = stats[EntityData.Stat.MaxHP];

        hp += val;
        if (hp>maxhp)
        {
            val -= hp-maxhp;
            hp = maxhp;
        }

        stats[EntityData.Stat.HP] = hp;
        UpdateHealth();
         if (UIManager.Instance!=null) UIManager.Instance.DMG(val,transform.position,ally,true);
        return val;
    }

    public virtual void Knockback(float strength, float duration, Vector2 dir, bool interuptable=true, bool immune=false, bool triggerAni=true)
    {
        if (interuptImmune) return;
        if (skillCoroutine!=null)
        {
            StopCoroutine(skillCoroutine);
            skillCoroutine = null;
        }

        if (TryGetComponent(out SkateScript skate))
        {
            skate.EndSkate();
        }

        // StartAnimation(duration,strength,dir.normalized);
        if (triggerAni && anim!=null) anim.SetTrigger("Idle");
        rb.linearVelocity = dir*strength;
        if (anim!=null) anim.speed = 0;

        if (TryGetComponent(out NavMeshAgent agent)) agent.isStopped = true;
        StartAnimation(duration,interuptable,immune);
    }


    protected virtual void UpdateHealth()
    {
        if (healthbar==null) return;

        healthbar.maxValue = stats[EntityData.Stat.MaxHP];
        healthbar.value = stats[EntityData.Stat.HP];
    }

    public virtual void TriggerAnimation(string trigger, float duration=0.3f, Vector2? direction=null)
    {
        if (anim==null) return;
        Vector2 dir = direction ?? Vector2.zero;
        if (dir.x<0 && !isFlipped || dir.x>0 && isFlipped) Flip();
        dir.x = dir.x*dir.x;
        if (dir.x==0)
        {
            dir.y = dir.y<0 ? -1 : dir.y>0 ? 1 : 0;
            dir.x = 0.1f;
        }

        anim.SetFloat("targetX",dir.x*dir.x);
        anim.SetFloat("targetY",dir.y);
        anim.SetFloat("duration",duration);
        anim.SetTrigger(trigger);
    }

    public virtual void AnimateDirection(Vector2 dir)
    {
        if (anim==null) return;
        if (dir.x<0 && !isFlipped || dir.x>0 && isFlipped) Flip();
        dir.x = dir.x*dir.x;
        if (dir.x==0)
        {
            dir.y = dir.y<0 ? -1 : dir.y>0 ? 1 : 0;
            dir.x = 0.1f;
        }

        anim.SetFloat("targetX",dir.x*dir.x);
        anim.SetFloat("targetY",dir.y);
    }

    public virtual void StartAnimation(float duration, bool interuptable=true, bool immune=false,float moveSpeed=0, Vector2? moveDir=null) //Add State for sprite changes
    {
        // resetAnimation(false);
        if (interuptImmune) return;

        

        aniLock = true;
        aniDuration = duration;
        interuptImmune = !interuptable;
        dmgImmune = immune;
        aniMoveSpeed = moveSpeed;
        aniMoveDir = moveDir;

        if (TryGetComponent(out PlayerController player)) player.SkillLock(true);

        if (interuptImmune) gameObject.layer = LayerMask.NameToLayer("EntityIgnore");
        if (TryGetComponent(out NavMeshAgent agent))
        {
            agent.isStopped = true;
        }

        //Insert Sprite Change

    }

    public virtual void resetAnimation(bool resetAgent=true)
    {
        if (anim!=null) anim.speed = 1;
        aniLock = false;
        aniDuration = 0;
        aniMoveSpeed = 0;
        interuptImmune = false;
        dmgImmune = false;

        gameObject.layer = LayerMask.NameToLayer(defaultLayer);
        if (!resetAgent) return;
        if (TryGetComponent(out NavMeshAgent agent))
        {
            agent.isStopped = false;
        }
        rb.linearVelocity = Vector2.zero;
        if (TryGetComponent(out PlayerController player)) player.SkillLock(false);
        if (anim!=null) anim.SetTrigger("Idle");
    }

    public virtual void Flip()
    {
        Vector3 scale = anim.transform.localScale;
        anim.transform.localScale = new Vector3 (scale.x*-1f,scale.y,scale.z);
        isFlipped = !isFlipped;
    }


    public bool IsDown() { return stats[EntityData.Stat.HP]<=0; }

    protected virtual void Down() { StartCoroutine(OnDown()); }

    protected IEnumerator OnDown()
    {
        //Disable Entity
        // spriteRenderer.color = Color.gray; //Placeholder
        if (anim!=null) anim.SetBool("Down",true);
        if (animator!=null) animator.SetState(StateAnimator.State.Down);
        coll.enabled = false;

        if (skillCoroutine!=null)
        {
            StopCoroutine(skillCoroutine);
            skillCoroutine = null;
        }

        if (hurtCoroutine!=null)
        {
            StopCoroutine(hurtCoroutine);
            hurtCoroutine = null;
        }

        //Down Animation


        ClearStatus();
        spriteRenderer.color = Color.gray;

        if (!ally) PersistentData.AddKill();

        yield return new WaitForSeconds(0.5f);

        if (!ally && entityData.xp>0)
        {
            XPOrbScript xpOrb = Instantiate(UIManager.Instance.XPOrb).GetComponent<XPOrbScript>();
            xpOrb.SetUp(transform.position, entityData.xp);            
        }

        yield return new WaitForSeconds(0.25f);

        if (!ally && UnityEngine.Random.value<=0.3f) Instantiate(UIManager.Instance.HPOrb).GetComponent<HPOrbScript>().SetUp(transform.position,20);

        yield return new WaitForSeconds(0.25f);

        //Despawn
        Destroy(gameObject);
    }


}

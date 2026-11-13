using System;
using System.Collections.Generic;
using System.Linq;
using SerializeReferenceEditor;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PlayerController : Entity
{
    [Header("Player Data")]
    [SerializeField] EntityData playerEntityData; //Only for temp Setup
    [SerializeField] SkillData dashData;
    [SerializeField] List<SkillData> skillDatas;
    [SerializeField] List<Sprite> skillHotKeys;

    Skill dash;

    [Header("Control References")]
    [SerializeField] InputActionAsset actionAsset;
    [SerializeField] Camera cam;
    InputAction moveAction;
    InputAction zoomAction;
    InputAction dashAction;
    InputAction skillAction;
    InputAction skillReleaseAction;

    [Header("Control Settings")]
    [SerializeField] float zoomSpeed = 5f;
    [SerializeField] float minZoom = 5f;
    [SerializeField] float maxZoom = 30f;

    public bool allowInput;
    public bool aniAllowedInput = true;

    public Dictionary<int,ActiveSkillSlot> SkillDisplays;
    [SerializeField] GameObject displaySlotPrefab;
    [SerializeField] Transform skillHolder;


    void Awake()
    {
        Setup(playerEntityData,true);
        PlayerSetUp(PersistentData.load);
    }

    public void PlayerSetUp(bool loadSave)
    {
        moveAction = actionAsset.FindAction("Movement");
        zoomAction = actionAsset.FindAction("Zoom");
        dashAction = actionAsset.FindAction("Dash");
        skillAction = actionAsset.FindAction("Skill");
        skillReleaseAction = actionAsset.FindAction("SkillRelease");
        rb = GetComponent<Rigidbody2D>();
        Application.targetFrameRate = 60;
        allowInput = true;
        dash = new Skill(dashData);

        if (loadSave)
        {
            skills = new Dictionary<int, Skill>(PersistentData.Instance.skills);
            statModifier = new Dictionary<EntityData.Stat, float>(PersistentData.Instance.statModifier);
            foreach(EntityData.Stat stat in stats.Keys.ToList()) stats[stat] = (int) (entityData.GetStats()[stat]*statModifier[stat]);
            foreach (Skill skill in skills.Values)
            {
                if (skill==null) continue;
                for (int i=0; i<3; i++)
                {
                    Upgrade upgrade = skill.upgradeSlots[i];
                    if(upgrade!=null) upgrade.OnEnable(skill,i);
                }
            }
            stats[EntityData.Stat.HP] = PersistentData.Instance.hp;
            // UpdateHealth();
        }

        //for (int i=0; i<skillDatas.Count; i++) skills[i] = new Skill(skillDatas[i]);
        SkillDisplays = new();
        for (int i=0; i<4; i++)
        {
            SkillDisplays[i] = Instantiate(displaySlotPrefab,skillHolder).GetComponent<ActiveSkillSlot>();
            SkillDisplays[i].UpdateDisplay(null,0,0,skillHotKeys[i]);
        }
    }

    void OnEnable()
    {
        skillReleaseAction.canceled += SkillRelease;
        SkillLock(false);
    }

    void OnDisable()
    {
        skillReleaseAction.canceled -= SkillRelease;
        SkillLock(true);
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
    }

    private void DashUse(InputAction.CallbackContext context)
    {
        if (Time.timeScale==0) return;
        Vector2 moveDir = moveAction.ReadValue<Vector2>();
        if (moveDir!=Vector2.zero)
        {
            dash.OnUse(this,(Vector2)transform.position+moveDir);
        }
    }

    private void SkillUse(InputAction.CallbackContext context)
    {
        if (Time.timeScale==0) return;
        int val = (int) skillAction.ReadValue<float>();
        if (skills.ContainsKey(val)) skills[val].OnUse(this,Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
    }

    private void SkillRelease(InputAction.CallbackContext context)
    {
        int val = -1;
        InputBinding binding = skillReleaseAction.GetBindingForControl(context.control) ?? InputBinding.MaskByGroup("test");
        foreach (var processor in NameAndParameters.ParseMultiple(binding.processors))
        {
            if (processor.name.Equals("scale", StringComparison.OrdinalIgnoreCase))
            {
                NamedValue factorParam = processor.parameters.FirstOrDefault(p => p.name=="factor");

                if (factorParam != default)
                {
                    // Convert the value to its expected type
                    val = factorParam.value.ToInt32();
                }
                else val = 1;
            }
        }
        if (skills.ContainsKey(val)) skills[val].OnRelease(this,Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
    }

    void FixedUpdate()
    {
        if (allowInput)
        {
            Vector2 moveDir = moveAction.ReadValue<Vector2>();
            if (moveDir!=Vector2.zero)
            {
                if (anim!=null)
                {
                    anim.SetFloat("targetX",moveDir.x==0 ? 0 : 1);
                    if (moveDir.x==0)
                    {
                        anim.SetFloat("targetY",moveDir.y<0 ? -1 : moveDir.y>0 ? 1 : 0);
                        anim.SetFloat("targetX",0.1f);
                    }
                    else anim.SetFloat("targetY",0);
                    if (moveDir.x<0 && !isFlipped || moveDir.x>0 && isFlipped) Flip();
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    {

                        anim.SetTrigger("Move");

                    }
                }
                
                rb.MovePosition((Vector2)transform.position+moveDir*Time.fixedDeltaTime*stats[EntityData.Stat.SPD]);
                if (animator!=null) animator.SetState(StateAnimator.State.Move);
            }
            else if (animator!=null) animator.SetState(StateAnimator.State.Idle);
            else if (anim!=null) anim.SetTrigger("Idle");
        }

        dash.CDTick(Time.fixedDeltaTime);

        TickUpdate();
        
    }

    void Update()
    {
        if (Time.timeScale!=1) return;

        if (!allowInput) return;
        
        float zoomVal = zoomAction.ReadValue<float>();
        if (zoomVal!=0)
        {
            float currentZoom = cam.orthographicSize;
            cam.orthographicSize = math.clamp(currentZoom-zoomVal*zoomSpeed,minZoom,maxZoom);
        }
        
        // if (dashAction.WasPressedThisFrame())
        // {
        //     Vector2 moveDir = moveAction.ReadValue<Vector2>();
        //     if (moveDir!=Vector2.zero)
        //     {
        //         dash.OnUse(this,(Vector2)transform.position+moveDir);
        //     }
        // }

        // else if (skillAction.WasPressedThisFrame())
        // {
        //     int val = (int) skillAction.ReadValue<float>();
        //     if (skills.ContainsKey(val)) skills[val].OnUse(this,Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
        // }
    }

    public override void TickUpdate()
    {
        base.TickUpdate();
        foreach (KeyValuePair<int,Skill> pair in skills) if (pair.Value!=null) SkillDisplays[pair.Key].UpdateDisplay(pair.Value.skillData.icon,pair.Value.cooldown,pair.Value.skillStats[SkillStat.Cooldown]);
    }


    public override void StartAnimation(float duration, bool interuptable=true, bool immune=false, float moveSpeed=0, Vector2? moveDir=null)
    {
        base.StartAnimation(duration,interuptable,immune,moveSpeed,moveDir);
        aniAllowedInput = allowInput || aniAllowedInput;
        allowInput = false;
    }

    public override void resetAnimation(bool resetAgent=true)
    {
        base.resetAnimation();
        rb.linearVelocity = Vector2.zero;
        if (resetAgent)
        {
            allowInput = aniAllowedInput;
        }
    }

    public void SkillLock(bool isLocked)
    {
        if (isLocked)
        {
            dashAction.performed -= DashUse;
            skillAction.performed -= SkillUse;
        }
        else
        {
            dashAction.performed += DashUse;
            skillAction.performed += SkillUse;
        }
    }

    protected override void Down()
    {
        UIManager.Instance.GameOver();
    }




    public InputAction GetMoveAction() { return moveAction; }
}

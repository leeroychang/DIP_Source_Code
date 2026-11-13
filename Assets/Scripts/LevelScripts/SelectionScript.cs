using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionScript : MonoBehaviour
{

    public static SelectionScript Instance;
    PlayerController player;

    private void Awake()
    {
        if (Instance!=null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy() {
        if (Instance==this) Instance = null;
    }

    [Header("References")]
    [SerializeField] Transform selectionHolder;
    [SerializeField] Transform activeHolder;
    [SerializeField] GameObject selectionSlotPrefab;
    [SerializeField] GameObject activeSlotPrefab;
    [SerializeField] GameObject Display;
    [SerializeField] SelectionPopupScript popup;
    [SerializeField] RefreshScript refresh;

    [Header("Modifiers")]
    [SerializeField] bool isTut = false;
    [SerializeField] int newWeight;
    [SerializeField] int upgradeWeight;


    Dictionary<(SkillData,SkillData),SkillData> MergeSkills;

    Dictionary<SkillSlotDisplay,Skill> skillSlots = new();

    public SkillSlotDisplay CurrentSlot {get; private set;}
    public SkillSlotDisplay SelectedSlot {get; private set;}
    public bool CanNew {get; private set;}
    Coroutine CloseCoroutine;
  

    //Temporary Debugging
    void Start()
    {
        GetMergeSkills();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        Display.SetActive(false);
        popup.gameObject.SetActive(false);
        if (refresh!=null) refresh.SetCount(UIManager.Instance.skillRefreshCount);
        //PopulateSelection();
    }

    void GetMergeSkills()
    {
        List<MergedSkillData> merges = Resources.LoadAll<MergedSkillData>("Skills/Merged/").ToList();
        MergeSkills = new();
        foreach (MergedSkillData skill in merges) MergeSkills[skill.mergePair] = skill;
    }

    void ClearSelection()
    {
        CurrentSlot = null;
        SelectedSlot = null;
        foreach (SkillSlotDisplay slot in skillSlots.Keys)
        {
            Destroy(slot.gameObject);
        }
        skillSlots.Clear();
    }

    public void PopulateSelection()
    {
        Display.SetActive(true);
        refresh.SetCount(UIManager.Instance.skillRefreshCount);

        ClearSelection();

        //Get list of possible skills
        List<SkillData> skillDatas = Resources.LoadAll<SkillData>($"Skills/{(isTut ? "tutorial" : "Debug")}/").ToList();
        Dictionary<int,SkillData> activeSkillDatas = player.GetSkillDatas();
        Dictionary<int,Skill> activeSkills = player.GetSkills();
        skillDatas.RemoveAll(skill => activeSkillDatas.Values.Contains(skill));


        //Update saved data
        skillSlots.Clear();
        CanNew = true;

        //Display New Skill Choices
        for (int i=0; i<3; i++)
        {
            SkillSlotDisplay slot = Instantiate(selectionSlotPrefab,selectionHolder).GetComponent<SkillSlotDisplay>();
            if (skillDatas.Count==0)
            {
                slot.SetUp(null,true);
                skillSlots.Add(slot,null);
                continue;
            }
            SkillData data = skillDatas[Random.Range(0,skillDatas.Count)];
            Skill skill = new Skill(data);
            skillDatas.Remove(data);
            slot.SetUp(skill.skillData,true);
            skillSlots.Add(slot,skill);
        }

        for (int i=1; i<4; i++)
        {
            SkillSlotDisplay slot = Instantiate(activeSlotPrefab,activeHolder).GetComponent<SkillSlotDisplay>();
            if (!activeSkills.Keys.Contains(i) || activeSkills[i]==null)
            {
                slot.SetUp(null,false,i);
                skillSlots.Add(slot,null);
                continue;
            }
            slot.SetUp(activeSkillDatas[i],false,i, i==0);
            skillSlots.Add(slot,activeSkills[i]);
        }

        foreach (SkillSlotDisplay slot in skillSlots.Keys) if (HasMerge(slot)) slot.Shake(true);
        Time.timeScale = 0;
    }

    public void RefreshSelection()
    {
        if (!CanNew) return;
        if (!UIManager.Instance.RefreshSkill()) return;
        ClearSelection();
        PopulateSelection();
        if (refresh!=null) refresh.SetCount(UIManager.Instance.skillRefreshCount);
    }

    public bool OnPickup(SkillSlotDisplay slot)
    {
        if (!CanNew && slot.isNew) return false;
        if (CurrentSlot!=null || skillSlots[slot]==null) return false;
        CurrentSlot = slot;
        CursorScript.SetCursor(CursorScript.Mode.Hold);
        foreach (SkillSlotDisplay checkslot in skillSlots.Keys)
        {
            if (checkslot==slot) checkslot.Shake(false);
            else checkslot.Shake(CheckMerge(slot,checkslot));
        }
        return true;
    }

    public void OnDrop()
    {
        bool check = DroppedSkill();
        CurrentSlot = null;
        SelectedSlot = null;

        foreach (SkillSlotDisplay slot in skillSlots.Keys) slot.Shake(HasMerge(slot));
        CursorScript.SetCursor(CursorScript.Mode.Pointer);

        if (!CanNew)
        {
            if (CloseCoroutine!=null) StopCoroutine(CloseCoroutine);
            CloseCoroutine = StartCoroutine(CloseDelay(1f));
        }
    }

    void SetSkill(SkillSlotDisplay slot, Skill skill, bool isNew)
    {
        slot.SetUp(skill==null ? null : skill.skillData,isNew,slot.slot);
        skillSlots[slot] = skill;
        if (slot.slot>=0) player.UpdateSkill(slot.slot,skill);
    }

    bool DroppedSkill()
    {
        if (CurrentSlot==null || SelectedSlot==null) return false;
        if (CurrentSlot.isNew && SelectedSlot.isNew) return false;
        if (SelectedSlot.isNew && !CanNew) return false;

        if (skillSlots[SelectedSlot]==null)
        {
            //Move Function
            return MoveSlot(false);
        }

        //Conditional to check mergable
        if (MergeSkills.ContainsKey((skillSlots[CurrentSlot].skillData,skillSlots[SelectedSlot].skillData))) //TEMPORARY FIX UNTIL WE CAN MERGE SKILLS
        {
            MergeSlots((skillSlots[CurrentSlot].skillData,skillSlots[SelectedSlot].skillData));
        }
        else if (MergeSkills.ContainsKey((skillSlots[SelectedSlot].skillData,skillSlots[CurrentSlot].skillData)))
        {
            MergeSlots((skillSlots[SelectedSlot].skillData,skillSlots[CurrentSlot].skillData));
        }
        else if (!CurrentSlot.isNew && !SelectedSlot.isNew)
        {
            //Code to swap
            Skill temp = skillSlots[CurrentSlot];
            SetSkill(CurrentSlot,skillSlots[SelectedSlot],false);
            SetSkill(SelectedSlot,temp,false);
        }
        else if (CurrentSlot.isNew && !SelectedSlot.isNew)
        {
            return MoveSlot(true);
        }

        return true;
    }

    public bool CanMerge(SkillSlotDisplay slot, SkillSlotDisplay checkSlot=null)
    {
        if (checkSlot==null) checkSlot = CurrentSlot;
        SkillData data;
        bool mergable = MergeSkills.ContainsKey((skillSlots[slot].skillData,skillSlots[checkSlot].skillData));
        if (mergable) data = MergeSkills[(skillSlots[slot].skillData,skillSlots[checkSlot].skillData)];
        else
        {
            mergable = MergeSkills.ContainsKey((skillSlots[checkSlot].skillData,skillSlots[slot].skillData));
            if (!mergable) return false;
            data = MergeSkills[(skillSlots[checkSlot].skillData,skillSlots[slot].skillData)];
        }
        PopUp(data);
        return true;
    }

    public bool HasMerge(SkillSlotDisplay slot)
    {
        foreach (SkillSlotDisplay checkslot in skillSlots.Keys)
        {
            if (checkslot==slot) continue;
            if (CheckMerge(slot,checkslot)) return true;
        }
        return false;
    }

    public bool CheckMerge(SkillSlotDisplay slot, SkillSlotDisplay checkSlot)
    {
        if (skillSlots[slot]==null || skillSlots[checkSlot]==null) return false;
        if (slot.isNew && checkSlot.isNew) return false;
        return MergeSkills.ContainsKey((skillSlots[slot].skillData,skillSlots[checkSlot].skillData)) || MergeSkills.ContainsKey((skillSlots[checkSlot].skillData,skillSlots[slot].skillData));
    }

    void MergeSlots((SkillData,SkillData) mergePair)
    {
        if (CanNew) CanNew = !CurrentSlot.isNew;
        Skill skill = new Skill(MergeSkills[mergePair]);
        MoveUpgrades(skill,skillSlots[SelectedSlot]);
        
        // Skill skill1 = skillSlots[CurrentSlot];
        // Skill skill2 = skillSlots[SelectedSlot];

        // List<Upgrade> upgrades = skill1.upgradeSlots.ToList();
        // foreach (Upgrade upgrade in upgrades) skill1.ReplaceUpgrade()

        // upgrades.AddRange(skill2.upgradeSlots.ToList());



        // skill.InheritUpgrade(upgrades);
        
        SetSkill(SelectedSlot,skill,false);
        SetSkill(CurrentSlot,null,CurrentSlot.isNew);
    }

    bool MoveSlot(bool replace)
    {
        //Move Function
        if (CanNew) CanNew = !CurrentSlot.isNew;
        if (replace)
        {
            MoveUpgrades(skillSlots[CurrentSlot],skillSlots[SelectedSlot]);
        }
        SetSkill(SelectedSlot,skillSlots[CurrentSlot],false);
        SetSkill(CurrentSlot,null,CurrentSlot.isNew);
        return !CanNew;
    }

    public void MoveUpgrades(Skill newSkill, Skill replacedSkill)
    {
        List<Upgrade> upgrades = replacedSkill.upgradeSlots.ToList();
            for (int i=0; i<3; i++)
            {
                replacedSkill.ReplaceUpgrade(i,null);
                newSkill.ReplaceUpgrade(i,upgrades[i]);
            }
    }

    public void Hover(SkillSlotDisplay slot)
    {
        if (CurrentSlot!=null && SelectedSlot==null) SelectedSlot = slot;
        if (CurrentSlot==null && (CanNew || !slot.isNew)) CursorScript.SetCursor(CursorScript.Mode.Grab);
    }

    public void Unhover(SkillSlotDisplay slot)
    {
        popup.gameObject.SetActive(false);
        if (SelectedSlot==slot) SelectedSlot = null;
        if (CurrentSlot==null) CursorScript.SetCursor(CursorScript.Mode.Pointer);
    }

    private IEnumerator CloseDelay(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        while (CurrentSlot!=null) yield return new WaitForSecondsRealtime(0.5f);
        Display.SetActive(false);
        ClearSelection();
        UIManager.SelectionClose();
    }

    public void PopUp(SkillSlotDisplay slot)
    {
        Skill skill = skillSlots[slot];
        if (skill==null) return;
        PopUp(skill.skillData);
    }

    public void PopUp(SkillData data)
    {
        if (popup==null) return;
        popup.SetUp(data.displayName,data.desc);
        SetPopUp();
    }

    public void SetPopUp()
    {
        popup.gameObject.SetActive(true);
        Vector2 mousePos = Mouse.current.position.ReadValue();
        RectTransform rect = popup.transform as RectTransform;
        Vector2 pivot = Vector2.zero;
        if (mousePos.x > Screen.width/2f) pivot.x = 1;
        else pivot.x = 0;
        if (mousePos.y > Screen.height/2f) pivot.y = 1;
        else pivot.y = 0;
        rect.pivot = pivot;
        rect.position = mousePos;
    }

    public void Exit()
    {
        if (CloseCoroutine!=null)
        {
            StopCoroutine(CloseCoroutine);
            CloseCoroutine = null;
        }
        Display.SetActive(false);
        ClearSelection();
        UIManager.SelectionClose();
    }


}

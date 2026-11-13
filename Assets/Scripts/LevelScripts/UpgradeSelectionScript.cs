using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
public class UpgradeSelectionScript : MonoBehaviour
{
    public static UpgradeSelectionScript Instance;
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
    [SerializeField] GameObject skillHolderPrefab;
    [SerializeField] GameObject activeSlotPrefab;
    [SerializeField] GameObject Display;
    [SerializeField] SelectionPopupScript popup;
    [SerializeField] RefreshScript refresh;

    Dictionary<UpgradeSlotDisplay,Upgrade> upgradeSlots = new();
    List<GameObject> activeHolders = new();

    public UpgradeSlotDisplay CurrentSlot {get; private set;}
    public UpgradeSlotDisplay SelectedSlot {get; private set;}
    public bool CanNew {get; private set;}
    public bool CanRefresh {get; private set;}
    Coroutine CloseCoroutine;
  

    //Temporary Debugging
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        Display.SetActive(false);
        popup.gameObject.SetActive(false);
        if (refresh!=null) refresh.SetCount(UIManager.Instance.upgradeRefreshCount);
        //PopulateSelection();
    }

    void ClearSelection()
    {
        CurrentSlot = null;
        SelectedSlot = null;
        foreach (UpgradeSlotDisplay slot in upgradeSlots.Keys) Destroy(slot.gameObject);
        foreach (GameObject holder in activeHolders) Destroy(holder);
        upgradeSlots.Clear();
    }

    public int GenTier(float tier1Weight=0.9f, float tier2Weight=0.09f, float tier3Weight=0.01f)
    {
        float sum = tier1Weight+tier2Weight+tier3Weight;
        float val = Random.value*sum;
        return val<=tier1Weight ? 1 : val<=tier1Weight+tier2Weight ? 2 : 3;
    }

    public void PopulateSelection(bool CanRefresh=true, float tier1Weight=0.9f, float tier2Weight=0.09f, float tier3Weight=0.01f)
    {
        Display.SetActive(true);
        refresh.SetCount(UIManager.Instance.upgradeRefreshCount);

        ClearSelection();

        //Get list of possible skills
        List<UpgradeData> upgradeDatas = Resources.LoadAll<UpgradeData>("Upgrades/Debug/").ToList();
        //Retrieve Slots for each active skill
        Dictionary<int,Skill> activeSkills = player.GetSkills();


        //Update saved data
        upgradeSlots.Clear();
        CanNew = true;
        this.CanRefresh = CanRefresh;

        //Display New Upgrade Choices
        for (int i=0; i<3; i++)
        {
            UpgradeSlotDisplay slot = Instantiate(selectionSlotPrefab,selectionHolder).GetComponent<UpgradeSlotDisplay>();
            if (upgradeDatas.Count==0)
            {
                slot.SetUp(null,0,true);
                upgradeSlots.Add(slot,null);
                continue;
            }
            UpgradeData data = upgradeDatas[Random.Range(0,upgradeDatas.Count)];
            Upgrade upgrade = new Upgrade(data,GenTier(tier1Weight,tier2Weight,tier3Weight)); //Add chance for higher lvl upgrades, scaling with player lvl
            upgradeDatas.Remove(data);
            slot.SetUp(upgrade.upgradeData,upgrade.lvl,true);
            upgradeSlots.Add(slot,upgrade);
        }

        for (int i=0; i<4; i++)
        {
            SkillHolderScript skillHolder = Instantiate(skillHolderPrefab,activeHolder).GetComponent<SkillHolderScript>();
            activeHolders.Add(skillHolder.gameObject);
            Skill skill = null;
            if (activeSkills.ContainsKey(i)) skill = activeSkills[i];
            skillHolder.SetUp(i,skill);
            if (skill==null) continue;
            Transform upgradeHolder = skillHolder.holder;
            for (int n=0; n<3; n++)
            {
                UpgradeSlotDisplay slot = Instantiate(activeSlotPrefab,upgradeHolder).GetComponent<UpgradeSlotDisplay>();
                if (skill.upgradeSlots[n]==null)
                {
                    slot.SetUp(null,0,false,i,n);
                    upgradeSlots.Add(slot,null);
                    continue;
                }
                slot.SetUp(skill.upgradeSlots[n].upgradeData,skill.upgradeSlots[n].lvl,false,i,n);
                upgradeSlots.Add(slot,skill.upgradeSlots[n]);
            }
            
        }

        Time.timeScale = 0;
    }

    public void RefreshSelection()
    {
        if (!CanNew || !CanRefresh) return;
        if (!UIManager.Instance.RefreshUpgrade()) return;
        ClearSelection();
        PopulateSelection();
        if (refresh!=null) refresh.SetCount(UIManager.Instance.upgradeRefreshCount);
    }

    public bool OnPickup(UpgradeSlotDisplay slot)
    {
        if (!CanNew && slot.isNew) return false;
        if (CurrentSlot!=null || upgradeSlots[slot]==null) return false;
        CurrentSlot = slot;
        CursorScript.SetCursor(CursorScript.Mode.Hold);
        return true;
    }

    public void OnDrop()
    {
        bool check = Dropped();
        CurrentSlot = null;
        SelectedSlot = null;
        CursorScript.SetCursor(CursorScript.Mode.Pointer);
        if (!CanNew)
        {
            if (CloseCoroutine!=null) StopCoroutine(CloseCoroutine);
            CloseCoroutine = StartCoroutine(CloseDelay(1f));
        }
    }

    void Set(UpgradeSlotDisplay slot, Upgrade upgrade, bool isNew)
    {
        if (upgrade==null) slot.SetUp(null,0,isNew,slot.slot,slot.upgradeSlot);
        else slot.SetUp(upgrade.upgradeData,upgrade.lvl,isNew,slot.slot,slot.upgradeSlot);
        upgradeSlots[slot] = upgrade;
        if (slot.slot>=0) player.UpdateSkillUpgrade(slot.slot,slot.upgradeSlot,upgrade);
    }

    bool Dropped()
    {
        if (CurrentSlot==null || SelectedSlot==null) return false;
        if (CurrentSlot.isNew && SelectedSlot.isNew) return false;
        if (SelectedSlot.isNew && !CanNew) return false;

        if (upgradeSlots[SelectedSlot]==null)
        {
            //Move Function
            return MoveSlot();
        }

        //Conditional to check mergable
        if (Upgrade.CanMerge(upgradeSlots[CurrentSlot],upgradeSlots[SelectedSlot])) //TEMPORARY FIX UNTIL WE CAN MERGE SKILLS
        {
            MergeSlots();
        }
        else if (!CurrentSlot.isNew && !SelectedSlot.isNew)
        {
            //Code to swap
            Upgrade temp = upgradeSlots[CurrentSlot];
            Set(CurrentSlot,upgradeSlots[SelectedSlot],false);
            Set(SelectedSlot,temp,false);
        }
        else if (CurrentSlot.isNew && !SelectedSlot.isNew)
        {
            return MoveSlot();
        }

        return true;
    }

    public bool CanMerge(UpgradeSlotDisplay slot)
    {
        if (Upgrade.CanMerge(upgradeSlots[CurrentSlot],upgradeSlots[slot]))
        {
            PopUp(upgradeSlots[CurrentSlot].upgradeData,upgradeSlots[CurrentSlot].lvl+1);
            return true;
        }
        return false;
    }

    void MergeSlots()
    {
        if (CanNew) CanNew = !CurrentSlot.isNew;
        Upgrade upgrade = new Upgrade(upgradeSlots[CurrentSlot].upgradeData,upgradeSlots[CurrentSlot].lvl+1);
        Set(SelectedSlot,upgrade,false);
        Set(CurrentSlot,null,CurrentSlot.isNew);
    }

    bool MoveSlot()
    {
        //Move Function
        if (CanNew) CanNew = !CurrentSlot.isNew;
        Set(SelectedSlot,upgradeSlots[CurrentSlot],false);
        Set(CurrentSlot,null,CurrentSlot.isNew);
        return !CanNew;
    }

    public void Hover(UpgradeSlotDisplay slot)
    {
        if (CurrentSlot!=null && SelectedSlot==null) SelectedSlot = slot;
        if (CurrentSlot==null && (CanNew || !slot.isNew)) CursorScript.SetCursor(CursorScript.Mode.Grab);
    }

    public void Unhover(UpgradeSlotDisplay slot)
    {
        if (SelectedSlot==slot)
        {
            SelectedSlot = null;
        }
        if (CurrentSlot==null) CursorScript.SetCursor(CursorScript.Mode.Pointer);
        popup.gameObject.SetActive(false);
    }

    public void ClosePopUp()
    {
        popup.gameObject.SetActive(false);
    }

    private IEnumerator CloseDelay(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        while (CurrentSlot!=null) yield return new WaitForSecondsRealtime(0.5f);
        Display.SetActive(false);
        ClearSelection();
        UIManager.SelectionClose();
    }

    public void PopUp(UpgradeSlotDisplay slot)
    {
        Upgrade upgrade = upgradeSlots[slot];
        if (upgrade==null) return;
        PopUp(upgrade);
    }

    public void PopUp(Upgrade upgrade)
    {
        if (popup==null) return;
        popup.SetUp(upgrade.upgradeData.displayName,upgrade.upgradeData.description(upgrade.lvl));
        SetPopUp();
    }

    public void PopUp(UpgradeData data, int lvl)
    {
        if (popup==null) return;
        popup.SetUp(data.displayName,data.description(lvl));
        SetPopUp();
    }

    public void PopUp(int skillSlot)
    {
        if (popup==null) return;
        SkillData data = player.GetSkills()[skillSlot].skillData;
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

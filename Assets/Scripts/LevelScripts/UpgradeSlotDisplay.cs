using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UpgradeSlotDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI displayName;
    [SerializeField] GameObject iconHolder;
    [SerializeField] Image icon;
    [SerializeField] Image lvlIcon;
    [SerializeField] TextMeshProUGUI desc;
    [SerializeField] Image border;
    [SerializeField] Image highlightBorder;
    [SerializeField] Image hoverOverlay;
    [SerializeField] Image hoverIcon;
    
    [SerializeField] List<Sprite> hoverIcons;

    public bool isNew {get; private set;}
    bool isHeld;
    bool hasUpgrade;
    Vector2 iconPos;

    public int slot {get; private set;}
    public int upgradeSlot {get; private set;}

    public void SetUp(UpgradeData data, int lvl, bool isNew, int slot=-1, int upgradeSlot=-1)
    {
        this.upgradeSlot = upgradeSlot;
        this.slot = slot;
        this.isNew = isNew;
        hasUpgrade = data!=null;
        if (!hasUpgrade)
        {   
            if (displayName!=null) displayName.text = "";
            if (icon!=null) icon.enabled = false;
            if (lvlIcon!=null) lvlIcon.enabled = false;
            if (border!=null) border.sprite = Resources.Load<Sprite>("RuneIcons/RuneSlotBorder");
            if (highlightBorder!=null) highlightBorder.sprite = Resources.Load<Sprite>("RuneIcons/RuneSlotHighlight");
            if (desc!=null) desc.text = "";
            if(hoverOverlay!=null) hoverOverlay.sprite = Resources.Load<Sprite>("RuneIcons/RuneSlot");
            SetHoverType(Color.grey);
            return;
        }

        Dictionary<string,Sprite> sprites = data.GetSprites(lvl);

        if (displayName!=null) displayName.text = data.displayName;
        if (icon!=null)
        {
            icon.enabled = true;
            icon.sprite = sprites["Base"];
            icon.color = data.GetColor();
        }
        if (lvlIcon!=null)
        {
            lvlIcon.enabled = true;
            lvlIcon.sprite = sprites["Lvl"];
        }
        if (border!=null) border.sprite = sprites["Border"];
        if (highlightBorder!=null) highlightBorder.sprite = sprites["Highlight"];
        if (desc!=null) desc.text = data.description(lvl);
        if(hoverOverlay!=null) hoverOverlay.sprite = icon.sprite;
        SetHoverType(Color.grey);
    }

    void Update()
    {
        if (!isHeld) return;
        iconHolder.transform.position = Mouse.current.position.ReadValue();
        //Add rot by speed
    }

    public void OnHold()
    {
        if (!UpgradeSelectionScript.Instance.OnPickup(this)) return;

        isHeld = true;
        if (iconHolder!=null) iconPos = iconHolder.transform.position;
        SetHoverType(Color.yellow);

    }

    public void OnHover()
    {
        if (UpgradeSelectionScript.Instance.CurrentSlot==this) return;
        if (UpgradeSelectionScript.Instance.CurrentSlot==null)
        {
            //Highlight to pickup
            if (!hasUpgrade) return;
            if (hasUpgrade && border!=null) SetHoverType(Color.white,-1,true);
            UpgradeSelectionScript.Instance.Hover(this);
            if (!isNew) UpgradeSelectionScript.Instance.PopUp(this);
        }
        else if (!isNew && UpgradeSelectionScript.Instance.SelectedSlot==null)
        {
            UpgradeSelectionScript.Instance.Hover(this);
            //Highlight to merge / replace & move(empty)
            if (hasUpgrade)
            {
                if (UpgradeSelectionScript.Instance.CanMerge(this)) SetHoverType(Color.purple, 3);
                else if (UpgradeSelectionScript.Instance.CurrentSlot.isNew) SetHoverType(Color.red, 1);
                else SetHoverType(Color.cyan, 2);
            }
            else SetHoverType(Color.limeGreen, 0);
        }
    }

    private void SetHoverType(Color borderColor, int iconN=-1, bool highlight=false)
    {
        Image activeBorder = border;
        if (highlightBorder!=null)
        {
            activeBorder = highlight ? highlightBorder : border;
            border.enabled = !highlight;
            highlightBorder.enabled = highlight;
        }
        
        if (activeBorder!=null) activeBorder.color = borderColor;
        if (hoverIcon==null || hoverOverlay==null || hoverIcons==null || hoverIcons.Count==0) return;
        if (iconN==-1)
        {
            hoverIcon.enabled=false;
            hoverOverlay.enabled=false;
            return;
        }
        hoverIcon.sprite = hoverIcons[iconN];
        hoverIcon.enabled = true;
        hoverOverlay.enabled = true;
    }

    public void OnUnhover()
    {
        if (isHeld) return;
        //Reset highlight
        UpgradeSelectionScript.Instance.Unhover(this);
        SetHoverType(Color.grey);
    }

    public void OnDrop()
    {
        if (!isHeld) return;

        //Code to drop on slot
        // RaycastHit2D[] hits = Physics2D.CircleCastAll(icon.transform.position,)

        UpgradeSelectionScript.Instance.OnDrop();

        isHeld = false;
        //Reset position
        if (iconHolder!=null) iconHolder.transform.position = iconPos;
        SetHoverType(Color.grey);
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SkillSlotDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI displayName;
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI desc;
    [SerializeField] Image overlay;
    [SerializeField] Image border;
    [SerializeField] Image highlightBorder;
    [SerializeField] Image hoverOverlay;
    [SerializeField] Image hoverIcon;
    
    [SerializeField] List<Sprite> hoverIcons;

    Coroutine shakeCoroutine;


    public bool isNew {get; private set;}
    bool isHeld;
    bool hasSkill;
    Vector2 iconPos;
    public int slot {get; private set;}
    public bool isLocked {get; private set;}
    bool hasMerge;

    public void SetUp(SkillData skillData, bool isNew, int slot=-1, bool hasMerge=false, bool locked=false)
    {
        this.slot = slot;
        this.isNew = isNew;
        hasSkill = skillData!=null;
        isLocked = locked;
        this.hasMerge = hasMerge;
        if (border!=null) border.color = Color.grey;
        if (!hasSkill)
        {   
            if (displayName!=null) displayName.text = "";
            if (icon!=null)
            {
                icon.sprite = null;
                icon.enabled = false;
            }
            if (desc!=null) desc.text = "";
            if (overlay!=null) overlay.color = new Color(0.5f,0.5f,0.5f,0.5f);
            SetHoverType(Color.grey);
            return;
        }

        if (overlay!=null) overlay.color = new Color(0,0,0,0);
        if (displayName!=null) displayName.text = skillData.displayName;
        if (icon!=null)
        {
            icon.enabled = true;
            icon.sprite = skillData.icon;
        }
        if (desc!=null) desc.text = skillData.desc;
        SetHoverType(Color.grey);
    }

    void Update()
    {
        if (!isHeld) return;
        icon.transform.position = Mouse.current.position.ReadValue();
        //Add rot by speed
    }

    public void OnHold()
    {
        if (isLocked || !SelectionScript.Instance.OnPickup(this)) return;

        isHeld = true;
        if (icon!=null) iconPos = icon.transform.position;
        SetHoverType(Color.yellow);

    }

    public void OnHover()
    {
        if (isLocked || SelectionScript.Instance.CurrentSlot==this) return;
        if (SelectionScript.Instance.CurrentSlot==null)
        {
            //Highlight to pickup
            if (!hasSkill) return;
            SetHoverType(Color.white,-1,true);
            SelectionScript.Instance.Hover(this);
            if (!isNew) SelectionScript.Instance.PopUp(this);
        }
        else if (!isNew && SelectionScript.Instance.SelectedSlot==null)
        {
            SelectionScript.Instance.Hover(this);
            //Highlight to merge / replace & move(empty)
            if (hasSkill)
            {
                if (SelectionScript.Instance.CanMerge(this)) SetHoverType(Color.purple, 3, true);
                else if (SelectionScript.Instance.CurrentSlot.isNew) SetHoverType(Color.red, 1);
                else SetHoverType(Color.cyan, 2, true);
            }
            else SetHoverType(Color.limeGreen, 0, true);
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
        if (isLocked || isHeld) return;
        //Reset highlight
        SelectionScript.Instance.Unhover(this);
        SetHoverType(Color.grey);
    }

    public void OnDrop()
    {
        if (isLocked || !isHeld) return;

        //Code to drop on slot
        // RaycastHit2D[] hits = Physics2D.CircleCastAll(icon.transform.position,)

        SelectionScript.Instance.OnDrop();

        isHeld = false;
        //Reset position
        if (icon!=null) icon.transform.position = iconPos;
        SetHoverType(Color.grey);

    }

    public void Shake(bool shake)
    {
        if (shakeCoroutine!=null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
            if (border!=null) border.transform.rotation = Quaternion.identity;
            if (highlightBorder!=null) highlightBorder.transform.rotation = Quaternion.identity;
        }

        if (!shake) return;

        shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        if (border==null && highlightBorder==null) yield break;
        while (true)
        {
            for (int dir=1; dir>=-1; dir-=2)
                for (int rot=0; rot<=4; rot++)
                {
                    int r = rot<=2 ? dir : -dir;
                    border.transform.Rotate(new Vector3(0,0,r));
                    highlightBorder.transform.Rotate(new Vector3(0,0,r));
                    //quaternion.Euler(0,0,dir*rot<=15 ? rot : 30-rot);
                    yield return new WaitForSecondsRealtime(0.05f);
                }
            yield return new WaitForSecondsRealtime(1);
        }
    }

    void OnDisable()
    {
        if (shakeCoroutine!=null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
    }
}

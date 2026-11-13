using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkillHolderScript : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Image border;
    public Transform holder;

    public int slot { get; private set; }
    bool hasSkill;

    public void SetUp(int slot, Skill skill)
    {
        this.slot = slot;
        hasSkill = skill!=null;
        if (!hasSkill)
        {
            if (icon!=null) icon.enabled = false;
            return;
        }
        if (icon!=null)
        {
            icon.sprite = skill.skillData.icon;
            icon.enabled = true;
        }
    }

    public void OnHover()
    {
        if (!hasSkill) return;
        UpgradeSelectionScript.Instance.PopUp(slot);
    }

    public void OnUnhover()
    {
        if (!hasSkill) return;
        UpgradeSelectionScript.Instance.ClosePopUp();
    }



}

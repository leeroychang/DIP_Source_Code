using System.Collections.Generic;
using UnityEngine;

public class Upgrade
{
    public UpgradeData upgradeData;
    public int lvl;

    public Upgrade(UpgradeData data, int lvl=1)
    {
        upgradeData = data;
        this.lvl = lvl;
    }

    public static bool CanMerge(Upgrade u1, Upgrade u2)
    {
        if (u1==null || u2==null) return false;
        if (u1.upgradeData != u2.upgradeData) return false;
        if (u1.lvl!=u2.lvl) return false;
        return u1.lvl<u1.upgradeData.MaxLevel();
    }

    public void OnEnable(Skill skill, int slot)
    {
        foreach (Modifier modifier in upgradeData.GetModifiers(lvl)) modifier.OnEnable(skill,slot);
    }

    public void OnDisable(Skill skill, int slot)
    {
        foreach (Modifier modifier in upgradeData.GetModifiers(lvl)) modifier.OnDisable(skill,slot);
    }

    


}

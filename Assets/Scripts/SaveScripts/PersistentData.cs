using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentData : Singleton<PersistentData>
{
    //Player Data
    public int lvl;
    public int xp;
    public int hp;
    public Dictionary<int,Skill> skills = new();
    public Dictionary<EntityData.Stat,float> statModifier;
    public bool loadSave=false;




    // Run Data

    public int kills;
    public float StartTime;
    public float EndTime;
    public float BossStartTime;

    public static bool load { get { return Instance==null ? false : Instance.loadSave; } }

    public static void Save()
    {
        if (Instance==null) return;
        Instance.SaveData();
    }

    public void SaveData()
    {
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        skills = player.GetSkills();
        foreach (Skill skill in skills.Values)
        {
            if (skill==null) continue;
            for (int i=0; i<3; i++)
            {
                Upgrade upgrade = skill.upgradeSlots[i];
                if(upgrade!=null) upgrade.OnDisable(skill,i);
            }
        }
        statModifier = player.GetStatModifiers();
        hp = player.HP;
        lvl = UIManager.Instance.level;
        xp = UIManager.Instance.xp;
        loadSave = true;
    }

    public static void Clear(bool clearAll=false)
    {
        if (Instance==null) return;
        Instance.ClearData(clearAll);
    }

    public void ClearData(bool clearAll)
    {
        lvl = 1;
        xp = 0;
        skills.Clear();
        loadSave = false;
        if (clearAll)
        {
            kills = 0;
            StartTime = 0;
            EndTime = 0;
            BossStartTime = 0;
        }
    }

    public static void DelayClear() { if (Instance!=null) Instance.StartCoroutine(Instance.Delayed()); }

    public IEnumerator Delayed()
    {
        yield return new WaitForEndOfFrame();
        Clear();
    }

    public static void AddKill() { if (Instance!=null) Instance.kills++; }
    
}
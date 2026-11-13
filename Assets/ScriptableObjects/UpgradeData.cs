using System;
using System.Collections.Generic;
using System.Linq;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "newUpgradeData", menuName = "Data/Upgrade", order = 4)]
public class UpgradeData : ScriptableObject
{
    public string displayName;
    [SerializeField] UpgradeType type;
    [SerializeField] ShapeType shape;
    [SerializeReference][SR] List<ModifierSet> lvlModifiers;

    public List<Modifier> GetModifiers(int lvl)
    {
        if (lvl<=0 || lvl>lvlModifiers.Count) return null;
        return lvlModifiers[lvl-1].modifiers;
    }

    public string description(int lvl)
    {
        if (lvl<=0 || lvl>lvlModifiers.Count) return "";
        return lvlModifiers[lvl-1].desc;
    }

    public int MaxLevel() { return lvlModifiers.Count; }

    public Color GetColor()
    {
        switch (type)
        {
            case UpgradeType.Offense:
                return Color.softRed;
            case UpgradeType.Utility:
                return Color.cyan;
            case UpgradeType.Survivability:
                return Color.limeGreen;
            case UpgradeType.OnHit:
                return Color.orange;
            default:
                return Color.gray;
        }
    }

    public Dictionary<string,Sprite> GetSprites(int lvl)
    {
        Dictionary<string,Sprite> sprites = new()
        {
            {"Base",null},
            {"Border",null},
            {"Highlight",null},
            {"Lvl",null}
        };

        string shapeName;

        switch (shape)
        {
            case ShapeType.Triangle:
                shapeName = "Triangle";
                break;
            case ShapeType.Oval:
                shapeName = "Oval";
                break;
            case ShapeType.Diamond:
                shapeName = "Diamond";
                break;
            default:
                shapeName = "Triangle";
                break;
        }
        
        foreach (string key in sprites.Keys.ToList())
        {
            if (key=="Lvl") sprites[key] = Resources.LoadAll<Sprite>("RuneIcons/Rune"+shapeName+"Letters")[lvl-1];
            else sprites[key] = Resources.Load<Sprite>("RuneIcons/Rune"+shapeName+key);
            if (sprites[key]==null) Debug.Log($"No sprite found at "+"RuneIcons/Rune"+shapeName+(key=="Lvl" ? "Letters_"+lvl.ToString() : key));
        }

        return sprites;
    }

    enum UpgradeType
    {
        Offense, //Damage, Crit, Count, OnHit, 
        Utility, //CD, 
        Survivability, //Lifesteal, Health Orbs
        OnHit //OnHit effects
    }

    enum ShapeType
    {
        Triangle,
        Oval,
        Diamond
    }

    [Serializable]
    private class ModifierSet
    {
        [SerializeReference][SR] public List<Modifier> modifiers;
        public string desc;
    }



}

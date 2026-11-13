using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "newSkillData", menuName = "Data/Skill", order = 2)]
public class SkillData : ScriptableObject
{
    [Header("Skill Stats")]

    public string displayName;
    public Sprite icon;
    public string desc;
    public Element element;
    public Element elementMerge;
    public bool altAtk=false;

    [SerializeReference]
    [SR]
    SkillType _skillType;

    public SkillType skillType
    {
        get { return _skillType; }
    }


}

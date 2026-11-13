using System;
using UnityEngine;

[CreateAssetMenu(fileName = "newSkillData", menuName = "Data/MergedSkill", order = 3)]
public class MergedSkillData : SkillData
{
    [SerializeField] SkillData MergeSkill1;
    [SerializeField] SkillData MergeSkill2;
    public bool showList = true;
    public (SkillData,SkillData) mergePair { get { return (MergeSkill1,MergeSkill2); } set {} }


}

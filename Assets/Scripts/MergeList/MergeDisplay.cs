using UnityEngine;
using UnityEngine.UI;

public class MergeDisplay : MonoBehaviour
{
    [SerializeField] Image skill1Display;
    [SerializeField] Image skill2Display;
    [SerializeField] Image skillMergeDisplay;

    SkillData skill1;
    SkillData skill2;
    SkillData skillMerge;


    public void SetUp(SkillData s1, SkillData s2, SkillData s3)
    {
        skill1 = s1;
        skill2 = s2;
        skillMerge = s3;

        if (skill1Display!=null && s1!=null) skill1Display.sprite = s1.icon;
        if (skill2Display!=null && s2!=null) skill2Display.sprite = s2.icon;
        if (skillMergeDisplay!=null && s3!=null) skillMergeDisplay.sprite = s3.icon;
    }

    public void Hover(int slot)
    {
        SkillData data = null;
        switch (slot)
        {
            case 1:
                data = skill1;
                break;
            case 2:
                data = skill2;
                break;
            case 3:
                data = skillMerge;
                break;
            
        }
        MergeListManager.Instance.PopUp(data);
    }

    public void UnHover()
    {
        MergeListManager.Instance.UnPopUp();
    }

    
}

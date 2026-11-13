using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MergeListManager : MonoBehaviour
{
    [SerializeField] GameObject MergePrefab;
    [SerializeField] Transform contentHolder;
    [SerializeField] SelectionPopupScript popup;

    List<MergedSkillData> skills;

    public static MergeListManager Instance;
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


    public void Populate()
    {
        skills = Resources.LoadAll<MergedSkillData>("Skills/Merged/").ToList();

        foreach (MergedSkillData mergeSkill in skills)
        {
            if (!mergeSkill.showList) continue;
            MergeDisplay display = Instantiate(MergePrefab,contentHolder).GetComponent<MergeDisplay>();
            (SkillData,SkillData) mergePair = mergeSkill.mergePair;
            display.SetUp(mergePair.Item1,mergePair.Item2,mergeSkill);
        }
    }

    public void PopUp(SkillData data)
    {
        if (popup==null) return;
        if (data==null) popup.SetUp("???", "Obtain to unlock...");
        else popup.SetUp(data.displayName,data.desc);
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

    public void UnPopUp()
    {
        if (popup!=null) popup.gameObject.SetActive(false);
    }

    public void UnPopulate()
    {
        foreach (Transform child in contentHolder) Destroy(child.gameObject);
        gameObject.SetActive(false);
    }





}

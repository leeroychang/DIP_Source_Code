using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreSkillDisplayScript : MonoBehaviour
{
    [SerializeField] GameObject runePrefab;
    [SerializeField] Image iconDisplay;
    [SerializeField] TextMeshProUGUI textDisplay;
    [SerializeField] Transform runeHolder;

    public void SetUp(Skill skill)
    {
        if (skill==null)
        {
            for (int i=0; i<3; i++) Instantiate(runePrefab,runeHolder);
            return;
        }

        iconDisplay.sprite = skill.skillData.icon;
        textDisplay.text = skill.skillData.displayName;
        List<Upgrade> upgrades = skill.upgradeSlots.ToList();
        string[] lvlText = new string[] {"I","II","III"};
        for (int i=0; i<3; i++)
        {
            RuneDisplayScript display = Instantiate(runePrefab,runeHolder).GetComponent<RuneDisplayScript>();
            if (upgrades[i]==null) continue;
            Upgrade upgrade = upgrades[i];
            Dictionary<string,Sprite> sprites = upgrade.upgradeData.GetSprites(upgrade.lvl);
            display.SetUp(sprites["Base"],upgrade.upgradeData.GetColor(),sprites["Lvl"],$"{upgrade.upgradeData.displayName} {lvlText[upgrade.lvl-1]}");
        }
    }
}

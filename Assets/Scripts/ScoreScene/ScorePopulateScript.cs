using System.IO;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScorePopulateScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI statDisplay;
    [SerializeField] Transform skillHolder;
    
    [SerializeField] GameObject skillPrefab;



    void Start()
    {
        if (PersistentData.Instance==null) return;
        // Display Stats
        float runTime = PersistentData.Instance.EndTime-PersistentData.Instance.StartTime;
        float bossTime = PersistentData.Instance.BossStartTime==-1 ? -1 : PersistentData.Instance.EndTime-PersistentData.Instance.BossStartTime;
        statDisplay.text =
            $"Time: {ConvertTime(runTime)}\n\n" +
            $"Boss Time: {(bossTime==-1 ? "N/A" : ConvertTime(bossTime))}\n\n" +
            $"Kills: {PersistentData.Instance.kills}\n\n" +
            $"Level: {PersistentData.Instance.lvl}";

        // Display Skills & Upgrades
        Dictionary<int,Skill> skills = new (PersistentData.Instance.skills);

        for (int i=0; i<4; i++)
        {
            if (!skills.ContainsKey(i))
            {
                ScoreSkillDisplayScript nulldisplay = Instantiate(skillPrefab,skillHolder).GetComponent<ScoreSkillDisplayScript>();
                nulldisplay.SetUp(null);
                continue;
            }
            Skill skill = skills[i];
            ScoreSkillDisplayScript display = Instantiate(skillPrefab,skillHolder).GetComponent<ScoreSkillDisplayScript>();
            display.SetUp(skill);
        }

        PersistentData.Clear(true);
        CursorScript.SetCursor(CursorScript.Mode.Pointer);
    }


    string ConvertTime(float duration)
    {
        int minTime = (int) duration / 60;
        int sTime = (int) duration % 60;
        return $"{minTime} min {sTime} s";
    }


    public void ChangeScene()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void TakeScreenshot()
    {
        // Generate a unique file name using the current timestamp to avoid overwriting
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"Protocol_{timestamp}.png";
        if (Application.isEditor) ScreenCapture.CaptureScreenshot(filename, 1);
        else
        {
            string folderPath = Path.Combine(Application.persistentDataPath, "Screenshots");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath); //
            }
            string finalPath = Path.Combine(folderPath, filename);
            ScreenCapture.CaptureScreenshot(finalPath, 1);
        }

    }


}

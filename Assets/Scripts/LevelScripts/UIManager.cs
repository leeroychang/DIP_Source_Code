using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("References")]
    public GameObject XPOrb;
    public GameObject HPOrb;
    [SerializeField] GameObject CombatUI;
    [SerializeField] GameObject GameOverUI;
    [SerializeField] GameObject lvlPopup;
    [SerializeField] Slider xpBar;
    [SerializeField] TextMeshProUGUI lvlText;
    [SerializeField] TextMeshProUGUI xpText;
    [SerializeField] GameObject settings;
    [SerializeField] InputActionAsset inputAsset;
    [SerializeField] GameObject LvlEffectPrefab;
    [SerializeField] GameObject DmgPopUpPrefab;
    InputAction closeAction;
    InputAction selectionAction;




    PlayerController player;
    public int level;
    public int xp;
    public int skillRefreshCount { get; private set; }
    public int upgradeRefreshCount { get; private set; }
    public bool inUpgrade;
    public bool inSkill;
    public bool canSelect;
    public bool gameOver;

    private void Awake() {
        if (Instance!=null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        level = 1;
        xp = 0;
        skillRefreshCount = 3;
        upgradeRefreshCount = 5;
        closeAction = inputAsset.FindAction("Close");
        closeAction.performed += Close;
        selectionAction = inputAsset.FindAction("Selection");
        selectionAction.performed += SelectionOpen;
        GameOverUI.SetActive(false);
        canSelect = false;
        if (lvlPopup!=null) lvlPopup.SetActive(false);

        if (PersistentData.load)
        {
            level = PersistentData.Instance.lvl;
            xp = PersistentData.Instance.xp;
            XPUP(0);
            PersistentData.DelayClear();
        }

    }

    void Start()
    {
        CursorScript.SetCursor(CursorScript.Mode.Target);
    }

    private void OnDestroy() {
        if (Instance==this) Instance = null;
        closeAction.performed -= Close;
        selectionAction.performed -= SelectionOpen;
    }

    public void Close(InputAction.CallbackContext context)
    {
        if (gameOver) return;
        if (inUpgrade) UpgradeSelectionScript.Instance.Exit();
        else if (inSkill) SelectionScript.Instance.Exit();
        else
        {
            settings.gameObject.SetActive(!settings.gameObject.activeSelf);
            if (MergeListManager.Instance!=null) MergeListManager.Instance.UnPopulate();
            Time.timeScale = settings.gameObject.activeSelf ? 0 : 1;
        }
    }



    public static void SelectionClose()
    {
        if (Instance==null) return;
        Time.timeScale = 1;
        CursorScript.SetCursor(CursorScript.Mode.Target);
        Instance.OnSelectionClose();
    }

    private void OnSelectionClose()
    {
        CombatUI.SetActive(true);
        inUpgrade = false;
        inSkill = false;
        int req = XPReq(level);
        if (xp>=req) SelectionOpen();
        else UpdateXPBar(xp,req);
        
    }


    public void XPUP(int exp)
    {
        xp += exp;
        int req = XPReq(level);


        if (xp>=req)
        {
            EnableSelection();
            UpdateXPBar(req,req);
            Instantiate(LvlEffectPrefab).transform.position = player.transform.position;
        }

        UpdateXPBar(xp,req);
    }
    
    public void LVLUP()
    {
        XPUP(XPReq(level));
    }

    public void UpdateXPBar(int xp, int req)
    {
        if (xpBar==null) return;
        xpBar.maxValue = req;
        xpBar.value = xp;

        lvlText.text = $"Lvl {level}";
        xpText.text = $"{xp}/{XPReq(level)}";
    }
    



    private int XPReq(int lvl)
    {
        //Insert XP requirement formula
        return lvl*2+1; //temp formula
    }

    public void EnableSelection()
    {
        canSelect = true;
        if (lvlPopup!=null) lvlPopup.SetActive(true);
    }

    public void SelectionOpen(InputAction.CallbackContext context)
    {
        if (!canSelect || settings.gameObject.activeSelf) return;
        SelectionOpen();
    }

    public void TreasureOpen(float tier1Weight, float tier2Weight, float tier3Weight)
    {
        Time.timeScale = 0;
        CursorScript.SetCursor(CursorScript.Mode.Pointer);
        canSelect = false;

        inUpgrade = true;
        UpgradeSelectionScript.Instance.PopulateSelection(false,tier1Weight,tier2Weight,tier3Weight);
        CombatUI.SetActive(false);
        if (lvlPopup!=null) lvlPopup.SetActive(false);

    }

    public void SelectionOpen()
    {
        Time.timeScale = 0;
        canSelect = false;
        int req = XPReq(level);
        xp -= req;
        level++;
        if (level%5==0)
        {
            skillRefreshCount++;
            upgradeRefreshCount++;
        }

        if (level%3==1)
        {
            inSkill = true;
            SelectionScript.Instance.PopulateSelection();
        }
        else
        {
            inUpgrade = true;
            UpgradeSelectionScript.Instance.PopulateSelection();
        }
        CombatUI.SetActive(false);
        if (lvlPopup!=null) lvlPopup.SetActive(false);
        UpdateXPBar(xp,req);
        CursorScript.SetCursor(CursorScript.Mode.Pointer);
    }

    public bool RefreshSkill()
    {
        if (skillRefreshCount==0) return false;
        skillRefreshCount--;
        return true;
    }

    public bool RefreshUpgrade()
    {
        if (upgradeRefreshCount==0) return false;
        upgradeRefreshCount--;
        return true;
    }

    public void DMG(int dmg, Vector3 pos, bool ally, bool heal=false)
    {
        if (DmgPopUpPrefab==null) return;
        DamagePopUpScript popup = Instantiate(DmgPopUpPrefab).GetComponent<DamagePopUpScript>();
        popup.SetUp(dmg,pos,ally,heal);
    }

    public void GameOver()
    {
        gameOver = true;
        Time.timeScale = 0;
        GameOverUI.SetActive(true);
    }

    public void ChangeScene(bool close)
    {
        Time.timeScale = 1;
        // PersistentData.Clear(true);
        PersistentData.Save();
        if (PersistentData.Instance!=null)
        {
            PersistentData.Instance.EndTime = Time.time;
            if (BossSceneScript.Instance==null || !BossSceneScript.Instance.bossBeat) PersistentData.Instance.BossStartTime = -1;
        }
        if (close)
        {
            Application.Quit();
            return;
        }
        SceneManager.LoadScene("ScoreScene");
    }

    public void NextLevel()
    {
        PersistentData.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }



}

using UnityEngine;

public class BossSceneScript : MonoBehaviour
{
    public static BossSceneScript Instance;
    public bool bossBeat=false;

    private void Awake() {
        if (Instance!=null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (PersistentData.Instance!=null) PersistentData.Instance.BossStartTime = Time.time;
    }

    public void BossDown()
    {
        bossBeat = true;
        if (UIManager.Instance!=null) UIManager.Instance.ChangeScene(false);
    }
}

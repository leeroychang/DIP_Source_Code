using UnityEngine;

public class TreasureScript : MonoBehaviour
{
    [Header("Upgrade Tiers")]
    [SerializeField] float tier1Weight = 0.7f;
    [SerializeField] float tier2Weight = 0.25f;
    [SerializeField] float tier3Weight = 0.05f;

    [Header("XP Values")]
    [SerializeField] int minXP = 10;
    [SerializeField] int maxXP = 20;

    [Header("Check Values")]
    [SerializeField] float checkRange = 15f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag!="Player") return;

        //Check if enemies nearby
        Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position,checkRange);
        foreach (Collider2D coll in colls)
        {
            if (coll.TryGetComponent(out Entity entity))
            {
                if (entity.ally) continue;
                Shake();
                return;
            }
        }

        if (UIManager.Instance==null) return;
    
        UIManager.Instance.TreasureOpen(tier1Weight,tier2Weight,tier3Weight);

        XPOrbScript xpOrb = Instantiate(UIManager.Instance.XPOrb).GetComponent<XPOrbScript>();
        xpOrb.SetUp(transform.position, Random.Range(minXP,maxXP+1));    

        Destroy(gameObject);
    
    }


    public void Shake()
    {
        Debug.Log("Shake... (Kill nearby enemies first)");
    }


}

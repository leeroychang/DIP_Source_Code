using Unity.Mathematics;
using UnityEngine;

public class XPOrbScript : MonoBehaviour
{
    public static PlayerController player;
    int xpValue;

    public void SetUp(Vector2 pos, int xpValue)
    {
        if (player==null) player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        this.xpValue = xpValue;
        transform.position = pos;
    }

    void FixedUpdate()
    {
        Vector2 dir = player.transform.position-transform.position;
        if (dir.magnitude <= 0.5f) Collect();
        else transform.Translate(dir.normalized*math.clamp(dir.magnitude,15,30)*Time.fixedDeltaTime);
    }

    void Collect()
    {
        UIManager.Instance.XPUP(xpValue);
        Destroy(gameObject);
    }


}

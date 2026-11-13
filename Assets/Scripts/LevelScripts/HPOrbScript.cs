using Unity.Mathematics;
using UnityEngine;

public class HPOrbScript : MonoBehaviour
{
    public static PlayerController player;
    int hpValue;

    public void SetUp(Vector2 pos, int hpValue)
    {
        if (player==null) player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        this.hpValue = hpValue;
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
        player.Heal(hpValue);
        Destroy(gameObject);
    }


}

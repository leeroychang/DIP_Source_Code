using System.Collections;
using TMPro;
using UnityEngine;

public class DamagePopUpScript : MonoBehaviour
{
    static float minSize = 10;
    static float maxSize = 20;
    static int minDmg = 5;
    static int maxDmg = 100;
    static int boldCutoff = 50;
    static float speed = 15f;

    [SerializeField] TextMeshProUGUI text;
    float duration;
    Vector3 direction;

    public void SetUp(int dmg, Vector3 pos, bool ally, bool heal=false)
    {
        transform.position = pos + Vector3.up;
        float ratio = Mathf.Clamp(dmg/(maxDmg-minDmg),0f,1f);
        float size = (ratio * (maxSize-minSize)) + minSize;
        duration = size/20f;
        
        float angle = Random.Range(-45f,45f);
        direction = Quaternion.Euler(0,0,angle)*Vector2.up;
        bool bold = dmg>=boldCutoff;
        text.text = dmg.ToString();
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.fontSize = size;

        if (heal) text.color = Color.mediumSeaGreen;
        else if (ally) text.color = Color.Lerp(Color.darkRed,Color.crimson,ratio);
        else text.color = Color.Lerp(Color.gray7,Color.whiteSmoke,ratio);
        // switch (element)
        // {
        //     case Element.None:
        //         text.color = Color.gray7;
        //         break;
        //     case Element.Fire:
        //         text.color = Color.darkRed;
        //         break;
        //     case Element.Earth:
        //         text.color = Color.saddleBrown;
        //         break;
        //     case Element.Ice:
        //         text.color = Color.teal;
        //         break;
        //     case Element.Electric:
        //         text.color = Color.rebeccaPurple;
        //         break;
        //     default:
        //         text.color = Color.gray7;
        //         break;
        // }
    }

    void FixedUpdate()
    {
        float timer = Time.fixedDeltaTime;
        transform.position += duration * speed * timer * direction;
        duration -= timer;
        if (duration<=0) Destroy(gameObject);
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusDisplayScript : MonoBehaviour
{

    [SerializeField] Image icon;
    [SerializeField] Slider fill;
    [SerializeField] TextMeshProUGUI stackText;

    public void SetUp(StatusEffect statusEffect)
    {
        Sprite sprite = statusEffect.icon;
        if (sprite!=null) icon.sprite = sprite;
        fill.maxValue = statusEffect.maxDuration;
        fill.value = statusEffect.duration;
        stackText.text = statusEffect.stacks<=1 ? "" : statusEffect.stacks.ToString();

    }

    public void UpdateDisplay(float duration, int stacks=0, float maxDuration=0)
    {
        if (maxDuration!=0) fill.maxValue = maxDuration;
        if (stacks!=0) stackText.text = stacks<=1 ? "" : stacks.ToString();
        fill.value = duration;
    }



}

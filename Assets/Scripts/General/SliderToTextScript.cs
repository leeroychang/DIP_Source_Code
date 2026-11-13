using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderToTextScript : MonoBehaviour
{
    Slider slider;
    [SerializeField] TextMeshProUGUI text;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void SetText()
    {
        if (text==null) return;
        if (slider==null) GetComponent<Slider>();
        if (slider.value==0 && slider.maxValue==0) text.text = "";
        else text.text = $"{slider.value}/{slider.maxValue}";
    }
}

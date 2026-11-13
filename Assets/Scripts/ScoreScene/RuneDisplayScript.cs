using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RuneDisplayScript : MonoBehaviour
{
    [SerializeField] Image iconDisplay;
    [SerializeField] Image lvlDisplay;
    [SerializeField] TextMeshProUGUI textDisplay;

    public void SetUp(Sprite icon, Color iconColor, Sprite lvlIcon, string displayName)
    {
        iconDisplay.sprite = icon;
        iconDisplay.color = iconColor;
        lvlDisplay.sprite = lvlIcon;
        textDisplay.text = displayName;
    }
}

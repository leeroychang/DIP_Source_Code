using UnityEngine;
using UnityEngine.UI;

public class ActiveSkillSlot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Image icon;
    [SerializeField] Slider fill;
    [SerializeField] Image border;
    [SerializeField] Image hotkey;

    public void UpdateDisplay(Sprite icon, float cd, float maxCD, Sprite hotkey=null)
    {
        this.icon.sprite = icon;
        this.icon.enabled = icon!=null;
        fill.maxValue = maxCD;
        fill.value = cd;
        this.icon.color = cd==0 ? Color.white : Color.gray;
        if (this.hotkey!=null && hotkey!=null) this.hotkey.sprite = hotkey;
    }
    
}

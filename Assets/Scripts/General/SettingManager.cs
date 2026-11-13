using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [SerializeField] GameObject holder;
    [SerializeField] Slider volumeSlider;
    [SerializeField] Slider brightnessSlider;
    [SerializeField] Image volumeIcon;
    [SerializeField] Image brightnessIcon;
    [SerializeField] List<Sprite> volumeSprites;
    [SerializeField] List<Sprite> brightnessSprites;
    [SerializeField] Image brightnessFilter;
    [SerializeField] MergeListManager mergeManager;

    [SerializeField] bool toggleState = false;

    float volume;
    float brightness;

    void Awake()
    {
        volume = PlayerPrefs.GetFloat("Volume",0.5f);
        brightness = PlayerPrefs.GetFloat("Brightness",0.5f);
        if (volumeSlider!=null) volumeSlider.value = volume;
        if (brightnessSlider!=null) brightnessSlider.value = brightness;
        AudioListener.volume = volume;
        if (brightnessFilter!=null) brightnessFilter.color = new Color(0,0,0,0.9f-brightness*0.9f);
        holder.SetActive(!toggleState);
        mergeManager.gameObject.SetActive(false);
    }

    public void SetVolume()
    {
        if (volumeSlider==null) return;
        volume = volumeSlider.value;
        PlayerPrefs.SetFloat("Volume",volume);
        AudioListener.volume = volume;
        if (volumeIcon==null || volumeSprites==null || volumeSprites.Count==0) return;
        if (volume==0) volumeIcon.sprite = volumeSprites[0];
        else if (volume==1) volumeIcon.sprite = volumeSprites.Last();
        else volumeIcon.sprite = volumeSprites[(int) (volume*(volumeSprites.Count-1))+1];
    }

    public void SetBrightness()
    {
        if (brightnessSlider==null) return;
        brightness = brightnessSlider.value;
        PlayerPrefs.SetFloat("Brightness",brightness);
        if (brightnessFilter!=null) brightnessFilter.color = new Color(0,0,0,0.9f-brightness*0.9f);
        if (brightnessIcon==null || brightnessSprites==null || brightnessSprites.Count==0) return;
        if (brightness==0) brightnessIcon.sprite = brightnessSprites[0];
        else if (brightness==1) brightnessIcon.sprite = brightnessSprites.Last();
        else brightnessIcon.sprite = brightnessSprites[(int) (brightness*(brightnessSprites.Count-1))+1];
    }

    public void OpenMerge()
    {
        if (mergeManager==null) return;
        mergeManager.gameObject.SetActive(true);
        mergeManager.Populate();
    }
}

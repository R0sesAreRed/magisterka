using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{

    private bool isInitializing = false;
   
    public Slider volumeSlider;
    public TMP_Text volumeValueText;
    void Start()
    {
        volumeSlider.value = GameManager.instance.volume;
        volumeValueText.text = Mathf.RoundToInt(GameManager.instance.volume * 100).ToString() + "%";
        isInitializing = false;
    }
    public void VolumeChanged()
    {
        if (isInitializing) return;
        GameManager.instance.volume = volumeSlider.value;
        volumeValueText.text = Mathf.RoundToInt(GameManager.instance.volume * 100).ToString() + "%";
    }
}

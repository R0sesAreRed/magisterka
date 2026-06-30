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
    private float volumeValue;

    public Slider pianoSlider;
    public TMP_Text pianoSliderText;
    private float pianoVolumeValue;

    public Slider metronomeSlider;
    public TMP_Text metronomeSliderText;
    private float metronomeVolumeValue;

    public Slider sfxSlider;
    public TMP_Text sfxSliderText;
    private float sfxVolumeValue;
    void Start()
    {
        volumeSlider.value = GameManager.instance.volume;
        volumeValue = GameManager.instance.volume;
        volumeValueText.text = Mathf.RoundToInt(GameManager.instance.volume * 100).ToString() + "%";

        pianoSlider.value = GameManager.instance.pianoVolume;
        pianoVolumeValue = GameManager.instance.pianoVolume;
        pianoSliderText.text = Mathf.RoundToInt(GameManager.instance.pianoVolume * 100).ToString() + "%";

        metronomeSlider.value = GameManager.instance.metronomeVolume;
        metronomeVolumeValue = GameManager.instance.metronomeVolume;
        metronomeSliderText.text = Mathf.RoundToInt(GameManager.instance.metronomeVolume * 100).ToString() + "%";

        sfxSlider.value = GameManager.instance.sfxVolume;
        sfxVolumeValue = GameManager.instance.sfxVolume;
        sfxSliderText.text = Mathf.RoundToInt(GameManager.instance.sfxVolume * 100).ToString() + "%";


        isInitializing = false;
    }
    public void VolumeChanged()
    {
        if (isInitializing) return;
        GameManager.instance.volume = volumeSlider.value;
        volumeValueText.text = Mathf.RoundToInt(GameManager.instance.volume * 100).ToString() + "%";
    }

    public void PianoVolumeChanged()
    {
        if (isInitializing) return;
        GameManager.instance.pianoVolume = pianoSlider.value;
        pianoSliderText.text = Mathf.RoundToInt(GameManager.instance.pianoVolume * 100).ToString() + "%";
    }

    public void MetronomeVolumeChanged()
    {
        if (isInitializing) return;
        GameManager.instance.metronomeVolume = metronomeSlider.value;
        metronomeSliderText.text = Mathf.RoundToInt(GameManager.instance.metronomeVolume * 100).ToString() + "%";
    }

    public void SFXVolumeChanged()
    {
        if (isInitializing) return;
        GameManager.instance.sfxVolume = sfxSlider.value;
        sfxSliderText.text = Mathf.RoundToInt(GameManager.instance.sfxVolume * 100).ToString() + "%";
    }


    public void SaveSettings()
    {
        if(GameManager.instance.SelectedAccount == "")
        {
            Debug.LogWarning("No account selected!");
            return;
        }
        else if (GameManager.instance.volume != volumeValue || GameManager.instance.pianoVolume != pianoVolumeValue || GameManager.instance.metronomeVolume != metronomeVolumeValue || GameManager.instance.sfxVolume != sfxVolumeValue)
        {
            AccountUtility.UpdateAccountVolume(GameManager.instance.volume, GameManager.instance.pianoVolume, GameManager.instance.metronomeVolume, GameManager.instance.sfxVolume);
        }
    }

    public void CancelSettings()
    {
        GameManager.instance.volume = volumeValue;
        GameManager.instance.pianoVolume = pianoVolumeValue;
        GameManager.instance.metronomeVolume = metronomeVolumeValue;
        GameManager.instance.sfxVolume = sfxVolumeValue;
    }
}

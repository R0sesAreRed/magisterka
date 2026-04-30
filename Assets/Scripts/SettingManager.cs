using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{

    public Toggle[] settingToggles;
    private bool isInitializing = false;
    private string SettingsHolder;
    public void TogglePoints()
    {
        if (isInitializing) return;
        GameManager.instance.pointsOn = !GameManager.instance.pointsOn;
        settingToggles[7].isOn = GameManager.instance.pointsOn == false ? false : GameManager.instance.leaderBoardOn;
        settingToggles[7].interactable = GameManager.instance.pointsOn;
    }
    public void ToggleLeaderBoard()
    {
        if (isInitializing) return;
        GameManager.instance.leaderBoardOn = !GameManager.instance.leaderBoardOn;
    }
    public void ToggleProgressBar()
    {
        if (isInitializing) return;
        GameManager.instance.progressBarOn = !GameManager.instance.progressBarOn;
    }
    public void ToggleHitQuality()
    {
        if (isInitializing) return;
        GameManager.instance.hitQualityOn = !GameManager.instance.hitQualityOn;
    }

    public void ToggleAchievements()
    {
        if (isInitializing) return;
        GameManager.instance.achievementsOn = !GameManager.instance.achievementsOn;
    }
    public void ToggleRewardsAndCosmetic()
    {
        if (isInitializing) return;
        GameManager.instance.rewardsAndCosmeticOn = !GameManager.instance.rewardsAndCosmeticOn;
        settingToggles[6].isOn = GameManager.instance.rewardsAndCosmeticOn == false ? false : GameManager.instance.questsOn;
        settingToggles[4].isOn = GameManager.instance.rewardsAndCosmeticOn == false ? false : GameManager.instance.shopAndCurrencyOn;
        settingToggles[6].interactable = GameManager.instance.rewardsAndCosmeticOn;
        settingToggles[4].interactable = GameManager.instance.rewardsAndCosmeticOn;
    }
    public void ToggleShopAndCurrency()
    {
        if (isInitializing) return;
        GameManager.instance.shopAndCurrencyOn = !GameManager.instance.shopAndCurrencyOn;
    }
    public void ToggleQuests()
    {
        if (isInitializing) return;
        GameManager.instance.questsOn = !GameManager.instance.questsOn;
    }

    private string GetSettingsString()
    {
        GameManager gm = GameManager.instance;
        return (gm.pointsOn ? "1" : "0") +
            (gm.progressBarOn ? "1" : "0") +
            (gm.hitQualityOn ? "1" : "0") +
            (gm.achievementsOn ? "1" : "0") +
            (gm.shopAndCurrencyOn ? "1" : "0") +
            (gm.rewardsAndCosmeticOn ? "1" : "0") +
            (gm.questsOn ? "1" : "0") +
            (gm.leaderBoardOn ? "1" : "0");
    }
    public void SaveSettings()
    {
        PlayerPrefs.SetString(GameManager.instance.GetPersistSettingsName(), GetSettingsString());
        AccountUtility.UpdateAccountVolume(GameManager.instance.volume);
    }

    public void CancelSettings()
    {
        GameManager.instance.ParseSettingsString(SettingsHolder);
    }
    public Slider volumeSlider;
    public TMP_Text volumeValueText;
    void Start()
    {
        SettingsHolder = GetSettingsString();
        isInitializing = true;
        settingToggles[0].isOn = GameManager.instance.pointsOn;
        settingToggles[1].isOn = GameManager.instance.progressBarOn;
        settingToggles[2].isOn = GameManager.instance.hitQualityOn;
        settingToggles[3].isOn = GameManager.instance.achievementsOn;
        settingToggles[4].isOn = GameManager.instance.shopAndCurrencyOn;
        settingToggles[5].isOn = GameManager.instance.rewardsAndCosmeticOn;
        settingToggles[6].isOn = GameManager.instance.questsOn;
        settingToggles[7].isOn = GameManager.instance.leaderBoardOn;
        if(!settingToggles[0].isOn)
        {
            settingToggles[7].interactable = false;
        }
        if(!settingToggles[5].isOn)
        {
            settingToggles[4].interactable = false;
            settingToggles[6].interactable = false;

        }
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

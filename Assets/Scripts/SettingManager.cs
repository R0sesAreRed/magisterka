using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    private const int ToggleCount = 9;
    private const int PointsIndex = 0;
    private const int ShopIndex = 4;
    private const int RewardsIndex = 5;
    private const int QuestsIndex = 6;
    private const int LeaderboardIndex = 7;
    private const int OffPerRandomization = 3;
    private const int MaxOnToggles = ToggleCount - OffPerRandomization;

    public Toggle[] settingToggles;
    private bool isInitializing = false;
    private string SettingsHolder;
    public void TogglePoints()
    {
        if (isInitializing) return;
        GameManager.instance.pointsOn = !GameManager.instance.pointsOn;
        settingToggles[7].isOn = GameManager.instance.pointsOn == false ? false : GameManager.instance.leaderBoardOn;
        settingToggles[7].interactable = GameManager.instance.pointsOn;
        ApplyToggleInteractabilityConstraints();
    }
    public void ToggleLeaderBoard()
    {
        if (isInitializing) return;
        GameManager.instance.leaderBoardOn = !GameManager.instance.leaderBoardOn;
        ApplyToggleInteractabilityConstraints();
    }
    public void ToggleProgressBar()
    {
        if (isInitializing) return;
        GameManager.instance.progressBarOn = !GameManager.instance.progressBarOn;
        ApplyToggleInteractabilityConstraints();
    }
    public void ToggleHitQuality()
    {
        if (isInitializing) return;
        GameManager.instance.hitQualityOn = !GameManager.instance.hitQualityOn;
        ApplyToggleInteractabilityConstraints();
    }

    public void ToggleAchievements()
    {
        if (isInitializing) return;
        GameManager.instance.achievementsOn = !GameManager.instance.achievementsOn;
        ApplyToggleInteractabilityConstraints();
    }
    public void ToggleRewardsAndCosmetic()
    {
        if (isInitializing) return;
        GameManager.instance.rewardsAndCosmeticOn = !GameManager.instance.rewardsAndCosmeticOn;
        settingToggles[6].isOn = GameManager.instance.rewardsAndCosmeticOn == false ? false : GameManager.instance.questsOn;
        settingToggles[4].isOn = GameManager.instance.rewardsAndCosmeticOn == false ? false : GameManager.instance.shopAndCurrencyOn;
        settingToggles[6].interactable = GameManager.instance.rewardsAndCosmeticOn;
        settingToggles[4].interactable = GameManager.instance.rewardsAndCosmeticOn;
        ApplyToggleInteractabilityConstraints();
    }
    public void ToggleShopAndCurrency()
    {
        if (isInitializing) return;
        GameManager.instance.shopAndCurrencyOn = !GameManager.instance.shopAndCurrencyOn;
        ApplyToggleInteractabilityConstraints();
    }
    public void ToggleQuests()
    {
        if (isInitializing) return;
        GameManager.instance.questsOn = !GameManager.instance.questsOn;
        ApplyToggleInteractabilityConstraints();
    }
    public void ToggleLevels()
    {
        if (isInitializing) return;
        GameManager.instance.levelsOn = !GameManager.instance.levelsOn;
        ApplyToggleInteractabilityConstraints();
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
            (gm.leaderBoardOn ? "1" : "0") +
            (gm.levelsOn ? "1" : "0");
    }
    public void SaveSettings()
    {
        PlayerPrefs.SetString(GameManager.instance.GetPersistSettingsName(), GetSettingsString());
        PlayerPrefs.Save();
        AccountUtility.UpdateAccountVolume(GameManager.instance.volume);
    }

    [ContextMenu("Randomize 3 Off (Balanced)")]
    public void RandomizeThreeOffBalancedContextMenu()
    {
        string randomized = RandomizeThreeOffBalanced(true);
        Debug.Log($"[SettingManager] Balanced random settings generated: {randomized}");
    }

    // Returns a valid 9-bit settings string with exactly 3 features off.
    public string RandomizeThreeOffBalanced(bool saveAfterApply)
    {
        List<int[]> candidates = BuildValidThreeOffCandidates();

        if (candidates.Count == 0)
        {
            Debug.LogError("[SettingManager] No valid candidates found for dependency-safe 3-off randomization.");
            return GetSettingsString();
        }

        int[] selectedOff = SelectWeightedCandidate(candidates);

        bool[] onStates = new bool[ToggleCount];
        for (int i = 0; i < ToggleCount; i++)
        {
            onStates[i] = true;
        }
        for (int i = 0; i < selectedOff.Length; i++)
        {
            onStates[selectedOff[i]] = false;
        }

        ApplyStatesToGameManagerAndUI(onStates);

        string settingsString = GetSettingsString();
        SettingsHolder = settingsString;

        if (saveAfterApply)
        {
            SaveSettings();
        }

        return settingsString;
    }

    private int[] SelectWeightedCandidate(List<int[]> candidates)
    {
        float[] toggleOffFrequency = new float[ToggleCount];

        for (int i = 0; i < candidates.Count; i++)
        {
            int[] candidate = candidates[i];
            for (int j = 0; j < candidate.Length; j++)
            {
                toggleOffFrequency[candidate[j]] += 1f;
            }
        }

        float[] weights = new float[candidates.Count];
        float totalWeight = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            int[] candidate = candidates[i];
            float weight = 0f;

            for (int j = 0; j < candidate.Length; j++)
            {
                int toggleIndex = candidate[j];
                float frequency = toggleOffFrequency[toggleIndex];
                if (frequency > 0f)
                {
                    weight += 1f / frequency;
                }
            }

            // Keep all valid candidates selectable.
            weight = Mathf.Max(weight, 0.0001f);
            weights[i] = weight;
            totalWeight += weight;
        }

        if (totalWeight <= 0f)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }

        float pick = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            cumulative += weights[i];
            if (pick <= cumulative)
            {
                return candidates[i];
            }
        }

        return candidates[candidates.Count - 1];
    }

    private List<int[]> BuildValidThreeOffCandidates()
    {
        List<int[]> result = new List<int[]>();

        for (int a = 0; a < ToggleCount - 2; a++)
        {
            for (int b = a + 1; b < ToggleCount - 1; b++)
            {
                for (int c = b + 1; c < ToggleCount; c++)
                {
                    int[] candidate = new int[] { a, b, c };
                    if (IsValidOffCandidate(candidate))
                    {
                        result.Add(candidate);
                    }
                }
            }
        }

        return result;
    }

    private bool IsValidOffCandidate(int[] off)
    {
        bool[] isOff = new bool[ToggleCount];
        for (int i = 0; i < off.Length; i++)
        {
            isOff[off[i]] = true;
        }

        // pointsOff => leaderboardOff
        if (isOff[PointsIndex] && !isOff[LeaderboardIndex])
        {
            return false;
        }

        // rewardsOff => shopOff and questsOff
        if (isOff[RewardsIndex] && (!isOff[ShopIndex] || !isOff[QuestsIndex]))
        {
            return false;
        }

        return true;
    }

    private void ApplyStatesToGameManagerAndUI(bool[] onStates)
    {
        GameManager gm = GameManager.instance;

        gm.pointsOn = onStates[PointsIndex];
        gm.progressBarOn = onStates[1];
        gm.hitQualityOn = onStates[2];
        gm.achievementsOn = onStates[3];
        gm.shopAndCurrencyOn = onStates[ShopIndex];
        gm.rewardsAndCosmeticOn = onStates[RewardsIndex];
        gm.questsOn = onStates[QuestsIndex];
        gm.leaderBoardOn = onStates[LeaderboardIndex];
        gm.levelsOn = onStates[8];

        if (!gm.pointsOn)
        {
            gm.leaderBoardOn = false;
        }
        if (!gm.rewardsAndCosmeticOn)
        {
            gm.shopAndCurrencyOn = false;
            gm.questsOn = false;
        }

        isInitializing = true;
        settingToggles[PointsIndex].isOn = gm.pointsOn;
        settingToggles[1].isOn = gm.progressBarOn;
        settingToggles[2].isOn = gm.hitQualityOn;
        settingToggles[3].isOn = gm.achievementsOn;
        settingToggles[ShopIndex].isOn = gm.shopAndCurrencyOn;
        settingToggles[RewardsIndex].isOn = gm.rewardsAndCosmeticOn;
        settingToggles[QuestsIndex].isOn = gm.questsOn;
        settingToggles[LeaderboardIndex].isOn = gm.leaderBoardOn;
        settingToggles[8].isOn = gm.levelsOn;

        ApplyToggleInteractabilityConstraints();
        isInitializing = false;
    }

    private void ApplyToggleInteractabilityConstraints()
    {
        GameManager gm = GameManager.instance;

        if (gm.tutorialCompleted)
        {
            for (int i = 0; i < settingToggles.Length; i++)
            {
                settingToggles[i].interactable = false;
            }
            return;
        }

        for (int i = 0; i < settingToggles.Length; i++)
        {
            settingToggles[i].interactable = true;
        }

        settingToggles[LeaderboardIndex].interactable = gm.pointsOn;
        settingToggles[ShopIndex].interactable = gm.rewardsAndCosmeticOn;
        settingToggles[QuestsIndex].interactable = gm.rewardsAndCosmeticOn;

        int onCount = 0;
        for (int i = 0; i < settingToggles.Length; i++)
        {
            if (settingToggles[i].isOn)
            {
                onCount++;
            }
        }

        // Keep at least 3 toggles OFF: when 6 are ON, block turning any OFF toggle back ON.
        if (onCount >= MaxOnToggles)
        {
            for (int i = 0; i < settingToggles.Length; i++)
            {
                if (!settingToggles[i].isOn)
                {
                    settingToggles[i].interactable = false;
                }
            }
        }
    }


    public void CancelSettings()
    {
        GameManager.instance.ParseSettingsString(SettingsHolder);
    }
    public Slider volumeSlider;
    public TMP_Text volumeValueText;
    void Start()
    {
        if (!GameManager.instance.tutorialCompleted)
        {
            // Override default account settings (e.g. all-on string) with randomized setup.
            RandomizeThreeOffBalanced(true);
        }

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
        settingToggles[8].isOn = GameManager.instance.levelsOn;
        ApplyToggleInteractabilityConstraints();
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

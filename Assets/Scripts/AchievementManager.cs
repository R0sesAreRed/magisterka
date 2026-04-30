using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    private void Awake()
    {
        //UnityEngine.Object.DontDestroyOnLoad(gameObject);
    }

    private string GetPath()
    {
        if (GameManager.instance.SelectedAccount != null)
            return Path.Combine(Application.persistentDataPath, $"{GameManager.instance.SelectedAccount}_achievements.json");
        else
            return Path.Combine(Application.persistentDataPath, "default_songs.json");
    }

    public void Save(List<AchievementData> items) //to trzeba dodaæ w miejscach ktore tego potrzebuja
    {
        AcheivementsDatabase db = new AcheivementsDatabase();

        foreach (var item in items)
        {
            db.songs.Add(new AchievementData(item));
        }

        string json = JsonUtility.ToJson(db, true);
        File.WriteAllText(GetPath(), json);

        Debug.Log("Saved achievements to: " + GetPath());
    }

    public List<AchievementData> Load()
    {
        string path = GetPath();

        if (!File.Exists(path))
            return GameManager.instance.playerAchievements;

        string json = File.ReadAllText(path);

        AcheivementsDatabase db = JsonUtility.FromJson<AcheivementsDatabase>(json);

        List<AchievementData> result = new List<AchievementData>();

        foreach (var data in db.songs)
        {
            result.Add(data.ToScriptableObject());
        }
        Debug.Log("loeadedAchievements");
        return result;
    }

    void OnEnable()
    {
        GameEvents.OnCompleteLevel += OnCompleteLevel;
        GameEvents.OnPerfectHit += OnPerfectHit;
        GameEvents.OnHit += OnHit;
        GameEvents.OnRegainedHp += OnRegainedHp;
        GameEvents.OnCompleteLevelQuality += OnCompleteLevelQuality;
        GameEvents.OnCompleteLevelHealthQuality += OnCompleteLevelHealthQuality;
        GameEvents.OnPurchaseItem += OnPurchaseItem;
        GameEvents.OnCompleteQuest += OnCompleteQuest;
        GameEvents.SetAchievementValue += SetAchievementValue;
        GameEvents.LoadAchievements += () => GameManager.instance.playerAchievements = Load();
    }

    void OnDisable()
    {
        GameEvents.OnCompleteLevel -= OnCompleteLevel;
        GameEvents.OnPerfectHit -= OnPerfectHit;
        GameEvents.OnHit -= OnHit;
        GameEvents.OnRegainedHp -= OnRegainedHp;
        GameEvents.OnCompleteLevelQuality -= OnCompleteLevelQuality;
        GameEvents.OnCompleteLevelHealthQuality -= OnCompleteLevelHealthQuality;
        GameEvents.OnPurchaseItem -= OnPurchaseItem;
        GameEvents.OnCompleteQuest -= OnCompleteQuest;
        GameEvents.SetAchievementValue -= SetAchievementValue;
        GameEvents.LoadAchievements -= () => GameManager.instance.playerAchievements = Load();
    }

    private void OnCompleteLevel(int count)
    {
        GrantAchievementProgress(AchievementRestriction.Levels, 1);
    }

    private void OnPerfectHit(int count)
    {
        GrantAchievementProgress(AchievementRestriction.HitQuality, count);
    }

    private void OnHit(int count)
    {
        GrantAchievementProgress(AchievementRestriction.HitAccuracy, count);
    }
    public void OnRegainedHp(int count)
    {
        GrantAchievementProgress(AchievementRestriction.RegainedHp, count);
    }

    public void OnCompleteLevelQuality(int quality)
    {
        GrantAchievementProgress(AchievementRestriction.LevelQuality, quality);
    }

    public void OnCompleteLevelHealthQuality(int HealthQuality)
    {
        GrantAchievementProgress(AchievementRestriction.LevelHealthQuality, HealthQuality);
    }

    public void OnPurchaseItem(int count)
    {
        GrantAchievementProgress(AchievementRestriction.Shop, count);
    }

    public void OnCompleteQuest(int count)
    {
        GrantAchievementProgress(AchievementRestriction.Quests, count);
    }

    private void GrantAchievementProgress(AchievementRestriction type, int value)
    {
        foreach (var ach in GameManager.instance.playerAchievements)
        {
            if (ach.rest == type && !ach.completed)
            {
                ach.currentValue += value;
                if (ach.currentValue >= ach.targetValue)
                {
                    ach.completed = true;
                    OnAchievementCompleted(ach);
                }
            }
        }
    }

    private void SetAchievementValue(AchievementRestriction type, int value)
    {
        foreach (var ach in GameManager.instance.playerAchievements)
        {
            if (ach.rest == type && !ach.completed)
            {
                ach.currentValue = value;
                if (ach.currentValue >= ach.targetValue)
                {
                    ach.completed = true;
                    OnAchievementCompleted(ach);
                }
            }
        }
    }

    private void OnAchievementCompleted(AchievementData ach)
    {
        ach.completed = true;
        Debug.Log($"Achievement unlocked: {ach.title}");
        Save(GameManager.instance.playerAchievements);
    }

}

public static class GameEvents
{
    public static Action<int> OnCompleteLevel;
    public static Action<int> OnPerfectHit;
    public static Action<int> OnHit;
    public static Action<int> OnRegainedHp;
    public static Action<int> OnCompleteLevelQuality;
    public static Action<int> OnCompleteLevelHealthQuality;
    public static Action<int> OnPurchaseItem;
    public static Action<int> OnCompleteQuest;
    public static Action<AchievementRestriction, int> SetAchievementValue;
    public static Action LoadAchievements;
}



// przejœcie poziomów 1, 3, 5, 10, 20, wszystkie -> levels

// + perfect hit                                  -> hit quality
// ci¹g trafieñ 10, 50, 100, 200                -> hit accuracy
// + ci¹g perfect hitów 5, 10, 25, 50             -> hit quality
// odzyskane hp                                 -> regained hp
// complete level:                              
// perfect hity 100%, 75%, 50%                  -> level quality
// bez straty hp                                -> level health quality

//kupowanie itemów:                             -> shop
//kupienie pierwaszego itemu
//kupienie ca³ego zestawu itemów
//wykupienie sklepu

//questy
//1, 3, 5, 10, 30                               -> quests

public class AcheivementsDatabase
{
    public List<AchievementData> songs = new List<AchievementData>();
}





using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public List<QuestData> allQuests = new List<QuestData>();
    void Start()
    {
        
    }
    void Update()
    {

    }

    private string GetPath()
    {
        if (GameManager.instance.SelectedAccount != null)
            return Path.Combine(Application.persistentDataPath, $"{GameManager.instance.SelectedAccount}_quests.json");
        else
            return Path.Combine(Application.persistentDataPath, "default_quests.json");
    }


    public void Save(List<QuestData> items) //to trzeba dodaæ w miejscach ktore tego potrzebuja
    {
        QuestsDatabase db = new QuestsDatabase();

        foreach (var item in items)
        {
            db.quest.Add(new QuestDataClass(item));
        }

        string json = JsonUtility.ToJson(db, true);
        File.WriteAllText(GetPath(), json);

        //Debug.Log("Saved achievements to: " + GetPath());
    }

    public List<QuestData> Load()
    {
        string path = GetPath();

        if (!File.Exists(path))
        {
            DrawNewQuests();
            return GameManager.instance.playerCurrentQuests;
        }
            

        string json = File.ReadAllText(path);

        QuestsDatabase db = JsonUtility.FromJson<QuestsDatabase>(json);

        List<QuestData> result = new List<QuestData>();

        foreach (var data in db.quest)
        {
            result.Add(data.ToScriptableObject());
        }
        Debug.Log("loeadedQuests");
        return result;
    }

    public void DrawNewQuests()
    {
        while (GameManager.instance.playerCurrentQuests.Count < 3)
        {
            QuestData randomQuest = allQuests[UnityEngine.Random.Range(0, allQuests.Count)];
            if (!GameManager.instance.playerCurrentQuests.Exists(q => q.id == randomQuest.id))
            {
                // Stwórz kopiê ScriptableObject
                QuestData questCopy = ScriptableObject.CreateInstance<QuestData>();
                questCopy.id = randomQuest.id;
                questCopy.description = randomQuest.description;
                questCopy.targetValue = randomQuest.targetValue;
                questCopy.rest = randomQuest.rest;
                questCopy.currentValue = 0;
                questCopy.OneLevel = randomQuest.OneLevel;
                questCopy.rewardCurrency = randomQuest.rewardCurrency;
                var availableCosmetics = GameManager.instance.allCosmetics.FindAll(c => !GameManager.instance.playerCosmetics.Contains(c));
                if (availableCosmetics.Count > 0)
                {
                    questCopy.altReward = availableCosmetics[UnityEngine.Random.Range(0, availableCosmetics.Count)];
                }
                else
                {
                    questCopy.altReward = null;
                }
                GameManager.instance.playerCurrentQuests.Add(questCopy);
            }
        }
        Save(GameManager.instance.playerCurrentQuests);
    }

    public void progressQuest(QuestRestriction rest, int value)
    {
        if(GameManager.instance.questsOn)
        {
            foreach(var quest in GameManager.instance.playerCurrentQuests)
            {
                if (quest.rest == rest)
                {
                    quest.currentValue += value;
                    if (quest.currentValue >= quest.targetValue)
                    {
                        
                        GameManager.instance.playerCurrentQuests.Remove(quest);
                        onQuestComplete(quest);
                        break;
                    }
                }
            }
        }
    }

    public void SingleLevelQuestComplete(QuestRestriction rest, int value)
    {
        if (GameManager.instance.questsOn)
        {
            foreach (var quest in GameManager.instance.playerCurrentQuests)
            {
                if (quest.rest == rest)
                {
                    if (value >= quest.targetValue)
                    {
                        
                        GameManager.instance.playerCurrentQuests.Remove(quest);
                        onQuestComplete(quest);
                        break;
                    }
                }
            }
        }
    }

    public void onQuestComplete(QuestData compquest)
    {
        if(GameManager.instance.shopAndCurrencyOn)
        {
            GameManager.instance.currency += compquest.rewardCurrency;
            AccountUtility.UpdateAccountCurrency(GameManager.instance.currency);
        }
        else
        {
            GameManager.instance.playerCosmetics.Add(compquest.altReward);
        }
        GameManager.instance.completedQuests++;
        DrawNewQuests();
        Save(GameManager.instance.playerCurrentQuests);

    }

    void OnEnable()
    {
        QuestEvents.LoadQuests += () => GameManager.instance.playerCurrentQuests = Load();
        QuestEvents.SaveQuests += (quests) => Save(quests);
        QuestEvents.DrawQuests += () => DrawNewQuests();
        QuestEvents.ProgressQuest += (rest, value) => progressQuest(rest, value);
        QuestEvents.SingleLevelProgress += (rest, value) => SingleLevelQuestComplete(rest, value);
    }

    private void OnDisable()
    {
        QuestEvents.LoadQuests -= () => GameManager.instance.playerCurrentQuests = Load();
        QuestEvents.SaveQuests -= (quests) => Save(quests);
        QuestEvents.DrawQuests += () => DrawNewQuests();
        QuestEvents.ProgressQuest -= (rest, value) => progressQuest(rest, value);
        QuestEvents.SingleLevelProgress -= (rest, value) => SingleLevelQuestComplete(rest, value);
    }

}

public static class QuestEvents
{
    public static Action LoadQuests;
    public static Action<List<QuestData>> SaveQuests;
    public static Action DrawQuests;
    public static Action<QuestRestriction, int> ProgressQuest;
    public static Action<QuestRestriction, int> SingleLevelProgress;
}


//Questy:
// rozegraj 2 melodii                                               //melodie +
// rozegraj 5 melodii                                               //melodie +
// rozegraj 10 melodii                                              //melodie +
// przejdŸ melodiê z 100% trafieñ                                   //100hits +
// w trakcie jednej melodii traf 5 perfekcyjnych trafieñ            //perfecthits +
// w trakcie jednej melodii traf 10 perfekcyjnych trafieñ           //pefrecthits +
// w trakcie jednej melodii traf 15 perfekcyjnych trafieñ           //perfecthits +
// w trakcie jednej melodii osi¹gnij 60% maksymalnego wyniku        //score +
// zdob¹dŸ osi¹gniêcie                                              //achievement
// ukoñcz melodiê                                                   //melodiefinish +
// ukoñcz 3 melodie                                                 //melodiefinish +
// ukoñcz 5 melodii                                                 //melodiefinish +


public class QuestsDatabase
{
    public List<QuestDataClass> quest = new List<QuestDataClass>();
}
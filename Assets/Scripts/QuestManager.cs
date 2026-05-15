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
            return GameManager.instance.playerCurrentQuests;

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
                questCopy.completed = false;
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
    }

    public void progressQuest()
    {

    }

    public void onQuestComplete(QuestData compquest)
    {
        if(GameManager.instance.shopAndCurrencyOn)
        {
            GameManager.instance.currency += compquest.rewardCurrency;
        }
        else
        {
            GameManager.instance.playerCosmetics.Add(compquest.altReward);
        }
        DrawNewQuests();
    }

    void OnEnable()
    {
        QuestEvents.LoadQuests += () => GameManager.instance.playerCurrentQuests = Load();
    }

    private void OnDisable()
    {
        QuestEvents.LoadQuests -= () => GameManager.instance.playerCurrentQuests = Load();
    }

}

public static class QuestEvents
{
    public static Action LoadQuests;
}


//Questy:
// rozegraj 2 melodii
// rozegraj 5 melodii
// rozegraj 10 melodii
// przejdŸ melodiê z 100% trafieñ
// w trakcie jednej melodii traf 5 perfekcyjnych trafieñ
// w trakcie jednej melodii osi¹gnij 60% maksymalnego wyniku
// zdob¹dŸ osi¹gniêcie
// w trakcie jednej melodii traf 3 perfekcyjne trafienia pod rz¹d
// ukoñcz melodiê
// ukoñcz 3 melodie
// ukoñcz 5 melodii


public class QuestsDatabase
{
    public List<QuestDataClass> quest = new List<QuestDataClass>();
}
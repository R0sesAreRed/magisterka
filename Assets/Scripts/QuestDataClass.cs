using System;
using UnityEngine;

[System.Serializable]
public class QuestDataClass
{
    public int id;
    public string description;
    public int targetValue;
    public QuestRestriction rest;
    public int currentValue;
    public bool completed;
    public int rewardCurrency;
    public CosmeticsData altReward;


    public QuestDataClass(QuestData item)
    {
        id = item.id;
        description = item.description;
        targetValue = item.targetValue;
        rest = item.rest;
        currentValue = item.currentValue;
        completed = item.completed;
        rewardCurrency = item.rewardCurrency;
        altReward = item.altReward;
    }

    public QuestData ToScriptableObject()
    {
        QuestData copy = ScriptableObject.CreateInstance<QuestData>();
        copy.id = id;
        copy.description = description;
        copy.targetValue = targetValue;
        copy.rest = rest;
        copy.currentValue = currentValue;
        copy.completed = completed;
        copy.rewardCurrency = rewardCurrency;
        copy.altReward = altReward;
        return copy;
    }
}

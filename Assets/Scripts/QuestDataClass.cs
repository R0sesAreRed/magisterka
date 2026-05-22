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
    public int rewardCurrency;
    public bool OneLevel;
    public CosmeticsData altReward;


    public QuestDataClass(QuestData item)
    {
        id = item.id;
        description = item.description;
        targetValue = item.targetValue;
        rest = item.rest;
        currentValue = item.currentValue;
        rewardCurrency = item.rewardCurrency;
        OneLevel = item.OneLevel;
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
        copy.rewardCurrency = rewardCurrency;
        copy.OneLevel = OneLevel;
        copy.altReward = altReward;
        return copy;
    }
}

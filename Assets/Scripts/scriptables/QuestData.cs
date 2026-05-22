using System;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject
{
    public int id;
    public string description;
    public int targetValue;
    public QuestRestriction rest;
    [NonSerialized] public int currentValue;
    public bool OneLevel;
    public int rewardCurrency;
    public CosmeticsData altReward;
}

public enum QuestRestriction //zmienic tutaj
{
    Melodie,
    NoMisses,
    PerfectHits,
    Score,
    Achievement,
    MelodieFinish
}

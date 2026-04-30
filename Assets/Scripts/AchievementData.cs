using System;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "AchievementData", menuName = "Scriptable Objects/AchievementData")]
public class AchievementData : ScriptableObject
{
    public int id;
    public string title;
    public string description;
    public int targetValue;
    public AchievementRestriction rest;

    [NonSerialized] public int currentValue; // aktualny postêp (nie serializowany do assetu)
    [NonSerialized] public bool completed = false;   // czy osi¹gniêcie zdobyte

    public AchievementData(AchievementData item)
    {
        id = item.id;
        title = item.title;
        description = item.description;
        targetValue = item.targetValue;
        rest = item.rest;
        currentValue = 0;
        completed = false;
    }

    public AchievementData ToScriptableObject()
    {
        AchievementData copy = CreateInstance<AchievementData>();
        copy.id = id;
        copy.title = title;
        copy.description = description;
        copy.targetValue = targetValue;
        copy.rest = rest;
        copy.currentValue = currentValue;
        copy.completed = completed;
        return copy;
    }
}


public enum AchievementRestriction
{
    Levels,
    HitQuality,
    HitAccuracy,
    RegainedHp,
    LevelQuality,
    LevelHealthQuality,
    Shop,
    Quests
}

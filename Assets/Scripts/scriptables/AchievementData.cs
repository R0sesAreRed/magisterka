using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AchievementData", menuName = "Scriptable Objects/AchievementData")]
[System.Serializable]
public class AchievementData : ScriptableObject
{
    public int id;
    public string title;
    public string description;
    public int targetValue;
    public AchievementRestriction rest;
    [NonSerialized] public int currentValue; 
    [NonSerialized] public bool completed = false;
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

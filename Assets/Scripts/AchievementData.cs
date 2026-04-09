using UnityEngine;

[CreateAssetMenu(fileName = "AchievementData", menuName = "Scriptable Objects/AchievementData")]
public class AchievementData : ScriptableObject
{
    public int id;
    public string title;
    public string description;
    public int targetValue;
    public AchievementRestriction rest;
}

public enum AchievementRestriction
{
    Points,
    HitQuality,
    Shop,
    Rewards,
    Quests,
    Scorboard
}

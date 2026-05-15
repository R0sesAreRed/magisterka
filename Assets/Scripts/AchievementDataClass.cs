using UnityEngine;

[System.Serializable]
public class AchievementDataClass
{
    public int id;
    public string title;
    public string description;
    public int targetValue;
    public AchievementRestriction rest;
    public int currentValue;
    public bool completed;

    public AchievementDataClass(AchievementData item)
    {
        id = item.id;
        title = item.title;
        description = item.description;
        targetValue = item.targetValue;
        rest = item.rest;
        currentValue = item.currentValue;
        completed = item.completed;
    }

    public AchievementData ToScriptableObject()
    {
        AchievementData copy = ScriptableObject.CreateInstance<AchievementData>();
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

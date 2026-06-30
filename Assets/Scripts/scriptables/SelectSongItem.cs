using UnityEngine;

[CreateAssetMenu(fileName = "SelectSongItem", menuName = "Scriptable Objects/SelectSongItem")]
[System.Serializable]
public class SelectSongItem : ScriptableObject
{
    public string Title;
    public string FilePath;
    public float BestScore;
    public int Level;
    public bool Completed;
    public bool added = false;
    public bool GamificationOn = false;
    public float LevelTimePlayed = 0;
}


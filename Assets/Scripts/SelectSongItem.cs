using UnityEngine;

[CreateAssetMenu(fileName = "SelectSongItem", menuName = "Scriptable Objects/SelectSongItem")]
[System.Serializable]
public class SelectSongItem : ScriptableObject
{
    public string Title;
    public string FilePath;
    public int HighScore;
    public int MaxScore;
    public bool added = false;
}


using UnityEngine;

[System.Serializable]
public class SelectSongItemData
{
    public string Title;
    public string FilePath;
    public int HighScore;
    public int MaxScore;
    public bool added;

    public SelectSongItemData(SelectSongItem item)
    {
        Title = item.Title;
        FilePath = item.FilePath;
        HighScore = item.HighScore;
        MaxScore = item.MaxScore;
        added = item.added;
    }

    public SelectSongItem ToScriptableObject()
    {
        var item = ScriptableObject.CreateInstance<SelectSongItem>();
        item.Title = Title;
        item.FilePath = FilePath;
        item.HighScore = HighScore;
        item.MaxScore = MaxScore;
        item.added = added;
        return item;
    }
}

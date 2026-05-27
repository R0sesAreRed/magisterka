using UnityEngine;

[System.Serializable]
public class SelectSongItemData
{
    public string Title;
    public string FilePath;
    public double BestScore;
    public int Level;
    public bool Completed;
    public bool added;

    public SelectSongItemData(SelectSongItem item)
    {
        Title = item.Title;
        FilePath = item.FilePath;
        BestScore = item.BestScore;
        Level = item.Level;
        Completed = item.Completed;
        added = item.added;
    }

    public SelectSongItem ToScriptableObject()
    {
        var item = ScriptableObject.CreateInstance<SelectSongItem>();
        item.Title = Title;
        item.FilePath = FilePath;
        item.BestScore = BestScore;
        item.Level = Level;
        item.Completed = Completed;
        item.added = added;
        return item;
    }
}

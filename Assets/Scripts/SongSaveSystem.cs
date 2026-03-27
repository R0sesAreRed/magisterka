using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class SongSaveSystem
{
    private string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, "songs.json");
    }

    public void Save(List<SelectSongItem> items)
    {
        SongDatabase db = new SongDatabase();

        foreach (var item in items)
        {
            db.songs.Add(new SelectSongItemData(item));
        }

        string json = JsonUtility.ToJson(db, true);
        File.WriteAllText(GetPath(), json);

        Debug.Log("Saved songs to: " + GetPath());
    }

    public List<SelectSongItem> Load()
    {
        string path = GetPath();

        if (!File.Exists(path))
            return new List<SelectSongItem>();

        string json = File.ReadAllText(path);

        SongDatabase db = JsonUtility.FromJson<SongDatabase>(json);

        List<SelectSongItem> result = new List<SelectSongItem>();

        foreach (var data in db.songs)
        {
            result.Add(data.ToScriptableObject());
        }

        return result;
    }
}

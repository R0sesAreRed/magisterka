using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CosmeticsManager : MonoBehaviour
{
    public List<CosmeticsData> AllCosmetics = new List<CosmeticsData>();
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private string GetPath()
    {
        if (GameManager.instance.SelectedAccount != null)
            return Path.Combine(Application.persistentDataPath, $"{GameManager.instance.SelectedAccount}_Cosmetics.json");
        else
            return Path.Combine(Application.persistentDataPath, "default_Cosmetics.json");
    }
    private string GetPathEquipped()
    {
        if (GameManager.instance.SelectedAccount != null)
            return Path.Combine(Application.persistentDataPath, $"{GameManager.instance.SelectedAccount}_Cosmetics.json");
        else
            return Path.Combine(Application.persistentDataPath, "default_Cosmetics.json");
    }

    public void Save(List<CosmeticsData> items)
    {
        CosmeticsDatabase db = new CosmeticsDatabase();

        foreach (var item in items)
        {
            db.cosm.Add(new CosmeticsDataClass(item));
        }

        string json = JsonUtility.ToJson(db, true);
        File.WriteAllText(GetPath(), json);
    }

    public List<CosmeticsData> Load()
    {
        string path = GetPath();

        if (!File.Exists(path))
            return GameManager.instance.playerCosmetics;

        string json = File.ReadAllText(path);

        CosmeticsDatabase db = JsonUtility.FromJson<CosmeticsDatabase>(json);

        List<CosmeticsData> result = new List<CosmeticsData>();

        foreach (var data in db.cosm)
        {
            result.Add(data.ToScriptableObject());
        }
        Debug.Log("loeadedcosmetics");
        return result;
    }

    public void SaveEquipped(List<CosmeticsData> items)
    {
        CosmeticsDatabase db = new CosmeticsDatabase();

        foreach (var item in items)
        {
            db.cosm.Add(new CosmeticsDataClass(item));
        }

        string json = JsonUtility.ToJson(db, true);
        File.WriteAllText(GetPathEquipped(), json);
    }

    public List<CosmeticsData> LoadEquipped()
    {
        string path = GetPathEquipped();

        if (!File.Exists(path))
            return GameManager.instance.playerCosmetics;

        string json = File.ReadAllText(path);

        CosmeticsDatabase db = JsonUtility.FromJson<CosmeticsDatabase>(json);

        List<CosmeticsData> result = new List<CosmeticsData>();

        foreach (var data in db.cosm)
        {
            result.Add(data.ToScriptableObject());
        }
        Debug.Log("loeadedcosmetics");
        return result;
    }

    void OnEnable()
    {
        CosmeticsEvents.SaveCosmetics += () => Save(GameManager.instance.playerCosmetics);
        CosmeticsEvents.SaveEquipped += () => SaveEquipped(GameManager.instance.playerEquippedCosmetics);
        CosmeticsEvents.LoadCosmetics += () => GameManager.instance.playerCosmetics = Load();
        CosmeticsEvents.LoadEquipped += () => GameManager.instance.playerEquippedCosmetics = LoadEquipped();
    }

    private void OnDisable()
    {
        CosmeticsEvents.SaveCosmetics -= () => Save(GameManager.instance.playerCosmetics);
        CosmeticsEvents.SaveEquipped -= () => SaveEquipped(GameManager.instance.playerEquippedCosmetics);
        CosmeticsEvents.LoadCosmetics -= () => GameManager.instance.playerCosmetics = Load();
        CosmeticsEvents.LoadEquipped -= () => GameManager.instance.playerEquippedCosmetics = LoadEquipped();
    }


}

public static class CosmeticsEvents
{
    public static Action SaveCosmetics;
    public static Action SaveEquipped;
    public static Action LoadCosmetics;
    public static Action LoadEquipped;
}

public class CosmeticsDatabase
{
    public List<CosmeticsDataClass> cosm = new List<CosmeticsDataClass>();
}
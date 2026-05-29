using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CosmeticsManager : MonoBehaviour
{
    public List<CosmeticsData> AllCosmetics = new List<CosmeticsData>();
    
    public List<CosmeticsData> defaultEquippedCosmetics = new List<CosmeticsData>();
    public List<CosmeticsData> defaultPlayerCosmetics = new List<CosmeticsData>();
 
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
            return Path.Combine(Application.persistentDataPath, $"{GameManager.instance.SelectedAccount}_PlayerCosmetics.json");
        else
            return Path.Combine(Application.persistentDataPath, "default_PlayerCosmetics.json");
    }
    private string GetPathEquipped()
    {
        if (GameManager.instance.SelectedAccount != null)
            return Path.Combine(Application.persistentDataPath, $"{GameManager.instance.SelectedAccount}_EquippedCosmetics.json");
        else
            return Path.Combine(Application.persistentDataPath, "default_EquippedCosmetics.json");
    }

    public void Save(List<CosmeticsData> items)
    {
        // Save only IDs to keep save files small and avoid recreating ScriptableObjects
        CosmeticsIdDatabase idDb = new CosmeticsIdDatabase();
        foreach (var item in items)
        {
            idDb.ids.Add(item.id);
        }

        string json = JsonUtility.ToJson(idDb, true);
        File.WriteAllText(GetPath(), json);
    }

    public List<CosmeticsData> Load()
    {
        string path = GetPath();

        if (!File.Exists(path))
            return defaultPlayerCosmetics;

        string json = File.ReadAllText(path);
        // Try new ID-based format first
        CosmeticsIdDatabase idDb = JsonUtility.FromJson<CosmeticsIdDatabase>(json);
        List<CosmeticsData> result = new List<CosmeticsData>();

        if (idDb != null && idDb.ids != null && idDb.ids.Count > 0)
        {
            foreach (var id in idDb.ids)
            {
                var asset = GameManager.instance.allCosmetics.Find(a => a.id == id);
                if (asset != null)
                    result.Add(asset);
                else
                    Debug.LogWarning($"CosmeticsManager: saved cosmetic id {id} not found in allCosmetics.");
            }
            Debug.Log("loaded cosmetics (ids)");
            return result;
        }

        // Fallback to old full-data format for backward compatibility
        CosmeticsDatabase db = JsonUtility.FromJson<CosmeticsDatabase>(json);
        if (db != null && db.cosm != null && db.cosm.Count > 0)
        {
            foreach (var data in db.cosm)
            {
                result.Add(data.ToScriptableObject());
            }
            Debug.Log("loaded cosmetics (legacy data)");
            return result;
        }

        Debug.LogWarning("CosmeticsManager: no cosmetics data found in file.");
        return result;
    }

    public void SaveEquipped(List<CosmeticsData> items)
    {
        CosmeticsIdDatabase idDb = new CosmeticsIdDatabase();
        foreach (var item in items)
        {
            idDb.ids.Add(item.id);
        }

        string json = JsonUtility.ToJson(idDb, true);
        File.WriteAllText(GetPathEquipped(), json);
    }

    public List<CosmeticsData> LoadEquipped()
    {
        string path = GetPathEquipped();

        if (!File.Exists(path))
            return defaultEquippedCosmetics;

        string json = File.ReadAllText(path);
        // Try id-based format
        CosmeticsIdDatabase idDb = JsonUtility.FromJson<CosmeticsIdDatabase>(json);
        List<CosmeticsData> result = new List<CosmeticsData>();

        if (idDb != null && idDb.ids != null && idDb.ids.Count > 0)
        {
            foreach (var id in idDb.ids)
            {
                var asset = GameManager.instance.allCosmetics.Find(a => a.id == id);
                if (asset != null)
                    result.Add(asset);
                else
                    Debug.LogWarning($"CosmeticsManager: equipped cosmetic id {id} not found in allCosmetics.");
            }
            Debug.Log("loaded equipped cosmetics (ids)");
            return result;
        }

        // Fallback to legacy format
        CosmeticsDatabase db = JsonUtility.FromJson<CosmeticsDatabase>(json);
        if (db != null && db.cosm != null && db.cosm.Count > 0)
        {
            foreach (var data in db.cosm)
            {
                result.Add(data.ToScriptableObject());
            }
            Debug.Log("loaded equipped cosmetics (legacy data)");
            return result;
        }

        Debug.LogWarning("CosmeticsManager: no equipped cosmetics data found in file.");
        return result;
    }

    void OnEnable()
    {
        CosmeticsEvents.SaveCosmetics += HandleSaveCosmetics;
        CosmeticsEvents.SaveEquipped += HandleSaveEquipped;
        CosmeticsEvents.LoadCosmetics += HandleLoadCosmetics;
        CosmeticsEvents.LoadEquipped += HandleLoadEquipped;
    }

    private void OnDisable()
    {
        CosmeticsEvents.SaveCosmetics -= HandleSaveCosmetics;
        CosmeticsEvents.SaveEquipped -= HandleSaveEquipped;
        CosmeticsEvents.LoadCosmetics -= HandleLoadCosmetics;
        CosmeticsEvents.LoadEquipped -= HandleLoadEquipped;
    }

    private void HandleSaveCosmetics()
    {
        Save(GameManager.instance.playerCosmetics);
    }

    private void HandleSaveEquipped()
    {
        SaveEquipped(GameManager.instance.playerEquippedCosmetics);
    }

    private void HandleLoadCosmetics()
    {
        GameManager.instance.playerCosmetics = Load();
    }

    private void HandleLoadEquipped()
    {
        GameManager.instance.playerEquippedCosmetics = LoadEquipped();
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

public class CosmeticsIdDatabase
{
    public List<int> ids = new List<int>();
}
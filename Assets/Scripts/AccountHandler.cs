using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class AccountHandler : MonoBehaviour
{
    public GameObject nameField;
    public GameObject AccountDisplay;
    public GameObject AccountDispPrefab;
    public List<AccountUtility.AccountData> accounts = new List<AccountUtility.AccountData>();
    
    public void Start()
    {
        accounts = AccountUtility.LoadAccounts();
        RefreshList();
        if (GameManager.instance.SelectedAccount != "")
            gameObject.SetActive(false);
    }

    public void AddAccount()
    {
        string name = nameField.GetComponent<TMPro.TMP_InputField>().text;
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Name field is empty!");
            return;
        }
        AccountUtility.AccountData newAccount = new AccountUtility.AccountData
        {
            AccountName = name,
            MasterVolume = 1.0f,
        };
        accounts.Add(newAccount);
        AccountUtility.SaveAccounts(accounts);
        RefreshList();
        nameField.GetComponent<TMPro.TMP_InputField>().text = "";
    }

    public void RefreshList()
    {
        foreach (Transform child in AccountDisplay.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in accounts)
        {
            var go = Instantiate(AccountDispPrefab, AccountDisplay.transform);
            go.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text = item.AccountName;
            var button = go.GetComponent<UnityEngine.UI.Button>();
            button.onClick.RemoveAllListeners();
            AccountUtility.AccountData captured = item;
            button.onClick.AddListener(() => SelectAccount(captured));
        }
    }

    public void SelectAccount(AccountUtility.AccountData acc)
    {
        GameManager.instance.SelectedAccount = acc.AccountName;
        GameManager.instance.volume = acc.MasterVolume;
        GameManager.instance.pianoVolume = acc.PianoVolume;
        GameManager.instance.metronomeVolume = acc.MetronomeVolume;
        GameManager.instance.sfxVolume = acc.SFXVolume;
        DataCollection.instance.TotalTimePlayed = acc.TotalTimePlayed;
        gameObject.SetActive(false);
    }
}

public static class AccountUtility
{
    public static string AccountJsonPath => Path.Combine(Application.persistentDataPath, "account.json");

    [System.Serializable]
    public class AccountData
    {
        public string AccountName;
        public float MasterVolume;
        public float PianoVolume;
        public float MetronomeVolume;
        public float SFXVolume;
        public double TotalTimePlayed = 0;
    }

    [System.Serializable]
    private class AccountDataArrayWrapper
    {
        public AccountData[] array;
    }

    public static List<AccountData> LoadAccounts()
    {
        if (!File.Exists(AccountJsonPath))
        {
            Debug.Log("No account file found, starting with empty list.");
            return new List<AccountData>();
        }
        string json = File.ReadAllText(AccountJsonPath);
        AccountDataArrayWrapper loaded = JsonUtility.FromJson<AccountDataArrayWrapper>(json);
        return loaded != null && loaded.array != null ? new List<AccountData>(loaded.array) : new List<AccountData>();
    }

    public static void SaveAccounts(List<AccountData> accounts)
    {
        AccountDataArrayWrapper wrapper = new AccountDataArrayWrapper { array = accounts.ToArray() };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(AccountJsonPath, json);
        Debug.Log("Saved accounts to: " + AccountJsonPath);
    }

    public static void UpdateAccountVolume(float newVolume, float newPianoVolume, float newMetronomeVolume, float newSFXVolume)
    {
        var accounts = LoadAccounts();
        foreach (var acc in accounts)
        {
            if (acc.AccountName == GameManager.instance.SelectedAccount)
            {
                acc.MasterVolume = newVolume;
                acc.PianoVolume = newPianoVolume;
                acc.MetronomeVolume = newMetronomeVolume;
                acc.SFXVolume = newSFXVolume;
                SaveAccounts(accounts);
                Debug.Log("Saving settings" + newVolume + " " + newPianoVolume + " " + newMetronomeVolume + " " + newSFXVolume + " for account " + GameManager.instance.SelectedAccount);
                return;
            }
        }
        
    }

    public static void UpdateAccountTimePlayed(double TimePlayed)
    {
        var accounts = LoadAccounts();
        foreach (var acc in accounts)
        {
            if (acc.AccountName == GameManager.instance.SelectedAccount)
            {
                acc.TotalTimePlayed += TimePlayed;
                SaveAccounts(accounts);
                Debug.Log($"Account '{GameManager.instance.SelectedAccount}' total time played updated: {acc.TotalTimePlayed}");
                return;
            }
        }
        Debug.LogWarning($"Account '{GameManager.instance.SelectedAccount}' not found.");
    }
}

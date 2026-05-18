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
            Volume = 1.0f,
            TutorialCompleted = false
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
        GameManager.instance.volume = acc.Volume;
        GameManager.instance.tutorialCompleted = acc.TutorialCompleted;
        GameManager.instance.currency = acc.currency;
        DataCollection.instance.TotalTimePlayed = acc.TotalTimePlayed;
        GameManager.instance.loadSettings();
        GameEvents.LoadAchievements.Invoke();
        QuestEvents.LoadQuests.Invoke();
        CosmeticsEvents.LoadCosmetics.Invoke();
        CosmeticsEvents.LoadEquipped.Invoke(); //wczytywanie cosmetików

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
        public float Volume;
        public bool TutorialCompleted = false;
        public int currency = 0;
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

    public static void UpdateAccountVolume(float newVolume)
    {
        var accounts = LoadAccounts();
        foreach (var acc in accounts)
        {
            if (acc.AccountName == GameManager.instance.SelectedAccount)
            {
                acc.Volume = newVolume;
                SaveAccounts(accounts);
                Debug.Log($"Account '{GameManager.instance.SelectedAccount}' volume updated: {newVolume}");
                return;
            }
        }
        Debug.LogWarning($"Account '{GameManager.instance.SelectedAccount}' not found.");
    }

    public static void UpdateAccountTutorialCompleted(bool newTutorialCompleted)
    {
        var accounts = LoadAccounts();
        foreach (var acc in accounts)
        {
            if (acc.AccountName == GameManager.instance.SelectedAccount)
            {
                acc.TutorialCompleted = newTutorialCompleted;
                SaveAccounts(accounts);
                Debug.Log($"Account '{GameManager.instance.SelectedAccount}' tutorialCompleted updated: {newTutorialCompleted}");
                return;
            }
        }
        Debug.LogWarning($"Account '{GameManager.instance.SelectedAccount}' not found.");
    }

    public static void UpdateAccountCurrency(int newCurrency) //<--- pamietac o tym
    {
        var accounts = LoadAccounts();
        foreach (var acc in accounts)
        {
            if (acc.AccountName == GameManager.instance.SelectedAccount)
            {
                acc.currency = newCurrency;
                SaveAccounts(accounts);
                Debug.Log($"Account '{GameManager.instance.SelectedAccount}' currency updated: {newCurrency}");
                return;
            }
        }
        Debug.LogWarning($"Account '{GameManager.instance.SelectedAccount}' not found.");
    }

    public static void UpdateAccountTimePlayed(double TimePlayed) //<--- pamietac o tym
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

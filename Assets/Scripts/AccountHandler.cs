using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class AccountHandler : MonoBehaviour
{
    public void Start()
    {
        accounts = LoadAccounts();
        RefreshList();
        if(GameManager.instance.SelectedAccount != null)
            gameObject.SetActive(false);
    }
    

    public string AccountJsonPath => Path.Combine(Application.persistentDataPath, "account.json");
    public GameObject nameField;
    public GameObject AccountDisplay;
    public GameObject AccountDispPrefab;
    public List<string> accounts = new List<string>();

    public List<string> LoadAccounts()
    {
        if (!File.Exists(AccountJsonPath))
        {
            Debug.Log("No account file found, starting with empty list.");
            return new List<string>();
        }
        string json = File.ReadAllText(AccountJsonPath);
        string[] loaded = JsonUtility.FromJson<StringArrayWrapper>(json)?.array;
        return loaded != null ? new List<string>(loaded) : new List<string>();
    }

    public void SaveAccounts(List<string> accounts)
    {
        StringArrayWrapper wrapper = new StringArrayWrapper { array = accounts.ToArray() };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(AccountJsonPath, json);
        Debug.Log("Saved accounts to: " + AccountJsonPath);
    }

    public void AddAccount()
    {
        string name = nameField.GetComponent<TMPro.TMP_InputField>().text;
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Name field is empty!");
            return;
        }
        string newAccount = name;
        accounts.Add(newAccount);
        SaveAccounts(accounts);
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
            go.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text = item;
            var button = go.GetComponent<UnityEngine.UI.Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectAccount(item));
        }
    }

    public void SelectAccount(string acc)
    {
        GameManager.instance.SelectedAccount = acc;
        gameObject.SetActive(false);
    }


    [System.Serializable]
    private class StringArrayWrapper
    {
        public string[] array;
    }
}

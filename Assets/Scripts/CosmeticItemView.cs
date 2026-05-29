using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CosmeticItemView : MonoBehaviour
{
    public CosmeticsData cosmdata;
    private PopulateCosmeticsList popcsom;
    public void Initialize(CosmeticsData item, PopulateCosmeticsList pop, bool shop)
    {
        cosmdata = item;
        popcsom = pop;
        if (shop)
        {
            var SelectButton = GetComponent<UnityEngine.UI.Button>();
            SelectButton.onClick.RemoveAllListeners();
            SelectButton.onClick.AddListener(() => popcsom.OpenPurchaseMenu(item));
            cosmdata = item;
            transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = item.itemName;
            transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = item.CurrencyCost.ToString() + "$";
            transform.GetChild(2).GetComponent<Image>().sprite = item.sprite;
        }
        else
        {
            var SelectButton = GetComponent<UnityEngine.UI.Button>();
            SelectButton.onClick.RemoveAllListeners();
            SelectButton.onClick.AddListener(() => popcsom.OpenCollectionMenu(item));
            cosmdata = item;
            transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = item.itemName;
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).GetComponent<Image>().sprite = item.sprite;
        }
    }

    private Dictionary<CosmeticType, string> Types = new Dictionary<CosmeticType, string>
    {
        { CosmeticType.Background, "T³o" },
        { CosmeticType.NoteSkin, "Nuta" },
        { CosmeticType.KeySkin, "Klawisze" },
        { CosmeticType.Font, "Czcionka" }
    };
    private List<string> Sets = new List<string>() { "Set 1", "Set 2", "Set 3", "Set 4", "Set 5" };
}


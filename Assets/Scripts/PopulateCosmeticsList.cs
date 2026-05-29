using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopulateCosmeticsList : MonoBehaviour
{
    public GameObject CosmeticItemPrefab;
    public GameObject ContentPanel;
    public GameObject PurchaseMenu;
    public GameObject CollectionMenu;
    public TMP_Text currency;
    [SerializeField] private bool shop;

    public void OpenPurchaseMenu(CosmeticsData data)
    {
        PurchaseMenu.SetActive(true);
        PurchaseMenu.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
        PurchaseMenu.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => Purchase(data));
        PurchaseMenu.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => PurchaseMenu.SetActive(false));

        PurchaseMenu.transform.GetChild(2).GetComponent<TMP_Text>().text = data.itemName;
    }

    private void Purchase(CosmeticsData data)
    {
        if (data.CurrencyCost <= GameManager.instance.currency)
        {
            GameManager.instance.currency -= data.CurrencyCost;
            AccountUtility.UpdateAccountCurrency(GameManager.instance.currency);
            GameManager.instance.playerCosmetics.Add(data);
            GameEvents.OnPurchaseItem.Invoke(1);
            CosmeticsEvents.SaveCosmetics?.Invoke();
            CosmeticsEvents.LoadCosmetics?.Invoke();
            RefreshList();
            Debug.Log("Purchase made");
        }
    }

    public void OpenCollectionMenu(CosmeticsData data)
    {
        CollectionMenu.SetActive(true);
        CollectionMenu.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
        CollectionMenu.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => Equip(data));
        CollectionMenu.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => CollectionMenu.SetActive(false));
        //wiecej z menu kupowania
        CollectionMenu.transform.GetChild(2).GetComponent<TMP_Text>().text = data.itemName;
    }

    private void Equip(CosmeticsData data)
    {
        var existingIndex = GameManager.instance.playerEquippedCosmetics.FindIndex(c => c.type == data.type);
        Debug.Log("Swapping Item at intex" + existingIndex);
        if (existingIndex >= 0)
            GameManager.instance.playerEquippedCosmetics[existingIndex] = data;
        else
            GameManager.instance.playerEquippedCosmetics.Add(data);
        CosmeticsEvents.SaveEquipped?.Invoke();
        CosmeticsEvents.LoadEquipped?.Invoke();
        RefreshList();
        Debug.Log("Equipped");
    }
    void Start()
    {
        RefreshList();
        Debug.Log("startpopulate");
        Debug.Log("playercosmetics count: " + GameManager.instance.playerCosmetics.Count);
    }
    void Update()
    {

    }

    public void RefreshList()
    {
        bool showFontCosmetics = GameManager.instance.pointsOn || GameManager.instance.hitQualityOn;

        if(shop)
        {
            foreach (Transform child in ContentPanel.transform)
            {
                Destroy(child.gameObject);
            }
            var availableCosmetics = GameManager.instance.allCosmetics.FindAll(c =>
                (!GameManager.instance.playerCosmetics.Exists(p => p.id == c.id)) &&
                (showFontCosmetics || c.type != CosmeticType.Font));
            
            foreach (var item in availableCosmetics)
            {
                var go = Instantiate(CosmeticItemPrefab, ContentPanel.transform);
                go.GetComponent<CosmeticItemView>().Initialize(item, this, shop);
            }
            currency.text = GameManager.instance.currency.ToString() + "$";
        }
        else
        {
            foreach (Transform child in ContentPanel.transform)
            {
                Destroy(child.gameObject);
            }
            var equipped = GameManager.instance.playerEquippedCosmetics;
            var owned = GameManager.instance.playerCosmetics;
            HashSet<int> spawnedIds = new HashSet<int>();
            foreach (var item in equipped)
            {
                if (owned.Exists(c => c.id == item.id) && (showFontCosmetics || item.type != CosmeticType.Font))
                {
                    var go = Instantiate(CosmeticItemPrefab, ContentPanel.transform);
                    go.GetComponent<CosmeticItemView>().Initialize(item, this, shop);

                    // Zmie� kolor na czerwony
                    var image = go.GetComponent<Image>();
                    if (image != null)
                        image.color = Color.red;

                    spawnedIds.Add(item.id);
                }
            }
            foreach (var item in owned)
            {
                if (!spawnedIds.Contains(item.id) && (showFontCosmetics || item.type != CosmeticType.Font))
                {
                    var go = Instantiate(CosmeticItemPrefab, ContentPanel.transform);
                    go.GetComponent<CosmeticItemView>().Initialize(item, this, shop);
                }
            }
        }
    }
}

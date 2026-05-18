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
            GameManager.instance.playerCosmetics.Add(data);
            CosmeticsEvents.SaveCosmetics?.Invoke();
            CosmeticsEvents.LoadCosmetics?.Invoke();
            RefreshList();
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
        if (existingIndex >= 0)
            GameManager.instance.playerEquippedCosmetics[existingIndex] = data;
        else
            GameManager.instance.playerEquippedCosmetics.Add(data);
        CosmeticsEvents.SaveEquipped?.Invoke();
        CosmeticsEvents.LoadEquipped?.Invoke();
        RefreshList();
    }
    void Start()
    {
        RefreshList();

    }
    void Update()
    {

    }

    public void RefreshList()
    {
        if(shop)
        {
            foreach (Transform child in ContentPanel.transform)
            {
                Destroy(child.gameObject);
            }
            var availableCosmetics = GameManager.instance.allCosmetics.FindAll(c => !GameManager.instance.playerCosmetics.Contains(c));
            foreach (var item in availableCosmetics)
            {
                var go = Instantiate(CosmeticItemPrefab, ContentPanel.transform);
                go.GetComponent<CosmeticItemView>().Initialize(item, this, shop);
            }
            currency.text = GameManager.instance.currency.ToString();
        }
        else
        {
            foreach (Transform child in ContentPanel.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (var item in GameManager.instance.playerCosmetics)
            {
                var go = Instantiate(CosmeticItemPrefab, ContentPanel.transform);
                go.GetComponent<CosmeticItemView>().Initialize(item, this, shop);
            }
        }
    }
}

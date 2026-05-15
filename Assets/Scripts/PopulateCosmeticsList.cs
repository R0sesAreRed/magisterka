using UnityEngine;

public class PopulateCosmeticsList : MonoBehaviour
{
    public GameObject CosmeticItemPrefab;
    public GameObject ContentPanel;
    public GameObject PurchaseMenu;
    public GameObject CollectionMenu;
    [SerializeField] private bool shop;

    public void OpenPurchaseMenu(CosmeticsData data)
    {
        PurchaseMenu.SetActive(true);
        PurchaseMenu.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
        PurchaseMenu.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => Purchase(data));
        PurchaseMenu.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => PurchaseMenu.SetActive(false));

        //wiecej z menu kupowania
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
        PurchaseMenu.SetActive(true);
        PurchaseMenu.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
        PurchaseMenu.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => Equip(data));
        PurchaseMenu.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => CollectionMenu.SetActive(false));
        //wiecej z menu kupowania
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
        foreach (Transform child in ContentPanel.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var item in GameManager.instance.allCosmetics)
        {
            var go = Instantiate(CosmeticItemPrefab, ContentPanel.transform);
            go.GetComponent<CosmeticItemView>().Initialize(item, this, shop);
        }
    }
}

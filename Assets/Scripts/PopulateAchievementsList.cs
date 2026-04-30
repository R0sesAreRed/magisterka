using UnityEngine;

public class PopulateAchievementsList : MonoBehaviour
{
    [SerializeField] private GameObject achievementListItemPrefab;
    [SerializeField] private GameObject listParent;
    void Start()
    {
        RefreshList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RefreshList()
    {
        foreach (Transform child in listParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in GameManager.instance.playerAchievements)
        {
            var go = Instantiate(achievementListItemPrefab, listParent.transform);
            go.GetComponent<AchievementItemView>().Initialize(item);
        }
    }
}

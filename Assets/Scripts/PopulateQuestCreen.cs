using UnityEngine;

public class PopulateQuestCreen : MonoBehaviour
{

    public GameObject QuestParent;
    public GameObject QuestPrefab;
    void OnEnable()
    {
        foreach (Transform child in QuestParent.transform)
        {
            if (child.GetComponent<QuestDisplay>() != null)
            {
                Destroy(child.gameObject);
            }
        }
        foreach (var quest in GameManager.instance.playerCurrentQuests)
        {
            var questObj = Instantiate(QuestPrefab, QuestParent.transform);
            if(GameManager.instance.shopAndCurrencyOn)
            {
                if (!quest.OneLevel)
                    questObj.GetComponent<QuestDisplay>().Initialize(quest.description, quest.targetValue, quest.currentValue, quest.rewardCurrency);
                else
                    questObj.GetComponent<QuestDisplay>().Initialize(quest.description, quest.rewardCurrency);
            }
            else
            {
                if (!quest.OneLevel)
                    questObj.GetComponent<QuestDisplay>().Initialize(quest.description, quest.targetValue, quest.currentValue, quest.altReward?.itemName);
                else
                    questObj.GetComponent<QuestDisplay>().Initialize(quest.description, quest.altReward?.itemName);
            }
            Debug.Log(quest.OneLevel);
        }
    }

    // Update is called once per frame
    void OnDisable()
    {
        foreach (Transform child in QuestParent.transform)
        {
            Destroy(child.gameObject);
        }
    }
}

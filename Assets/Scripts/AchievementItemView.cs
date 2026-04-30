using UnityEngine;
using UnityEngine.UI;

public class AchievementItemView : MonoBehaviour
{
    public AchievementData achdata;

    public void Initialize(AchievementData item)
    {
        achdata = item;
        transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = item.title;
        transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = item.description;
        transform.GetComponent<Image>().color = item.completed ? Color.green : Color.white;
    }
}

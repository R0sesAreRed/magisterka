using UnityEngine;

public class QuestDisplay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(string desc, int goal, int currprog, int reward)
    {
        transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = desc;
        transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = $"{currprog}/{goal}";
        transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = $"Nagroda: {reward.ToString()}$";
    }
    public void Initialize(string desc, int goal, int currprog, string altreward)
    {
        transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = desc;
        transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = $"{currprog}/{goal}";
        transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = $"Nagroda: {altreward}$";
    }
    public void Initialize(string desc, int reward)
    {
        transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = desc;
        transform.GetChild(1).gameObject.SetActive(false);
        transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = $"Nagroda: {reward.ToString()}$";
    }
    public void Initialize(string desc, string altreward)
    {
        transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = desc;
        transform.GetChild(1).gameObject.SetActive(false);
        transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = $"Nagroda: {altreward}$";
    }
}

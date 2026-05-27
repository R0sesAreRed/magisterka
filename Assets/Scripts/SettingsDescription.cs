using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SettingsDescription : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string Description;
    public Sprite Icon;
    public GameObject DescriptionPanel;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse is over the settings item." + gameObject.name);
        DescriptionPanel.SetActive(true);
        DescriptionPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Description;
        // DescriptionPanel.transform.GetChild(1).GetComponent<Image>().sprite = Icon;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DescriptionPanel.SetActive(false);
    }
}

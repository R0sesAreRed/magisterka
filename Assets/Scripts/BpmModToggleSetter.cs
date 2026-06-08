using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class BpmModToggleSetter : MonoBehaviour
{
    [SerializeField] private double bpmModValue = 1.0;

    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        toggle.onValueChanged.AddListener(HandleValueChanged);
    }

    private void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(HandleValueChanged);
    }

    private void HandleValueChanged(bool isOn)
    {
        if (!isOn || GameManager.instance == null)
        {
            return;
        }

        GameManager.instance.BPMmod = bpmModValue;
        Debug.Log($"BPM mod set to: {GameManager.instance.BPMmod}");
    }
}

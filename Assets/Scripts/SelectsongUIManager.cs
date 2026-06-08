using UnityEngine;

public class SelectsongUIManager : MonoBehaviour
{
    public static SelectsongUIManager instance;
    public GameObject GameModeSelection;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

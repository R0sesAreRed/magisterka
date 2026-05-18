using UnityEngine;

public class DataCollection : MonoBehaviour
{
    public int TotalNotes;
    public int MissedNotes = 0;
    public int OkNotes = 0;
    public int GoodNotes = 0;
    public int PerfectNotes = 0;
    public double TotalTimePlayed = 0;
    public bool LevelSuccess = false;

    public static DataCollection instance;
    void Start()
    {
        if (instance == null && instance != this)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SubmitData()
    {
        TotalNotes = GameManager.instance.CurrMidiNotes.Count;
        //strukturyzacja danych

        //przes³anie danych do bazy

        //reset liczników
        ResetCouters();
    }

    public void ResetCouters()
    {     
        MissedNotes = 0;
        OkNotes = 0;
        GoodNotes = 0;
        PerfectNotes = 0;
        LevelSuccess = false;
    }

}

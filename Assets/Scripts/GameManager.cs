using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public InputSystem_Actions inputActions;

    public float screenHeight = Screen.height;
    public bool IsPaused = false;
    public List<SelectSongItem> importedFiles = new List<SelectSongItem>();
    [SerializeField] private SongSaveSystem saveSystem;


    public float songStartTime;
    public int nextNoteIndex; //te dwie nie wiem czy tu powinny byæ, na razie tu zostaj¹

    //[SerializeField] TextMeshProUGUI scoreText;

    //private int score = 0; //punkty zdobywane w czasie gry

    //public int Score
    //{
    //    get { return score; }
    //    set
    //    {
    //        score = value;
    //        scoreText.text = "Score: " + score.ToString();
    //    }
    //}

    public List<Notes> CurrMidiNotes;
    public enum NK { C3, CS3, D3, DS3, E3, F3, FS3, G3, GS3, A3, AS3, B3, C4, CS4, D4, DS4, E4, F4, FS4, G4, GS4, A4, AS4, B4, C5 } //enum z nazwami nut

    private void Awake()
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
        inputActions = new InputSystem_Actions();
        Object.DontDestroyOnLoad(gameObject);
    }



    

    public class Notes
    {
        public NK Note;
        public double StartTime; // w mili
        public double Length;    // w mili
    }
}

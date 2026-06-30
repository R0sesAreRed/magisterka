using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string SelectedAccount; 

    public static GameManager instance;
    public InputSystem_Actions inputActions;

    public float screenHeight = Screen.height;

    public bool IsPaused = false;

    public List<SelectSongItem> importedFiles = new List<SelectSongItem>();
    public Dictionary<GameManager.NK, AudioClip> noteClipsCopy = new Dictionary<GameManager.NK, AudioClip>();
    [SerializeField] private SongSaveSystem saveSystem;
    [SerializeField] private AudioMixer audiomixer;
    public SelectSongItem currentSong;


    public float songStartTime;
    public int nextNoteIndex; //te dwie nie wiem czy tu powinny by�, na razie tu zostaj�

    public bool pointsOn = false; //+
    public bool progressBarOn = false ; //+
    public bool hitQualityOn = false; //+
    // public bool comboOn = true; //+
    // public bool perKeyFeedbackOn = true; //+
    // public bool onlyGoodSoundOn = true; //+
    //public bool notesHighlightingOn = true; //+

    public bool gamificationOn;

    public int CurrSongBPM;



    private float Volume;
    public float volume
    {
        get { return Volume; }
        set
        {
            Volume = value;
            audiomixer.SetFloat("masterVol", value == 0 ? -80 : Mathf.Log10(value) * 20);
        }
    }
    private float PianoVolume;
    public float pianoVolume
    {
        get { return PianoVolume; }
        set
        {
            PianoVolume = value;
            audiomixer.SetFloat("pianoVol", value == 0 ? -80 : Mathf.Log10(value) * 20);
        }
    }
    private float MetronomeVolume;
    public float metronomeVolume
    {
        get { return MetronomeVolume; }
        set
        {
            MetronomeVolume = value;
            audiomixer.SetFloat("metronomVol", value == 0 ? -80 : Mathf.Log10(value) * 20);
        }
    }
    private float SFXVolume;
    public float sfxVolume
    {
        get { return SFXVolume; }
        set
        {
            SFXVolume = value;
            audiomixer.SetFloat("sfxVol", value == 0 ? -80 : Mathf.Log10(value) * 20);
        }
    }




    public bool tutorialCompleted = false;
    public bool verification = false;
    public double BPMmod = 1.0;

    public string GetPersistSettingsName()
    {
        return SelectedAccount != null ? $"{SelectedAccount}_settings" : "default_settings";
    }

    [SerializeField] MainMenuUIMAnager MMUIM;
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        MMUIM = FindFirstObjectByType<MainMenuUIMAnager>();
    }

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
        //Time.timeScale = 0.5f;
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

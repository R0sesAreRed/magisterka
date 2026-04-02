using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static KeyboardManager;

public class KeyboardManager : MonoBehaviour
{
    public static KeyboardManager instance;

    [SerializeField] public GameObject[] Keys; //tablica przechowuj¹ca klawisze do przypiêcia
    public Dictionary<GameManager.NK, GameObject> KeysDict; //s³ownik mapuj¹cy klawisze do nazw nut
    private Dictionary<GameManager.NK, BoxCollider2D> KeyColliders; //s³ownik mapuj¹cy collidery klawiszy do nazw nut
    public Dictionary<GameManager.NK, GameObject> KeyVisualsDict; //s³ownik trzymaj¹cy wizualn¹ czêœæ klawiszy


    private Dictionary<GameManager.NK, Action<InputAction.CallbackContext>> performedDelegates; //s³ownik przechowuj¹cy delegaty dla zdarzeñ "performed"
    private Dictionary<GameManager.NK, Action<InputAction.CallbackContext>> canceledDelegates; //s³ownik przechowuj¹cy delegaty dla zdarzeñ "canceled"
    private Dictionary<GameManager.NK, float> lastCollisionTime = new(); //s³ownik przechowuj¹cy czas ostatniej kolizji dla ka¿dej nuty
    private Dictionary<GameManager.NK, float> lastKeyPressTime = new(); //s³ownik przechowuj¹cy czas ostatniego naciœniêcia klawisza dla ka¿dej nuty

    private Dictionary<GameManager.NK, NoteTiming> noteTimings = new();

    public float ScreenHeight = 0;
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
        } //dok instancji
        ScreenHeight = this.GetComponent<RectTransform>().rect.height;
        KeysDict = new Dictionary<GameManager.NK, GameObject>();
        KeyColliders = new Dictionary<GameManager.NK, BoxCollider2D>();
        KeyVisualsDict = new Dictionary<GameManager.NK, GameObject>();

        for (int i = 0; i < Keys.Length && i < Enum.GetValues(typeof(GameManager.NK)).Length; i++)
        {
            KeysDict[(GameManager.NK)i] = Keys[i];
            KeysDict[(GameManager.NK)i].name = ((GameManager.NK)i).ToString();
            KeyColliders[(GameManager.NK)i] = Keys[i].GetComponentInChildren<BoxCollider2D>();
            KeyVisualsDict[(GameManager.NK)i] = Keys[i].transform.GetChild(0).gameObject;
            Debug.Log(KeyVisualsDict[(GameManager.NK)i]);
            var detector = Keys[i].GetComponent<KeyCollisionDetector>();
            if (detector != null)
            {
                detector.note = (GameManager.NK)i;
            }
        }


    }

    private void Start()
    {
        foreach (var noteData in GameManager.instance.CurrMidiNotes) 
        {
            noteTimings[noteData.Note] = new NoteTiming
            {
                noteStartTime = (noteData.StartTime / 1000.0) + 1.5f, // PRZY ZMIANIE TEMPA LOTU NUT TO TRZEBA ZMIENIÆ. NAJLEPIEJ W OGOLE TO ZROBIC ZALEZNE (3/4 * tempo spadania -> obecnie 2)
                noteLength = (noteData.Length / 1000.0)
            };
            Debug.Log($"Zarejestrowano nutê {noteData.Note} z czasem startu {noteTimings[noteData.Note].noteStartTime} i d³ugoœci¹ {noteTimings[noteData.Note].noteLength}");
        }
    }


    private void OnEnable() //przypisuje funkcje
    {
        GameManager.instance.inputActions.Piano.Enable();
        performedDelegates = new();
        canceledDelegates = new();

        foreach (GameManager.NK note in Enum.GetValues(typeof(GameManager.NK)))
        {
            performedDelegates[note] = ctx => OnKeyPerformed(note, ctx);
            canceledDelegates[note] = ctx => OnKeyCanceled(note, ctx);

            var piano = GameManager.instance.inputActions.Piano;
            var action = piano.GetType().GetProperty(note.ToString())?.GetValue(piano) as InputAction;
            if (action != null)
            {
                action.performed += performedDelegates[note];
                action.canceled += canceledDelegates[note];
            }
        }
    }

    private void OnDisable() //odpiananie funkcji przy wy³¹czeniu skryptu
    {
        foreach (GameManager.NK note in Enum.GetValues(typeof(GameManager.NK)))
        {
            var piano = GameManager.instance.inputActions.Piano;
            var action = piano.GetType().GetProperty(note.ToString())?.GetValue(piano) as InputAction;
            if (action != null)
            {
                action.performed -= performedDelegates[note];
                action.canceled -= canceledDelegates[note];
            }
        }
        GameManager.instance.inputActions.Piano.Disable();
    }

    

    private void OnKeyPerformed(GameManager.NK note, InputAction.CallbackContext ctx)
    {
        KeyPressColor(ctx, note);

        double hitTime = GetCurrentSongTime(); // np. Time.time - songStartTime
        if (noteTimings.TryGetValue(note, out var timing))
        {
            timing.hitTime = hitTime;
        }
    }

    private void OnKeyCanceled(GameManager.NK note, InputAction.CallbackContext ctx)
    {
        KeyReleaseColor(ctx, note);

        double releaseTime = GetCurrentSongTime();
        if (noteTimings.TryGetValue(note, out var timing) && timing.hitTime.HasValue && !timing.scored)
        {
            timing.releaseTime = releaseTime;

            // Oblicz wynik
            int score = GameUIManager.instance.CalculateScore(
                timing.hitTime.Value,
                timing.noteStartTime,
                timing.releaseTime.Value,
                timing.noteLength,
                note
            );

            // Dodaj punkty do wyniku
            GameUIManager.instance.Score += score;

            // Oznacz nutê jako rozliczon¹
            timing.scored = true;

            // (opcjonalnie) wyczyœæ dane, jeœli nie chcesz liczyæ ponownie
            timing.hitTime = null;
            timing.releaseTime = null;
        }
    }



    public class NoteTiming
    {
        public double noteStartTime;
        public double noteLength;
        public double? hitTime;
        public double? releaseTime;
        public bool scored = false; // czy ju¿ przyznano punkty za tê nutê
    }

    private double GetCurrentSongTime()
    {
        return Time.time - GameManager.instance.songStartTime;
    }



    private void KeyPressColor(InputAction.CallbackContext context, GameManager.NK note)
    {
        KeyVisualsDict[note].GetComponent<Image>().color = KeyVisualsDict[note].GetComponent<Image>().color == Color.white ? Color.gray : Color.blue;
    }
    private void KeyReleaseColor(InputAction.CallbackContext context, GameManager.NK note)
    {
        KeyVisualsDict[note].GetComponent<Image>().color = KeyVisualsDict[note].GetComponent<Image>().color == Color.gray ? Color.white : Color.black;
    }


    public void RegisterCollision(GameManager.NK note, float time)
    {
        lastCollisionTime[note] = time;

        if (lastKeyPressTime.TryGetValue(note, out float pressTime))
        {
            float delta = pressTime - time;
            if (delta >= -0.1f && delta <= 0.1f)
            {
                GameUIManager.instance.Score += 10;
                lastKeyPressTime.Remove(note); //zapobiega podwójnym punktom
                //Debug.Log($"Punkty (kolizja po naciœniêciu): {GameManager.instance.Score}");
            }
        }
    }

    public void CheckMissedNote(GameManager.NK note, float exitTime)
    {
        if (lastCollisionTime.ContainsKey(note))
        {
            if (noteTimings.TryGetValue(note, out var timing) && timing.hitTime == null && !timing.scored)
            {
                int score = GameUIManager.instance.CalculateScore(
                    double.NaN,
                    timing.noteStartTime,
                    double.NaN,
                    timing.noteLength,
                    note
                );
                GameUIManager.instance.Score += score;
                timing.scored = true;
                Debug.Log($"MISS: Nuta {note} zosta³a ca³kowicie pominiêta, punkty: {score}");
            }

            lastCollisionTime.Remove(note);
        }
    }

}

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

    private Dictionary<GameManager.NK, Queue<NoteTiming>> noteTimings = new();


    public float ScreenHeight = 0;
    private void Awake()
    {
        Debug.Log("KeyboardManager Awake: " + gameObject.name);
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
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
            //Debug.Log(KeyVisualsDict[(GameManager.NK)i]);
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
            if (!noteTimings.ContainsKey(noteData.Note))
                noteTimings[noteData.Note] = new Queue<NoteTiming>();

            noteTimings[noteData.Note].Enqueue(new NoteTiming
            {
                noteStartTime = (noteData.StartTime / 1000.0) + 1.5f,
                noteLength = (noteData.Length / 1000.0)
            });
            //Debug.Log($"Zarejestrowano nutê {noteData.Note} z czasem startu {noteTimings[noteData.Note].noteStartTime} i d³ugoœci¹ {noteTimings[noteData.Note].noteLength}");
        }
    }


    private void OnEnable() //przypisuje funkcje
    {
        Debug.Log("KeyboardManager OnEnable: " + gameObject.name);
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


    private Dictionary<GameManager.NK, NoteTiming> activeNoteTimings = new();
    private NoteTiming FindActiveNote(Queue<NoteTiming> queue, double currentTime)
    {
        foreach (var timing in queue)
        {
            // Za³ó¿my, ¿e nuta jest aktywna, jeœli jej startTime <= currentTime < startTime + noteLength + tolerancja
            if (!timing.scored && currentTime >= timing.noteStartTime && currentTime <= timing.noteStartTime + timing.noteLength)
            {
                return timing;
            }
        }
        return null;
    }
    private void OnKeyPerformed(GameManager.NK note, InputAction.CallbackContext ctx)
    {
        KeyPressColor(ctx, note);

        double hitTime = GetCurrentSongTime();
        if (noteTimings.TryGetValue(note, out var queue) && queue.Count > 0)
        {
            var timing = FindActiveNote(queue, hitTime);
            if (timing != null)
            {
                timing.hitTime = hitTime;
                activeNoteTimings[note] = timing;
            }
        }
    }

    private void OnKeyCanceled(GameManager.NK note, InputAction.CallbackContext ctx)
    {
        KeyReleaseColor(ctx, note);

        double releaseTime = GetCurrentSongTime();
        if (activeNoteTimings.TryGetValue(note, out var timing) && timing.hitTime.HasValue && !timing.scored)
        {
            timing.releaseTime = releaseTime;

            int score = GameUIManager.instance.CalculateScore(
                timing.hitTime.Value,
                timing.noteStartTime,
                timing.releaseTime.Value,
                timing.noteLength,
                note
            );

            GameUIManager.instance.Score += score;
            timing.scored = true;
            timing.hitTime = null;
            timing.releaseTime = null;

            // Usuñ tê nutê z kolejki, bo zosta³a ju¿ zagrana
            if (noteTimings.TryGetValue(note, out var queue2))
            {
                // ZnajdŸ i usuñ tê nutê z kolejki
                var arr = queue2.ToArray();
                queue2.Clear();
                foreach (var t in arr)
                {
                    if (t != timing)
                        queue2.Enqueue(t);
                }
            }

            activeNoteTimings.Remove(note);
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
                GameUIManager.instance.Score += 10; //??
                lastKeyPressTime.Remove(note); //zapobiega podwójnym punktom
                //Debug.Log($"Punkty (kolizja po naciœniêciu): {GameManager.instance.Score}");
            }
        }
    }
    private List<NoteTiming> FindMissedChordNotes(Queue<NoteTiming> queue, double currentTime)
    {
        List<NoteTiming> missedNotes = new List<NoteTiming>();
        // Szukamy najwczeœniejszego niezaliczonego startu
        double? earliestStart = null;
        foreach (var timing in queue)
        {
            if (!timing.scored && timing.hitTime == null && currentTime > timing.noteStartTime + timing.noteLength)
            {
                if (earliestStart == null || timing.noteStartTime < earliestStart)
                    earliestStart = timing.noteStartTime;
            }
        }
        if (earliestStart == null)
            return missedNotes;

        // Zbieramy wszystkie nuty z tym samym startem
        foreach (var timing in queue)
        {
            if (!timing.scored && timing.hitTime == null && Math.Abs(timing.noteStartTime - earliestStart.Value) < 0.0001)
            {
                missedNotes.Add(timing);
            }
        }
        return missedNotes;
    }

    public void CheckMissedNote(GameManager.NK note, float exitTime)
    {
        if (lastCollisionTime.ContainsKey(note))
        {
            if (noteTimings.TryGetValue(note, out var queue) && queue.Count > 0)
            {
                double currentTime = GetCurrentSongTime();
                var missedNotes = FindMissedChordNotes(queue, currentTime);

                // Jeœli s¹ nuty do rozliczenia
                if (missedNotes.Count > 0)
                {
                    // Rozlicz miss tylko raz dla akordu
                    Debug.Log("Missed chord or note at time: " + missedNotes[0].noteStartTime);

                    foreach (var timing in missedNotes)
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
                    }
                    GameUIManager.instance.HealthPoints -= 1000;
                    // Usuñ rozliczone nuty z kolejki
                    var arr = queue.ToArray();
                    queue.Clear();
                    foreach (var t in arr)
                    {
                        if (!missedNotes.Contains(t))
                            queue.Enqueue(t);
                    }
                }
            }
            lastCollisionTime.Remove(note);
        }
    }


}

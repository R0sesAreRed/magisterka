using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FalingNotesSpawner : MonoBehaviour
{
    [SerializeField] private KeyboardManager keyboardManager;
    private Dictionary<GameManager.NK, float> XPositions = new();

    public GameObject notePrefab;
    public Transform notesParent;

    
    [HideInInspector] public float pauseTime;


    private void Awake()
    {
        notesParent = this.transform;
        if (GameManager.instance != null)
        {
            GameManager.instance.songStartTime = Time.time;
            GameManager.instance.nextNoteIndex = 0;
            Debug.Log("[FalingNotesSpawner] songStartTime set to " + GameManager.instance.songStartTime);
        }
        if(!GameManager.instance.tutorialCompleted)
        {
            Pause();
        }
    }

    private IEnumerator Start() //UI zajmuje troche czasu u³ozenie sie po pocz¹tku sceny
    {
        yield return null;
        
        XPositions.Clear();
        int enumLength = System.Enum.GetValues(typeof(GameManager.NK)).Length; //zbiera pozycje x klawiszy z opoznieniem ¿eby ui mia³o czas sie u³o¿yæ

        GameObject[] keysArray = keyboardManager.Keys;
        for (int i = 0; i < enumLength; i++)
        {
            GameManager.NK note = (GameManager.NK)i;
            if (keysArray != null && i < keysArray.Length && keysArray[i] != null)
            {
                XPositions[note] = keysArray[i].transform.position.x;
            }
        }
        //Vector3 spawnPos = new Vector3(XPositions[GameManager.NK.C4], notesParent.position.y, 0f);
        //GameObject noteObj = Instantiate(notePrefab, spawnPos, Quaternion.identity, notesParent);
        GameManager.instance.songStartTime = Time.time;

    }

    private Action<InputAction.CallbackContext> pauseResumeDelegate;

    private void OnEnable()
    {
        if (pauseResumeDelegate == null)
            pauseResumeDelegate = ctx => PauseResume(ctx);
        GameManager.instance.inputActions.Piano.PauseResume.performed += pauseResumeDelegate;
    }

    private void OnDisable()
    {
        if (pauseResumeDelegate != null)
            GameManager.instance.inputActions.Piano.PauseResume.performed -= pauseResumeDelegate;
    }



    void Update()
    {
        if (GameManager.instance.IsPaused)
            return;

        float now = Time.time;
        if (GameManager.instance == null || GameManager.instance.CurrMidiNotes == null)
            return;

        float elapsedTime = (now - GameManager.instance.songStartTime) * 1000f; // w ms
        GameUIManager.instance.songTimePlayed = elapsedTime;
        while (GameManager.instance.nextNoteIndex < GameManager.instance.CurrMidiNotes.Count && GameManager.instance.CurrMidiNotes[GameManager.instance.nextNoteIndex].StartTime <= elapsedTime)
        {
            var noteData = GameManager.instance.CurrMidiNotes[GameManager.instance.nextNoteIndex];
            SpawnNote(noteData);
            GameManager.instance.nextNoteIndex++;
        }
    }

    private void SpawnNote(GameManager.Notes noteData)
    {
       
        if (!XPositions.TryGetValue(noteData.Note, out float xPos))
            return;

        // 1. Oblicz czas trwania nuty w sekundach
        float noteDurationSec = (float)(noteData.Length / 1000.0);

        // 2. Prêdkoœæ spadania (ScreenHeight / 2 na sekundê)
        float fallSpeed = KeyboardManager.instance.ScreenHeight / 2f;

        // 3. Oblicz wysokoœæ nuty
        float noteHeight = noteDurationSec * fallSpeed;

        // 4. Pobierz szerokoœæ klawisza
        var keyRect = KeyboardManager.instance.KeyVisualsDict[noteData.Note]?.GetComponent<RectTransform>();
        float noteWidth = keyRect != null ? keyRect.rect.width : 40f; // domyœlna szerokoœæ, jeœli brak

        Vector3 spawnPos = new Vector3(xPos, 1000, 0f);

        // 6. Stwórz nutê
        GameObject noteObj = Instantiate(notePrefab, spawnPos, Quaternion.identity, notesParent);
        //Debug.Log($"Spawnuje nutê: {noteData.Note} o czasie: {noteData.StartTime} is d³ugoœci : {noteData.Length}");
        //StartCoroutine(LogWhenNoteShouldBePlayed(noteData));
        noteObj.name = $"{noteData.Note} {noteData.StartTime} {noteData.Length}";
        // 7. Ustaw rozmiar nuty
        var noteRect = noteObj.GetComponent<RectTransform>();
        if (noteRect != null)
            noteRect.sizeDelta = new Vector2(noteWidth, noteHeight);

        // 8. Ustaw rozmiar kolidera (opcjonalnie, jeœli u¿ywasz BoxCollider2D)
        var collider = noteObj.GetComponent<BoxCollider2D>();
        if (collider != null && noteRect != null)
            collider.size = new Vector2(noteRect.rect.width / 2f, noteRect.rect.height);
            collider.offset = new Vector2(0f, noteHeight / 2f);

        // 9. Przeka¿ dane do prefabrykatu
        var fallingNote = noteObj.GetComponent<FallingNote>();
        if (fallingNote != null)
            fallingNote.Init(noteData.StartTime, noteData.Length, 0);
    }
    public IEnumerator LogWhenNoteShouldBePlayed(GameManager.Notes noteData)
    {
        yield return new WaitForSeconds(1.5f);
        Debug.Log($"Nuta {noteData.Note} powinna byæ zagrana teraz!");
        yield return new WaitForSeconds((float)noteData.Length/1000f);
        Debug.Log($"Nuta {noteData.Note} powinna zostaæ wypuszczona teraz!");
    }

    public void PauseResume(InputAction.CallbackContext ctx)
    {
        Debug.Log("[FalingNotesSpawner] PauseResume called. IsPaused before: " + GameManager.instance.IsPaused);
        if (GameManager.instance.IsPaused)
        {
            float pausedDuration = Time.time - pauseTime;
            GameManager.instance.songStartTime += pausedDuration;
            GameManager.instance.IsPaused = false;
            GameUIManager.instance.TurnOffPauseMenu();
        }
        else
        {
            pauseTime = Time.time;
            GameManager.instance.IsPaused = true;
            GameUIManager.instance.TurnOnPauseMenu();
        }
    }

    public void PauseEndLevel()
    {
        pauseTime = Time.time;
        GameManager.instance.IsPaused = true;
        //Debug.Log("Paused");
    }

    public void Resume()
    {
        float pausedDuration = Time.time - pauseTime;
        GameManager.instance.songStartTime += pausedDuration;
        GameManager.instance.IsPaused = false;
        GameUIManager.instance.TurnOffPauseMenu();
        //Debug.Log("unPaused");
    }

    public void Pause()
    {
        pauseTime = Time.time;
        GameManager.instance.IsPaused = true;
    }
    public void Upause()
    {
        GameManager.instance.IsPaused = false;
    }

    public void UnpauseTutorial()
    {
        StartCoroutine(UnpauseTutorialEnum());
    }

    private IEnumerator UnpauseTutorialEnum()
    {
        Resume();
        yield return new WaitForSeconds(4.5f);
        Pause();
    }
}
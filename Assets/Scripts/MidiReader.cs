using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


public class MidiReader : MonoBehaviour
{
    public static MidiReader instance;
    //public Dictionary<GameManager.NK, float> NotesPositionDict;
    public string midiPath;
    private bool midiInitialized;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        
    }

    void Start()
    {
        StartCoroutine(InitializeMidiWhenReady());
    }

    private IEnumerator InitializeMidiWhenReady()
    {
        while (GameManager.instance == null || GameManager.instance.currentSong == null || DataCollection.instance == null)
        {
            yield return null;
        }

        if (midiInitialized)
        {
            yield break;
        }

        midiPath = SetMidiPath(GameManager.instance.currentSong.FilePath);
        Debug.Log($"Midi path set to: {midiPath}");

        var loadedNotes = ReadMidiNotes(midiPath);
        if (loadedNotes == null)
        {
            GameManager.instance.CurrMidiNotes = new List<GameManager.Notes>();
            DataCollection.instance.TotalNotes = 0;
            midiInitialized = true;
            yield break;
        }

        GameManager.instance.CurrMidiNotes = CleanUpMidi(loadedNotes);
        DataCollection.instance.TotalNotes = GameManager.instance.CurrMidiNotes.Count;
        midiInitialized = true;
        //foreach (var note in GameManager.instance.CurrMidiNotes)
        //{
        //    Debug.Log($"Note: {note.Note}, StartTime: {note.StartTime}, Length: {note.Length}");
        //}
    }

    private List<GameManager.Notes> CleanUpMidi(List<GameManager.Notes> rawNotes) //MIDI NIE DZIA�A TO PEWNIE TUTAJ
    {
        const double epsilonMs = 0.5;

        // Usu� duplikaty
        var cleanNotes = rawNotes
            .GroupBy(n => new
            {
                n.Note,
                StartBucket = System.Math.Round(n.StartTime / epsilonMs),
                LengthBucket = System.Math.Round(n.Length / epsilonMs)
            })
            .Select(g => g.First())
            .ToList();

        var result = new List<GameManager.Notes>();

        // Grupuj po klawiszu
        foreach (var group in cleanNotes.GroupBy(n => n.Note))
        {
            // Sortuj po czasie rozpocz�cia
            var notes = group.OrderBy(n => n.StartTime).ToList();
            var toRemove = new HashSet<GameManager.Notes>();

            for (int i = 0; i < notes.Count; i++)
            {
                var outer = notes[i];
                double outerStart = outer.StartTime;
                double outerEnd = outer.StartTime + outer.Length;

                for (int j = 0; j < notes.Count; j++)
                {
                    if (i == j) continue;
                    var inner = notes[j];
                    double innerStart = inner.StartTime;
                    double innerEnd = inner.StartTime + inner.Length;

                    // Sprawd�, czy nuta inner jest ca�kowicie zawarta w outer i kr�tsza
                    if (innerStart >= outerStart && innerEnd <= outerEnd && inner.Length < outer.Length)
                    {
                        toRemove.Add(inner);
                    }
                }
            }

            foreach (var note in notes)
            {
                if (!toRemove.Contains(note))
                    result.Add(note);
            }
        }

        result = result.OrderBy(n => n.StartTime).ToList();
        if (result.Count == 0)
        {
            GameUIManager.instance.totalSongTime = 0;
            GameManager.instance.longestNoteLength = 0;
            return result;
        }

        GameUIManager.instance.totalSongTime = result.Last().StartTime + result.Last().Length + 1500; //ustaw czas trwania piosenki na czas zako�czenia ostatniej nuty
        GameManager.instance.longestNoteLength = (float)result.Max(n => n.Length); //ustaw d�ugo�� najd�u�szej nuty
        //Debug.Log("longest note" + GameManager.instance.longestNoteLength);
        return result;
    }

    private string SetMidiPath(string path) 
    {
        var filePath = path; // GameManager.instance.inputActions.Piano.MidiPath.ReadValue<string>();
        return filePath;
    }

    public List<GameManager.Notes> ReadMidiNotes(string midiPath)
    {
        var notesList = new List<GameManager.Notes>();
        const int c3Number = 36;
        const int c5Number = 60;
        const int twoOctaveSpan = 24;

        if (string.IsNullOrWhiteSpace(midiPath))
        {
            Debug.LogError("ReadMidiNotes failed: MIDI path is null or empty.");
            return null;
        }

        if (!File.Exists(midiPath))
        {
            Debug.LogError($"ReadMidiNotes failed: MIDI file not found at path '{midiPath}'.");
            return null;
        }

        try
        {
            // Wczytaj plik MIDI
            var midiFile = MidiFile.Read(midiPath);

            // Ustaw konwersj� czasu
            var tempoMap = midiFile.GetTempoMap();

            // Pobierz nuty z pliku
            var notes = midiFile.GetNotes().ToList();

            int transposeSemitones = 0;
            if (notes.Count > 0)
            {
                int minMidi = notes.Min(n => (int)n.NoteNumber);
                int maxMidi = notes.Max(n => (int)n.NoteNumber);
                int span = maxMidi - minMidi;

                // If the song pitch span fits in two octaves, shift all notes into C3-C5.
                if (span <= twoOctaveSpan)
                {
                    int minShift = c3Number - minMidi;
                    int maxShift = c5Number - maxMidi;

                    // Prefer octave transposition (multiples of 12 semitones) whenever possible.
                    var octaveCandidates = new List<int>();
                    for (int shift = minShift; shift <= maxShift; shift++)
                    {
                        if (shift % 12 == 0)
                        {
                            octaveCandidates.Add(shift);
                        }
                    }

                    if (octaveCandidates.Count > 0)
                    {
                        transposeSemitones = octaveCandidates
                            .OrderBy(s => System.Math.Abs(s))
                            .First();
                    }
                    else
                    {
                        // Fallback: choose the nearest non-octave shift that still fits C3-C5.
                        if (0 < minShift)
                        {
                            transposeSemitones = minShift;
                        }
                        else if (0 > maxShift)
                        {
                            transposeSemitones = maxShift;
                        }
                    }

                    if (transposeSemitones != 0)
                    {
                        Debug.Log($"ReadMidiNotes: applied transpose of {transposeSemitones} semitones to fit C3-C5 range.");
                    }
                }
                else
                {
                    Debug.LogWarning($"ReadMidiNotes: source pitch span is {span} semitones (>24), cannot fully fit C3-C5 with one transpose.");
                }
            }

            foreach (var note in notes)
            {
                int midiNumber = note.NoteNumber + transposeSemitones;
                int index = midiNumber - c3Number;
                if (index < 0 || index > (int)GameManager.NK.C5) //to do zmiany jako�
                    continue; // pomi� nuty spoza zakresu

                var nk = (GameManager.NK)index;

                // Przelicz czas na milisekundy
                double start = (double)TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, tempoMap).TotalMilliseconds + 3000.0;
                double length = (double)LengthConverter.ConvertTo<MetricTimeSpan>(note.Length, note.Time, tempoMap).TotalMilliseconds;

                notesList.Add(new GameManager.Notes
                {
                    Note = nk,
                    StartTime = start,
                    Length = length
                });
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ReadMidiNotes failed for path '{midiPath}': {ex.Message}");
            return null;
        }

        return notesList;
    }
}

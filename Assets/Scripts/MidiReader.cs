using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


public class MidiReader : MonoBehaviour
{
    public static MidiReader instance;
    public Dictionary<GameManager.NK, float> NotesPositionDict;
    public string midiPath;



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
        
    }

    void Start()
    {
        midiPath = SetMidiPath(GameManager.instance.currentSong.FilePath);
        Debug.Log($"Midi path set to: {midiPath}");
        GameManager.instance.CurrMidiNotes = ReadMidiNotes(midiPath);
        GameManager.instance.CurrMidiNotes = CleanUpMidi(GameManager.instance.CurrMidiNotes);
        //foreach (var note in GameManager.instance.CurrMidiNotes)
        //{
        //    Debug.Log($"Note: {note.Note}, StartTime: {note.StartTime}, Length: {note.Length}");
        //}
    }

    private List<GameManager.Notes> CleanUpMidi(List<GameManager.Notes> rawNotes) //MIDI NIE DZIA£A TO PEWNIE TUTAJ
    {
        // Usuñ duplikaty
        var cleanNotes = rawNotes
            .GroupBy(n => new { n.Note, n.StartTime, n.Length })
            .Select(g => g.First())
            .ToList();

        var result = new List<GameManager.Notes>();

        // Grupuj po klawiszu
        foreach (var group in cleanNotes.GroupBy(n => n.Note))
        {
            // Sortuj po czasie rozpoczêcia
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

                    // SprawdŸ, czy nuta inner jest ca³kowicie zawarta w outer i krótsza
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

        // Posortuj koñcowo po czasie rozpoczêcia
        result = result.OrderBy(n => n.StartTime).ToList();
        GameUIManager.instance.totalSongTime = result.Last().StartTime + result.Last().Length + 1500; //ustaw czas trwania piosenki na czas zakoñczenia ostatniej nuty
        GameManager.instance.longestNoteLength = (float)result.Max(n => n.Length); //ustaw d³ugoœæ najd³u¿szej nuty
        Debug.Log("longest note" + GameManager.instance.longestNoteLength);
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

        // Wczytaj plik MIDI
        var midiFile = MidiFile.Read(midiPath);

        // Ustaw konwersjê czasu
        var tempoMap = midiFile.GetTempoMap();

        // Pobierz nuty z pliku
        var notes = midiFile.GetNotes();

        foreach (var note in notes)
        {
            int midiNumber = note.NoteNumber;
            int c3Number = 36;                  //TODO dodac rozpoznawanie numeru C3
            int index = midiNumber - c3Number;
            if (index < 0 || index > (int)GameManager.NK.C5) //to do zmiany jakoœ
                continue; // pomiñ nuty spoza zakresu

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

        return notesList;
    }
}

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
    private int midiLoadRequestId;



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
        StartCoroutine(InitializeMidiWhenReady(midiLoadRequestId));
    }

    private IEnumerator InitializeMidiWhenReady(int requestId)
    {
        while (GameManager.instance == null || GameManager.instance.currentSong == null || DataCollection.instance == null)
        {
            yield return null;
        }

        if (requestId != midiLoadRequestId)
        {
            yield break;
        }

        if (midiInitialized)
        {
            yield break;
        }

        midiPath = SetMidiPath(GameManager.instance.currentSong.FilePath);
        Debug.Log($"Midi path set to: {midiPath}");

        var loadedNotes = ReadMidiNotes(midiPath, GameManager.instance.BPMmod);
        Metronome.instance.StartMetronome(GameManager.instance.CurrSongBPM);
        if (requestId != midiLoadRequestId)
        {
            yield break;
        }

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

    /// <summary>
    /// Resets internal MIDI initialization so a new currentSong can be loaded.
    /// </summary>
    public void ResetMidiForNewSong()
    {
        midiInitialized = false;
        midiLoadRequestId++;
        StartCoroutine(InitializeMidiWhenReady(midiLoadRequestId));
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
        return ResolveMidiPath(path);
    }

    private string ResolveMidiPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        var normalizedPath = path.Replace('\\', '/');
        var normalizedLower = normalizedPath.ToLowerInvariant();

        var tried = new List<string>();

        // 1) If path already exists on disk, use it.
        tried.Add(path);
        if (File.Exists(path))
        {
            Debug.Log($"ResolveMidiPath: found original path: {path}");
            return path;
        }

        // 2) If path looks like an Asset path (case-insensitive), try StreamingAssets and dataPath using the relative part.
        if (normalizedLower.StartsWith("assets/") || normalizedLower.Contains("/streamingassets/") || normalizedLower.StartsWith("streamingassets/"))
        {
            string relativePath;

            if (normalizedLower.StartsWith("assets/"))
                relativePath = normalizedPath.Substring("Assets/".Length);
            else
            {
                // find streamingassets/ index and take the remainder
                int idx = normalizedLower.IndexOf("streamingassets/");
                if (idx >= 0)
                    relativePath = normalizedPath.Substring(idx + "streamingassets/".Length);
                else
                    relativePath = normalizedPath;
            }

            // If relativePath still starts with StreamingAssets/, strip it to avoid duplication when combining
            if (relativePath.StartsWith("StreamingAssets/", System.StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Substring("StreamingAssets/".Length);
            }

            string streamingAssetsCandidate = Path.Combine(Application.streamingAssetsPath, relativePath);
            tried.Add(streamingAssetsCandidate);
            if (File.Exists(streamingAssetsCandidate))
            {
                Debug.Log($"ResolveMidiPath: found in StreamingAssets: {streamingAssetsCandidate}");
                return streamingAssetsCandidate;
            }

            string dataPathCandidate = Path.Combine(Application.dataPath, relativePath);
            tried.Add(dataPathCandidate);
            if (File.Exists(dataPathCandidate))
            {
                Debug.Log($"ResolveMidiPath: found in dataPath: {dataPathCandidate}");
                return dataPathCandidate;
            }

            // Also check Resources under Assets/Resources
            var resourcesIndex = normalizedLower.IndexOf("resources/");
            if (resourcesIndex >= 0)
            {
                string resourceRelative = normalizedPath.Substring(resourcesIndex + "resources/".Length);
                string resourcePathNoExt = Path.ChangeExtension(resourceRelative, null).Replace('/', Path.DirectorySeparatorChar).Replace('\\', '/');
                tried.Add($"Resources:{resourcePathNoExt}");
                var ta = Resources.Load<TextAsset>(resourcePathNoExt);
                if (ta != null)
                {
                    // write to persistent and return
                    string outPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(resourceRelative));
                    File.WriteAllBytes(outPath, ta.bytes);
                    Debug.Log($"ResolveMidiPath: extracted TextAsset from Resources to: {outPath}");
                    return outPath;
                }
            }
        }

        // 3) Try appending the provided path to StreamingAssets and data paths in case stored path was relative.
        string streamingCandidate2 = Path.Combine(Application.streamingAssetsPath, normalizedPath);
        tried.Add(streamingCandidate2);
        if (File.Exists(streamingCandidate2))
        {
            Debug.Log($"ResolveMidiPath: found in StreamingAssets (alt): {streamingCandidate2}");
            return streamingCandidate2;
        }

        string dataCandidate2 = Path.Combine(Application.dataPath, normalizedPath);
        tried.Add(dataCandidate2);
        if (File.Exists(dataCandidate2))
        {
            Debug.Log($"ResolveMidiPath: found in dataPath (alt): {dataCandidate2}");
            return dataCandidate2;
        }

        // 4) Check persistentDataPath by filename
        string persistentCandidate = Path.Combine(Application.persistentDataPath, Path.GetFileName(path));
        tried.Add(persistentCandidate);
        if (File.Exists(persistentCandidate))
        {
            Debug.Log($"ResolveMidiPath: found in persistentDataPath: {persistentCandidate}");
            return persistentCandidate;
        }

        // 5) Fallback: search StreamingAssets recursively for the same filename (helps when folder structure differs)
        try
        {
            string filename = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(filename) && Directory.Exists(Application.streamingAssetsPath))
            {
                var matches = Directory.GetFiles(Application.streamingAssetsPath, filename, System.IO.SearchOption.AllDirectories);
                if (matches != null && matches.Length > 0)
                {
                    tried.Add(matches[0]);
                    Debug.Log($"ResolveMidiPath: found by filename search in StreamingAssets: {matches[0]}");
                    return matches[0];
                }
            }

            // Also search persistentDataPath
            if (!string.IsNullOrEmpty(filename) && Directory.Exists(Application.persistentDataPath))
            {
                var pmatches = Directory.GetFiles(Application.persistentDataPath, filename, System.IO.SearchOption.AllDirectories);
                if (pmatches != null && pmatches.Length > 0)
                {
                    tried.Add(pmatches[0]);
                    Debug.Log($"ResolveMidiPath: found by filename search in persistentDataPath: {pmatches[0]}");
                    return pmatches[0];
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"ResolveMidiPath: error during filename search fallback: {ex.Message}");
        }

        Debug.LogWarning($"ResolveMidiPath: could not resolve path '{path}'. Tried: {string.Join(", ", tried)}");
        return path;
    }

    public List<GameManager.Notes> ReadMidiNotes(string midiPath, double BPMmult = 1.0)
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

                start /= BPMmult;
                length /= BPMmult;

                notesList.Add(new GameManager.Notes
                {
                    Note = nk,
                    StartTime = start,
                    Length = length
                });
            }

            var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(0));
            GameManager.instance.CurrSongBPM = (int)(tempo.BeatsPerMinute * BPMmult);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ReadMidiNotes failed for path '{midiPath}': {ex.Message}");
            return null;
        }

        return notesList;
    }
}

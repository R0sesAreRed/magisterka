using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;

public class NotesTest : MonoBehaviour
{
    public float duration = 1f;
    public float volume = 0.5f;

    public AudioClip toneClip;

    [SerializeField] private AudioSource[] ASs; //audiosources

    private Dictionary<GameManager.NK, AudioSource> ASsDict; //audioSourcesDict
    private Dictionary<GameManager.NK, AudioClip> noteClips;


    private readonly Dictionary<GameManager.NK, float> noteFreq = new()
    {
        { GameManager.NK.C3, 130.81f }, { GameManager.NK.CS3, 138.59f }, { GameManager.NK.D3, 146.83f }, { GameManager.NK.DS3, 155.56f },
        { GameManager.NK.E3, 164.81f }, { GameManager.NK.F3, 174.61f }, { GameManager.NK.FS3, 185.00f }, { GameManager.NK.G3, 196.00f },
        { GameManager.NK.GS3, 207.65f }, { GameManager.NK.A3, 220.00f }, { GameManager.NK.AS3, 233.08f }, { GameManager.NK.B3, 246.94f },
        { GameManager.NK.C4, 261.63f }, { GameManager.NK.CS4, 277.18f }, { GameManager.NK.D4, 293.66f }, { GameManager.NK.DS4, 311.13f },
        { GameManager.NK.E4, 329.63f }, { GameManager.NK.F4, 349.23f }, { GameManager.NK.FS4, 369.99f }, { GameManager.NK.G4, 392.00f },
        { GameManager.NK.GS4, 415.30f }, { GameManager.NK.A4, 440.00f }, { GameManager.NK.AS4, 466.16f }, { GameManager.NK.B4, 493.88f },
        { GameManager.NK.C5, 523.25f }
    };

    void Start()
    {
        //AudioClip clip = CreateTone(frequency, duration, volume);
        //AudioSource.PlayClipAtPoint(clip, Vector3.zero);
    }

    private void Awake()
    {
        ASsDict = new Dictionary<GameManager.NK, AudioSource>();
        noteClips = new Dictionary<GameManager.NK, AudioClip>();
        for (int i = 0; i < ASs.Length && i < Enum.GetValues(typeof(GameManager.NK)).Length; i++)
        {
            var note = (GameManager.NK)i;
            ASsDict[note] = ASs[i];
            noteClips[note] = CreateTone(noteFreq[note], 5f, volume); //tworzenie klipów ¿eby mozna by³o spamowac klawiature
        }
    }


    private Dictionary<GameManager.NK, Action<InputAction.CallbackContext>> performedDelegates;
    private Dictionary<GameManager.NK, Action<InputAction.CallbackContext>> canceledDelegates;

    private void OnEnable() //przypisuje funkcje
    {
        GameManager.instance.inputActions.Piano.Enable();
        performedDelegates = new();
        canceledDelegates = new();

        foreach (GameManager.NK note in Enum.GetValues(typeof(GameManager.NK)))
        {
            performedDelegates[note] = ctx => OnNotePerformed(note, ctx);
            canceledDelegates[note] = ctx => OnNoteCanceled(note, ctx);

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

    private void OnNotePerformed(GameManager.NK note, InputAction.CallbackContext ctx) //rozpoczyna granie nuty przy wciœniêciu klawisza do przypiêcia
    {
        PlayNote(ctx, note, noteFreq[note]);
    }
    private void OnNoteCanceled(GameManager.NK note, InputAction.CallbackContext ctx) //zatrzymuje gran¹ nutê przy wypuszczeniu klawisza do przypiêcia
    {
        StopNote(ctx, note);
    }

    private void PlayNote(InputAction.CallbackContext context, GameManager.NK note, float freq)
    {
        //Debug.Log("Playing Note: " + note);
        if (ASsDict[note] == null)
        {
            ASsDict[note] = gameObject.AddComponent<AudioSource>();
            ASsDict[note].loop = true;
        }
        ASsDict[note].clip = noteClips[note]; // u¿yj gotowego klipu
        ASsDict[note].Play();
    }

    private void StopNote(InputAction.CallbackContext context, GameManager.NK note) //zatrzymuje gran¹ nutê przy wypuszczeniu klawisza
    {
        if (ASsDict[note] != null)
        {
            ASsDict[note].Stop();
        }
    }

    AudioClip CreateTone(float freq, float lengthSec, float vol)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int sampleCount = (int)(sampleRate * lengthSec);
        int channels = 1; // stereo
        float[] samples = new float[sampleCount * channels];

        for (int i = 0; i < sampleCount; i++)
        {
            float sample = vol * Mathf.Sin(2 * Mathf.PI * freq * i / sampleRate);
            for (int c = 0; c < channels; c++)
            {
                samples[i * channels + c] = sample;
            }
        }

        AudioClip clip = AudioClip.Create("Tone", sampleCount, channels, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    } //placeholder
}

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
             
        }
        if(GameManager.instance.noteClipsCopy.Count == 0)
        {
            for (int i = 0; i < ASs.Length && i < Enum.GetValues(typeof(GameManager.NK)).Length; i++)
            {
                var note = (GameManager.NK)i;
                noteClips[note] = CreatePianoTone(noteFreq[note], 5f, volume);//tworzenie klip�w �eby mozna by�o spamowac klawiature
            }
            GameManager.instance.noteClipsCopy = noteClips;
        }
        else
        {
            noteClips = GameManager.instance.noteClipsCopy;
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

    private void OnDisable() //odpiananie funkcji przy wy��czeniu skryptu
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

    private void OnNotePerformed(GameManager.NK note, InputAction.CallbackContext ctx) //rozpoczyna granie nuty przy wci�ni�ciu klawisza do przypi�cia
    {
        if(GameManager.instance.gamificationOn) //setting od rodzaju graia nut
        {
            StartCoroutine(PlayNoteWhenRight(ctx, note, noteFreq[note]));
        }
        else
            PlayNote(ctx, note, noteFreq[note]);
    }
    private void OnNoteCanceled(GameManager.NK note, InputAction.CallbackContext ctx) //zatrzymuje gran� nut� przy wypuszczeniu klawisza do przypi�cia
    {
        if (GameManager.instance.gamificationOn) //setting od rodzaju graia nut
        {
            StopCoroutine(PlayNoteWhenRight(ctx, note, noteFreq[note]));
        }

        StopNote(ctx, note);
    }

    private void PlayNote(InputAction.CallbackContext context, GameManager.NK note, float freq)
    {
        //Debug.Log("Playing Note: " + note);
        if (ASsDict[note] == null)
        {
            ASsDict[note].loop = true;
        }
        ASsDict[note].clip = noteClips[note];
        ASsDict[note].Play();
    }

    private IEnumerator PlayNoteWhenRight(InputAction.CallbackContext context, GameManager.NK note, float freq)
    {

        while (!KeyboardManager.instance.hasActiveCollision[note])
        {
            yield return new WaitForSeconds(0.03f);
        }
        if (ASsDict[note] == null)
        {
            ASsDict[note] = gameObject.AddComponent<AudioSource>();
            ASsDict[note].loop = true;
        }

        ASsDict[note].clip = noteClips[note];
        ASsDict[note].Play();
        while (KeyboardManager.instance.hasActiveCollision[note])
        {
            yield return new WaitForSeconds(0.03f);
        }
        StopNote(context, note);
    }

    private void StopNote(InputAction.CallbackContext context, GameManager.NK note) //zatrzymuje gran� nut� przy wypuszczeniu klawisza
    {
        if (ASsDict[note] != null)
        {
            ASsDict[note].Stop();
        }
    }

    public static AudioClip CreatePianoTone(
        float freq,
        float lengthSec,
        float volume = 0.7f,
        float velocity = 1.0f)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int sampleCount = Mathf.CeilToInt(sampleRate * lengthSec);
        int channels = 1;

        float[] samples = new float[sampleCount * channels];

        velocity = Mathf.Clamp01(velocity);
        volume = Mathf.Clamp01(volume);

        // Piano strings are slightly inharmonic:
        // upper partials are slightly sharper than exact integer multiples.
        float inharmonicity = GetInharmonicity(freq);

        // Harmonic amplitudes. These define the basic "timbre".
        // More low harmonics = warmer sound.
        // More high harmonics = brighter, more percussive sound.
        float[] harmonicAmplitudes =
        {
            1.000f,  // 1st harmonic, fundamental
            0.720f,  // 2nd
            0.520f,  // 3rd
            0.340f,  // 4th
            0.240f,  // 5th
            0.170f,  // 6th
            0.120f,  // 7th
            0.090f,  // 8th
            0.065f,  // 9th
            0.048f,  // 10th
            0.036f,  // 11th
            0.026f,  // 12th
            0.018f,  // 13th
            0.013f,  // 14th
            0.009f,  // 15th
            0.006f   // 16th
        };

        float maxAbs = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            float sample = 0f;

            for (int h = 1; h <= harmonicAmplitudes.Length; h++)
            {
                float harmonicFrequency = GetInharmonicPartial(freq, h, inharmonicity);

                // Avoid frequencies above Nyquist.
                if (harmonicFrequency >= sampleRate * 0.5f)
                    continue;

                float harmonicAmp = harmonicAmplitudes[h - 1];

                // Higher harmonics decay faster, which is very important for piano-like sound.
                float decayRate = GetHarmonicDecayRate(freq, h);

                float envelope = GetPianoPartialEnvelope(t, lengthSec, decayRate, h);

                // Slight velocity brightness: harder notes contain stronger upper partials.
                float brightnessBoost = Mathf.Lerp(0.65f, 1.35f, velocity);
                float harmonicBrightness = Mathf.Pow(brightnessBoost, Mathf.InverseLerp(1, harmonicAmplitudes.Length, h));

                sample += harmonicAmp * harmonicBrightness * envelope *
                          Mathf.Sin(2f * Mathf.PI * harmonicFrequency * t);
            }

            // Add short sine-based hammer transient.
            // This is not noise; it is a cluster of fast-decaying high sine tones.
            sample += GetHammerTransient(t, freq, velocity);

            // Add subtle doubled-string beating.
            sample += GetStringBeating(t, freq, lengthSec, velocity);

            sample *= volume;

            samples[i] = sample;

            float abs = Mathf.Abs(sample);
            if (abs > maxAbs)
                maxAbs = abs;
        }

        // Normalize to avoid clipping.
        if (maxAbs > 1f)
        {
            float normalize = 0.95f / maxAbs;

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= normalize;
            }
        }

        AudioClip clip = AudioClip.Create(
            "GeneratedPiano_" + freq.ToString("F1"),
            sampleCount,
            channels,
            sampleRate,
            false
        );

        clip.SetData(samples, 0);
        return clip;
    }

    private static float GetInharmonicity(float freq)
    {
        // Approximation:
        // lower notes have more audible inharmonicity,
        // higher notes are cleaner.
        if (freq < 120f)
            return 0.0009f;

        if (freq < 300f)
            return 0.00045f;

        if (freq < 800f)
            return 0.00022f;

        return 0.00010f;
    }

    private static float GetInharmonicPartial(float fundamental, int harmonic, float inharmonicity)
    {
        // Piano partial approximation:
        // f_n = f_0 * n * sqrt(1 + B * n^2)
        return fundamental * harmonic * Mathf.Sqrt(1f + inharmonicity * harmonic * harmonic);
    }

    private static float GetHarmonicDecayRate(float freq, int harmonic)
    {
        // Higher notes decay faster than low notes.
        float pitchDecay = Mathf.InverseLerp(80f, 1200f, freq);

        float baseDecay = Mathf.Lerp(0.75f, 2.2f, pitchDecay);

        // Higher harmonics decay much faster.
        float harmonicDecay = Mathf.Pow(harmonic, 1.25f) * 0.45f;

        return baseDecay + harmonicDecay;
    }

    private static float GetPianoPartialEnvelope(float t, float lengthSec, float decayRate, int harmonic)
    {
        // Very fast attack.
        float attack = Mathf.Lerp(0.0025f, 0.0008f, Mathf.InverseLerp(1, 16, harmonic));

        float attackEnvelope;

        if (t < attack)
        {
            attackEnvelope = t / attack;
        }
        else
        {
            attackEnvelope = 1f;
        }

        // Exponential decay is more piano-like than linear ADSR.
        float decayEnvelope = Mathf.Exp(-decayRate * t);

        // Final fade prevents clipping/clicking at the end of the generated clip.
        float releaseLength = Mathf.Min(0.12f, lengthSec * 0.25f);
        float releaseStart = Mathf.Max(0f, lengthSec - releaseLength);

        float releaseEnvelope = 1f;

        if (t > releaseStart)
        {
            float releaseT = Mathf.InverseLerp(lengthSec, releaseStart, t);
            releaseEnvelope = Mathf.SmoothStep(0f, 1f, releaseT);
        }

        return attackEnvelope * decayEnvelope * releaseEnvelope;
    }

    private static float GetHammerTransient(float t, float freq, float velocity)
    {
        // Very short bright impact made only from sine waves.
        float env = Mathf.Exp(-t * 90f);

        float transient = 0f;

        transient += 0.035f * Mathf.Sin(2f * Mathf.PI * freq * 7.1f * t);
        transient += 0.025f * Mathf.Sin(2f * Mathf.PI * freq * 11.3f * t);
        transient += 0.018f * Mathf.Sin(2f * Mathf.PI * freq * 15.7f * t);
        transient += 0.012f * Mathf.Sin(2f * Mathf.PI * freq * 21.2f * t);

        return transient * env * velocity;
    }

    private static float GetStringBeating(float t, float freq, float lengthSec, float velocity)
    {
        // Real piano notes usually have multiple strings per note.
        // Slight detuning creates slow beating/richness.
        float env = Mathf.Exp(-1.25f * t);

        float beating = 0f;

        beating += 0.055f * Mathf.Sin(2f * Mathf.PI * freq * 0.9975f * t);
        beating += 0.045f * Mathf.Sin(2f * Mathf.PI * freq * 1.0028f * t);

        return beating * env * Mathf.Lerp(0.6f, 1.0f, velocity);
    }

}

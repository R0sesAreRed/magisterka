using UnityEngine;

public class Metronome : MonoBehaviour
{
    public static Metronome instance;
    [SerializeField] private int bpm = 120;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip tickClip;
    private bool isRunning = false;
    private double nextTickTime;
    private double secondsPerBeat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!isRunning)
            return;

        double currentTime = AudioSettings.dspTime;

        if (currentTime >= nextTickTime)
        {
            Tick();

            // Schedule the next beat using audio-clock time, not frame time
            nextTickTime += secondsPerBeat;

            // Safety catch-up in case of a large frame hitch
            while (nextTickTime <= currentTime)
            {
                nextTickTime += secondsPerBeat;
            }
        }
    }


    public void StartMetronome(int newBpm)
    {
        bpm = Mathf.Max(1, newBpm);

        secondsPerBeat = 60.0 / bpm;
        nextTickTime = AudioSettings.dspTime;

        isRunning = true;
    }

    public void StopMetronome()
    {
        isRunning = false;
    }

    public void ResumeMetronome()
    {
        nextTickTime = AudioSettings.dspTime + secondsPerBeat;
        isRunning = true;
    }
    public void SetBpm(int newBpm)
    {
        bpm = Mathf.Max(1, newBpm);
        secondsPerBeat = 60.0 / bpm;
    }

    private void Tick()
    {
        if (audioSource != null && tickClip != null)
        {
            audioSource.PlayOneShot(tickClip);
        }
    }
}

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

    private double scaledDspTime;
    private double lastRealDspTime;

    public float tempoScale = 1f;
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


    private void Start()
    {
        lastRealDspTime = AudioSettings.dspTime;
    }

    private void Update()
    {
        UpdateScaledDspTime();

        if (!isRunning)
            return;

        double currentTime = AudioSettings.dspTime;

        if (scaledDspTime >= nextTickTime)
        {
            Tick();

            nextTickTime += secondsPerBeat;

            while (nextTickTime <= scaledDspTime)
            {
                nextTickTime += secondsPerBeat;
            }
        }
    }

    private void UpdateScaledDspTime()
    {
        double realDspTime = AudioSettings.dspTime;
        double realDelta = realDspTime - lastRealDspTime;

        lastRealDspTime = realDspTime;

        scaledDspTime += realDelta * tempoScale;
    }

    public void StartMetronome(int newBpm)
    {
        bpm = Mathf.Max(1, newBpm);
        secondsPerBeat = 60.0 / bpm;

        scaledDspTime = 0.0;
        lastRealDspTime = AudioSettings.dspTime;

        nextTickTime = scaledDspTime;

        isRunning = true;
    }

    public void StopMetronome()
    {
        isRunning = false;
    }


    public void ResumeMetronome()
    {
        lastRealDspTime = AudioSettings.dspTime;
        nextTickTime = scaledDspTime + secondsPerBeat;
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

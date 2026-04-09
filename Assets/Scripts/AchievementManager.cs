using System;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public static class GameEvents
{
    public static Action<int> OnPerfectHit;
    public static Action<float> OnReachScore;
    public static Action<int> OnRegainLifeLife;
    public static Action<int> OnCompleteLevel;
}

// przejœcie poziomów 1, 3, 5, 10, 20, wszystkie

// perfect hit
// ci¹g trafieñ 10, 50, 100, 200
// ci¹g perfect hitów 5, 10, 25, 50
// odzyskane hp
// complete level:
    // perfect hity 100%, 75%, 50%
    // bez straty hp

//kupowanie itemów:
    //kupienie pierwaszego itemu
    //kupienie ca³ego zestawu itemów
    //wykupienie sklepu


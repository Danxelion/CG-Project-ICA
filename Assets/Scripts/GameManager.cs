using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public enum GameMode { WarmUp, Survival }
    public enum GameState { Playing, GameOver }

    [Header("Mode Configuration")]
    public GameMode currentMode = GameMode.WarmUp;
    public float warmUpDuration = 60f; // 1-minute Warm-up timer

    // Current runtime state
    public GameState CurrentState { get; private set; } = GameState.Playing;
    public float CurrentTimer { get; private set; }
    public int KillCount { get; private set; }

    public event Action<float> OnTimerUpdated;
    public event Action<int> OnKillCountUpdated;
    public event Action<GameState> OnGameStateChanged;

    void Start()
    {
        StartMode(currentMode);
    }

    public void StartMode(GameMode mode)
    {
        currentMode = mode;
        CurrentState = GameState.Playing;
        KillCount = 0;

        // Warm-up counts down from 60s; Survival counts up from 0s
        CurrentTimer = (currentMode == GameMode.WarmUp) ? warmUpDuration : 0f;

        OnTimerUpdated?.Invoke(CurrentTimer);
        OnKillCountUpdated?.Invoke(KillCount);
        OnGameStateChanged?.Invoke(CurrentState);
    }

    void Update()
    {
        if (CurrentState != GameState.Playing) return;

        if (currentMode == GameMode.WarmUp)
        {
            CurrentTimer -= Time.deltaTime;
            if (CurrentTimer <= 0f)
            {
                CurrentTimer = 0f;
                EndGame();
            }
        }
        else // Survival Mode
        {
            CurrentTimer += Time.deltaTime;
        }

        OnTimerUpdated?.Invoke(CurrentTimer);
    }

    public void RegisterEnemyKill()
    {
        if (CurrentState != GameState.Playing) return;
        KillCount++;
        OnKillCountUpdated?.Invoke(KillCount);
    }

    public void EndGame()
    {
        if (CurrentState == GameState.GameOver) return;
        CurrentState = GameState.GameOver;
        OnGameStateChanged?.Invoke(CurrentState);
        Debug.Log($"Game Over! Mode: {currentMode} | Kills: {KillCount}");
    }
}

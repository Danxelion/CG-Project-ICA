using UnityEngine;
using TMPro;

public class MatchHUD : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;

    [Header("Top HUD")]
    public TMP_Text timerText;
    public TMP_Text killCountText;

    [Header("Game Over / Mode Selection")]
    public GameObject gameOverPanel;
    public TMP_Text resultText;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (gameManager != null)
        {
            gameManager.OnTimerUpdated += UpdateTimerUI;
            gameManager.OnKillCountUpdated += UpdateKillsUI;
            gameManager.OnGameStateChanged += HandleGameStateChanged;

            // Sync initial state on frame 1
            UpdateTimerUI(gameManager.CurrentTimer);
            UpdateKillsUI(gameManager.KillCount);
            HandleGameStateChanged(gameManager.CurrentState);
        }
    }

    private void UpdateTimerUI(float timeSeconds)
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timeSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeSeconds % 60f);
        timerText.text = $"{gameManager.currentMode}: {minutes:00}:{seconds:00}";
    }

    private void UpdateKillsUI(int kills)
    {
        if (killCountText != null)
        {
            killCountText.text = $"Kills: {kills}";
        }
    }

    private void HandleGameStateChanged(GameManager.GameState state)
    {
        bool isEnded = (state == GameManager.GameState.Playing);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(isEnded);
        }

        if (isEnded && resultText != null)
        {
            string title = (state == GameManager.GameState.Victory) ? "WARM-UP CLEARED!" : "YOU DIED";
            resultText.text = $"MODE: {gameManager.currentMode}\nKILLS: {gameManager.KillCount}";
        }
    }

    public void OnSelectWarmUpMode()
    {
        if (gameManager != null)
        {
            gameManager.StartMode(GameManager.GameMode.WarmUp);
        }
    }

    public void OnSelectSurvivalMode()
    {
        if (gameManager != null)
        {
            gameManager.StartMode(GameManager.GameMode.Survival);
        }
    }

    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnTimerUpdated -= UpdateTimerUI;
            gameManager.OnKillCountUpdated -= UpdateKillsUI;
            gameManager.OnGameStateChanged -= HandleGameStateChanged;
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public Transform player;
    public GameObject meleeEnemyPrefab;

    [Header("Horde Settings")]
    public float spawnRadius = 12f;          // Distance from player to spawn
    public float arenaHalfExtent = 18f;      // Half-width of playable floor (keeps spawns inside walls)
    public int maxActiveEnemies = 10;        // Caps horde size to maintain FPS

    [Header("Wave Pacing")]
    public float initialSpawnInterval = 1.5f;
    public float minSpawnInterval = 0.35f;   // Fastest spawn rate in Survival
    public float difficultyRampRate = 0.02f; // Seconds shaved off interval per second survived

    private float spawnTimer;
    private float elapsedSurvivalTime;
    private readonly List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Playing)
        {
            ClearAllEnemies();
            spawnTimer = 0.5f; // First enemy spawns quickly
            elapsedSurvivalTime = 0f;
        }
    }

    void Update()
    {
        if (gameManager == null || gameManager.CurrentState != GameManager.GameState.Playing) return;
        if (player == null || meleeEnemyPrefab == null) return;

        // Clean out destroyed enemy references
        activeEnemies.RemoveAll(enemy => enemy == null);

        elapsedSurvivalTime += Time.deltaTime;
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f && activeEnemies.Count < maxActiveEnemies)
        {
            SpawnEnemyAroundPlayer();

            // Warm-Up keeps a steady pace; Survival escalates 
            float currentInterval = (gameManager.currentMode == GameManager.GameMode.WarmUp)
                ? initialSpawnInterval
                : Mathf.Max(minSpawnInterval, initialSpawnInterval - elapsedSurvivalTime * difficultyRampRate);

            spawnTimer = currentInterval;
        }
    }

    private void SpawnEnemyAroundPlayer()
    {
        // Random angle in radians [0, 2*PI]
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spawnRadius;
        Vector3 spawnPos = player.position + offset;

        // Clamp inside arena boundaries so enemies never spawn outside walls
        spawnPos.x = Mathf.Clamp(spawnPos.x, -arenaHalfExtent, arenaHalfExtent);
        spawnPos.z = Mathf.Clamp(spawnPos.z, -arenaHalfExtent, arenaHalfExtent);
        spawnPos.y = player.position.y;

        GameObject enemy = Instantiate(meleeEnemyPrefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(enemy);
    }

    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        activeEnemies.Clear();
    }

    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= HandleGameStateChanged;
        }
    }
}

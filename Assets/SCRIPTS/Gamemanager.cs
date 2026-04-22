using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene di transizione")]
    [Tooltip("Nome della scena da caricare alla vittoria")]
    public string winSceneName = "WinScene";
    [Tooltip("Nome della scena da caricare alla sconfitta")]
    public string loseSceneName = "LoseScene";

    [Header("Debug")]
    public bool logDebug = true;

    private int _totalGhosts;
    private int _killedGhosts;
    private bool _gameOver;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Conta tutti i GhostAI presenti in scena all'avvio
        _totalGhosts = FindObjectsByType<GhostAI>(FindObjectsSortMode.None).Length;
        Log($"Fantasmi totali in scena: {_totalGhosts}");
    }

    void OnEnable()
    {
        GameEvents.OnGhostKilled += HandleGhostKilled;
    }

    void OnDisable()
    {
        GameEvents.OnGhostKilled -= HandleGhostKilled;
    }

    // -------------------------------------------------------
    // Chiamato da GameEvents ogni volta che un fantasma muore
    // -------------------------------------------------------
    private void HandleGhostKilled()
    {
        if (_gameOver) return;

        _killedGhosts++;
        Log($"Fantasmi uccisi: {_killedGhosts}/{_totalGhosts}");

        if (_killedGhosts >= _totalGhosts)
            TriggerWin();
    }

    // -------------------------------------------------------
    // Collega questo metodo a WorldClock.onLevelComplete
    // dall'Inspector di WorldClock
    // -------------------------------------------------------
    public void HandleTimerExpired()
    {
        if (_gameOver) return;

        // Se ha già ucciso tutti prima dello scadere, non fa nulla
        // (TriggerWin è già stato chiamato da HandleGhostKilled)
        if (_killedGhosts >= _totalGhosts) return;

        TriggerLose();
    }

    // -------------------------------------------------------
    // Win / Lose
    // -------------------------------------------------------
    private void TriggerWin()
    {
        _gameOver = true;
        GameEvents.PlayerWin();
        Log("VITTORIA! Carico scena tra 10 secondi: " + winSceneName);
        StartCoroutine(LoadSceneDelayed(winSceneName, 5f));
    }

    private void TriggerLose()
    {
        _gameOver = true;
        GameEvents.PlayerLose();
        Log("SCONFITTA - tempo scaduto. Carico scena: " + loseSceneName);
        StartCoroutine(LoadSceneDelayed(loseSceneName, 0f));
    }

    private IEnumerator LoadSceneDelayed(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    // -------------------------------------------------------
    // Utility
    // -------------------------------------------------------
    private void Log(string msg)
    {
        if (logDebug)
            Debug.Log($"[GameManager] {msg}");
    }

    // Espone lo stato corrente per eventuali UI
    public int TotalGhosts  => _totalGhosts;
    public int KilledGhosts => _killedGhosts;
    public bool IsGameOver  => _gameOver;
}
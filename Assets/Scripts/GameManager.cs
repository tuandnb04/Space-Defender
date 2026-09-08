using SpaceDefender;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private const string HighScoreKey = "SPACE_DEFENDER_HIGHSCORE";
    private static GameManager _instance;

    [Header("State")] public bool showMainMenuOnStart = true;

    public static GameManager Instance
    {
        get
        {
            if (!_instance) _instance = FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
            return _instance;
        }
        private set => _instance = value;
    }

    public bool IsGameStarted { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }

    private int Score { get; set; }
    private int HighScore { get; set; }
    private bool IsNewHighScore { get; set; }

    private void Awake()
    {
        Application.runInBackground = true;
        Instance = this;
        Score = 0;
        IsGameOver = false;
        IsPaused = false;
        Time.timeScale = 1f;

        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        IsNewHighScore = false;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (showMainMenuOnStart)
        {
            IsGameStarted = false;
            if (UIManager.Instance != null) UIManager.Instance.ShowMainMenu();
        }
        else
        {
            StartGame();
        }
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
#endif

        // Start game with Enter or Space if on Main Menu
        if (!IsGameStarted)
        {
            var startPressed = keyboard != null &&
                               (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame);
#if ENABLE_INPUT_SYSTEM
#else
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    startPressed = true;
                }
#endif
            if (startPressed)
            {
                StartGame();
                return;
            }
        }

        // Toggle Pause with Escape or P key
        if (IsGameStarted && !IsGameOver)
        {
            var pausePressed = keyboard != null &&
                               (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame);
#if ENABLE_INPUT_SYSTEM
#else
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                {
                    pausePressed = true;
                }
#endif
            if (pausePressed) TogglePause();
        }

        // Quick restart when Game Over
        if (!IsGameOver) return;
        var restartPressed = keyboard != null && (keyboard.rKey.wasPressedThisFrame ||
                                                  keyboard.enterKey.wasPressedThisFrame ||
                                                  keyboard.spaceKey.wasPressedThisFrame);
#if ENABLE_INPUT_SYSTEM
#else
                if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    restartPressed = true;
                }
#endif
        if (restartPressed) RestartGame();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void StartGame()
    {
        IsGameStarted = true;
        IsGameOver = false;
        IsPaused = false;
        Score = 0;
        IsNewHighScore = false;
        Time.timeScale = 1f;

        if (UIManager.Instance)
        {
            UIManager.Instance.ShowInGameHUD();
            UIManager.Instance.UpdateScore(Score, HighScore);
            UIManager.Instance.UpdateLives(3);
        }

        if (ComboManager.Instance) ComboManager.Instance.ResetCombo();

        // Find player and make active/reset
        var player = FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
        if (player)
        {
            player.gameObject.SetActive(true);
            player.ResetPlayer();
        }

        // Activate spawner and start wave 1
        var spawner = FindAnyObjectByType<EnemySpawner>(FindObjectsInactive.Include);
        if (!spawner) return;
        spawner.gameObject.SetActive(true);
        spawner.StartWaveSequence();
    }

    public void AwardWaveBonus(int wave, int bonus)
    {
        AddScore(bonus);
        var player = PlayerController.Instance;
        if (!player || !player.floatingScorePrefab) return;
        var p = player.transform.position + Vector3.up * 1.2f;
        FloatingScore.SpawnText(player.floatingScorePrefab, p, $"WAVE {wave} CLEAR! +{bonus}", new Color(0.2f, 1f, 0.4f));
    }

    public void AddScore(int amount)
    {
        if (IsGameOver || !IsGameStarted) return;

        Score += amount;
        if (Score > HighScore)
        {
            HighScore = Score;
            IsNewHighScore = true;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }

        if (Score >= 200 && AchievementManager.Instance) AchievementManager.Instance.UnlockAchievement("SCORE_200");

        if (UIManager.Instance) UIManager.Instance.UpdateScore(Score, HighScore);
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayGameOver();

        if (UIManager.Instance != null) UIManager.Instance.ShowGameOver(Score, HighScore, IsNewHighScore);
    }

    public void TogglePause()
    {
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void PauseGame()
    {
        if (IsGameOver || !IsGameStarted) return;

        IsPaused = true;
        Time.timeScale = 0f;

        if (UIManager.Instance) UIManager.Instance.ShowPausePanel(true);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (UIManager.Instance) UIManager.Instance.ShowPausePanel(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        IsGameOver = false;
        IsPaused = false;
        Score = 0;
        IsNewHighScore = false;
        StartGame();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        IsGameStarted = false;
        IsGameOver = false;
        IsPaused = false;

        // Clear active enemies
        var spawner = FindAnyObjectByType<EnemySpawner>(FindObjectsInactive.Include);
        if (spawner) spawner.ClearAllEnemies();

        var player = FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
        if (player) player.gameObject.SetActive(false);

        if (ComboManager.Instance) ComboManager.Instance.ResetCombo();

        if (UIManager.Instance) UIManager.Instance.ShowMainMenu();
    }

    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey(HighScoreKey);
        HighScore = 0;
        if (UIManager.Instance) UIManager.Instance.UpdateScore(Score, HighScore);
    }
}
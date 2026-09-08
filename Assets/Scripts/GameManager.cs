using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceDefender
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public const string HighScoreKey = "SPACE_DEFENDER_HIGHSCORE";

        [Header("State")]
        public bool showMainMenuOnStart = true;
        public bool IsGameStarted { get; private set; } = false;
        public bool IsGameOver { get; private set; } = false;
        public bool IsPaused { get; private set; } = false;

        public int Score { get; private set; } = 0;
        public int HighScore { get; private set; } = 0;
        public bool IsNewHighScore { get; private set; } = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

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
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMainMenu();
                }
            }
            else
            {
                StartGame();
            }
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
#endif

            // Start game with Enter or Space if on Main Menu
            if (!IsGameStarted)
            {
                bool startPressed = false;
#if ENABLE_INPUT_SYSTEM
                if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
                {
                    startPressed = true;
                }
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
                bool pausePressed = false;
#if ENABLE_INPUT_SYSTEM
                if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame))
                {
                    pausePressed = true;
                }
#else
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                {
                    pausePressed = true;
                }
#endif
                if (pausePressed)
                {
                    TogglePause();
                }
            }

            // Quick restart when Game Over
            if (IsGameOver)
            {
                bool restartPressed = false;
#if ENABLE_INPUT_SYSTEM
                if (keyboard != null && (keyboard.rKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
                {
                    restartPressed = true;
                }
#else
                if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    restartPressed = true;
                }
#endif
                if (restartPressed)
                {
                    RestartGame();
                }
            }
        }

        public void StartGame()
        {
            IsGameStarted = true;
            IsGameOver = false;
            IsPaused = false;
            Score = 0;
            IsNewHighScore = false;
            Time.timeScale = 1f;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowInGameHUD();
                UIManager.Instance.UpdateScore(Score, HighScore);
                UIManager.Instance.UpdateLives(3);
            }

            // Find player and make active/reset
            PlayerController player = FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
            if (player != null)
            {
                player.gameObject.SetActive(true);
                player.ResetPlayer();
            }

            // Activate spawner
            EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>(FindObjectsInactive.Include);
            if (spawner != null)
            {
                spawner.gameObject.SetActive(true);
                spawner.ClearAllEnemies();
            }
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

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateScore(Score, HighScore);
            }
        }

        public void GameOver()
        {
            if (IsGameOver) return;

            IsGameOver = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameOver();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGameOver(Score, HighScore, IsNewHighScore);
            }
        }

        public void TogglePause()
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            if (IsGameOver || !IsGameStarted) return;

            IsPaused = true;
            Time.timeScale = 0f;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPausePanel(true);
            }
        }

        public void ResumeGame()
        {
            IsPaused = false;
            Time.timeScale = 1f;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPausePanel(false);
            }
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
            EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>(FindObjectsInactive.Include);
            if (spawner != null)
            {
                spawner.ClearAllEnemies();
            }

            PlayerController player = FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
            if (player != null)
            {
                player.gameObject.SetActive(false);
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMainMenu();
            }
        }

        public void ResetHighScore()
        {
            PlayerPrefs.DeleteKey(HighScoreKey);
            HighScore = 0;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateScore(Score, HighScore);
            }
        }
    }
}

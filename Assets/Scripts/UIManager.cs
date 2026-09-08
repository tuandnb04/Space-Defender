using UnityEngine;
using UnityEngine.UI;

namespace SpaceDefender
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Main Menu Panel")]
        public GameObject mainMenuPanel;
        public Button playButton;
        public Button howToPlayButton;
        public GameObject howToPlayModal;
        public Button closeHowToPlayButton;
        public Button exitButton;

        [Header("In-Game HUD")]
        public GameObject inGameHUD;
        public Text scoreText;
        public Text highScoreText;
        public Image[] heartImages;
        public Button pauseButton;

        [Header("Boss HUD")]
        public GameObject bossBarPanel;
        public Slider bossHPSlider;
        public Text bossNameText;
        public Text bossHPText;

        [Header("Pause Panel")]
        public GameObject pausePanel;
        public Button resumeButton;
        public Button restartButton;
        public Button pauseMainMenuButton;

        [Header("Game Over Screen")]
        public GameObject gameOverPanel;
        public Text finalScoreText;
        public Text gameOverHighScoreText;
        public GameObject newRecordObject;
        public Button replayButton;
        public Button gameOverMainMenuButton;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        private void Awake()
        {
            Instance = this;
            WireButtonListeners();
        }

        private void WireButtonListeners()
        {
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                    if (GameManager.Instance != null) GameManager.Instance.StartGame();
                });
            }

            if (howToPlayButton != null)
            {
                howToPlayButton.onClick.RemoveAllListeners();
                howToPlayButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                    if (howToPlayModal != null) howToPlayModal.SetActive(true);
                });
            }

            if (closeHowToPlayButton != null)
            {
                closeHowToPlayButton.onClick.RemoveAllListeners();
                closeHowToPlayButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                    if (howToPlayModal != null) howToPlayModal.SetActive(false);
                });
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveAllListeners();
                pauseButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                    if (GameManager.Instance != null) GameManager.Instance.TogglePause();
                });
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                    if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
                });
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                    if (GameManager.Instance != null) GameManager.Instance.RestartGame();
                });
            }

            if (pauseMainMenuButton != null)
            {
                pauseMainMenuButton.onClick.RemoveAllListeners();
                pauseMainMenuButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                    if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();
                });
            }

            if (replayButton != null)
            {
                replayButton.onClick.RemoveAllListeners();
                replayButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                    if (GameManager.Instance != null) GameManager.Instance.RestartGame();
                });
            }

            if (gameOverMainMenuButton != null)
            {
                gameOverMainMenuButton.onClick.RemoveAllListeners();
                gameOverMainMenuButton.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                    if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();
                });
            }
        }

        public void ShowMainMenu()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (inGameHUD != null) inGameHUD.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (howToPlayModal != null) howToPlayModal.SetActive(false);
            ShowBossBar(false);
        }

        public void ShowInGameHUD()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (inGameHUD != null) inGameHUD.SetActive(true);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (howToPlayModal != null) howToPlayModal.SetActive(false);
            ShowBossBar(false);
        }

        public void ShowPausePanel(bool show)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(show);
            }
        }

        public void ShowHowToPlay(bool show)
        {
            if (howToPlayModal != null)
            {
                howToPlayModal.SetActive(show);
            }
        }

        public void ShowBossBar(bool show, string bossName = "RED UFO MOTHERSHIP")
        {
            if (bossBarPanel != null)
            {
                bossBarPanel.SetActive(show);
            }

            if (bossNameText != null && show)
            {
                bossNameText.text = bossName;
            }
        }

        public void UpdateBossHP(int current, int max)
        {
            if (bossHPSlider != null)
            {
                bossHPSlider.value = max > 0 ? (float)current / max : 0f;
            }

            if (bossHPText != null)
            {
                bossHPText.text = $"{current} / {max}";
            }
        }

        public void ShowGameOver(int finalScore, int highScore, bool isNewRecord = false)
        {
            ShowBossBar(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(true);

            if (finalScoreText != null)
            {
                finalScoreText.text = $"YOUR SCORE\n{finalScore}";
            }

            if (gameOverHighScoreText != null)
            {
                gameOverHighScoreText.text = $"BEST: {highScore}";
            }

            if (newRecordObject != null)
            {
                newRecordObject.SetActive(isNewRecord);
            }
        }

        public void UpdateScore(int score, int highScore = 0)
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE\n{score:D4}";
            }

            if (highScoreText != null)
            {
                highScoreText.text = $"BEST: {highScore:D4}";
            }
        }

        public void UpdateLives(int lives)
        {
            if (heartImages == null) return;

            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] != null)
                {
                    heartImages[i].enabled = (i < lives);
                }
            }
        }
    }
}

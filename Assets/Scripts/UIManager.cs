using System.Collections;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;

    [Header("Main Menu Panel")] public GameObject mainMenuPanel;

    public Button playButton;
    public Button howToPlayButton;
    public GameObject howToPlayModal;
    public Button closeHowToPlayButton;
    public Button exitButton;

    [Header("Main Menu Extras")] public Button openHangarButton;

    public Button openAchievementsButton;
    public Button openSettingsButton;

    [Header("In-Game HUD")] public GameObject inGameHUD;

    public Text scoreText;
    public Text highScoreText;
    public Image[] heartImages;
    public Button pauseButton;

    [Header("Bombs HUD")] public Text bombText;

    public Button bombButton;

    [Header("Combo HUD")] public GameObject comboPanel;

    public Text comboText;
    public Slider comboSlider;

    [Header("Boss HUD")] public GameObject bossBarPanel;

    [FormerlySerializedAs("bossHPSlider")] public Slider bossHpSlider;
    public Text bossNameText;
    [FormerlySerializedAs("bossHPText")] public Text bossHpText;

    [Header("Pause Panel")] public GameObject pausePanel;

    public Button resumeButton;
    public Button restartButton;
    public Button pauseSettingsButton;
    public Button pauseMainMenuButton;

    [Header("Game Over Screen")] public GameObject gameOverPanel;

    public Text finalScoreText;
    public Text gameOverHighScoreText;
    public GameObject newRecordObject;
    public Button replayButton;
    public Button gameOverMainMenuButton;

    [Header("Settings Modal")] public GameObject settingsModal;

    public Slider bgmSlider;
    public Slider sfxSlider;
    public Toggle muteToggle;
    public Button closeSettingsButton;

    [Header("Hangar Modal")] public GameObject hangarModal;

    public Image shipPreviewImage;
    public Text shipNameText;
    public Text shipStatsText;
    public Button prevShipButton;
    public Button nextShipButton;
    public Button selectShipButton;
    public Text selectShipButtonText;
    public Button closeHangarButton;
    public Sprite[] hangarShipSprites;

    [Header("Achievements Modal")] public GameObject achievementsModal;

    public Text achievementsListText;
    public Button closeAchievementsButton;

    [Header("Achievement Toast")] public GameObject achievementToast;

    public Text toastTitleText;
    public Text toastDescText;

    // Static Ship Info for Hangar
    private readonly (string name, string speed, string fireRate, string bombs, string perk)[] _shipInfo =
    {
        ("BLUE VANGUARD", "9.5 (Balanced)", "0.22s (Standard)", "2 Bombs",
            "All-around fleet fighter with balanced stats"),
        ("ORANGE INTERCEPTOR", "12.0 (High Speed)", "0.22s (Standard)", "1 Bomb",
            "Supersonic thrusters for agile evasion"),
        ("GREEN STRIKER", "9.0 (Standard)", "0.16s (Rapid Fire)", "2 Bombs", "Dual rapid-fire plasma cannons"),
        ("RED DREADNOUGHT", "8.2 (Heavy Armor)", "0.24s (Heavy)", "3 Bombs", "Deploys with ENERGY SHIELD & 3 Bombs")
    };

    private int _previewShipIndex;
    private Coroutine _toastCoroutine;

    public static UIManager Instance
    {
        get
        {
            if (!_instance) _instance = FindAnyObjectByType<UIManager>(FindObjectsInactive.Include);
            return _instance;
        }
        private set => _instance = value;
    }

    private void Awake()
    {
        Instance = this;
        _previewShipIndex = PlayerPrefs.GetInt("SD_SELECTED_SHIP", 0);
        WireButtonListeners();
    }

    private void Start()
    {
        WireButtonListeners();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    private void WireButtonListeners()
    {
        // Main Menu Buttons
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
                ShowHowToPlay(true);
            });
        }

        if (closeHowToPlayButton != null)
        {
            closeHowToPlayButton.onClick.RemoveAllListeners();
            closeHowToPlayButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                ShowHowToPlay(false);
            });
        }

        if (openHangarButton != null)
        {
            openHangarButton.onClick.RemoveAllListeners();
            openHangarButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                ShowHangar(true);
            });
        }

        if (closeHangarButton != null)
        {
            closeHangarButton.onClick.RemoveAllListeners();
            closeHangarButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                ShowHangar(false);
            });
        }

        if (prevShipButton != null)
        {
            prevShipButton.onClick.RemoveAllListeners();
            prevShipButton.onClick.AddListener(() => ChangeHangarSelection(-1));
        }

        if (nextShipButton != null)
        {
            nextShipButton.onClick.RemoveAllListeners();
            nextShipButton.onClick.AddListener(() => ChangeHangarSelection(1));
        }

        if (selectShipButton != null)
        {
            selectShipButton.onClick.RemoveAllListeners();
            selectShipButton.onClick.AddListener(ConfirmShipSelection);
        }

        if (openAchievementsButton != null)
        {
            openAchievementsButton.onClick.RemoveAllListeners();
            openAchievementsButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                ShowAchievements(true);
            });
        }

        if (closeAchievementsButton != null)
        {
            closeAchievementsButton.onClick.RemoveAllListeners();
            closeAchievementsButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                ShowAchievements(false);
            });
        }

        if (openSettingsButton != null)
        {
            openSettingsButton.onClick.RemoveAllListeners();
            openSettingsButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                ShowSettings(true);
            });
        }

        if (pauseSettingsButton != null)
        {
            pauseSettingsButton.onClick.RemoveAllListeners();
            pauseSettingsButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                ShowSettings(true);
            });
        }

        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.RemoveAllListeners();
            closeSettingsButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                ShowSettings(false);
            });
        }

        // Settings sliders
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.AddListener(val =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.SetBGMVolume(val);
            });
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(val =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(val);
            });
        }

        if (muteToggle != null)
        {
            muteToggle.onValueChanged.RemoveAllListeners();
            muteToggle.onValueChanged.AddListener(val =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.SetMute(val);
            });
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
            });
        }

        // HUD Bomb Button
        if (bombButton)
        {
            bombButton.onClick.RemoveAllListeners();
            bombButton.onClick.AddListener(() =>
            {
                var player = FindAnyObjectByType<PlayerController>();
                if (player) player.UseBomb();
            });
        }

        if (pauseButton)
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

        if (gameOverMainMenuButton == null) return;
        gameOverMainMenuButton.onClick.RemoveAllListeners();
        gameOverMainMenuButton.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();
        });
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (inGameHUD != null) inGameHUD.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (howToPlayModal != null) howToPlayModal.SetActive(false);
        if (settingsModal != null) settingsModal.SetActive(false);
        if (hangarModal != null) hangarModal.SetActive(false);
        if (achievementsModal != null) achievementsModal.SetActive(false);
        HideCombo();
        ShowBossBar(false);
    }

    public void ShowInGameHUD()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (inGameHUD != null) inGameHUD.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (howToPlayModal != null) howToPlayModal.SetActive(false);
        if (settingsModal != null) settingsModal.SetActive(false);
        if (hangarModal != null) hangarModal.SetActive(false);
        if (achievementsModal != null) achievementsModal.SetActive(false);
        HideCombo();
        ShowBossBar(false);
    }

    public void ShowPausePanel(bool show)
    {
        if (pausePanel) pausePanel.SetActive(show);
    }

    private void ShowHowToPlay(bool show)
    {
        if (howToPlayModal != null) howToPlayModal.SetActive(show);
    }

    public void ShowSettings(bool show)
    {
        if (settingsModal != null) settingsModal.SetActive(show);

        if (show)
        {
            if (hangarModal != null) hangarModal.SetActive(false);
            if (achievementsModal != null) achievementsModal.SetActive(false);
            if (howToPlayModal != null) howToPlayModal.SetActive(false);

            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            {
                if (pausePanel != null) pausePanel.SetActive(false);
            }
            else
            {
                if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
                var player = PlayerController.Instance;
                if (player != null) player.gameObject.SetActive(false);
            }

            if (AudioManager.Instance == null) return;
            if (bgmSlider != null) bgmSlider.value = AudioManager.Instance.bgmVolume;
            if (sfxSlider != null) sfxSlider.value = AudioManager.Instance.sfxVolume;
            if (muteToggle != null) muteToggle.isOn = AudioManager.Instance.isMuted;
        }
        else
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            {
                if (pausePanel != null) pausePanel.SetActive(true);
            }
            else if (GameManager.Instance == null || !GameManager.Instance.IsGameStarted)
            {
                if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                var player = PlayerController.Instance;
                if (player != null) player.gameObject.SetActive(true);
            }
        }
    }

    public void ShowHangar(bool show)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(!show);

        var player = PlayerController.Instance;
        if (player != null) player.gameObject.SetActive(!show);

        if (hangarModal != null) hangarModal.SetActive(show);

        if (!show) return;
        _previewShipIndex = PlayerPrefs.GetInt("SD_SELECTED_SHIP", 0);
        RefreshHangarUI();
    }

    private void ChangeHangarSelection(int delta)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        _previewShipIndex = (_previewShipIndex + delta + 4) % 4;
        RefreshHangarUI();
    }

    private void ConfirmShipSelection()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPowerUp();
        PlayerPrefs.SetInt("SD_SELECTED_SHIP", _previewShipIndex);
        PlayerPrefs.Save();
        RefreshHangarUI();
    }

    private void RefreshHangarUI()
    {
        var selectedShip = PlayerPrefs.GetInt("SD_SELECTED_SHIP", 0);

        if (hangarShipSprites != null && _previewShipIndex < hangarShipSprites.Length && shipPreviewImage != null)
            shipPreviewImage.sprite = hangarShipSprites[_previewShipIndex];

        if (shipNameText != null) shipNameText.text = _shipInfo[_previewShipIndex].name;

        if (shipStatsText != null)
        {
            var s = _shipInfo[_previewShipIndex];
            shipStatsText.text = $"Speed: {s.speed}\nFire Rate: {s.fireRate}\nBombs: {s.bombs}\nPerk: {s.perk}";
        }

        if (selectShipButtonText != null)
            selectShipButtonText.text = _previewShipIndex == selectedShip ? "SELECTED" : "SELECT SHIP";

        if (selectShipButton != null) selectShipButton.interactable = _previewShipIndex != selectedShip;
    }

    public void ShowAchievements(bool show)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(!show);

        var player = PlayerController.Instance;
        if (player != null) player.gameObject.SetActive(!show);

        if (achievementsModal != null) achievementsModal.SetActive(show);

        if (show) RefreshAchievementsUI();
    }

    private void RefreshAchievementsUI()
    {
        if (achievementsListText == null) return;

        if (AchievementManager.Instance == null) return;
        AchievementManager.Instance.EnsureInitialized();
        var sb = new StringBuilder();
        foreach (var ach in AchievementManager.Instance.achievements)
        {
            var status = ach.isUnlocked ? "<color=#00FF88>UNLOCKED</color>" : "<color=#888888>LOCKED</color>";
            sb.AppendLine($"<b>{ach.title}</b> - {status}");
            sb.AppendLine($"<i>{ach.description}</i>\n");
        }

        achievementsListText.text = sb.ToString();
    }

    public void ShowAchievementToast(string title, string description)
    {
        if (!achievementToast) return;

        if (_toastCoroutine != null) StopCoroutine(_toastCoroutine);
        _toastCoroutine = StartCoroutine(ToastRoutine(title, description));
    }

    private IEnumerator ToastRoutine(string title, string description)
    {
        if (toastTitleText) toastTitleText.text = $"ACHIEVEMENT UNLOCKED!\n{title}";
        if (toastDescText) toastDescText.text = description;

        achievementToast.SetActive(true);
        yield return new WaitForSecondsRealtime(3.2f);
        achievementToast.SetActive(false);
        _toastCoroutine = null;
    }

    public void UpdateBombs(int bombs)
    {
        if (bombText) bombText.text = $"BOMB [B]\nx{bombs}";

        if (bombButton) bombButton.interactable = bombs > 0;
    }

    public void UpdateCombo(int combo, int multiplier, float fillProgress)
    {
        if (comboPanel) comboPanel.SetActive(true);

        if (comboText)
        {
            comboText.text = $"COMBO x{multiplier}!\n({combo} HITS)";
            comboText.color = ComboManager.GetComboColor(multiplier);
        }

        if (comboSlider) comboSlider.value = fillProgress;
    }

    public void HideCombo()
    {
        if (comboPanel) comboPanel.SetActive(false);
    }

    public void ShowBossBar(bool show, string bossName = "RED UFO MOTHERSHIP")
    {
        if (bossBarPanel) bossBarPanel.SetActive(show);

        if (bossNameText && show) bossNameText.text = bossName;
    }

    public void UpdateBossHp(int current, int max)
    {
        if (bossHpSlider) bossHpSlider.value = max > 0 ? (float)current / max : 0f;

        if (bossHpText) bossHpText.text = $"{current} / {max}";
    }

    public void UpdateBossHP(int current, int max)
    {
        UpdateBossHp(current, max);
    }

    public void ShowGameOver(int finalScore, int highScore, bool isNewRecord = false)
    {
        ShowBossBar(false);
        HideCombo();
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (finalScoreText != null) finalScoreText.text = $"YOUR SCORE\n{finalScore}";

        if (gameOverHighScoreText != null) gameOverHighScoreText.text = $"BEST: {highScore}";

        if (newRecordObject != null) newRecordObject.SetActive(isNewRecord);
    }

    public void UpdateScore(int score, int highScore = 0)
    {
        if (scoreText) scoreText.text = $"SCORE\n{score:D4}";

        if (highScoreText) highScoreText.text = $"BEST: {highScore:D4}";
    }

    public void UpdateLives(int lives)
    {
        if (heartImages == null) return;

        for (var i = 0; i < heartImages.Length; i++)
            if (heartImages[i] != null)
                heartImages[i].enabled = i < lives;
    }
}
using Core;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class UIManager : MonoBehaviour
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
    public Button openLeaderboardButton;

    [Header("In-Game HUD")] public GameObject inGameHUD;
    public Text scoreText;
    public Text highScoreText;
    public Text waveText;
    public Image[] heartImages;
    public Button pauseButton;
    public Text starText;
    public Text weaponLevelText;

    [Header("Overload & Dash HUD")] public Slider overloadSlider;
    public Text overloadText;
    public Slider dashSlider;

    [Header("Wave Announcements")] public GameObject waveBannerPanel;
    public Text waveBannerTitle;
    public Text waveBannerSubtitle;

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
    public Toggle screenShakeToggle;
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

    [Header("Leaderboard Modal")] public GameObject leaderboardModal;
    public Text leaderboardListText;
    public Button closeLeaderboardButton;

    [Header("Perk Selection Modal")] public GameObject perkModal;
    public Transform perkCardsContainer;

    [Header("Hangar Shop Extras")] public Text hangarTotalStarsText;
    public Button upgradeSpeedButton;
    public Text upgradeSpeedButtonText;
    public Button upgradeMagnetButton;
    public Text upgradeMagnetButtonText;
    public Button upgradeHpButton;
    public Text upgradeHpButtonText;

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
        EnsureDynamicUI();
        WireButtonListeners();
    }

    private void Start()
    {
        WireButtonListeners();
    }

    private void Update()
    {
        // Instant Replay Hook on Game Over
        if (gameOverPanel == null || !gameOverPanel.activeInHierarchy) return;
        var restartPressed = false;
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
            restartPressed = true;
#else
            try
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                    restartPressed = true;
            }
            catch (System.InvalidOperationException) {}
#endif
        if (!restartPressed) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (GameManager.Instance != null) GameManager.Instance.RestartGame();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
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
        if (perkModal != null) perkModal.SetActive(false);
        HideCombo();
        ShowBossBar(false);
        HideWaveBanner();
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
        if (perkModal != null) perkModal.SetActive(false);
        HideCombo();
        ShowBossBar(false);
        HideWaveBanner();
    }

    public void ShowPausePanel(bool show)
    {
        if (pausePanel) pausePanel.SetActive(show);
    }

    private void EnsureDynamicUI()
    {
        var font = scoreText != null ? scoreText.font : Resources.GetBuiltinResource<Font>("Arial.ttf");

        // In-game HUD dynamic elements
        if (inGameHUD != null)
        {
            var hudTransform = inGameHUD.transform;

            if (starText == null)
            {
                var starObj = new GameObject("StarText_Dynamic");
                starObj.transform.SetParent(hudTransform, false);
                var rect = starObj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -60f);
                rect.sizeDelta = new Vector2(260f, 35f);

                starText = starObj.AddComponent<Text>();
                starText.font = font;
                starText.fontSize = 18;
                starText.alignment = TextAnchor.MiddleCenter;
                starText.color = new Color(1f, 0.9f, 0.2f);
                starText.text = "★ 0 (0)";
            }

            if (weaponLevelText == null)
            {
                var wpnObj = new GameObject("WeaponLevelText_Dynamic");
                wpnObj.transform.SetParent(hudTransform, false);
                var rect = wpnObj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 0f);
                rect.anchoredPosition = new Vector2(20f, 20f);
                rect.sizeDelta = new Vector2(180f, 30f);

                weaponLevelText = wpnObj.AddComponent<Text>();
                weaponLevelText.font = font;
                weaponLevelText.fontSize = 15;
                weaponLevelText.alignment = TextAnchor.MiddleLeft;
                weaponLevelText.color = new Color(0.2f, 1f, 0.5f);
                weaponLevelText.text = "WEAPON LV 1";
            }

            if (overloadSlider == null)
            {
                var ovObj = new GameObject("OverloadGauge_Dynamic");
                ovObj.transform.SetParent(hudTransform, false);
                var rect = ovObj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, 22f);
                rect.sizeDelta = new Vector2(220f, 20f);

                var bgImg = ovObj.AddComponent<Image>();
                bgImg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

                overloadSlider = ovObj.AddComponent<Slider>();
                overloadSlider.minValue = 0f;
                overloadSlider.maxValue = 1f;
                overloadSlider.value = 0f;

                var fillArea = new GameObject("FillArea");
                fillArea.transform.SetParent(ovObj.transform, false);
                var faRect = fillArea.AddComponent<RectTransform>();
                faRect.anchorMin = Vector2.zero;
                faRect.anchorMax = Vector2.one;
                faRect.sizeDelta = Vector2.zero;

                var fillObj = new GameObject("Fill");
                fillObj.transform.SetParent(fillArea.transform, false);
                var fRect = fillObj.AddComponent<RectTransform>();
                fRect.anchorMin = Vector2.zero;
                fRect.anchorMax = Vector2.one;
                fRect.sizeDelta = Vector2.zero;

                var fillImg = fillObj.AddComponent<Image>();
                fillImg.color = new Color(0.2f, 0.9f, 1f);
                overloadSlider.fillRect = fRect;

                var txtObj = new GameObject("OverloadText");
                txtObj.transform.SetParent(ovObj.transform, false);
                var tRect = txtObj.AddComponent<RectTransform>();
                tRect.anchorMin = Vector2.zero;
                tRect.anchorMax = Vector2.one;
                tRect.sizeDelta = Vector2.zero;

                overloadText = txtObj.AddComponent<Text>();
                overloadText.font = font;
                overloadText.fontSize = 12;
                overloadText.alignment = TextAnchor.MiddleCenter;
                overloadText.color = Color.white;
                overloadText.text = "OVERLOAD: 0%";
            }
        }

        // Dynamic modals
        EnsureLeaderboardModal();

        // Clean up any stray LeaderboardButton_Dynamic if it was previously spawned
        if (mainMenuPanel == null) return;
        var stray = mainMenuPanel.transform.Find("LeaderboardButton_Dynamic");
        if (stray == null) return;
        if (Application.isPlaying) Destroy(stray.gameObject);
        else DestroyImmediate(stray.gameObject);
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

        // Sub-systems / Modals Wire Listeners
        WireHangarButtons();
        WireModalButtons();
        WireLeaderboardButtons();

        // HUD Controls
        if (bombButton != null)
        {
            bombButton.onClick.RemoveAllListeners();
            bombButton.onClick.AddListener(() =>
            {
                var player = FindAnyObjectByType<PlayerController>();
                if (player) player.UseBomb();
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

        // Pause Panel Buttons
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

        // Game Over Buttons
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
}
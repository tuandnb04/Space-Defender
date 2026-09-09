using System.Collections;
using System.Text;
using Core;
using Player;
using UnityEngine;
using UnityEngine.UI;

public partial class UIManager
{
    private Coroutine _toastCoroutine;

    private void WireModalButtons()
    {
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

        if (screenShakeToggle == null && muteToggle != null && settingsModal != null)
        {
            var shakeGo = Instantiate(muteToggle.gameObject, settingsModal.transform);
            shakeGo.name = "ScreenShakeToggle";
            var rt = shakeGo.GetComponent<RectTransform>();
            if (rt != null) rt.anchoredPosition = new Vector2(-60, -220);
            screenShakeToggle = shakeGo.GetComponent<Toggle>();

            var origLbl = settingsModal.transform.Find("MuteLabel");
            if (origLbl != null)
            {
                var lblGo = Instantiate(origLbl.gameObject, settingsModal.transform);
                lblGo.name = "ScreenShakeLabel";
                var lblRt = lblGo.GetComponent<RectTransform>();
                if (lblRt != null) lblRt.anchoredPosition = new Vector2(40, -220);
                var tComp = lblGo.GetComponent<Text>();
                if (tComp != null) tComp.text = "SCREEN SHAKE";
            }
        }

        if (screenShakeToggle == null) return;
        {
            screenShakeToggle.onValueChanged.RemoveAllListeners();
            screenShakeToggle.onValueChanged.AddListener(val => { CameraShake.ScreenShakeEnabled = val; });
        }
    }

    private void ShowHowToPlay(bool show)
    {
        if (howToPlayModal == null) return;
        howToPlayModal.SetActive(show);
        if (!show) return;
        var contentText = howToPlayModal.transform.Find("ModalContent")?.GetComponent<Text>();
        if (contentText != null)
            contentText.text =
                "PC KEYBOARD & MOUSE:\n" +
                "- WASD / Arrow Keys: 2D Movement\n" +
                "- Left Click / Space: Fire Primary Weapons\n" +
                "- Left Shift: Tactical Dash (Invulnerable i-frame)\n" +
                "- B / Right Click: EMP Shockwave Bomb\n" +
                "- Esc / P: Pause Mission\n\n" +
                "GAMEPAD / CONTROLLER:\n" +
                "- Left Stick / D-Pad: 360° Flight Controls\n" +
                "- RT / A: Fire Weapons (Dual-Motor Rumble Haptics)\n" +
                "- LB / B: Tactical Dash (i-frames)\n" +
                "- Y / RB: EMP Shockwave Bomb\n" +
                "- Start: Pause Mission\n\n" +
                "POWER-UPS & COMBAT:\n" +
                "- Shield (Blue): Absorbs 1 lethal hit\n" +
                "- Star (Gold): Restores +1 Life Heart\n" +
                "- Chain combos to build x2 - x5 score multipliers!\n" +
                "- Dodge bullet hell rings & defeat the Mothership!";
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

            if (screenShakeToggle != null) screenShakeToggle.isOn = CameraShake.ScreenShakeEnabled;

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
}
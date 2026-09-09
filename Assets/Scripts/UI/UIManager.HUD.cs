using System.Collections;
using Core;
using UnityEngine;

public partial class UIManager
{
    private int _lastBombs = -1;
    private int _lastBossCurrentHp = -1;
    private int _lastBossMaxHp = -1;
    private float _lastDashRatio = -1f;
    private int _lastHighScore = -1;

    private float _lastOverloadRatio = -1f;

    // Performance Optimization: Dirty-flag cache to eliminate redundant canvas mesh rebuilds
    private int _lastScore = -1;
    private int _lastStars = -1;
    private int _lastTotalStars = -1;
    private int _lastWave = -1;
    private int _lastWeaponLevel = -1;
    private Coroutine _waveBannerCoroutine;

    public void UpdateScore(int score, int highScore = 0)
    {
        if (_lastScore != score)
        {
            _lastScore = score;
            if (scoreText) scoreText.text = $"SCORE\n{score:D4}";
        }

        if (_lastHighScore == highScore) return;
        _lastHighScore = highScore;
        if (highScoreText) highScoreText.text = $"BEST: {highScore:D4}";
    }

    public void UpdateWave(int wave)
    {
        if (_lastWave == wave) return;
        _lastWave = wave;

        if (waveText) waveText.text = $"WAVE\n{wave:D2}";
    }

    public void UpdateLives(int lives)
    {
        if (heartImages == null) return;

        for (var i = 0; i < heartImages.Length; i++)
            if (heartImages[i] != null)
                heartImages[i].enabled = i < lives;
    }

    public void UpdateBombs(int bombs)
    {
        if (_lastBombs == bombs) return;
        _lastBombs = bombs;

        if (bombText) bombText.text = $"BOMB [B / RB]\nx{bombs}";
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
        if (_lastBossCurrentHp == current && _lastBossMaxHp == max) return;
        _lastBossCurrentHp = current;
        _lastBossMaxHp = max;

        if (bossHpSlider) bossHpSlider.value = max > 0 ? (float)current / max : 0f;
        if (bossHpText) bossHpText.text = $"{current} / {max}";
    }

    public void UpdateWeaponLevel(int level)
    {
        if (_lastWeaponLevel == level) return;
        _lastWeaponLevel = level;

        if (weaponLevelText != null)
            weaponLevelText.text = level == 5 ? "<color=#00FFFF>OVERDRIVE</color>" : $"WEAPON LV {level}";
    }

    public void UpdateOverload(float ratio)
    {
        if (Mathf.Abs(_lastOverloadRatio - ratio) < 0.005f) return;
        _lastOverloadRatio = ratio;

        if (overloadSlider) overloadSlider.value = ratio;
        if (!overloadText) return;
        overloadText.text = ratio >= 0.99f ? "FEVER READY!" : $"OVERLOAD: {Mathf.FloorToInt(ratio * 100)}%";
        overloadText.color = ratio >= 0.99f ? new Color(0.2f, 1f, 0.9f) : Color.white;
    }

    public void UpdateDashCooldown(float ratio)
    {
        if (Mathf.Abs(_lastDashRatio - ratio) < 0.008f) return;
        _lastDashRatio = ratio;

        if (dashSlider) dashSlider.value = 1f - ratio;
    }

    public void UpdateStars(int sessionStars, int totalStars)
    {
        if (_lastStars == sessionStars && _lastTotalStars == totalStars) return;
        _lastStars = sessionStars;
        _lastTotalStars = totalStars;

        if (starText) starText.text = $"★ {sessionStars} ({totalStars})";
    }

    public void ShowWaveBanner(string title, string subtitle, Color color, float duration = 2.0f)
    {
        if (!waveBannerPanel) return;
        if (_waveBannerCoroutine != null) StopCoroutine(_waveBannerCoroutine);
        _waveBannerCoroutine = StartCoroutine(WaveBannerRoutine(title, subtitle, color, duration, false));
    }

    public void ShowBossWarning(string bossName = "RED UFO MOTHERSHIP", float duration = 2.5f)
    {
        if (!waveBannerPanel) return;
        if (_waveBannerCoroutine != null) StopCoroutine(_waveBannerCoroutine);
        _waveBannerCoroutine = StartCoroutine(WaveBannerRoutine("WARNING: BOSS DETECTED!",
            $"{bossName} APPROACHING", new Color(1f, 0.2f, 0.25f), duration, true));
    }

    public void HideWaveBanner()
    {
        if (_waveBannerCoroutine != null)
        {
            StopCoroutine(_waveBannerCoroutine);
            _waveBannerCoroutine = null;
        }

        if (waveBannerPanel) waveBannerPanel.SetActive(false);
    }

    private IEnumerator WaveBannerRoutine(string title, string subtitle, Color color, float duration, bool isWarning)
    {
        if (waveBannerTitle)
        {
            waveBannerTitle.text = title;
            waveBannerTitle.color = color;
        }

        if (waveBannerSubtitle) waveBannerSubtitle.text = subtitle;

        waveBannerPanel.SetActive(true);

        var elapsed = 0f;
        while (elapsed < duration)
        {
            if (isWarning && waveBannerTitle)
            {
                var flash = Mathf.PingPong(elapsed * 6f, 1f);
                waveBannerTitle.color = Color.Lerp(new Color(1f, 0.15f, 0.15f), new Color(1f, 0.9f, 0.2f), flash);
            }

            yield return new WaitForSeconds(0.05f);
            elapsed += 0.05f;
        }

        waveBannerPanel.SetActive(false);
        _waveBannerCoroutine = null;
    }

    public void ShowGameOver(int finalScore, int highScore, bool isNewRecord = false, int rank = 0)
    {
        ShowBossBar(false);
        HideCombo();
        HideWaveBanner();
        if (pausePanel != null) pausePanel.SetActive(false);
        if (perkModal != null) perkModal.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        var top1Score = highScore;
        if (LeaderboardManager.Instance != null)
        {
            var entries = LeaderboardManager.Instance.GetTopEntries();
            if (entries is { Count: > 0 } && entries[0].score > top1Score) top1Score = entries[0].score;
        }

        var gapLine = "";
        if (finalScore >= top1Score && finalScore > 0)
        {
            gapLine = "<color=#00FF88>★ NEW HIGH SCORE CHAMPION! ★</color>";
        }
        else if (top1Score > finalScore)
        {
            var gap = top1Score - finalScore;
            gapLine = $"<color=#FFDD44>Only {gap} pts behind #1!</color>";
        }

        var rankLine = rank > 0 ? $"<color=#00FF88>TOP 5 RANK #{rank}!</color>\n" : "";

        if (finalScoreText != null)
            finalScoreText.text =
                $"YOUR SCORE\n{finalScore}\n{rankLine}{gapLine}\n\n<color=#00FFFF><b>[SPACE] RETRY IMMEDIATELY</b></color>";

        if (gameOverHighScoreText != null) gameOverHighScoreText.text = $"BEST: {highScore}";

        if (newRecordObject != null) newRecordObject.SetActive(isNewRecord);
    }

    public void ShowGameOver(int finalScore, int highScore, bool isNewRecord)
    {
        ShowGameOver(finalScore, highScore, isNewRecord, 0);
    }
}
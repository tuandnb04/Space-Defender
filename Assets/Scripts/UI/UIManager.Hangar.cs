using Core;
using Player;
using UnityEngine;

public partial class UIManager
{
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

    private readonly int[] _shipUnlockCosts = { 0, 50, 100, 200 };
    private int _previewShipIndex;

    private void WireHangarButtons()
    {
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

        if (upgradeSpeedButton != null)
        {
            upgradeSpeedButton.onClick.RemoveAllListeners();
            upgradeSpeedButton.onClick.AddListener(BuySpeedUpgrade);
        }

        if (upgradeMagnetButton != null)
        {
            upgradeMagnetButton.onClick.RemoveAllListeners();
            upgradeMagnetButton.onClick.AddListener(BuyMagnetUpgrade);
        }

        if (upgradeHpButton != null)
        {
            upgradeHpButton.onClick.RemoveAllListeners();
            upgradeHpButton.onClick.AddListener(BuyHpUpgrade);
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

    private static bool IsShipUnlocked(int index)
    {
        if (index == 0) return true;
        return PlayerPrefs.GetInt("SD_SHIP_UNLOCKED_" + index, 0) == 1;
    }

    private void ConfirmShipSelection()
    {
        var isUnlocked = IsShipUnlocked(_previewShipIndex);
        var cost = _shipUnlockCosts[_previewShipIndex];
        var totalStars = PlayerPrefs.GetInt("SD_TOTAL_STARS", 0);

        if (!isUnlocked)
        {
            if (totalStars >= cost)
            {
                totalStars -= cost;
                PlayerPrefs.SetInt("SD_TOTAL_STARS", totalStars);
                PlayerPrefs.SetInt("SD_SHIP_UNLOCKED_" + _previewShipIndex, 1);
                PlayerPrefs.SetInt("SD_SELECTED_SHIP", _previewShipIndex);
                PlayerPrefs.Save();

                if (AudioManager.Instance != null) AudioManager.Instance.PlayPowerUp();
                RefreshHangarUI();
            }
            else
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayShieldDown();
            }

            return;
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayPowerUp();
        PlayerPrefs.SetInt("SD_SELECTED_SHIP", _previewShipIndex);
        PlayerPrefs.Save();
        RefreshHangarUI();
    }

    private void RefreshHangarUI()
    {
        var selectedShip = PlayerPrefs.GetInt("SD_SELECTED_SHIP", 0);
        var totalStars = PlayerPrefs.GetInt("SD_TOTAL_STARS", 0);
        var isUnlocked = IsShipUnlocked(_previewShipIndex);
        var unlockCost = _shipUnlockCosts[_previewShipIndex];

        if (hangarTotalStarsText != null)
            hangarTotalStarsText.text = $"STARS: {totalStars}";

        if (hangarShipSprites != null && _previewShipIndex < hangarShipSprites.Length && shipPreviewImage != null)
            shipPreviewImage.sprite = hangarShipSprites[_previewShipIndex];

        if (shipNameText != null)
            shipNameText.text = isUnlocked
                ? _shipInfo[_previewShipIndex].name
                : $"{_shipInfo[_previewShipIndex].name} (LOCKED)";

        if (shipStatsText != null)
        {
            var s = _shipInfo[_previewShipIndex];
            var spd = PlayerPrefs.GetInt("SD_UPGRADE_SPEED_LV", 0);
            var mag = PlayerPrefs.GetInt("SD_UPGRADE_MAGNET_LV", 0);
            var hp = PlayerPrefs.GetInt("SD_UPGRADE_MAXHP_LV", 0);
            shipStatsText.text =
                $"Speed: {s.speed} (+{spd * 6}%)\nFire Rate: {s.fireRate}\nBombs: {s.bombs}\nPerk: {s.perk}\n" +
                $"<color=#00FFFF>Permanent Upgrades: Speed Lv{spd}/5 | Magnet Lv{mag}/5 | Armor Lv{hp}/3</color>";
        }

        if (selectShipButtonText != null)
        {
            if (!isUnlocked)
                selectShipButtonText.text = totalStars >= unlockCost
                    ? $"UNLOCK ({unlockCost} STARS)"
                    : $"NEED {unlockCost} STARS";
            else
                selectShipButtonText.text = _previewShipIndex == selectedShip ? "SELECTED" : "SELECT SHIP";
        }

        if (selectShipButton != null)
        {
            if (!isUnlocked)
                selectShipButton.interactable = totalStars >= unlockCost;
            else
                selectShipButton.interactable = _previewShipIndex != selectedShip;
        }

        // Upgrade Buttons UI
        var spdLv = PlayerPrefs.GetInt("SD_UPGRADE_SPEED_LV", 0);
        var magLv = PlayerPrefs.GetInt("SD_UPGRADE_MAGNET_LV", 0);
        var hpLv = PlayerPrefs.GetInt("SD_UPGRADE_MAXHP_LV", 0);

        if (upgradeSpeedButtonText != null)
            upgradeSpeedButtonText.text = spdLv >= 5 ? "SPEED: MAX" : $"SPEED +6% ({20 * (spdLv + 1)} ★)";
        if (upgradeSpeedButton != null)
            upgradeSpeedButton.interactable = spdLv < 5 && totalStars >= 20 * (spdLv + 1);

        if (upgradeMagnetButtonText != null)
            upgradeMagnetButtonText.text = magLv >= 5 ? "MAGNET: MAX" : $"MAGNET +1m ({25 * (magLv + 1)} ★)";
        if (upgradeMagnetButton != null)
            upgradeMagnetButton.interactable = magLv < 5 && totalStars >= 25 * (magLv + 1);

        if (upgradeHpButtonText != null)
            upgradeHpButtonText.text = hpLv >= 3 ? "HULL: MAX" : $"HULL +1 HP ({50 * (hpLv + 1)} ★)";
        if (upgradeHpButton != null)
            upgradeHpButton.interactable = hpLv < 3 && totalStars >= 50 * (hpLv + 1);
    }

    private void BuySpeedUpgrade()
    {
        var currentLv = PlayerPrefs.GetInt("SD_UPGRADE_SPEED_LV", 0);
        if (currentLv >= 5) return;
        var cost = 20 * (currentLv + 1);
        var totalStars = PlayerPrefs.GetInt("SD_TOTAL_STARS", 0);
        if (totalStars < cost) return;
        totalStars -= cost;
        PlayerPrefs.SetInt("SD_TOTAL_STARS", totalStars);
        PlayerPrefs.SetInt("SD_UPGRADE_SPEED_LV", currentLv + 1);
        PlayerPrefs.Save();
        if (AudioManager.Instance) AudioManager.Instance.PlayPowerUp();
        var player = PlayerController.Instance;
        if (player) player.ApplyPermanentUpgrades();
        RefreshHangarUI();
    }

    private void BuyMagnetUpgrade()
    {
        var currentLv = PlayerPrefs.GetInt("SD_UPGRADE_MAGNET_LV", 0);
        if (currentLv >= 5) return;
        var cost = 25 * (currentLv + 1);
        var totalStars = PlayerPrefs.GetInt("SD_TOTAL_STARS", 0);
        if (totalStars < cost) return;
        totalStars -= cost;
        PlayerPrefs.SetInt("SD_TOTAL_STARS", totalStars);
        PlayerPrefs.SetInt("SD_UPGRADE_MAGNET_LV", currentLv + 1);
        PlayerPrefs.Save();
        if (AudioManager.Instance) AudioManager.Instance.PlayPowerUp();
        RefreshHangarUI();
    }

    private void BuyHpUpgrade()
    {
        var currentLv = PlayerPrefs.GetInt("SD_UPGRADE_MAXHP_LV", 0);
        if (currentLv >= 3) return;
        var cost = 50 * (currentLv + 1);
        var totalStars = PlayerPrefs.GetInt("SD_TOTAL_STARS", 0);
        if (totalStars < cost) return;
        totalStars -= cost;
        PlayerPrefs.SetInt("SD_TOTAL_STARS", totalStars);
        PlayerPrefs.SetInt("SD_UPGRADE_MAXHP_LV", currentLv + 1);
        PlayerPrefs.Save();
        if (AudioManager.Instance) AudioManager.Instance.PlayPowerUp();
        var player = PlayerController.Instance;
        if (player) player.ApplyPermanentUpgrades();
        RefreshHangarUI();
    }
}
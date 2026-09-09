using System.Text;
using Core;
using Player;
using UnityEngine;
using UnityEngine.UI;

public partial class UIManager
{
    private void WireLeaderboardButtons()
    {
        if (openLeaderboardButton != null)
        {
            openLeaderboardButton.onClick.RemoveAllListeners();
            openLeaderboardButton.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                ShowLeaderboard(true);
            });
        }

        if (closeLeaderboardButton == null) return;
        closeLeaderboardButton.onClick.RemoveAllListeners();
        closeLeaderboardButton.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            ShowLeaderboard(false);
        });
    }

    private void ShowLeaderboard(bool show)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(!show);

        var player = PlayerController.Instance;
        if (player != null) player.gameObject.SetActive(!show);

        EnsureLeaderboardModal();

        if (leaderboardModal != null) leaderboardModal.SetActive(show);

        if (show) RefreshLeaderboardUI();
    }

    private void RefreshLeaderboardUI()
    {
        if (leaderboardListText == null) return;
        if (LeaderboardManager.Instance == null) return;

        var entries = LeaderboardManager.Instance.GetTopEntries();
        var sb = new StringBuilder();
        sb.AppendLine("<b><color=#FFD700>TOP 5 SPACE DEFENDERS</color></b>\n");

        var rankColors = new[] { "#FFD700", "#C0C0C0", "#CD7F32", "#00FFFF", "#FFFFFF" };

        for (var i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            var col = i < rankColors.Length ? rankColors[i] : "#FFFFFF";
            sb.AppendLine($"<color={col}><b>#{i + 1}</b>  {e.score:D4} PTS  |  WAVE {e.wave:D2}</color>");
            sb.AppendLine($"     <size=11><i>{e.shipName} ({e.date})</i></size>\n");
        }

        leaderboardListText.text = sb.ToString();
    }

    private void EnsureLeaderboardModal()
    {
        if (leaderboardModal != null) return;

        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        var font = scoreText != null ? scoreText.font : Resources.GetBuiltinResource<Font>("Arial.ttf");

        var modalObj = new GameObject("LeaderboardModal_Dynamic");
        modalObj.transform.SetParent(canvas.transform, false);
        var rect = modalObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(420f, 500f);

        var img = modalObj.AddComponent<Image>();
        img.color = new Color(0.06f, 0.08f, 0.16f, 0.96f);

        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(modalObj.transform, false);
        var tRect = titleObj.AddComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.5f, 1f);
        tRect.anchorMax = new Vector2(0.5f, 1f);
        tRect.pivot = new Vector2(0.5f, 1f);
        tRect.anchoredPosition = new Vector2(0f, -20f);
        tRect.sizeDelta = new Vector2(380f, 40f);

        var titleText = titleObj.AddComponent<Text>();
        titleText.font = font;
        titleText.fontSize = 20;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(1f, 0.85f, 0.2f);
        titleText.text = "TOP 5 LEADERBOARD";

        var listObj = new GameObject("ListText");
        listObj.transform.SetParent(modalObj.transform, false);
        var lRect = listObj.AddComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0.5f, 0.5f);
        lRect.anchorMax = new Vector2(0.5f, 0.5f);
        lRect.pivot = new Vector2(0.5f, 0.5f);
        lRect.anchoredPosition = new Vector2(0f, 10f);
        lRect.sizeDelta = new Vector2(380f, 320f);

        leaderboardListText = listObj.AddComponent<Text>();
        leaderboardListText.font = font;
        leaderboardListText.fontSize = 15;
        leaderboardListText.lineSpacing = 1.25f;
        leaderboardListText.alignment = TextAnchor.UpperLeft;
        leaderboardListText.color = Color.white;

        var btnObj = new GameObject("CloseButton");
        btnObj.transform.SetParent(modalObj.transform, false);
        var bRect = btnObj.AddComponent<RectTransform>();
        bRect.anchorMin = new Vector2(0.5f, 0f);
        bRect.anchorMax = new Vector2(0.5f, 0f);
        bRect.pivot = new Vector2(0.5f, 0f);
        bRect.anchoredPosition = new Vector2(0f, 20f);
        bRect.sizeDelta = new Vector2(160f, 40f);

        var bImg = btnObj.AddComponent<Image>();
        bImg.color = new Color(0.85f, 0.2f, 0.25f);

        closeLeaderboardButton = btnObj.AddComponent<Button>();
        closeLeaderboardButton.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            ShowLeaderboard(false);
        });

        var bTextObj = new GameObject("Text");
        bTextObj.transform.SetParent(btnObj.transform, false);
        var btRect = bTextObj.AddComponent<RectTransform>();
        btRect.anchorMin = Vector2.zero;
        btRect.anchorMax = Vector2.one;
        btRect.sizeDelta = Vector2.zero;

        var bText = bTextObj.AddComponent<Text>();
        bText.font = font;
        bText.fontSize = 16;
        bText.alignment = TextAnchor.MiddleCenter;
        bText.color = Color.white;
        bText.text = "CLOSE";

        modalObj.SetActive(false);
        leaderboardModal = modalObj;
    }
}
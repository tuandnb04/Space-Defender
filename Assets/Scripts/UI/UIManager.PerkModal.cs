using Core;
using UnityEngine;
using UnityEngine.UI;

public partial class UIManager
{
    public void ShowPerkModal()
    {
        if (PerkManager.Instance == null) return;
        var perks = PerkManager.Instance.GetThreeRandomPerks();
        if (perks == null || perks.Count == 0) return;

        EnsurePerkModalExists();
        if (perkModal == null) return;

        perkModal.SetActive(true);
        Time.timeScale = 0f; // Freeze game during rogue-lite perk card selection

        for (var i = 0; i < perkCardsContainer.childCount; i++)
        {
            var card = perkCardsContainer.GetChild(i);
            if (i < perks.Count)
            {
                card.gameObject.SetActive(true);
                var perk = perks[i];

                var title = card.Find("Title")?.GetComponent<Text>();
                var desc = card.Find("Desc")?.GetComponent<Text>();
                var tag = card.Find("Tag")?.GetComponent<Text>();
                var btn = card.Find("SelectBtn")?.GetComponent<Button>();

                if (title != null)
                {
                    title.text = perk.perkName;
                    title.color = perk.themeColor;
                }

                if (desc != null) desc.text = perk.description;
                if (tag != null)
                {
                    tag.text = perk.iconSymbol;
                    tag.color = perk.themeColor;
                }

                if (btn == null) continue;
                btn.onClick.RemoveAllListeners();
                var pType = perk.perkType;
                btn.onClick.AddListener(() =>
                {
                    Time.timeScale = 1.0f;
                    perkModal.SetActive(false);
                    PerkManager.Instance.ApplyPerk(pType);
                });
            }
            else
            {
                card.gameObject.SetActive(false);
            }
        }
    }

    private void EnsurePerkModalExists()
    {
        if (perkModal != null) return;

        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        var font = scoreText != null ? scoreText.font : Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Modal Root
        var modalObj = new GameObject("PerkModal_Dynamic");
        modalObj.transform.SetParent(canvas.transform, false);
        var rect = modalObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        var bg = modalObj.AddComponent<Image>();
        bg.color = new Color(0.02f, 0.04f, 0.10f, 0.94f);

        // Panel Container
        var panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(modalObj.transform, false);
        var pRect = panelObj.AddComponent<RectTransform>();
        pRect.anchorMin = new Vector2(0.5f, 0.5f);
        pRect.anchorMax = new Vector2(0.5f, 0.5f);
        pRect.pivot = new Vector2(0.5f, 0.5f);
        pRect.anchoredPosition = Vector2.zero;
        pRect.sizeDelta = new Vector2(580f, 420f);

        // Header Title
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        var tRect = titleObj.AddComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.5f, 1f);
        tRect.anchorMax = new Vector2(0.5f, 1f);
        tRect.pivot = new Vector2(0.5f, 1f);
        tRect.anchoredPosition = new Vector2(0f, -15f);
        tRect.sizeDelta = new Vector2(540f, 40f);

        var titleTxt = titleObj.AddComponent<Text>();
        titleTxt.font = font;
        titleTxt.fontSize = 22;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = new Color(1f, 0.85f, 0.2f);
        titleTxt.text = "BOSS TECHNOLOGY RECOVERED";

        // Subtitle
        var subObj = new GameObject("Subtitle");
        subObj.transform.SetParent(panelObj.transform, false);
        var sRect = subObj.AddComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0.5f, 1f);
        sRect.anchorMax = new Vector2(0.5f, 1f);
        sRect.pivot = new Vector2(0.5f, 1f);
        sRect.anchoredPosition = new Vector2(0f, -50f);
        sRect.sizeDelta = new Vector2(540f, 25f);

        var subTxt = subObj.AddComponent<Text>();
        subTxt.font = font;
        subTxt.fontSize = 13;
        subTxt.alignment = TextAnchor.MiddleCenter;
        subTxt.color = new Color(0.7f, 0.85f, 1f);
        subTxt.text = "CHOOSE 1 PERK FOR THIS RUN";

        // Cards Container
        var cardsObj = new GameObject("CardsContainer");
        cardsObj.transform.SetParent(panelObj.transform, false);
        var cRect = cardsObj.AddComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.pivot = new Vector2(0.5f, 0.5f);
        cRect.anchoredPosition = new Vector2(0f, -25f);
        cRect.sizeDelta = new Vector2(560f, 300f);
        perkCardsContainer = cardsObj.transform;

        const float cardWidth = 170f;
        const float cardHeight = 280f;
        const float cardSpacing = 190f;

        for (var i = 0; i < 3; i++)
        {
            var card = new GameObject($"Card_{i}");
            card.transform.SetParent(perkCardsContainer, false);
            var cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = new Vector2((i - 1) * cardSpacing, 0f);
            cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);

            var cardImg = card.AddComponent<Image>();
            cardImg.color = new Color(0.08f, 0.12f, 0.22f, 0.95f);

            // Tag / Icon
            var tagObj = new GameObject("Tag");
            tagObj.transform.SetParent(card.transform, false);
            var tagRect = tagObj.AddComponent<RectTransform>();
            tagRect.anchorMin = new Vector2(0.5f, 1f);
            tagRect.anchorMax = new Vector2(0.5f, 1f);
            tagRect.pivot = new Vector2(0.5f, 1f);
            tagRect.anchoredPosition = new Vector2(0f, -15f);
            tagRect.sizeDelta = new Vector2(cardWidth - 20f, 30f);

            var tagTxt = tagObj.AddComponent<Text>();
            tagTxt.font = font;
            tagTxt.fontSize = 18;
            tagTxt.alignment = TextAnchor.MiddleCenter;
            tagTxt.color = Color.cyan;

            // Title
            var cardTitleObj = new GameObject("Title");
            cardTitleObj.transform.SetParent(card.transform, false);
            var ctRect = cardTitleObj.AddComponent<RectTransform>();
            ctRect.anchorMin = new Vector2(0.5f, 1f);
            ctRect.anchorMax = new Vector2(0.5f, 1f);
            ctRect.pivot = new Vector2(0.5f, 1f);
            ctRect.anchoredPosition = new Vector2(0f, -50f);
            ctRect.sizeDelta = new Vector2(cardWidth - 20f, 45f);

            var ctTxt = cardTitleObj.AddComponent<Text>();
            ctTxt.font = font;
            ctTxt.fontSize = 13;
            ctTxt.lineSpacing = 1.1f;
            ctTxt.alignment = TextAnchor.MiddleCenter;
            ctTxt.color = Color.white;

            // Description
            var descObj = new GameObject("Desc");
            descObj.transform.SetParent(card.transform, false);
            var dRect = descObj.AddComponent<RectTransform>();
            dRect.anchorMin = new Vector2(0.5f, 0.5f);
            dRect.anchorMax = new Vector2(0.5f, 0.5f);
            dRect.pivot = new Vector2(0.5f, 0.5f);
            dRect.anchoredPosition = new Vector2(0f, -5f);
            dRect.sizeDelta = new Vector2(cardWidth - 24f, 120f);

            var dTxt = descObj.AddComponent<Text>();
            dTxt.font = font;
            dTxt.fontSize = 11;
            dTxt.lineSpacing = 1.25f;
            dTxt.alignment = TextAnchor.MiddleCenter;
            dTxt.color = new Color(0.85f, 0.88f, 0.95f);

            // Select Button
            var btnObj = new GameObject("SelectBtn");
            btnObj.transform.SetParent(card.transform, false);
            var bRect = btnObj.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0f);
            bRect.anchorMax = new Vector2(0.5f, 0f);
            bRect.pivot = new Vector2(0.5f, 0f);
            bRect.anchoredPosition = new Vector2(0f, 15f);
            bRect.sizeDelta = new Vector2(cardWidth - 30f, 35f);

            var bImg = btnObj.AddComponent<Image>();
            bImg.color = new Color(0.12f, 0.55f, 0.95f);

            btnObj.AddComponent<Button>();

            var btnTxtObj = new GameObject("Text");
            btnTxtObj.transform.SetParent(btnObj.transform, false);
            var btRect = btnTxtObj.AddComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;
            btRect.sizeDelta = Vector2.zero;

            var btTxt = btnTxtObj.AddComponent<Text>();
            btTxt.font = font;
            btTxt.fontSize = 14;
            btTxt.alignment = TextAnchor.MiddleCenter;
            btTxt.color = Color.white;
            btTxt.text = "SELECT";
        }

        modalObj.SetActive(false);
        perkModal = modalObj;
    }
}
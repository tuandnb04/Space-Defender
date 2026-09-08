using UnityEngine;

public class ComboManager : MonoBehaviour
{
    private static ComboManager _instance;
    public static ComboManager Instance
    {
        get
        {
            if (!_instance) _instance = FindAnyObjectByType<ComboManager>(FindObjectsInactive.Include);
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Combo Settings")]
    public float comboTimeout = 2.2f;
    public int maxMultiplier = 5;

    private int CurrentCombo { get; set; }
    private int Multiplier { get; set; } = 1;
    private float TimeRemaining { get; set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    private void Awake()
    {
        Instance = this;
        ResetCombo();
    }

    private void Update()
    {
        if (CurrentCombo <= 0) return;
        TimeRemaining -= Time.deltaTime;

        if (TimeRemaining <= 0f)
        {
            ResetCombo();
        }
        else
        {
            if (UIManager.Instance)
            {
                UIManager.Instance.UpdateCombo(CurrentCombo, Multiplier, Mathf.Clamp01(TimeRemaining / comboTimeout));
            }
        }
    }

    public int RegisterKill(int baseScore, Vector3 position, GameObject floatingScorePrefab = null)
    {
        if (TimeRemaining > 0f)
        {
            CurrentCombo++;
        }
        else
        {
            CurrentCombo = 1;
        }

        TimeRemaining = comboTimeout;
        Multiplier = Mathf.Clamp(CurrentCombo, 1, maxMultiplier);

        var finalScore = baseScore * Multiplier;

        // Check combo achievement
        if (CurrentCombo >= 5 && AchievementManager.Instance != null)
        {
            AchievementManager.Instance.UnlockAchievement("COMBO_5X");
        }

        // Spawn floating score with combo colors
        if (floatingScorePrefab)
        {
            var scoreColor = GetComboColor(Multiplier);
            var text = Multiplier > 1 ? $"+{finalScore} (x{Multiplier}!)" : $"+{finalScore}";
            FloatingScore.SpawnText(floatingScorePrefab, position, text, scoreColor);
        }

        if (UIManager.Instance)
        {
            UIManager.Instance.UpdateCombo(CurrentCombo, Multiplier, 1f);
        }

        return finalScore;
    }

    private void ResetCombo()
    {
        CurrentCombo = 0;
        Multiplier = 1;
        TimeRemaining = 0f;

        if (UIManager.Instance)
        {
            UIManager.Instance.HideCombo();
        }
    }

    public static Color GetComboColor(int multiplier)
    {
        return multiplier switch
        {
            1 => Color.white,
            2 => new Color(1f, 0.9f, 0.2f) // Golden yellow
            ,
            3 => new Color(1f, 0.55f, 0.1f) // Vibrant orange
            ,
            4 => new Color(1f, 0.25f, 0.4f) // Neon crimson
            ,
            5 => new Color(0.9f, 0.3f, 1f) // Blazing neon purple
            ,
            _ => new Color(0.2f, 1f, 0.9f)
        };
    }
}
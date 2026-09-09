using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [Serializable]
    public class Achievement
    {
        public string id;
        public string title;
        public string description;
        public string icon;
        public bool isUnlocked;
    }

    public class AchievementManager : MonoBehaviour
    {
        private const string PrefKeyPrefix = "SD_ACH_";
        private static AchievementManager _instance;

        public List<Achievement> achievements = new();

        public static AchievementManager Instance
        {
            get
            {
                if (!_instance) _instance = FindAnyObjectByType<AchievementManager>(FindObjectsInactive.Include);
                return _instance;
            }
            private set => _instance = value;
        }

        private void Awake()
        {
            Instance = this;
            InitializeAchievements();
            LoadAchievements();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        public void EnsureInitialized()
        {
            if (achievements == null || achievements.Count == 0) InitializeAchievements();
            LoadAchievements();
        }

        private void InitializeAchievements()
        {
            if (achievements == null || achievements.Count == 0)
                achievements = new List<Achievement>
                {
                    new()
                    {
                        id = "FIRST_BLOOD", title = "First Blood", description = "Destroy your first enemy ship",
                        icon = "STAR"
                    },
                    new()
                    {
                        id = "SHIELD_UP", title = "Iron Wall", description = "Deploy an Energy Shield barrier",
                        icon = "SHIELD"
                    },
                    new()
                    {
                        id = "TRIPLE_POWER", title = "Overcharged", description = "Activate Triple Shot plasma cannons",
                        icon = "BOLT"
                    },
                    new()
                    {
                        id = "NUKE_HERO", title = "Tactical Nuke", description = "Detonate an EMP Shockwave Bomb",
                        icon = "BOMB"
                    },
                    new()
                    {
                        id = "COMBO_5X", title = "Combo Master", description = "Achieve a 5x Combo Streak",
                        icon = "FIRE"
                    },
                    new()
                    {
                        id = "BOSS_SLAYER", title = "Boss Slayer", description = "Defeat the Red UFO Mothership",
                        icon = "UFO"
                    },
                    new()
                    {
                        id = "SCORE_200", title = "Star Veteran",
                        description = "Score 200 or more points in a single match", icon = "MEDAL"
                    }
                };
        }

        private void LoadAchievements()
        {
            foreach (var ach in achievements) ach.isUnlocked = PlayerPrefs.GetInt(PrefKeyPrefix + ach.id, 0) == 1;
        }

        public bool IsUnlocked(string id)
        {
            var ach = achievements.Find(a => a.id == id);
            return ach is { isUnlocked: true };
        }

        public void UnlockAchievement(string id)
        {
            var ach = achievements.Find(a => a.id == id);
            if (ach == null || ach.isUnlocked) return;
            ach.isUnlocked = true;
            PlayerPrefs.SetInt(PrefKeyPrefix + ach.id, 1);
            PlayerPrefs.Save();

            if (AudioManager.Instance) AudioManager.Instance.PlayPowerUp();

            if (UIManager.Instance) UIManager.Instance.ShowAchievementToast(ach.title, ach.description);
        }

        public void ResetAllAchievements()
        {
            foreach (var ach in achievements)
            {
                ach.isUnlocked = false;
                PlayerPrefs.DeleteKey(PrefKeyPrefix + ach.id);
            }

            PlayerPrefs.Save();
        }
    }
}
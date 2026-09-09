using System;
using System.Collections.Generic;
using System.Linq;
using Player;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    public enum PerkType
    {
        TeslaArc,
        CompanionDrone,
        AfterburnerBlast,
        ClusterRockets,
        SuperMagnet
    }

    [Serializable]
    public class PerkData
    {
        public PerkType perkType;
        public string perkName;
        public string description;
        public string iconSymbol; // Text emoji or tag
        public Color themeColor;

        public PerkData(PerkType type, string name, string desc, string symbol, Color color)
        {
            perkType = type;
            perkName = name;
            description = desc;
            iconSymbol = symbol;
            themeColor = color;
        }
    }

    public class PerkManager : MonoBehaviour
    {
        private static PerkManager _instance;

        private static readonly List<PerkData> AllPerks = new()
        {
            new PerkData(
                PerkType.TeslaArc,
                "TESLA LIGHTNING",
                "Lasers zap lightning to 2 nearby hostiles on hit (50% bonus shock damage)",
                "[ZAP]",
                new Color(0.2f, 0.9f, 1f)
            ),
            new PerkData(
                PerkType.CompanionDrone,
                "ATTACK DRONE",
                "Deploys an autonomous companion drone orbiting your ship that auto-fires plasma",
                "[DRONE]",
                new Color(0.2f, 1f, 0.5f)
            ),
            new PerkData(
                PerkType.AfterburnerBlast,
                "AFTERBURNER BLAST",
                "Dashing [Shift] ignites a fiery explosion trail that vaporizes bullets and enemies",
                "[BURN]",
                new Color(1f, 0.45f, 0.1f)
            ),
            new PerkData(
                PerkType.ClusterRockets,
                "CLUSTER WARHEADS",
                "Secondary missiles split into 3 micro-explosives upon detonation",
                "[CLUSTER]",
                new Color(1f, 0.85f, 0.1f)
            ),
            new PerkData(
                PerkType.SuperMagnet,
                "SUPER VACUUM",
                "Triples star & power-up attraction radius and speed",
                "[VORTEX]",
                new Color(0.85f, 0.3f, 1f)
            )
        };

        private readonly HashSet<PerkType> _activePerks = new();

        public static PerkManager Instance
        {
            get
            {
                if (!_instance) _instance = FindAnyObjectByType<PerkManager>(FindObjectsInactive.Include);
                if (_instance) return _instance;
                var go = new GameObject("PerkManager");
                _instance = go.AddComponent<PerkManager>();

                return _instance;
            }
            private set => _instance = value;
        }

        private void Awake()
        {
            Instance = this;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        public void ResetPerks()
        {
            _activePerks.Clear();
        }

        public bool HasPerk(PerkType perkType)
        {
            return _activePerks.Contains(perkType);
        }

        public List<PerkData> GetThreeRandomPerks()
        {
            var available = AllPerks.Where(p => !_activePerks.Contains(p.perkType)).ToList();

            // If all acquired, allow any
            if (available.Count < 3) available.AddRange(AllPerks);

            // Shuffle
            for (var i = 0; i < available.Count; i++)
            {
                var rand = Random.Range(i, available.Count);
                (available[i], available[rand]) = (available[rand], available[i]);
            }

            var result = new List<PerkData>();
            var addedTypes = new HashSet<PerkType>();
            foreach (var p in available.Where(p => addedTypes.Add(p.perkType)))
            {
                result.Add(p);
                if (result.Count >= 3) break;
            }

            return result;
        }

        public void ApplyPerk(PerkType perkType)
        {
            _activePerks.Add(perkType);

            var player = PlayerController.Instance;
            if (player != null) player.OnPerkAcquired(perkType);

            if (AudioManager.Instance != null) AudioManager.Instance.PlayPowerUp();

            if (player != null && player.floatingScorePrefab != null)
                FloatingScore.SpawnText(player.floatingScorePrefab, player.transform.position + Vector3.up * 1.2f,
                    "PERK UNLOCKED!", Color.yellow);
        }
    }
}
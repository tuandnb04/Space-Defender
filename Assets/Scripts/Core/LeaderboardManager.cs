using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [Serializable]
    public class LeaderboardEntry
    {
        public int score;
        public int wave;
        public string shipName;
        public string date;

        public LeaderboardEntry()
        {
        }

        public LeaderboardEntry(int s, int w, string ship, string d)
        {
            score = s;
            wave = w;
            shipName = ship;
            date = d;
        }
    }

    [Serializable]
    public class LeaderboardData
    {
        public List<LeaderboardEntry> entries = new();
    }

    public class LeaderboardManager : MonoBehaviour
    {
        private const string LeaderboardKey = "SD_LOCAL_LEADERBOARD";
        private static LeaderboardManager _instance;

        private LeaderboardData _data;

        public static LeaderboardManager Instance
        {
            get
            {
                if (_instance) return _instance;
                _instance = FindAnyObjectByType<LeaderboardManager>(FindObjectsInactive.Include);
                if (_instance) return _instance;
                var go = new GameObject("LeaderboardManager");
                _instance = go.AddComponent<LeaderboardManager>();
                return _instance;
            }
            private set => _instance = value;
        }

        private void Awake()
        {
            if (_instance == null)
                Instance = this;
            LoadLeaderboard();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _instance = null;
        }

        private void LoadLeaderboard()
        {
            var json = PlayerPrefs.GetString(LeaderboardKey, string.Empty);
            if (!string.IsNullOrEmpty(json))
                try
                {
                    _data = JsonUtility.FromJson<LeaderboardData>(json);
                }
                catch
                {
                    _data = null;
                }

            if (_data?.entries == null || _data.entries.Count == 0) CreateDefaultLeaderboard();
        }

        private void CreateDefaultLeaderboard()
        {
            _data = new LeaderboardData();
            _data.entries.Add(new LeaderboardEntry(2500, 8, "VANGUARD", "ACE"));
            _data.entries.Add(new LeaderboardEntry(1800, 6, "INTERCEPTOR", "PILOT"));
            _data.entries.Add(new LeaderboardEntry(1200, 4, "STRIKER", "CADET"));
            _data.entries.Add(new LeaderboardEntry(800, 3, "DREADNOUGHT", "ROOKIE"));
            _data.entries.Add(new LeaderboardEntry(400, 2, "VANGUARD", "TRAINEE"));
            Save();
        }

        public int SubmitScore(int score, int wave, string shipName)
        {
            if (_data == null) LoadLeaderboard();

            var dateStr = DateTime.Now.ToString("MM/dd");
            var newEntry = new LeaderboardEntry(score, wave, shipName, dateStr);

            var placedRank = -1;
            if (_data != null)
                for (var i = 0; i < _data.entries.Count; i++)
                {
                    if (score <= _data.entries[i].score) continue;
                    _data.entries.Insert(i, newEntry);
                    placedRank = i + 1;
                    break;
                }

            if (placedRank == -1 && _data != null && _data.entries.Count < 5)
            {
                _data.entries.Add(newEntry);
                placedRank = _data.entries.Count;
            }

            while (_data != null && _data.entries.Count > 5) _data.entries.RemoveAt(_data.entries.Count - 1);

            if (placedRank is < 1 or > 5) return 0; // Not in Top 5
            Save();
            return placedRank;
        }

        public List<LeaderboardEntry> GetTopEntries()
        {
            if (_data == null) LoadLeaderboard();
            return _data?.entries;
        }

        private void Save()
        {
            if (_data == null) return;
            var json = JsonUtility.ToJson(_data);
            PlayerPrefs.SetString(LeaderboardKey, json);
            PlayerPrefs.Save();
        }

        public void ResetLeaderboard()
        {
            PlayerPrefs.DeleteKey(LeaderboardKey);
            CreateDefaultLeaderboard();
        }
    }
}
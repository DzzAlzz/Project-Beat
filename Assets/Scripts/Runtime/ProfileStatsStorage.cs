using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Guardado local de estadisticas por perfil.
    /// Usa el selectedId del sistema de perfiles del menu principal y guarda
    /// los resultados finales por separado para no mezclar usuarios.
    /// </summary>
    public static class ProfileStatsStorage
    {
        private const string ProfilesPrefsKey = "ProjectBeat.Profiles.v1";
        private const string StatsPrefsPrefix = "ProjectBeat.ProfileStats.v1.";

        [Serializable]
        private class ProfileRef
        {
            public string id;
            public string name;
            public string createdAt;
            public bool partidaCreada;
            public int version = 1;
        }

        [Serializable]
        private class ProfileSaveDataRef
        {
            public List<ProfileRef> profiles = new List<ProfileRef>();
            public string selectedId;
        }

        [Serializable]
        public class LevelStats
        {
            public string levelName;
            public int timesPlayed;
            public int bestScore;
            public float bestAccuracy;
            public string bestRank;
            public int bestMaxCombo;
            public int lastScore;
            public float lastAccuracy;
            public string lastRank;
            public int lastMaxCombo;
            public int totalPerfect;
            public int totalGood;
            public int totalBad;
            public int totalMiss;
        }

        [Serializable]
        public class ProfileStatsData
        {
            public string profileId;
            public string profileName;
            public List<LevelStats> levels = new List<LevelStats>();
            public int version = 1;
        }

        public static bool TryGetCurrentProfile(out string profileId, out string profileName)
        {
            profileId = string.Empty;
            profileName = string.Empty;

            string json = PlayerPrefs.GetString(ProfilesPrefsKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return false;

            try
            {
                ProfileSaveDataRef data = JsonUtility.FromJson<ProfileSaveDataRef>(json);
                if (data == null || string.IsNullOrEmpty(data.selectedId) || data.profiles == null)
                    return false;

                for (int i = 0; i < data.profiles.Count; i++)
                {
                    ProfileRef p = data.profiles[i];
                    if (p != null && p.id == data.selectedId)
                    {
                        profileId = p.id;
                        profileName = string.IsNullOrEmpty(p.name) ? "SIN NOMBRE" : p.name;
                        return true;
                    }
                }
            }
            catch
            {
                profileId = string.Empty;
                profileName = string.Empty;
            }

            return false;
        }

        public static bool RecordLevelResult(string levelName, ScoreManager scoreManager)
        {
            if (scoreManager == null) return false;
            if (!TryGetCurrentProfile(out string profileId, out string profileName)) return false;

            if (string.IsNullOrWhiteSpace(levelName))
                levelName = "NIVEL";

            ProfileStatsData data = LoadStats(profileId, profileName);
            LevelStats level = FindOrCreateLevel(data, levelName.Trim());

            string rank = scoreManager.GetRank();
            int score = scoreManager.Score;
            float accuracy = scoreManager.Accuracy;
            int maxCombo = scoreManager.MaxCombo;

            level.timesPlayed++;
            level.lastScore = score;
            level.lastAccuracy = accuracy;
            level.lastRank = rank;
            level.lastMaxCombo = maxCombo;

            if (score > level.bestScore) level.bestScore = score;
            if (accuracy > level.bestAccuracy) level.bestAccuracy = accuracy;
            if (IsRankBetter(rank, level.bestRank)) level.bestRank = rank;
            if (maxCombo > level.bestMaxCombo) level.bestMaxCombo = maxCombo;

            level.totalPerfect += scoreManager.PerfectCount;
            level.totalGood += scoreManager.GoodCount;
            level.totalBad += scoreManager.BadCount;
            level.totalMiss += scoreManager.MissCount;

            SaveStats(data);
            return true;
        }

        public static ProfileStatsData LoadCurrentStats()
        {
            if (!TryGetCurrentProfile(out string profileId, out string profileName))
                return null;
            return LoadStats(profileId, profileName);
        }

        public static void DeleteStatsForProfile(string profileId)
        {
            if (string.IsNullOrEmpty(profileId)) return;
            PlayerPrefs.DeleteKey(StatsPrefsPrefix + profileId);
            PlayerPrefs.Save();
        }

        private static ProfileStatsData LoadStats(string profileId, string profileName)
        {
            string key = StatsPrefsPrefix + profileId;
            string json = PlayerPrefs.GetString(key, string.Empty);
            ProfileStatsData data = null;

            if (!string.IsNullOrEmpty(json))
            {
                try { data = JsonUtility.FromJson<ProfileStatsData>(json); }
                catch { data = null; }
            }

            if (data == null)
                data = new ProfileStatsData();

            data.profileId = profileId;
            data.profileName = profileName;
            if (data.levels == null)
                data.levels = new List<LevelStats>();
            return data;
        }

        private static void SaveStats(ProfileStatsData data)
        {
            if (data == null || string.IsNullOrEmpty(data.profileId)) return;
            PlayerPrefs.SetString(StatsPrefsPrefix + data.profileId, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        private static LevelStats FindOrCreateLevel(ProfileStatsData data, string levelName)
        {
            if (data.levels == null)
                data.levels = new List<LevelStats>();

            for (int i = 0; i < data.levels.Count; i++)
            {
                LevelStats s = data.levels[i];
                if (s != null && string.Equals(s.levelName, levelName, StringComparison.OrdinalIgnoreCase))
                    return s;
            }

            LevelStats created = new LevelStats
            {
                levelName = levelName,
                bestRank = "D",
                lastRank = "D"
            };
            data.levels.Add(created);
            return created;
        }

        private static bool IsRankBetter(string newRank, string oldRank)
        {
            return RankValue(newRank) > RankValue(oldRank);
        }

        private static int RankValue(string rank)
        {
            switch ((rank ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "S+": return 6;
                case "S": return 5;
                case "A": return 4;
                case "B": return 3;
                case "C": return 2;
                case "D": return 1;
                default: return 0;
            }
        }
    }
}

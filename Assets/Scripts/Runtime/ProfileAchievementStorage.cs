using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Avance 85 rehecho: logros locales por perfil.
    /// Guarda solo IDs de logros desbloqueados, separados por profileId.
    /// No altera gameplay, puntuacion, rango ni hit detection.
    /// </summary>
    public static class ProfileAchievementStorage
    {
        private const string AchievementsPrefsPrefix = "ProjectBeat.ProfileAchievements.v1.";

        [Serializable]
        public class AchievementDefinition
        {
            public string id;
            public string title;
            public string description;

            public AchievementDefinition(string id, string title, string description)
            {
                this.id = id;
                this.title = title;
                this.description = description;
            }
        }

        [Serializable]
        private class AchievementSaveData
        {
            public string profileId;
            public List<string> unlockedIds = new List<string>();
            public int version = 1;
        }

        private static readonly AchievementDefinition[] Definitions =
        {
            new AchievementDefinition("FIRST_GAME", "PRIMERA PARTIDA", "Crea una nueva partida en un perfil."),
            new AchievementDefinition("FIRST_BEAT", "PRIMER BEAT", "Completa cualquier nivel por primera vez."),
            new AchievementDefinition("COMBO_50", "RITMO INICIAL", "Alcanza combo 50 en cualquier nivel."),
            new AchievementDefinition("ACC_90", "BUENA PRECISION", "Termina un nivel con 90% o mas de precision."),
            new AchievementDefinition("RANK_A", "RANGO A", "Obtén rango A o superior en cualquier nivel."),
            new AchievementDefinition("RANK_S", "RANGO S", "Obtén rango S o superior en cualquier nivel."),
            new AchievementDefinition("FULL_COMBO", "FULL COMBO", "Completa un nivel sin fallos."),
            new AchievementDefinition("MASTER_RHYTHM", "MAESTRO DEL RITMO", "Completa todos los niveles disponibles.")
        };

        public static AchievementDefinition[] GetAllDefinitions()
        {
            return Definitions;
        }

        public static HashSet<string> GetUnlockedSetForCurrentProfile()
        {
            if (!ProfileStatsStorage.TryGetCurrentProfile(out string profileId, out _))
                return new HashSet<string>();

            AchievementSaveData data = Load(profileId);
            return new HashSet<string>(data.unlockedIds ?? new List<string>());
        }

        public static bool TryUnlockFirstGame(out AchievementDefinition unlocked)
        {
            return TryUnlock("FIRST_GAME", out unlocked);
        }

        public static List<AchievementDefinition> EvaluateAfterLevelComplete(ScoreManager scoreManager, int currentLevelIndex, int totalLevels)
        {
            List<AchievementDefinition> unlocked = new List<AchievementDefinition>();
            if (scoreManager == null) return unlocked;
            if (!ProfileStatsStorage.TryGetCurrentProfile(out _, out _, out bool partidaCreada) || !partidaCreada)
                return unlocked;

            TryAddUnlock("FIRST_BEAT", unlocked);

            if (scoreManager.MaxCombo >= 50)
                TryAddUnlock("COMBO_50", unlocked);

            if (scoreManager.Accuracy >= 90f)
                TryAddUnlock("ACC_90", unlocked);

            string rank = scoreManager.GetRank();
            if (RankValue(rank) >= RankValue("A"))
                TryAddUnlock("RANK_A", unlocked);

            if (RankValue(rank) >= RankValue("S"))
                TryAddUnlock("RANK_S", unlocked);

            if (scoreManager.TotalNotes > 0 && scoreManager.MissCount == 0)
                TryAddUnlock("FULL_COMBO", unlocked);

            if (totalLevels > 0 && currentLevelIndex >= totalLevels - 1)
                TryAddUnlock("MASTER_RHYTHM", unlocked);

            return unlocked;
        }

        public static bool IsUnlockedForCurrentProfile(string achievementId)
        {
            if (!ProfileStatsStorage.TryGetCurrentProfile(out string profileId, out _))
                return false;
            AchievementSaveData data = Load(profileId);
            return data.unlockedIds != null && data.unlockedIds.Contains(achievementId);
        }

        public static void DeleteAchievementsForProfile(string profileId)
        {
            if (string.IsNullOrEmpty(profileId)) return;
            PlayerPrefs.DeleteKey(AchievementsPrefsPrefix + profileId);
            PlayerPrefs.Save();
        }

        private static void TryAddUnlock(string id, List<AchievementDefinition> list)
        {
            if (TryUnlock(id, out AchievementDefinition definition) && definition != null)
                list.Add(definition);
        }

        private static bool TryUnlock(string achievementId, out AchievementDefinition unlocked)
        {
            unlocked = null;
            if (string.IsNullOrEmpty(achievementId)) return false;
            if (!ProfileStatsStorage.TryGetCurrentProfile(out string profileId, out _)) return false;

            AchievementDefinition definition = FindDefinition(achievementId);
            if (definition == null) return false;

            AchievementSaveData data = Load(profileId);
            if (data.unlockedIds == null)
                data.unlockedIds = new List<string>();

            if (data.unlockedIds.Contains(achievementId))
                return false;

            data.unlockedIds.Add(achievementId);
            Save(data);
            unlocked = definition;
            return true;
        }

        private static AchievementDefinition FindDefinition(string id)
        {
            for (int i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i] != null && Definitions[i].id == id)
                    return Definitions[i];
            }
            return null;
        }

        private static AchievementSaveData Load(string profileId)
        {
            string json = PlayerPrefs.GetString(AchievementsPrefsPrefix + profileId, string.Empty);
            AchievementSaveData data = null;
            if (!string.IsNullOrEmpty(json))
            {
                try { data = JsonUtility.FromJson<AchievementSaveData>(json); }
                catch { data = null; }
            }
            if (data == null)
                data = new AchievementSaveData();

            data.profileId = profileId;
            if (data.unlockedIds == null)
                data.unlockedIds = new List<string>();
            return data;
        }

        private static void Save(AchievementSaveData data)
        {
            if (data == null || string.IsNullOrEmpty(data.profileId)) return;
            PlayerPrefs.SetString(AchievementsPrefsPrefix + data.profileId, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
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

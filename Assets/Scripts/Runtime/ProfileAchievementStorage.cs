using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Logros locales por perfil. Avance 87: ampliacion de logros y easter eggs secretos.
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
            public bool secret;
            public string hiddenTitle;
            public string hiddenDescription;

            public AchievementDefinition(string id, string title, string description, bool secret = false, string hiddenTitle = "LOGRO SECRETO", string hiddenDescription = "???")
            {
                this.id = id;
                this.title = title;
                this.description = description;
                this.secret = secret;
                this.hiddenTitle = hiddenTitle;
                this.hiddenDescription = hiddenDescription;
            }
        }

        [Serializable]
        private class AchievementSaveData
        {
            public string profileId;
            public List<string> unlockedIds = new List<string>();
            public int version = 2;
        }

        private static readonly AchievementDefinition[] Definitions =
        {
            // Logros base del Avance 85
            new AchievementDefinition("FIRST_GAME", "PRIMERA PARTIDA", "Crea una nueva partida en un perfil."),
            new AchievementDefinition("FIRST_BEAT", "PRIMER BEAT", "Completa cualquier nivel por primera vez."),
            new AchievementDefinition("COMBO_50", "RITMO INICIAL", "Alcanza combo 50 en cualquier nivel."),
            new AchievementDefinition("ACC_90", "BUENA PRECISION", "Termina un nivel con 90% o mas de precision."),
            new AchievementDefinition("RANK_A", "RANGO A", "Obtén rango A o superior en cualquier nivel."),
            new AchievementDefinition("RANK_S", "RANGO S", "Obtén rango S o superior en cualquier nivel."),
            new AchievementDefinition("FULL_COMBO", "FULL COMBO", "Completa un nivel sin fallos."),
            new AchievementDefinition("MASTER_RHYTHM", "MAESTRO DEL RITMO", "Completa todos los niveles disponibles."),

            // Avance 87 - nuevos logros de progreso y rendimiento
            new AchievementDefinition("WELCOME_RHYTHM", "BIENVENIDO AL RITMO", "Entra al menu principal con un perfil cargado."),
            new AchievementDefinition("FIRST_SONG", "PRIMERA CANCION", "Inicia cualquier nivel por primera vez."),
            new AchievementDefinition("ACELERADA_CLEAR", "ACELERADA SUPERADA", "Completa el nivel ACELERADA."),
            new AchievementDefinition("FUNK_UNLOCKED", "FUNK DESBLOQUEADO", "Desbloquea RITMO FUNK."),
            new AchievementDefinition("SUMMER_BEAT", "SUMMER BEAT", "Completa SUMMER VACATION."),
            new AchievementDefinition("ESTRELLA_RHYTHM", "ESTRELLA DEL RITMO", "Completa ESTRELAR."),
            new AchievementDefinition("FRONTLINE_READY", "FRONTLINE READY", "Completa FRONTLINES."),
            new AchievementDefinition("REQUIEM_CLEAR", "REQUIEM COMPLETADO", "Completa REQUIEM."),
            new AchievementDefinition("HER_NAME_IS_CLEAR", "HER NAME IS", "Completa HER NAME IS."),
            new AchievementDefinition("COMBO_100", "COMBO 100", "Alcanza combo 100 en cualquier nivel."),
            new AchievementDefinition("COMBO_200", "COMBO 200", "Alcanza combo 200 en cualquier nivel."),
            new AchievementDefinition("ACC_95", "PRECISION 95", "Termina un nivel con 95% o mas de precision."),
            new AchievementDefinition("ACC_100", "PRECISION PERFECTA", "Termina un nivel con 100% de precision."),
            new AchievementDefinition("ALMOST_PERFECT", "CASI PERFECTO", "Termina un nivel con 0 fallos."),
            new AchievementDefinition("FEARLESS_FAIL", "SIN MIEDO AL FALLO", "Completa un nivel aunque tengas varios fallos."),
            new AchievementDefinition("PERSISTENT", "INSISTENTE", "Juega el mismo nivel 3 veces con el mismo perfil."),
            new AchievementDefinition("DEDICATED", "DEDICADO", "Juega 5 niveles en total con el mismo perfil."),
            new AchievementDefinition("STEADY_RHYTHM", "RITMO CONSTANTE", "Completa 3 niveles distintos en el mismo perfil."),
            new AchievementDefinition("RANK_COLLECTOR", "COLECCIONISTA DE RANGOS", "Obtén al menos un rango A, S o S+."),
            new AchievementDefinition("MASTER_PATH", "CAMINO DEL MAESTRO", "Ten todos los niveles completados al menos una vez."),

            // Avance 87 - easter eggs secretos por teclado
            new AchievementDefinition("SECRET_PROJECTBEAT", "SECRETO DEL BEAT", "Descubriste el codigo oculto de Project Beat.", true),
            new AchievementDefinition("SECRET_RITMO", "RITMO OCULTO", "El ritmo tambien tiene secretos.", true),
            new AchievementDefinition("SECRET_FEEL", "FEEL THE RHYTHM", "Encontraste el mensaje secreto del juego.", true)
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
            return TryUnlockById("FIRST_GAME", out unlocked);
        }

        public static bool TryUnlockById(string achievementId, out AchievementDefinition unlocked)
        {
            return TryUnlock(achievementId, out unlocked);
        }

        public static bool TryUnlockSecretCode(string code, out AchievementDefinition unlocked)
        {
            unlocked = null;
            string normalized = NormalizeCode(code);
            if (normalized == "PROJECTBEAT") return TryUnlock("SECRET_PROJECTBEAT", out unlocked);
            if (normalized == "RITMO") return TryUnlock("SECRET_RITMO", out unlocked);
            if (normalized == "FEELTHERHYTHM") return TryUnlock("SECRET_FEEL", out unlocked);
            return false;
        }

        public static List<AchievementDefinition> EvaluateAfterLevelComplete(ScoreManager scoreManager, int currentLevelIndex, int totalLevels, string levelName = null)
        {
            List<AchievementDefinition> unlocked = new List<AchievementDefinition>();
            if (scoreManager == null) return unlocked;
            if (!ProfileStatsStorage.TryGetCurrentProfile(out _, out _, out bool partidaCreada) || !partidaCreada)
                return unlocked;

            TryAddUnlock("FIRST_BEAT", unlocked);
            TryAddUnlock("FIRST_SONG", unlocked);

            if (scoreManager.MaxCombo >= 50)
                TryAddUnlock("COMBO_50", unlocked);
            if (scoreManager.MaxCombo >= 100)
                TryAddUnlock("COMBO_100", unlocked);
            if (scoreManager.MaxCombo >= 200)
                TryAddUnlock("COMBO_200", unlocked);

            if (scoreManager.Accuracy >= 90f)
                TryAddUnlock("ACC_90", unlocked);
            if (scoreManager.Accuracy >= 95f)
                TryAddUnlock("ACC_95", unlocked);
            if (scoreManager.Accuracy >= 99.99f)
                TryAddUnlock("ACC_100", unlocked);

            string rank = scoreManager.GetRank();
            if (RankValue(rank) >= RankValue("A"))
            {
                TryAddUnlock("RANK_A", unlocked);
                TryAddUnlock("RANK_COLLECTOR", unlocked);
            }
            if (RankValue(rank) >= RankValue("S"))
                TryAddUnlock("RANK_S", unlocked);

            if (scoreManager.TotalNotes > 0 && scoreManager.MissCount == 0)
            {
                TryAddUnlock("FULL_COMBO", unlocked);
                TryAddUnlock("ALMOST_PERFECT", unlocked);
            }

            if (scoreManager.MissCount >= 10)
                TryAddUnlock("FEARLESS_FAIL", unlocked);

            UnlockLevelSpecificAchievements(levelName, currentLevelIndex, unlocked);
            UnlockAggregateAchievements(totalLevels, unlocked);

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

        private static void UnlockLevelSpecificAchievements(string levelName, int currentLevelIndex, List<AchievementDefinition> unlocked)
        {
            string n = NormalizeLevelName(levelName);

            if (n.Contains("ACELERADA"))
            {
                TryAddUnlock("ACELERADA_CLEAR", unlocked);
                TryAddUnlock("FUNK_UNLOCKED", unlocked);
            }
            if (n.Contains("RITMOFUNK") || n.Contains("FUNK"))
                TryAddUnlock("FUNK_UNLOCKED", unlocked);
            if (n.Contains("SUMMER") || n.Contains("VACATION"))
                TryAddUnlock("SUMMER_BEAT", unlocked);
            if (n.Contains("ESTRELAR") || n.Contains("ESTRELLA"))
                TryAddUnlock("ESTRELLA_RHYTHM", unlocked);
            if (n.Contains("FRONTLINES") || n.Contains("FRONTLINE"))
                TryAddUnlock("FRONTLINE_READY", unlocked);
            if (n.Contains("REQUIEM"))
                TryAddUnlock("REQUIEM_CLEAR", unlocked);
            if (n.Contains("HERNAMEIS") || n.Contains("HERNAME") || n.Contains("HER"))
                TryAddUnlock("HER_NAME_IS_CLEAR", unlocked);
        }

        private static void UnlockAggregateAchievements(int totalLevels, List<AchievementDefinition> unlocked)
        {
            ProfileStatsStorage.ProfileStatsData stats = ProfileStatsStorage.LoadCurrentStats();
            if (stats == null || stats.levels == null) return;

            int totalPlays = 0;
            int completedDistinct = 0;
            for (int i = 0; i < stats.levels.Count; i++)
            {
                ProfileStatsStorage.LevelStats s = stats.levels[i];
                if (s == null) continue;
                totalPlays += Mathf.Max(0, s.timesPlayed);
                if (s.timesPlayed > 0) completedDistinct++;
                if (s.timesPlayed >= 3) TryAddUnlock("PERSISTENT", unlocked);
            }

            if (totalPlays >= 5) TryAddUnlock("DEDICATED", unlocked);
            if (completedDistinct >= 3) TryAddUnlock("STEADY_RHYTHM", unlocked);

            int neededLevels = Mathf.Max(1, totalLevels - 1); // excluye tutorial cuando existe.
            if (totalLevels <= 0) neededLevels = 7;
            if (completedDistinct >= neededLevels)
            {
                TryAddUnlock("MASTER_PATH", unlocked);
                TryAddUnlock("MASTER_RHYTHM", unlocked);
            }
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
            if (!ProfileStatsStorage.TryGetCurrentProfile(out string profileId, out _, out bool partidaCreada) || !partidaCreada) return false;

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

        private static string NormalizeLevelName(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.ToUpperInvariant().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).Replace(".", string.Empty);
        }

        private static string NormalizeCode(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.ToUpperInvariant().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty);
        }
    }
}

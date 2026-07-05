using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Singleton estable que mantiene la lista de niveles y el nivel seleccionado.
    /// Avance 48: ya no depende de PlayerPrefs para seleccionar niveles, porque eso
    /// provocaba estados distintos entre computadores o sesiones con cache previa.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [SerializeField] private LevelData[] levels;
        [SerializeField] private int defaultArcadeLevelIndex = 1;

        public LevelData[] Levels => levels;
        public int CurrentLevelIndex { get; private set; } = 1;

        private static int cachedLevelIndex = 1;

        public LevelData CurrentLevel
        {
            get
            {
                if (levels == null || levels.Length == 0) return null;
                CurrentLevelIndex = Mathf.Clamp(CurrentLevelIndex, 0, levels.Length - 1);
                return levels[CurrentLevelIndex];
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // Avance 48: si sobrevive un LevelManager antiguo sin datos o con datos
                // incompletos, se actualiza con los datos del nuevo antes de destruirlo.
                Instance.AdoptLevelsIfNeeded(levels);
                Instance.SetLevel(cachedLevelIndex);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            AdoptLevelsIfNeeded(levels);
            SetLevel(cachedLevelIndex);
        }

        private void AdoptLevelsIfNeeded(LevelData[] incomingLevels)
        {
            if ((levels == null || levels.Length == 0) && incomingLevels != null && incomingLevels.Length > 0)
                levels = incomingLevels;

            if (levels != null && levels.Length > 0)
            {
                defaultArcadeLevelIndex = Mathf.Clamp(defaultArcadeLevelIndex, 0, levels.Length - 1);
                CurrentLevelIndex = Mathf.Clamp(CurrentLevelIndex, 0, levels.Length - 1);
                cachedLevelIndex = Mathf.Clamp(cachedLevelIndex, 0, levels.Length - 1);
            }
        }

        public int GetFirstArcadeLevelIndex()
        {
            if (levels == null || levels.Length == 0) return 0;
            return Mathf.Clamp(defaultArcadeLevelIndex, 0, levels.Length - 1);
        }

        public void SelectFirstArcadeLevel()
        {
            SetLevel(GetFirstArcadeLevelIndex());
        }

        public void ResetToSafeDefault()
        {
            SelectFirstArcadeLevel();
        }

        public void SetLevel(int index)
        {
            if (levels == null || levels.Length == 0)
            {
                CurrentLevelIndex = Mathf.Max(0, index);
                cachedLevelIndex = CurrentLevelIndex;
                return;
            }

            CurrentLevelIndex = Mathf.Clamp(index, 0, levels.Length - 1);
            cachedLevelIndex = CurrentLevelIndex;
        }

        public void NextLevel()
        {
            if (levels == null || levels.Length == 0) return;
            CurrentLevelIndex = (CurrentLevelIndex + 1) % levels.Length;
            cachedLevelIndex = CurrentLevelIndex;
        }

        public void PreviousLevel()
        {
            if (levels == null || levels.Length == 0) return;
            CurrentLevelIndex = (CurrentLevelIndex - 1 + levels.Length) % levels.Length;
            cachedLevelIndex = CurrentLevelIndex;
        }
    }
}

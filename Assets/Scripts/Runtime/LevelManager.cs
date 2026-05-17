using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Singleton that holds the list of all levels and remembers which one is selected.
    /// Persists between scene reloads via DontDestroyOnLoad.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [SerializeField] private LevelData[] levels;

        public LevelData[] Levels => levels;
        public int CurrentLevelIndex { get; private set; } = 0;

        public LevelData CurrentLevel => (levels != null && levels.Length > 0)
            ? levels[CurrentLevelIndex]
            : null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetLevel(int index)
        {
            if (levels == null || levels.Length == 0) return;
            CurrentLevelIndex = Mathf.Clamp(index, 0, levels.Length - 1);
        }

        public void NextLevel()
        {
            if (levels == null || levels.Length == 0) return;
            CurrentLevelIndex = (CurrentLevelIndex + 1) % levels.Length;
        }

        public void PreviousLevel()
        {
            if (levels == null || levels.Length == 0) return;
            CurrentLevelIndex = (CurrentLevelIndex - 1 + levels.Length) % levels.Length;
        }
    }
}

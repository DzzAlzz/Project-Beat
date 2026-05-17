using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectBeat.Runtime
{
    public class GameController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Conductor     conductor;
        [SerializeField] private BeatmapPlayer beatmapPlayer;
        [SerializeField] private ScoreManager  scoreManager;
        [SerializeField] private GameplayUI    gameplayUI;
        [SerializeField] private LaneInput[]   lanes;
        [SerializeField] private Transform[]   laneSpawnPoints;
        [SerializeField] private Transform[]   laneHitPoints;
        [SerializeField] private GameObject    notePrefab;
        [SerializeField] private GameObject    hitEffectPrefab;
        [SerializeField] private PauseMenu     pauseMenu;
        [SerializeField] private BackgroundThemeController backgroundThemeController;

        [Header("Beatmap (Fallback)")]
        [SerializeField] private TextAsset beatmapJson;
        [SerializeField] private AudioClip songOverride;

        [Header("Hit Windows (seconds)")]
        [SerializeField] private float perfectWindow = 0.055f;
        [SerializeField] private float goodWindow    = 0.110f;
        [SerializeField] private float badWindow     = 0.160f;

        [Header("Visual — ACELERADA Orange Palette")]
        [SerializeField] private Color[] laneColors = new Color[]
        {
            new Color(1.00f, 0.55f, 0.05f),
            new Color(1.00f, 0.35f, 0.05f),
            new Color(1.00f, 0.80f, 0.10f),
            new Color(1.00f, 0.30f, 0.00f)
        };

        public BeatmapData Beatmap           { get; private set; }
        public Conductor   Conductor         => conductor;
        public float       BadWindow         => badWindow;
        public bool        IsGameplayRunning { get; private set; }

        private bool finished;
        private static readonly int[] MilestoneCombos = { 10, 25, 50, 100 };

        private void Start()
        {
            LevelData ld   = LevelManager.Instance?.CurrentLevel;
            TextAsset json  = ld?.beatmapJson ?? beatmapJson;
            AudioClip audio = ld?.audioClip   ?? songOverride;
            Color[]   cols  = (ld?.laneColors != null && ld.laneColors.Length >= 4)
                              ? ld.laneColors : laneColors;

            if (backgroundThemeController != null && ld != null)
                backgroundThemeController.ApplyTheme(ld.backgroundTheme);

            KeyCode[] keys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };
            for (int i = 0; i < lanes.Length; i++)
                lanes[i].Initialize(this, i, keys[Mathf.Clamp(i, 0, keys.Length - 1)],
                                    cols[Mathf.Clamp(i, 0, cols.Length - 1)]);
            laneColors = cols;

            beatmapPlayer.Initialize(this, json, audio);
            if (Beatmap != null)
            {
                scoreManager.Initialize(Beatmap.notes.Length);
                gameplayUI.Initialize(Beatmap);
                conductor.StartSong();
                IsGameplayRunning = true;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && (finished || !IsGameplayRunning))
                RestartScene();
        }

        public void SetBeatmap(BeatmapData beatmap, AudioClip overrideClip)
        {
            Beatmap = beatmap;
            // overrideClip viene de LevelData.audioClip o del campo songOverride.
            // No hay fallback a Resources.Load porque no hay carpeta Resources/ en el proyecto.
            conductor.Initialize(beatmap.bpm, beatmap.offset, overrideClip);
        }

        public void SpawnNote(int laneIndex, float hitTime)
        {
            if (laneIndex < 0 || laneIndex >= lanes.Length || notePrefab == null) return;
            var note = Instantiate(notePrefab).GetComponent<NoteObject>();
            Color c  = laneColors[Mathf.Clamp(laneIndex, 0, laneColors.Length - 1)];
            note.Initialize(this, lanes[laneIndex], hitTime,
                            laneSpawnPoints[laneIndex].position,
                            laneHitPoints[laneIndex].position, c);
        }

        public JudgementType GetJudgement(float delta)
        {
            if (delta <= perfectWindow) return JudgementType.Perfect;
            if (delta <= goodWindow)    return JudgementType.Good;
            if (delta <= badWindow)     return JudgementType.Bad;
            return JudgementType.None;
        }

        // Full version with laneIndex
        public void RegisterJudgement(JudgementType judgement, Vector3 position, int laneIndex)
        {
            scoreManager.Register(judgement);

            string label = judgement switch
            {
                JudgementType.Perfect => "PERFECT",
                JudgementType.Good    => "GOOD",
                JudgementType.Bad     => "BAD",
                JudgementType.Miss    => "MISS",
                _                     => ""
            };
            gameplayUI.ShowJudgement(label);
            gameplayUI.UpdateStats(scoreManager.Score, scoreManager.Combo,
                                   scoreManager.Accuracy, scoreManager.Multiplier);

            // Combo milestones
            foreach (int m in MilestoneCombos)
            {
                if (scoreManager.Combo == m)
                {
                    gameplayUI.TriggerComboPop();
                    gameplayUI.ShowMilestoneBanner(m);
                }
            }

            bool isPerfect = judgement == JudgementType.Perfect;

            // Lane glow flash
            if (isPerfect && laneIndex >= 0 && laneIndex < lanes.Length)
                lanes[laneIndex].TriggerPerfectFlash();

            // Hit effects
            if (hitEffectPrefab != null && judgement != JudgementType.Miss && judgement != JudgementType.Bad)
            {
                Color fxColor = isPerfect
                    ? new Color(1f, 0.92f, 0.25f)
                    : new Color(1f, 0.60f, 0.90f);
                SpawnEffect(position, fxColor, isPerfect);
                if (isPerfect)
                    SpawnEffect(position, new Color(1f, 0.65f, 0.10f, 0.55f), false, 1.5f);
            }
        }

        // Backwards-compatible overload
        public void RegisterJudgement(JudgementType judgement, Vector3 position)
            => RegisterJudgement(judgement, position, -1);

        public void RegisterEmptyTap()
            => gameplayUI.ShowJudgement("MISS");

        public void FinishSong()
        {
            if (finished) return;
            finished = true;
            IsGameplayRunning = false;
            gameplayUI.ShowResults(scoreManager, Beatmap);
        }

        public void RestartScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void PauseAudio(bool pause) => conductor?.SetPaused(pause);

        private void SpawnEffect(Vector3 pos, Color color, bool perfect, float scaleMult = 1f)
        {
            if (hitEffectPrefab == null) return;
            var fx = Instantiate(hitEffectPrefab, pos, Quaternion.identity).GetComponent<HitEffect>();
            fx?.Initialize(color, perfect, scaleMult);
        }
    }
}

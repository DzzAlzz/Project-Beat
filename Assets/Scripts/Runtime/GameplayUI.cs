using TMPro;
using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Avance 12: HUD intermedio mas limpio: informacion de cancion, score, combo,
    /// precision y feedback visual, sin pantalla final moderna avanzada.
    /// </summary>
    public class GameplayUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private TMP_Text songText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private TMP_Text accuracyText;
        [SerializeField] private TMP_Text judgementText;
        [SerializeField] private TMP_Text multiplierText;
        [SerializeField] private TMP_Text milestoneText;

        [Header("Results")]
        [SerializeField] private CanvasGroup resultGroup;
        [SerializeField] private TMP_Text    resultTitleText;
        [SerializeField] private TMP_Text    resultBodyText;

        // ── Colours ───────────────────────────────────────────────────────
        private static readonly Color ColPerfect = new Color(1.00f, 0.95f, 0.20f);
        private static readonly Color ColGood    = new Color(0.40f, 1.00f, 0.60f);
        private static readonly Color ColBad     = new Color(1.00f, 0.55f, 0.10f);
        private static readonly Color ColMiss    = new Color(1.00f, 0.25f, 0.25f);

        // ── Timers ────────────────────────────────────────────────────────
        private float judgementTimer;
        private float comboPopTimer;
        private float milestoneTimer;
        private float scorePopTimer;
        private int   displayedScore;
        private int   targetScore;

        private const float JudgeFade     = 0.65f;
        private const float ComboPop      = 0.30f;
        private const float MilestoneDur  = 1.2f;
        private const float ScorePopDur   = 0.20f;

        // ── Init ──────────────────────────────────────────────────────────
        public void Initialize(BeatmapData beatmap)
        {
            if (songText != null)
                songText.text = $"<b><color=#ff8800>{beatmap.songName}</color></b>\n" +
                                $"<size=18><color=#ffcc66>{beatmap.artist}</color></size>";

            if (resultGroup != null)
            {
                resultGroup.alpha = 0f;
                resultGroup.interactable = resultGroup.blocksRaycasts = false;
            }

            if (multiplierText != null) multiplierText.text = "";
            if (milestoneText  != null) milestoneText.text  = "";
            displayedScore = targetScore = 0;
            ShowJudgement("");
            UpdateStats(0, 0, 100f, 1);
        }

        // ── Stats ─────────────────────────────────────────────────────────
        public void UpdateStats(int score, int combo, float accuracy, int multiplier = 1)
        {
            // Score animates to target
            targetScore = score;
            if (score > displayedScore) scorePopTimer = ScorePopDur;

            // Combo pop size
            float sz = comboPopTimer > 0f
                ? Mathf.Lerp(44f, 58f, comboPopTimer / ComboPop)
                : 44f;

            if (comboText != null)
            {
                if (combo > 1)
                    comboText.text = $"<size=18><color=#ffeeaa>COMBO</color></size>\n" +
                                     $"<b><size={sz:0}><color=#ffdd00>{combo}</color><color=#ff9900>x</color></size></b>";
                else
                    comboText.text = "";
            }

            if (accuracyText != null)
                accuracyText.text = $"<size=18><color=#ffaa44>PRECISION</color></size>\n" +
                                    $"<b><color=#ffffff>{accuracy:0.00}%</color></b>";

            // Multiplier badge
            if (multiplierText != null)
            {
                if (multiplier > 1)
                {
                    string mc = multiplier switch
                    {
                        2 => "#88ff88", 3 => "#ffdd00", 4 => "#ff55ff", _ => "#ffffff"
                    };
                    multiplierText.text = $"<b><color={mc}>x{multiplier}</color></b>";
                }
                else multiplierText.text = "";
            }
        }

        // ── Judgement ─────────────────────────────────────────────────────
        public void ShowJudgement(string msg)
        {
            if (judgementText == null) return;
            Color c = msg switch
            {
                "PERFECT" => ColPerfect, "GOOD" => ColGood,
                "BAD"     => ColBad,     "MISS" => ColMiss,
                _         => Color.white
            };
            string styled = msg switch
            {
                "PERFECT" => "<b><size=54>* PERFECTO *</size></b>",
                "GOOD"    => "<b><size=46>• BIEN</size></b>",
                "BAD"     => "<b><size=38>/\\ MAL</size></b>",
                "MISS"    => "<b><size=36>X FALLO</size></b>",
                _         => ""
            };
            judgementText.text  = styled;
            judgementText.color = new Color(c.r, c.g, c.b, 1f);
            judgementTimer      = JudgeFade;
        }

        // ── Combo pop ─────────────────────────────────────────────────────
        public void TriggerComboPop()
        {
            comboPopTimer = ComboPop;
        }

        // ── Milestone banner ──────────────────────────────────────────────
        public void ShowMilestoneBanner(int combo)
        {
            if (milestoneText == null) return;
            string banner = combo switch
            {
                10  => "<b><size=38><color=#88ff88>COMBO x10!</color></size></b>",
                25  => "<b><size=42><color=#ffdd00>COMBO x25!</color></size></b>",
                50  => "<b><size=46><color=#ff88ff>COMBO x50!!</color></size></b>",
                100 => "<b><size=52><color=#ff4444>COMBO x100!!!</color></size></b>",
                _   => ""
            };
            milestoneText.text  = banner;
            milestoneTimer      = MilestoneDur;
            Color mc = milestoneText.color; mc.a = 1f;
            milestoneText.color = mc;
        }

        // ── Results ───────────────────────────────────────────────────────
        public void ShowResults(ScoreManager sm, BeatmapData beatmap)
        {
            if (resultGroup == null) return;
            resultGroup.alpha = 1f;
            resultGroup.interactable = resultGroup.blocksRaycasts = true;

            string rank = sm.GetRank();
            Color rc = rank switch
            {
                "S+" => new Color(1f, 0.5f, 1f),  "S" => new Color(1f, 0.88f, 0.08f),
                "A"  => new Color(0.4f, 1f, 0.5f), "B" => new Color(0.4f, 0.7f, 1f),
                "C"  => new Color(1f, 0.6f, 0.2f),  _  => new Color(1f, 0.3f, 0.3f)
            };
            string rh   = ColorUtility.ToHtmlStringRGB(rc);
            string fc   = sm.IsFullCombo ? "\n<size=26><color=#ffdd00>* FULL COMBO *</color></size>" : "";

            if (resultTitleText != null)
                resultTitleText.text =
                    $"<b><color=#ff8800>{beatmap.songName}</color></b>\n" +
                    $"<size=64><color=#{rh}>{rank}</color></size>{fc}";

            if (resultBodyText != null)
                resultBodyText.text =
                    $"<color=#ffdd00>Puntuación</color>  <b>{sm.Score:N0}</b>\n" +
                    $"<color=#ffaa44>Precisión</color>   <b>{sm.Accuracy:0.00}%</b>\n" +
                    $"<color=#aaffaa>Máx. Combo</color>  <b>{sm.MaxCombo}</b>\n\n" +
                    $"<color=#ffff88>* Perfecto</color>  {sm.PerfectCount}\n" +
                    $"<color=#88ff88>• Bien</color>      {sm.GoodCount}\n" +
                    $"<color=#ffaa44>/\\ Mal</color>       {sm.BadCount}\n" +
                    $"<color=#ff4444>X Fallo</color>     {sm.MissCount}\n\n" +
                    $"<size=22><color=#aaaaaa>[ R ] Reiniciar   [ ESC ] Pausa</color></size>";
        }

        // ── Update ────────────────────────────────────────────────────────
        private void Update()
        {
            // Judgement fade
            if (judgementText != null && judgementTimer > 0f)
            {
                judgementTimer -= Time.deltaTime;
                Color c = judgementText.color;
                c.a = Mathf.Clamp01(judgementTimer / (JudgeFade * 0.35f));
                judgementText.color = c;
                if (judgementTimer <= 0f) judgementText.text = "";
            }

            // Combo pop decay
            if (comboPopTimer > 0f) comboPopTimer -= Time.deltaTime;

            // Milestone banner fade
            if (milestoneText != null && milestoneTimer > 0f)
            {
                milestoneTimer -= Time.deltaTime;
                Color c = milestoneText.color;
                c.a = Mathf.Clamp01(milestoneTimer / (MilestoneDur * 0.3f));
                milestoneText.color = c;
                if (milestoneTimer <= 0f) milestoneText.text = "";
            }

            // Score smooth count-up
            if (scoreText != null)
            {
                if (scorePopTimer > 0f)
                {
                    scorePopTimer  -= Time.deltaTime;
                    displayedScore = (int)Mathf.Lerp(displayedScore, targetScore, 1f - scorePopTimer / ScorePopDur);
                }
                else displayedScore = targetScore;

                scoreText.text = $"<size=18><color=#ffaa44>PUNTUACIÓN</color></size>\n" +
                                 $"<b><color=#ffffff>{displayedScore:0000000}</color></b>";
            }
        }
    }
}

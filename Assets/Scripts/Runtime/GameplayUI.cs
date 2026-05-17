using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// In-game HUD v1.4 — animated score counter, glowing combo ring, 
    /// judgement with scale pop, milestone banner, multiplier badge,
    /// cinematic results screen with animated rank reveal.
    /// </summary>
    public class GameplayUI : MonoBehaviour
    {
        [Header("HUD — Top")]
        [SerializeField] private TMP_Text songText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text multiplierText;

        [Header("HUD — Center Play Area")]
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private TMP_Text judgementText;
        [SerializeField] private TMP_Text milestoneText;

        [Header("HUD — Bottom")]
        [SerializeField] private TMP_Text accuracyText;
        [SerializeField] private TMP_Text keyHintText;

        [Header("Results Screen")]
        [SerializeField] private CanvasGroup resultGroup;
        [SerializeField] private TMP_Text    resultTitleText;
        [SerializeField] private TMP_Text    resultBodyText;
        [SerializeField] private Image       resultBg;
        [SerializeField] private RectTransform resultPanelRect;   // for scale-in animation

        // ── Judgement colours ─────────────────────────────────────────────
        private static readonly Color ColPerfect = new Color(1.00f, 0.95f, 0.20f);
        private static readonly Color ColGood    = new Color(0.35f, 1.00f, 0.55f);
        private static readonly Color ColBad     = new Color(1.00f, 0.50f, 0.05f);
        private static readonly Color ColMiss    = new Color(1.00f, 0.20f, 0.20f);

        // ── Timers ────────────────────────────────────────────────────────
        private float judgTimer;
        private float judgScaleTimer;
        private float comboPopTimer;
        private float milestoneTimer;
        private float scoreAnim;
        private float resultOpenTimer;
        private bool  resultOpening;
        private int   displayedScore;
        private int   targetScore;

        private const float JudgDur       = 0.70f;
        private const float JudgScaleDur  = 0.12f;
        private const float ComboDur      = 0.28f;
        private const float MilestoneDur  = 1.40f;
        private const float ResultOpenDur = 0.35f;
        private const float ScoreAnimSpeed= 8f;

        // ── Init ──────────────────────────────────────────────────────────
        public void Initialize(BeatmapData beatmap)
        {
            if (songText != null)
                songText.text =
                    $"<b><color=#ff8800>{beatmap.songName}</color></b>\n" +
                    $"<size=18><color=#cc6622>{beatmap.artist}</color></size>";

            if (keyHintText != null)
                keyHintText.text =
                    "<size=16><color=#442200>[ D ]  [ F ]  [ J ]  [ K ]</color></size>";

            if (resultGroup != null)
            {
                resultGroup.alpha = 0f;
                resultGroup.interactable = resultGroup.blocksRaycasts = false;
            }
            if (resultPanelRect != null)
                resultPanelRect.localScale = Vector3.one * 0.7f;

            if (multiplierText != null) multiplierText.text = "";
            if (milestoneText  != null) milestoneText.text  = "";
            displayedScore = targetScore = 0;
            ShowJudgement("");
            UpdateStats(0, 0, 100f, 1);
        }

        // ── Stats update ──────────────────────────────────────────────────
        public void UpdateStats(int score, int combo, float accuracy, int multiplier = 1)
        {
            targetScore = score;

            // Combo with pop-scale
            float csz = comboPopTimer > 0f
                ? Mathf.Lerp(44f, 62f, comboPopTimer / ComboDur)
                : 44f;

            if (comboText != null)
            {
                comboText.text = combo > 1
                    ? $"<b><size={csz:0}><color=#ffdd00>{combo}" +
                      $"</color><color=#ff8800>x</color></size></b> " +
                      $"<size=22><color=#ffcc66>COMBO</color></size>"
                    : "";
            }

            if (accuracyText != null)
            {
                string accColor = accuracy >= 97f ? "#ffdd00"
                                : accuracy >= 90f ? "#88ff88"
                                : accuracy >= 80f ? "#88ccff"
                                : "#ff8844";
                accuracyText.text =
                    $"<size=16><color=#664422>PREC</color></size> " +
                    $"<b><color={accColor}>{accuracy:0.00}%</color></b>";
            }

            // Multiplier badge — colour escalates with tier
            if (multiplierText != null)
            {
                multiplierText.text = multiplier > 1
                    ? multiplier switch
                    {
                        2 => "<b><color=#88ff88>\u2605 x2</color></b>",
                        3 => "<b><color=#ffdd00>\u2605\u2605 x3</color></b>",
                        4 => "<b><color=#ff44ff>\u2605\u2605\u2605\u2605 x4</color></b>",
                        _ => $"<b><color=#ffffff>x{multiplier}</color></b>"
                    }
                    : "";
            }
        }

        // ── Judgement pop ─────────────────────────────────────────────────
        public void ShowJudgement(string msg)
        {
            if (judgementText == null) return;

            Color c = msg switch
            {
                "PERFECT" => ColPerfect, "GOOD" => ColGood,
                "BAD"     => ColBad,     "MISS" => ColMiss,
                _         => Color.clear
            };

            // Stylised with surrounding decorators
            string styled = msg switch
            {
                "PERFECT" => "<b><size=52>\u2726 PERFECTO \u2726</size></b>",
                "GOOD"    => "<b><size=44>\u25CF BIEN \u25CF</size></b>",
                "BAD"     => "<b><size=36>\u25C6 MAL</size></b>",
                "MISS"    => "<b><size=34>\u2715 FALLO</size></b>",
                _         => ""
            };

            judgementText.text  = styled;
            judgementText.color = new Color(c.r, c.g, c.b, 1f);
            judgTimer           = JudgDur;
            judgScaleTimer      = JudgScaleDur;
        }

        // ── Combo pop trigger ─────────────────────────────────────────────
        public void TriggerComboPop() { comboPopTimer = ComboDur; }

        // ── Milestone banner ──────────────────────────────────────────────
        public void ShowMilestoneBanner(int combo)
        {
            if (milestoneText == null) return;
            string t = combo switch
            {
                10  => "<b><size=34><color=#88ff88>\ud83d\udd25 x10 COMBO!</color></size></b>",
                25  => "<b><size=38><color=#ffdd00>\u26a1 x25 COMBO!!</color></size></b>",
                50  => "<b><size=42><color=#ff88ff>\u2605 x50 COMBO!!!</color></size></b>",
                100 => "<b><size=48><color=#ff3333>\ud83d\udca5 x100 COMBO!!!!</color></size></b>",
                _   => ""
            };
            milestoneText.text  = t;
            milestoneTimer      = MilestoneDur;
            Color mc = milestoneText.color; mc.a = 1f; milestoneText.color = mc;
        }

        // ── Results screen ────────────────────────────────────────────────
        public void ShowResults(ScoreManager sm, BeatmapData beatmap)
        {
            if (resultGroup == null) return;
            resultGroup.alpha = 1f;
            resultGroup.interactable = resultGroup.blocksRaycasts = true;
            resultOpening    = true;
            resultOpenTimer  = 0f;

            string rank = sm.GetRank();
            Color rc = rank switch
            {
                "S+" => new Color(1.0f, 0.4f, 1.0f),
                "S"  => new Color(1.0f, 0.9f, 0.1f),
                "A"  => new Color(0.4f, 1.0f, 0.5f),
                "B"  => new Color(0.4f, 0.7f, 1.0f),
                "C"  => new Color(1.0f, 0.6f, 0.2f),
                _    => new Color(1.0f, 0.3f, 0.3f)
            };
            string rh = ColorUtility.ToHtmlStringRGB(rc);
            string fc = sm.IsFullCombo
                ? "\n<size=24><color=#ffdd00>\u2605 FULL COMBO \u2605</color></size>"
                : "";

            // Result BG tint based on rank
            if (resultBg != null)
                resultBg.color = new Color(rc.r * 0.06f, rc.g * 0.04f, rc.b * 0.05f, 0.95f);

            if (resultTitleText != null)
                resultTitleText.text =
                    $"<b><color=#ff8800>{beatmap.songName}</color></b>\n" +
                    $"<b><color=#cc4400>{beatmap.artist}</color></b>\n" +
                    $"<size=72><color=#{rh}>{rank}</color></size>{fc}";

            if (resultBodyText != null)
                resultBodyText.text =
                    $"<color=#ffdd00>\u2726 Puntuación</color>   <b><color=#ffffff>{sm.Score:N0}</color></b>\n" +
                    $"<color=#ffaa44>\u25C6 Precisión</color>    <b><color=#ffffff>{sm.Accuracy:0.00}%</color></b>\n" +
                    $"<color=#aaffaa>\u221e Máx.Combo</color>    <b><color=#ffffff>{sm.MaxCombo}</color></b>\n" +
                    $"\n" +
                    $"<color=#ffff88>\u2726</color> Perfecto   <b>{sm.PerfectCount}</b>     " +
                    $"<color=#88ff88>\u25CF</color> Bien    <b>{sm.GoodCount}</b>\n" +
                    $"<color=#ffaa44>\u25C6</color> Mal        <b>{sm.BadCount}</b>     " +
                    $"<color=#ff4444>\u2715</color> Fallo   <b>{sm.MissCount}</b>\n" +
                    $"\n" +
                    $"<size=20><color=#553311>[ R ] Reiniciar     [ ESC ] Pausa</color></size>";
        }

        // ── Update ────────────────────────────────────────────────────────
        private void Update()
        {
            float dt = Time.deltaTime;

            // Judgement fade + scale pop
            if (judgementText != null && judgTimer > 0f)
            {
                judgTimer -= dt;
                // Scale pop
                if (judgScaleTimer > 0f)
                {
                    judgScaleTimer -= dt;
                    float sp = 1f - judgScaleTimer / JudgScaleDur;
                    judgementText.transform.localScale =
                        Vector3.one * Mathf.Lerp(1.4f, 1f, Mathf.SmoothStep(0f, 1f, sp));
                }
                else
                {
                    judgementText.transform.localScale = Vector3.one;
                }
                // Fade out last 35%
                Color c = judgementText.color;
                c.a = Mathf.Clamp01(judgTimer / (JudgDur * 0.35f));
                judgementText.color = c;
                if (judgTimer <= 0f) { judgementText.text = ""; judgementText.transform.localScale = Vector3.one; }
            }

            // Combo pop decay
            if (comboPopTimer > 0f) comboPopTimer -= dt;

            // Milestone banner fade
            if (milestoneText != null && milestoneTimer > 0f)
            {
                milestoneTimer -= dt;
                Color c = milestoneText.color;
                c.a = Mathf.Clamp01(milestoneTimer / (MilestoneDur * 0.25f));
                milestoneText.color = c;
                if (milestoneTimer <= 0f) milestoneText.text = "";
            }

            // Score smooth lerp
            if (scoreText != null)
            {
                displayedScore = Mathf.RoundToInt(
                    Mathf.Lerp(displayedScore, targetScore, dt * ScoreAnimSpeed));
                if (Mathf.Abs(displayedScore - targetScore) < 5) displayedScore = targetScore;
                scoreText.text =
                    $"<size=16><color=#aa5500>SCORE</color></size>\n" +
                    $"<b><color=#ffffff>{displayedScore:0000000}</color></b>";
            }

            // Result panel scale-in
            if (resultOpening && resultOpenTimer < ResultOpenDur)
            {
                resultOpenTimer += dt;
                float t = Mathf.SmoothStep(0f, 1f, resultOpenTimer / ResultOpenDur);
                if (resultPanelRect != null)
                    resultPanelRect.localScale = Vector3.one * Mathf.Lerp(0.7f, 1f, t);
                if (resultGroup != null)
                    resultGroup.alpha = t;
                if (resultOpenTimer >= ResultOpenDur) resultOpening = false;
            }
        }
    }
}

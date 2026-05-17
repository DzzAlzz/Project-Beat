using TMPro;
using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Full in-game HUD: song info, score, combo, accuracy, multiplier badge,
    /// judgement pop, milestone banner, results screen with S+ and Full Combo badge.
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

        private CanvasGroup introGroup;
        private TMP_Text introTitleText;
        private TMP_Text introSubText;
        private float introTimer;
        private const float IntroDuration = 2.15f;

        private const float JudgeFade     = 0.65f;
        private const float ComboPop      = 0.30f;
        private const float MilestoneDur  = 1.2f;
        private const float ScorePopDur   = 0.20f;

        // ── Init ──────────────────────────────────────────────────────────
        public void Initialize(BeatmapData beatmap)
        {
            if (songText != null)
                songText.text = $"<b><color=#ff8800>{beatmap.songName}</color></b>\n" +
                                $"<size=20><color=#ffcc66>{beatmap.artist}</color></size>";

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
            ShowLevelIntro(beatmap);
        }

        private void ShowLevelIntro(BeatmapData beatmap)
        {
            EnsureIntroOverlay();
            if (introGroup == null || introTitleText == null) return;

            string songName = beatmap != null ? beatmap.songName : "PROJECT BEAT";
            introTitleText.text = "<size=24><color=#00F1FF>PREPARATE</color></size>\n" +
                                  "<b><color=#FFF000>" + songName + "</color></b>";
            if (introSubText != null)
                introSubText.text = "<color=#FFAA44>3 • 2 • 1</color>   <color=#FFFFFF>LISTO PARA JUGAR</color>";
            introTimer = IntroDuration;
            introGroup.alpha = 1f;
            introGroup.blocksRaycasts = false;
            introGroup.interactable = false;
            introGroup.transform.localScale = new Vector3(0.96f, 0.96f, 1f);
        }

        private void EnsureIntroOverlay()
        {
            if (introGroup != null) return;

            GameObject canvasGO = new GameObject("PB_LevelIntroCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 70;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            GameObject groupGO = new GameObject("PB_LevelIntroGroup", typeof(RectTransform));
            groupGO.transform.SetParent(canvasGO.transform, false);
            introGroup = groupGO.AddComponent<CanvasGroup>();
            RectTransform grt = groupGO.GetComponent<RectTransform>();
            grt.anchorMin = Vector2.zero;
            grt.anchorMax = Vector2.one;
            grt.offsetMin = Vector2.zero;
            grt.offsetMax = Vector2.zero;

            GameObject dimGO = new GameObject("PB_Intro_Dim");
            dimGO.transform.SetParent(groupGO.transform, false);
            UnityEngine.UI.Image dim = dimGO.AddComponent<UnityEngine.UI.Image>();
            dim.color = new Color(0f, 0f, 0f, 0.46f);
            dim.raycastTarget = false;
            RectTransform drt = dimGO.GetComponent<RectTransform>();
            drt.anchorMin = Vector2.zero;
            drt.anchorMax = Vector2.one;
            drt.offsetMin = Vector2.zero;
            drt.offsetMax = Vector2.zero;

            introTitleText = CreateIntroText(groupGO.transform, "PB_Intro_Title", new Vector2(0f, 38f), new Vector2(850f, 135f), 48f, TextAlignmentOptions.Center, FontStyles.Bold, 5f);
            introSubText = CreateIntroText(groupGO.transform, "PB_Intro_Subtitle", new Vector2(0f, -72f), new Vector2(720f, 42f), 22f, TextAlignmentOptions.Center, FontStyles.Normal, 2f);
        }

        private TMP_Text CreateIntroText(Transform parent, string name, Vector2 pos, Vector2 size, float fontSize, TextAlignmentOptions alignment, FontStyles style, float spacing)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.characterSpacing = spacing;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = false;
            tmp.color = Color.white;
            UnityEngine.UI.Shadow shadow = go.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.88f);
            shadow.effectDistance = new Vector2(3f, -3f);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return tmp;
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
                    comboText.text = $"<b><size={sz:0}><color=#ffdd00>{combo}</color>" +
                                     $"<color=#ff9900>x</color></size></b> " +
                                     $"<size=24><color=#ffeeaa>COMBO</color></size>";
                else
                    comboText.text = "";
            }

            if (accuracyText != null)
                accuracyText.text = $"<color=#ffaa44>PREC</color> " +
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
            // Level intro fade + subtle scale
            if (introGroup != null && introTimer > 0f)
            {
                introTimer -= Time.deltaTime;
                float t = Mathf.Clamp01(introTimer / IntroDuration);
                float fadeIn = Mathf.Clamp01((IntroDuration - introTimer) / 0.35f);
                float fadeOut = Mathf.Clamp01(introTimer / 0.55f);
                introGroup.alpha = Mathf.Min(fadeIn, fadeOut);
                float s = Mathf.Lerp(1.04f, 0.98f, 1f - t);
                introGroup.transform.localScale = new Vector3(s, s, 1f);
                if (introTimer <= 0f) introGroup.alpha = 0f;
            }

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

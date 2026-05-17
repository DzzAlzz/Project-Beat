using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        // Modern results screen (created at runtime so existing scenes keep working)
        private RectTransform resultCard;
        private TMP_Text modernLevelText;
        private TMP_Text modernRankText;
        private TMP_Text modernScoreText;
        private TMP_Text modernAccuracyText;
        private TMP_Text modernComboText;
        private TMP_Text modernHitStatsText;
        private TMP_Text modernOptionsText;
        private Image modernAccentLine;
        private float resultAnimTimer;
        private int resultDisplayedScore;
        private int resultTargetScore;
        private string resultScorePrefix = "";
        private const float ResultAnimDur = 0.75f;

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

            EnsureModernResultsScreen();

            resultGroup.alpha = 1f;
            resultGroup.interactable = resultGroup.blocksRaycasts = true;

            string rank = sm.GetRank();
            Color rc = GetRankColor(rank);
            string rh = ColorUtility.ToHtmlStringRGB(rc);
            string levelName = beatmap != null ? beatmap.songName : "PROJECT BEAT";

            if (modernLevelText != null)
                modernLevelText.text = $"<size=18><color=#00F1FF>RESULTADOS DEL NIVEL</color></size>\n" +
                                       $"<b><color=#FFFFFF>{levelName}</color></b>";

            if (modernRankText != null)
            {
                modernRankText.color = rc;
                modernRankText.text = $"<size=22><color=#B8C7FF>RANGO</color></size>\n<b><size=120><color=#{rh}>{rank}</color></size></b>" +
                                      (sm.IsFullCombo ? "\n<size=24><color=#FFDD00>FULL COMBO</color></size>" : "");
            }

            resultTargetScore = sm.Score;
            resultDisplayedScore = 0;
            resultScorePrefix = "<size=18><color=#B8C7FF>PUNTAJE TOTAL</color></size>\n";
            if (modernScoreText != null) modernScoreText.text = resultScorePrefix + "<b><size=34>0000000</size></b>";
            if (modernAccuracyText != null) modernAccuracyText.text = $"<size=18><color=#B8C7FF>PRECISIÓN</color></size>\n<b><size=34>{sm.Accuracy:0.00}%</size></b>";
            if (modernComboText != null) modernComboText.text = $"<size=18><color=#B8C7FF>MAX COMBO</color></size>\n<b><size=34>{sm.MaxCombo}</size></b>";
            if (modernHitStatsText != null)
                modernHitStatsText.text =
                    $"<color=#FFF36B>PERFECT</color> <b>{sm.PerfectCount}</b>     " +
                    $"<color=#7CFFB2>GOOD</color> <b>{sm.GoodCount}</b>     " +
                    $"<color=#FFB25C>MISS</color> <b>{sm.BadCount}</b>     " +
                    $"<color=#FF5A66>FAIL</color> <b>{sm.MissCount}</b>";
            if (modernOptionsText != null)
                modernOptionsText.text = "<color=#00F1FF>[ R ] Reiniciar</color>     <color=#FFDD00>[ ESC ] Volver / Pausa</color>";
            if (modernAccentLine != null) modernAccentLine.color = new Color(rc.r, rc.g, rc.b, 0.9f);

            // keep old serialized texts from drawing over the new layout
            if (resultTitleText != null) resultTitleText.text = "";
            if (resultBodyText != null) resultBodyText.text = "";

            resultAnimTimer = ResultAnimDur;
            if (resultCard != null) resultCard.localScale = new Vector3(0.92f, 0.92f, 1f);
        }

        private Color GetRankColor(string rank)
        {
            return rank switch
            {
                "S+" => new Color(1f, 0.45f, 1f),
                "S"  => new Color(1f, 0.88f, 0.10f),
                "A"  => new Color(0.35f, 1f, 0.55f),
                "B"  => new Color(0.35f, 0.72f, 1f),
                "C"  => new Color(1f, 0.62f, 0.25f),
                _    => new Color(1f, 0.28f, 0.32f)
            };
        }

        private void EnsureModernResultsScreen()
        {
            if (resultCard != null) return;

            RectTransform root = resultGroup.GetComponent<RectTransform>();
            if (root == null) return;

            // Convert the old centered panel into a full-screen overlay.
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.pivot = new Vector2(0.5f, 0.5f);
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            Image rootImg = resultGroup.GetComponent<Image>();
            if (rootImg != null) rootImg.color = new Color(0f, 0f, 0f, 0.68f);

            // Hide any old decorative children to avoid duplicated boxes/text.
            for (int i = 0; i < resultGroup.transform.childCount; i++)
                resultGroup.transform.GetChild(i).gameObject.SetActive(false);

            GameObject cardGO = new GameObject("PB_ResultCard_Final", typeof(RectTransform));
            cardGO.transform.SetParent(resultGroup.transform, false);
            resultCard = cardGO.GetComponent<RectTransform>();
            resultCard.anchorMin = resultCard.anchorMax = new Vector2(0.5f, 0.5f);
            resultCard.pivot = new Vector2(0.5f, 0.5f);
            resultCard.sizeDelta = new Vector2(820f, 610f);
            resultCard.anchoredPosition = Vector2.zero;

            Image cardImg = cardGO.AddComponent<Image>();
            cardImg.color = new Color(0.025f, 0.045f, 0.085f, 0.96f);

            Outline outline = cardGO.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.94f, 1f, 0.45f);
            outline.effectDistance = new Vector2(2f, -2f);

            modernAccentLine = CreateResultImage(resultCard, "AccentLine", new Vector2(0f, 280f), new Vector2(650f, 4f), new Color(1f, 0.86f, 0f, 0.9f));
            CreateResultImage(resultCard, "BottomLine", new Vector2(0f, -270f), new Vector2(500f, 2f), new Color(0f, 0.94f, 1f, 0.55f));

            modernLevelText = CreateResultText(resultCard, "LevelName", new Vector2(0f, 225f), new Vector2(740f, 80f), 34f, TextAlignmentOptions.Center, FontStyles.Bold, 4f);
            modernRankText = CreateResultText(resultCard, "Rank", new Vector2(0f, 95f), new Vector2(420f, 190f), 42f, TextAlignmentOptions.Center, FontStyles.Bold, 2f);

            modernScoreText = CreateResultText(resultCard, "ScoreStat", new Vector2(-250f, -90f), new Vector2(230f, 95f), 26f, TextAlignmentOptions.Center, FontStyles.Bold, 1f);
            modernAccuracyText = CreateResultText(resultCard, "AccuracyStat", new Vector2(0f, -90f), new Vector2(230f, 95f), 26f, TextAlignmentOptions.Center, FontStyles.Bold, 1f);
            modernComboText = CreateResultText(resultCard, "ComboStat", new Vector2(250f, -90f), new Vector2(230f, 95f), 26f, TextAlignmentOptions.Center, FontStyles.Bold, 1f);

            modernHitStatsText = CreateResultText(resultCard, "HitStats", new Vector2(0f, -185f), new Vector2(720f, 60f), 24f, TextAlignmentOptions.Center, FontStyles.Bold, 1f);
            modernOptionsText = CreateResultText(resultCard, "Options", new Vector2(0f, -240f), new Vector2(720f, 38f), 20f, TextAlignmentOptions.Center, FontStyles.Normal, 1f);
        }

        private Image CreateResultImage(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return img;
        }

        private TMP_Text CreateResultText(Transform parent, string name, Vector2 pos, Vector2 size, float fontSize, TextAlignmentOptions alignment, FontStyles style, float spacing)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.characterSpacing = spacing;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = false;
            tmp.richText = true;
            tmp.color = Color.white;
            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.82f);
            shadow.effectDistance = new Vector2(2f, -2f);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return tmp;
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

            // Results entrance animation + animated score count-up
            if (resultGroup != null && resultGroup.alpha > 0.01f && resultCard != null)
            {
                if (resultAnimTimer > 0f)
                {
                    resultAnimTimer -= Time.unscaledDeltaTime;
                    float k = 1f - Mathf.Clamp01(resultAnimTimer / ResultAnimDur);
                    float eased = 1f - Mathf.Pow(1f - k, 3f);
                    float s = Mathf.Lerp(0.92f, 1f, eased);
                    resultCard.localScale = new Vector3(s, s, 1f);
                    resultDisplayedScore = Mathf.RoundToInt(Mathf.Lerp(0, resultTargetScore, eased));
                }
                else
                {
                    resultCard.localScale = Vector3.one;
                    resultDisplayedScore = resultTargetScore;
                }

                if (modernScoreText != null)
                    modernScoreText.text = resultScorePrefix + $"<b><size=34>{resultDisplayedScore:0000000}</size></b>";
            }
        }
    }
}

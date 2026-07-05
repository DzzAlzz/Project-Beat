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

        // Gameplay key indicators (Avance 25)
        private RectTransform keyIndicatorsRoot;
        private TMP_Text[] keyTexts;
        private Image[] keyBackImages;
        private Outline[] keyOutlines;
        private readonly KeyCode[] hudKeys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };
        private readonly string[] hudKeyLabels = { "D", "F", "J", "K" };

        // Avance 25 HUD reorganizado: agrupa título, puntaje, precisión y pausa.
        private RectTransform hudInfoPanel;
        private TMP_Text pauseHintHudText;

        // Avance 64 rehecho: feedback visual más vivo, pero compacto y no invasivo.
        private Outline judgementOutline;
        private Outline comboOutline;
        private Outline multiplierOutline;
        private Shadow judgementShadow;
        private Shadow comboShadow;
        private Shadow multiplierShadow;
        private Vector3 judgementBaseScale = Vector3.one;
        private Vector3 comboBaseScale = Vector3.one;
        private Vector3 multiplierBaseScale = Vector3.one;
        private Vector2 judgementBasePos;
        private bool feedbackPolishReady;
        private float judgementPopTimer;
        private float multiplierPopTimer;
        private int lastComboValue;
        private int lastMultiplierValue = 1;
        private const float JudgementPopDur = 0.28f;
        private const float MultiplierPopDur = 0.22f;

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
            RepositionGameplayHud();
            EnsureGameplayKeyIndicators();
            EnsureFeedbackPolish();
            lastComboValue = 0;
            lastMultiplierValue = 1;
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
            EnsureFeedbackPolish();

            // Score animates to target
            targetScore = score;
            if (score > displayedScore) scorePopTimer = ScorePopDur;

            // Pop compacto al subir combo: visible, pero sin dominar la pantalla.
            if (combo > lastComboValue && combo > 1)
                comboPopTimer = ComboPop;
            lastComboValue = combo;

            if (multiplier != lastMultiplierValue)
            {
                multiplierPopTimer = MultiplierPopDur;
                lastMultiplierValue = multiplier;
            }

            float comboSize = comboPopTimer > 0f
                ? Mathf.Lerp(30f, 38f, comboPopTimer / ComboPop)
                : 30f;

            if (comboText != null)
            {
                if (combo > 1)
                {
                    string comboColor = GetComboColorHex(combo, multiplier);
                    comboText.text = $"<b><size={comboSize:0}><color={comboColor}>{combo}x</color></size></b> " +
                                     $"<size=18><color=#DDEEFF>COMBO</color></size>";
                    comboText.enableWordWrapping = false;
                    comboText.richText = true;
                }
                else
                    comboText.text = "";
            }

            if (accuracyText != null)
                accuracyText.text = $"<color=#ffaa44>PREC</color> " +
                                    $"<b><color=#ffffff>{accuracy:0.00}%</color></b>";

            // Multiplier badge compacto: más vivo, pero sin tamaño exagerado.
            if (multiplierText != null)
            {
                if (multiplier > 1)
                {
                    string multiplierColor = GetMultiplierColorHex(multiplier);
                    float multSize = multiplierPopTimer > 0f
                        ? Mathf.Lerp(22f, 28f, multiplierPopTimer / MultiplierPopDur)
                        : 22f;
                    multiplierText.text = $"<b><size={multSize:0}><color={multiplierColor}>x{multiplier}</color></size></b>";
                    multiplierText.enableWordWrapping = false;
                    multiplierText.richText = true;
                }
                else multiplierText.text = "";
            }
        }

        // ── Judgement ─────────────────────────────────────────────────────
        public void ShowJudgement(string msg)
        {
            if (judgementText == null) return;
            EnsureFeedbackPolish();

            Color c = GetJudgementColor(msg);
            string styled = msg switch
            {
                "PERFECT" => "<b><size=38>PERFECTO</size></b>",
                "GOOD"    => "<b><size=34>BIEN</size></b>",
                "BAD"     => "<b><size=30>MAL</size></b>",
                "MISS"    => "<b><size=30>FALLO</size></b>",
                _          => ""
            };

            judgementText.text = styled;
            judgementText.color = new Color(c.r, c.g, c.b, string.IsNullOrEmpty(styled) ? 0f : 1f);
            judgementText.fontStyle = FontStyles.Bold;
            judgementText.characterSpacing = msg == "PERFECT" ? 2.2f : 1.4f;
            judgementText.enableWordWrapping = false;
            judgementText.richText = true;

            if (judgementOutline != null)
            {
                judgementOutline.effectColor = new Color(c.r, c.g, c.b, msg == "PERFECT" ? 0.58f : 0.42f);
                judgementOutline.effectDistance = msg == "PERFECT" ? new Vector2(2.2f, -2.2f) : new Vector2(1.6f, -1.6f);
            }
            if (judgementShadow != null)
            {
                judgementShadow.effectColor = new Color(0f, 0f, 0f, 0.82f);
                judgementShadow.effectDistance = new Vector2(2f, -2f);
            }

            RectTransform rt = judgementText.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = judgementBasePos;
            judgementText.transform.localScale = judgementBaseScale * 1.05f;

            judgementTimer = string.IsNullOrEmpty(styled) ? 0f : JudgeFade;
            judgementPopTimer = string.IsNullOrEmpty(styled) ? 0f : JudgementPopDur;
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


        // ── Avance 25: Gameplay HUD polish ───────────────────────────────
        private void RepositionGameplayHud()
        {
            EnsureHudInfoPanel();

            // Bloque HUD en la zona derecha: título, nivel, puntuación, precisión y ESC Pausa.
            PositionTmp(songText,     new Vector2(1f, 1f), new Vector2(-54f, -54f),  new Vector2(360f, 78f),  TextAlignmentOptions.TopRight);
            PositionTmp(scoreText,    new Vector2(1f, 1f), new Vector2(-54f, -132f), new Vector2(360f, 62f),  TextAlignmentOptions.TopRight);
            PositionTmp(accuracyText, new Vector2(1f, 1f), new Vector2(-54f, -195f), new Vector2(360f, 42f),  TextAlignmentOptions.TopRight);

            pauseHintHudText = FindTmpByName(transform, "EscHint");
            if (pauseHintHudText != null)
            {
                pauseHintHudText.text = "<color=#FF6A00>[ ESC ]</color> <color=#D7E5FF>Pausa</color>";
                pauseHintHudText.fontSize = 17f;
                pauseHintHudText.enableWordWrapping = false;
                PositionTmp(pauseHintHudText, new Vector2(1f, 1f), new Vector2(-54f, -240f), new Vector2(360f, 32f), TextAlignmentOptions.TopRight);
            }
        }

        private void EnsureHudInfoPanel()
        {
            if (hudInfoPanel != null) return;

            GameObject panelGO = new GameObject("PB_HUD_InfoBlock", typeof(RectTransform));
            panelGO.transform.SetParent(transform, false);
            hudInfoPanel = panelGO.GetComponent<RectTransform>();
            hudInfoPanel.anchorMin = hudInfoPanel.anchorMax = new Vector2(1f, 1f);
            hudInfoPanel.pivot = new Vector2(1f, 1f);
            hudInfoPanel.anchoredPosition = new Vector2(-32f, -35f);
            hudInfoPanel.sizeDelta = new Vector2(410f, 245f);

            Image bg = panelGO.AddComponent<Image>();
            bg.raycastTarget = false;
            bg.color = new Color(0.015f, 0.025f, 0.045f, 0.28f);

            Outline outline = panelGO.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.43f, 0.05f, 0.18f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // Detrás de los textos existentes.
            hudInfoPanel.SetAsFirstSibling();
        }

        private void PositionTmp(TMP_Text tmp, Vector2 anchor, Vector2 pos, Vector2 size, TextAlignmentOptions alignment)
        {
            if (tmp == null) return;
            RectTransform rt = tmp.GetComponent<RectTransform>();
            if (rt == null) return;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            tmp.alignment = alignment;
        }

        private TMP_Text FindTmpByName(Transform root, string objectName)
        {
            if (root == null) return null;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.name == objectName)
                {
                    TMP_Text found = child.GetComponent<TMP_Text>();
                    if (found != null) return found;
                }
                TMP_Text nested = FindTmpByName(child, objectName);
                if (nested != null) return nested;
            }
            return null;
        }

        private void EnsureGameplayKeyIndicators()
        {
            if (keyIndicatorsRoot != null) return;

            Transform parent = transform; // GameplayUI lives on Canvas_HUD
            GameObject rootGO = new GameObject("PB_Gameplay_KeyIndicators", typeof(RectTransform));
            rootGO.transform.SetParent(parent, false);
            keyIndicatorsRoot = rootGO.GetComponent<RectTransform>();
            keyIndicatorsRoot.anchorMin = new Vector2(0.5f, 0f);
            keyIndicatorsRoot.anchorMax = new Vector2(0.5f, 0f);
            keyIndicatorsRoot.pivot = new Vector2(0.5f, 0f);
            keyIndicatorsRoot.anchoredPosition = new Vector2(0f, 78f);
            keyIndicatorsRoot.sizeDelta = new Vector2(520f, 76f);

            keyTexts = new TMP_Text[4];
            keyBackImages = new Image[4];
            keyOutlines = new Outline[4];

            float[] x = { -195f, -65f, 65f, 195f };
            for (int i = 0; i < 4; i++)
            {
                GameObject keyGO = new GameObject("PB_Key_" + hudKeyLabels[i], typeof(RectTransform));
                keyGO.transform.SetParent(keyIndicatorsRoot, false);
                RectTransform rt = keyGO.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(x[i], 0f);
                rt.sizeDelta = new Vector2(58f, 46f);

                Image bg = keyGO.AddComponent<Image>();
                bg.raycastTarget = false;
                bg.color = new Color(0.035f, 0.055f, 0.095f, 0.78f);

                Outline outline = keyGO.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0.9f, 1f, 0.45f);
                outline.effectDistance = new Vector2(2f, -2f);

                GameObject textGO = new GameObject("Label", typeof(RectTransform));
                textGO.transform.SetParent(keyGO.transform, false);
                TMP_Text label = textGO.AddComponent<TextMeshProUGUI>();
                label.text = "<b>" + hudKeyLabels[i] + "</b>";
                label.fontSize = 24f;
                label.alignment = TextAlignmentOptions.Center;
                label.color = new Color(1f, 0.95f, 0.35f, 1f);
                label.raycastTarget = false;
                RectTransform trt = textGO.GetComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.offsetMin = Vector2.zero;
                trt.offsetMax = Vector2.zero;

                keyTexts[i] = label;
                keyBackImages[i] = bg;
                keyOutlines[i] = outline;
            }
        }

        private void UpdateGameplayKeyIndicators()
        {
            if (keyIndicatorsRoot == null || keyBackImages == null) return;

            for (int i = 0; i < keyBackImages.Length; i++)
            {
                bool pressed = Input.GetKey(hudKeys[i]);
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5f + i * 0.75f);

                if (keyBackImages[i] != null)
                    keyBackImages[i].color = pressed
                        ? new Color(0f, 0.85f, 1f, 0.86f)
                        : new Color(0.035f, 0.055f, 0.095f, Mathf.Lerp(0.62f, 0.78f, pulse));

                if (keyOutlines[i] != null)
                    keyOutlines[i].effectColor = pressed
                        ? new Color(1f, 0.92f, 0.2f, 0.95f)
                        : new Color(0f, 0.9f, 1f, 0.45f);

                if (keyTexts[i] != null)
                    keyTexts[i].color = pressed
                        ? Color.white
                        : new Color(1f, 0.95f, 0.35f, 1f);
            }
        }


        // ── Avance 64 rehecho: pulido compacto de feedback ─────────────────
        private void EnsureFeedbackPolish()
        {
            if (feedbackPolishReady) return;

            SetupFeedbackText(judgementText, ref judgementOutline, ref judgementShadow, new Vector2(1.6f, -1.6f));
            SetupFeedbackText(comboText, ref comboOutline, ref comboShadow, new Vector2(1.2f, -1.2f));
            SetupFeedbackText(multiplierText, ref multiplierOutline, ref multiplierShadow, new Vector2(1.1f, -1.1f));

            if (judgementText != null)
            {
                judgementBaseScale = judgementText.transform.localScale;
                RectTransform rt = judgementText.GetComponent<RectTransform>();
                if (rt != null)
                {
                    judgementBasePos = rt.anchoredPosition;
                    rt.sizeDelta = new Vector2(Mathf.Max(rt.sizeDelta.x, 420f), Mathf.Max(rt.sizeDelta.y, 74f));
                }
            }
            if (comboText != null)
                comboBaseScale = comboText.transform.localScale;
            if (multiplierText != null)
                multiplierBaseScale = multiplierText.transform.localScale;

            feedbackPolishReady = true;
        }

        private void SetupFeedbackText(TMP_Text tmp, ref Outline outline, ref Shadow shadow, Vector2 outlineDistance)
        {
            if (tmp == null) return;

            tmp.richText = true;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;

            outline = tmp.GetComponent<Outline>();
            if (outline == null) outline = tmp.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.42f);
            outline.effectDistance = outlineDistance;

            shadow = tmp.GetComponent<Shadow>();
            if (shadow == null) shadow = tmp.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.78f);
            shadow.effectDistance = new Vector2(1.8f, -1.8f);
        }

        private Color GetJudgementColor(string msg)
        {
            return msg switch
            {
                "PERFECT" => ColPerfect,
                "GOOD"    => new Color(0.18f, 0.95f, 1f),
                "BAD"     => ColBad,
                "MISS"    => ColMiss,
                _          => Color.white
            };
        }

        private string GetComboColorHex(int combo, int multiplier)
        {
            if (combo >= 100 || multiplier >= 5) return "#FF66F5";
            if (combo >= 50  || multiplier >= 4) return "#FFF36B";
            if (combo >= 25  || multiplier >= 3) return "#8DFF7A";
            if (combo >= 10  || multiplier >= 2) return "#00F1FF";
            return "#FFE680";
        }

        private string GetMultiplierColorHex(int multiplier)
        {
            return multiplier switch
            {
                2 => "#00F1FF",
                3 => "#8DFF7A",
                4 => "#FFF36B",
                5 => "#FF66F5",
                _ => "#FFFFFF"
            };
        }

        private Color GetMultiplierColor(int multiplier)
        {
            return multiplier switch
            {
                2 => new Color(0f, 0.95f, 1f, 1f),
                3 => new Color(0.55f, 1f, 0.48f, 1f),
                4 => new Color(1f, 0.95f, 0.35f, 1f),
                5 => new Color(1f, 0.38f, 0.95f, 1f),
                _ => Color.white
            };
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


        public void HideResults()
        {
            if (resultGroup == null) return;

            resultGroup.alpha = 0f;
            resultGroup.interactable = false;
            resultGroup.blocksRaycasts = false;
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
            if (hudInfoPanel == null || pauseHintHudText == null) RepositionGameplayHud();
            EnsureGameplayKeyIndicators();
            UpdateGameplayKeyIndicators();
            EnsureFeedbackPolish();

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

            // Judgement fade: pop pequeño, subida leve y desaparición limpia.
            if (judgementText != null && judgementTimer > 0f)
            {
                judgementTimer -= Time.deltaTime;
                float life = 1f - Mathf.Clamp01(judgementTimer / JudgeFade);
                float fade = Mathf.Clamp01(judgementTimer / (JudgeFade * 0.42f));
                Color c = judgementText.color;
                c.a = fade;
                judgementText.color = c;

                if (judgementPopTimer > 0f)
                    judgementPopTimer -= Time.deltaTime;

                float pop = judgementPopTimer > 0f ? Mathf.Clamp01(judgementPopTimer / JudgementPopDur) : 0f;
                float scale = 1f + 0.08f * pop;
                judgementText.transform.localScale = judgementBaseScale * scale;

                RectTransform jrt = judgementText.GetComponent<RectTransform>();
                if (jrt != null)
                    jrt.anchoredPosition = judgementBasePos + new Vector2(0f, life * 10f);

                if (judgementTimer <= 0f)
                {
                    judgementText.text = "";
                    judgementText.transform.localScale = judgementBaseScale;
                    if (jrt != null) jrt.anchoredPosition = judgementBasePos;
                }
            }

            // Combo/multiplicador: pulso controlado, sin invadir la pista.
            if (comboPopTimer > 0f) comboPopTimer -= Time.deltaTime;
            if (comboText != null)
            {
                float comboPulse = comboPopTimer > 0f ? Mathf.Clamp01(comboPopTimer / ComboPop) : 0f;
                comboText.transform.localScale = comboBaseScale * (1f + 0.06f * comboPulse);
                if (comboOutline != null)
                    comboOutline.effectColor = new Color(0f, 0.95f, 1f, Mathf.Lerp(0.22f, 0.52f, comboPulse));
            }

            if (multiplierPopTimer > 0f) multiplierPopTimer -= Time.deltaTime;
            if (multiplierText != null)
            {
                float multPulse = multiplierPopTimer > 0f ? Mathf.Clamp01(multiplierPopTimer / MultiplierPopDur) : 0f;
                multiplierText.transform.localScale = multiplierBaseScale * (1f + 0.10f * multPulse);
                if (multiplierOutline != null)
                {
                    Color mc = GetMultiplierColor(lastMultiplierValue);
                    multiplierOutline.effectColor = new Color(mc.r, mc.g, mc.b, Mathf.Lerp(0.25f, 0.64f, multPulse));
                }
            }

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

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
        private TMP_Text modernNextLevelText;
        private Image modernAccentLine;
        private Image modernResultBackground;
        private Image modernResultDimLayer;
        private Image modernResultGlowMagenta;
        private Image modernResultGlowCyan;
        private Button resultNextButton;
        private Button resultRestartButton;
        private Button resultBackButton;
        private TMP_Text resultNextButtonLabel;
        private TMP_Text resultRestartButtonLabel;
        private TMP_Text resultBackButtonLabel;
        private Button[] resultButtons;
        private Image[] resultButtonImages;
        private Outline[] resultButtonOutlines;
        private RectTransform[] resultButtonRects;
        private int resultSelectedIndex;
        private int resultHoveredIndex = -1;
        private float resultButtonPulse;
        private readonly Color resultButtonNormalColor = new Color(0.105f, 0.018f, 0.220f, 0.96f);
        private readonly Color resultButtonHoverColor = new Color(0.190f, 0.035f, 0.360f, 1f);
        private readonly Color resultButtonSelectedColor = new Color(0.070f, 0.180f, 0.320f, 1f);
        private readonly Color resultButtonPressedColor = new Color(0.000f, 0.900f, 1.000f, 1f);
        private float resultAnimTimer;
        private int resultDisplayedScore;
        private int resultTargetScore;
        private string resultScorePrefix = "";
        private const float ResultAnimDur = 0.75f;

        // Gameplay key indicators (Avance 25)
        private RectTransform keyIndicatorsRoot;
        private TMP_Text[] keyTexts;
        private Image[] keyBackImages;
        private Image[] keyGlowImages;
        private Outline[] keyOutlines;
        private Sprite keyCapSprite;
        private Sprite keyGlowSprite;
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

        // Avance 66 polish: aura de fuego / energía para racha más pulida y visible.
        // Solo visual: no modifica puntuación, timing ni detección de notas.
        private RectTransform comboAuraRoot;
        private CanvasGroup comboAuraGroup;
        private Image comboAuraFloorGlow;
        private Image comboAuraCoreGlow;
        private Image comboAuraPulseGlow;
        private Image comboAuraLeftWall;
        private Image comboAuraRightWall;
        private Image[] comboAuraFlames;
        private Image[] comboAuraSparks;
        private Sprite comboAuraSoftSprite;
        private Sprite comboAuraFlameSprite;
        private Sprite comboAuraSparkSprite;
        private float comboAuraCurrent;
        private float comboAuraTarget;
        private int comboAuraCombo;
        private int comboAuraMultiplier = 1;
        private Color comboAuraMainColor = new Color(1f, 0.42f, 0.06f, 1f);

        public bool IsResultsVisibleForCursor
        {
            get { return resultGroup != null && resultGroup.alpha > 0.01f && resultGroup.gameObject.activeInHierarchy; }
        }

        // ── Init ──────────────────────────────────────────────────────────
        public void Initialize(BeatmapData beatmap)
        {
            if (songText != null)
                songText.text = $"<b><color=#FF66D9>{beatmap.songName}</color></b>\n" +
                                $"<size=20><color=#00F1FF>{beatmap.artist}</color></size>";

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
            SetGameplayKeyIndicatorsVisible(true);
            EnsureFeedbackPolish();
            EnsureComboAuraFire();
            comboAuraCurrent = 0f;
            comboAuraTarget = 0f;
            UpdateComboAuraTarget(0, 1);
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
            introTitleText.text = "<size=25><color=#00F1FF>PREPARATE</color></size>\n" +
                                  "<size=56><b><color=#FFF6FF>" + songName + "</color></b></size>";
            if (introSubText != null)
                introSubText.text = "<color=#00F1FF><b>3</b></color> <color=#FF37D6>•</color> <color=#FFFFFF><b>2</b></color> <color=#FF37D6>•</color> <color=#00F1FF><b>1</b></color>    <color=#FFF6FF><b>LISTO PARA JUGAR</b></color>";
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

            // Avance 91: marco neón sutil para que el 3 • 2 • 1 se vea integrado
            // con la estética del menú principal sin alterar timing ni gameplay.
            Image cyanGlow = CreateResultImage(groupGO.transform, "PB_Intro_CyanGlow", new Vector2(0f, 18f), new Vector2(720f, 150f), new Color(0f, 0.92f, 1f, 0.055f));
            cyanGlow.raycastTarget = false;
            Image magentaGlow = CreateResultImage(groupGO.transform, "PB_Intro_MagentaGlow", new Vector2(0f, -42f), new Vector2(780f, 120f), new Color(1f, 0.15f, 0.75f, 0.055f));
            magentaGlow.raycastTarget = false;
            CreateResultImage(groupGO.transform, "PB_Intro_TopCyanLine", new Vector2(0f, 112f), new Vector2(520f, 3f), new Color(0f, 0.92f, 1f, 0.82f));
            CreateResultImage(groupGO.transform, "PB_Intro_MidMagentaLine", new Vector2(0f, -24f), new Vector2(620f, 3f), new Color(1f, 0.14f, 0.82f, 0.74f));
            CreateResultImage(groupGO.transform, "PB_Intro_BottomCyanLine", new Vector2(0f, -108f), new Vector2(460f, 3f), new Color(0f, 0.92f, 1f, 0.68f));

            introTitleText = CreateIntroText(groupGO.transform, "PB_Intro_Title", new Vector2(0f, 40f), new Vector2(920f, 150f), 50f, TextAlignmentOptions.Center, FontStyles.Bold, 7f);
            introSubText = CreateIntroText(groupGO.transform, "PB_Intro_Subtitle", new Vector2(0f, -72f), new Vector2(870f, 48f), 25f, TextAlignmentOptions.Center, FontStyles.Bold, 2.5f);
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
            UnityEngine.UI.Outline outline = go.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(1f, 0.18f, 0.82f, 0.24f);
            outline.effectDistance = new Vector2(1.4f, -1.4f);
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
            if (combo > lastComboValue && combo > 1 && VisualAccessibilitySettings.ComboEffectsEnabled)
                comboPopTimer = ComboPop;
            lastComboValue = combo;

            if (multiplier != lastMultiplierValue)
            {
                multiplierPopTimer = VisualAccessibilitySettings.ComboEffectsEnabled ? MultiplierPopDur : 0f;
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
                accuracyText.text = $"<size=17><color=#FF66D9>PRECISION</color></size>\n" +
                                    $"<b><color=#FFFFFF>{accuracy:0.00}%</color></b>";

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

            UpdateComboAuraTarget(combo, multiplier);
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
            if (VisualAccessibilitySettings.ComboEffectsEnabled)
                comboPopTimer = ComboPop;
        }

        // ── Milestone banner ──────────────────────────────────────────────
        public void ShowMilestoneBanner(int combo)
        {
            if (milestoneText == null || !VisualAccessibilitySettings.ComboEffectsEnabled) return;
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

            // Avance 76: HUD menos apretado. Se distribuye en más altura y con
            // márgenes internos amplios para que PUNTUACION, PREC y ESC Pausa no se monten.
            PositionTmp(songText,     new Vector2(1f, 1f), new Vector2(-62f, -54f),  new Vector2(370f, 76f),  TextAlignmentOptions.TopRight);
            PositionTmp(scoreText,    new Vector2(1f, 1f), new Vector2(-62f, -142f), new Vector2(370f, 72f),  TextAlignmentOptions.TopRight);
            PositionTmp(accuracyText, new Vector2(1f, 1f), new Vector2(-62f, -220f), new Vector2(370f, 60f),  TextAlignmentOptions.TopRight);

            ConfigureHudText(songText, 23f, 1.8f);
            ConfigureHudText(scoreText, 25f, 0.8f);
            ConfigureHudText(accuracyText, 24f, 0.8f);

            pauseHintHudText = FindTmpByName(transform, "EscHint");
            if (pauseHintHudText != null)
            {
                pauseHintHudText.text = "<color=#FF8A24>[ ESC ]</color> <color=#E9ECFF>Pausa</color>";
                pauseHintHudText.fontSize = 16f;
                pauseHintHudText.characterSpacing = 0.8f;
                pauseHintHudText.enableWordWrapping = false;
                PositionTmp(pauseHintHudText, new Vector2(1f, 1f), new Vector2(-62f, -278f), new Vector2(370f, 32f), TextAlignmentOptions.TopRight);
            }
        }

        private void EnsureHudInfoPanel()
        {
            if (hudInfoPanel != null)
            {
                hudInfoPanel.anchoredPosition = new Vector2(-24f, -26f);
                hudInfoPanel.sizeDelta = new Vector2(430f, 320f);
                return;
            }

            GameObject panelGO = new GameObject("PB_HUD_InfoBlock", typeof(RectTransform));
            panelGO.transform.SetParent(transform, false);
            hudInfoPanel = panelGO.GetComponent<RectTransform>();
            hudInfoPanel.anchorMin = hudInfoPanel.anchorMax = new Vector2(1f, 1f);
            hudInfoPanel.pivot = new Vector2(1f, 1f);
            hudInfoPanel.anchoredPosition = new Vector2(-24f, -26f);
            hudInfoPanel.sizeDelta = new Vector2(430f, 320f);

            Image bg = panelGO.AddComponent<Image>();
            bg.raycastTarget = false;
            bg.color = new Color(0.045f, 0.004f, 0.105f, 0.66f);

            Outline outline = panelGO.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.94f, 1f, 0.42f);
            outline.effectDistance = new Vector2(2.0f, -2.0f);

            CreateHudDecorLine(hudInfoPanel, new Vector2(0f, 143f), new Vector2(318f, 3f), new Color(1f, 0.12f, 0.82f, 0.68f));
            CreateHudDecorLine(hudInfoPanel, new Vector2(0f, 131f), new Vector2(238f, 2f), new Color(0f, 0.94f, 1f, 0.55f));
            CreateHudDecorLine(hudInfoPanel, new Vector2(0f, -132f), new Vector2(300f, 2f), new Color(0f, 0.94f, 1f, 0.38f));
            CreateHudDecorLine(hudInfoPanel, new Vector2(0f, -146f), new Vector2(222f, 2f), new Color(1f, 0.12f, 0.82f, 0.46f));

            // Detrás de los textos existentes.
            hudInfoPanel.SetAsFirstSibling();
        }

        private void ConfigureHudText(TMP_Text tmp, float fontSize, float characterSpacing)
        {
            if (tmp == null) return;
            tmp.fontSize = fontSize;
            tmp.characterSpacing = characterSpacing;
            tmp.enableWordWrapping = false;
            tmp.richText = true;
        }

        private void CreateHudDecorLine(Transform parent, Vector2 pos, Vector2 size, Color color)
        {
            if (parent == null) return;
            GameObject go = new GameObject("PB_HUD_DecorLine", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
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
            keyGlowImages = new Image[4];
            keyOutlines = new Outline[4];
            if (keyCapSprite == null) keyCapSprite = CreateKeyCapSprite("PB_KeyCapSprite", 96, 72, 14);
            if (keyGlowSprite == null) keyGlowSprite = CreateKeyGlowSprite("PB_KeyGlowSprite", 96, 72);

            float[] x = { -195f, -65f, 65f, 195f };
            for (int i = 0; i < 4; i++)
            {
                GameObject keyGO = new GameObject("PB_Key_" + hudKeyLabels[i], typeof(RectTransform));
                keyGO.transform.SetParent(keyIndicatorsRoot, false);
                RectTransform rt = keyGO.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(x[i], 0f);
                rt.sizeDelta = new Vector2(64f, 50f);

                GameObject glowGO = new GameObject("Glow", typeof(RectTransform));
                glowGO.transform.SetParent(keyGO.transform, false);
                RectTransform glowRt = glowGO.GetComponent<RectTransform>();
                glowRt.anchorMin = glowRt.anchorMax = new Vector2(0.5f, 0.5f);
                glowRt.pivot = new Vector2(0.5f, 0.5f);
                glowRt.anchoredPosition = Vector2.zero;
                glowRt.sizeDelta = new Vector2(88f, 68f);
                Image glowImg = glowGO.AddComponent<Image>();
                glowImg.sprite = keyGlowSprite;
                glowImg.color = new Color(0f, 0.94f, 1f, 0.20f);
                glowImg.raycastTarget = false;

                Image bg = keyGO.AddComponent<Image>();
                bg.sprite = keyCapSprite;
                bg.type = Image.Type.Sliced;
                bg.raycastTarget = false;
                bg.color = new Color(0.070f, 0.022f, 0.155f, 0.92f);

                Outline outline = keyGO.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0.94f, 1f, 0.64f);
                outline.effectDistance = new Vector2(2.4f, -2.4f);

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
                keyGlowImages[i] = glowImg;
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
                float scale = pressed ? 1.14f : Mathf.Lerp(1.00f, 1.045f, pulse);

                if (keyBackImages[i] != null)
                {
                    RectTransform rt = keyBackImages[i].rectTransform;
                    rt.localScale = Vector3.Lerp(rt.localScale, Vector3.one * scale, Time.unscaledDeltaTime * 16f);
                    keyBackImages[i].color = pressed
                        ? new Color(0.02f, 0.92f, 1f, 0.96f)
                        : Color.Lerp(new Color(0.060f, 0.018f, 0.145f, 0.86f), new Color(0.130f, 0.045f, 0.250f, 0.92f), pulse);
                }

                if (keyGlowImages != null && i < keyGlowImages.Length && keyGlowImages[i] != null)
                    keyGlowImages[i].color = pressed
                        ? new Color(1f, 0.26f, 0.82f, 0.62f)
                        : new Color(0f, 0.94f, 1f, Mathf.Lerp(0.18f, 0.34f, pulse));

                if (keyOutlines[i] != null)
                    keyOutlines[i].effectColor = pressed
                        ? new Color(1f, 0.95f, 0.25f, 0.98f)
                        : new Color(0f, 0.94f, 1f, Mathf.Lerp(0.50f, 0.72f, pulse));

                if (keyTexts[i] != null)
                {
                    keyTexts[i].color = pressed
                        ? Color.white
                        : Color.Lerp(new Color(1f, 0.94f, 0.28f, 1f), new Color(0.65f, 1f, 1f, 1f), pulse * 0.35f);
                    keyTexts[i].rectTransform.localScale = Vector3.Lerp(keyTexts[i].rectTransform.localScale, Vector3.one * (pressed ? 1.10f : 1f), Time.unscaledDeltaTime * 14f);
                }
            }
        }

        private Sprite CreateKeyCapSprite(string spriteName, int width, int height, int radius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.name = spriteName;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color32[] pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = Mathf.Min(x, width - 1 - x);
                    float dy = Mathf.Min(y, height - 1 - y);
                    float corner = Mathf.Min(dx, dy);
                    bool insideCorner = true;
                    if (dx < radius && dy < radius)
                    {
                        float cx = dx - radius;
                        float cy = dy - radius;
                        insideCorner = (cx * cx + cy * cy) <= radius * radius;
                    }
                    float edge = Mathf.Min(Mathf.Min(x, width - 1 - x), Mathf.Min(y, height - 1 - y));
                    float alpha = insideCorner ? Mathf.Clamp01(edge / 4f) : 0f;
                    pixels[y * width + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect, new Vector4(16f, 14f, 16f, 14f));
        }

        private Sprite CreateKeyGlowSprite(string spriteName, int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.name = spriteName;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color32[] pixels = new Color32[width * height];
            Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (x - center.x) / (width * 0.5f);
                    float ny = (y - center.y) / (height * 0.5f);
                    float d = Mathf.Sqrt(nx * nx + ny * ny);
                    float alpha = Mathf.Clamp01(1f - d);
                    alpha = alpha * alpha * 0.75f;
                    pixels[y * width + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect, new Vector4(20f, 16f, 20f, 16f));
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

        // ── Avance 66 rehecho: aura visible de combo / racha ─────────────────
        private void EnsureComboAuraFire()
        {
            if (comboAuraRoot != null) return;

            comboAuraSoftSprite = CreateComboAuraSoftSprite("PB_ComboAuraFire_Soft", 128, 48);
            comboAuraFlameSprite = CreateComboAuraFlameSprite("PB_ComboAuraFire_Flame", 32, 96);
            comboAuraSparkSprite = CreateComboAuraSoftSprite("PB_ComboAuraFire_Spark", 32, 32);

            GameObject rootGO = new GameObject("PB_ComboAuraFire_Root", typeof(RectTransform));
            rootGO.transform.SetParent(transform, false);
            comboAuraRoot = rootGO.GetComponent<RectTransform>();
            comboAuraRoot.anchorMin = new Vector2(0.5f, 0f);
            comboAuraRoot.anchorMax = new Vector2(0.5f, 0f);
            comboAuraRoot.pivot = new Vector2(0.5f, 0f);
            comboAuraRoot.anchoredPosition = new Vector2(0f, 72f);
            comboAuraRoot.sizeDelta = new Vector2(960f, 285f);
            comboAuraRoot.SetAsFirstSibling();

            comboAuraGroup = rootGO.AddComponent<CanvasGroup>();
            comboAuraGroup.alpha = 0f;
            comboAuraGroup.interactable = false;
            comboAuraGroup.blocksRaycasts = false;

            // Halo principal en la base de la pista. Es más visible que el avance anterior,
            // pero se mantiene en la zona inferior para no tapar notas ni feedback.
            comboAuraFloorGlow = CreateComboAuraImage(comboAuraRoot, "PB_Aura_FloorGlow", comboAuraSoftSprite, new Vector2(0f, 24f), new Vector2(840f, 135f), true, 0);
            comboAuraCoreGlow = CreateComboAuraImage(comboAuraRoot, "PB_Aura_CoreFire", comboAuraSoftSprite, new Vector2(0f, 43f), new Vector2(590f, 86f), true, 1);
            comboAuraPulseGlow = CreateComboAuraImage(comboAuraRoot, "PB_Aura_PulseWave", comboAuraSoftSprite, new Vector2(0f, 58f), new Vector2(440f, 36f), true, 2);
            comboAuraLeftWall = CreateComboAuraImage(comboAuraRoot, "PB_Aura_LeftSide", comboAuraSoftSprite, new Vector2(-375f, 70f), new Vector2(190f, 205f), true, 3);
            comboAuraRightWall = CreateComboAuraImage(comboAuraRoot, "PB_Aura_RightSide", comboAuraSoftSprite, new Vector2(375f, 70f), new Vector2(190f, 205f), true, 4);

            comboAuraFlames = new Image[28];
            for (int i = 0; i < comboAuraFlames.Length; i++)
            {
                float t = comboAuraFlames.Length <= 1 ? 0f : i / (comboAuraFlames.Length - 1f);
                float x = Mathf.Lerp(-385f, 385f, t);
                float centerBias = 1f - Mathf.Abs(t - 0.5f) * 1.35f;
                float y = 12f + (i % 4) * 3f;
                float h = Mathf.Lerp(52f, 84f, Mathf.Clamp01(centerBias)) + (i % 3) * 8f;
                comboAuraFlames[i] = CreateComboAuraImage(comboAuraRoot, "PB_Aura_Flame_" + i, comboAuraFlameSprite, new Vector2(x, y), new Vector2(28f, h), false, 5 + i);
            }

            comboAuraSparks = new Image[24];
            for (int i = 0; i < comboAuraSparks.Length; i++)
            {
                float t = comboAuraSparks.Length <= 1 ? 0f : i / (comboAuraSparks.Length - 1f);
                float x = Mathf.Lerp(-360f, 360f, t);
                float y = 52f + (i % 5) * 17f;
                comboAuraSparks[i] = CreateComboAuraImage(comboAuraRoot, "PB_Aura_Spark_" + i, comboAuraSparkSprite, new Vector2(x, y), new Vector2(12f, 12f), false, 30 + i);
            }
        }

        private Image CreateComboAuraImage(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size, bool sliced, int sibling)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            Image img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            img.raycastTarget = false;
            img.color = new Color(1f, 1f, 1f, 0f);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            go.transform.SetSiblingIndex(Mathf.Clamp(sibling, 0, parent.childCount - 1));
            return img;
        }

        private Sprite CreateComboAuraSoftSprite(string spriteName, int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.name = spriteName;
            tex.filterMode = FilterMode.Bilinear;

            Color32[] pixels = new Color32[width * height];
            Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
            float maxX = Mathf.Max(1f, width * 0.50f);
            float maxY = Mathf.Max(1f, height * 0.50f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = (x - center.x) / maxX;
                    float dy = (y - center.y) / maxY;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(1f - dist);
                    alpha = alpha * alpha * (3f - 2f * alpha);
                    pixels[y * width + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect, new Vector4(18f, 12f, 18f, 12f));
        }

        private Sprite CreateComboAuraFlameSprite(string spriteName, int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.name = spriteName;
            tex.filterMode = FilterMode.Bilinear;

            Color32[] pixels = new Color32[width * height];
            float centerX = (width - 1) * 0.5f;
            for (int y = 0; y < height; y++)
            {
                float v = y / Mathf.Max(1f, height - 1f);
                float widthAtY = Mathf.Lerp(width * 0.47f, width * 0.10f, v);
                float vertical = Mathf.Sin(v * Mathf.PI);
                float tip = Mathf.Clamp01(1f - v * 0.15f);
                for (int x = 0; x < width; x++)
                {
                    float side = Mathf.Clamp01(1f - Mathf.Abs(x - centerX) / Mathf.Max(1f, widthAtY));
                    float alpha = Mathf.Pow(side, 1.55f) * Mathf.Pow(vertical, 0.72f) * tip;
                    pixels[y * width + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0f), 32f);
        }

        private void UpdateComboAuraTarget(int combo, int multiplier)
        {
            EnsureComboAuraFire();

            if (!VisualAccessibilitySettings.ComboEffectsEnabled || !VisualAccessibilitySettings.ComboAuraEnabled)
            {
                comboAuraTarget = 0f;
                return;
            }

            comboAuraCombo = combo;
            comboAuraMultiplier = Mathf.Max(1, multiplier);
            comboAuraMainColor = GetComboAuraFireColor(comboAuraCombo, comboAuraMultiplier);

            // Debe notarse claramente al tener racha, pero no aparecer al jugar sin combo.
            if (combo < 4)
            {
                comboAuraTarget = 0f;
                return;
            }

            float comboPart = Mathf.InverseLerp(4f, 80f, combo);
            float multPart = Mathf.InverseLerp(1f, 5f, comboAuraMultiplier);
            // Avance 66 polish: se eleva el mínimo visible y se deja una progresión más clara.
            comboAuraTarget = Mathf.Clamp01(0.42f + comboPart * 0.44f + multPart * 0.28f);
        }

        private Color GetComboAuraFireColor(int combo, int multiplier)
        {
            if (multiplier >= 5 || combo >= 90) return new Color(1f, 0.22f, 0.92f, 1f);      // magenta intenso
            if (multiplier >= 4 || combo >= 60) return new Color(1f, 0.76f, 0.16f, 1f);      // dorado
            if (multiplier >= 3 || combo >= 35) return new Color(0.25f, 1f, 0.42f, 1f);      // verde-cian
            if (multiplier >= 2 || combo >= 12) return new Color(0f, 0.92f, 1f, 1f);         // cian
            return new Color(1f, 0.38f, 0.04f, 1f);                                         // fuego naranja
        }

        private void UpdateComboAuraFireVisual()
        {
            if (comboAuraGroup == null || comboAuraRoot == null) return;

            float dt = Time.deltaTime;
            float changeSpeed = comboAuraTarget > comboAuraCurrent ? 4.4f : 3.2f;
            comboAuraCurrent = Mathf.MoveTowards(comboAuraCurrent, comboAuraTarget, dt * changeSpeed);

            float intensity = Mathf.Clamp01(comboAuraCurrent);
            if (intensity <= 0.001f)
            {
                comboAuraGroup.alpha = 0f;
                return;
            }

            float comboNormalized = Mathf.InverseLerp(5f, 85f, comboAuraCombo);
            float waveA = 0.5f + 0.5f * Mathf.Sin(Time.time * Mathf.Lerp(5.4f, 11.2f, comboNormalized));
            float waveB = 0.5f + 0.5f * Mathf.Sin(Time.time * Mathf.Lerp(7.8f, 14.2f, intensity) + 1.7f);
            float pulse = Mathf.Lerp(0.72f, 1.0f, waveA);

            Color main = comboAuraMainColor;
            Color warm = Color.Lerp(main, new Color(1f, 0.34f, 0.02f, 1f), 0.38f);
            Color hot = Color.Lerp(main, Color.white, 0.42f);

            comboAuraGroup.alpha = Mathf.Clamp01(0.72f + 0.28f * intensity);
            comboAuraRoot.localScale = new Vector3(1f + 0.030f * intensity * waveA, 1f + 0.070f * intensity * waveB, 1f);

            if (comboAuraFloorGlow != null)
            {
                comboAuraFloorGlow.color = new Color(main.r, main.g, main.b, (0.24f + 0.34f * pulse) * intensity);
                comboAuraFloorGlow.rectTransform.sizeDelta = new Vector2(780f + 150f * intensity * waveA, 115f + 76f * intensity * pulse);
            }
            if (comboAuraCoreGlow != null)
            {
                comboAuraCoreGlow.color = new Color(hot.r, hot.g, hot.b, (0.30f + 0.42f * waveB) * intensity);
                comboAuraCoreGlow.rectTransform.sizeDelta = new Vector2(500f + 185f * intensity, 60f + 52f * intensity * pulse);
            }
            if (comboAuraPulseGlow != null)
            {
                comboAuraPulseGlow.color = new Color(warm.r, warm.g, warm.b, (0.34f + 0.42f * waveA) * intensity);
                comboAuraPulseGlow.rectTransform.sizeDelta = new Vector2(380f + 400f * intensity * waveA, 24f + 38f * intensity);
            }
            if (comboAuraLeftWall != null)
            {
                comboAuraLeftWall.color = new Color(main.r, main.g, main.b, (0.14f + 0.30f * waveB) * intensity);
                comboAuraLeftWall.rectTransform.sizeDelta = new Vector2(140f + 105f * intensity, 145f + 115f * intensity * waveA);
            }
            if (comboAuraRightWall != null)
            {
                comboAuraRightWall.color = new Color(main.r, main.g, main.b, (0.14f + 0.30f * waveA) * intensity);
                comboAuraRightWall.rectTransform.sizeDelta = new Vector2(140f + 105f * intensity, 145f + 115f * intensity * waveB);
            }

            if (comboAuraFlames != null)
            {
                for (int i = 0; i < comboAuraFlames.Length; i++)
                {
                    Image flame = comboAuraFlames[i];
                    if (flame == null) continue;

                    RectTransform rt = flame.rectTransform;
                    float t = comboAuraFlames.Length <= 1 ? 0f : i / (comboAuraFlames.Length - 1f);
                    float center = 1f - Mathf.Abs(t - 0.5f) * 1.55f;
                    float local = 0.5f + 0.5f * Mathf.Sin(Time.time * (5.5f + (i % 6) * 0.72f) + i * 0.64f);
                    float baseX = Mathf.Lerp(-382f, 382f, t);
                    float sway = Mathf.Sin(Time.time * 3.2f + i * 0.9f) * Mathf.Lerp(2f, 12f, intensity);
                    float height = Mathf.Lerp(38f, 145f, intensity) * Mathf.Lerp(0.72f, 1.28f, local) * Mathf.Lerp(0.75f, 1.18f, Mathf.Clamp01(center));
                    float width = Mathf.Lerp(14f, 42f, intensity) * Mathf.Lerp(0.84f, 1.18f, local);
                    rt.anchoredPosition = new Vector2(baseX + sway, 5f + local * 34f + intensity * 14f);
                    rt.sizeDelta = new Vector2(width, height);

                    Color flameColor = i % 3 == 0 ? warm : main;
                    flameColor = Color.Lerp(flameColor, hot, 0.18f + 0.14f * local);
                    flame.color = new Color(flameColor.r, flameColor.g, flameColor.b, (0.25f + 0.56f * local) * intensity);
                }
            }

            if (comboAuraSparks != null)
            {
                for (int i = 0; i < comboAuraSparks.Length; i++)
                {
                    Image spark = comboAuraSparks[i];
                    if (spark == null) continue;

                    RectTransform rt = spark.rectTransform;
                    float t = comboAuraSparks.Length <= 1 ? 0f : i / (comboAuraSparks.Length - 1f);
                    float drift = Mathf.Repeat(Time.time * (0.23f + (i % 4) * 0.035f) + i * 0.137f, 1f);
                    float x = Mathf.Lerp(-360f, 360f, t) + Mathf.Sin(Time.time * 1.7f + i) * 18f * intensity;
                    float y = Mathf.Lerp(38f, 190f, drift);
                    float size = Mathf.Lerp(5f, 16f, intensity) * Mathf.Lerp(0.65f, 1.18f, Mathf.Sin(drift * Mathf.PI));
                    rt.anchoredPosition = new Vector2(x, y);
                    rt.sizeDelta = new Vector2(size, size);
                    Color sparkColor = Color.Lerp(main, Color.white, 0.36f);
                    spark.color = new Color(sparkColor.r, sparkColor.g, sparkColor.b, Mathf.Sin(drift * Mathf.PI) * intensity * 0.72f);
                }
            }
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
            SetGameplayHudVisible(false);
            if (resultGroup != null)
                resultGroup.transform.SetAsLastSibling();

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
                modernRankText.text = $"<size=22><color=#D8CBFF>RANGO</color></size>\n<b><size=118><color=#{rh}>{rank}</color></size></b>" +
                                      (sm.IsFullCombo ? "\n<size=24><color=#FFDD00>FULL COMBO</color></size>" : "");
            }

            resultTargetScore = sm.Score;
            resultDisplayedScore = 0;
            resultScorePrefix = "<size=18><color=#D8CBFF>PUNTAJE TOTAL</color></size>\n";
            if (modernScoreText != null) modernScoreText.text = resultScorePrefix + "<b><size=34>0000000</size></b>";
            if (modernAccuracyText != null) modernAccuracyText.text = $"<size=18><color=#D8CBFF>PRECISIÓN</color></size>\n<b><size=34>{sm.Accuracy:0.00}%</size></b>";
            if (modernComboText != null) modernComboText.text = $"<size=18><color=#D8CBFF>MAX COMBO</color></size>\n<b><size=34>{sm.MaxCombo}</size></b>";
            if (modernHitStatsText != null)
                modernHitStatsText.text =
                    $"<color=#FFF36B>PERFECTO</color> <b>{sm.PerfectCount}</b>     " +
                    $"<color=#7CFFB2>BIEN</color> <b>{sm.GoodCount}</b>     " +
                    $"<color=#FFB25C>MAL</color> <b>{sm.BadCount}</b>     " +
                    $"<color=#FF5A66>FALLO</color> <b>{sm.MissCount}</b>";

            bool hasNextLevel = HasNextLevelAvailable();
            if (resultNextButton != null)
                resultNextButton.gameObject.SetActive(hasNextLevel);
            if (resultNextButtonLabel != null)
                resultNextButtonLabel.text = "SIGUIENTE NIVEL";
            if (resultRestartButtonLabel != null)
                resultRestartButtonLabel.text = "REINICIAR";
            if (resultBackButtonLabel != null)
                resultBackButtonLabel.text = "VOLVER";

            resultSelectedIndex = hasNextLevel ? 0 : 1;
            EnsureResultEventSystem();
            UpdateResultButtonVisuals(true);

            // Mantiene una guía mínima de teclado, pero la acción principal ahora son botones reales.
            if (modernNextLevelText != null)
                modernNextLevelText.text = "";
            if (modernOptionsText != null)
                modernOptionsText.text = hasNextLevel
                    ? "<color=#00F1FF>[N]</color> Siguiente   <color=#00F1FF>[R]</color> Reiniciar   <color=#FF66D9>[ESC]</color> Volver"
                    : "<color=#00F1FF>[R]</color> Reiniciar   <color=#FF66D9>[ESC]</color> Volver";

            if (modernAccentLine != null) modernAccentLine.color = new Color(rc.r, rc.g, rc.b, 0.96f);
            if (modernResultBackground != null) modernResultBackground.color = new Color(0.018f, 0.000f, 0.045f, 0.985f);
            if (modernResultDimLayer != null) modernResultDimLayer.color = new Color(0.018f, 0.000f, 0.045f, 0.96f);

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
            SetGameplayHudVisible(true);
        }


        private bool HasNextLevelAvailable()
        {
            LevelManager lm = LevelManager.Instance;
            return lm != null && lm.Levels != null && lm.Levels.Length > 0 && lm.CurrentLevelIndex < lm.Levels.Length - 1;
        }

        private void SetGameplayKeyIndicatorsVisible(bool visible)
        {
            if (keyIndicatorsRoot != null)
                keyIndicatorsRoot.gameObject.SetActive(visible);
        }

        private void SetGameplayHudVisible(bool visible)
        {
            SetGameplayKeyIndicatorsVisible(visible);
            SetTmpVisible(songText, visible);
            SetTmpVisible(scoreText, visible);
            SetTmpVisible(comboText, visible);
            SetTmpVisible(accuracyText, visible);
            SetTmpVisible(judgementText, visible);
            SetTmpVisible(multiplierText, visible);
            SetTmpVisible(milestoneText, visible);
            if (hudInfoPanel != null) hudInfoPanel.gameObject.SetActive(visible);
            if (comboAuraRoot != null) comboAuraRoot.gameObject.SetActive(visible);
        }

        private void SetTmpVisible(TMP_Text tmp, bool visible)
        {
            if (tmp != null)
                tmp.gameObject.SetActive(visible);
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

            // Pantalla de resultados como overlay completo y opaco: no deja ver gameplay,
            // HUD ni rectangulos residuales detras.
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.pivot = new Vector2(0.5f, 0.5f);
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            Image rootImg = resultGroup.GetComponent<Image>();
            if (rootImg == null) rootImg = resultGroup.gameObject.AddComponent<Image>();
            rootImg.raycastTarget = true;
            rootImg.color = new Color(0.018f, 0.000f, 0.045f, 0.985f);
            modernResultBackground = rootImg;

            // Hide any old decorative children to avoid duplicated boxes/text.
            for (int i = 0; i < resultGroup.transform.childCount; i++)
                resultGroup.transform.GetChild(i).gameObject.SetActive(false);

            modernResultDimLayer = CreateResultImage(resultGroup.transform, "PB_Result_FullCleanBackground", Vector2.zero, new Vector2(2800f, 1600f), new Color(0.018f, 0.000f, 0.045f, 0.96f));

            GameObject cardGO = new GameObject("PB_ResultCard_CleanSpanish", typeof(RectTransform));
            cardGO.transform.SetParent(resultGroup.transform, false);
            resultCard = cardGO.GetComponent<RectTransform>();
            resultCard.anchorMin = resultCard.anchorMax = new Vector2(0.5f, 0.5f);
            resultCard.pivot = new Vector2(0.5f, 0.5f);
            resultCard.sizeDelta = new Vector2(940f, 690f);
            resultCard.anchoredPosition = Vector2.zero;

            Image cardImg = cardGO.AddComponent<Image>();
            cardImg.raycastTarget = false;
            cardImg.color = new Color(0.055f, 0.012f, 0.110f, 0.98f);

            Outline outline = cardGO.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.94f, 1f, 0.55f);
            outline.effectDistance = new Vector2(2.3f, -2.3f);

            modernAccentLine = CreateResultImage(resultCard, "AccentLine", new Vector2(0f, 318f), new Vector2(700f, 4f), new Color(1f, 0.12f, 0.82f, 0.96f));
            CreateResultImage(resultCard, "CyanLine", new Vector2(0f, 304f), new Vector2(540f, 3f), new Color(0f, 0.94f, 1f, 0.88f));
            CreateResultImage(resultCard, "BottomLine", new Vector2(0f, -320f), new Vector2(660f, 3f), new Color(1f, 0.12f, 0.82f, 0.82f));
            CreateResultImage(resultCard, "StatsBack", new Vector2(0f, -82f), new Vector2(800f, 108f), new Color(0.030f, 0.000f, 0.080f, 0.64f));

            modernLevelText = CreateResultText(resultCard, "LevelName", new Vector2(0f, 260f), new Vector2(790f, 82f), 34f, TextAlignmentOptions.Center, FontStyles.Bold, 4f);
            modernRankText = CreateResultText(resultCard, "Rank", new Vector2(0f, 122f), new Vector2(450f, 195f), 42f, TextAlignmentOptions.Center, FontStyles.Bold, 2f);

            modernScoreText = CreateResultText(resultCard, "ScoreStat", new Vector2(-285f, -72f), new Vector2(250f, 92f), 26f, TextAlignmentOptions.Center, FontStyles.Bold, 1f);
            modernAccuracyText = CreateResultText(resultCard, "AccuracyStat", new Vector2(0f, -72f), new Vector2(250f, 92f), 26f, TextAlignmentOptions.Center, FontStyles.Bold, 1f);
            modernComboText = CreateResultText(resultCard, "ComboStat", new Vector2(285f, -72f), new Vector2(250f, 92f), 26f, TextAlignmentOptions.Center, FontStyles.Bold, 1f);

            modernHitStatsText = CreateResultText(resultCard, "HitStats", new Vector2(0f, -172f), new Vector2(820f, 58f), 24f, TextAlignmentOptions.Center, FontStyles.Bold, 1f);
            modernNextLevelText = CreateResultText(resultCard, "InputHintSpacer", new Vector2(0f, -244f), new Vector2(760f, 28f), 18f, TextAlignmentOptions.Center, FontStyles.Normal, 1f);

            resultNextButton = CreateResultButton(resultCard, "ButtonNextLevel", new Vector2(-270f, -246f), new Vector2(250f, 66f), "SIGUIENTE NIVEL", out resultNextButtonLabel, new Color(0.10f, 0.02f, 0.22f, 0.96f), new Color(0f, 0.94f, 1f, 0.80f));
            resultRestartButton = CreateResultButton(resultCard, "ButtonRestart", new Vector2(0f, -246f), new Vector2(220f, 66f), "REINICIAR", out resultRestartButtonLabel, new Color(0.10f, 0.02f, 0.22f, 0.96f), new Color(1f, 0.12f, 0.82f, 0.80f));
            resultBackButton = CreateResultButton(resultCard, "ButtonBack", new Vector2(255f, -246f), new Vector2(200f, 66f), "VOLVER", out resultBackButtonLabel, new Color(0.10f, 0.02f, 0.22f, 0.96f), new Color(1f, 0.55f, 0.16f, 0.80f));

            resultButtons = new[] { resultNextButton, resultRestartButton, resultBackButton };
            resultButtonImages = new Image[resultButtons.Length];
            resultButtonOutlines = new Outline[resultButtons.Length];
            resultButtonRects = new RectTransform[resultButtons.Length];
            for (int i = 0; i < resultButtons.Length; i++)
            {
                if (resultButtons[i] == null) continue;
                resultButtonImages[i] = resultButtons[i].GetComponent<Image>();
                resultButtonOutlines[i] = resultButtons[i].GetComponent<Outline>();
                resultButtonRects[i] = resultButtons[i].GetComponent<RectTransform>();
            }

            BindResultButtons();
            modernOptionsText = CreateResultText(resultCard, "KeyboardHint", new Vector2(0f, -296f), new Vector2(790f, 32f), 16f, TextAlignmentOptions.Center, FontStyles.Normal, 1f);
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


        private Button CreateResultButton(Transform parent, string name, Vector2 pos, Vector2 size, string labelText, out TMP_Text label, Color baseColor, Color outlineColor)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image bg = go.AddComponent<Image>();
            bg.raycastTarget = true;
            bg.color = baseColor;

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(2f, -2f);

            Button button = go.AddComponent<Button>();
            button.targetGraphic = bg;
            ColorBlock cb = button.colors;
            cb.normalColor = baseColor;
            cb.highlightedColor = new Color(baseColor.r + 0.10f, baseColor.g + 0.04f, baseColor.b + 0.12f, 1f);
            cb.pressedColor = new Color(0f, 0.88f, 1f, 1f);
            cb.selectedColor = cb.highlightedColor;
            cb.disabledColor = new Color(0.08f, 0.08f, 0.12f, 0.42f);
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.10f;
            button.colors = cb;

            GameObject textGO = new GameObject("Label", typeof(RectTransform));
            textGO.transform.SetParent(go.transform, false);
            label = textGO.AddComponent<TextMeshProUGUI>();
            label.text = "<b>" + labelText + "</b>";
            label.raycastTarget = false;
            label.richText = true;
            label.fontSize = 20f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.characterSpacing = 1.2f;
            Shadow shadow = textGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.86f);
            shadow.effectDistance = new Vector2(2f, -2f);
            RectTransform trt = textGO.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            return button;
        }

        private void BindResultButtons()
        {
            if (resultNextButton != null)
            {
                resultNextButton.onClick.RemoveAllListeners();
                resultNextButton.onClick.AddListener(HandleResultNextButton);
            }
            if (resultRestartButton != null)
            {
                resultRestartButton.onClick.RemoveAllListeners();
                resultRestartButton.onClick.AddListener(HandleResultRestartButton);
            }
            if (resultBackButton != null)
            {
                resultBackButton.onClick.RemoveAllListeners();
                resultBackButton.onClick.AddListener(HandleResultBackButton);
            }
        }

        private GameController FindResultGameController()
        {
            return FindObjectOfType<GameController>();
        }

        private void HandleResultNextButton()
        {
            GameController gc = FindResultGameController();
            if (gc != null) gc.LoadNextLevelFromResults();
        }

        private void HandleResultRestartButton()
        {
            GameController gc = FindResultGameController();
            if (gc != null) gc.RestartScene();
        }

        private void HandleResultBackButton()
        {
            GameController gc = FindResultGameController();
            if (gc != null) gc.ReturnToMainMenuFromResults();
        }

        private void EnsureResultEventSystem()
        {
            if (EventSystem.current != null) return;

            GameObject eventSystemGO = new GameObject("PB_EventSystem_Runtime");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }

        private void HandleResultKeyboardAndHover()
        {
            if (resultButtons == null || resultButtons.Length == 0) return;

            bool moved = false;
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                resultSelectedIndex = GetNextActiveResultButton(resultSelectedIndex, 1);
                moved = true;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                resultSelectedIndex = GetNextActiveResultButton(resultSelectedIndex, -1);
                moved = true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                Button selected = GetActiveResultButton(resultSelectedIndex);
                if (selected != null) selected.onClick.Invoke();
            }

            resultHoveredIndex = GetHoveredResultButtonIndex();
            if (resultHoveredIndex >= 0 && resultHoveredIndex < resultButtons.Length)
                resultSelectedIndex = resultHoveredIndex;

            if (moved && EventSystem.current != null)
            {
                Button selected = GetActiveResultButton(resultSelectedIndex);
                EventSystem.current.SetSelectedGameObject(selected != null ? selected.gameObject : null);
            }

            UpdateResultButtonVisuals(false);
        }

        private Button GetActiveResultButton(int index)
        {
            if (resultButtons == null || index < 0 || index >= resultButtons.Length) return null;
            Button b = resultButtons[index];
            return (b != null && b.gameObject.activeInHierarchy && b.interactable) ? b : null;
        }

        private int GetNextActiveResultButton(int startIndex, int direction)
        {
            if (resultButtons == null || resultButtons.Length == 0) return 0;
            int index = Mathf.Clamp(startIndex, 0, resultButtons.Length - 1);
            for (int i = 0; i < resultButtons.Length; i++)
            {
                index = (index + direction + resultButtons.Length) % resultButtons.Length;
                if (GetActiveResultButton(index) != null) return index;
            }
            return Mathf.Clamp(startIndex, 0, resultButtons.Length - 1);
        }

        private int GetHoveredResultButtonIndex()
        {
            if (resultButtonRects == null) return -1;
            Vector2 mouse = Input.mousePosition;
            for (int i = 0; i < resultButtonRects.Length; i++)
            {
                if (GetActiveResultButton(i) == null || resultButtonRects[i] == null) continue;
                if (RectTransformUtility.RectangleContainsScreenPoint(resultButtonRects[i], mouse, null))
                    return i;
            }
            return -1;
        }

        private void UpdateResultButtonVisuals(bool immediate)
        {
            if (resultButtons == null || resultButtonImages == null || resultButtonOutlines == null) return;
            resultButtonPulse += Time.unscaledDeltaTime * 6.0f;
            float pulse = 0.5f + 0.5f * Mathf.Sin(resultButtonPulse);

            for (int i = 0; i < resultButtons.Length; i++)
            {
                Button b = resultButtons[i];
                if (b == null || !b.gameObject.activeInHierarchy) continue;

                bool active = i == resultSelectedIndex;
                bool hovered = i == resultHoveredIndex;
                bool hot = active || hovered;
                Color target = hot ? Color.Lerp(resultButtonSelectedColor, resultButtonHoverColor, pulse * 0.35f) : resultButtonNormalColor;

                if (resultButtonImages[i] != null)
                    resultButtonImages[i].color = immediate ? target : Color.Lerp(resultButtonImages[i].color, target, Time.unscaledDeltaTime * 12f);

                if (resultButtonOutlines[i] != null)
                {
                    Color outline = i switch
                    {
                        0 => new Color(0f, 0.96f, 1f, hot ? 0.95f : 0.55f),
                        1 => new Color(1f, 0.15f, 0.86f, hot ? 0.95f : 0.55f),
                        _ => new Color(1f, 0.55f, 0.16f, hot ? 0.95f : 0.55f)
                    };
                    resultButtonOutlines[i].effectColor = outline;
                    resultButtonOutlines[i].effectDistance = hot ? new Vector2(3f, -3f) : new Vector2(2f, -2f);
                }

                if (resultButtonRects != null && i < resultButtonRects.Length && resultButtonRects[i] != null)
                {
                    float scale = hot ? 1.045f + pulse * 0.015f : 1f;
                    resultButtonRects[i].localScale = immediate ? new Vector3(scale, scale, 1f) : Vector3.Lerp(resultButtonRects[i].localScale, new Vector3(scale, scale, 1f), Time.unscaledDeltaTime * 12f);
                }
            }
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
            bool resultsVisible = resultGroup != null && resultGroup.alpha > 0.01f;
            EnsureGameplayKeyIndicators();
            SetGameplayKeyIndicatorsVisible(!resultsVisible);
            if (!resultsVisible)
                UpdateGameplayKeyIndicators();
            EnsureFeedbackPolish();
            EnsureComboAuraFire();
            UpdateComboAuraFireVisual();

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

                scoreText.text = $"<size=17><color=#00F1FF>PUNTUACION</color></size>\n" +
                                 $"<b><color=#FFFFFF>{displayedScore:0000000}</color></b>";
            }

            // Results entrance animation + animated score count-up
            if (resultGroup != null && resultGroup.alpha > 0.01f && resultCard != null)
            {
                HandleResultKeyboardAndHover();
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

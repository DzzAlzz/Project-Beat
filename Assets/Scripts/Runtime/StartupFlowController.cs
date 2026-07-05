using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Crea una intro moderna y un menu principal antes del selector Arcade.
    /// No modifica LevelManager ni los niveles; solo controla el flujo visual inicial.
    /// Flujo: Intro -> Menu Principal -> Arcade -> Selector de niveles existente.
    /// </summary>
    public class StartupFlowController : MonoBehaviour
    {
        public const string SkipStartupPrefsKey = "ProjectBeat.SkipStartupIntro";
        public const string ForceMainMenuPrefsKey = "ProjectBeat.ForceMainMenu";

        private Canvas canvas;
        private CanvasGroup rootGroup;
        private CanvasGroup splashGroup;
        private CanvasGroup menuGroup;
        private RectTransform tutorialButton;
        private RectTransform arcadeButton;
        private RectTransform futureButton;
        private RectTransform exitButton;
        private TMP_Text tutorialLabel;
        private TMP_Text arcadeLabel;
        private TMP_Text futureLabel;
        private TMP_Text exitLabel;
        private Image tutorialBg;
        private Image arcadeBg;
        private Image futureBg;
        private Image exitBg;
        private Image tutorialGlow;
        private Image arcadeGlow;
        private Image futureGlow;
        private Image exitGlow;
        private const int MainMenuOptionCount = 4;
        private int selectedIndex;
        private int lastVisualSelectedIndex = -1;
        private float selectionPop;
        private float confirmPop;
        private float pulse;
        private GameController gameController;
        private PauseMenu pauseMenu;
        private AudioSource menuMusicSource;
        private AudioClip menuMusicClip;
        private TMP_Text menuTitleText;
        private Image rgbAuraImage;
        private Image topRgbLine;
        private Image bottomRgbLine;
        private Image[] ambientBars;
        private Image[] equalizerBars;
        private Image[] waveLines;
        private Image[] beatParticles;
        private Image[] beatRings;
        private Image menuTitleGlow;
        private Image backgroundGradient;
        private Image[] galaxyNebulas;
        private Image[] galaxyStars;

        // Avance 51 - acceso directo a configuracion desde menu principal.
        private RectTransform settingsButton;
        private Image settingsButtonBg;
        private TMP_Text settingsButtonIcon;
        private Image settingsButtonIconImage;
        private Image settingsButtonGlow;
        private bool settingsButtonHover;

        // Avance 67 real: boton Ayuda y panel Como Jugar.
        private RectTransform helpButton;
        private Image helpButtonBg;
        private Image helpButtonIconHorizontal;
        private Image helpButtonIconVertical;
        private Image helpButtonGlow;
        private TMP_Text helpButtonLabel;
        private bool helpButtonHover;

        private CanvasGroup helpPanelGroup;
        private RectTransform helpPanelBox;
        private TMP_Text helpPanelTitle;
        private TMP_Text helpPanelSubtitle;
        private TMP_Text helpPanelBody;
        private TMP_Text helpPanelCounter;
        private TMP_Text helpNextText;
        private TMP_Text helpPrevText;
        private Image helpNextBg;
        private Image helpPrevBg;
        private Image helpCloseBg;
        private bool helpPanelOpen;
        private int helpPage;
        private float helpPop;

        private CanvasGroup mainMenuSettingsGroup;
        private TMP_Text mainMenuSettingsTitle;
        private TMP_Text mainMenuSettingsHint;
        private TMP_Text[] mainMenuSettingsLabels;
        private TMP_Text[] mainMenuSettingsValues;
        private Image[] mainMenuSettingsRows;
        private bool isMainMenuSettingsOpen;
        private int selectedMainMenuSettingsOption;

        private const int MainMenuSettingsOptionCount = 7;
        private const string BrightnessPrefsKey = "ProjectBeat_Brightness";
        private const string MasterVolumePrefsKey = "ProjectBeat_MasterVolume";
        private const string ResolutionPrefsKey = "ProjectBeat_ResolutionIndex";
        private const string DisplayModePrefsKey = "ProjectBeat_DisplayModeIndex";
        private static readonly Vector2Int[] ResolutionOptions =
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1366, 768),
            new Vector2Int(1600, 900),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440)
        };
        private static readonly string[] DisplayModeNames = { "PANTALLA COMPLETA", "VENTANA", "VENTANA SIN BORDES" };
        private float menuBrightness = 1f;
        private float menuVolume = 1f;
        private int menuResolutionIndex = 3;
        private int menuDisplayModeIndex = 0;

        private const string MenuMusicResourcePath = "Timecop1983";
        private static bool forceMainMenuInMemory;
        private static bool showMenuImmediatelyOnNextInstance;
        private static bool skipStartupOnce;
        private bool showMenuImmediately;
        public static bool IsMainMenuVisible { get; private set; }
        public static bool SuppressGameplayStartup { get; private set; }

        // Avance 58: paleta galactica/neon. Se reduce el negro puro y se prioriza
        // morado, violeta, rosado y magenta con cian como color de apoyo.
        private static readonly Color BgDark = new Color(0.045f, 0.012f, 0.105f, 1f);
        private static readonly Color Panel = new Color(0.105f, 0.026f, 0.170f, 0.86f);
        private static readonly Color PanelSoft = new Color(0.150f, 0.050f, 0.250f, 0.64f);
        private static readonly Color NeonCyan = new Color(0.0f, 0.94f, 1f, 1f);
        private static readonly Color NeonYellow = new Color(1.0f, 0.84f, 1.0f, 1f);
        private static readonly Color NeonOrange = new Color(1.0f, 0.18f, 0.78f, 1f);
        private static readonly Color NeonPurple = new Color(0.56f, 0.22f, 1.0f, 1f);
        private static readonly Color NeonPink = new Color(1.0f, 0.22f, 0.72f, 1f);
        private static readonly Color TextNormal = new Color(0.98f, 0.92f, 1f, 1f);
        private static readonly Color TextDim = new Color(0.72f, 0.62f, 0.94f, 1f);

        private static readonly string[] HelpTitles =
        {
            "CONTROLES BASICOS",
            "HOLD NOTES",
            "PRECISION",
            "COMBO Y MULTIPLICADOR",
            "PAUSA Y OPCIONES"
        };

        private static readonly string[] HelpBodies =
        {
            "Usa D / F / J / K para golpear las notas.\nPresiona cuando la nota llegue a la linea de golpe.",
            "Algunas notas son largas.\nMantén presionada la tecla durante toda la nota.\nSuelta al terminar la hold note.",
            "PERFECTO, BIEN, MAL y FALLO dependen de tu precision.\nMejor precision entrega mejor puntuacion.",
            "Encadenar aciertos aumenta el combo.\nMientras mas combo logras, mayor sera el multiplicador.\nFallar rompe la racha.",
            "ESC abre pausa durante el nivel.\nDesde pausa puedes continuar, reiniciar, configurar o volver al menu."
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateOnSceneLoad()
        {
            if (SceneManager.GetActiveScene().name.ToLower().Contains("preview")) return;
            if (FindObjectOfType<StartupFlowController>() != null) return;

            // Avance 48: el flujo de inicio ya no depende de PlayerPrefs.
            // PlayerPrefs quedaba guardado en un computador y provocaba comportamientos
            // distintos en PCs limpias o con cache antigua. Estos flags son solo de memoria
            // y duran lo necesario para una recarga de escena.
            if (forceMainMenuInMemory)
            {
                forceMainMenuInMemory = false;
                showMenuImmediatelyOnNextInstance = true;
                skipStartupOnce = false;
            }
            else if (skipStartupOnce)
            {
                skipStartupOnce = false;
                SuppressGameplayStartup = false;
                IsMainMenuVisible = false;
                return;
            }

            GameObject go = new GameObject("StartupFlowController");
            go.AddComponent<StartupFlowController>();
        }

        public static void RequestSkipStartupOnce()
        {
            skipStartupOnce = true;
            forceMainMenuInMemory = false;
            showMenuImmediatelyOnNextInstance = false;
            SuppressGameplayStartup = false;
            IsMainMenuVisible = false;
        }

        public static void RequestMainMenuOnNextLoad()
        {
            forceMainMenuInMemory = true;
            skipStartupOnce = false;
            SuppressGameplayStartup = true;
        }

        public static void ForceShowMainMenuOnCurrentScene()
        {
            forceMainMenuInMemory = false;
            skipStartupOnce = false;
            SuppressGameplayStartup = true;

            StartupFlowController existing = FindObjectOfType<StartupFlowController>();
            if (existing != null)
            {
                existing.ForceShowMenuImmediate();
                return;
            }

            showMenuImmediatelyOnNextInstance = true;
            GameObject go = new GameObject("StartupFlowController");
            go.AddComponent<StartupFlowController>();
        }

        private void Awake()
        {
            showMenuImmediately = showMenuImmediatelyOnNextInstance;
            showMenuImmediatelyOnNextInstance = false;
            gameController = FindObjectOfType<GameController>();
            pauseMenu = FindObjectOfType<PauseMenu>();

            DisableGameplayWhileMenuIsVisible();

            Time.timeScale = 1f;
            BuildUI();
            SetupMenuMusic();
            if (showMenuImmediately)
                ForceShowMenuImmediate();
            else
                StartCoroutine(FlowRoutine());
        }

        public void ForceShowMenuImmediate()
        {
            StopAllCoroutines();
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            gameController = FindObjectOfType<GameController>();
            pauseMenu = FindObjectOfType<PauseMenu>();
            DisableGameplayWhileMenuIsVisible();

            if (canvas == null || rootGroup == null || splashGroup == null || menuGroup == null)
                BuildUI();

            SetupMenuMusic();
            IsMainMenuVisible = true;
            rootGroup.alpha = 1f;
            splashGroup.alpha = 0f;
            splashGroup.interactable = false;
            splashGroup.blocksRaycasts = false;
            menuGroup.alpha = 1f;
            menuGroup.interactable = true;
            menuGroup.blocksRaycasts = true;
            selectedIndex = 0;
            lastVisualSelectedIndex = -1;
            selectionPop = 1f;
        }

        private void Update()
        {
            pulse += Time.unscaledDeltaTime * 4.2f;
            selectionPop = Mathf.MoveTowards(selectionPop, 0f, Time.unscaledDeltaTime * 3.8f);
            confirmPop = Mathf.MoveTowards(confirmPop, 0f, Time.unscaledDeltaTime * 4.5f);
            helpPop = Mathf.MoveTowards(helpPop, 0f, Time.unscaledDeltaTime * 5.0f);

            if (selectedIndex != lastVisualSelectedIndex)
            {
                lastVisualSelectedIndex = selectedIndex;
                selectionPop = 1f;
            }

            AnimateButtons();
            AnimateMenuVisuals();
            AnimateSettingsButton();
            AnimateHelpButton();
            AnimateHelpPanel();

            if (menuGroup == null || menuGroup.alpha < 0.95f) return;

            if (helpPanelOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    CloseHelpPanel();
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.RightArrow))
                    NextHelpPage();
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                    PreviousHelpPage();
                return;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) ||
                Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                selectedIndex += (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) ? -1 : 1;
                if (selectedIndex < 0) selectedIndex = MainMenuOptionCount - 1;
                if (selectedIndex >= MainMenuOptionCount) selectedIndex = 0;
                selectionPop = 1f;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                confirmPop = 1f;
                if (selectedIndex == 0)
                    StartCoroutine(OpenTutorialRoutine());
                else if (selectedIndex == 1)
                    StartCoroutine(OpenArcadeRoutine());
                else if (selectedIndex == 2)
                    StartCoroutine(ShakeLockedRoutine());
                else
                    QuitGame();
            }
        }


        private void DisableGameplayWhileMenuIsVisible()
        {
            SuppressGameplayStartup = true;
            IsMainMenuVisible = true;

            if (gameController != null)
                gameController.enabled = false;

            // Avance 45: el menu inicial no debe convivir visualmente con el panel de tutorial.
            TutorialOverlayController[] tutorialOverlays = FindObjectsOfType<TutorialOverlayController>();
            for (int i = 0; i < tutorialOverlays.Length; i++)
            {
                if (tutorialOverlays[i] != null)
                    Destroy(tutorialOverlays[i].gameObject);
            }

            // El menu inicial debe sentirse como pantalla limpia, no como overlay del nivel.
            Time.timeScale = 1f;
        }

        private void OnDestroy()
        {
            if (rootGroup != null && rootGroup.alpha > 0.01f)
                IsMainMenuVisible = false;
        }

        private void QuitGame()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator FlowRoutine()
        {
            IsMainMenuVisible = true;
            rootGroup.alpha = 1f;
            splashGroup.alpha = 0f;
            menuGroup.alpha = 0f;
            menuGroup.interactable = false;
            menuGroup.blocksRaycasts = false;

            yield return Fade(splashGroup, 0f, 1f, 0.55f);
            yield return new WaitForSecondsRealtime(1.15f);
            yield return Fade(splashGroup, 1f, 0f, 0.45f);
            yield return Fade(menuGroup, 0f, 1f, 0.55f);

            menuGroup.interactable = true;
            menuGroup.blocksRaycasts = true;
        }

        private IEnumerator OpenTutorialRoutine()
        {
            IsMainMenuVisible = false;
            menuGroup.interactable = false;
            menuGroup.blocksRaycasts = false;
            StopMenuMusic();
            Cursor.visible = false;
            yield return Fade(rootGroup, 1f, 0f, 0.35f);

            if (LevelManager.Instance != null)
                LevelManager.Instance.SetLevel(0);

            // Avance 48: el tutorial se inicia siempre desde una escena limpia
            // usando flag de memoria, no PlayerPrefs persistente.
            RequestSkipStartupOnce();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }

        private IEnumerator OpenArcadeRoutine()
        {
            IsMainMenuVisible = false;
            menuGroup.interactable = false;
            menuGroup.blocksRaycasts = false;
            StopMenuMusic();
            Cursor.visible = true;
            yield return Fade(rootGroup, 1f, 0f, 0.35f);

            // Avance 48: Arcade debe abrir siempre en un estado estable.
            // Si el jugador venia desde Tutorial, se selecciona el primer nivel arcade
            // para evitar que el selector aparezca mezclado con el Tutorial.
            if (LevelManager.Instance != null)
                LevelManager.Instance.SelectFirstArcadeLevel();

            if (pauseMenu != null)
                pauseMenu.OpenLevelSelectFromStartup();
            else if (gameController != null)
                gameController.enabled = true;

            Destroy(gameObject);
        }

        private IEnumerator ShakeLockedRoutine()
        {
            RectTransform target = futureButton;
            if (target == null) yield break;

            Vector2 basePos = target.anchoredPosition;
            for (int i = 0; i < 8; i++)
            {
                float dir = i % 2 == 0 ? 1f : -1f;
                target.anchoredPosition = basePos + new Vector2(dir * 8f, 0f);
                yield return new WaitForSecondsRealtime(0.025f);
            }
            target.anchoredPosition = basePos;
        }

        private void SetupMenuMusic()
        {
            if (menuMusicSource != null) return;

            menuMusicClip = Resources.Load<AudioClip>(MenuMusicResourcePath);
            if (menuMusicClip == null) return;

            menuMusicSource = gameObject.AddComponent<AudioSource>();
            menuMusicSource.clip = menuMusicClip;
            menuMusicSource.loop = true;
            menuMusicSource.playOnAwake = false;
            menuMusicSource.spatialBlend = 0f;
            menuMusicSource.volume = 0.46f * Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumePrefsKey, 1f));
            menuMusicSource.priority = 32;
            menuMusicSource.ignoreListenerPause = true;
            menuMusicSource.Play();
        }

        private void StopMenuMusic()
        {
            if (menuMusicSource == null) return;
            menuMusicSource.Stop();
        }

        private void BuildUI()
        {
            EnsureEventSystem();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            canvas = new GameObject("StartupCanvas").AddComponent<Canvas>();
            canvas.transform.SetParent(transform, false);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9000;
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;

            rootGroup = canvas.gameObject.AddComponent<CanvasGroup>();

            Image bg = CreateImage("Background", canvas.transform, BgDark);
            Stretch(bg.rectTransform);
            bg.raycastTarget = false;
            CreateGalacticBackground(canvas.transform);

            // Avance 43: ambientacion del menu inicial. Elementos visuales livianos,
            // creados por codigo para no depender de assets externos ni tocar gameplay.
            rgbAuraImage = CreateImage("MenuAmbientAura", canvas.transform, new Color(1f, 0.18f, 0.78f, 0.105f));
            rgbAuraImage.sprite = MakeRadialGlowSprite(160);
            rgbAuraImage.rectTransform.sizeDelta = new Vector2(1060f, 600f);
            rgbAuraImage.rectTransform.anchoredPosition = new Vector2(0f, 0f);

            topRgbLine = CreateLine("MenuTopNeonLine", canvas.transform, new Vector2(0f, 316f), new Vector2(760f, 4f), NeonPink);
            bottomRgbLine = CreateLine("MenuBottomNeonLine", canvas.transform, new Vector2(0f, -316f), new Vector2(760f, 4f), NeonCyan);

            ambientBars = new Image[12];
            for (int i = 0; i < ambientBars.Length; i++)
            {
                float x = -560f + i * 102f;
                float h = 120f + (i % 4) * 42f;
                Color barBase = i % 3 == 0 ? NeonPink : (i % 3 == 1 ? NeonPurple : NeonCyan);
                Image bar = CreateLine("MenuAmbientBar_" + i, canvas.transform, new Vector2(x, -230f + (i % 3) * 20f), new Vector2(3.5f, h), barBase);
                bar.color = new Color(bar.color.r, bar.color.g, bar.color.b, 0.22f);
                bar.rectTransform.localEulerAngles = new Vector3(0f, 0f, i % 2 == 0 ? -18f : 18f);
                ambientBars[i] = bar;
            }

            CreateMainMenuMusicBackground(canvas.transform);

            splashGroup = CreateGroup("Splash", canvas.transform);
            TMP_Text logo = CreateText("Logo", splashGroup.transform, "PROJECT BEAT", 54, NeonYellow, FontStyles.Bold);
            logo.alignment = TextAlignmentOptions.Center;
            logo.rectTransform.anchoredPosition = new Vector2(0f, 24f);
            logo.rectTransform.sizeDelta = new Vector2(900f, 80f);

            TMP_Text subtitle = CreateText("Subtitle", splashGroup.transform, "RHYTHM ARCADE EXPERIENCE", 18, NeonCyan, FontStyles.Bold);
            subtitle.alignment = TextAlignmentOptions.Center;
            subtitle.characterSpacing = 10f;
            subtitle.rectTransform.anchoredPosition = new Vector2(0f, -42f);
            subtitle.rectTransform.sizeDelta = new Vector2(900f, 42f);

            menuGroup = CreateGroup("MainMenu", canvas.transform);
            Image panel = CreateImage("MainPanel", menuGroup.transform, new Color(0.090f, 0.025f, 0.150f, 0.82f));
            panel.rectTransform.sizeDelta = new Vector2(760f, 530f);
            panel.rectTransform.anchoredPosition = Vector2.zero;
            panel.type = Image.Type.Sliced;
            panel.sprite = MakeSprite(new Color(1f,1f,1f,1f));

            menuTitleGlow = CreateImage("MenuTitleNeonGlow", menuGroup.transform, new Color(1f, 0.22f, 0.72f, 0.20f));
            menuTitleGlow.sprite = MakeRadialGlowSprite(160);
            menuTitleGlow.rectTransform.sizeDelta = new Vector2(690f, 165f);
            menuTitleGlow.rectTransform.anchoredPosition = new Vector2(0f, 188f);
            menuTitleGlow.raycastTarget = false;

            TMP_Text title = CreateText("MenuTitle", menuGroup.transform, "PROJECT BEAT", 56, NeonYellow, FontStyles.Bold);
            menuTitleText = title;
            title.alignment = TextAlignmentOptions.Center;
            title.rectTransform.anchoredPosition = new Vector2(0f, 184f);
            title.rectTransform.sizeDelta = new Vector2(640f, 78f);

            TMP_Text label = CreateText("MenuSubtitle", menuGroup.transform, "SELECCIONA MODO DE JUEGO", 18, NeonCyan, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            label.characterSpacing = 4f;
            label.rectTransform.anchoredPosition = new Vector2(0f, 132f);
            label.rectTransform.sizeDelta = new Vector2(640f, 38f);

            CreateLine("MainPanelTopAccent", menuGroup.transform, new Vector2(0f, 238f), new Vector2(560f, 3f), NeonPink);
            CreateLine("MainPanelBottomAccent", menuGroup.transform, new Vector2(0f, -238f), new Vector2(560f, 3f), NeonCyan);
            CreateMenuBeatRings(menuGroup.transform);

            tutorialButton = CreateModeButton(menuGroup.transform, "TutorialButton", new Vector2(0f, 86f), true, out tutorialBg, out tutorialLabel, "TUTORIAL", "Aprende a jugar");
            arcadeButton = CreateModeButton(menuGroup.transform, "ArcadeButton", new Vector2(0f, -4f), true, out arcadeBg, out arcadeLabel, "ARCADE", "Selector de niveles");
            futureButton = CreateModeButton(menuGroup.transform, "FutureButton", new Vector2(0f, -94f), false, out futureBg, out futureLabel, "PROXIMAMENTE", "Nuevo modo futuro");
            exitButton = CreateModeButton(menuGroup.transform, "ExitButton", new Vector2(0f, -184f), true, out exitBg, out exitLabel, "SALIR", "Cerrar el juego");

            CreateMainMenuSettingsButton(menuGroup.transform);
            CreateMainMenuHelpButton(menuGroup.transform);
            BuildMainMenuSettingsPanel();
            BuildHelpPanel(menuGroup.transform);

            TMP_Text hint = CreateText("Hint", menuGroup.transform, "[W/S] Navegar     [ENTER] Confirmar     [MOUSE] Seleccionar", 14, TextDim, FontStyles.Bold);
            hint.alignment = TextAlignmentOptions.Center;
            hint.rectTransform.anchoredPosition = new Vector2(0f, -246f);
            hint.rectTransform.sizeDelta = new Vector2(620f, 32f);
        }

        private void CreateGalacticBackground(Transform parent)
        {
            backgroundGradient = CreateImage("GalacticGradient", parent, Color.white);
            Stretch(backgroundGradient.rectTransform);
            backgroundGradient.sprite = MakeVerticalGradientSprite(96,
                new Color(0.10f, 0.025f, 0.21f, 1f),
                new Color(0.32f, 0.055f, 0.42f, 1f),
                new Color(0.050f, 0.020f, 0.125f, 1f));
            backgroundGradient.raycastTarget = false;

            galaxyNebulas = new Image[5];
            Vector2[] nebulaPositions =
            {
                new Vector2(-420f, 150f),
                new Vector2(385f, 120f),
                new Vector2(0f, -95f),
                new Vector2(-170f, -220f),
                new Vector2(280f, -235f)
            };
            Vector2[] nebulaSizes =
            {
                new Vector2(520f, 320f),
                new Vector2(520f, 300f),
                new Vector2(760f, 420f),
                new Vector2(420f, 230f),
                new Vector2(420f, 230f)
            };

            for (int i = 0; i < galaxyNebulas.Length; i++)
            {
                Color c = i % 3 == 0 ? NeonPink : (i % 3 == 1 ? NeonPurple : NeonCyan);
                Image nebula = CreateImage("GalacticNebula_" + i, parent, new Color(c.r, c.g, c.b, 0.13f));
                nebula.sprite = MakeRadialGlowSprite(160);
                nebula.rectTransform.sizeDelta = nebulaSizes[i];
                nebula.rectTransform.anchoredPosition = nebulaPositions[i];
                nebula.raycastTarget = false;
                galaxyNebulas[i] = nebula;
            }

            galaxyStars = new Image[42];
            for (int i = 0; i < galaxyStars.Length; i++)
            {
                float x = -610f + ((i * 97) % 1220);
                float y = -310f + ((i * 53) % 620);
                Color c = i % 4 == 0 ? NeonCyan : (i % 4 == 1 ? NeonPink : (i % 4 == 2 ? NeonPurple : TextNormal));
                Image star = CreateImage("GalacticStar_" + i, parent, new Color(c.r, c.g, c.b, 0.18f));
                star.sprite = MakeRadialGlowSprite(32);
                float size = 5f + (i % 5) * 2.2f;
                star.rectTransform.sizeDelta = new Vector2(size, size);
                star.rectTransform.anchoredPosition = new Vector2(x, y);
                star.raycastTarget = false;
                galaxyStars[i] = star;
            }
        }

        private void CreateMainMenuMusicBackground(Transform parent)
        {
            equalizerBars = new Image[18];
            for (int i = 0; i < equalizerBars.Length; i++)
            {
                float x = -510f + i * 60f;
                Color barColor = i % 3 == 0 ? NeonPink : (i % 3 == 1 ? NeonPurple : NeonCyan);
                Image bar = CreateLine("MenuEqualizerBar_" + i, parent, new Vector2(x, -292f), new Vector2(18f, 34f), barColor);
                bar.color = new Color(bar.color.r, bar.color.g, bar.color.b, 0.18f);
                bar.raycastTarget = false;
                equalizerBars[i] = bar;
            }

            waveLines = new Image[10];
            for (int i = 0; i < waveLines.Length; i++)
            {
                float y = 238f - i * 38f;
                float x = i % 2 == 0 ? -430f : 430f;
                Color c = i % 3 == 0 ? NeonPink : (i % 3 == 1 ? NeonPurple : NeonCyan);
                Image line = CreateLine("MenuWaveLine_" + i, parent, new Vector2(x, y), new Vector2(90f + i * 12f, 2.2f), c);
                line.color = new Color(c.r, c.g, c.b, 0.08f);
                line.rectTransform.localEulerAngles = new Vector3(0f, 0f, i % 2 == 0 ? 10f : -10f);
                line.raycastTarget = false;
                waveLines[i] = line;
            }

            beatParticles = new Image[22];
            for (int i = 0; i < beatParticles.Length; i++)
            {
                float x = -575f + (i * 113f) % 1150f;
                float y = -180f + (i * 71f) % 380f;
                Color c = i % 2 == 0 ? NeonCyan : NeonOrange;
                Image particle = CreateImage("MenuBeatParticle_" + i, parent, c);
                particle.rectTransform.sizeDelta = new Vector2(6f + (i % 3) * 2f, 6f + (i % 3) * 2f);
                particle.rectTransform.anchoredPosition = new Vector2(x, y);
                particle.rectTransform.localEulerAngles = new Vector3(0f, 0f, 45f);
                particle.color = new Color(c.r, c.g, c.b, 0.06f);
                particle.raycastTarget = false;
                beatParticles[i] = particle;
            }
        }

        private void CreateMenuBeatRings(Transform parent)
        {
            beatRings = new Image[4];
            Vector2[] positions =
            {
                new Vector2(-302f, 142f),
                new Vector2(302f, 142f),
                new Vector2(-304f, -142f),
                new Vector2(304f, -142f)
            };

            for (int i = 0; i < beatRings.Length; i++)
            {
                Color c = i % 2 == 0 ? NeonPink : NeonPurple;
                Image ring = CreateImage("MainPanelBeatRing_" + i, parent, new Color(c.r, c.g, c.b, 0.13f));
                ring.sprite = MakeRingSprite(96, 0.46f, 0.39f);
                ring.rectTransform.sizeDelta = new Vector2(92f, 92f);
                ring.rectTransform.anchoredPosition = positions[i];
                ring.raycastTarget = false;
                beatRings[i] = ring;
            }
        }

        private void AnimateGalaxyBackground(float wave)
        {
            if (backgroundGradient != null)
            {
                backgroundGradient.color = Color.Lerp(Color.white, new Color(1f, 0.82f, 1f, 1f), wave * 0.18f);
            }

            if (galaxyNebulas != null)
            {
                for (int i = 0; i < galaxyNebulas.Length; i++)
                {
                    Image nebula = galaxyNebulas[i];
                    if (nebula == null) continue;
                    float local = (Mathf.Sin(pulse * 0.36f + i * 0.88f) + 1f) * 0.5f;
                    Color c = i % 3 == 0 ? NeonPink : (i % 3 == 1 ? NeonPurple : NeonCyan);
                    nebula.color = new Color(c.r, c.g, c.b, 0.075f + local * 0.115f);
                    nebula.rectTransform.localScale = Vector3.one * (0.96f + local * 0.10f);
                }
            }

            if (galaxyStars != null)
            {
                for (int i = 0; i < galaxyStars.Length; i++)
                {
                    Image star = galaxyStars[i];
                    if (star == null) continue;
                    float local = (Mathf.Sin(pulse * 0.92f + i * 1.37f) + 1f) * 0.5f;
                    Color c = i % 4 == 0 ? NeonCyan : (i % 4 == 1 ? NeonPink : (i % 4 == 2 ? NeonPurple : TextNormal));
                    star.color = new Color(c.r, c.g, c.b, 0.08f + local * 0.23f);
                    star.rectTransform.localScale = Vector3.one * (0.72f + local * 0.55f);
                }
            }
        }

        private void AnimateMusicalBackground(float wave)
        {
            if (equalizerBars != null)
            {
                for (int i = 0; i < equalizerBars.Length; i++)
                {
                    Image bar = equalizerBars[i];
                    if (bar == null) continue;
                    float local = (Mathf.Sin(pulse * 1.4f + i * 0.47f) + 1f) * 0.5f;
                    Color c = i % 3 == 0 ? NeonPink : (i % 3 == 1 ? NeonPurple : NeonCyan);
                    Vector2 size = bar.rectTransform.sizeDelta;
                    size.y = 18f + local * 72f;
                    bar.rectTransform.sizeDelta = size;
                    bar.color = new Color(c.r, c.g, c.b, 0.045f + local * 0.16f);
                }
            }

            if (waveLines != null)
            {
                for (int i = 0; i < waveLines.Length; i++)
                {
                    Image line = waveLines[i];
                    if (line == null) continue;
                    float local = (Mathf.Sin(pulse * 0.82f + i * 0.8f) + 1f) * 0.5f;
                    Color c = i % 3 == 0 ? NeonPink : (i % 3 == 1 ? NeonPurple : NeonCyan);
                    Vector2 size = line.rectTransform.sizeDelta;
                    size.x = 74f + local * 128f;
                    line.rectTransform.sizeDelta = size;
                    line.color = new Color(c.r, c.g, c.b, 0.035f + local * 0.13f);
                }
            }

            if (beatParticles != null)
            {
                for (int i = 0; i < beatParticles.Length; i++)
                {
                    Image particle = beatParticles[i];
                    if (particle == null) continue;
                    float local = (Mathf.Sin(pulse * 0.72f + i * 1.11f) + 1f) * 0.5f;
                    Color c = i % 3 == 0 ? NeonPink : (i % 3 == 1 ? NeonPurple : NeonCyan);
                    particle.color = new Color(c.r, c.g, c.b, 0.025f + local * 0.14f);
                    particle.rectTransform.localScale = Vector3.one * (0.8f + local * 0.55f);
                }
            }

            if (beatRings != null)
            {
                for (int i = 0; i < beatRings.Length; i++)
                {
                    Image ring = beatRings[i];
                    if (ring == null) continue;
                    float local = (Mathf.Sin(pulse * 0.7f + i * 0.95f) + 1f) * 0.5f;
                    Color c = i % 3 == 0 ? NeonPink : (i % 3 == 1 ? NeonPurple : NeonCyan);
                    ring.color = new Color(c.r, c.g, c.b, 0.045f + local * 0.085f);
                    ring.rectTransform.localScale = Vector3.one * (0.88f + local * 0.22f);
                }
            }
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private void AddMouseEventsToModeButton(GameObject target, int index, bool available)
        {
            if (target == null) return;
            EventTrigger trigger = target.GetComponent<EventTrigger>();
            if (trigger == null) trigger = target.AddComponent<EventTrigger>();
            if (trigger.triggers == null) trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();

            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((_) =>
            {
                selectedIndex = index;
                selectionPop = 1f;
            });
            trigger.triggers.Add(enter);

            EventTrigger.Entry click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            click.callback.AddListener((_) =>
            {
                selectedIndex = index;
                confirmPop = 1f;
                if (index == 0)
                    StartCoroutine(OpenTutorialRoutine());
                else if (index == 1)
                    StartCoroutine(OpenArcadeRoutine());
                else if (index == 2)
                    StartCoroutine(ShakeLockedRoutine());
                else
                    QuitGame();
            });
            trigger.triggers.Add(click);
        }


        private void CreateMainMenuHelpButton(Transform parent)
        {
            GameObject go = new GameObject("HelpButton");
            go.transform.SetParent(parent, false);
            helpButton = go.AddComponent<RectTransform>();
            helpButton.sizeDelta = new Vector2(108f, 108f);
            helpButton.anchoredPosition = new Vector2(-455f, 142f);

            helpButtonBg = go.AddComponent<Image>();
            helpButtonBg.sprite = MakeSprite(Color.white);
            helpButtonBg.type = Image.Type.Sliced;
            helpButtonBg.color = new Color(0.115f, 0.030f, 0.195f, 0.92f);

            helpButtonGlow = CreateImage("HelpButtonGlow", go.transform, new Color(0f, 0.94f, 1f, 0.18f));
            helpButtonGlow.sprite = MakeRadialGlowSprite(96);
            helpButtonGlow.rectTransform.sizeDelta = new Vector2(122f, 122f);
            helpButtonGlow.rectTransform.anchoredPosition = Vector2.zero;
            helpButtonGlow.raycastTarget = false;

            helpButtonIconHorizontal = CreateLine("HelpPlusHorizontal", go.transform, new Vector2(0f, 8f), new Vector2(56f, 12f), NeonCyan);
            helpButtonIconHorizontal.raycastTarget = false;
            helpButtonIconVertical = CreateLine("HelpPlusVertical", go.transform, new Vector2(0f, 8f), new Vector2(12f, 56f), NeonCyan);
            helpButtonIconVertical.raycastTarget = false;

            TMP_Text question = CreateText("HelpQuestionMark", go.transform, "?", 28, Color.white, FontStyles.Bold);
            question.alignment = TextAlignmentOptions.Center;
            question.rectTransform.sizeDelta = new Vector2(64f, 44f);
            question.rectTransform.anchoredPosition = new Vector2(0f, 4f);

            helpButtonLabel = CreateText("HelpLabel", go.transform, "AYUDA", 12, NeonPink, FontStyles.Bold);
            helpButtonLabel.alignment = TextAlignmentOptions.Center;
            helpButtonLabel.characterSpacing = 3f;
            helpButtonLabel.rectTransform.sizeDelta = new Vector2(108f, 24f);
            helpButtonLabel.rectTransform.anchoredPosition = new Vector2(0f, -38f);

            EventTrigger trigger = go.AddComponent<EventTrigger>();
            trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();

            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((_) =>
            {
                helpButtonHover = true;
                if (helpButtonBg != null) helpButtonBg.color = new Color(0.24f, 0.06f, 0.34f, 0.98f);
            });
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((_) =>
            {
                helpButtonHover = false;
                if (helpButtonBg != null) helpButtonBg.color = new Color(0.115f, 0.030f, 0.195f, 0.92f);
            });
            trigger.triggers.Add(exit);

            EventTrigger.Entry click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            click.callback.AddListener((_) =>
            {
                confirmPop = 1f;
                OpenHelpPanel();
            });
            trigger.triggers.Add(click);
        }

        private void BuildHelpPanel(Transform parent)
        {
            GameObject root = new GameObject("HelpHowToPlayPanel");
            root.transform.SetParent(parent, false);
            RectTransform rt = root.AddComponent<RectTransform>();
            Stretch(rt);

            helpPanelGroup = root.AddComponent<CanvasGroup>();
            helpPanelGroup.alpha = 0f;
            helpPanelGroup.interactable = false;
            helpPanelGroup.blocksRaycasts = false;

            Image dim = CreateImage("HelpDim", root.transform, new Color(0.010f, 0.000f, 0.040f, 0.70f));
            Stretch(dim.rectTransform);
            dim.raycastTarget = true;

            Image glow = CreateImage("HelpGlow", root.transform, new Color(1f, 0.20f, 0.75f, 0.18f));
            glow.sprite = MakeRadialGlowSprite(160);
            glow.rectTransform.sizeDelta = new Vector2(860f, 560f);
            glow.rectTransform.anchoredPosition = Vector2.zero;
            glow.raycastTarget = false;

            Image box = CreateImage("HelpPanelBox", root.transform, new Color(0.095f, 0.030f, 0.160f, 0.98f));
            box.sprite = MakeSprite(Color.white);
            box.type = Image.Type.Sliced;
            box.rectTransform.sizeDelta = new Vector2(840f, 520f);
            box.rectTransform.anchoredPosition = Vector2.zero;
            box.raycastTarget = true;
            helpPanelBox = box.rectTransform;

            CreateLine("HelpTopLine", box.transform, new Vector2(0f, 236f), new Vector2(700f, 4f), NeonCyan).raycastTarget = false;
            CreateLine("HelpBottomLine", box.transform, new Vector2(0f, -236f), new Vector2(700f, 4f), NeonPink).raycastTarget = false;

            helpPanelTitle = CreateText("HelpTitle", box.transform, "COMO JUGAR", 46, Color.white, FontStyles.Bold);
            helpPanelTitle.alignment = TextAlignmentOptions.Center;
            helpPanelTitle.characterSpacing = 5f;
            helpPanelTitle.rectTransform.sizeDelta = new Vector2(720f, 62f);
            helpPanelTitle.rectTransform.anchoredPosition = new Vector2(0f, 190f);

            helpPanelSubtitle = CreateText("HelpSubtitle", box.transform, "CONTROLES BASICOS", 22, NeonCyan, FontStyles.Bold);
            helpPanelSubtitle.alignment = TextAlignmentOptions.Center;
            helpPanelSubtitle.characterSpacing = 3f;
            helpPanelSubtitle.rectTransform.sizeDelta = new Vector2(720f, 36f);
            helpPanelSubtitle.rectTransform.anchoredPosition = new Vector2(0f, 125f);

            Image bodyBox = CreateImage("HelpBodyBox", box.transform, new Color(0.050f, 0.012f, 0.105f, 0.82f));
            bodyBox.sprite = MakeSprite(Color.white);
            bodyBox.type = Image.Type.Sliced;
            bodyBox.rectTransform.sizeDelta = new Vector2(700f, 210f);
            bodyBox.rectTransform.anchoredPosition = new Vector2(0f, 8f);
            bodyBox.raycastTarget = false;

            helpPanelBody = CreateText("HelpBody", bodyBox.transform, "", 28, TextNormal, FontStyles.Bold);
            helpPanelBody.alignment = TextAlignmentOptions.Center;
            helpPanelBody.enableWordWrapping = true;
            helpPanelBody.rectTransform.sizeDelta = new Vector2(650f, 170f);
            helpPanelBody.rectTransform.anchoredPosition = Vector2.zero;

            helpPanelCounter = CreateText("HelpCounter", box.transform, "1 / 5", 16, TextDim, FontStyles.Bold);
            helpPanelCounter.alignment = TextAlignmentOptions.Center;
            helpPanelCounter.rectTransform.sizeDelta = new Vector2(160f, 26f);
            helpPanelCounter.rectTransform.anchoredPosition = new Vector2(0f, -130f);

            helpPrevBg = CreateHelpButton(box.transform, "HelpPrev", "ANTERIOR", new Vector2(-220f, -190f), false, out helpPrevText);
            helpNextBg = CreateHelpButton(box.transform, "HelpNext", "SIGUIENTE", new Vector2(220f, -190f), true, out helpNextText);
            TMP_Text unusedCloseText;
            helpCloseBg = CreateHelpButton(box.transform, "HelpClose", "X", new Vector2(376f, 208f), false, out unusedCloseText);

            EventTrigger closeTrigger = helpCloseBg.gameObject.GetComponent<EventTrigger>();
            if (closeTrigger == null) closeTrigger = helpCloseBg.gameObject.AddComponent<EventTrigger>();
            AddSimpleClick(closeTrigger, CloseHelpPanel);

            SetHelpPage(0);
        }

        private Image CreateHelpButton(Transform parent, string name, string label, Vector2 pos, bool next, out TMP_Text labelText)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = label == "X" ? new Vector2(56f, 46f) : new Vector2(180f, 50f);
            rt.anchoredPosition = pos;

            Image bg = go.AddComponent<Image>();
            bg.sprite = MakeSprite(Color.white);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.140f, 0.040f, 0.225f, 0.96f);

            labelText = CreateText("Label", go.transform, label, label == "X" ? 28 : 18, Color.white, FontStyles.Bold);
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.rectTransform.sizeDelta = rt.sizeDelta;
            labelText.rectTransform.anchoredPosition = Vector2.zero;
            labelText.raycastTarget = false;

            EventTrigger trigger = go.AddComponent<EventTrigger>();
            trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();

            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((_) => { if (bg != null) bg.color = new Color(0.30f, 0.08f, 0.42f, 1f); });
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((_) => { if (bg != null) bg.color = new Color(0.140f, 0.040f, 0.225f, 0.96f); });
            trigger.triggers.Add(exit);

            if (label != "X")
            {
                if (next)
                    AddSimpleClick(trigger, NextHelpPage);
                else
                    AddSimpleClick(trigger, PreviousHelpPage);
            }

            return bg;
        }

        private void AddSimpleClick(EventTrigger trigger, System.Action action)
        {
            if (trigger == null || action == null) return;
            if (trigger.triggers == null) trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();
            EventTrigger.Entry click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            click.callback.AddListener((_) => action());
            trigger.triggers.Add(click);
        }

        private void OpenHelpPanel()
        {
            if (helpPanelGroup == null) return;
            helpPanelOpen = true;
            helpPop = 1f;
            SetHelpPage(0);
            helpPanelGroup.alpha = 1f;
            helpPanelGroup.interactable = true;
            helpPanelGroup.blocksRaycasts = true;
        }

        private void CloseHelpPanel()
        {
            helpPanelOpen = false;
            if (helpPanelGroup == null) return;
            helpPanelGroup.alpha = 0f;
            helpPanelGroup.interactable = false;
            helpPanelGroup.blocksRaycasts = false;
        }

        private void NextHelpPage()
        {
            if (helpPage >= HelpTitles.Length - 1)
                CloseHelpPanel();
            else
                SetHelpPage(helpPage + 1);
        }

        private void PreviousHelpPage()
        {
            SetHelpPage(helpPage - 1);
        }

        private void SetHelpPage(int page)
        {
            int max = Mathf.Min(HelpTitles.Length, HelpBodies.Length) - 1;
            if (max < 0) return;
            helpPage = Mathf.Clamp(page, 0, max);
            if (helpPanelSubtitle != null) helpPanelSubtitle.text = HelpTitles[helpPage];
            if (helpPanelBody != null) helpPanelBody.text = HelpBodies[helpPage];
            if (helpPanelCounter != null) helpPanelCounter.text = (helpPage + 1).ToString() + " / " + (max + 1).ToString();
            if (helpNextText != null) helpNextText.text = helpPage >= max ? "CERRAR" : "SIGUIENTE";
            if (helpPrevText != null) helpPrevText.color = helpPage == 0 ? TextDim : Color.white;
        }

        private void AnimateHelpButton()
        {
            if (helpButton == null || menuGroup == null || !menuGroup.interactable) return;
            float targetScale = helpButtonHover ? 1.07f : 1f + Mathf.Sin(pulse * 1.15f) * 0.015f;
            helpButton.localScale = Vector3.Lerp(helpButton.localScale, Vector3.one * targetScale, Time.unscaledDeltaTime * 8f);

            if (helpButtonGlow != null)
            {
                float alpha = helpButtonHover ? 0.34f : 0.16f + Mathf.Sin(pulse * 1.25f) * 0.05f;
                helpButtonGlow.color = new Color(0f, 0.94f, 1f, alpha);
            }

            Color iconColor = helpButtonHover ? Color.white : NeonCyan;
            if (helpButtonIconHorizontal != null) helpButtonIconHorizontal.color = iconColor;
            if (helpButtonIconVertical != null) helpButtonIconVertical.color = iconColor;
            if (helpButtonLabel != null) helpButtonLabel.color = helpButtonHover ? Color.white : NeonPink;
        }

        private void AnimateHelpPanel()
        {
            if (!helpPanelOpen || helpPanelBox == null) return;
            float scale = 1f + helpPop * 0.045f;
            helpPanelBox.localScale = Vector3.Lerp(helpPanelBox.localScale, Vector3.one * scale, Time.unscaledDeltaTime * 10f);
            if (helpPop < 0.02f)
                helpPanelBox.localScale = Vector3.Lerp(helpPanelBox.localScale, Vector3.one, Time.unscaledDeltaTime * 8f);
        }

        private void CreateMainMenuSettingsButton(Transform parent)
        {
            GameObject go = new GameObject("SettingsGearButton");
            go.transform.SetParent(parent, false);
            settingsButton = go.AddComponent<RectTransform>();
            settingsButton.sizeDelta = new Vector2(108f, 108f);
            settingsButton.anchoredPosition = new Vector2(-455f, 0f);

            settingsButtonBg = go.AddComponent<Image>();
            settingsButtonBg.sprite = MakeSprite(Color.white);
            settingsButtonBg.type = Image.Type.Sliced;
            settingsButtonBg.color = new Color(0.115f, 0.030f, 0.195f, 0.92f);

            Image innerGlow = CreateImage("SettingsGearGlow", go.transform, new Color(1f, 0.18f, 0.78f, 0.18f));
            settingsButtonGlow = innerGlow;
            innerGlow.rectTransform.sizeDelta = new Vector2(124f, 124f);
            innerGlow.rectTransform.anchoredPosition = Vector2.zero;
            innerGlow.raycastTarget = false;

            GameObject iconGO = new GameObject("SettingsGearIconImage");
            iconGO.transform.SetParent(go.transform, false);
            RectTransform iconRt = iconGO.AddComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(70f, 70f);
            iconRt.anchoredPosition = new Vector2(0f, 8f);
            settingsButtonIconImage = iconGO.AddComponent<Image>();
            settingsButtonIconImage.sprite = MakeGearSprite(96);
            settingsButtonIconImage.color = NeonPink;
            settingsButtonIconImage.raycastTarget = false;

            TMP_Text small = CreateText("SettingsGearSmall", go.transform, "CONFIG", 12, NeonCyan, FontStyles.Bold);
            small.alignment = TextAlignmentOptions.Center;
            small.characterSpacing = 3f;
            small.rectTransform.sizeDelta = new Vector2(108f, 24f);
            small.rectTransform.anchoredPosition = new Vector2(0f, -38f);

            EventTrigger trigger = go.AddComponent<EventTrigger>();
            trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();

            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((_) =>
            {
                settingsButtonHover = true;
                if (settingsButtonBg != null) settingsButtonBg.color = new Color(0.24f, 0.06f, 0.34f, 0.98f);
                if (settingsButtonIconImage != null) settingsButtonIconImage.color = NeonCyan;
            });
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((_) =>
            {
                settingsButtonHover = false;
                if (settingsButtonBg != null) settingsButtonBg.color = new Color(0.115f, 0.030f, 0.195f, 0.92f);
                if (settingsButtonIconImage != null) settingsButtonIconImage.color = NeonPink;
            });
            trigger.triggers.Add(exit);

            EventTrigger.Entry click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            click.callback.AddListener((_) =>
            {
                confirmPop = 1f;
                OpenMainMenuSettings();
            });
            trigger.triggers.Add(click);
        }

        private void BuildMainMenuSettingsPanel()
        {
            // Avance 52: eliminado el segundo panel de configuracion del menu principal.
            // La configuracion se abre reutilizando el panel real de PauseMenu.
        }

        private void OpenMainMenuSettings()
        {
            // Avance 54: el engranaje del menu principal reutiliza EXACTAMENTE
            // el SettingsGroup real de PauseMenu.
            // Causa del panel vacio: StartupCanvas tiene sortingOrder 9000 y quedaba
            // encima del Canvas de pausa. Aunque SettingsGroup se activaba, el canvas
            // del menu inicial seguia tapandolo con su fondo/lineas decorativas.
            // Por eso aqui se oculta TODO el StartupCanvas mediante rootGroup antes
            // de abrir la configuracion real de PauseMenu.
            if (pauseMenu == null) pauseMenu = FindObjectOfType<PauseMenu>();
            if (pauseMenu == null) return;

            if (rootGroup != null)
            {
                rootGroup.alpha = 0f;
                rootGroup.interactable = false;
                rootGroup.blocksRaycasts = false;
            }

            if (menuGroup != null)
            {
                menuGroup.alpha = 0f;
                menuGroup.interactable = false;
                menuGroup.blocksRaycasts = false;
            }

            pauseMenu.OpenSettingsFromMainMenu(OnMainMenuSettingsClosed);
        }

        private void OnMainMenuSettingsClosed()
        {
            if (rootGroup != null)
            {
                rootGroup.alpha = 1f;
                rootGroup.interactable = true;
                rootGroup.blocksRaycasts = true;
            }

            if (menuGroup != null)
            {
                menuGroup.alpha = 1f;
                menuGroup.interactable = true;
                menuGroup.blocksRaycasts = true;
            }

            selectedIndex = 0;
        }

        private void CloseMainMenuSettings()
        {
            isMainMenuSettingsOpen = false;
            OnMainMenuSettingsClosed();
        }

        private void AnimateSettingsButton()
        {
            if (settingsButton == null || menuGroup == null || !menuGroup.interactable) return;

            float hoverBoost = settingsButtonHover ? 0.08f : 0f;
            float s = 1f + hoverBoost + Mathf.Sin(pulse * 1.15f) * 0.018f;
            settingsButton.localScale = Vector3.Lerp(settingsButton.localScale, Vector3.one * s, Time.unscaledDeltaTime * 7f);

            if (settingsButtonBg != null)
            {
                Color normal = new Color(0.115f, 0.030f, 0.195f, 0.92f);
                Color hover = new Color(0.24f, 0.06f, 0.34f, 0.98f);
                settingsButtonBg.color = Color.Lerp(settingsButtonBg.color, settingsButtonHover ? hover : normal, Time.unscaledDeltaTime * 10f);
            }

            if (settingsButtonGlow != null)
            {
                float glow = settingsButtonHover ? 0.32f : 0.14f + Mathf.Sin(pulse * 1.35f) * 0.05f;
                settingsButtonGlow.color = Color.Lerp(new Color(1f, 0.18f, 0.78f, glow), new Color(0f, 0.94f, 1f, glow), settingsButtonHover ? 1f : 0.35f);
            }

            if (settingsButtonIconImage != null)
            {
                Color animated = Color.Lerp(NeonPink, NeonCyan, (Mathf.Sin(pulse * 0.75f) + 1f) * 0.5f);
                settingsButtonIconImage.color = Color.Lerp(settingsButtonIconImage.color, settingsButtonHover ? NeonCyan : animated, Time.unscaledDeltaTime * 10f);
            }
        }

        private RectTransform CreateModeButton(Transform parent, string name, Vector2 pos, bool available, out Image bg, out TMP_Text label, string textOverride = null, string subOverride = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(530f, 76f);
            rt.anchoredPosition = pos;
            bg = go.AddComponent<Image>();
            bg.sprite = MakeSprite(Color.white);
            bg.type = Image.Type.Sliced;
            bg.color = available ? PanelSoft : new Color(0.145f, 0.055f, 0.205f, 0.62f);

            int buttonIndex = name.Contains("Tutorial") ? 0 : name.Contains("Arcade") ? 1 : name.Contains("Future") ? 2 : 3;
            Image glow = CreateImage("SelectionGlow", go.transform, new Color(0f, 0.92f, 1f, 0f));
            glow.rectTransform.sizeDelta = new Vector2(552f, 94f);
            glow.rectTransform.anchoredPosition = Vector2.zero;
            glow.raycastTarget = false;
            if (buttonIndex == 0) tutorialGlow = glow;
            else if (buttonIndex == 1) arcadeGlow = glow;
            else if (buttonIndex == 2) futureGlow = glow;
            else exitGlow = glow;

            Color accentColor = available ? NeonPink : NeonPurple;
            Image topAccent = CreateLine("ButtonTopNeonEdge", go.transform, new Vector2(0f, 37f), new Vector2(500f, 2.4f), accentColor);
            topAccent.color = new Color(accentColor.r, accentColor.g, accentColor.b, available ? 0.34f : 0.20f);
            topAccent.raycastTarget = false;
            Image bottomAccent = CreateLine("ButtonBottomNeonEdge", go.transform, new Vector2(0f, -37f), new Vector2(500f, 2.4f), NeonCyan);
            bottomAccent.color = new Color(NeonCyan.r, NeonCyan.g, NeonCyan.b, available ? 0.26f : 0.18f);
            bottomAccent.raycastTarget = false;
            Image leftBeat = CreateLine("ButtonLeftBeatMark", go.transform, new Vector2(-258f, 0f), new Vector2(4f, 42f), accentColor);
            leftBeat.color = new Color(accentColor.r, accentColor.g, accentColor.b, available ? 0.45f : 0.22f);
            leftBeat.raycastTarget = false;

            string text = textOverride ?? (available ? "ARCADE" : "PROXIMAMENTE");
            string sub = subOverride ?? (available ? "Selector de niveles" : "Nuevo modo futuro");

            label = CreateText("Text", go.transform, text + "\n" + sub, available ? 28 : 23, available ? NeonYellow : TextDim, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.sizeDelta = rt.sizeDelta;
            label.rectTransform.anchoredPosition = Vector2.zero;
            label.lineSpacing = -19f;
            AddMouseEventsToModeButton(go, buttonIndex, available);
            return rt;
        }

        private void AnimateMenuVisuals()
        {
            if (menuGroup == null || menuGroup.alpha <= 0.01f) return;

            float wave = (Mathf.Sin(pulse * 0.72f) + 1f) * 0.5f;
            AnimateGalaxyBackground(wave);
            if (menuTitleText != null)
            {
                float titleScale = 1f + Mathf.Sin(pulse * 0.55f) * 0.018f;
                menuTitleText.rectTransform.localScale = Vector3.Lerp(menuTitleText.rectTransform.localScale, Vector3.one * titleScale, Time.unscaledDeltaTime * 6f);
                menuTitleText.color = Color.Lerp(NeonYellow, Color.white, wave * 0.30f);
            }

            if (rgbAuraImage != null)
                rgbAuraImage.color = Color.Lerp(new Color(1f, 0.18f, 0.78f, 0.09f), new Color(0.56f, 0.22f, 1f, 0.13f), wave);

            if (topRgbLine != null)
                topRgbLine.color = Color.Lerp(NeonPink, NeonPurple, wave);
            if (bottomRgbLine != null)
                bottomRgbLine.color = Color.Lerp(NeonCyan, NeonPink, wave);

            if (ambientBars != null)
            {
                for (int i = 0; i < ambientBars.Length; i++)
                {
                    Image bar = ambientBars[i];
                    if (bar == null) continue;
                    float local = (Mathf.Sin(pulse * 0.9f + i * 0.65f) + 1f) * 0.5f;
                    Color baseColor = i % 3 == 0 ? NeonPink : (i % 3 == 1 ? NeonPurple : NeonCyan);
                    bar.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.08f + local * 0.18f);
                    Vector2 size = bar.rectTransform.sizeDelta;
                    size.y = 90f + local * 170f;
                    bar.rectTransform.sizeDelta = size;
                }
            }

            if (menuTitleGlow != null)
            {
                float glowAlpha = 0.08f + wave * 0.10f;
                menuTitleGlow.color = Color.Lerp(new Color(1f, 0.18f, 0.78f, glowAlpha), new Color(0.56f, 0.22f, 1f, glowAlpha + 0.04f), wave);
                menuTitleGlow.rectTransform.localScale = Vector3.one * (1f + Mathf.Sin(pulse * 0.55f) * 0.035f);
            }

            AnimateMusicalBackground(wave);
        }

        private void AnimateButtons()
        {
            AnimateButton(tutorialButton, tutorialBg, tutorialGlow, tutorialLabel, selectedIndex == 0, true);
            AnimateButton(arcadeButton, arcadeBg, arcadeGlow, arcadeLabel, selectedIndex == 1, true);
            AnimateButton(futureButton, futureBg, futureGlow, futureLabel, selectedIndex == 2, false);
            AnimateButton(exitButton, exitBg, exitGlow, exitLabel, selectedIndex == 3, true);
        }

        private void AnimateButton(RectTransform rt, Image bg, Image glow, TMP_Text label, bool selected, bool available)
        {
            if (rt == null || bg == null || label == null) return;

            float popBoost = selected ? selectionPop * 0.035f + confirmPop * 0.025f : 0f;
            float target = selected ? 1.075f + popBoost + Mathf.Sin(pulse) * 0.010f : 1f;
            rt.localScale = Vector3.Lerp(rt.localScale, Vector3.one * target, Time.unscaledDeltaTime * 12f);

            Color normal = available ? PanelSoft : new Color(0.145f, 0.055f, 0.205f, 0.62f);
            Color highlight = available ? new Color(0.72f, 0.11f, 0.92f, 0.88f) : new Color(0.31f, 0.10f, 0.38f, 0.76f);
            bg.color = Color.Lerp(bg.color, selected ? highlight : normal, Time.unscaledDeltaTime * 14f);

            Color targetText = selected ? (available ? Color.white : NeonPink) : (available ? NeonYellow : TextDim);
            label.color = Color.Lerp(label.color, targetText, Time.unscaledDeltaTime * 14f);

            if (glow != null)
            {
                Color glowColor = available ? NeonPink : NeonPurple;
                float alpha = selected ? 0.22f + selectionPop * 0.18f + Mathf.Sin(pulse * 1.5f) * 0.04f : 0f;
                glow.color = Color.Lerp(glow.color, new Color(glowColor.r, glowColor.g, glowColor.b, Mathf.Clamp01(alpha)), Time.unscaledDeltaTime * 12f);
            }
        }

        private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
        {
            float t = 0f;
            group.alpha = from;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                yield return null;
            }
            group.alpha = to;
        }

        private CanvasGroup CreateGroup(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            Stretch(rt);
            return go.AddComponent<CanvasGroup>();
        }

        private Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            Image img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private TMP_Text CreateText(string name, Transform parent, string text, int size, Color color, FontStyles style)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;
            return tmp;
        }

        private Image CreateLine(string name, Transform parent, Vector2 pos, Vector2 size, Color color)
        {
            Image img = CreateImage(name, parent, color);
            img.rectTransform.sizeDelta = size;
            img.rectTransform.anchoredPosition = pos;
            return img;
        }

        private Sprite MakeVerticalGradientSprite(int size, Color top, Color middle, Color bottom)
        {
            Texture2D tex = new Texture2D(8, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                float t = size <= 1 ? 0f : y / (float)(size - 1);
                Color c = t < 0.5f
                    ? Color.Lerp(bottom, middle, t / 0.5f)
                    : Color.Lerp(middle, top, (t - 0.5f) / 0.5f);
                for (int x = 0; x < 8; x++)
                    tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 8, size), new Vector2(0.5f, 0.5f), size);
        }

        private Sprite MakeRadialGlowSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(1f, 1f, 1f, 0f);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float max = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float r = (new Vector2(x, y) - center).magnitude / max;
                    float a = Mathf.Clamp01(1f - r);
                    a *= a;
                    tex.SetPixel(x, y, a > 0.001f ? new Color(1f, 1f, 1f, a) : clear);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private Sprite MakeRingSprite(int size, float outerRatio, float innerRatio)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(1f, 1f, 1f, 0f);
            Color solid = Color.white;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float outer = size * outerRatio;
            float inner = size * innerRatio;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float r = (new Vector2(x, y) - center).magnitude;
                    tex.SetPixel(x, y, r <= outer && r >= inner ? solid : clear);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private Sprite MakeGearSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(1f, 1f, 1f, 0f);
            Color solid = Color.white;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float outer = size * 0.40f;
            float inner = size * 0.22f;
            float hole = size * 0.13f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y) - center;
                    float r = p.magnitude;
                    float angle = Mathf.Atan2(p.y, p.x);
                    float teeth = Mathf.Abs(Mathf.Sin(angle * 6f));
                    float localOuter = Mathf.Lerp(outer * 0.82f, outer, teeth);
                    bool body = r <= localOuter && r >= hole;
                    bool ringCut = r > inner && r < inner + size * 0.035f;
                    tex.SetPixel(x, y, body && !ringCut ? solid : clear);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private Sprite MakeSprite(Color color)
        {
            Texture2D tex = new Texture2D(16, 16);
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16, 4, SpriteMeshType.FullRect, new Vector4(5, 5, 5, 5));
        }

        private void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}

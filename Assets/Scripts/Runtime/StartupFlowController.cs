using System;
using System.Collections;
using System.Collections.Generic;
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
        private TMP_Text splashPressText;

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

        // Avance 68: boton Creditos y panel de creditos desde menu principal.
        private RectTransform creditsButton;
        private Image creditsButtonBg;
        private Image creditsButtonIconImage;
        private Image creditsButtonGlow;
        private TMP_Text creditsButtonLabel;
        private bool creditsButtonHover;

        private CanvasGroup creditsPanelGroup;
        private RectTransform creditsPanelBox;
        private TMP_Text creditsPanelTitle;
        private TMP_Text creditsPanelBody;
        private Image creditsCloseBg;
        private bool creditsPanelOpen;
        private float creditsPop;

        // Avance 77: sistema base de perfiles locales del menu principal.
        private const int MaxProfiles = 5;
        private const int MaxProfileNameLength = 16;
        private const string ProfilesPrefsKey = "ProjectBeat.Profiles.v1";
        private List<ProfileEntry> profiles = new List<ProfileEntry>();
        private int selectedProfileIndex = -1;

        private RectTransform profileButton;
        private Image profileButtonBg;
        private Image profileButtonIconImage;
        private Image profileButtonGlow;
        private TMP_Text profileButtonLabel;
        private bool profileButtonHover;

        private CanvasGroup profilePanelGroup;
        private RectTransform profilePanelBox;
        private TMP_Text profileSelectedText;
        private TMP_Text profileStatusText;
        private Image[] profileRowBgs;
        private TMP_Text[] profileRowLabels;
        private Image profileCreateBg;
        private Image profileNewGameBg;
        private Image profileLoadGameBg;
        private Image profileDeleteBg;
        private Image profileCloseBg;
        private CanvasGroup profileInputGroup;
        private TMP_InputField profileNameInput;
        private CanvasGroup profileDeleteConfirmGroup;
        private bool profilePanelOpen;
        private bool profileInputOpen;
        private bool profileDeleteConfirmOpen;
        private float profilePop;

        // Avance 78: estadisticas por perfil local.
        private RectTransform statsButton;
        private Image statsButtonBg;
        private Image statsButtonIconImage;
        private Image statsButtonGlow;
        private TMP_Text statsButtonLabel;
        private bool statsButtonHover;

        private CanvasGroup statsPanelGroup;
        private RectTransform statsPanelBox;
        private TMP_Text statsPanelTitle;
        private TMP_Text statsPanelProfileText;
        private TMP_Text statsPanelBodyText;
        private ScrollRect statsScrollRect;
        private RectTransform statsScrollContent;
        private Scrollbar statsScrollbar;
        private Image statsCloseBg;
        private bool statsPanelOpen;
        private float statsPop;

        // Avance 85 rehecho: boton Logros y panel de logros por perfil.
        private RectTransform achievementsButton;
        private Image achievementsButtonBg;
        private Image achievementsButtonIconImage;
        private Image achievementsButtonGlow;
        private TMP_Text achievementsButtonLabel;
        private bool achievementsButtonHover;

        private CanvasGroup achievementsPanelGroup;
        private RectTransform achievementsPanelBox;
        private TMP_Text achievementsPanelTitle;
        private TMP_Text achievementsPanelProfileText;
        private TMP_Text achievementsPanelBodyText;
        private TMP_Text achievementsPanelEmptyText;
        private ScrollRect achievementsScrollRect;
        private RectTransform achievementsScrollContent;
        private Scrollbar achievementsScrollbar;
        private readonly List<GameObject> achievementsRowObjects = new List<GameObject>();
        private Image achievementsCloseBg;
        private bool achievementsPanelOpen;
        private float achievementsPop;

        [Serializable]
        private class ProfileEntry
        {
            public string id;
            public string name;
            public string createdAt;
            public bool partidaCreada;
            public int version = 1;
        }

        [Serializable]
        private class ProfileSaveData
        {
            public List<ProfileEntry> profiles = new List<ProfileEntry>();
            public string selectedId;
        }

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
            UnifiedLoadingScreen.Hide(0.20f);
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
            creditsPop = Mathf.MoveTowards(creditsPop, 0f, Time.unscaledDeltaTime * 5.0f);
            profilePop = Mathf.MoveTowards(profilePop, 0f, Time.unscaledDeltaTime * 5.0f);
            statsPop = Mathf.MoveTowards(statsPop, 0f, Time.unscaledDeltaTime * 5.0f);
            achievementsPop = Mathf.MoveTowards(achievementsPop, 0f, Time.unscaledDeltaTime * 5.0f);

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
            AnimateCreditsButton();
            AnimateCreditsPanel();
            AnimateProfileButton();
            AnimateProfilePanel();
            AnimateStatsButton();
            AnimateStatsPanel();
            AnimateAchievementsButton();
            AnimateAchievementsPanel();

            if (menuGroup == null || menuGroup.alpha < 0.95f) return;

            if (achievementsPanelOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    CloseAchievementsPanel();
                return;
            }

            if (statsPanelOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    CloseStatsPanel();
                return;
            }

            if (profilePanelOpen)
            {
                if (profileInputOpen)
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                        CloseProfileInput();
                    else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        ConfirmCreateProfile();
                    return;
                }

                if (profileDeleteConfirmOpen)
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                        CloseProfileDeleteConfirm();
                    else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        ConfirmDeleteProfile();
                    return;
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                    CloseProfilePanel();
                return;
            }

            if (creditsPanelOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    CloseCreditsPanel();
                return;
            }

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
            UnifiedLoadingScreen.Hide(0.20f);
            IsMainMenuVisible = true;
            rootGroup.alpha = 1f;
            splashGroup.alpha = 0f;
            splashGroup.interactable = true;
            splashGroup.blocksRaycasts = true;
            menuGroup.alpha = 0f;
            menuGroup.interactable = false;
            menuGroup.blocksRaycasts = false;

            yield return Fade(splashGroup, 0f, 1f, 0.55f);

            // Avance 79: pantalla inicial tipo press any key.
            // No se salta automaticamente al menu; espera teclado, click, mando o toque.
            float minVisible = Time.unscaledTime + 0.30f;
            while (Time.unscaledTime < minVisible || !HasAnyStartInput())
            {
                if (splashPressText != null)
                {
                    float a = 0.45f + (Mathf.Sin(Time.unscaledTime * 4.2f) + 1f) * 0.275f;
                    splashPressText.color = new Color(0.0f, 0.94f, 1f, a);
                }
                yield return null;
            }

            yield return Fade(splashGroup, 1f, 0f, 0.35f);
            splashGroup.interactable = false;
            splashGroup.blocksRaycasts = false;
            yield return Fade(menuGroup, 0f, 1f, 0.55f);

            menuGroup.interactable = true;
            menuGroup.blocksRaycasts = true;
        }

        private bool HasAnyStartInput()
        {
            if (Input.anyKeyDown) return true;
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) return true;
            if (Input.touchCount > 0) return true;
            return false;
        }

        private IEnumerator OpenTutorialRoutine()
        {
            IsMainMenuVisible = false;
            menuGroup.interactable = false;
            menuGroup.blocksRaycasts = false;
            StopMenuMusic();
            Cursor.visible = true;

            // Avance 84: todas las transiciones a gameplay usan el mismo cargando.
            // Fondo negro completo para no mostrar la escena detras.
            UnifiedLoadingScreen.Show("CARGANDO...", true);
            yield return Fade(rootGroup, 1f, 0f, 0.20f);
            yield return new WaitForSecondsRealtime(0.20f);

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

            // Avance 84: al entrar al selector se muestra una pantalla de carga
            // unificada, negra y opaca, evitando que se vea el gameplay detras.
            UnifiedLoadingScreen.Show("CARGANDO...", true);
            yield return Fade(rootGroup, 1f, 0f, 0.20f);
            yield return new WaitForSecondsRealtime(0.28f);

            // Avance 48: Arcade debe abrir siempre en un estado estable.
            // Si el jugador venia desde Tutorial, se selecciona el primer nivel arcade
            // para evitar que el selector aparezca mezclado con el Tutorial.
            if (LevelManager.Instance != null)
                LevelManager.Instance.SelectFirstArcadeLevel();

            if (pauseMenu != null)
                pauseMenu.OpenLevelSelectFromStartup();
            else if (gameController != null)
                gameController.enabled = true;

            yield return new WaitForSecondsRealtime(0.15f);
            UnifiedLoadingScreen.Hide(0.20f);
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

            splashPressText = CreateText("SplashPressAny", splashGroup.transform, "PRESIONA CUALQUIER TECLA / BOTON / TOCA LA PANTALLA", 16, NeonCyan, FontStyles.Bold);
            splashPressText.alignment = TextAlignmentOptions.Center;
            splashPressText.characterSpacing = 4f;
            splashPressText.rectTransform.anchoredPosition = new Vector2(0f, -260f);
            splashPressText.rectTransform.sizeDelta = new Vector2(980f, 42f);

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
            CreateMainMenuCreditsButton(menuGroup.transform);
            CreateMainMenuProfileButton(menuGroup.transform);
            CreateMainMenuStatsButton(menuGroup.transform);
            CreateMainMenuAchievementsButton(menuGroup.transform);
            BuildMainMenuSettingsPanel();
            BuildHelpPanel(menuGroup.transform);
            BuildCreditsPanel(menuGroup.transform);
            BuildProfilePanel(menuGroup.transform);
            BuildStatsPanel(menuGroup.transform);
            BuildAchievementsPanel(menuGroup.transform);
            EasterEggSecretCodeController.Ensure();

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

        private void CreateMainMenuCreditsButton(Transform parent)
        {
            GameObject go = new GameObject("CreditsButton");
            go.transform.SetParent(parent, false);
            creditsButton = go.AddComponent<RectTransform>();
            creditsButton.sizeDelta = new Vector2(108f, 108f);
            creditsButton.anchoredPosition = new Vector2(-455f, -142f);

            creditsButtonBg = go.AddComponent<Image>();
            creditsButtonBg.sprite = MakeSprite(Color.white);
            creditsButtonBg.type = Image.Type.Sliced;
            creditsButtonBg.color = new Color(0.115f, 0.030f, 0.195f, 0.92f);

            creditsButtonGlow = CreateImage("CreditsButtonGlow", go.transform, new Color(1f, 0.22f, 0.72f, 0.18f));
            creditsButtonGlow.sprite = MakeRadialGlowSprite(96);
            creditsButtonGlow.rectTransform.sizeDelta = new Vector2(122f, 122f);
            creditsButtonGlow.rectTransform.anchoredPosition = Vector2.zero;
            creditsButtonGlow.raycastTarget = false;

            GameObject iconGO = new GameObject("CreditsBadgeIcon");
            iconGO.transform.SetParent(go.transform, false);
            RectTransform iconRt = iconGO.AddComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(70f, 70f);
            iconRt.anchoredPosition = new Vector2(0f, 8f);
            creditsButtonIconImage = iconGO.AddComponent<Image>();
            creditsButtonIconImage.sprite = MakeCreditsBadgeSprite(96);
            creditsButtonIconImage.color = NeonPink;
            creditsButtonIconImage.raycastTarget = false;

            creditsButtonLabel = CreateText("CreditsLabel", go.transform, "CREDITOS", 11, NeonCyan, FontStyles.Bold);
            creditsButtonLabel.alignment = TextAlignmentOptions.Center;
            creditsButtonLabel.characterSpacing = 2f;
            creditsButtonLabel.rectTransform.sizeDelta = new Vector2(108f, 24f);
            creditsButtonLabel.rectTransform.anchoredPosition = new Vector2(0f, -38f);

            EventTrigger trigger = go.AddComponent<EventTrigger>();
            trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();

            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((_) =>
            {
                creditsButtonHover = true;
                if (creditsButtonBg != null) creditsButtonBg.color = new Color(0.24f, 0.06f, 0.34f, 0.98f);
                if (creditsButtonIconImage != null) creditsButtonIconImage.color = NeonCyan;
            });
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((_) =>
            {
                creditsButtonHover = false;
                if (creditsButtonBg != null) creditsButtonBg.color = new Color(0.115f, 0.030f, 0.195f, 0.92f);
                if (creditsButtonIconImage != null) creditsButtonIconImage.color = NeonPink;
            });
            trigger.triggers.Add(exit);

            AddSimpleClick(trigger, OpenCreditsPanel);
        }

        private void BuildCreditsPanel(Transform parent)
        {
            GameObject root = new GameObject("CreditsPanel");
            root.transform.SetParent(parent, false);
            RectTransform rt = root.AddComponent<RectTransform>();
            Stretch(rt);

            creditsPanelGroup = root.AddComponent<CanvasGroup>();
            creditsPanelGroup.alpha = 0f;
            creditsPanelGroup.interactable = false;
            creditsPanelGroup.blocksRaycasts = false;

            Image dim = CreateImage("CreditsDim", root.transform, new Color(0.010f, 0.000f, 0.040f, 0.72f));
            Stretch(dim.rectTransform);
            dim.raycastTarget = true;

            Image glow = CreateImage("CreditsGlow", root.transform, new Color(1f, 0.20f, 0.75f, 0.18f));
            glow.sprite = MakeRadialGlowSprite(160);
            glow.rectTransform.sizeDelta = new Vector2(760f, 560f);
            glow.rectTransform.anchoredPosition = Vector2.zero;
            glow.raycastTarget = false;

            Image box = CreateImage("CreditsPanelBox", root.transform, new Color(0.095f, 0.030f, 0.160f, 0.98f));
            box.sprite = MakeSprite(Color.white);
            box.type = Image.Type.Sliced;
            box.rectTransform.sizeDelta = new Vector2(640f, 560f);
            box.rectTransform.anchoredPosition = Vector2.zero;
            box.raycastTarget = true;
            creditsPanelBox = box.rectTransform;

            CreateLine("CreditsTopLine", box.transform, new Vector2(0f, 252f), new Vector2(500f, 4f), NeonCyan).raycastTarget = false;
            CreateLine("CreditsBottomLine", box.transform, new Vector2(0f, -252f), new Vector2(500f, 4f), NeonPink).raycastTarget = false;

            creditsPanelTitle = CreateText("CreditsTitle", box.transform, "CREDITOS", 42, NeonYellow, FontStyles.Bold);
            creditsPanelTitle.alignment = TextAlignmentOptions.Center;
            creditsPanelTitle.characterSpacing = 5f;
            creditsPanelTitle.rectTransform.sizeDelta = new Vector2(560f, 62f);
            creditsPanelTitle.rectTransform.anchoredPosition = new Vector2(0f, 214f);

            Image bodyBox = CreateImage("CreditsBodyBox", box.transform, new Color(0.050f, 0.012f, 0.105f, 0.72f));
            bodyBox.sprite = MakeSprite(Color.white);
            bodyBox.type = Image.Type.Sliced;
            bodyBox.rectTransform.sizeDelta = new Vector2(560f, 400f);
            bodyBox.rectTransform.anchoredPosition = new Vector2(0f, -8f);
            bodyBox.raycastTarget = false;

            string body =
                "<color=#00F0FF><b>PROJECT BEAT v3.0+</b></color>\n\n" +
                "<color=#FFE600><b>Desarrolladores</b></color>\n" +
                "Denzel Alvarez\nAlonso Leiva\n\n" +
                "<color=#FFE600><b>Asignatura</b></color>\n" +
                "Programacion de Videojuegos\n\n" +
                "<color=#FFE600><b>Institucion</b></color>\n" +
                "Santo Tomas Iquique\n\n" +
                "<color=#FFE600><b>Tecnologias</b></color>\n" +
                "Unity  |  C#  |  TextMeshPro  |  Unity UI\n\n" +
                "<color=#FFE600><b>Inspiraciones</b></color>\n" +
                "osu!  |  Fortnite Festival  |  Geometry Dash  |  Guitar Hero\n\n" +
                "<color=#00F0FF><i>\"Feel the rhythm.\"</i></color>";

            creditsPanelBody = CreateText("CreditsBody", bodyBox.transform, body, 18, TextNormal, FontStyles.Bold);
            creditsPanelBody.alignment = TextAlignmentOptions.Center;
            creditsPanelBody.enableWordWrapping = true;
            creditsPanelBody.richText = true;
            creditsPanelBody.rectTransform.sizeDelta = new Vector2(520f, 372f);
            creditsPanelBody.rectTransform.anchoredPosition = Vector2.zero;

            TMP_Text unusedCloseText;
            creditsCloseBg = CreateHelpButton(box.transform, "CreditsClose", "X", new Vector2(286f, 230f), false, out unusedCloseText);
            EventTrigger closeTrigger = creditsCloseBg.gameObject.GetComponent<EventTrigger>();
            if (closeTrigger == null) closeTrigger = creditsCloseBg.gameObject.AddComponent<EventTrigger>();
            AddSimpleClick(closeTrigger, CloseCreditsPanel);
        }

        private void OpenCreditsPanel()
        {
            if (creditsPanelGroup == null) return;
            creditsPanelOpen = true;
            creditsPop = 1f;
            creditsPanelGroup.alpha = 1f;
            creditsPanelGroup.interactable = true;
            creditsPanelGroup.blocksRaycasts = true;
        }

        private void CloseCreditsPanel()
        {
            creditsPanelOpen = false;
            if (creditsPanelGroup == null) return;
            creditsPanelGroup.alpha = 0f;
            creditsPanelGroup.interactable = false;
            creditsPanelGroup.blocksRaycasts = false;
        }

        private void AnimateCreditsButton()
        {
            if (creditsButton == null || menuGroup == null || !menuGroup.interactable) return;
            float targetScale = creditsButtonHover ? 1.07f : 1f + Mathf.Sin(pulse * 1.10f + 0.8f) * 0.015f;
            creditsButton.localScale = Vector3.Lerp(creditsButton.localScale, Vector3.one * targetScale, Time.unscaledDeltaTime * 8f);

            if (creditsButtonGlow != null)
            {
                float alpha = creditsButtonHover ? 0.34f : 0.15f + Mathf.Sin(pulse * 1.2f + 0.7f) * 0.05f;
                creditsButtonGlow.color = new Color(1f, 0.22f, 0.72f, alpha);
            }

            if (creditsButtonIconImage != null)
            {
                Color animated = Color.Lerp(NeonPink, NeonCyan, (Mathf.Sin(pulse * 0.8f) + 1f) * 0.5f);
                creditsButtonIconImage.color = Color.Lerp(creditsButtonIconImage.color, creditsButtonHover ? NeonCyan : animated, Time.unscaledDeltaTime * 10f);
            }

            if (creditsButtonLabel != null)
                creditsButtonLabel.color = creditsButtonHover ? Color.white : NeonCyan;
        }

        private void AnimateCreditsPanel()
        {
            if (!creditsPanelOpen || creditsPanelBox == null) return;
            float scale = 1f + creditsPop * 0.045f;
            creditsPanelBox.localScale = Vector3.Lerp(creditsPanelBox.localScale, Vector3.one * scale, Time.unscaledDeltaTime * 10f);
            if (creditsPop < 0.02f)
                creditsPanelBox.localScale = Vector3.Lerp(creditsPanelBox.localScale, Vector3.one, Time.unscaledDeltaTime * 8f);
        }


        private void CreateMainMenuProfileButton(Transform parent)
        {
            GameObject go = new GameObject("ProfileButton");
            go.transform.SetParent(parent, false);
            profileButton = go.AddComponent<RectTransform>();
            profileButton.sizeDelta = new Vector2(108f, 108f);
            profileButton.anchoredPosition = new Vector2(455f, 142f);

            profileButtonBg = go.AddComponent<Image>();
            profileButtonBg.sprite = MakeSprite(Color.white);
            profileButtonBg.type = Image.Type.Sliced;
            profileButtonBg.color = new Color(0.115f, 0.030f, 0.195f, 0.92f);

            profileButtonGlow = CreateImage("ProfileButtonGlow", go.transform, new Color(0f, 0.94f, 1f, 0.18f));
            profileButtonGlow.sprite = MakeRadialGlowSprite(96);
            profileButtonGlow.rectTransform.sizeDelta = new Vector2(122f, 122f);
            profileButtonGlow.rectTransform.anchoredPosition = Vector2.zero;
            profileButtonGlow.raycastTarget = false;

            GameObject iconGO = new GameObject("ProfileUserIcon");
            iconGO.transform.SetParent(go.transform, false);
            RectTransform iconRt = iconGO.AddComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(70f, 70f);
            iconRt.anchoredPosition = new Vector2(0f, 8f);
            profileButtonIconImage = iconGO.AddComponent<Image>();
            profileButtonIconImage.sprite = MakeUserProfileSprite(96);
            profileButtonIconImage.color = NeonCyan;
            profileButtonIconImage.raycastTarget = false;

            profileButtonLabel = CreateText("ProfileLabel", go.transform, "PERFIL", 12, NeonPink, FontStyles.Bold);
            profileButtonLabel.alignment = TextAlignmentOptions.Center;
            profileButtonLabel.characterSpacing = 3f;
            profileButtonLabel.rectTransform.sizeDelta = new Vector2(108f, 24f);
            profileButtonLabel.rectTransform.anchoredPosition = new Vector2(0f, -38f);

            EventTrigger trigger = go.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();

            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((_) =>
            {
                profileButtonHover = true;
                if (profileButtonBg != null) profileButtonBg.color = new Color(0.24f, 0.06f, 0.34f, 0.98f);
                if (profileButtonIconImage != null) profileButtonIconImage.color = Color.white;
            });
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((_) =>
            {
                profileButtonHover = false;
                if (profileButtonBg != null) profileButtonBg.color = new Color(0.115f, 0.030f, 0.195f, 0.92f);
                if (profileButtonIconImage != null) profileButtonIconImage.color = NeonCyan;
            });
            trigger.triggers.Add(exit);

            AddSimpleClick(trigger, OpenProfilePanel);
        }

        private void BuildProfilePanel(Transform parent)
        {
            LoadProfiles();

            GameObject root = new GameObject("ProfilePanel");
            root.transform.SetParent(parent, false);
            RectTransform rootRt = root.AddComponent<RectTransform>();
            Stretch(rootRt);

            profilePanelGroup = root.AddComponent<CanvasGroup>();
            profilePanelGroup.alpha = 0f;
            profilePanelGroup.interactable = false;
            profilePanelGroup.blocksRaycasts = false;

            Image dim = CreateImage("ProfileDim", root.transform, new Color(0.010f, 0.000f, 0.040f, 0.72f));
            Stretch(dim.rectTransform);
            dim.raycastTarget = true;

            Image glow = CreateImage("ProfileGlow", root.transform, new Color(0f, 0.94f, 1f, 0.15f));
            glow.sprite = MakeRadialGlowSprite(160);
            glow.rectTransform.sizeDelta = new Vector2(880f, 570f);
            glow.rectTransform.anchoredPosition = Vector2.zero;
            glow.raycastTarget = false;

            Image box = CreateImage("ProfilePanelBox", root.transform, new Color(0.095f, 0.030f, 0.160f, 0.98f));
            box.sprite = MakeSprite(Color.white);
            box.type = Image.Type.Sliced;
            box.rectTransform.sizeDelta = new Vector2(850f, 545f);
            box.rectTransform.anchoredPosition = Vector2.zero;
            box.raycastTarget = true;
            profilePanelBox = box.rectTransform;

            CreateLine("ProfileTopLine", box.transform, new Vector2(0f, 248f), new Vector2(700f, 4f), NeonCyan).raycastTarget = false;
            CreateLine("ProfileBottomLine", box.transform, new Vector2(0f, -248f), new Vector2(700f, 4f), NeonPink).raycastTarget = false;

            TMP_Text title = CreateText("ProfileTitle", box.transform, "PERFIL", 44, Color.white, FontStyles.Bold);
            title.alignment = TextAlignmentOptions.Center;
            title.characterSpacing = 5f;
            title.rectTransform.sizeDelta = new Vector2(720f, 58f);
            title.rectTransform.anchoredPosition = new Vector2(0f, 206f);

            TMP_Text help = CreateText("ProfileHelp", box.transform, "Crea hasta 5 perfiles locales. Mas adelante el progreso y las estadisticas se guardaran por usuario.", 15, TextDim, FontStyles.Bold);
            help.alignment = TextAlignmentOptions.Center;
            help.enableWordWrapping = true;
            help.rectTransform.sizeDelta = new Vector2(690f, 40f);
            help.rectTransform.anchoredPosition = new Vector2(0f, 160f);

            Image listBox = CreateImage("ProfileListBox", box.transform, new Color(0.040f, 0.010f, 0.075f, 0.82f));
            listBox.sprite = MakeSprite(Color.white);
            listBox.type = Image.Type.Sliced;
            listBox.rectTransform.sizeDelta = new Vector2(380f, 275f);
            listBox.rectTransform.anchoredPosition = new Vector2(-185f, 10f);
            listBox.raycastTarget = false;

            profileRowBgs = new Image[MaxProfiles];
            profileRowLabels = new TMP_Text[MaxProfiles];
            for (int i = 0; i < MaxProfiles; i++)
            {
                int rowIndex = i;
                Image row = CreateProfileListRow(listBox.transform, rowIndex, new Vector2(0f, 100f - i * 50f));
                profileRowBgs[i] = row;
            }

            profileSelectedText = CreateText("ProfileSelectedText", box.transform, "SIN PERFIL SELECCIONADO", 18, NeonCyan, FontStyles.Bold);
            profileSelectedText.alignment = TextAlignmentOptions.Center;
            profileSelectedText.enableWordWrapping = true;
            profileSelectedText.rectTransform.sizeDelta = new Vector2(330f, 48f);
            profileSelectedText.rectTransform.anchoredPosition = new Vector2(220f, 110f);

            profileStatusText = CreateText("ProfileStatus", box.transform, "Selecciona o crea un perfil para comenzar.", 16, TextDim, FontStyles.Bold);
            profileStatusText.alignment = TextAlignmentOptions.Center;
            profileStatusText.enableWordWrapping = true;
            profileStatusText.rectTransform.sizeDelta = new Vector2(330f, 60f);
            profileStatusText.rectTransform.anchoredPosition = new Vector2(220f, 54f);

            TMP_Text tmp;
            profileCreateBg = CreateProfileActionButton(box.transform, "ProfileCreate", "CREAR PERFIL", new Vector2(220f, -15f), out tmp, OpenProfileInput);
            profileNewGameBg = CreateProfileActionButton(box.transform, "ProfileNewGame", "NUEVA PARTIDA", new Vector2(220f, -75f), out tmp, CreateProfileNewGame);
            profileLoadGameBg = CreateProfileActionButton(box.transform, "ProfileLoadGame", "CARGAR PARTIDA", new Vector2(220f, -135f), out tmp, LoadProfileGame);
            profileDeleteBg = CreateProfileActionButton(box.transform, "ProfileDelete", "ELIMINAR", new Vector2(220f, -195f), out tmp, OpenProfileDeleteConfirm);
            profileCloseBg = CreateProfileActionButton(box.transform, "ProfileClose", "X", new Vector2(376f, 208f), out tmp, CloseProfilePanel);

            BuildProfileInputDialog(box.transform);
            BuildProfileDeleteConfirm(box.transform);
            RefreshProfilePanel();
        }

        private Image CreateProfileListRow(Transform parent, int index, Vector2 pos)
        {
            GameObject go = new GameObject("ProfileRow_" + index);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(330f, 42f);
            rt.anchoredPosition = pos;

            Image bg = go.AddComponent<Image>();
            bg.sprite = MakeSprite(Color.white);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.100f, 0.026f, 0.160f, 0.86f);

            TMP_Text label = CreateText("ProfileRowLabel", go.transform, "VACIO", 17, TextDim, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.sizeDelta = rt.sizeDelta;
            label.rectTransform.anchoredPosition = Vector2.zero;
            profileRowLabels[index] = label;

            EventTrigger trigger = go.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();
            AddSimpleClick(trigger, () => SelectProfile(index));
            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((_) => { if (index < profiles.Count && selectedProfileIndex != index) bg.color = new Color(0.18f, 0.05f, 0.30f, 0.96f); });
            trigger.triggers.Add(enter);
            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((_) => RefreshProfilePanel());
            trigger.triggers.Add(exit);
            return bg;
        }

        private Image CreateProfileActionButton(Transform parent, string name, string label, Vector2 pos, out TMP_Text labelText, System.Action action)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = label == "X" ? new Vector2(56f, 46f) : new Vector2(250f, 46f);
            rt.anchoredPosition = pos;

            Image bg = go.AddComponent<Image>();
            bg.sprite = MakeSprite(Color.white);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.140f, 0.040f, 0.225f, 0.96f);

            labelText = CreateText("Label", go.transform, label, label == "X" ? 28 : 17, Color.white, FontStyles.Bold);
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.rectTransform.sizeDelta = rt.sizeDelta;
            labelText.rectTransform.anchoredPosition = Vector2.zero;
            labelText.raycastTarget = false;

            EventTrigger trigger = go.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();
            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((_) => { if (bg != null) bg.color = new Color(0.30f, 0.08f, 0.42f, 1f); });
            trigger.triggers.Add(enter);
            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((_) => { if (bg != null) bg.color = new Color(0.140f, 0.040f, 0.225f, 0.96f); RefreshProfilePanel(); });
            trigger.triggers.Add(exit);
            AddSimpleClick(trigger, action);
            return bg;
        }

        private void BuildProfileInputDialog(Transform parent)
        {
            GameObject root = new GameObject("ProfileInputDialog");
            root.transform.SetParent(parent, false);
            RectTransform rt = root.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500f, 230f);
            rt.anchoredPosition = Vector2.zero;
            profileInputGroup = root.AddComponent<CanvasGroup>();
            profileInputGroup.alpha = 0f;
            profileInputGroup.interactable = false;
            profileInputGroup.blocksRaycasts = false;

            Image bg = root.AddComponent<Image>();
            bg.sprite = MakeSprite(Color.white);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.045f, 0.010f, 0.090f, 0.98f);

            TMP_Text title = CreateText("InputTitle", root.transform, "NUEVO PERFIL", 26, NeonCyan, FontStyles.Bold);
            title.alignment = TextAlignmentOptions.Center;
            title.rectTransform.sizeDelta = new Vector2(440f, 40f);
            title.rectTransform.anchoredPosition = new Vector2(0f, 82f);

            Image inputBg = CreateImage("NameInput", root.transform, new Color(0.120f, 0.035f, 0.190f, 1f));
            inputBg.sprite = MakeSprite(Color.white);
            inputBg.type = Image.Type.Sliced;
            inputBg.rectTransform.sizeDelta = new Vector2(380f, 46f);
            inputBg.rectTransform.anchoredPosition = new Vector2(0f, 26f);
            profileNameInput = inputBg.gameObject.AddComponent<TMP_InputField>();
            profileNameInput.characterLimit = MaxProfileNameLength;
            profileNameInput.targetGraphic = inputBg;

            TMP_Text text = CreateText("InputText", inputBg.transform, "", 20, Color.white, FontStyles.Bold);
            text.alignment = TextAlignmentOptions.Left;
            text.rectTransform.sizeDelta = new Vector2(340f, 34f);
            text.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            TMP_Text placeholder = CreateText("Placeholder", inputBg.transform, "Nombre del perfil", 18, TextDim, FontStyles.Bold);
            placeholder.alignment = TextAlignmentOptions.Left;
            placeholder.rectTransform.sizeDelta = new Vector2(340f, 34f);
            placeholder.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            profileNameInput.textComponent = text;
            profileNameInput.placeholder = placeholder;
            profileNameInput.textViewport = inputBg.rectTransform;

            TMP_Text tmp;
            CreateProfileActionButton(root.transform, "InputConfirm", "CONFIRMAR", new Vector2(-112f, -66f), out tmp, ConfirmCreateProfile);
            CreateProfileActionButton(root.transform, "InputCancel", "CANCELAR", new Vector2(112f, -66f), out tmp, CloseProfileInput);
        }

        private void BuildProfileDeleteConfirm(Transform parent)
        {
            GameObject root = new GameObject("ProfileDeleteConfirm");
            root.transform.SetParent(parent, false);
            RectTransform rt = root.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500f, 220f);
            rt.anchoredPosition = Vector2.zero;
            profileDeleteConfirmGroup = root.AddComponent<CanvasGroup>();
            profileDeleteConfirmGroup.alpha = 0f;
            profileDeleteConfirmGroup.interactable = false;
            profileDeleteConfirmGroup.blocksRaycasts = false;

            Image bg = root.AddComponent<Image>();
            bg.sprite = MakeSprite(Color.white);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.045f, 0.010f, 0.090f, 0.98f);

            TMP_Text title = CreateText("DeleteTitle", root.transform, "¿ELIMINAR PERFIL?", 25, NeonPink, FontStyles.Bold);
            title.alignment = TextAlignmentOptions.Center;
            title.rectTransform.sizeDelta = new Vector2(440f, 44f);
            title.rectTransform.anchoredPosition = new Vector2(0f, 60f);

            TMP_Text body = CreateText("DeleteBody", root.transform, "Esta accion quitara el perfil seleccionado.", 18, TextNormal, FontStyles.Bold);
            body.alignment = TextAlignmentOptions.Center;
            body.rectTransform.sizeDelta = new Vector2(440f, 40f);
            body.rectTransform.anchoredPosition = new Vector2(0f, 12f);

            TMP_Text tmp;
            CreateProfileActionButton(root.transform, "DeleteConfirm", "CONFIRMAR", new Vector2(-112f, -70f), out tmp, ConfirmDeleteProfile);
            CreateProfileActionButton(root.transform, "DeleteCancel", "CANCELAR", new Vector2(112f, -70f), out tmp, CloseProfileDeleteConfirm);
        }

        private void OpenProfilePanel()
        {
            LoadProfiles();
            RefreshProfilePanel();
            profilePanelOpen = true;
            profilePop = 1f;
            if (profilePanelGroup == null) return;
            profilePanelGroup.alpha = 1f;
            profilePanelGroup.interactable = true;
            profilePanelGroup.blocksRaycasts = true;
            SetProfileStatus("Selecciona o crea un perfil para continuar.");
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void CloseProfilePanel()
        {
            profilePanelOpen = false;
            CloseProfileInput();
            CloseProfileDeleteConfirm();
            if (profilePanelGroup == null) return;
            profilePanelGroup.alpha = 0f;
            profilePanelGroup.interactable = false;
            profilePanelGroup.blocksRaycasts = false;
        }

        private void OpenProfileInput()
        {
            if (profiles.Count >= MaxProfiles)
            {
                SetProfileStatus("LIMITE DE PERFILES ALCANZADO");
                return;
            }
            if (profileInputGroup == null) return;
            profileInputOpen = true;
            profileInputGroup.alpha = 1f;
            profileInputGroup.interactable = true;
            profileInputGroup.blocksRaycasts = true;
            if (profileNameInput != null)
            {
                profileNameInput.text = string.Empty;
                profileNameInput.Select();
                profileNameInput.ActivateInputField();
            }
        }

        private void CloseProfileInput()
        {
            profileInputOpen = false;
            if (profileInputGroup == null) return;
            profileInputGroup.alpha = 0f;
            profileInputGroup.interactable = false;
            profileInputGroup.blocksRaycasts = false;
        }

        private void OpenProfileDeleteConfirm()
        {
            if (!HasSelectedProfile())
            {
                SetProfileStatus("Selecciona un perfil para eliminar.");
                return;
            }
            if (profileDeleteConfirmGroup == null) return;
            profileDeleteConfirmOpen = true;
            profileDeleteConfirmGroup.alpha = 1f;
            profileDeleteConfirmGroup.interactable = true;
            profileDeleteConfirmGroup.blocksRaycasts = true;
        }

        private void CloseProfileDeleteConfirm()
        {
            profileDeleteConfirmOpen = false;
            if (profileDeleteConfirmGroup == null) return;
            profileDeleteConfirmGroup.alpha = 0f;
            profileDeleteConfirmGroup.interactable = false;
            profileDeleteConfirmGroup.blocksRaycasts = false;
        }

        private void ConfirmCreateProfile()
        {
            if (profileNameInput == null) return;
            string name = (profileNameInput.text ?? string.Empty).Trim();
            if (name.Length == 0)
            {
                SetProfileStatus("El nombre no puede estar vacio.");
                return;
            }
            if (name.Length > MaxProfileNameLength)
                name = name.Substring(0, MaxProfileNameLength);
            if (profiles.Count >= MaxProfiles)
            {
                SetProfileStatus("LIMITE DE PERFILES ALCANZADO");
                return;
            }

            ProfileEntry entry = new ProfileEntry
            {
                id = Guid.NewGuid().ToString("N"),
                name = name,
                createdAt = DateTime.UtcNow.ToString("o"),
                partidaCreada = false,
                version = 1
            };
            profiles.Add(entry);
            selectedProfileIndex = profiles.Count - 1;
            SaveProfiles();
            CloseProfileInput();
            RefreshProfilePanel();
            SetProfileStatus("PERFIL CREADO: " + name);
        }

        private void ConfirmDeleteProfile()
        {
            if (!HasSelectedProfile())
            {
                CloseProfileDeleteConfirm();
                return;
            }
            string removedName = profiles[selectedProfileIndex].name;
            string removedId = profiles[selectedProfileIndex].id;
            profiles.RemoveAt(selectedProfileIndex);
            ProfileStatsStorage.DeleteStatsForProfile(removedId);
            ProfileAchievementStorage.DeleteAchievementsForProfile(removedId);
            selectedProfileIndex = profiles.Count > 0 ? Mathf.Clamp(selectedProfileIndex, 0, profiles.Count - 1) : -1;
            SaveProfiles();
            CloseProfileDeleteConfirm();
            RefreshProfilePanel();
            SetProfileStatus("PERFIL ELIMINADO: " + removedName);
        }

        private void SelectProfile(int index)
        {
            if (index < 0 || index >= profiles.Count)
            {
                SetProfileStatus("Ese espacio de perfil esta vacio.");
                return;
            }
            selectedProfileIndex = index;
            SaveProfiles();
            RefreshProfilePanel();
            SetProfileStatus("PERFIL SELECCIONADO: " + profiles[index].name);
        }

        private void CreateProfileNewGame()
        {
            if (!HasSelectedProfile())
            {
                SetProfileStatus("Selecciona un perfil para crear partida.");
                return;
            }
            profiles[selectedProfileIndex].partidaCreada = true;
            SaveProfiles();
            int firstArcade = LevelManager.Instance != null ? LevelManager.Instance.GetFirstArcadeLevelIndex() : 1;
            ProfileStatsStorage.ResetProgressForCurrentProfile(firstArcade);
            if (ProfileAchievementStorage.TryUnlockFirstGame(out var newGameAchievement))
                AchievementNotification.Show(newGameAchievement);
            RefreshProfilePanel();
            SetProfileStatus("PARTIDA CREADA PARA " + profiles[selectedProfileIndex].name);
        }

        private void LoadProfileGame()
        {
            if (!HasSelectedProfile())
            {
                SetProfileStatus("Selecciona un perfil para cargar partida.");
                return;
            }
            if (!profiles[selectedProfileIndex].partidaCreada)
            {
                SetProfileStatus("Este perfil aun no tiene partida. Usa NUEVA PARTIDA.");
                return;
            }
            SaveProfiles();
            int firstArcade = LevelManager.Instance != null ? LevelManager.Instance.GetFirstArcadeLevelIndex() : 1;
            ProfileStatsStorage.GetUnlockedLevelIndex(firstArcade);
            SetProfileStatus("PARTIDA CARGADA: " + profiles[selectedProfileIndex].name);
        }

        private bool HasSelectedProfile()
        {
            return selectedProfileIndex >= 0 && selectedProfileIndex < profiles.Count;
        }

        private void SetProfileStatus(string message)
        {
            if (profileStatusText != null)
                profileStatusText.text = message;
        }

        private void RefreshProfilePanel()
        {
            if (profileRowBgs != null && profileRowLabels != null)
            {
                for (int i = 0; i < MaxProfiles; i++)
                {
                    bool exists = i < profiles.Count;
                    bool selected = i == selectedProfileIndex && exists;
                    if (profileRowLabels[i] != null)
                    {
                        profileRowLabels[i].text = exists
                            ? (i + 1).ToString() + ". " + profiles[i].name + (profiles[i].partidaCreada ? "  • PARTIDA" : "")
                            : (i + 1).ToString() + ". VACIO";
                        profileRowLabels[i].color = exists ? (selected ? Color.white : TextNormal) : TextDim;
                    }
                    if (profileRowBgs[i] != null)
                    {
                        profileRowBgs[i].color = selected
                            ? new Color(0.85f, 0.14f, 0.86f, 0.96f)
                            : (exists ? new Color(0.100f, 0.026f, 0.160f, 0.86f) : new Color(0.060f, 0.014f, 0.100f, 0.60f));
                    }
                }
            }

            if (profileSelectedText != null)
            {
                profileSelectedText.text = HasSelectedProfile()
                    ? "ACTUAL: " + profiles[selectedProfileIndex].name
                    : "SIN PERFIL SELECCIONADO";
            }

            if (profileCreateBg != null)
                profileCreateBg.color = profiles.Count >= MaxProfiles ? new Color(0.08f, 0.06f, 0.09f, 0.78f) : new Color(0.140f, 0.040f, 0.225f, 0.96f);
        }

        private void LoadProfiles()
        {
            profiles.Clear();
            selectedProfileIndex = -1;
            string json = PlayerPrefs.GetString(ProfilesPrefsKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                ProfileSaveData data = JsonUtility.FromJson<ProfileSaveData>(json);
                if (data != null && data.profiles != null)
                {
                    for (int i = 0; i < data.profiles.Count && profiles.Count < MaxProfiles; i++)
                    {
                        ProfileEntry p = data.profiles[i];
                        if (p != null && !string.IsNullOrEmpty(p.id) && !string.IsNullOrEmpty(p.name))
                            profiles.Add(p);
                    }
                    if (!string.IsNullOrEmpty(data.selectedId))
                    {
                        for (int i = 0; i < profiles.Count; i++)
                        {
                            if (profiles[i].id == data.selectedId)
                            {
                                selectedProfileIndex = i;
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                profiles.Clear();
                selectedProfileIndex = -1;
            }
        }

        private void SaveProfiles()
        {
            ProfileSaveData data = new ProfileSaveData();
            data.profiles = profiles;
            data.selectedId = HasSelectedProfile() ? profiles[selectedProfileIndex].id : string.Empty;
            PlayerPrefs.SetString(ProfilesPrefsKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        private void AnimateProfileButton()
        {
            if (profileButton == null || menuGroup == null || !menuGroup.interactable) return;
            float targetScale = profileButtonHover ? 1.07f : 1f + Mathf.Sin(pulse * 1.10f + 1.6f) * 0.015f;
            profileButton.localScale = Vector3.Lerp(profileButton.localScale, Vector3.one * targetScale, Time.unscaledDeltaTime * 8f);
            if (profileButtonGlow != null)
            {
                float alpha = profileButtonHover ? 0.34f : 0.15f + Mathf.Sin(pulse * 1.2f + 1.3f) * 0.05f;
                profileButtonGlow.color = new Color(0f, 0.94f, 1f, alpha);
            }
            if (profileButtonIconImage != null)
            {
                Color animated = Color.Lerp(NeonCyan, NeonPink, (Mathf.Sin(pulse * 0.8f + 1.2f) + 1f) * 0.5f);
                profileButtonIconImage.color = Color.Lerp(profileButtonIconImage.color, profileButtonHover ? Color.white : animated, Time.unscaledDeltaTime * 10f);
            }
            if (profileButtonLabel != null)
                profileButtonLabel.color = profileButtonHover ? Color.white : NeonPink;
        }

        private void AnimateProfilePanel()
        {
            if (!profilePanelOpen || profilePanelBox == null) return;
            float scale = 1f + profilePop * 0.045f;
            profilePanelBox.localScale = Vector3.Lerp(profilePanelBox.localScale, Vector3.one * scale, Time.unscaledDeltaTime * 10f);
            if (profilePop < 0.02f)
                profilePanelBox.localScale = Vector3.Lerp(profilePanelBox.localScale, Vector3.one, Time.unscaledDeltaTime * 8f);
        }

        private void CreateMainMenuStatsButton(Transform parent)
        {
            GameObject go = new GameObject("StatsButton");
            go.transform.SetParent(parent, false);
            statsButton = go.AddComponent<RectTransform>();
            statsButton.sizeDelta = new Vector2(108f, 108f);
            statsButton.anchoredPosition = new Vector2(455f, 0f);

            statsButtonBg = go.AddComponent<Image>();
            statsButtonBg.sprite = MakeSprite(Color.white);
            statsButtonBg.type = Image.Type.Sliced;
            statsButtonBg.color = new Color(0.115f, 0.030f, 0.195f, 0.92f);

            statsButtonGlow = CreateImage("StatsButtonGlow", go.transform, new Color(1f, 0.22f, 0.72f, 0.18f));
            statsButtonGlow.sprite = MakeRadialGlowSprite(96);
            statsButtonGlow.rectTransform.sizeDelta = new Vector2(122f, 122f);
            statsButtonGlow.rectTransform.anchoredPosition = Vector2.zero;
            statsButtonGlow.raycastTarget = false;

            GameObject iconGO = new GameObject("StatsBarsIcon");
            iconGO.transform.SetParent(go.transform, false);
            RectTransform iconRt = iconGO.AddComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(70f, 70f);
            iconRt.anchoredPosition = new Vector2(0f, 8f);
            statsButtonIconImage = iconGO.AddComponent<Image>();
            statsButtonIconImage.sprite = MakeStatsBarsSprite(96);
            statsButtonIconImage.color = NeonCyan;
            statsButtonIconImage.raycastTarget = false;

            statsButtonLabel = CreateText("StatsLabel", go.transform, "ESTADISTICAS", 8, NeonPink, FontStyles.Bold);
            statsButtonLabel.alignment = TextAlignmentOptions.Center;
            statsButtonLabel.characterSpacing = 2f;
            statsButtonLabel.rectTransform.sizeDelta = new Vector2(108f, 24f);
            statsButtonLabel.rectTransform.anchoredPosition = new Vector2(0f, -38f);

            EventTrigger trigger = go.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();

            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((_) =>
            {
                statsButtonHover = true;
                if (statsButtonBg != null) statsButtonBg.color = new Color(0.24f, 0.06f, 0.34f, 0.98f);
                if (statsButtonIconImage != null) statsButtonIconImage.color = Color.white;
            });
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((_) =>
            {
                statsButtonHover = false;
                if (statsButtonBg != null) statsButtonBg.color = new Color(0.115f, 0.030f, 0.195f, 0.92f);
                if (statsButtonIconImage != null) statsButtonIconImage.color = NeonCyan;
            });
            trigger.triggers.Add(exit);

            AddSimpleClick(trigger, OpenStatsPanel);
        }

        private void BuildStatsPanel(Transform parent)
        {
            GameObject root = new GameObject("StatsPanel");
            root.transform.SetParent(parent, false);
            RectTransform rt = root.AddComponent<RectTransform>();
            Stretch(rt);

            statsPanelGroup = root.AddComponent<CanvasGroup>();
            statsPanelGroup.alpha = 0f;
            statsPanelGroup.interactable = false;
            statsPanelGroup.blocksRaycasts = false;

            Image dim = CreateImage("StatsDim", root.transform, new Color(0.010f, 0.000f, 0.040f, 0.72f));
            Stretch(dim.rectTransform);
            dim.raycastTarget = true;

            Image glow = CreateImage("StatsGlow", root.transform, new Color(0f, 0.94f, 1f, 0.16f));
            glow.sprite = MakeRadialGlowSprite(160);
            glow.rectTransform.sizeDelta = new Vector2(900f, 620f);
            glow.rectTransform.anchoredPosition = Vector2.zero;
            glow.raycastTarget = false;

            Image box = CreateImage("StatsPanelBox", root.transform, new Color(0.095f, 0.030f, 0.160f, 0.98f));
            box.sprite = MakeSprite(Color.white);
            box.type = Image.Type.Sliced;
            box.rectTransform.sizeDelta = new Vector2(820f, 600f);
            box.rectTransform.anchoredPosition = Vector2.zero;
            box.raycastTarget = true;
            statsPanelBox = box.rectTransform;

            CreateLine("StatsTopLine", box.transform, new Vector2(0f, 270f), new Vector2(650f, 4f), NeonCyan).raycastTarget = false;
            CreateLine("StatsBottomLine", box.transform, new Vector2(0f, -270f), new Vector2(650f, 4f), NeonPink).raycastTarget = false;

            statsPanelTitle = CreateText("StatsTitle", box.transform, "ESTADISTICAS", 38, Color.white, FontStyles.Bold);
            statsPanelTitle.alignment = TextAlignmentOptions.Center;
            statsPanelTitle.characterSpacing = 5f;
            statsPanelTitle.rectTransform.sizeDelta = new Vector2(700f, 56f);
            statsPanelTitle.rectTransform.anchoredPosition = new Vector2(0f, 232f);

            statsPanelProfileText = CreateText("StatsProfile", box.transform, "PERFIL: -", 18, NeonCyan, FontStyles.Bold);
            statsPanelProfileText.alignment = TextAlignmentOptions.Center;
            statsPanelProfileText.characterSpacing = 2f;
            statsPanelProfileText.rectTransform.sizeDelta = new Vector2(700f, 34f);
            statsPanelProfileText.rectTransform.anchoredPosition = new Vector2(0f, 190f);

            Image bodyBox = CreateImage("StatsBodyBox", box.transform, new Color(0.050f, 0.012f, 0.105f, 0.78f));
            bodyBox.sprite = MakeSprite(Color.white);
            bodyBox.type = Image.Type.Sliced;
            bodyBox.rectTransform.sizeDelta = new Vector2(720f, 395f);
            bodyBox.rectTransform.anchoredPosition = new Vector2(0f, -24f);
            bodyBox.raycastTarget = true;

            Mask bodyMask = bodyBox.gameObject.AddComponent<Mask>();
            bodyMask.showMaskGraphic = true;

            GameObject contentGO = new GameObject("StatsScrollContent", typeof(RectTransform));
            contentGO.transform.SetParent(bodyBox.transform, false);
            statsScrollContent = contentGO.GetComponent<RectTransform>();
            statsScrollContent.anchorMin = new Vector2(0f, 1f);
            statsScrollContent.anchorMax = new Vector2(1f, 1f);
            statsScrollContent.pivot = new Vector2(0.5f, 1f);
            statsScrollContent.anchoredPosition = new Vector2(-12f, -16f);
            statsScrollContent.sizeDelta = new Vector2(-54f, 360f);

            statsPanelBodyText = CreateText("StatsBody", statsScrollContent, "", 15, TextNormal, FontStyles.Bold);
            statsPanelBodyText.alignment = TextAlignmentOptions.TopLeft;
            statsPanelBodyText.enableWordWrapping = true;
            statsPanelBodyText.richText = true;
            statsPanelBodyText.rectTransform.anchorMin = new Vector2(0f, 1f);
            statsPanelBodyText.rectTransform.anchorMax = new Vector2(1f, 1f);
            statsPanelBodyText.rectTransform.pivot = new Vector2(0f, 1f);
            statsPanelBodyText.rectTransform.sizeDelta = new Vector2(0f, 360f);
            statsPanelBodyText.rectTransform.anchoredPosition = new Vector2(14f, 0f);

            Image scrollbarBg = CreateImage("StatsScrollbarBg", bodyBox.transform, new Color(0.18f, 0.06f, 0.28f, 0.42f));
            scrollbarBg.rectTransform.anchorMin = new Vector2(1f, 0f);
            scrollbarBg.rectTransform.anchorMax = new Vector2(1f, 1f);
            scrollbarBg.rectTransform.pivot = new Vector2(1f, 0.5f);
            scrollbarBg.rectTransform.anchoredPosition = new Vector2(-14f, 0f);
            scrollbarBg.rectTransform.sizeDelta = new Vector2(14f, -34f);
            scrollbarBg.raycastTarget = true;

            Image scrollbarHandle = CreateImage("StatsScrollbarHandle", scrollbarBg.transform, NeonCyan);
            scrollbarHandle.rectTransform.sizeDelta = new Vector2(14f, 80f);
            scrollbarHandle.raycastTarget = true;

            statsScrollbar = scrollbarBg.gameObject.AddComponent<Scrollbar>();
            statsScrollbar.direction = Scrollbar.Direction.BottomToTop;
            statsScrollbar.targetGraphic = scrollbarHandle;
            statsScrollbar.handleRect = scrollbarHandle.rectTransform;
            statsScrollbar.size = 0.35f;

            statsScrollRect = bodyBox.gameObject.AddComponent<ScrollRect>();
            statsScrollRect.viewport = bodyBox.rectTransform;
            statsScrollRect.content = statsScrollContent;
            statsScrollRect.vertical = true;
            statsScrollRect.horizontal = false;
            statsScrollRect.movementType = ScrollRect.MovementType.Clamped;
            statsScrollRect.scrollSensitivity = 34f;
            statsScrollRect.verticalScrollbar = statsScrollbar;
            statsScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            statsScrollRect.verticalScrollbarSpacing = -4f;

            TMP_Text closeText;
            statsCloseBg = CreateHelpButton(box.transform, "StatsClose", "X", new Vector2(366f, 238f), false, out closeText);
            EventTrigger closeTrigger = statsCloseBg.gameObject.GetComponent<EventTrigger>();
            if (closeTrigger == null) closeTrigger = statsCloseBg.gameObject.AddComponent<EventTrigger>();
            AddSimpleClick(closeTrigger, CloseStatsPanel);

            RefreshStatsPanel();
        }

        private void OpenStatsPanel()
        {
            if (statsPanelGroup == null) return;
            if (profilePanelOpen) CloseProfilePanel();
            if (creditsPanelOpen) CloseCreditsPanel();
            if (helpPanelOpen) CloseHelpPanel();
            if (achievementsPanelOpen) CloseAchievementsPanel();

            RefreshStatsPanel();
            statsPanelOpen = true;
            statsPop = 1f;
            statsPanelGroup.alpha = 1f;
            statsPanelGroup.interactable = true;
            statsPanelGroup.blocksRaycasts = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void CloseStatsPanel()
        {
            statsPanelOpen = false;
            if (statsPanelGroup == null) return;
            statsPanelGroup.alpha = 0f;
            statsPanelGroup.interactable = false;
            statsPanelGroup.blocksRaycasts = false;
        }

        private void RefreshStatsPanel()
        {
            ProfileStatsStorage.ProfileStatsData data = ProfileStatsStorage.LoadCurrentStats();
            if (data == null)
            {
                if (statsPanelProfileText != null)
                    statsPanelProfileText.text = "NO HAY PERFIL CARGADO";
                if (statsPanelBodyText != null)
                    statsPanelBodyText.text = "<align=\"center\"><color=#FF66D9><b>NO HAY PERFIL CARGADO</b></color>\n\nCarga una partida desde <color=#00F1FF>PERFIL</color> para ver tus estadisticas.</align>";
                if (statsScrollContent != null) statsScrollContent.sizeDelta = new Vector2(statsScrollContent.sizeDelta.x, 360f);
                return;
            }

            if (statsPanelProfileText != null)
                statsPanelProfileText.text = "PERFIL ACTUAL: " + data.profileName;

            if (statsPanelBodyText == null) return;

            if (data.levels == null || data.levels.Count == 0)
            {
                statsPanelBodyText.text = "<align=\"center\"><color=#FFE600><b>SIN ESTADISTICAS</b></color>\n\nJuega un nivel para registrar tus resultados.</align>";
                if (statsScrollContent != null) statsScrollContent.sizeDelta = new Vector2(statsScrollContent.sizeDelta.x, 360f);
                return;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int count = data.levels.Count;
            for (int i = 0; i < count; i++)
            {
                ProfileStatsStorage.LevelStats st = data.levels[i];
                if (st == null) continue;
                string rank = string.IsNullOrEmpty(st.bestRank) ? "-" : st.bestRank;
                string levelName = string.IsNullOrEmpty(st.levelName) ? "NIVEL" : st.levelName.ToUpperInvariant();
                sb.Append("<size=18><color=#00F1FF><b>").Append(levelName).Append("</b></color></size>\n");
                sb.Append("<color=#D8CBFF>Jugado:</color> ").Append(st.timesPlayed);
                sb.Append("  <color=#D8CBFF>Mejor:</color> ").Append(st.bestScore.ToString("0000000"));
                sb.Append("  <color=#D8CBFF>Prec:</color> ").Append(st.bestAccuracy.ToString("0.00")).Append("%");
                sb.Append("  <color=#D8CBFF>Rango:</color> <color=#FFE600><b>").Append(rank).Append("</b></color>");
                sb.Append("  <color=#D8CBFF>Max:</color> ").Append(st.bestMaxCombo).Append("\n");
                sb.Append("<size=14><color=#FFF36B>PERFECTO</color> ").Append(st.totalPerfect);
                sb.Append("  <color=#7CFFB2>BIEN</color> ").Append(st.totalGood);
                sb.Append("  <color=#FFB25C>MAL</color> ").Append(st.totalBad);
                sb.Append("  <color=#FF5A66>FALLO</color> ").Append(st.totalMiss).Append("</size>\n");
                sb.Append("<size=13><color=#9E8DD8>Ultimo:</color> ").Append(st.lastScore.ToString("0000000"));
                sb.Append(" / ").Append(st.lastAccuracy.ToString("0.00")).Append("% / ").Append(st.lastRank).Append("</size>\n\n");
            }
            statsPanelBodyText.text = sb.ToString();
            float contentHeight = Mathf.Max(360f, 104f * Mathf.Max(1, count) + 40f);
            if (statsScrollContent != null)
                statsScrollContent.sizeDelta = new Vector2(statsScrollContent.sizeDelta.x, contentHeight);
            if (statsPanelBodyText != null)
                statsPanelBodyText.rectTransform.sizeDelta = new Vector2(statsPanelBodyText.rectTransform.sizeDelta.x, contentHeight);
            if (statsScrollRect != null)
                statsScrollRect.verticalNormalizedPosition = 1f;
        }

        private void AnimateStatsButton()
        {
            if (statsButton == null || menuGroup == null || !menuGroup.interactable) return;
            float targetScale = statsButtonHover ? 1.07f : 1f + Mathf.Sin(pulse * 1.10f + 2.0f) * 0.015f;
            statsButton.localScale = Vector3.Lerp(statsButton.localScale, Vector3.one * targetScale, Time.unscaledDeltaTime * 8f);
            if (statsButtonGlow != null)
            {
                float alpha = statsButtonHover ? 0.34f : 0.15f + Mathf.Sin(pulse * 1.2f + 2.2f) * 0.05f;
                statsButtonGlow.color = new Color(1f, 0.22f, 0.72f, alpha);
            }
            if (statsButtonIconImage != null)
            {
                Color animated = Color.Lerp(NeonCyan, NeonPink, (Mathf.Sin(pulse * 0.8f + 2.0f) + 1f) * 0.5f);
                statsButtonIconImage.color = Color.Lerp(statsButtonIconImage.color, statsButtonHover ? Color.white : animated, Time.unscaledDeltaTime * 10f);
            }
            if (statsButtonLabel != null)
                statsButtonLabel.color = statsButtonHover ? Color.white : NeonPink;
        }

        private void AnimateStatsPanel()
        {
            if (!statsPanelOpen || statsPanelBox == null) return;
            float scale = 1f + statsPop * 0.045f;
            statsPanelBox.localScale = Vector3.Lerp(statsPanelBox.localScale, Vector3.one * scale, Time.unscaledDeltaTime * 10f);
            if (statsPop < 0.02f)
                statsPanelBox.localScale = Vector3.Lerp(statsPanelBox.localScale, Vector3.one, Time.unscaledDeltaTime * 8f);
        }

        private void CreateMainMenuAchievementsButton(Transform parent)
        {
            GameObject go = new GameObject("AchievementsButton");
            go.transform.SetParent(parent, false);
            achievementsButton = go.AddComponent<RectTransform>();
            achievementsButton.sizeDelta = new Vector2(108f, 108f);
            achievementsButton.anchoredPosition = new Vector2(455f, -128f);

            achievementsButtonBg = go.AddComponent<Image>();
            achievementsButtonBg.sprite = MakeSprite(Color.white);
            achievementsButtonBg.type = Image.Type.Sliced;
            achievementsButtonBg.color = new Color(0.115f, 0.030f, 0.195f, 0.92f);

            achievementsButtonGlow = CreateImage("AchievementsButtonGlow", go.transform, new Color(0.0f, 0.94f, 1f, 0.16f));
            achievementsButtonGlow.sprite = MakeRadialGlowSprite(96);
            achievementsButtonGlow.rectTransform.sizeDelta = new Vector2(118f, 118f);
            achievementsButtonGlow.rectTransform.anchoredPosition = Vector2.zero;
            achievementsButtonGlow.raycastTarget = false;

            GameObject iconGO = new GameObject("AchievementsTrophyIcon");
            iconGO.transform.SetParent(go.transform, false);
            RectTransform iconRt = iconGO.AddComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(62f, 62f);
            iconRt.anchoredPosition = new Vector2(0f, 7f);
            achievementsButtonIconImage = iconGO.AddComponent<Image>();
            achievementsButtonIconImage.sprite = MakeTrophySprite(128);
            achievementsButtonIconImage.color = NeonCyan;
            achievementsButtonIconImage.raycastTarget = false;

            achievementsButtonLabel = CreateText("AchievementsLabel", go.transform, "LOGROS", 11, NeonCyan, FontStyles.Bold);
            achievementsButtonLabel.alignment = TextAlignmentOptions.Center;
            achievementsButtonLabel.characterSpacing = 2f;
            achievementsButtonLabel.rectTransform.sizeDelta = new Vector2(108f, 24f);
            achievementsButtonLabel.rectTransform.anchoredPosition = new Vector2(0f, -38f);

            EventTrigger trigger = go.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();

            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((_) =>
            {
                achievementsButtonHover = true;
                if (achievementsButtonBg != null) achievementsButtonBg.color = new Color(0.21f, 0.05f, 0.34f, 0.98f);
                if (achievementsButtonIconImage != null) achievementsButtonIconImage.color = Color.white;
            });
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((_) =>
            {
                achievementsButtonHover = false;
                if (achievementsButtonBg != null) achievementsButtonBg.color = new Color(0.115f, 0.030f, 0.195f, 0.92f);
                if (achievementsButtonIconImage != null) achievementsButtonIconImage.color = NeonCyan;
            });
            trigger.triggers.Add(exit);

            AddSimpleClick(trigger, OpenAchievementsPanel);
        }

        private void BuildAchievementsPanel(Transform parent)
        {
            GameObject root = new GameObject("AchievementsPanel");
            root.transform.SetParent(parent, false);
            RectTransform rt = root.AddComponent<RectTransform>();
            Stretch(rt);

            achievementsPanelGroup = root.AddComponent<CanvasGroup>();
            achievementsPanelGroup.alpha = 0f;
            achievementsPanelGroup.interactable = false;
            achievementsPanelGroup.blocksRaycasts = false;

            Image dim = CreateImage("AchievementsDim", root.transform, new Color(0.010f, 0.000f, 0.040f, 0.76f));
            Stretch(dim.rectTransform);
            dim.raycastTarget = true;

            Image glow = CreateImage("AchievementsGlow", root.transform, new Color(0.0f, 0.94f, 1f, 0.13f));
            glow.sprite = MakeRadialGlowSprite(160);
            glow.rectTransform.sizeDelta = new Vector2(900f, 630f);
            glow.rectTransform.anchoredPosition = Vector2.zero;
            glow.raycastTarget = false;

            Image box = CreateImage("AchievementsPanelBox", root.transform, new Color(0.095f, 0.030f, 0.160f, 0.985f));
            box.sprite = MakeSprite(Color.white);
            box.type = Image.Type.Sliced;
            box.rectTransform.sizeDelta = new Vector2(780f, 580f);
            box.rectTransform.anchoredPosition = Vector2.zero;
            box.raycastTarget = true;
            achievementsPanelBox = box.rectTransform;

            CreateLine("AchievementsTopLine", box.transform, new Vector2(0f, 258f), new Vector2(620f, 4f), NeonCyan).raycastTarget = false;
            CreateLine("AchievementsBottomLine", box.transform, new Vector2(0f, -258f), new Vector2(620f, 4f), NeonPink).raycastTarget = false;

            achievementsPanelTitle = CreateText("AchievementsTitle", box.transform, "LOGROS", 44, Color.white, FontStyles.Bold);
            achievementsPanelTitle.alignment = TextAlignmentOptions.Center;
            achievementsPanelTitle.characterSpacing = 6f;
            achievementsPanelTitle.rectTransform.sizeDelta = new Vector2(640f, 60f);
            achievementsPanelTitle.rectTransform.anchoredPosition = new Vector2(0f, 222f);

            achievementsPanelProfileText = CreateText("AchievementsProfile", box.transform, "PERFIL: -", 18, NeonCyan, FontStyles.Bold);
            achievementsPanelProfileText.alignment = TextAlignmentOptions.Center;
            achievementsPanelProfileText.characterSpacing = 2f;
            achievementsPanelProfileText.rectTransform.sizeDelta = new Vector2(640f, 34f);
            achievementsPanelProfileText.rectTransform.anchoredPosition = new Vector2(0f, 180f);

            Image bodyBox = CreateImage("AchievementsBodyBox", box.transform, new Color(0.045f, 0.010f, 0.095f, 0.82f));
            bodyBox.sprite = MakeSprite(Color.white);
            bodyBox.type = Image.Type.Sliced;
            bodyBox.rectTransform.sizeDelta = new Vector2(684f, 386f);
            bodyBox.rectTransform.anchoredPosition = new Vector2(0f, -28f);
            bodyBox.raycastTarget = true;

            achievementsScrollRect = bodyBox.gameObject.AddComponent<ScrollRect>();
            achievementsScrollRect.horizontal = false;
            achievementsScrollRect.vertical = true;
            achievementsScrollRect.movementType = ScrollRect.MovementType.Clamped;
            achievementsScrollRect.inertia = true;
            achievementsScrollRect.scrollSensitivity = 46f;

            GameObject viewportGO = new GameObject("AchievementsViewport");
            viewportGO.transform.SetParent(bodyBox.transform, false);
            RectTransform viewportRT = viewportGO.AddComponent<RectTransform>();
            viewportRT.sizeDelta = new Vector2(622f, 348f);
            viewportRT.anchoredPosition = new Vector2(-10f, -2f);
            Image viewportImg = viewportGO.AddComponent<Image>();
            viewportImg.color = new Color(1f, 1f, 1f, 0.001f);
            viewportImg.raycastTarget = true;
            viewportGO.AddComponent<RectMask2D>();

            GameObject contentGO = new GameObject("AchievementsScrollContent");
            contentGO.transform.SetParent(viewportGO.transform, false);
            achievementsScrollContent = contentGO.AddComponent<RectTransform>();
            achievementsScrollContent.anchorMin = new Vector2(0f, 1f);
            achievementsScrollContent.anchorMax = new Vector2(0f, 1f);
            achievementsScrollContent.pivot = new Vector2(0f, 1f);
            achievementsScrollContent.anchoredPosition = Vector2.zero;
            achievementsScrollContent.sizeDelta = new Vector2(600f, 348f);

            achievementsScrollRect.viewport = viewportRT;
            achievementsScrollRect.content = achievementsScrollContent;

            GameObject scrollbarGO = new GameObject("AchievementsScrollbar");
            scrollbarGO.transform.SetParent(bodyBox.transform, false);
            RectTransform scrollbarRT = scrollbarGO.AddComponent<RectTransform>();
            scrollbarRT.sizeDelta = new Vector2(12f, 344f);
            scrollbarRT.anchoredPosition = new Vector2(324f, -2f);
            Image scrollbarBg = scrollbarGO.AddComponent<Image>();
            scrollbarBg.color = new Color(0.0f, 0.94f, 1f, 0.10f);
            scrollbarBg.sprite = MakeSprite(Color.white);
            scrollbarBg.type = Image.Type.Sliced;

            GameObject handleGO = new GameObject("Handle");
            handleGO.transform.SetParent(scrollbarGO.transform, false);
            RectTransform handleRT = handleGO.AddComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(12f, 92f);
            Image handleImg = handleGO.AddComponent<Image>();
            handleImg.color = new Color(0.0f, 0.94f, 1f, 0.92f);
            handleImg.sprite = MakeSprite(Color.white);
            handleImg.type = Image.Type.Sliced;

            achievementsScrollbar = scrollbarGO.AddComponent<Scrollbar>();
            achievementsScrollbar.direction = Scrollbar.Direction.BottomToTop;
            achievementsScrollbar.targetGraphic = handleImg;
            achievementsScrollbar.handleRect = handleRT;
            achievementsScrollbar.value = 1f;
            achievementsScrollRect.verticalScrollbar = achievementsScrollbar;
            achievementsScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

            achievementsPanelBodyText = CreateText("AchievementsBodyFallback", bodyBox.transform, "", 18, TextNormal, FontStyles.Bold);
            achievementsPanelBodyText.alignment = TextAlignmentOptions.Center;
            achievementsPanelBodyText.enableWordWrapping = true;
            achievementsPanelBodyText.richText = true;
            achievementsPanelBodyText.rectTransform.sizeDelta = new Vector2(600f, 260f);
            achievementsPanelBodyText.rectTransform.anchoredPosition = new Vector2(-8f, -18f);
            achievementsPanelBodyText.gameObject.SetActive(false);
            achievementsPanelEmptyText = achievementsPanelBodyText;

            TMP_Text closeText;
            achievementsCloseBg = CreateHelpButton(box.transform, "AchievementsClose", "X", new Vector2(346f, 226f), false, out closeText);
            EventTrigger closeTrigger = achievementsCloseBg.gameObject.GetComponent<EventTrigger>();
            if (closeTrigger == null) closeTrigger = achievementsCloseBg.gameObject.AddComponent<EventTrigger>();
            AddSimpleClick(closeTrigger, CloseAchievementsPanel);

            RefreshAchievementsPanel();
        }

        private void OpenAchievementsPanel()
        {
            if (achievementsPanelGroup == null) return;
            if (profilePanelOpen) CloseProfilePanel();
            if (creditsPanelOpen) CloseCreditsPanel();
            if (helpPanelOpen) CloseHelpPanel();
            if (statsPanelOpen) CloseStatsPanel();

            RefreshAchievementsPanel();
            achievementsPanelOpen = true;
            achievementsPop = 1f;
            achievementsPanelGroup.alpha = 1f;
            achievementsPanelGroup.interactable = true;
            achievementsPanelGroup.blocksRaycasts = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void CloseAchievementsPanel()
        {
            achievementsPanelOpen = false;
            if (achievementsPanelGroup == null) return;
            achievementsPanelGroup.alpha = 0f;
            achievementsPanelGroup.interactable = false;
            achievementsPanelGroup.blocksRaycasts = false;
        }

        private void RefreshAchievementsPanel()
        {
            ClearAchievementRows();
            if (achievementsPanelBodyText != null)
                achievementsPanelBodyText.gameObject.SetActive(false);

            if (!ProfileStatsStorage.TryGetCurrentProfile(out string profileId, out string profileName))
            {
                if (achievementsPanelProfileText != null)
                    achievementsPanelProfileText.text = "NO HAY PERFIL CARGADO";
                if (achievementsPanelBodyText != null)
                {
                    achievementsPanelBodyText.gameObject.SetActive(true);
                    achievementsPanelBodyText.text = "<color=#FF66D9><b>NO HAY PERFIL CARGADO</b></color>\n\nCarga una partida desde <color=#00F1FF>PERFIL</color> para ver tus logros.";
                }
                if (achievementsScrollContent != null)
                    achievementsScrollContent.sizeDelta = new Vector2(600f, 348f);
                return;
            }

            if (achievementsPanelProfileText != null)
                achievementsPanelProfileText.text = "PERFIL ACTUAL: " + profileName;

            HashSet<string> unlocked = ProfileAchievementStorage.GetUnlockedSetForCurrentProfile();
            ProfileAchievementStorage.AchievementDefinition[] defs = ProfileAchievementStorage.GetAllDefinitions();
            if (defs == null || achievementsScrollContent == null) return;

            float rowHeight = 86f;
            float spacing = 10f;
            float contentHeight = Mathf.Max(348f, defs.Length * (rowHeight + spacing) + 12f);
            achievementsScrollContent.sizeDelta = new Vector2(600f, contentHeight);

            for (int i = 0; i < defs.Length; i++)
            {
                var def = defs[i];
                bool isUnlocked = def != null && unlocked.Contains(def.id);
                CreateAchievementRow(def, isUnlocked, i, rowHeight, spacing);
            }

            if (achievementsScrollRect != null)
                achievementsScrollRect.verticalNormalizedPosition = 1f;
        }

        private void ClearAchievementRows()
        {
            for (int i = 0; i < achievementsRowObjects.Count; i++)
            {
                if (achievementsRowObjects[i] != null)
                    Destroy(achievementsRowObjects[i]);
            }
            achievementsRowObjects.Clear();
        }

        private void CreateAchievementRow(ProfileAchievementStorage.AchievementDefinition def, bool unlocked, int index, float rowHeight, float spacing)
        {
            if (def == null || achievementsScrollContent == null) return;

            bool hiddenSecret = def.secret && !unlocked;
            string displayTitle = hiddenSecret ? def.hiddenTitle : def.title;
            string displayDescription = hiddenSecret ? def.hiddenDescription : def.description;
            string iconId = hiddenSecret ? "SECRET_LOCKED" : def.id;

            GameObject row = new GameObject("AchievementRow_" + def.id);
            row.transform.SetParent(achievementsScrollContent, false);
            achievementsRowObjects.Add(row);

            RectTransform rowRt = row.AddComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0f, 1f);
            rowRt.anchorMax = new Vector2(0f, 1f);
            rowRt.pivot = new Vector2(0f, 1f);
            rowRt.sizeDelta = new Vector2(586f, rowHeight);
            rowRt.anchoredPosition = new Vector2(4f, -8f - index * (rowHeight + spacing));

            Image bg = row.AddComponent<Image>();
            bg.sprite = MakeSprite(Color.white);
            bg.type = Image.Type.Sliced;
            bg.color = unlocked ? new Color(0.080f, 0.030f, 0.145f, 0.92f) : new Color(0.040f, 0.018f, 0.075f, 0.82f);
            bg.raycastTarget = false;

            Image accent = CreateImage("Accent", row.transform, unlocked ? NeonCyan : new Color(0.42f, 0.31f, 0.62f, 1f));
            accent.rectTransform.sizeDelta = new Vector2(4f, rowHeight - 18f);
            accent.rectTransform.anchoredPosition = new Vector2(-286f, 0f);
            accent.raycastTarget = false;

            Image iconGlow = CreateImage("IconGlow", row.transform, unlocked ? new Color(0.0f, 0.94f, 1f, 0.18f) : new Color(0.35f, 0.25f, 0.55f, 0.12f));
            iconGlow.sprite = MakeRadialGlowSprite(96);
            iconGlow.rectTransform.sizeDelta = new Vector2(70f, 70f);
            iconGlow.rectTransform.anchoredPosition = new Vector2(-246f, 0f);
            iconGlow.raycastTarget = false;

            Image icon = CreateImage("Icon_" + def.id, row.transform, unlocked ? NeonCyan : new Color(0.45f, 0.36f, 0.66f, 1f));
            icon.sprite = MakeAchievementIconSprite(iconId, 96);
            icon.rectTransform.sizeDelta = new Vector2(48f, 48f);
            icon.rectTransform.anchoredPosition = new Vector2(-246f, 0f);
            icon.raycastTarget = false;

            TMP_Text title = CreateText("Title", row.transform, displayTitle, 17, unlocked ? Color.white : new Color(0.62f, 0.52f, 0.80f, 1f), FontStyles.Bold);
            title.alignment = TextAlignmentOptions.Left;
            title.characterSpacing = 1.5f;
            title.enableWordWrapping = false;
            title.rectTransform.sizeDelta = new Vector2(314f, 26f);
            title.rectTransform.anchoredPosition = new Vector2(-38f, 19f);

            TMP_Text status = CreateText("Status", row.transform, unlocked ? "DESBLOQUEADO" : "BLOQUEADO", 13, unlocked ? NeonCyan : new Color(0.68f, 0.56f, 0.93f, 1f), FontStyles.Bold);
            status.alignment = TextAlignmentOptions.Right;
            status.characterSpacing = 1f;
            status.rectTransform.sizeDelta = new Vector2(150f, 24f);
            status.rectTransform.anchoredPosition = new Vector2(208f, 19f);

            TMP_Text desc = CreateText("Description", row.transform, displayDescription, 13, unlocked ? new Color(0.86f, 0.82f, 1f, 1f) : new Color(0.55f, 0.48f, 0.70f, 1f), FontStyles.Bold);
            desc.alignment = TextAlignmentOptions.Left;
            desc.enableWordWrapping = true;
            desc.rectTransform.sizeDelta = new Vector2(464f, 42f);
            desc.rectTransform.anchoredPosition = new Vector2(36f, -18f);
        }

        private Sprite MakeAchievementIconSprite(string id, int size)
        {
            return AchievementIconFactory.MakeIcon(id, size);
        }

        private Sprite MakeProfileIconSprite(int size)
        {
            Texture2D tex = NewIconTexture(size);
            Vector2 c = IconCenter(size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y) - c;
                    bool head = Mathf.Abs(p.magnitude - size * 0.15f) < size * 0.035f || p.magnitude < size * 0.105f && p.y > size * 0.06f;
                    Vector2 bp = p - new Vector2(0f, -size * 0.18f);
                    bool body = Mathf.Abs(bp.x) < size * 0.22f && bp.y > -size * 0.04f && bp.y < size * 0.10f;
                    bool shoulders = Mathf.Abs(bp.y + size * 0.05f) < size * 0.035f && Mathf.Abs(bp.x) < size * 0.30f;
                    tex.SetPixel(x, y, head || body || shoulders ? Color.white : new Color(1f, 1f, 1f, 0f));
                }
            }
            return IconSprite(tex, size);
        }

        private Sprite MakeMusicNoteIconSprite(int size)
        {
            Texture2D tex = NewIconTexture(size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float xf = x / (float)size;
                    float yf = y / (float)size;
                    bool stem = xf > 0.56f && xf < 0.64f && yf > 0.30f && yf < 0.78f;
                    bool flag = yf > 0.70f && yf < 0.80f && xf > 0.60f && xf < 0.82f;
                    bool head = Mathf.Pow((xf - 0.43f) / 0.19f, 2f) + Mathf.Pow((yf - 0.28f) / 0.12f, 2f) < 1f;
                    tex.SetPixel(x, y, stem || flag || head ? Color.white : new Color(1f, 1f, 1f, 0f));
                }
            }
            return IconSprite(tex, size);
        }

        private Sprite MakeChainIconSprite(int size)
        {
            Texture2D tex = NewIconTexture(size);
            Vector2 c = IconCenter(size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y) - c;
                    bool l1 = Mathf.Abs(Mathf.Pow((p.x + size * 0.16f) / (size * 0.18f), 2f) + Mathf.Pow(p.y / (size * 0.11f), 2f) - 1f) < 0.28f && p.x < size * 0.04f;
                    bool l2 = Mathf.Abs(Mathf.Pow((p.x - size * 0.16f) / (size * 0.18f), 2f) + Mathf.Pow(p.y / (size * 0.11f), 2f) - 1f) < 0.28f && p.x > -size * 0.04f;
                    bool center = Mathf.Abs(p.y) < size * 0.035f && Mathf.Abs(p.x) < size * 0.18f;
                    tex.SetPixel(x, y, l1 || l2 || center ? Color.white : new Color(1f, 1f, 1f, 0f));
                }
            }
            return IconSprite(tex, size);
        }

        private Sprite MakeTargetIconSprite(int size)
        {
            Texture2D tex = NewIconTexture(size);
            Vector2 c = IconCenter(size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y) - c;
                    float r = p.magnitude;
                    bool rings = Mathf.Abs(r - size * 0.31f) < size * 0.025f || Mathf.Abs(r - size * 0.18f) < size * 0.025f || r < size * 0.055f;
                    bool cross = (Mathf.Abs(p.x) < size * 0.020f || Mathf.Abs(p.y) < size * 0.020f) && r < size * 0.36f;
                    tex.SetPixel(x, y, rings || cross ? Color.white : new Color(1f, 1f, 1f, 0f));
                }
            }
            return IconSprite(tex, size);
        }

        private Sprite MakeRankLetterIconSprite(int size, char letter)
        {
            Texture2D tex = NewIconTexture(size);
            Vector2 c = IconCenter(size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y) - c;
                    bool border = Mathf.Abs(p.y + size * 0.33f) < size * 0.025f && Mathf.Abs(p.x) < size * 0.32f;
                    bool draw = border;
                    if (letter == 'A')
                    {
                        draw |= Mathf.Abs(Mathf.Abs(p.x) - (p.y + size * 0.10f) * 0.55f) < size * 0.035f && p.y > -size * 0.23f && p.y < size * 0.28f;
                        draw |= Mathf.Abs(p.y - size * 0.02f) < size * 0.030f && Mathf.Abs(p.x) < size * 0.16f;
                    }
                    else
                    {
                        float r1 = Vector2.Distance(p, new Vector2(0f, size * 0.13f));
                        float r2 = Vector2.Distance(p, new Vector2(0f, -size * 0.13f));
                        draw |= (Mathf.Abs(r1 - size * 0.20f) < size * 0.035f && p.x < size * 0.20f) || (Mathf.Abs(r2 - size * 0.20f) < size * 0.035f && p.x > -size * 0.20f);
                        draw |= Mathf.Abs(p.y) < size * 0.030f && Mathf.Abs(p.x) < size * 0.20f;
                    }
                    tex.SetPixel(x, y, draw ? Color.white : new Color(1f, 1f, 1f, 0f));
                }
            }
            return IconSprite(tex, size);
        }

        private Sprite MakeComboSparkIconSprite(int size)
        {
            Texture2D tex = NewIconTexture(size);
            Vector2 c = IconCenter(size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y) - c;
                    float angle = Mathf.Atan2(p.y, p.x);
                    float r = p.magnitude;
                    float starR = size * (0.18f + 0.09f * Mathf.Abs(Mathf.Sin(angle * 4f)));
                    bool star = r < starR;
                    bool ring = Mathf.Abs(r - size * 0.32f) < size * 0.025f;
                    tex.SetPixel(x, y, star || ring ? Color.white : new Color(1f, 1f, 1f, 0f));
                }
            }
            return IconSprite(tex, size);
        }

        private Sprite MakeCrownTrophyIconSprite(int size)
        {
            Texture2D tex = NewIconTexture(size);
            Vector2 c = IconCenter(size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y) - c;
                    bool crown = p.y > size * 0.03f && p.y < size * 0.24f && Mathf.Abs(p.x) < size * 0.33f;
                    crown &= p.y < size * 0.10f + Mathf.Abs(Mathf.Sin((p.x / size + 0.5f) * Mathf.PI * 3f)) * size * 0.17f;
                    bool baseLine = Mathf.Abs(p.y - size * 0.01f) < size * 0.035f && Mathf.Abs(p.x) < size * 0.32f;
                    bool trophy = p.y > -size * 0.32f && p.y < -size * 0.02f && Mathf.Abs(p.x) < Mathf.Lerp(size * 0.15f, size * 0.26f, Mathf.InverseLerp(-size * 0.32f, -size * 0.02f, p.y));
                    bool stem = Mathf.Abs(p.x) < size * 0.055f && p.y > -size * 0.42f && p.y < -size * 0.28f;
                    bool foot = Mathf.Abs(p.y + size * 0.43f) < size * 0.035f && Mathf.Abs(p.x) < size * 0.30f;
                    tex.SetPixel(x, y, crown || baseLine || trophy || stem || foot ? Color.white : new Color(1f, 1f, 1f, 0f));
                }
            }
            return IconSprite(tex, size);
        }

        private Texture2D NewIconTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0f));
            return tex;
        }

        private Vector2 IconCenter(int size)
        {
            return new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        }

        private Sprite IconSprite(Texture2D tex, int size)
        {
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private void AnimateAchievementsButton()
        {
            if (achievementsButton == null || menuGroup == null || !menuGroup.interactable) return;
            float targetScale = achievementsButtonHover ? 1.07f : 1f + Mathf.Sin(pulse * 1.10f + 2.55f) * 0.015f;
            achievementsButton.localScale = Vector3.Lerp(achievementsButton.localScale, Vector3.one * targetScale, Time.unscaledDeltaTime * 8f);
            if (achievementsButtonGlow != null)
            {
                float alpha = achievementsButtonHover ? 0.38f : 0.14f + Mathf.Sin(pulse * 1.2f + 2.5f) * 0.045f;
                achievementsButtonGlow.color = new Color(0.0f, 0.94f, 1f, alpha);
            }
            if (achievementsButtonIconImage != null)
            {
                Color animated = Color.Lerp(NeonCyan, new Color(0.50f, 0.74f, 1f, 1f), (Mathf.Sin(pulse * 0.8f + 2.4f) + 1f) * 0.5f);
                achievementsButtonIconImage.color = Color.Lerp(achievementsButtonIconImage.color, achievementsButtonHover ? Color.white : animated, Time.unscaledDeltaTime * 10f);
            }
            if (achievementsButtonLabel != null)
                achievementsButtonLabel.color = achievementsButtonHover ? Color.white : NeonCyan;
        }

        private void AnimateAchievementsPanel()
        {
            if (!achievementsPanelOpen || achievementsPanelBox == null) return;
            float scale = 1f + achievementsPop * 0.045f;
            achievementsPanelBox.localScale = Vector3.Lerp(achievementsPanelBox.localScale, Vector3.one * scale, Time.unscaledDeltaTime * 10f);
            if (achievementsPop < 0.02f)
                achievementsPanelBox.localScale = Vector3.Lerp(achievementsPanelBox.localScale, Vector3.one, Time.unscaledDeltaTime * 8f);
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


        private Sprite MakeCreditsBadgeSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(1f, 1f, 1f, 0f);
            Color solid = Color.white;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float outer = size * 0.40f;
            float inner = size * 0.34f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y) - center;
                    float r = p.magnitude;
                    bool ring = r <= outer && r >= inner;
                    bool smallDot = (p - new Vector2(0f, size * 0.15f)).magnitude <= size * 0.055f;
                    bool stem = Mathf.Abs(p.x) <= size * 0.04f && p.y <= size * 0.04f && p.y >= -size * 0.22f;
                    bool baseLine = Mathf.Abs(p.y + size * 0.23f) <= size * 0.025f && Mathf.Abs(p.x) <= size * 0.17f;
                    tex.SetPixel(x, y, ring || smallDot || stem || baseLine ? solid : clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private Sprite MakeUserProfileSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(1f, 1f, 1f, 0f);
            Color solid = Color.white;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float outer = size * 0.42f;
            float ringInner = size * 0.36f;
            Vector2 headCenter = new Vector2(0f, size * 0.12f);
            float headRadius = size * 0.13f;
            Vector2 bodyCenter = new Vector2(0f, -size * 0.16f);
            float bodyRadiusX = size * 0.24f;
            float bodyRadiusY = size * 0.17f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y) - center;
                    float r = p.magnitude;
                    bool ring = r <= outer && r >= ringInner;
                    bool head = (p - headCenter).magnitude <= headRadius;
                    Vector2 bp = p - bodyCenter;
                    bool body = (bp.x * bp.x) / (bodyRadiusX * bodyRadiusX) + (bp.y * bp.y) / (bodyRadiusY * bodyRadiusY) <= 1f && bp.y <= bodyRadiusY * 0.65f;
                    tex.SetPixel(x, y, ring || head || body ? solid : clear);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private Sprite MakeStatsBarsSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(1f, 1f, 1f, 0f);
            Color solid = Color.white;
            float baseY = size * 0.22f;
            float lineH = size * 0.055f;
            float barW = size * 0.18f;
            float gap = size * 0.055f;
            float startX = size * 0.24f;
            float[] heights = { size * 0.42f, size * 0.62f, size * 0.50f };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool draw = Mathf.Abs(y - baseY) <= lineH * 0.5f && x >= size * 0.12f && x <= size * 0.88f;
                    for (int b = 0; b < 3; b++)
                    {
                        float x0 = startX + b * (barW + gap);
                        float x1 = x0 + barW;
                        float y0 = baseY;
                        float y1 = baseY + heights[b];
                        bool inBar = x >= x0 && x <= x1 && y >= y0 && y <= y1;
                        bool edge = inBar && (Mathf.Abs(x - x0) <= lineH || Mathf.Abs(x - x1) <= lineH || Mathf.Abs(y - y1) <= lineH || Mathf.Abs(y - y0) <= lineH);
                        draw |= edge;
                    }
                    tex.SetPixel(x, y, draw ? solid : clear);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private Sprite MakeTrophySprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(1f, 1f, 1f, 0f);
            Color solid = Color.white;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y) - center;
                    float cupWidth = Mathf.Lerp(size * 0.35f, size * 0.23f, Mathf.InverseLerp(-size * 0.10f, size * 0.30f, p.y));
                    bool cup = p.y > -size * 0.10f && p.y < size * 0.30f && Mathf.Abs(p.x) < cupWidth;
                    bool leftHandle = Mathf.Abs((p.x + size * 0.36f) * (p.x + size * 0.36f) / (size * 0.15f * size * 0.15f) + (p.y - size * 0.12f) * (p.y - size * 0.12f) / (size * 0.18f * size * 0.18f) - 1f) < 0.28f && p.x < -size * 0.20f;
                    bool rightHandle = Mathf.Abs((p.x - size * 0.36f) * (p.x - size * 0.36f) / (size * 0.15f * size * 0.15f) + (p.y - size * 0.12f) * (p.y - size * 0.12f) / (size * 0.18f * size * 0.18f) - 1f) < 0.28f && p.x > size * 0.20f;
                    bool stem = Mathf.Abs(p.x) < size * 0.07f && p.y > -size * 0.34f && p.y < -size * 0.08f;
                    bool baseTop = Mathf.Abs(p.y + size * 0.35f) < size * 0.045f && Mathf.Abs(p.x) < size * 0.25f;
                    bool baseBottom = Mathf.Abs(p.y + size * 0.44f) < size * 0.045f && Mathf.Abs(p.x) < size * 0.36f;
                    Vector2 sp = p - new Vector2(0f, size * 0.11f);
                    float angle = Mathf.Atan2(sp.y, sp.x);
                    float starRadius = size * (0.09f + 0.035f * Mathf.Cos(angle * 5f));
                    bool starCut = sp.magnitude < starRadius;
                    bool draw = cup || leftHandle || rightHandle || stem || baseTop || baseBottom;
                    tex.SetPixel(x, y, draw && !starCut ? solid : clear);
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

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
        private const int MainMenuOptionCount = 4;
        private int selectedIndex;
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

        // Avance 51 - acceso directo a configuracion desde menu principal.
        private RectTransform settingsButton;
        private Image settingsButtonBg;
        private TMP_Text settingsButtonIcon;
        private Image settingsButtonIconImage;
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

        private static readonly Color BgDark = new Color(0.003f, 0.006f, 0.014f, 1f);
        private static readonly Color Panel = new Color(0.015f, 0.035f, 0.065f, 0.92f);
        private static readonly Color PanelSoft = new Color(0.04f, 0.10f, 0.16f, 0.58f);
        private static readonly Color NeonCyan = new Color(0.0f, 0.92f, 1f, 1f);
        private static readonly Color NeonYellow = new Color(1f, 0.94f, 0.04f, 1f);
        private static readonly Color NeonOrange = new Color(1f, 0.42f, 0.02f, 1f);
        private static readonly Color TextNormal = new Color(0.92f, 0.90f, 1f, 1f);
        private static readonly Color TextDim = new Color(0.62f, 0.72f, 0.86f, 1f);

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
        }

        private void Update()
        {
            pulse += Time.unscaledDeltaTime * 4.2f;
            AnimateButtons();
            AnimateMenuVisuals();
            AnimateSettingsButton();


            if (menuGroup == null || menuGroup.alpha < 0.95f) return;

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) ||
                Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                selectedIndex += (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) ? -1 : 1;
                if (selectedIndex < 0) selectedIndex = MainMenuOptionCount - 1;
                if (selectedIndex >= MainMenuOptionCount) selectedIndex = 0;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
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

            // Avance 43: ambientacion del menu inicial. Elementos visuales livianos,
            // creados por codigo para no depender de assets externos ni tocar gameplay.
            rgbAuraImage = CreateImage("MenuAmbientAura", canvas.transform, new Color(0f, 0.82f, 1f, 0.055f));
            rgbAuraImage.rectTransform.sizeDelta = new Vector2(980f, 540f);
            rgbAuraImage.rectTransform.anchoredPosition = new Vector2(0f, 0f);

            topRgbLine = CreateLine("MenuTopNeonLine", canvas.transform, new Vector2(0f, 316f), new Vector2(760f, 4f), NeonOrange);
            bottomRgbLine = CreateLine("MenuBottomNeonLine", canvas.transform, new Vector2(0f, -316f), new Vector2(760f, 4f), NeonCyan);

            ambientBars = new Image[12];
            for (int i = 0; i < ambientBars.Length; i++)
            {
                float x = -560f + i * 102f;
                float h = 120f + (i % 4) * 42f;
                Image bar = CreateLine("MenuAmbientBar_" + i, canvas.transform, new Vector2(x, -230f + (i % 3) * 20f), new Vector2(3.5f, h), i % 2 == 0 ? NeonCyan : NeonOrange);
                bar.color = new Color(bar.color.r, bar.color.g, bar.color.b, 0.18f);
                bar.rectTransform.localEulerAngles = new Vector3(0f, 0f, i % 2 == 0 ? -18f : 18f);
                ambientBars[i] = bar;
            }

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
            Image panel = CreateImage("MainPanel", menuGroup.transform, new Color(0.01f, 0.025f, 0.048f, 0.94f));
            panel.rectTransform.sizeDelta = new Vector2(680f, 500f);
            panel.rectTransform.anchoredPosition = Vector2.zero;
            panel.type = Image.Type.Sliced;
            panel.sprite = MakeSprite(new Color(1f,1f,1f,1f));

            TMP_Text title = CreateText("MenuTitle", menuGroup.transform, "PROJECT BEAT", 54, NeonYellow, FontStyles.Bold);
            menuTitleText = title;
            title.alignment = TextAlignmentOptions.Center;
            title.rectTransform.anchoredPosition = new Vector2(0f, 174f);
            title.rectTransform.sizeDelta = new Vector2(640f, 78f);

            TMP_Text label = CreateText("MenuSubtitle", menuGroup.transform, "SELECCIONA MODO DE JUEGO", 18, NeonCyan, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            label.characterSpacing = 4f;
            label.rectTransform.anchoredPosition = new Vector2(0f, 122f);
            label.rectTransform.sizeDelta = new Vector2(640f, 38f);

            CreateLine("MainPanelTopAccent", menuGroup.transform, new Vector2(0f, 224f), new Vector2(520f, 3f), NeonCyan);
            CreateLine("MainPanelBottomAccent", menuGroup.transform, new Vector2(0f, -224f), new Vector2(520f, 3f), NeonOrange);

            tutorialButton = CreateModeButton(menuGroup.transform, "TutorialButton", new Vector2(0f, 74f), true, out tutorialBg, out tutorialLabel, "TUTORIAL", "Aprende a jugar");
            arcadeButton = CreateModeButton(menuGroup.transform, "ArcadeButton", new Vector2(0f, -12f), true, out arcadeBg, out arcadeLabel, "ARCADE", "Selector de niveles");
            futureButton = CreateModeButton(menuGroup.transform, "FutureButton", new Vector2(0f, -98f), false, out futureBg, out futureLabel, "PROXIMAMENTE", "Nuevo modo futuro");
            exitButton = CreateModeButton(menuGroup.transform, "ExitButton", new Vector2(0f, -184f), true, out exitBg, out exitLabel, "SALIR", "Cerrar el juego");

            CreateMainMenuSettingsButton(menuGroup.transform);
            BuildMainMenuSettingsPanel();

            TMP_Text hint = CreateText("Hint", menuGroup.transform, "[W/S] Navegar     [ENTER] Confirmar     [MOUSE] Seleccionar", 14, TextDim, FontStyles.Bold);
            hint.alignment = TextAlignmentOptions.Center;
            hint.rectTransform.anchoredPosition = new Vector2(0f, -232f);
            hint.rectTransform.sizeDelta = new Vector2(620f, 32f);
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
            });
            trigger.triggers.Add(enter);

            EventTrigger.Entry click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            click.callback.AddListener((_) =>
            {
                selectedIndex = index;
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


        private void CreateMainMenuSettingsButton(Transform parent)
        {
            GameObject go = new GameObject("SettingsGearButton");
            go.transform.SetParent(parent, false);
            settingsButton = go.AddComponent<RectTransform>();
            settingsButton.sizeDelta = new Vector2(96f, 96f);
            settingsButton.anchoredPosition = new Vector2(-430f, 0f);

            settingsButtonBg = go.AddComponent<Image>();
            settingsButtonBg.sprite = MakeSprite(Color.white);
            settingsButtonBg.type = Image.Type.Sliced;
            settingsButtonBg.color = new Color(0.015f, 0.045f, 0.075f, 0.88f);

            Image innerGlow = CreateImage("SettingsGearGlow", go.transform, new Color(0f, 0.92f, 1f, 0.13f));
            innerGlow.rectTransform.sizeDelta = new Vector2(108f, 108f);
            innerGlow.rectTransform.anchoredPosition = Vector2.zero;
            innerGlow.raycastTarget = false;

            GameObject iconGO = new GameObject("SettingsGearIconImage");
            iconGO.transform.SetParent(go.transform, false);
            RectTransform iconRt = iconGO.AddComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(62f, 62f);
            iconRt.anchoredPosition = new Vector2(0f, 8f);
            settingsButtonIconImage = iconGO.AddComponent<Image>();
            settingsButtonIconImage.sprite = MakeGearSprite(96);
            settingsButtonIconImage.color = NeonCyan;
            settingsButtonIconImage.raycastTarget = false;

            TMP_Text small = CreateText("SettingsGearSmall", go.transform, "CONFIG", 12, NeonOrange, FontStyles.Bold);
            small.alignment = TextAlignmentOptions.Center;
            small.characterSpacing = 3f;
            small.rectTransform.sizeDelta = new Vector2(96f, 24f);
            small.rectTransform.anchoredPosition = new Vector2(0f, -32f);

            EventTrigger trigger = go.AddComponent<EventTrigger>();
            trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();

            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((_) =>
            {
                if (settingsButtonBg != null) settingsButtonBg.color = new Color(0.06f, 0.16f, 0.23f, 0.98f);
                if (settingsButtonIconImage != null) settingsButtonIconImage.color = NeonOrange;
            });
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((_) =>
            {
                if (settingsButtonBg != null) settingsButtonBg.color = new Color(0.015f, 0.045f, 0.075f, 0.88f);
                if (settingsButtonIconImage != null) settingsButtonIconImage.color = NeonCyan;
            });
            trigger.triggers.Add(exit);

            EventTrigger.Entry click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            click.callback.AddListener((_) => OpenMainMenuSettings());
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
            if (settingsButton == null || !menuGroup.interactable) return;
            float s = 1f + Mathf.Sin(pulse * 1.15f) * 0.018f;
            settingsButton.localScale = Vector3.Lerp(settingsButton.localScale, Vector3.one * s, Time.unscaledDeltaTime * 5f);
            if (settingsButtonIconImage != null)
                settingsButtonIconImage.color = Color.Lerp(NeonCyan, NeonOrange, (Mathf.Sin(pulse * 0.75f) + 1f) * 0.5f);
        }

        private RectTransform CreateModeButton(Transform parent, string name, Vector2 pos, bool available, out Image bg, out TMP_Text label, string textOverride = null, string subOverride = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500f, 70f);
            rt.anchoredPosition = pos;
            bg = go.AddComponent<Image>();
            bg.sprite = MakeSprite(Color.white);
            bg.type = Image.Type.Sliced;
            bg.color = available ? PanelSoft : new Color(0.08f, 0.08f, 0.12f, 0.62f);

            string text = textOverride ?? (available ? "ARCADE" : "PROXIMAMENTE");
            string sub = subOverride ?? (available ? "Selector de niveles" : "Nuevo modo futuro");

            label = CreateText("Text", go.transform, text + "\n" + sub, available ? 27 : 23, available ? NeonYellow : TextDim, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.sizeDelta = rt.sizeDelta;
            label.rectTransform.anchoredPosition = Vector2.zero;
            label.lineSpacing = -18f;
            int buttonIndex = name.Contains("Tutorial") ? 0 : name.Contains("Arcade") ? 1 : name.Contains("Future") ? 2 : 3;
            AddMouseEventsToModeButton(go, buttonIndex, available);
            return rt;
        }

        private void AnimateMenuVisuals()
        {
            if (menuGroup == null || menuGroup.alpha <= 0.01f) return;

            float wave = (Mathf.Sin(pulse * 0.72f) + 1f) * 0.5f;
            if (menuTitleText != null)
            {
                float titleScale = 1f + Mathf.Sin(pulse * 0.55f) * 0.018f;
                menuTitleText.rectTransform.localScale = Vector3.Lerp(menuTitleText.rectTransform.localScale, Vector3.one * titleScale, Time.unscaledDeltaTime * 6f);
                menuTitleText.color = Color.Lerp(NeonYellow, Color.white, wave * 0.22f);
            }

            if (rgbAuraImage != null)
                rgbAuraImage.color = Color.Lerp(new Color(0f, 0.75f, 1f, 0.035f), new Color(1f, 0.35f, 0f, 0.055f), wave);

            if (topRgbLine != null)
                topRgbLine.color = Color.Lerp(NeonOrange, NeonCyan, wave);
            if (bottomRgbLine != null)
                bottomRgbLine.color = Color.Lerp(NeonCyan, NeonOrange, wave);

            if (ambientBars != null)
            {
                for (int i = 0; i < ambientBars.Length; i++)
                {
                    Image bar = ambientBars[i];
                    if (bar == null) continue;
                    float local = (Mathf.Sin(pulse * 0.9f + i * 0.65f) + 1f) * 0.5f;
                    Color baseColor = i % 2 == 0 ? NeonCyan : NeonOrange;
                    bar.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.08f + local * 0.18f);
                    Vector2 size = bar.rectTransform.sizeDelta;
                    size.y = 90f + local * 170f;
                    bar.rectTransform.sizeDelta = size;
                }
            }
        }

        private void AnimateButtons()
        {
            AnimateButton(tutorialButton, tutorialBg, tutorialLabel, selectedIndex == 0, true);
            AnimateButton(arcadeButton, arcadeBg, arcadeLabel, selectedIndex == 1, true);
            AnimateButton(futureButton, futureBg, futureLabel, selectedIndex == 2, false);
            AnimateButton(exitButton, exitBg, exitLabel, selectedIndex == 3, true);
        }

        private void AnimateButton(RectTransform rt, Image bg, TMP_Text label, bool selected, bool available)
        {
            if (rt == null || bg == null || label == null) return;
            float target = selected ? 1.06f + Mathf.Sin(pulse) * 0.012f : 1f;
            rt.localScale = Vector3.Lerp(rt.localScale, Vector3.one * target, Time.unscaledDeltaTime * 10f);

            Color normal = available ? PanelSoft : new Color(0.08f, 0.08f, 0.12f, 0.62f);
            Color highlight = available ? new Color(1f, 0.42f, 0.02f, 0.86f) : new Color(0.25f, 0.18f, 0.30f, 0.74f);
            bg.color = Color.Lerp(bg.color, selected ? highlight : normal, Time.unscaledDeltaTime * 12f);
            label.color = Color.Lerp(label.color, selected ? (available ? Color.white : NeonOrange) : (available ? NeonYellow : TextDim), Time.unscaledDeltaTime * 12f);
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

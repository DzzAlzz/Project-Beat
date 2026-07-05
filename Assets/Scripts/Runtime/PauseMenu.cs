using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// UI comercial para Project Beat: pausa y selector de nivel.
    /// No cambia LevelManager ni la logica principal; solo redibuja/pule la capa visual y la navegacion.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private CanvasGroup pauseGroup;
        [SerializeField] private CanvasGroup levelSelectGroup;

        [Header("Main Menu Labels (TMP)")]
        [SerializeField] private TMP_Text resumeLabel;
        [SerializeField] private TMP_Text selectLevelLabel;
        [SerializeField] private TMP_Text restartLabel;
        [SerializeField] private TMP_Text quitLabel;

        [Header("Level Select")]
        [SerializeField] private TMP_Text levelNameText;
        [SerializeField] private TMP_Text levelArtistText;
        [SerializeField] private TMP_Text levelHintText;

        [Header("Controller")]
        [SerializeField] private GameController gameController;

        [Header("Modern UI Tuning")]
        [SerializeField] private float fadeSpeed = 10.5f;
        [SerializeField] private float selectedScale = 1.08f;
        [SerializeField] private float normalScale = 1.0f;

        private bool isPaused;
        private bool isInLevelSelect;
        private bool isInSettings;
        private bool isInCredits;
        private int selectedOption;
        private int selectedSettingsOption;
        private bool settingsOpenedFromMainMenu;
        private System.Action onMainMenuSettingsClosed;
        private const int OptionCount = 7;
        private const int SettingsOptionCount = 8;

        private const string BrightnessPrefsKey = "ProjectBeat_Brightness";
        private const string MasterVolumePrefsKey = "ProjectBeat_MasterVolume";
        private const string ResolutionPrefsKey = "ProjectBeat_ResolutionIndex";
        private const string DisplayModePrefsKey = "ProjectBeat_DisplayModeIndex";
        private const string MainSceneName = "ProjectBeat_Demo";
        private float brightness = 1f;
        private float masterVolume = 1f;
        private int resolutionIndex = 3;
        private int displayModeIndex = 0;

        private static readonly Color DeepDim = new Color(0.006f, 0.007f, 0.014f, 0.88f);
        private static readonly Color Glass = new Color(0.025f, 0.035f, 0.065f, 0.92f);
        private static readonly Color GlassLight = new Color(0.04f, 0.10f, 0.16f, 0.30f);
        private static readonly Color NeonYellow = new Color(1f, 0.94f, 0.04f, 1f);
        private static readonly Color NeonOrange = new Color(1f, 0.42f, 0.02f, 1f);
        private static readonly Color NeonCyan = new Color(0.0f, 0.92f, 1f, 1f);
        private static readonly Color TextNormal = new Color(0.92f, 0.90f, 1f, 1f);
        private static readonly Color TextDim = new Color(0.70f, 0.68f, 0.78f, 1f);

        public bool IsPausedForOverlay => isPaused;

        private static readonly string[] OptionNames = { "CONTINUAR", "ELEGIR NIVEL", "REINICIAR", "CONFIGURACION", "CREDITOS", "MENU PRINCIPAL", "SALIR" };
        private static readonly string[] OptionIcons = { "PLAY", "LEVEL", "RETRY", "SETUP", "INFO", "HOME", "QUIT" };
        private static readonly Vector2Int[] ResolutionOptions =
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1366, 768),
            new Vector2Int(1600, 900),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440)
        };
        private static readonly string[] DisplayModeNames = { "PANTALLA COMPLETA", "VENTANA", "VENTANA SIN BORDES" };

        private TMP_Text settingsMenuLabel;
        private TMP_Text[] menuLabels;
        private Image[] menuButtonImages;
        private Image[] menuGlowImages;
        private CanvasGroup[] menuButtonGroups;
        private RectTransform[] menuButtonRects;
        private CanvasGroup settingsGroup;
        private TMP_Text settingsTitleText;
        private TMP_Text settingsBodyText;
        private TMP_Text settingsHintText;
        private TMP_Text controlsHeaderText;
        private TMP_Text controlsDescriptionText;
        private TMP_Text graphicsHeaderText;
        private TMP_Text brightnessLabelText;
        private TMP_Text resolutionLabelText;
        private TMP_Text resolutionValueText;
        private TMP_Text displayModeLabelText;
        private TMP_Text displayModeValueText;
        private TMP_Text effectsLabelText;
        private TMP_Text sensitivityLabelText;
        private TMP_Text soundHeaderText;
        private TMP_Text volumeLabelText;
        private TMP_Text settingsBackText;
        private CanvasGroup creditsGroup;
        private TMP_Text creditsTitleText;
        private TMP_Text creditsBodyText;
        private TMP_Text creditsHintText;
        private Image brightnessOverlay;
        private Image brightnessSliderFill;
        private Image brightnessSliderGlow;
        private RectTransform brightnessSliderHandle;
        private TMP_Text brightnessValueText;
        private Image effectsSliderFill;
        private Image effectsSliderGlow;
        private RectTransform effectsSliderHandle;
        private TMP_Text effectsValueText;
        private TMP_Text sensitivityValueText;
        private CanvasGroup mainMenuLoadingGroup;
        private TMP_Text mainMenuLoadingText;
        private bool isReturningToInitialMenu;
        private bool isResumeCountdown;
        private string resumeCountdownLabel = string.Empty;
        private Coroutine resumeCountdownCoroutine;
        private static bool mainMenuSceneLoadPending;
        private Image volumeSliderFill;
        private Image volumeSliderGlow;
        private RectTransform volumeSliderHandle;
        private TMP_Text volumeValueText;

        private float fadeAlpha;
        private float fadeTarget;
        private float levelAlpha;
        private float levelTarget;
        private float pulseT;

        private Sprite roundedPanelSprite;
        private Sprite roundedButtonSprite;
        private Sprite lineSprite;

        public bool IsPaused => isPaused;

        private void Awake()
        {
            menuLabels = new TMP_Text[OptionCount];
            menuLabels[0] = resumeLabel;
            menuLabels[1] = selectLevelLabel;
            menuLabels[2] = restartLabel;
            menuLabels[3] = settingsMenuLabel;
            menuLabels[4] = null;
            menuLabels[5] = null;
            menuLabels[6] = quitLabel;
            BuildSprites();
            BuildCommercialPauseMenu();
            BuildCommercialLevelSelect();
            BuildSettingsPanel();
            BuildCreditsPanel();
            BuildBrightnessOverlay();
            BuildMainMenuLoadingOverlay();
            ShowMainMenuLoading(false);
            LoadSettingsPrefs();
            ApplyVisualAudioSettings();
            StyleStaticTexts();
            ShowLevelSelectGroup(false, true);
            ShowSettingsGroup(false, true);
            ShowCreditsGroup(false, true);

            if (pauseGroup != null)
            {
                pauseGroup.alpha = 0f;
                pauseGroup.interactable = false;
                pauseGroup.blocksRaycasts = false;
            }
        }

        private void Update()
        {
            AnimateGroups();
            pulseT += Time.unscaledDeltaTime * 5.4f;

            if (isReturningToInitialMenu || isResumeCountdown)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!isPaused && gameController != null && gameController.IsGameplayRunning)
                    OpenPause();
                else if (isPaused && isInSettings)
                    ExitSettings();
                else if (isPaused && isInCredits)
                    ExitCredits();
                else if (isPaused && isInLevelSelect)
                    ExitLevelSelect();
                else if (isPaused)
                    ClosePause();
                return;
            }

            if (!isPaused) return;

            if (isInSettings)
                HandleSettingsInput();
            else if (isInCredits)
                HandleCreditsInput();
            else if (isInLevelSelect)
                HandleLevelSelectInput();
            else
                HandleMenuInput();

            ApplyVisualAudioSettings();

            AnimateMenuLabels();
        }

        public void OpenPause()
        {
            if (isReturningToInitialMenu || isResumeCountdown) return;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Avance 50: al abrir pausa se limpia cualquier subpanel residual.
            // Esto evita el panel morado/intermedio que quedaba activo sin mostrar
            // correctamente el menu de pausa.
            isPaused = true;
            isInLevelSelect = false;
            isInSettings = false;
            isInCredits = false;
            Time.timeScale = 0f;
            if (gameController != null) gameController.PauseAudio(true);

            selectedOption = 0;
            fadeTarget = 1f;
            levelTarget = 0f;
            ShowMainMenuLoading(false);
            ShowLevelSelectGroup(false, true);
            ShowSettingsGroup(false, true);
            ShowCreditsGroup(false, true);
            ShowPauseGroup(true);
            RefreshLabels();
        }

        public void ClosePause()
        {
            // Avance 50: Continuar ya no reanuda instantaneamente.
            // Se usa una cuenta regresiva con el gameplay congelado para no
            // perder notas al volver desde pausa.
            if (isResumeCountdown || isReturningToInitialMenu) return;
            resumeCountdownCoroutine = StartCoroutine(ResumeCountdownRoutine());
        }

        // Avance 50.1: cierre seguro para transiciones/cargas.
        // IMPORTANTE: no inicia la cuenta regresiva. Se usa cuando ResultsScreen,
        // reinicio o menu principal necesitan ocultar la pausa sin ejecutar 3-2-1-GO.
        public void ForceCloseWithoutResumeCountdown()
        {
            if (resumeCountdownCoroutine != null)
            {
                StopCoroutine(resumeCountdownCoroutine);
                resumeCountdownCoroutine = null;
            }

            isResumeCountdown = false;
            resumeCountdownLabel = string.Empty;
            isPaused = false;
            isInLevelSelect = false;
            isInSettings = false;
            isInCredits = false;

            fadeTarget = 0f;
            levelTarget = 0f;
            ShowPauseGroup(false);
            ShowLevelSelectGroup(false, true);
            ShowSettingsGroup(false, true);
            ShowCreditsGroup(false, true);
        }

        private System.Collections.IEnumerator ResumeCountdownRoutine()
        {
            isResumeCountdown = true;
            isInLevelSelect = false;
            isInSettings = false;
            isInCredits = false;

            ShowPauseGroup(false);
            ShowLevelSelectGroup(false, true);
            ShowSettingsGroup(false, true);
            ShowCreditsGroup(false, true);
            ShowMainMenuLoading(false);

            Time.timeScale = 0f;
            if (gameController != null) gameController.PauseAudio(true);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;

            string[] labels = { "3", "2", "1", "GO" };
            for (int i = 0; i < labels.Length; i++)
            {
                resumeCountdownLabel = labels[i];
                yield return new WaitForSecondsRealtime(i == labels.Length - 1 ? 0.45f : 0.70f);
            }

            resumeCountdownLabel = string.Empty;
            isResumeCountdown = false;
            isPaused = false;
            fadeTarget = 0f;
            levelTarget = 0f;

            Time.timeScale = 1f;
            if (gameController != null) gameController.PauseAudio(false);
            resumeCountdownCoroutine = null;
        }

        private void OnGUI()
        {
            // Avance 50.1: el countdown solo pertenece a CONTINUAR.
            // Si hay carga/retorno a menu, nunca debe dibujarse encima de CARGANDO...
            if (!isResumeCountdown || isReturningToInitialMenu || string.IsNullOrEmpty(resumeCountdownLabel)) return;
            if (mainMenuLoadingGroup != null && mainMenuLoadingGroup.alpha > 0.01f) return;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height * 0.13f, 72f, 150f)),
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = new Color(1f, 0.94f, 0.04f, 1f);

            GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), resumeCountdownLabel, style);
        }

        private void HandleMenuInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                selectedOption = (selectedOption - 1 + OptionCount) % OptionCount;
                RefreshLabels();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                selectedOption = (selectedOption + 1) % OptionCount;
                RefreshLabels();
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                ConfirmOption();
            }
        }

        private void ConfirmOption()
        {
            switch (selectedOption)
            {
                case 0: ClosePause(); break;
                case 1: EnterLevelSelect(); break;
                case 2: RestartLevel(); break;
                case 3: EnterSettings(); break;
                case 4: EnterCredits(); break;
                case 5: ReturnToMainMenu(); break;
                case 6: QuitGame(); break;
            }
        }

        private void EnterLevelSelect()
        {
            isInLevelSelect = true;
            ShowPauseGroup(false);
            ShowLevelSelectGroup(true);
            RefreshLevelSelectLabels();
        }

        private void ExitLevelSelect()
        {
            isInLevelSelect = false;
            ShowPauseGroup(true);
            ShowLevelSelectGroup(false);
            RefreshLabels();
        }

        public void OpenSettingsFromMainMenu(System.Action onClosed = null)
        {
            // Avance 53: se reutiliza exactamente el panel de configuracion de PauseMenu.
            // Causa corregida: en Avance 52 se activaba SettingsGroup, pero se mantenia
            // PauseGroup con fadeTarget = 0. Como SettingsGroup es hijo de PauseGroup,
            // el CanvasGroup padre dejaba todos los textos/sliders invisibles.
            settingsOpenedFromMainMenu = true;
            onMainMenuSettingsClosed = onClosed;
            isPaused = true;
            isInSettings = true;
            isInCredits = false;
            isInLevelSelect = false;
            selectedSettingsOption = 0;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 1f;

            // El panel real de configuracion vive dentro de pauseGroup, por eso
            // el grupo padre debe quedar visible mientras se abre desde el menu inicial.
            fadeAlpha = 1f;
            fadeTarget = 1f;
            if (pauseGroup != null)
            {
                pauseGroup.alpha = 1f;
                pauseGroup.interactable = true;
                pauseGroup.blocksRaycasts = true;
                pauseGroup.transform.localScale = Vector3.one;
            }

            ShowLevelSelectGroup(false, true);
            ShowCreditsGroup(false, true);
            ShowSettingsGroup(true, true);
            RefreshSettingsPanel();
        }

        private void EnterSettings()
        {
            isInSettings = true;
            isInCredits = false;
            isInLevelSelect = false;
            selectedSettingsOption = 0;
            ShowLevelSelectGroup(false);
            ShowSettingsGroup(true);
            RefreshSettingsPanel();
        }

        private void ExitSettings()
        {
            isInSettings = false;
            ShowSettingsGroup(false);

            if (settingsOpenedFromMainMenu)
            {
                settingsOpenedFromMainMenu = false;
                isPaused = false;
                fadeTarget = 0f;
                levelTarget = 0f;
                ShowPauseGroup(false);
                ShowLevelSelectGroup(false, true);
                ShowCreditsGroup(false, true);

                System.Action callback = onMainMenuSettingsClosed;
                onMainMenuSettingsClosed = null;
                callback?.Invoke();
                return;
            }

            RefreshLabels();
        }

        private void EnterCredits()
        {
            isInCredits = true;
            isInSettings = false;
            isInLevelSelect = false;
            ShowLevelSelectGroup(false);
            ShowSettingsGroup(false);
            ShowCreditsGroup(true);
            RefreshCreditsPanel();
        }

        private void ExitCredits()
        {
            isInCredits = false;
            ShowCreditsGroup(false);
            RefreshLabels();
        }

        private void HandleCreditsInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.Space))
            {
                ExitCredits();
            }
        }

        private void HandleSettingsInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                selectedSettingsOption = (selectedSettingsOption - 1 + SettingsOptionCount) % SettingsOptionCount;
                RefreshSettingsPanel();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                selectedSettingsOption = (selectedSettingsOption + 1) % SettingsOptionCount;
                RefreshSettingsPanel();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                AdjustSelectedSetting(-0.05f);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                AdjustSelectedSetting(0.05f);
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                if (selectedSettingsOption == 5)
                {
                    VisualAccessibilitySettings.ToggleSensitivityMode();
                    ApplyVisualAudioSettings();
                    RefreshSettingsPanel();
                }
                else if (selectedSettingsOption == 7)
                    ExitSettings();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitSettings();
            }
        }

        private void AdjustSelectedSetting(float delta)
        {
            switch (selectedSettingsOption)
            {
                case 1:
                    brightness = Mathf.Clamp(brightness + delta, 0.55f, 1.35f);
                    PlayerPrefs.SetFloat(BrightnessPrefsKey, brightness);
                    break;
                case 2:
                    resolutionIndex = WrapIndex(resolutionIndex + (delta > 0f ? 1 : -1), ResolutionOptions.Length);
                    PlayerPrefs.SetInt(ResolutionPrefsKey, resolutionIndex);
                    ApplyDisplaySettings();
                    break;
                case 3:
                    displayModeIndex = WrapIndex(displayModeIndex + (delta > 0f ? 1 : -1), DisplayModeNames.Length);
                    PlayerPrefs.SetInt(DisplayModePrefsKey, displayModeIndex);
                    ApplyDisplaySettings();
                    break;
                case 4:
                    VisualAccessibilitySettings.AdjustIntensity(delta > 0f ? 1 : -1);
                    break;
                case 5:
                    VisualAccessibilitySettings.ToggleSensitivityMode();
                    break;
                case 6:
                    masterVolume = Mathf.Clamp01(masterVolume + delta);
                    PlayerPrefs.SetFloat(MasterVolumePrefsKey, masterVolume);
                    break;
                case 7:
                    // ENTER vuelve; izquierda/derecha no hacen nada aqui.
                    break;
            }

            PlayerPrefs.Save();
            ApplyVisualAudioSettings();
            RefreshSettingsPanel();
        }

        private void HandleLevelSelectInput()
        {
            LevelManager lm = LevelManager.Instance;
            if (lm == null) return;

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                lm.PreviousLevel();
                RefreshLevelSelectLabels();
                PopText(levelNameText, 1.08f);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                lm.NextLevel();
                RefreshLevelSelectLabels();
                PopText(levelNameText, 1.08f);
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                ConfirmLevelSelect();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitLevelSelect();
            }
        }

        public void OpenLevelSelectFromStartup()
        {
            isPaused = true;
            isInLevelSelect = true;
            Time.timeScale = 0f;
            selectedOption = 1;
            ShowPauseGroup(false);
            ShowLevelSelectGroup(true, true);
            RefreshLevelSelectLabels();
        }

        private void ConfirmLevelSelect()
        {
            LevelManager lm = LevelManager.Instance;
            if (lm == null || lm.Levels == null || lm.Levels.Length == 0)
            {
                RefreshLevelSelectLabels();
                return;
            }

            // Avance 48: al iniciar un nivel desde el selector se usa un flag
            // temporal de memoria, no PlayerPrefs, para evitar estados cruzados
            // entre computadores o sesiones anteriores.
            StartupFlowController.RequestSkipStartupOnce();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }

        private void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void ReturnToMainMenu()
        {
            // Avance 41 Safe Fix: este flujo es independiente del resto de botones.
            // No toca Reiniciar, Tutorial, Elegir Nivel, Configuracion ni Creditos.
            if (isReturningToInitialMenu) return;
            StartCoroutine(ReturnToInitialMenuSafeRoutine());
        }

        private System.Collections.IEnumerator ReturnToInitialMenuSafeRoutine()
        {
            isReturningToInitialMenu = true;

            // Avance 44: flujo lineal. CARGANDO -> flag menu inicial -> recarga limpia.
            // No se reutiliza el GameController actual para evitar estados pegados.
            isPaused = false;
            isInLevelSelect = false;
            isInSettings = false;
            isInCredits = false;
            Time.timeScale = 1f;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            ShowPauseGroup(false);
            ShowLevelSelectGroup(false);
            ShowSettingsGroup(false);
            ShowCreditsGroup(false);
            ShowMainMenuLoading(true);

            CleanupGameplayBeforeMainMenu();

            // Avance 45: antes de recargar se evita dejar seleccionado TUTORIAL como nivel activo,
            // porque el overlay de instrucciones se reconstruia detras del menu inicial.
            // El menu inicial sigue permitiendo entrar a Tutorial, pero el estado base queda limpio.
            if (LevelManager.Instance != null && LevelManager.Instance.Levels != null && LevelManager.Instance.Levels.Length > 1)
                LevelManager.Instance.SetLevel(1);

            StartupFlowController.RequestMainMenuOnNextLoad();

            yield return new WaitForSecondsRealtime(0.95f);

            SceneManager.sceneLoaded -= HandleMainMenuSceneLoadedSafe;
            SceneManager.sceneLoaded += HandleMainMenuSceneLoadedSafe;
            mainMenuSceneLoadPending = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }

        private static void HandleMainMenuSceneLoadedSafe(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= HandleMainMenuSceneLoadedSafe;
            mainMenuSceneLoadPending = false;
            Time.timeScale = 1f;
            StartupFlowController.ForceShowMainMenuOnCurrentScene();
        }

        private void CleanupGameplayBeforeMainMenu()
        {
            if (gameController != null)
                gameController.PauseAudio(false);

            AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
            for (int i = 0; i < audioSources.Length; i++)
            {
                if (audioSources[i] != null)
                    audioSources[i].Stop();
            }

            NoteObject[] notes = FindObjectsOfType<NoteObject>();
            for (int i = 0; i < notes.Length; i++)
            {
                if (notes[i] != null)
                    Destroy(notes[i].gameObject);
            }

            HitEffect[] hitEffects = FindObjectsOfType<HitEffect>();
            for (int i = 0; i < hitEffects.Length; i++)
            {
                if (hitEffects[i] != null)
                    Destroy(hitEffects[i].gameObject);
            }

            ParticleSystem[] particles = FindObjectsOfType<ParticleSystem>();
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i] == null) continue;
                particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            TutorialOverlayController[] tutorialOverlays = FindObjectsOfType<TutorialOverlayController>();
            for (int i = 0; i < tutorialOverlays.Length; i++)
            {
                if (tutorialOverlays[i] != null)
                    Destroy(tutorialOverlays[i].gameObject);
            }
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

        private void RefreshLabels()
        {
            for (int i = 0; i < menuLabels.Length; i++)
            {
                TMP_Text label = menuLabels[i];
                if (label == null) continue;

                bool active = i == selectedOption;
                label.enableWordWrapping = false;
                label.alignment = TextAlignmentOptions.Center;
                label.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
                label.fontSize = active ? 28f : 24f;
                label.characterSpacing = active ? 5f : 3f;
                label.color = active ? NeonYellow : TextNormal;
                label.text = active
                    ? "<color=#00F1FF>></color>  " + OptionNames[i] + "  <color=#00F1FF><</color>"
                    : "<size=18><color=#00F1FF>" + OptionIcons[i] + "</color></size>   " + OptionNames[i];
            }
        }

        private void RefreshLevelSelectLabels()
        {
            LevelManager lm = LevelManager.Instance;
            if (lm == null || lm.Levels == null || lm.Levels.Length == 0 || lm.CurrentLevel == null)
            {
                if (levelNameText != null)
                {
                    levelNameText.alignment = TextAlignmentOptions.Center;
                    levelNameText.fontSize = 34f;
                    levelNameText.text = "<color=#FFF000>NO HAY NIVELES CARGADOS</color>";
                }
                if (levelArtistText != null)
                    levelArtistText.text = "<color=#BFB6FF>Revisa LevelManager y Build limpio.</color>";
                if (levelHintText != null)
                    levelHintText.text = "<color=#FF6A00>[ESC]</color> Volver";
                return;
            }

            LevelData level = lm.CurrentLevel;
            int idx = lm.CurrentLevelIndex;
            int total = lm.Levels.Length;

            if (levelNameText != null)
            {
                levelNameText.alignment = TextAlignmentOptions.Center;
                levelNameText.fontSize = 54f;
                levelNameText.characterSpacing = 3f;
                levelNameText.text =
                    "<size=20><color=#00F1FF>SELECCIONA TU TRACK</color></size>\n" +
                    "<size=36><color=#FF6A00><</color></size>  " +
                    "<b><color=#FFF000>" + level.levelName + "</color></b>" +
                    "  <size=36><color=#FF6A00>></color></size>\n" +
                    "<size=22><color=#BFB6FF>PISTA " + (idx + 1) + " DE " + total + "</color></size>";
            }

            if (levelArtistText != null)
            {
                levelArtistText.alignment = TextAlignmentOptions.Center;
                levelArtistText.fontSize = 25f;
                levelArtistText.characterSpacing = 1.5f;
                levelArtistText.text = "<color=#FFAA44>Nivel " + (idx + 1) + "</color>  <color=#00F1FF>•</color>  <color=#FFFFFF>LISTO PARA JUGAR</color>";
            }

            if (levelHintText != null)
            {
                levelHintText.alignment = TextAlignmentOptions.Center;
                levelHintText.fontSize = 18f;
                levelHintText.characterSpacing = 1.5f;
                levelHintText.text = "<color=#00F1FF>[A/D]</color> Cambiar pista    <color=#FFF000>[ENTER]</color> Iniciar    <color=#FF6A00>[ESC]</color> Volver    <color=#BFB6FF>[MOUSE]</color> Click";
            }
        }

        private void ShowPauseGroup(bool show) { fadeTarget = show ? 1f : 0f; }

        private void ShowLevelSelectGroup(bool show, bool instant = false)
        {
            levelTarget = show ? 1f : 0f;
            if (instant) levelAlpha = levelTarget;
            if (levelSelectGroup != null)
            {
                levelSelectGroup.interactable = show;
                levelSelectGroup.blocksRaycasts = show;
            }
        }

        private void AnimateGroups()
        {
            if (pauseGroup != null)
            {
                fadeAlpha = Mathf.MoveTowards(fadeAlpha, fadeTarget, Time.unscaledDeltaTime * fadeSpeed);
                pauseGroup.alpha = fadeAlpha;
                pauseGroup.interactable = fadeAlpha > 0.5f && !isInLevelSelect;
                pauseGroup.blocksRaycasts = fadeAlpha > 0.5f && !isInLevelSelect;
                float s = Mathf.Lerp(0.94f, 1f, Mathf.SmoothStep(0f, 1f, fadeAlpha));
                pauseGroup.transform.localScale = new Vector3(s, s, 1f);
            }

            if (levelSelectGroup != null)
            {
                levelAlpha = Mathf.MoveTowards(levelAlpha, levelTarget, Time.unscaledDeltaTime * fadeSpeed);
                levelSelectGroup.alpha = levelAlpha;
                levelSelectGroup.interactable = levelAlpha > 0.5f && isInLevelSelect;
                levelSelectGroup.blocksRaycasts = levelAlpha > 0.5f && isInLevelSelect;
                float s = Mathf.Lerp(0.93f, 1f, Mathf.SmoothStep(0f, 1f, levelAlpha));
                levelSelectGroup.transform.localScale = new Vector3(s, s, 1f);
            }
        }

        private void AnimateMenuLabels()
        {
            if (menuLabels == null) return;
            float glow = 0.65f + 0.35f * Mathf.Sin(pulseT);

            for (int i = 0; i < menuLabels.Length; i++)
            {
                TMP_Text label = menuLabels[i];
                if (label == null) continue;
                bool active = i == selectedOption;

                float target = active ? selectedScale : normalScale;
                label.transform.localScale = Vector3.Lerp(label.transform.localScale, new Vector3(target, target, 1f), Time.unscaledDeltaTime * 14f);
                label.color = active ? Color.Lerp(NeonOrange, NeonYellow, glow) : Color.Lerp(label.color, TextDim, Time.unscaledDeltaTime * 7f);

                if (menuButtonImages != null && i < menuButtonImages.Length && menuButtonImages[i] != null)
                {
                    Color targetColor = active ? new Color(1f, 0.34f, 0.02f, 0.32f + glow * 0.18f) : new Color(0.10f, 0.045f, 0.15f, 0.42f);
                    menuButtonImages[i].color = Color.Lerp(menuButtonImages[i].color, targetColor, Time.unscaledDeltaTime * 10f);
                }

                if (menuGlowImages != null && i < menuGlowImages.Length && menuGlowImages[i] != null)
                {
                    Color targetGlow = active ? new Color(1f, 0.78f, 0.05f, 0.20f + glow * 0.16f) : new Color(1f, 0.78f, 0.05f, 0f);
                    menuGlowImages[i].color = Color.Lerp(menuGlowImages[i].color, targetGlow, Time.unscaledDeltaTime * 10f);
                    float glowScale = active ? 1.08f + glow * 0.02f : 0.96f;
                    menuGlowImages[i].rectTransform.localScale = Vector3.Lerp(menuGlowImages[i].rectTransform.localScale, new Vector3(glowScale, glowScale, 1f), Time.unscaledDeltaTime * 10f);
                }

                if (menuButtonRects != null && i < menuButtonRects.Length && menuButtonRects[i] != null)
                {
                    float buttonScale = active ? 1.035f : 1f;
                    menuButtonRects[i].localScale = Vector3.Lerp(menuButtonRects[i].localScale, new Vector3(buttonScale, buttonScale, 1f), Time.unscaledDeltaTime * 12f);
                }

                if (menuButtonGroups != null && i < menuButtonGroups.Length && menuButtonGroups[i] != null)
                    menuButtonGroups[i].alpha = active ? 1f : 0.62f;
            }
        }

        private void StyleStaticTexts()
        {
            TMP_Text[] allTexts = { resumeLabel, selectLevelLabel, restartLabel, settingsMenuLabel, quitLabel, levelNameText, levelArtistText, levelHintText };
            foreach (TMP_Text text in allTexts)
            {
                if (text == null) continue;
                text.enableWordWrapping = false;
                text.alignment = TextAlignmentOptions.Center;
                AddShadow(text);
            }
        }

        private void BuildSprites()
        {
            roundedPanelSprite = MakeRoundedSprite(96, 96, 18, Color.white);
            roundedButtonSprite = MakeRoundedSprite(96, 34, 12, Color.white);
            lineSprite = MakeRoundedSprite(64, 8, 4, Color.white);
        }

        private void BuildCommercialPauseMenu()
        {
            EnsureEventSystem();
            if (pauseGroup == null) return;
            RectTransform root = pauseGroup.GetComponent<RectTransform>();
            if (root == null) return;

            DisableRootImage(pauseGroup);
            RemoveOldVisuals(pauseGroup.transform);
            CreateFullScreenImage(pauseGroup.transform, "PB_UI_DimBlur", DeepDim, 0);
            CreateFullScreenImage(pauseGroup.transform, "PB_UI_ColorWash", new Color(0.00f, 0.01f, 0.03f, 0.34f), 1);
            CreateFloatingGlow(pauseGroup.transform, "PB_UI_Glow_Cyan", new Vector2(-520f, 220f), new Vector2(520f, 110f), new Color(0f, 0.9f, 1f, 0.10f), 2);
            CreateFloatingGlow(pauseGroup.transform, "PB_UI_Glow_Orange", new Vector2(520f, -220f), new Vector2(520f, 110f), new Color(1f, 0.32f, 0f, 0.11f), 3);

            RectTransform card = CreateCard(pauseGroup.transform, "PB_UI_PauseCard", new Vector2(660f, 700f), Vector2.zero, 4);
            CreateLine(pauseGroup.transform, "PB_UI_PauseTopNeon", new Vector2(0f, 325f), new Vector2(500f, 4f), NeonOrange, 5);
            CreateLine(pauseGroup.transform, "PB_UI_PauseCyanLine", new Vector2(0f, 313f), new Vector2(320f, 2f), NeonCyan, 6);
            CreateTmp(pauseGroup.transform, "PB_UI_PauseSubtitle", "PROJECT BEAT", new Vector2(0f, 246f), new Vector2(500f, 34f), 18f, NeonCyan, 7, FontStyles.Bold, 5f);
            CreateTmp(pauseGroup.transform, "PB_UI_PauseTitle", "PAUSA", new Vector2(0f, 200f), new Vector2(500f, 64f), 48f, NeonYellow, 8, FontStyles.Bold, 7f);

            menuButtonImages = new Image[OptionCount];
            menuGlowImages = new Image[OptionCount];
            menuButtonGroups = new CanvasGroup[OptionCount];
            menuButtonRects = new RectTransform[OptionCount];

            float startY = 126f;
            for (int i = 0; i < OptionCount; i++)
            {
                RectTransform button = CreateButtonShell(pauseGroup.transform, "PB_UI_MenuButton_" + i, new Vector2(0f, startY - i * 58f), 9 + i);
                menuButtonRects[i] = button;
                menuButtonImages[i] = button.GetComponent<Image>();
                menuGlowImages[i] = CreateFloatingGlow(button, "PB_UI_SelectedGlow_" + i, Vector2.zero, new Vector2(470f, 66f), new Color(1f, 0.75f, 0.04f, 0f), 0);
                menuButtonGroups[i] = button.gameObject.AddComponent<CanvasGroup>();
                AddMouseEventsToPauseButton(button.gameObject, i);

                if (menuLabels[i] == null)
                {
                    menuLabels[i] = CreateTmp(button, "PB_UI_MenuLabel_" + i, OptionNames[i], Vector2.zero, new Vector2(470f, 60f), 24f, TextNormal, 1, FontStyles.Normal, 3f);
                    if (i == 3) settingsMenuLabel = menuLabels[i];
                }

                RectTransform labelRT = menuLabels[i].GetComponent<RectTransform>();
                labelRT.SetParent(button, false);
                labelRT.anchorMin = Vector2.zero;
                labelRT.anchorMax = Vector2.one;
                labelRT.offsetMin = Vector2.zero;
                labelRT.offsetMax = Vector2.zero;
                menuLabels[i].transform.SetAsLastSibling();
            }

            CreateTmp(pauseGroup.transform, "PB_UI_PauseHint", "<color=#00F1FF>[W/S]</color> Navegar     <color=#FFF000>[ENTER]</color> Confirmar     <color=#FF6A00>[ESC]</color> Cerrar", new Vector2(0f, -318f), new Vector2(560f, 28f), 17f, TextNormal, 20, FontStyles.Normal, 1.5f);
            RefreshLabels();
        }


        private void BuildSettingsPanel()
        {
            if (pauseGroup == null || settingsGroup != null) return;

            GameObject groupGO = new GameObject("PB_UI_SettingsGroup", typeof(RectTransform));
            groupGO.transform.SetParent(pauseGroup.transform, false);
            groupGO.transform.SetAsLastSibling();
            settingsGroup = groupGO.AddComponent<CanvasGroup>();
            RectTransform grt = groupGO.GetComponent<RectTransform>();
            grt.anchorMin = Vector2.zero;
            grt.anchorMax = Vector2.one;
            grt.offsetMin = Vector2.zero;
            grt.offsetMax = Vector2.zero;

            CreateFullScreenImage(groupGO.transform, "PB_Settings_Dim", new Color(0f, 0f, 0f, 0.38f), 0);
            CreateCard(groupGO.transform, "PB_Settings_Card", new Vector2(1500f, 900f), Vector2.zero, 1);
            CreateLine(groupGO.transform, "PB_Settings_TopLine", new Vector2(0f, 425f), new Vector2(1260f, 4f), NeonCyan, 2);
            CreateLine(groupGO.transform, "PB_Settings_BottomLine", new Vector2(0f, -420f), new Vector2(760f, 3f), NeonOrange, 3);

            settingsTitleText = CreateTmp(groupGO.transform, "PB_Settings_Title", "CONFIGURACION", new Vector2(0f, 374f), new Vector2(900f, 62f), 44f, NeonYellow, 4, FontStyles.Bold, 5f);
            settingsBodyText = CreateTmp(groupGO.transform, "PB_Settings_Body", "", new Vector2(0f, 0f), new Vector2(1f, 1f), 1f, TextNormal, 5, FontStyles.Normal, 0f);
            settingsBodyText.enabled = false;

            controlsHeaderText = CreateTmp(groupGO.transform, "PB_Settings_ControlsHeader", "", new Vector2(-330f, 315f), new Vector2(620f, 34f), 23f, NeonCyan, 6, FontStyles.Bold, 2f);
            controlsDescriptionText = CreateTmp(groupGO.transform, "PB_Settings_ControlsDescription", "", new Vector2(-322f, 286f), new Vector2(720f, 28f), 15f, TextDim, 7, FontStyles.Normal, 0f);
            graphicsHeaderText = CreateTmp(groupGO.transform, "PB_Settings_GraphicsHeader", "", new Vector2(-330f, 235f), new Vector2(620f, 36f), 24f, NeonCyan, 8, FontStyles.Bold, 2f);
            brightnessLabelText = CreateTmp(groupGO.transform, "PB_Settings_BrightnessLabel", "", new Vector2(-330f, 176f), new Vector2(620f, 52f), 20f, TextNormal, 9, FontStyles.Normal, 0f);
            resolutionLabelText = CreateTmp(groupGO.transform, "PB_Settings_ResolutionLabel", "", new Vector2(-330f, 112f), new Vector2(620f, 52f), 20f, TextNormal, 10, FontStyles.Normal, 0f);
            displayModeLabelText = CreateTmp(groupGO.transform, "PB_Settings_DisplayModeLabel", "", new Vector2(-330f, 48f), new Vector2(620f, 52f), 20f, TextNormal, 11, FontStyles.Normal, 0f);
            effectsLabelText = CreateTmp(groupGO.transform, "PB_Settings_EffectsLabel", "", new Vector2(-330f, -16f), new Vector2(660f, 52f), 20f, TextNormal, 12, FontStyles.Normal, 0f);
            sensitivityLabelText = CreateTmp(groupGO.transform, "PB_Settings_SensitivityLabel", "", new Vector2(-330f, -80f), new Vector2(660f, 52f), 20f, TextNormal, 13, FontStyles.Normal, 0f);
            soundHeaderText = CreateTmp(groupGO.transform, "PB_Settings_SoundHeader", "", new Vector2(-330f, -170f), new Vector2(620f, 36f), 24f, NeonCyan, 14, FontStyles.Bold, 2f);
            volumeLabelText = CreateTmp(groupGO.transform, "PB_Settings_VolumeLabel", "", new Vector2(-330f, -230f), new Vector2(620f, 52f), 20f, TextNormal, 15, FontStyles.Normal, 0f);
            settingsBackText = CreateTmp(groupGO.transform, "PB_Settings_Back", "", new Vector2(-330f, -320f), new Vector2(620f, 50f), 25f, NeonOrange, 16, FontStyles.Bold, 1.5f);

            TMP_Text[] leftLabels = { controlsHeaderText, controlsDescriptionText, graphicsHeaderText, brightnessLabelText, resolutionLabelText, displayModeLabelText, effectsLabelText, sensitivityLabelText, soundHeaderText, volumeLabelText, settingsBackText };
            foreach (TMP_Text label in leftLabels)
            {
                if (label == null) continue;
                label.alignment = TextAlignmentOptions.Left;
                label.enableWordWrapping = true;
            }

            CreateLine(groupGO.transform, "PB_Settings_Separator_Top", new Vector2(0f, 258f), new Vector2(1180f, 2f), new Color(0f, 0.92f, 1f, 0.16f), 17);
            CreateLine(groupGO.transform, "PB_Settings_Separator_Mid", new Vector2(0f, -130f), new Vector2(1180f, 2f), new Color(0f, 0.92f, 1f, 0.16f), 18);
            CreateLine(groupGO.transform, "PB_Settings_Separator_Bottom", new Vector2(0f, -282f), new Vector2(1180f, 2f), new Color(0f, 0.92f, 1f, 0.16f), 19);

            CreateSliderVisual(groupGO.transform, "PB_Settings_BrightnessSlider", new Vector2(260f, 176f), out brightnessSliderFill, out brightnessSliderGlow, out brightnessSliderHandle);
            brightnessValueText = CreateTmp(groupGO.transform, "PB_Settings_BrightnessValue", "100%", new Vector2(610f, 176f), new Vector2(210f, 34f), 22f, NeonYellow, 20, FontStyles.Bold, 1.0f);
            brightnessValueText.alignment = TextAlignmentOptions.Right;

            resolutionValueText = CreateTmp(groupGO.transform, "PB_Settings_ResolutionValue", "1920x1080", new Vector2(570f, 112f), new Vector2(300f, 34f), 22f, NeonYellow, 21, FontStyles.Bold, 1.0f);
            resolutionValueText.alignment = TextAlignmentOptions.Right;

            displayModeValueText = CreateTmp(groupGO.transform, "PB_Settings_DisplayModeValue", "PANTALLA COMPLETA", new Vector2(540f, 48f), new Vector2(420f, 34f), 22f, NeonYellow, 22, FontStyles.Bold, 0.5f);
            displayModeValueText.alignment = TextAlignmentOptions.Right;

            CreateSliderVisual(groupGO.transform, "PB_Settings_EffectsSlider", new Vector2(260f, -16f), out effectsSliderFill, out effectsSliderGlow, out effectsSliderHandle);
            effectsValueText = CreateTmp(groupGO.transform, "PB_Settings_EffectsValue", "MEDIO", new Vector2(610f, -16f), new Vector2(210f, 34f), 22f, NeonYellow, 23, FontStyles.Bold, 1.0f);
            effectsValueText.alignment = TextAlignmentOptions.Right;

            sensitivityValueText = CreateTmp(groupGO.transform, "PB_Settings_SensitivityValue", "OFF", new Vector2(610f, -80f), new Vector2(210f, 34f), 22f, NeonYellow, 24, FontStyles.Bold, 1.0f);
            sensitivityValueText.alignment = TextAlignmentOptions.Right;

            CreateSliderVisual(groupGO.transform, "PB_Settings_VolumeSlider", new Vector2(260f, -230f), out volumeSliderFill, out volumeSliderGlow, out volumeSliderHandle);
            volumeValueText = CreateTmp(groupGO.transform, "PB_Settings_VolumeValue", "100%", new Vector2(610f, -230f), new Vector2(210f, 34f), 22f, NeonYellow, 25, FontStyles.Bold, 1.0f);
            volumeValueText.alignment = TextAlignmentOptions.Right;

            settingsHintText = CreateTmp(groupGO.transform, "PB_Settings_Hint", "", new Vector2(0f, -374f), new Vector2(1040f, 42f), 17f, TextDim, 26, FontStyles.Normal, 1.0f);

            // Avance 47: zonas de mouse transparentes sobre cada opcion.
            // Mantienen intacta la navegacion por teclado y solo agregan hover/click/drag.
            AddSettingsMouseZone(groupGO.transform, "PB_Settings_Mouse_Controls", new Vector2(-330f, 300f), new Vector2(760f, 62f), 0);
            AddSettingsMouseZone(groupGO.transform, "PB_Settings_Mouse_BrightnessRow", new Vector2(-330f, 176f), new Vector2(760f, 58f), 1);
            AddSettingsSliderMouseZone(groupGO.transform, "PB_Settings_Mouse_BrightnessSlider", new Vector2(260f, 176f), new Vector2(560f, 48f), 1);
            AddSettingsMouseZone(groupGO.transform, "PB_Settings_Mouse_Resolution", new Vector2(-330f, 112f), new Vector2(760f, 58f), 2, true);
            AddSettingsMouseZone(groupGO.transform, "PB_Settings_Mouse_DisplayMode", new Vector2(-330f, 48f), new Vector2(760f, 58f), 3, true);
            AddSettingsMouseZone(groupGO.transform, "PB_Settings_Mouse_EffectsRow", new Vector2(-330f, -16f), new Vector2(760f, 58f), 4);
            AddSettingsSliderMouseZone(groupGO.transform, "PB_Settings_Mouse_EffectsSlider", new Vector2(260f, -16f), new Vector2(560f, 48f), 4);
            AddSettingsMouseZone(groupGO.transform, "PB_Settings_Mouse_Sensitivity", new Vector2(-330f, -80f), new Vector2(760f, 58f), 5, true);
            AddSettingsMouseZone(groupGO.transform, "PB_Settings_Mouse_VolumeRow", new Vector2(-330f, -230f), new Vector2(760f, 58f), 6);
            AddSettingsSliderMouseZone(groupGO.transform, "PB_Settings_Mouse_VolumeSlider", new Vector2(260f, -230f), new Vector2(560f, 48f), 6);
            AddSettingsMouseZone(groupGO.transform, "PB_Settings_Mouse_Back", new Vector2(-330f, -320f), new Vector2(760f, 58f), 7, true);

            RefreshSettingsPanel();
        }

        private void BuildCreditsPanel()
        {
            if (pauseGroup == null || creditsGroup != null) return;

            GameObject groupGO = new GameObject("PB_UI_CreditsGroup", typeof(RectTransform));
            groupGO.transform.SetParent(pauseGroup.transform, false);
            groupGO.transform.SetAsLastSibling();
            creditsGroup = groupGO.AddComponent<CanvasGroup>();

            RectTransform grt = groupGO.GetComponent<RectTransform>();
            grt.anchorMin = Vector2.zero;
            grt.anchorMax = Vector2.one;
            grt.offsetMin = Vector2.zero;
            grt.offsetMax = Vector2.zero;

            CreateFullScreenImage(groupGO.transform, "PB_Credits_Dim", new Color(0f, 0f, 0f, 0.42f), 0);
            CreateFloatingGlow(groupGO.transform, "PB_Credits_Glow_Cyan", new Vector2(-180f, 180f), new Vector2(620f, 130f), new Color(0f, 0.92f, 1f, 0.10f), 1);
            CreateFloatingGlow(groupGO.transform, "PB_Credits_Glow_Orange", new Vector2(210f, -210f), new Vector2(520f, 120f), new Color(1f, 0.42f, 0.02f, 0.10f), 2);
            CreateCard(groupGO.transform, "PB_Credits_Card", new Vector2(760f, 610f), Vector2.zero, 3);
            CreateLine(groupGO.transform, "PB_Credits_TopLine", new Vector2(0f, 292f), new Vector2(610f, 4f), NeonCyan, 4);
            CreateLine(groupGO.transform, "PB_Credits_BottomLine", new Vector2(0f, -292f), new Vector2(420f, 3f), NeonOrange, 5);

            creditsTitleText = CreateTmp(groupGO.transform, "PB_Credits_Title", "CREDITOS", new Vector2(0f, 238f), new Vector2(660f, 54f), 36f, NeonYellow, 6, FontStyles.Bold, 5f);
            creditsBodyText = CreateTmp(groupGO.transform, "PB_Credits_Body", "", new Vector2(0f, 15f), new Vector2(650f, 360f), 22f, TextNormal, 7, FontStyles.Normal, 1.1f);
            creditsBodyText.alignment = TextAlignmentOptions.Top;
            creditsBodyText.enableWordWrapping = true;
            creditsHintText = CreateTmp(groupGO.transform, "PB_Credits_Hint", "", new Vector2(0f, -252f), new Vector2(690f, 44f), 17f, TextDim, 8, FontStyles.Normal, 1.0f);

            RefreshCreditsPanel();
        }

        private void RefreshCreditsPanel()
        {
            if (creditsBodyText != null)
            {
                creditsBodyText.text =
                    "<size=20><color=#00F1FF><b>PROJECT BEAT v3.0+</b></color></size>\n\n" +
                    "<color=#FFF000><b>Desarrolladores</b></color>\n" +
                    "Denzel Alvarez\n" +
                    "Alonso Leiva\n\n" +
                    "<color=#FFF000><b>Curso</b></color>\n" +
                    "Programacion de Videojuegos\n\n" +
                    "<color=#FFF000><b>Institucion</b></color>\n" +
                    "Santo Tomas Iquique\n\n" +
                    "<color=#FFF000><b>Tecnologias</b></color>\n" +
                    "Unity  |  C#  |  TextMeshPro  |  Unity UI\n\n" +
                    "<color=#FFF000><b>Inspiracion</b></color>\n" +
                    "osu!mania  |  Fortnite Festival  |  Guitar Hero\n\n" +
                    "<i><color=#00F1FF>\"Feel the rhythm.\"</color></i>";
            }

            if (creditsHintText != null)
                creditsHintText.text = "<color=#FFF000>[ENTER]</color> Volver    <color=#FF6A00>[ESC]</color> Volver";
        }

        private void ShowCreditsGroup(bool show, bool instant = false)
        {
            if (creditsGroup == null) return;
            creditsGroup.alpha = show ? 1f : 0f;
            creditsGroup.interactable = show;
            creditsGroup.blocksRaycasts = show;
            if (show) creditsGroup.transform.SetAsLastSibling();
        }

        private void BuildMainMenuLoadingOverlay()
        {
            if (mainMenuLoadingGroup != null) return;

            GameObject canvasGO = new GameObject("PB_MainMenuLoadingCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 6000;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            GameObject root = new GameObject("PB_MainMenuLoading", typeof(RectTransform));
            root.transform.SetParent(canvasGO.transform, false);
            RectTransform rootRT = root.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0.002f, 0.004f, 0.012f, 0.96f);
            mainMenuLoadingGroup = root.AddComponent<CanvasGroup>();

            GameObject textGO = new GameObject("PB_MainMenuLoadingText", typeof(RectTransform));
            textGO.transform.SetParent(root.transform, false);
            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.5f, 0.5f);
            textRT.anchorMax = new Vector2(0.5f, 0.5f);
            textRT.sizeDelta = new Vector2(700f, 120f);
            textRT.anchoredPosition = Vector2.zero;

            mainMenuLoadingText = textGO.AddComponent<TextMeshProUGUI>();
            mainMenuLoadingText.text = "CARGANDO...";
            mainMenuLoadingText.alignment = TextAlignmentOptions.Center;
            mainMenuLoadingText.fontSize = 42f;
            mainMenuLoadingText.fontStyle = FontStyles.Bold;
            mainMenuLoadingText.characterSpacing = 8f;
            mainMenuLoadingText.color = NeonYellow;
            mainMenuLoadingText.raycastTarget = false;
        }

        private void ShowMainMenuLoading(bool show)
        {
            if (mainMenuLoadingGroup == null) return;
            mainMenuLoadingGroup.alpha = show ? 1f : 0f;
            mainMenuLoadingGroup.interactable = show;
            mainMenuLoadingGroup.blocksRaycasts = show;
            if (show) mainMenuLoadingGroup.transform.SetAsLastSibling();
        }

        private void BuildBrightnessOverlay()
        {
            if (brightnessOverlay != null) return;
            GameObject canvasGO = new GameObject("PB_BrightnessOverlayCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            GameObject overlayGO = new GameObject("PB_BrightnessOverlay", typeof(RectTransform));
            overlayGO.transform.SetParent(canvasGO.transform, false);
            brightnessOverlay = overlayGO.AddComponent<Image>();
            brightnessOverlay.raycastTarget = false;
            RectTransform rt = overlayGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void ShowSettingsGroup(bool show, bool instant = false)
        {
            if (settingsGroup == null) return;
            settingsGroup.alpha = show ? 1f : 0f;
            settingsGroup.interactable = show;
            settingsGroup.blocksRaycasts = show;
            if (show) settingsGroup.transform.SetAsLastSibling();
        }

        private void LoadSettingsPrefs()
        {
            brightness = PlayerPrefs.GetFloat(BrightnessPrefsKey, 1f);
            masterVolume = PlayerPrefs.GetFloat(MasterVolumePrefsKey, 1f);
            resolutionIndex = Mathf.Clamp(PlayerPrefs.GetInt(ResolutionPrefsKey, 3), 0, ResolutionOptions.Length - 1);
            displayModeIndex = Mathf.Clamp(PlayerPrefs.GetInt(DisplayModePrefsKey, 0), 0, DisplayModeNames.Length - 1);
            brightness = Mathf.Clamp(brightness, 0.55f, 1.35f);
            masterVolume = Mathf.Clamp01(masterVolume);
            ApplyDisplaySettings();
        }

        private void ApplyVisualAudioSettings()
        {
            AudioListener.volume = masterVolume;

            if (brightnessOverlay != null)
            {
                if (brightness < 1f)
                    brightnessOverlay.color = new Color(0f, 0f, 0f, Mathf.InverseLerp(1f, 0.55f, brightness) * 0.38f);
                else if (brightness > 1f)
                    brightnessOverlay.color = new Color(1f, 1f, 1f, Mathf.InverseLerp(1f, 1.35f, brightness) * 0.16f);
                else
                    brightnessOverlay.color = new Color(0f, 0f, 0f, 0f);
            }
        }

        private void RefreshSettingsPanel()
        {
            if (settingsHintText == null) return;

            SetHeaderText(controlsHeaderText, 0, "VER CONTROLES");
            if (controlsDescriptionText != null)
                controlsDescriptionText.text = "D/F/J/K Carriles   ESC Pausa   ENTER Confirmar   F1-F4 Offset";

            SetHeaderText(graphicsHeaderText, -1, "GRAFICOS");
            SetOptionText(brightnessLabelText, 1, "Brillo General", "Ajusta la iluminacion general del juego.");
            SetOptionText(resolutionLabelText, 2, "Resolucion", "Cambia la resolucion de pantalla.");
            SetOptionText(displayModeLabelText, 3, "Modo Pantalla", "Alterna pantalla completa, ventana o sin bordes.");
            SetOptionText(effectsLabelText, 4, "Intensidad Efectos Visuales", "Controla glow, flashes, particulas y transiciones.");
            SetOptionText(sensitivityLabelText, 5, "Modo Sensibilidad Visual", "Reduce destellos rapidos para mayor comodidad visual.");
            SetHeaderText(soundHeaderText, -1, "SONIDO");
            SetOptionText(volumeLabelText, 6, "Volumen General", "Ajusta el volumen principal del juego.");

            if (settingsBackText != null)
            {
                settingsBackText.text = selectedSettingsOption == 7
                    ? "<color=#FFF000><b>> VOLVER</b></color>"
                    : "<color=#FF6A00><b>  VOLVER</b></color>";
            }

            UpdateSliderVisual(brightnessSliderFill, brightnessSliderGlow, brightnessSliderHandle, brightnessValueText, brightness, 0.55f, 1.35f, selectedSettingsOption == 1);
            UpdateSliderVisual(effectsSliderFill, effectsSliderGlow, effectsSliderHandle, effectsValueText, VisualAccessibilitySettings.IntensityIndex, 0f, 4f, selectedSettingsOption == 4);
            UpdateSliderVisual(volumeSliderFill, volumeSliderGlow, volumeSliderHandle, volumeValueText, masterVolume, 0f, 1f, selectedSettingsOption == 6);

            if (resolutionValueText != null)
            {
                Vector2Int res = ResolutionOptions[Mathf.Clamp(resolutionIndex, 0, ResolutionOptions.Length - 1)];
                resolutionValueText.text = (selectedSettingsOption == 2 ? "<color=#00F1FF><</color> " : "") + res.x + "x" + res.y + (selectedSettingsOption == 2 ? " <color=#00F1FF>></color>" : "");
                resolutionValueText.color = selectedSettingsOption == 2 ? NeonYellow : TextNormal;
            }

            if (displayModeValueText != null)
            {
                displayModeValueText.text = (selectedSettingsOption == 3 ? "<color=#00F1FF><</color> " : "") + DisplayModeNames[Mathf.Clamp(displayModeIndex, 0, DisplayModeNames.Length - 1)] + (selectedSettingsOption == 3 ? " <color=#00F1FF>></color>" : "");
                displayModeValueText.color = selectedSettingsOption == 3 ? NeonYellow : TextNormal;
            }

            if (effectsValueText != null)
            {
                effectsValueText.text = VisualAccessibilitySettings.IntensityName;
                effectsValueText.color = selectedSettingsOption == 4 ? NeonYellow : TextNormal;
            }

            if (sensitivityValueText != null)
            {
                sensitivityValueText.text = VisualAccessibilitySettings.SensitivityMode ? "ON" : "OFF";
                sensitivityValueText.color = selectedSettingsOption == 5 ? NeonYellow : TextNormal;
            }

            settingsHintText.text = "<color=#00F1FF>[W/S]</color> Seleccionar    <color=#FFF000>[A/D]</color> Ajustar / Cambiar    <color=#FF6A00>[ESC]</color> Volver    <color=#BFB6FF>[MOUSE]</color> Click / Arrastrar";
        }

        private void SetHeaderText(TMP_Text text, int optionIndex, string title)
        {
            if (text == null) return;
            bool selected = optionIndex >= 0 && selectedSettingsOption == optionIndex;
            text.text = selected ? "<color=#FFF000><b>> " + title + "</b></color>" : "<color=#00F1FF><b>  " + title + "</b></color>";
            text.color = selected ? NeonYellow : NeonCyan;
        }

        private void SetOptionText(TMP_Text text, int optionIndex, string title, string description)
        {
            if (text == null) return;
            bool selected = selectedSettingsOption == optionIndex;
            string marker = selected ? "<color=#FFF000><b>> </b></color>" : "  ";
            string titleColor = selected ? "#FFF000" : "#00F1FF";
            text.text = marker + "<color=" + titleColor + "><b>" + title + "</b></color>\n" +
                        "<size=15><color=#DDEEFF>" + description + "</color></size>";
            text.color = selected ? NeonYellow : TextNormal;
        }

        private int WrapIndex(int value, int count)
        {
            if (count <= 0) return 0;
            value %= count;
            if (value < 0) value += count;
            return value;
        }

        private void ApplyDisplaySettings()
        {
            resolutionIndex = Mathf.Clamp(resolutionIndex, 0, ResolutionOptions.Length - 1);
            displayModeIndex = Mathf.Clamp(displayModeIndex, 0, DisplayModeNames.Length - 1);
            Vector2Int resolution = ResolutionOptions[resolutionIndex];
            FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;

            switch (displayModeIndex)
            {
                case 0:
                    mode = FullScreenMode.ExclusiveFullScreen;
                    break;
                case 1:
                    mode = FullScreenMode.Windowed;
                    break;
                case 2:
                    mode = FullScreenMode.FullScreenWindow;
                    break;
            }

            if (Screen.width != resolution.x || Screen.height != resolution.y || Screen.fullScreenMode != mode)
                Screen.SetResolution(resolution.x, resolution.y, mode);
        }

        private string SectionTitle(int optionIndex, string title)
        {
            if (selectedSettingsOption == optionIndex)
                return "<size=19><color=#FFF000><b>> " + title + "</b></color></size>";

            return "<size=18><color=#00F1FF><b>  " + title + "</b></color></size>";
        }

        private string MakeCleanBar(float value, float min, float max)
        {
            int total = 16;
            int marker = Mathf.RoundToInt(Mathf.InverseLerp(min, max, value) * total);
            marker = Mathf.Clamp(marker, 0, total);

            string left = marker > 0 ? new string('=', marker) : string.Empty;
            string right = marker < total ? new string('-', total - marker) : string.Empty;
            return "<color=#00F1FF>[" + left + "</color><color=#FFF000>o</color><color=#40546A>" + right + "]</color>";
        }

        private void BuildCommercialLevelSelect()
        {
            if (levelSelectGroup == null) return;
            DisableRootImage(levelSelectGroup);
            RemoveOldVisuals(levelSelectGroup.transform);
            CreateFullScreenImage(levelSelectGroup.transform, "PB_UI_LevelDimBlur", DeepDim, 0);
            CreateFullScreenImage(levelSelectGroup.transform, "PB_UI_LevelColorWash", new Color(0.01f, 0.02f, 0.08f, 0.36f), 1);
            CreateFloatingGlow(levelSelectGroup.transform, "PB_UI_LevelGlow", new Vector2(0f, 0f), new Vector2(760f, 150f), new Color(0f, 0.9f, 1f, 0.12f), 2);

            CreateCard(levelSelectGroup.transform, "PB_UI_LevelCard", new Vector2(760f, 430f), Vector2.zero, 3);
            CreateLine(levelSelectGroup.transform, "PB_UI_LevelTopNeon", new Vector2(0f, 210f), new Vector2(640f, 4f), NeonOrange, 4);
            CreateLine(levelSelectGroup.transform, "PB_UI_LevelBottomNeon", new Vector2(0f, -210f), new Vector2(360f, 3f), NeonCyan, 5);
            CreateTmp(levelSelectGroup.transform, "PB_UI_LevelBadge", "ARCADE SELECT", new Vector2(0f, 155f), new Vector2(500f, 30f), 18f, NeonCyan, 6, FontStyles.Bold, 4f);

            if (levelNameText != null)
            {
                RectTransform rt = levelNameText.GetComponent<RectTransform>();
                rt.SetParent(levelSelectGroup.transform, false);
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 42f);
                rt.sizeDelta = new Vector2(720f, 170f);
                levelNameText.transform.SetSiblingIndex(8);
            }

            if (levelArtistText != null)
            {
                RectTransform rt = levelArtistText.GetComponent<RectTransform>();
                rt.SetParent(levelSelectGroup.transform, false);
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, -70f);
                rt.sizeDelta = new Vector2(650f, 42f);
                levelArtistText.transform.SetSiblingIndex(9);
            }

            if (levelHintText != null)
            {
                RectTransform rt = levelHintText.GetComponent<RectTransform>();
                rt.SetParent(levelSelectGroup.transform, false);
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, -160f);
                rt.sizeDelta = new Vector2(720f, 36f);
                levelHintText.transform.SetSiblingIndex(10);
            }

            // Avance 47: flechas, iniciar y volver clickeables en el selector.
            CreateLevelMouseButton(levelSelectGroup.transform, "PB_UI_LevelMouse_Left", new Vector2(-290f, 42f), new Vector2(120f, 140f), () => ChangeLevelWithMouse(-1));
            CreateLevelMouseButton(levelSelectGroup.transform, "PB_UI_LevelMouse_Right", new Vector2(290f, 42f), new Vector2(120f, 140f), () => ChangeLevelWithMouse(1));
            CreateLevelMouseButton(levelSelectGroup.transform, "PB_UI_LevelMouse_Start", new Vector2(0f, -70f), new Vector2(660f, 72f), ConfirmLevelSelect);
            TMP_Text backText = CreateTmp(levelSelectGroup.transform, "PB_UI_LevelBackButton", "<color=#FF6A00><b>VOLVER</b></color>", new Vector2(0f, -196f), new Vector2(240f, 34f), 19f, NeonOrange, 11, FontStyles.Bold, 1.0f);
            CreateLevelMouseButton(levelSelectGroup.transform, "PB_UI_LevelMouse_Back", new Vector2(0f, -196f), new Vector2(260f, 44f), ExitLevelSelect);

            RefreshLevelSelectLabels();
        }

        private void RemoveOldVisuals(Transform root)
        {
            HashSet<Transform> keep = new HashSet<Transform>();
            AddKeep(keep, resumeLabel);
            AddKeep(keep, selectLevelLabel);
            AddKeep(keep, restartLabel);
            AddKeep(keep, quitLabel);
            AddKeep(keep, levelNameText);
            AddKeep(keep, levelArtistText);
            AddKeep(keep, levelHintText);

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                if (keep.Contains(child))
                    continue;

                // Limpieza agresiva: elimina restos visuales anteriores del constructor de escena
                // (PauseTitle, PHint, Border, TopAccent, cards viejas, etc.) para evitar textos duplicados.
                Destroy(child.gameObject);
            }
        }

        private void AddKeep(HashSet<Transform> keep, TMP_Text text)
        {
            if (text != null) keep.Add(text.transform);
        }

        private void DisableRootImage(CanvasGroup group)
        {
            if (group == null) return;
            Image image = group.GetComponent<Image>();
            if (image != null)
                image.enabled = false;
        }

        private RectTransform CreateCard(Transform parent, string name, Vector2 size, Vector2 pos, int sibling)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(Mathf.Min(sibling, parent.childCount - 1));
            Image img = go.AddComponent<Image>();
            img.raycastTarget = false;
            img.sprite = roundedPanelSprite;
            img.type = Image.Type.Sliced;
            img.color = Glass;
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.94f, 1f, 0.28f);
            outline.effectDistance = new Vector2(2f, -2f);
            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
            shadow.effectDistance = new Vector2(0f, -12f);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            GameObject shine = new GameObject(name + "_InnerShine");
            shine.transform.SetParent(go.transform, false);
            Image shineImg = shine.AddComponent<Image>();
            shineImg.raycastTarget = false;
            shineImg.sprite = roundedPanelSprite;
            shineImg.type = Image.Type.Sliced;
            shineImg.color = GlassLight;
            RectTransform srt = shine.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.05f, 0.55f);
            srt.anchorMax = new Vector2(0.95f, 0.95f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;
            return rt;
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private RectTransform CreateTransparentHitZone(Transform parent, string name, Vector2 pos, Vector2 size, int sibling = 80)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(Mathf.Min(sibling, parent.childCount - 1));
            Image img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0f);
            img.raycastTarget = true;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        private void AddTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(action);
            trigger.triggers.Add(entry);
        }

        private void AddSettingsMouseZone(Transform parent, string name, Vector2 pos, Vector2 size, int optionIndex, bool clickActs = false)
        {
            RectTransform hit = CreateTransparentHitZone(parent, name, pos, size, 90 + optionIndex);
            EventTrigger trigger = hit.gameObject.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();

            AddTrigger(trigger, EventTriggerType.PointerEnter, (_) =>
            {
                if (!isPaused || !isInSettings || isReturningToInitialMenu) return;
                selectedSettingsOption = optionIndex;
                RefreshSettingsPanel();
            });

            AddTrigger(trigger, EventTriggerType.PointerClick, (_) =>
            {
                if (!isPaused || !isInSettings || isReturningToInitialMenu) return;
                selectedSettingsOption = optionIndex;
                if (clickActs)
                {
                    if (optionIndex == 2 || optionIndex == 3)
                        AdjustSelectedSetting(0.05f);
                    else if (optionIndex == 5)
                    {
                        VisualAccessibilitySettings.ToggleSensitivityMode();
                        ApplyVisualAudioSettings();
                        RefreshSettingsPanel();
                    }
                    else if (optionIndex == 7)
                        ExitSettings();
                }
                else
                    RefreshSettingsPanel();
            });
        }

        private void AddSettingsSliderMouseZone(Transform parent, string name, Vector2 pos, Vector2 size, int optionIndex)
        {
            RectTransform hit = CreateTransparentHitZone(parent, name, pos, size, 110 + optionIndex);
            EventTrigger trigger = hit.gameObject.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();

            UnityEngine.Events.UnityAction<BaseEventData> apply = (data) =>
            {
                if (!isPaused || !isInSettings || isReturningToInitialMenu) return;
                selectedSettingsOption = optionIndex;
                PointerEventData pointer = data as PointerEventData;
                if (pointer != null)
                    ApplySliderFromMouse(optionIndex, hit, pointer);
            };

            AddTrigger(trigger, EventTriggerType.PointerEnter, (_) =>
            {
                if (!isPaused || !isInSettings || isReturningToInitialMenu) return;
                selectedSettingsOption = optionIndex;
                RefreshSettingsPanel();
            });
            AddTrigger(trigger, EventTriggerType.PointerDown, apply);
            AddTrigger(trigger, EventTriggerType.Drag, apply);
            AddTrigger(trigger, EventTriggerType.PointerClick, apply);
        }

        private void ApplySliderFromMouse(int optionIndex, RectTransform zone, PointerEventData eventData)
        {
            Vector2 localPoint;
            Camera cam = eventData.pressEventCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(zone, eventData.position, cam, out localPoint)) return;

            float t = Mathf.Clamp01((localPoint.x / Mathf.Max(1f, zone.rect.width)) + 0.5f);

            switch (optionIndex)
            {
                case 1:
                    brightness = Mathf.Lerp(0.55f, 1.35f, t);
                    PlayerPrefs.SetFloat(BrightnessPrefsKey, brightness);
                    break;
                case 4:
                    VisualAccessibilitySettings.SetIntensityIndex(Mathf.RoundToInt(t * 4f));
                    break;
                case 6:
                    masterVolume = t;
                    PlayerPrefs.SetFloat(MasterVolumePrefsKey, masterVolume);
                    break;
            }

            PlayerPrefs.Save();
            ApplyVisualAudioSettings();
            RefreshSettingsPanel();
        }

        private void CreateLevelMouseButton(Transform parent, string name, Vector2 pos, Vector2 size, System.Action clickAction)
        {
            RectTransform hit = CreateTransparentHitZone(parent, name, pos, size, 90);
            EventTrigger trigger = hit.gameObject.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();

            AddTrigger(trigger, EventTriggerType.PointerEnter, (_) =>
            {
                if (!isPaused || !isInLevelSelect || isReturningToInitialMenu) return;
                PopText(levelNameText, 1.035f);
            });

            AddTrigger(trigger, EventTriggerType.PointerClick, (_) =>
            {
                if (!isPaused || !isInLevelSelect || isReturningToInitialMenu) return;
                if (clickAction != null) clickAction.Invoke();
            });
        }

        private void ChangeLevelWithMouse(int direction)
        {
            LevelManager lm = LevelManager.Instance;
            if (lm == null) return;
            if (direction < 0) lm.PreviousLevel();
            else lm.NextLevel();
            RefreshLevelSelectLabels();
            PopText(levelNameText, 1.08f);
        }

        private void AddMouseEventsToPauseButton(GameObject target, int optionIndex)
        {
            if (target == null) return;
            EventTrigger trigger = target.GetComponent<EventTrigger>();
            if (trigger == null) trigger = target.AddComponent<EventTrigger>();
            if (trigger.triggers == null) trigger.triggers = new List<EventTrigger.Entry>();

            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((_) =>
            {
                if (!isPaused || isInLevelSelect || isInSettings || isInCredits || isReturningToInitialMenu) return;
                selectedOption = optionIndex;
                RefreshLabels();
            });
            trigger.triggers.Add(enter);

            EventTrigger.Entry click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            click.callback.AddListener((_) =>
            {
                if (!isPaused || isInLevelSelect || isInSettings || isInCredits || isReturningToInitialMenu) return;
                selectedOption = optionIndex;
                RefreshLabels();
                ConfirmOption();
            });
            trigger.triggers.Add(click);
        }

        private RectTransform CreateButtonShell(Transform parent, string name, Vector2 pos, int sibling)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(Mathf.Min(sibling, parent.childCount - 1));
            Image img = go.AddComponent<Image>();
            img.raycastTarget = true;
            img.sprite = roundedButtonSprite;
            img.type = Image.Type.Sliced;
            img.color = new Color(0.10f, 0.045f, 0.15f, 0.50f);
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.5f, 0f, 0.22f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(470f, 52f);
            return rt;
        }

        private void CreateFullScreenImage(Transform parent, string name, Color color, int sibling)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(Mathf.Min(sibling, parent.childCount - 1));
            Image img = go.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = color;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private Image CreateFloatingGlow(Transform parent, string name, Vector2 pos, Vector2 size, Color color, int sibling)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(Mathf.Min(sibling, parent.childCount - 1));
            Image img = go.AddComponent<Image>();
            img.raycastTarget = false;
            img.sprite = roundedPanelSprite;
            img.type = Image.Type.Sliced;
            img.color = color;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return img;
        }

        private void CreateLine(Transform parent, string name, Vector2 pos, Vector2 size, Color color, int sibling)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(Mathf.Min(sibling, parent.childCount - 1));
            Image img = go.AddComponent<Image>();
            img.raycastTarget = false;
            img.sprite = lineSprite;
            img.type = Image.Type.Sliced;
            img.color = color;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        private void CreateSliderVisual(Transform parent, string name, Vector2 pos, out Image fill, out Image glow, out RectTransform handle)
        {
            GameObject root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            root.transform.SetAsLastSibling();
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(500f, 20f);

            GameObject baseGO = new GameObject(name + "_Base", typeof(RectTransform));
            baseGO.transform.SetParent(root.transform, false);
            Image baseImage = baseGO.AddComponent<Image>();
            baseImage.raycastTarget = false;
            baseImage.sprite = lineSprite;
            baseImage.type = Image.Type.Sliced;
            baseImage.color = new Color(0.08f, 0.18f, 0.24f, 0.92f);
            RectTransform baseRT = baseGO.GetComponent<RectTransform>();
            baseRT.anchorMin = new Vector2(0f, 0.5f);
            baseRT.anchorMax = new Vector2(1f, 0.5f);
            baseRT.pivot = new Vector2(0.5f, 0.5f);
            baseRT.offsetMin = new Vector2(0f, -4f);
            baseRT.offsetMax = new Vector2(0f, 4f);

            GameObject fillGO = new GameObject(name + "_Fill", typeof(RectTransform));
            fillGO.transform.SetParent(root.transform, false);
            fill = fillGO.AddComponent<Image>();
            fill.raycastTarget = false;
            fill.sprite = lineSprite;
            fill.type = Image.Type.Sliced;
            fill.color = NeonCyan;
            RectTransform fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0f, 0.5f);
            fillRT.anchorMax = new Vector2(0.5f, 0.5f);
            fillRT.pivot = new Vector2(0f, 0.5f);
            fillRT.offsetMin = new Vector2(0f, -4f);
            fillRT.offsetMax = new Vector2(0f, 4f);

            GameObject glowGO = new GameObject(name + "_Glow", typeof(RectTransform));
            glowGO.transform.SetParent(root.transform, false);
            glow = glowGO.AddComponent<Image>();
            glow.raycastTarget = false;
            glow.sprite = roundedButtonSprite;
            glow.type = Image.Type.Sliced;
            glow.color = new Color(0f, 0.92f, 1f, 0f);
            RectTransform glowRT = glowGO.GetComponent<RectTransform>();
            glowRT.anchorMin = new Vector2(0f, 0.5f);
            glowRT.anchorMax = new Vector2(1f, 0.5f);
            glowRT.pivot = new Vector2(0.5f, 0.5f);
            glowRT.offsetMin = new Vector2(-8f, -10f);
            glowRT.offsetMax = new Vector2(8f, 10f);
            glowGO.transform.SetAsFirstSibling();

            GameObject handleGO = new GameObject(name + "_Handle", typeof(RectTransform));
            handleGO.transform.SetParent(root.transform, false);
            Image handleImage = handleGO.AddComponent<Image>();
            handleImage.raycastTarget = false;
            handleImage.sprite = roundedPanelSprite;
            handleImage.type = Image.Type.Sliced;
            handleImage.color = NeonYellow;
            Outline outline = handleGO.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.92f, 1f, 0.55f);
            outline.effectDistance = new Vector2(2f, -2f);
            handle = handleGO.GetComponent<RectTransform>();
            handle.anchorMin = new Vector2(0f, 0.5f);
            handle.anchorMax = new Vector2(0f, 0.5f);
            handle.pivot = new Vector2(0.5f, 0.5f);
            handle.sizeDelta = new Vector2(18f, 18f);
            handle.anchoredPosition = Vector2.zero;
        }

        private void UpdateSliderVisual(Image fill, Image glow, RectTransform handle, TMP_Text valueText, float value, float min, float max, bool selected)
        {
            float t = Mathf.Clamp01(Mathf.InverseLerp(min, max, value));

            if (fill != null)
            {
                RectTransform rt = fill.rectTransform;
                rt.anchorMax = new Vector2(t, 0.5f);
                fill.color = selected ? NeonYellow : NeonCyan;
            }

            if (handle != null)
            {
                handle.anchorMin = new Vector2(t, 0.5f);
                handle.anchorMax = new Vector2(t, 0.5f);
                float scale = selected ? 1.22f : 1f;
                handle.localScale = new Vector3(scale, scale, 1f);
            }

            if (glow != null)
                glow.color = selected ? new Color(1f, 0.92f, 0.02f, 0.18f) : new Color(0f, 0.92f, 1f, 0.06f);

            if (valueText != null)
            {
                valueText.text = Mathf.RoundToInt(value * 100f) + "%";
                valueText.color = selected ? NeonYellow : TextNormal;
            }
        }

        private TMP_Text CreateTmp(Transform parent, string name, string text, Vector2 pos, Vector2 size, float fontSize, Color color, int sibling, FontStyles style, float spacing)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(Mathf.Min(sibling, parent.childCount - 1));
            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.characterSpacing = spacing;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            AddShadow(tmp);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return tmp;
        }

        private Sprite MakeRoundedSprite(int width, int height, int radius, Color color)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float a = RoundedAlpha(x, y, width, height, radius);
                    tex.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * a));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        private float RoundedAlpha(int x, int y, int w, int h, int r)
        {
            int px = x < r ? r : (x > w - r - 1 ? w - r - 1 : x);
            int py = y < r ? r : (y > h - r - 1 ? h - r - 1 : y);
            float dx = x - px;
            float dy = y - py;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            return dist <= r ? 1f : 0f;
        }

        private void AddShadow(TMP_Text label)
        {
            if (label == null || label.GetComponent<Shadow>() != null) return;
            Shadow shadow = label.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.82f);
            shadow.effectDistance = new Vector2(2.5f, -2.5f);
        }

        private void PopText(TMP_Text text, float scale)
        {
            if (text == null) return;
            text.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}

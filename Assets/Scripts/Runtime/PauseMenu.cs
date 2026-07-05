using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        private const int OptionCount = 6;
        private const int SettingsOptionCount = 6;

        private const string BrightnessPrefsKey = "ProjectBeat_Brightness";
        private const string MasterVolumePrefsKey = "ProjectBeat_MasterVolume";
        private float brightness = 1f;
        private float masterVolume = 1f;

        private static readonly Color DeepDim = new Color(0.006f, 0.007f, 0.014f, 0.88f);
        private static readonly Color Glass = new Color(0.025f, 0.035f, 0.065f, 0.92f);
        private static readonly Color GlassLight = new Color(0.04f, 0.10f, 0.16f, 0.30f);
        private static readonly Color NeonYellow = new Color(1f, 0.94f, 0.04f, 1f);
        private static readonly Color NeonOrange = new Color(1f, 0.42f, 0.02f, 1f);
        private static readonly Color NeonCyan = new Color(0.0f, 0.92f, 1f, 1f);
        private static readonly Color TextNormal = new Color(0.92f, 0.90f, 1f, 1f);
        private static readonly Color TextDim = new Color(0.70f, 0.68f, 0.78f, 1f);

        private static readonly string[] OptionNames = { "CONTINUAR", "ELEGIR NIVEL", "REINICIAR", "CONFIGURACION", "CREDITOS", "SALIR" };
        private static readonly string[] OptionIcons = { "PLAY", "LEVEL", "RETRY", "SETUP", "INFO", "QUIT" };

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
            menuLabels[5] = quitLabel;
            BuildSprites();
            BuildCommercialPauseMenu();
            BuildCommercialLevelSelect();
            BuildSettingsPanel();
            BuildCreditsPanel();
            BuildBrightnessOverlay();
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
            isPaused = true;
            Time.timeScale = 0f;
            if (gameController != null) gameController.PauseAudio(true);

            selectedOption = 0;
            fadeTarget = 1f;
            ShowLevelSelectGroup(false);
            RefreshLabels();
        }

        public void ClosePause()
        {
            isPaused = false;
            isInLevelSelect = false;
            isInSettings = false;
            isInCredits = false;
            Time.timeScale = 1f;
            if (gameController != null) gameController.PauseAudio(false);

            fadeTarget = 0f;
            ShowLevelSelectGroup(false);
            ShowSettingsGroup(false);
            ShowCreditsGroup(false);
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
                case 5: QuitGame(); break;
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
                if (selectedSettingsOption == 3)
                {
                    VisualAccessibilitySettings.ToggleSensitivityMode();
                    ApplyVisualAudioSettings();
                    RefreshSettingsPanel();
                }
                else if (selectedSettingsOption == 5)
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
                    VisualAccessibilitySettings.AdjustIntensity(delta > 0f ? 1 : -1);
                    break;
                case 3:
                    VisualAccessibilitySettings.ToggleSensitivityMode();
                    break;
                case 4:
                    masterVolume = Mathf.Clamp01(masterVolume + delta);
                    PlayerPrefs.SetFloat(MasterVolumePrefsKey, masterVolume);
                    break;
                case 5:
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
            PlayerPrefs.SetInt(StartupFlowController.SkipStartupPrefsKey, 1);
            PlayerPrefs.Save();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            if (lm == null || lm.CurrentLevel == null) return;

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
                levelHintText.text = "<color=#00F1FF>[A/D]</color> Cambiar pista    <color=#FFF000>[ENTER]</color> Iniciar    <color=#FF6A00>[ESC]</color> Volver";
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
            if (pauseGroup == null) return;
            RectTransform root = pauseGroup.GetComponent<RectTransform>();
            if (root == null) return;

            DisableRootImage(pauseGroup);
            RemoveOldVisuals(pauseGroup.transform);
            CreateFullScreenImage(pauseGroup.transform, "PB_UI_DimBlur", DeepDim, 0);
            CreateFullScreenImage(pauseGroup.transform, "PB_UI_ColorWash", new Color(0.00f, 0.01f, 0.03f, 0.34f), 1);
            CreateFloatingGlow(pauseGroup.transform, "PB_UI_Glow_Cyan", new Vector2(-520f, 220f), new Vector2(520f, 110f), new Color(0f, 0.9f, 1f, 0.10f), 2);
            CreateFloatingGlow(pauseGroup.transform, "PB_UI_Glow_Orange", new Vector2(520f, -220f), new Vector2(520f, 110f), new Color(1f, 0.32f, 0f, 0.11f), 3);

            RectTransform card = CreateCard(pauseGroup.transform, "PB_UI_PauseCard", new Vector2(640f, 640f), Vector2.zero, 4);
            CreateLine(pauseGroup.transform, "PB_UI_PauseTopNeon", new Vector2(0f, 305f), new Vector2(500f, 4f), NeonOrange, 5);
            CreateLine(pauseGroup.transform, "PB_UI_PauseCyanLine", new Vector2(0f, 293f), new Vector2(320f, 2f), NeonCyan, 6);
            CreateTmp(pauseGroup.transform, "PB_UI_PauseSubtitle", "PROJECT BEAT", new Vector2(0f, 220f), new Vector2(500f, 34f), 18f, NeonCyan, 7, FontStyles.Bold, 5f);
            CreateTmp(pauseGroup.transform, "PB_UI_PauseTitle", "PAUSA", new Vector2(0f, 174f), new Vector2(500f, 64f), 48f, NeonYellow, 8, FontStyles.Bold, 7f);

            menuButtonImages = new Image[OptionCount];
            menuGlowImages = new Image[OptionCount];
            menuButtonGroups = new CanvasGroup[OptionCount];
            menuButtonRects = new RectTransform[OptionCount];

            float startY = 92f;
            for (int i = 0; i < OptionCount; i++)
            {
                RectTransform button = CreateButtonShell(pauseGroup.transform, "PB_UI_MenuButton_" + i, new Vector2(0f, startY - i * 66f), 9 + i);
                menuButtonRects[i] = button;
                menuButtonImages[i] = button.GetComponent<Image>();
                menuGlowImages[i] = CreateFloatingGlow(button, "PB_UI_SelectedGlow_" + i, Vector2.zero, new Vector2(470f, 66f), new Color(1f, 0.75f, 0.04f, 0f), 0);
                menuButtonGroups[i] = button.gameObject.AddComponent<CanvasGroup>();

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

            CreateTmp(pauseGroup.transform, "PB_UI_PauseHint", "<color=#00F1FF>[W/S]</color> Navegar     <color=#FFF000>[ENTER]</color> Confirmar     <color=#FF6A00>[ESC]</color> Cerrar", new Vector2(0f, -286f), new Vector2(540f, 28f), 17f, TextNormal, 20, FontStyles.Normal, 1.5f);
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

            CreateFullScreenImage(groupGO.transform, "PB_Settings_Dim", new Color(0f, 0f, 0f, 0.35f), 0);
            CreateCard(groupGO.transform, "PB_Settings_Card", new Vector2(760f, 610f), Vector2.zero, 1);
            CreateLine(groupGO.transform, "PB_Settings_TopLine", new Vector2(0f, 292f), new Vector2(610f, 4f), NeonCyan, 2);
            CreateLine(groupGO.transform, "PB_Settings_BottomLine", new Vector2(0f, -292f), new Vector2(420f, 3f), NeonOrange, 3);

            settingsTitleText = CreateTmp(groupGO.transform, "PB_Settings_Title", "CONFIGURACION", new Vector2(0f, 238f), new Vector2(660f, 54f), 36f, NeonYellow, 4, FontStyles.Bold, 5f);
            settingsBodyText = CreateTmp(groupGO.transform, "PB_Settings_Body", "", new Vector2(0f, -8f), new Vector2(660f, 370f), 20f, TextNormal, 5, FontStyles.Normal, 1.2f);
            settingsBodyText.alignment = TextAlignmentOptions.TopLeft;
            settingsBodyText.enableWordWrapping = true;
            settingsHintText = CreateTmp(groupGO.transform, "PB_Settings_Hint", "", new Vector2(0f, -252f), new Vector2(690f, 44f), 17f, TextDim, 6, FontStyles.Normal, 1.0f);

            CreateSliderVisual(groupGO.transform, "PB_Settings_BrightnessSlider", new Vector2(70f, 66f), out brightnessSliderFill, out brightnessSliderGlow, out brightnessSliderHandle);
            brightnessValueText = CreateTmp(groupGO.transform, "PB_Settings_BrightnessValue", "100%", new Vector2(290f, 66f), new Vector2(110f, 28f), 18f, NeonYellow, 7, FontStyles.Bold, 1.0f);
            brightnessValueText.alignment = TextAlignmentOptions.Left;

            CreateSliderVisual(groupGO.transform, "PB_Settings_EffectsSlider", new Vector2(70f, -34f), out effectsSliderFill, out effectsSliderGlow, out effectsSliderHandle);
            effectsValueText = CreateTmp(groupGO.transform, "PB_Settings_EffectsValue", "MEDIO", new Vector2(290f, -34f), new Vector2(130f, 28f), 18f, NeonYellow, 7, FontStyles.Bold, 1.0f);
            effectsValueText.alignment = TextAlignmentOptions.Left;

            sensitivityValueText = CreateTmp(groupGO.transform, "PB_Settings_SensitivityValue", "OFF", new Vector2(290f, -84f), new Vector2(130f, 28f), 18f, NeonYellow, 7, FontStyles.Bold, 1.0f);
            sensitivityValueText.alignment = TextAlignmentOptions.Left;

            CreateSliderVisual(groupGO.transform, "PB_Settings_VolumeSlider", new Vector2(70f, -166f), out volumeSliderFill, out volumeSliderGlow, out volumeSliderHandle);
            volumeValueText = CreateTmp(groupGO.transform, "PB_Settings_VolumeValue", "100%", new Vector2(290f, -166f), new Vector2(110f, 28f), 18f, NeonYellow, 7, FontStyles.Bold, 1.0f);
            volumeValueText.alignment = TextAlignmentOptions.Left;

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
            brightness = Mathf.Clamp(brightness, 0.55f, 1.35f);
            masterVolume = Mathf.Clamp01(masterVolume);
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
            if (settingsBodyText == null || settingsHintText == null) return;

            string controls = SectionTitle(0, "VER CONTROLES");
            string graphics = SectionTitle(1, "GRAFICOS");
            string effects = SectionTitle(2, "INTENSIDAD EFECTOS VISUALES");
            string sensitivity = SectionTitle(3, "MODO SENSIBILIDAD VISUAL");
            string sound = SectionTitle(4, "SONIDO");
            string back = selectedSettingsOption == 5
                ? "<size=21><color=#FFF000><b>> VOLVER</b></color></size>"
                : "<size=20><color=#FF6A00><b>  VOLVER</b></color></size>";

            settingsBodyText.text =
                controls + "\n" +
                "<size=15><color=#FFFFFF>D/F/J/K</color> Carriles   <color=#FFFFFF>ESC</color> Pausa   <color=#FFFFFF>ENTER</color> Confirmar   <color=#FFFFFF>F1-F4</color> Offset</size>\n" +
                "<color=#224455>----------------------------------------------</color>\n" +
                graphics + "\n" +
                "<size=16><color=#DDEEFF>Brillo general</color></size>\n\n" +
                effects + "\n" +
                "<size=15><color=#DDEEFF>Regula glow, flashes, partículas, fondos y transiciones.</color></size>\n\n" +
                sensitivity + "\n" +
                "<size=15><color=#DDEEFF>Reduce destellos rápidos para mayor comodidad visual.</color></size>\n" +
                "<color=#224455>----------------------------------------------</color>\n" +
                sound + "\n" +
                "<size=16><color=#DDEEFF>Volumen general</color></size>\n\n" +
                back;

            UpdateSliderVisual(brightnessSliderFill, brightnessSliderGlow, brightnessSliderHandle, brightnessValueText, brightness, 0.55f, 1.35f, selectedSettingsOption == 1);
            UpdateSliderVisual(effectsSliderFill, effectsSliderGlow, effectsSliderHandle, effectsValueText, VisualAccessibilitySettings.IntensityIndex, 0f, 4f, selectedSettingsOption == 2);
            UpdateSliderVisual(volumeSliderFill, volumeSliderGlow, volumeSliderHandle, volumeValueText, masterVolume, 0f, 1f, selectedSettingsOption == 4);

            if (effectsValueText != null)
            {
                effectsValueText.text = VisualAccessibilitySettings.IntensityName;
                effectsValueText.color = selectedSettingsOption == 2 ? NeonYellow : TextNormal;
            }

            if (sensitivityValueText != null)
            {
                sensitivityValueText.text = VisualAccessibilitySettings.SensitivityMode ? "ON" : "OFF";
                sensitivityValueText.color = selectedSettingsOption == 3 ? NeonYellow : TextNormal;
            }

            settingsHintText.text = "<color=#00F1FF>[W/S]</color> Seleccionar    <color=#FFF000>[A/D]</color> Ajustar / Cambiar    <color=#FF6A00>[ESC]</color> Volver";
        }

        private string SectionTitle(int optionIndex, string title)
        {
            if (selectedSettingsOption == optionIndex)
                return "<size=21><color=#FFF000><b>> " + title + "</b></color></size>";

            return "<size=20><color=#00F1FF><b>  " + title + "</b></color></size>";
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

        private RectTransform CreateButtonShell(Transform parent, string name, Vector2 pos, int sibling)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(Mathf.Min(sibling, parent.childCount - 1));
            Image img = go.AddComponent<Image>();
            img.raycastTarget = false;
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
            rt.sizeDelta = new Vector2(470f, 60f);
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
            rt.sizeDelta = new Vector2(280f, 18f);

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

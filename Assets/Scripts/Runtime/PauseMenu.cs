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
        private int selectedOption;
        private int settingsOption;
        private const int OptionCount = 5;
        private const int SettingsOptionCount = 4;

        private static readonly Color DeepDim = new Color(0.006f, 0.007f, 0.014f, 0.88f);
        private static readonly Color Glass = new Color(0.025f, 0.035f, 0.065f, 0.92f);
        private static readonly Color GlassLight = new Color(0.04f, 0.10f, 0.16f, 0.30f);
        private static readonly Color NeonYellow = new Color(1f, 0.94f, 0.04f, 1f);
        private static readonly Color NeonOrange = new Color(1f, 0.42f, 0.02f, 1f);
        private static readonly Color NeonCyan = new Color(0.0f, 0.92f, 1f, 1f);
        private static readonly Color TextNormal = new Color(0.92f, 0.90f, 1f, 1f);
        private static readonly Color TextDim = new Color(0.70f, 0.68f, 0.78f, 1f);

        private static readonly string[] OptionNames = { "CONTINUAR", "ELEGIR NIVEL", "REINICIAR", "SALIR", "CONFIGURACION" };
        private static readonly string[] OptionIcons = { "PLAY", "LEVEL", "RETRY", "QUIT", "SET" };

        private TMP_Text[] menuLabels;
        private Image[] menuButtonImages;
        private Image[] menuGlowImages;
        private CanvasGroup[] menuButtonGroups;
        private RectTransform[] menuButtonRects;

        private CanvasGroup settingsGroup;
        private TMP_Text settingsTitleText;
        private TMP_Text settingsBodyText;
        private TMP_Text settingsHintText;
        private Image brightnessOverlay;
        private float settingsAlpha;
        private float settingsTarget;
        private float brightnessValue = 0.50f;
        private float volumeValue = 1.00f;

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
            menuLabels = new[] { resumeLabel, selectLevelLabel, restartLabel, quitLabel };
            BuildSprites();
            BuildCommercialPauseMenu();
            BuildCommercialLevelSelect();
            BuildBasicSettingsPanel();
            StyleStaticTexts();
            ShowLevelSelectGroup(false, true);
            ShowSettingsGroup(false, true);

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
                else if (isPaused && !isInLevelSelect)
                    ClosePause();
                else if (isPaused && isInLevelSelect)
                    ExitLevelSelect();
                return;
            }

            if (!isPaused) return;

            if (isInLevelSelect)
                HandleLevelSelectInput();
            else if (isInSettings)
                HandleSettingsInput();
            else
                HandleMenuInput();

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
            Time.timeScale = 1f;
            if (gameController != null) gameController.PauseAudio(false);

            fadeTarget = 0f;
            ShowLevelSelectGroup(false);
            ShowSettingsGroup(false);
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
                case 3: QuitGame(); break;
                case 4: EnterSettings(); break;
            }
        }

        private void EnterSettings()
        {
            isInSettings = true;
            settingsOption = 0;
            ShowPauseGroup(false);
            ShowSettingsGroup(true);
            RefreshSettingsLabels();
        }

        private void ExitSettings()
        {
            isInSettings = false;
            ShowPauseGroup(true);
            ShowSettingsGroup(false);
            RefreshLabels();
        }

        private void HandleSettingsInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                settingsOption = (settingsOption - 1 + SettingsOptionCount) % SettingsOptionCount;
                RefreshSettingsLabels();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                settingsOption = (settingsOption + 1) % SettingsOptionCount;
                RefreshSettingsLabels();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                AdjustSetting(-0.05f);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                AdjustSetting(0.05f);
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                if (settingsOption == 3) ExitSettings();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitSettings();
            }
        }

        private void AdjustSetting(float delta)
        {
            if (settingsOption == 1)
            {
                brightnessValue = Mathf.Clamp01(brightnessValue + delta);
                ApplyBrightness();
            }
            else if (settingsOption == 2)
            {
                volumeValue = Mathf.Clamp01(volumeValue + delta);
                AudioListener.volume = volumeValue;
            }
            RefreshSettingsLabels();
        }

        private void ApplyBrightness()
        {
            if (brightnessOverlay != null)
                brightnessOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.40f, 0.0f, brightnessValue));
        }

        private string Bar(float value)
        {
            int segments = 14;
            int filled = Mathf.RoundToInt(Mathf.Clamp01(value) * segments);
            string bar = "";
            for (int i = 0; i < segments; i++)
                bar += i < filled ? "=" : "-";

            return "|" + bar + "|  " + Mathf.RoundToInt(value * 100f) + "%";
        }

        private void RefreshSettingsLabels()
        {
            if (settingsTitleText != null)
                settingsTitleText.text = "CONFIGURACION";

            if (settingsBodyText != null)
            {
                string s0 = settingsOption == 0 ? "<color=#FFF000>> VER CONTROLES</color>" : "  VER CONTROLES";
                string s1 = settingsOption == 1 ? "<color=#FFF000>> GRAFICOS</color>" : "  GRAFICOS";
                string s2 = settingsOption == 2 ? "<color=#FFF000>> SONIDO</color>" : "  SONIDO";
                string s3 = settingsOption == 3 ? "<color=#FFF000>> VOLVER</color>" : "  VOLVER";
                settingsBodyText.text =
                    s0 + "\n" +
                    "      D / F / J / K      Carriles\n" +
                    "      ESC                Pausa\n" +
                    "      ENTER              Confirmar\n" +
                    "      Flechas o W/S      Navegar\n" +
                    "      F2 / F3 / F4       Offset\n\n\n" +
                    s1 + "\n" +
                    "      Brillo             " + Bar(brightnessValue) + "\n\n\n" +
                    s2 + "\n" +
                    "      Volumen general    " + Bar(volumeValue) + "\n\n\n" +
                    s3;
            }

            if (settingsHintText != null)
                settingsHintText.text = "<color=#00F1FF>[W/S]</color> Navegar   <color=#FFF000>[A/D]</color> Ajustar   <color=#FF6A00>[ESC]</color> Volver";
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
                levelArtistText.text = "<color=#FFAA44>Nivel " + (idx + 1) + "</color>  <color=#00F1FF>-</color>  <color=#FFFFFF>LISTO PARA JUGAR</color>";
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

        private void ShowSettingsGroup(bool show, bool instant = false)
        {
            settingsTarget = show ? 1f : 0f;
            if (instant) settingsAlpha = settingsTarget;
            if (settingsGroup != null)
            {
                settingsGroup.interactable = show;
                settingsGroup.blocksRaycasts = show;
            }
        }

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

            if (settingsGroup != null)
            {
                settingsAlpha = Mathf.MoveTowards(settingsAlpha, settingsTarget, Time.unscaledDeltaTime * fadeSpeed);
                settingsGroup.alpha = settingsAlpha;
                settingsGroup.interactable = settingsAlpha > 0.5f && isInSettings;
                settingsGroup.blocksRaycasts = settingsAlpha > 0.5f && isInSettings;
                float ss = Mathf.Lerp(0.93f, 1f, Mathf.SmoothStep(0f, 1f, settingsAlpha));
                settingsGroup.transform.localScale = new Vector3(ss, ss, 1f);
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
            TMP_Text[] allTexts = { resumeLabel, selectLevelLabel, restartLabel, quitLabel, levelNameText, levelArtistText, levelHintText, settingsTitleText, settingsBodyText, settingsHintText };
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

            RectTransform card = CreateCard(pauseGroup.transform, "PB_UI_PauseCard", new Vector2(620f, 560f), Vector2.zero, 4);
            CreateLine(pauseGroup.transform, "PB_UI_PauseTopNeon", new Vector2(0f, 270f), new Vector2(500f, 4f), NeonOrange, 5);
            CreateLine(pauseGroup.transform, "PB_UI_PauseCyanLine", new Vector2(0f, 258f), new Vector2(320f, 2f), NeonCyan, 6);
            CreateTmp(pauseGroup.transform, "PB_UI_PauseSubtitle", "PROJECT BEAT", new Vector2(0f, 188f), new Vector2(500f, 34f), 18f, NeonCyan, 7, FontStyles.Bold, 5f);
            CreateTmp(pauseGroup.transform, "PB_UI_PauseTitle", "PAUSA", new Vector2(0f, 142f), new Vector2(500f, 64f), 48f, NeonYellow, 8, FontStyles.Bold, 7f);

            if (menuLabels == null || menuLabels.Length < OptionCount)
            {
                TMP_Text[] expanded = new TMP_Text[OptionCount];
                for (int i = 0; i < OptionCount - 1; i++) expanded[i] = (menuLabels != null && i < menuLabels.Length) ? menuLabels[i] : null;
                expanded[OptionCount - 1] = CreateTmp(pauseGroup.transform, "PB_UI_SettingsButtonLabel", "CONFIGURACION", Vector2.zero, new Vector2(470f, 60f), 24f, TextNormal, 19, FontStyles.Normal, 3f);
                menuLabels = expanded;
            }

            menuButtonImages = new Image[OptionCount];
            menuGlowImages = new Image[OptionCount];
            menuButtonGroups = new CanvasGroup[OptionCount];
            menuButtonRects = new RectTransform[OptionCount];

            float startY = 82f;
            for (int i = 0; i < OptionCount; i++)
            {
                RectTransform button = CreateButtonShell(pauseGroup.transform, "PB_UI_MenuButton_" + i, new Vector2(0f, startY - i * 68f), 9 + i);
                menuButtonRects[i] = button;
                menuButtonImages[i] = button.GetComponent<Image>();
                menuGlowImages[i] = CreateFloatingGlow(button, "PB_UI_SelectedGlow_" + i, Vector2.zero, new Vector2(470f, 66f), new Color(1f, 0.75f, 0.04f, 0f), 0);
                menuButtonGroups[i] = button.gameObject.AddComponent<CanvasGroup>();

                if (menuLabels[i] != null)
                {
                    RectTransform labelRT = menuLabels[i].GetComponent<RectTransform>();
                    labelRT.SetParent(button, false);
                    labelRT.anchorMin = Vector2.zero;
                    labelRT.anchorMax = Vector2.one;
                    labelRT.offsetMin = Vector2.zero;
                    labelRT.offsetMax = Vector2.zero;
                    menuLabels[i].transform.SetAsLastSibling();
                }
            }

            CreateTmp(pauseGroup.transform, "PB_UI_PauseHint", "<color=#00F1FF>[W/S]</color> Navegar     <color=#FFF000>[ENTER]</color> Confirmar     <color=#FF6A00>[ESC]</color> Cerrar", new Vector2(0f, -258f), new Vector2(540f, 28f), 17f, TextNormal, 20, FontStyles.Normal, 1.5f);
            RefreshLabels();
        }


        private void BuildBasicSettingsPanel()
        {
            if (pauseGroup == null) return;

            GameObject root = new GameObject("PB_UI_SettingsGroup");
            root.transform.SetParent(pauseGroup.transform.parent != null ? pauseGroup.transform.parent : pauseGroup.transform, false);

            // Importante: este objeto es UI y debe tener RectTransform desde el inicio.
            // Antes se creaba como GameObject normal y eso producia MissingComponentException.
            RectTransform rt = root.AddComponent<RectTransform>();
            settingsGroup = root.AddComponent<CanvasGroup>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;

            CreateFullScreenImage(root.transform, "PB_Settings_Dim", DeepDim, 0);
            CreateFullScreenImage(root.transform, "PB_Settings_BrightnessOverlay", new Color(0f, 0f, 0f, 0.20f), 1);
            Transform overlayTransform = root.transform.Find("PB_Settings_BrightnessOverlay");
            if (overlayTransform != null) brightnessOverlay = overlayTransform.GetComponent<Image>();

            CreateCard(root.transform, "PB_Settings_Card", new Vector2(760f, 540f), Vector2.zero, 2);
            CreateLine(root.transform, "PB_Settings_TopLine", new Vector2(0f, 245f), new Vector2(620f, 4f), NeonCyan, 3);
            settingsTitleText = CreateTmp(root.transform, "PB_Settings_Title", "CONFIGURACION", new Vector2(0f, 190f), new Vector2(640f, 54f), 42f, NeonYellow, 4, FontStyles.Bold, 5f);
            settingsBodyText = CreateTmp(root.transform, "PB_Settings_Body", "", new Vector2(0f, -10f), new Vector2(680f, 340f), 19f, TextNormal, 5, FontStyles.Normal, 1.2f);
            settingsBodyText.alignment = TextAlignmentOptions.Left;
            settingsHintText = CreateTmp(root.transform, "PB_Settings_Hint", "", new Vector2(0f, -238f), new Vector2(690f, 30f), 16f, TextNormal, 6, FontStyles.Normal, 1.5f);
            ApplyBrightness();
            RefreshSettingsLabels();
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

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Pause menu: animates open/close with alpha fade + scale punch.
    /// Cursor pulsates on the active option. Cinematic overlay dims the game behind it.
    /// Options: Resume | Restart | Quit  (level select hidden if only 1 level)
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private CanvasGroup pauseGroup;
        [SerializeField] private CanvasGroup levelSelectGroup;
        [SerializeField] private RectTransform pausePanelRect;   // for scale punch

        [Header("Main Menu Labels (TMP)")]
        [SerializeField] private TMP_Text resumeLabel;
        [SerializeField] private TMP_Text selectLevelLabel;
        [SerializeField] private TMP_Text restartLabel;
        [SerializeField] private TMP_Text quitLabel;

        [Header("Decorative")]
        [SerializeField] private TMP_Text titleLabel;       // "II PAUSA" big title
        [SerializeField] private TMP_Text subtitleLabel;    // song name under title
        [SerializeField] private Image    dividerLine;      // horizontal line below title

        [Header("Level Select")]
        [SerializeField] private TMP_Text levelNameText;
        [SerializeField] private TMP_Text levelArtistText;
        [SerializeField] private TMP_Text levelHintText;

        [Header("Controller")]
        [SerializeField] private GameController gameController;

        // ── Runtime ───────────────────────────────────────────────────────
        private bool  isPaused;
        private bool  isInLevelSelect;
        private int   selectedOption;
        private const int OptionCount = 4;

        // Animation
        private float openTimer;
        private bool  isOpening;
        private const float OpenDur = 0.18f;

        // Cursor pulse
        private float cursorPulse;

        // Colour palette
        private static readonly Color ColNormal   = new Color(0.85f, 0.72f, 0.50f, 1f);  // warm gold-tan
        private static readonly Color ColActive   = new Color(1.00f, 0.90f, 0.20f, 1f);  // bright yellow
        private static readonly Color ColDim      = new Color(0.45f, 0.38f, 0.28f, 1f);  // dim brown

        private static readonly string[] OptionNames =
        {
            "CONTINUAR",
            "ELEGIR NIVEL",
            "REINICIAR",
            "SALIR"
        };

        // Icon glyphs per option (Unicode)
        private static readonly string[] OptionIcons =
        {
            "\u25B6",   // ▶
            "\u266B",   // ♫
            "\u21BA",   // ↺
            "\u00D7"    // ×
        };

        public bool IsPaused => isPaused;

        // ── Update ────────────────────────────────────────────────────────
        private void Update()
        {
            // Open/close animation
            if (isOpening && openTimer < OpenDur)
            {
                openTimer += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, openTimer / OpenDur);
                if (pauseGroup != null) pauseGroup.alpha = t;
                if (pausePanelRect != null)
                {
                    float s = Mathf.Lerp(0.88f, 1f, t);
                    pausePanelRect.localScale = Vector3.one * s;
                }
            }

            cursorPulse += Time.unscaledDeltaTime * 4.5f;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!isPaused && gameController != null && gameController.IsGameplayRunning)
                    OpenPause();
                else if (isPaused && !isInLevelSelect)
                    ClosePause();
                else if (isPaused && isInLevelSelect)
                    ExitLevelSelect();
                return;
            }

            if (!isPaused) return;

            if (isInLevelSelect) HandleLevelSelectInput();
            else                 HandleMenuInput();

            // Animated cursor on active label
            AnimateCursor();
        }

        // ── Open / Close ──────────────────────────────────────────────────
        public void OpenPause()
        {
            isPaused       = true;
            isOpening      = true;
            openTimer      = 0f;
            Time.timeScale = 0f;
            gameController?.PauseAudio(true);
            selectedOption = 0;
            ShowPauseGroup(true);
            ShowLevelSelectGroup(false);
            RefreshLabels();
            UpdateSubtitle();
        }

        public void ClosePause()
        {
            isPaused        = false;
            isInLevelSelect = false;
            isOpening       = false;
            Time.timeScale  = 1f;
            gameController?.PauseAudio(false);
            ShowPauseGroup(false);
            ShowLevelSelectGroup(false);
        }

        // ── Menu input ────────────────────────────────────────────────────
        private void HandleMenuInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                selectedOption = (selectedOption - 1 + OptionCount) % OptionCount;
                RefreshLabels();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                selectedOption = (selectedOption + 1) % OptionCount;
                RefreshLabels();
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ConfirmOption();
            }
        }

        private void ConfirmOption()
        {
            switch (selectedOption)
            {
                case 0: ClosePause();       break;
                case 1: EnterLevelSelect(); break;
                case 2: RestartLevel();     break;
                case 3: QuitGame();         break;
            }
        }

        // ── Level Select ──────────────────────────────────────────────────
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
            var lm = LevelManager.Instance;
            if (lm == null) return;

            if (Input.GetKeyDown(KeyCode.LeftArrow))  { lm.PreviousLevel(); RefreshLevelSelectLabels(); }
            if (Input.GetKeyDown(KeyCode.RightArrow)) { lm.NextLevel();     RefreshLevelSelectLabels(); }
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                ConfirmLevelSelect();
            if (Input.GetKeyDown(KeyCode.Escape)) ExitLevelSelect();
        }

        private void ConfirmLevelSelect()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // ── Actions ───────────────────────────────────────────────────────
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

        // ── Label refresh ─────────────────────────────────────────────────
        private void RefreshLabels()
        {
            TMP_Text[] labels = { resumeLabel, selectLevelLabel, restartLabel, quitLabel };
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] == null) continue;
                bool active = (i == selectedOption);
                labels[i].color     = active ? ColActive : ColNormal;
                labels[i].fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
                labels[i].fontSize  = active ? 40f : 32f;

                if (active)
                    labels[i].text = $"<color=#ff8800>{OptionIcons[i]}</color>  " +
                                     $"<color=#ffee00>{OptionNames[i]}</color>";
                else
                    labels[i].text = $"<color=#664400>{OptionIcons[i]}</color>  " +
                                     $"<color=#aa8844>{OptionNames[i]}</color>";
            }
        }

        // Pulsating cursor on the active option label
        private void AnimateCursor()
        {
            TMP_Text[] labels = { resumeLabel, selectLevelLabel, restartLabel, quitLabel };
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] == null || i != selectedOption) continue;
                float pulse = 0.85f + 0.15f * Mathf.Sin(cursorPulse);
                Color c = labels[i].color;
                c.a = pulse;
                labels[i].color = c;
            }
        }

        private void UpdateSubtitle()
        {
            if (subtitleLabel == null) return;
            var ld = LevelManager.Instance?.CurrentLevel;
            if (ld != null)
                subtitleLabel.text = $"<color=#ff6600>{ld.levelName}</color>  " +
                                     $"<size=20><color=#aa6622>{ld.artistName}</color></size>";
        }

        private void RefreshLevelSelectLabels()
        {
            var lm = LevelManager.Instance;
            if (lm == null || lm.CurrentLevel == null) return;
            LevelData lv = lm.CurrentLevel;
            int idx = lm.CurrentLevelIndex, total = lm.Levels.Length;
            if (levelNameText   != null)
                levelNameText.text   = $"<color=#ff9900>\u25C4</color>  " +
                                       $"<b><color=#ffee00>{lv.levelName}</color></b>  " +
                                       $"<color=#ff9900>\u25BA</color>\n" +
                                       $"<size=20><color=#888866>({idx+1} / {total})</color></size>";
            if (levelArtistText != null)
                levelArtistText.text = $"<color=#ffaa44>{lv.artistName}</color>";
            if (levelHintText   != null)
                levelHintText.text   = "<size=18><color=#554422>\u2190 \u2192  Cambiar" +
                                       "     Enter  Confirmar     ESC  Volver</color></size>";
        }

        // ── Group helpers ─────────────────────────────────────────────────
        private void ShowPauseGroup(bool show)
        {
            if (pauseGroup == null) return;
            pauseGroup.alpha          = show ? 1f : 0f;
            pauseGroup.interactable   = show;
            pauseGroup.blocksRaycasts = show;
        }

        private void ShowLevelSelectGroup(bool show)
        {
            if (levelSelectGroup == null) return;
            levelSelectGroup.alpha          = show ? 1f : 0f;
            levelSelectGroup.interactable   = show;
            levelSelectGroup.blocksRaycasts = show;
        }
    }
}

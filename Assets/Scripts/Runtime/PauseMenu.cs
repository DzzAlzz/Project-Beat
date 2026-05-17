using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Pause menu with arrow-key navigation and Enter to confirm.
    /// Options: Continue | Level Select | Restart | Quit
    /// Avance 13: cleaner menu with subtle glow/fade/scale, still not final UI.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        // ── Serialized ─────────────────────────────────────────────────────
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

        // ── Runtime ────────────────────────────────────────────────────────
        private bool isPaused;
        private bool isInLevelSelect;
        private int selectedOption; // 0=Resume 1=SelectLevel 2=Restart 3=Quit
        private const int OptionCount = 4;

        private static readonly Color NormalColor = new Color(0.82f, 0.84f, 0.96f, 1f);
        private static readonly Color ActiveColor = new Color(1f, 0.88f, 0.16f, 1f);

        // ASCII puro — garantizado en cualquier atlas TMP por defecto de Unity
        // (los caracteres especiales no estan en el atlas TMP por defecto y aparecen como cuadros vacios)
        private static readonly string[] OptionNames =
        {
            "CONTINUAR",
            "SELECCIONAR NIVEL",
            "REINICIAR",
            "SALIR"
        };

        // Fade animado (unscaledDeltaTime: funciona con timeScale = 0)
        private float _fadeAlpha  = 0f;
        private float _fadeTarget = 0f;
        private const float FadeSpeed = 10f;

        // Pulso del cursor activo
        private float _pulseT = 0f;

        // ── Public API ─────────────────────────────────────────────────────
        public bool IsPaused => isPaused;

        private void Update()
        {
            // Fade suave del panel (unscaled — funciona con timeScale = 0)
            if (pauseGroup != null)
            {
                _fadeAlpha = Mathf.MoveTowards(_fadeAlpha, _fadeTarget,
                                               Time.unscaledDeltaTime * FadeSpeed);
                pauseGroup.alpha          = _fadeAlpha;
                pauseGroup.interactable   = _fadeAlpha > 0.5f;
                pauseGroup.blocksRaycasts = _fadeAlpha > 0.5f;
            }

            _pulseT += Time.unscaledDeltaTime * 4f;

            // ESC toggles pause (only during gameplay)
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

            if (isInLevelSelect)
                HandleLevelSelectInput();
            else
                HandleMenuInput();

            PulseActiveLabel();
        }

        // ── Pause Open/Close ───────────────────────────────────────────────
        public void OpenPause()
        {
            isPaused       = true;
            Time.timeScale = 0f;

            if (gameController != null)
                gameController.PauseAudio(true);

            selectedOption = 0;
            _fadeTarget    = 1f;         // fade in suave
            ShowLevelSelectGroup(false);
            RefreshLabels();
        }

        public void ClosePause()
        {
            isPaused        = false;
            isInLevelSelect = false;
            Time.timeScale  = 1f;

            if (gameController != null)
                gameController.PauseAudio(false);

            _fadeTarget = 0f;            // fade out suave
            ShowLevelSelectGroup(false);
        }

        // ── Main Menu Input ────────────────────────────────────────────────
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
                case 0: ClosePause();           break;  // Resume
                case 1: EnterLevelSelect();     break;  // Select Level
                case 2: RestartLevel();         break;  // Restart
                case 3: QuitGame();             break;  // Quit
            }
        }

        // ── Level Select Input ─────────────────────────────────────────────
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

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                lm.PreviousLevel();
                RefreshLevelSelectLabels();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                lm.NextLevel();
                RefreshLevelSelectLabels();
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

        private void ConfirmLevelSelect()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // ── Actions ────────────────────────────────────────────────────────
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

        // ── Label Refresh ──────────────────────────────────────────────────
        private void RefreshLabels()
        {
            TMP_Text[] labels = { resumeLabel, selectLevelLabel, restartLabel, quitLabel };
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] == null) continue;
                bool active = (i == selectedOption);

                labels[i].fontStyle = active ? FontStyles.Bold   : FontStyles.Normal;
                labels[i].fontSize  = active ? 40f               : 32f;
                labels[i].color     = active ? ActiveColor        : NormalColor;

                // ">" cursor ASCII visible en cualquier fuente TMP
                // opcion activa: cursor naranja + texto amarillo resaltado
                // opcion normal: sin cursor + texto tenue
                if (active)
                    labels[i].text =
                        "<color=#ff9900>></color>  " +
                        "<color=#ffee66>" + OptionNames[i] + "</color>";
                else
                    labels[i].text =
                        "   <color=#c8c8d8>" + OptionNames[i] + "</color>";
            }
        }

        // Pulso de brillo suave en la etiqueta activa
        private void PulseActiveLabel()
        {
            TMP_Text[] labels = { resumeLabel, selectLevelLabel, restartLabel, quitLabel };
            if (selectedOption < 0 || selectedOption >= labels.Length) return;
            var lbl = labels[selectedOption];
            if (lbl == null) return;
            float a = 0.75f + 0.25f * Mathf.Sin(_pulseT);
            Color c = lbl.color; c.a = a; lbl.color = c;
        }

        private void RefreshLevelSelectLabels()
        {
            var lm = LevelManager.Instance;
            if (lm == null || lm.CurrentLevel == null) return;

            LevelData level = lm.CurrentLevel;
            int idx   = lm.CurrentLevelIndex;
            int total = lm.Levels.Length;

            if (levelNameText != null)
                levelNameText.text =
                    "<  <b><color=#ffee66>" + level.levelName + "</color></b>  >\n" +
                    "<size=22><color=#c9c9ff>Nivel " + (idx + 1) + " de " + total + "</color></size>";

            if (levelArtistText != null)
                levelArtistText.text = level.artistName;

            if (levelHintText != null)
                levelHintText.text =
                    "A/D o Flechas: cambiar   Enter/Espacio: confirmar   ESC: volver";
        }

        // ── Group Helpers ──────────────────────────────────────────────────
        private void ShowPauseGroup(bool show)
        {
            // El fade lo gestiona Update() via _fadeTarget
            _fadeTarget = show ? 1f : 0f;
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

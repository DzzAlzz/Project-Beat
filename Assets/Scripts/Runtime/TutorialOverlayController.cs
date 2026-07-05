using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Overlay simple para el Nivel 0 Tutorial.
    /// Avance 46:
    /// - Se reconstruye de forma segura cuando el nivel activo es TUTORIAL.
    /// - Se mantiene visible durante el tutorial.
    /// - Se oculta mientras el juego esta en pausa.
    /// - Se destruye al salir del tutorial, al ir al menu principal o al cambiar de nivel.
    /// </summary>
    public class TutorialOverlayController : MonoBehaviour
    {
        private CanvasGroup group;
        private bool uiBuilt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateIfTutorial()
        {
            // Primer intento. GameController vuelve a llamar cuando ya esta inicializado el nivel,
            // evitando que el overlay se pierda por orden de carga.
            EnsureForCurrentLevel();
        }

        public static void EnsureForCurrentLevel()
        {
            if (!ShouldShowForCurrentLevel())
            {
                DestroyAll();
                return;
            }

            if (FindObjectOfType<TutorialOverlayController>() != null)
                return;

            GameObject go = new GameObject("TutorialOverlayController");
            go.AddComponent<TutorialOverlayController>();
        }

        public static void DestroyAll()
        {
            TutorialOverlayController[] overlays = FindObjectsOfType<TutorialOverlayController>();
            for (int i = 0; i < overlays.Length; i++)
            {
                if (overlays[i] != null)
                    Destroy(overlays[i].gameObject);
            }
        }

        private static bool ShouldShowForCurrentLevel()
        {
            if (StartupFlowController.IsMainMenuVisible)
                return false;

            if (PlayerPrefs.GetInt(StartupFlowController.ForceMainMenuPrefsKey, 0) == 1)
                return false;

            LevelData current = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : null;
            if (current == null || string.IsNullOrEmpty(current.levelName))
                return false;

            return current.levelName.Trim().ToUpperInvariant() == "TUTORIAL";
        }

        private void Start()
        {
            if (!ShouldShowForCurrentLevel())
            {
                Destroy(gameObject);
                return;
            }

            BuildUI();
        }

        private void BuildUI()
        {
            if (uiBuilt) return;
            uiBuilt = true;

            Canvas canvas = new GameObject("TutorialOverlayCanvas").AddComponent<Canvas>();
            canvas.transform.SetParent(transform, false);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 3500;
            canvas.gameObject.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;

            group = canvas.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 1f;
            group.blocksRaycasts = false;
            group.interactable = false;

            Image panel = new GameObject("Panel").AddComponent<Image>();
            panel.transform.SetParent(canvas.transform, false);
            panel.color = new Color(0.025f, 0.028f, 0.038f, 0.76f);
            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0f, 0f);
            prt.anchorMax = new Vector2(0f, 1f);
            prt.pivot = new Vector2(0f, 0.5f);
            prt.sizeDelta = new Vector2(330f, 0f);
            prt.anchoredPosition = Vector2.zero;

            TMP_Text text = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            text.transform.SetParent(panel.transform, false);
            text.text = "<b><color=#e8edf5>TUTORIAL</color></b>\n\n" +
                        "D  F  J  K  = carriles\n" +
                        "Toca cuando la nota llegue a la barra.\n\n" +
                        "PERFECT / GOOD aumentan tu puntaje.\n" +
                        "Mantén combo para mejorar el resultado.\n\n" +
                        "PREC muestra tu precisión.\n" +
                        "ESC pausa el juego.";
            text.fontSize = 20f;
            text.color = new Color(0.88f, 0.92f, 0.98f, 1f);
            text.alignment = TextAlignmentOptions.TopLeft;
            RectTransform trt = text.rectTransform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(24f, 72f);
            trt.offsetMax = new Vector2(-20f, -72f);
        }

        private void Update()
        {
            if (!ShouldShowForCurrentLevel())
            {
                Destroy(gameObject);
                return;
            }

            if (!uiBuilt)
                BuildUI();

            PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
            bool pauseVisible = pauseMenu != null && pauseMenu.IsPausedForOverlay;
            bool mainMenuVisible = StartupFlowController.IsMainMenuVisible;

            // Visible solo mientras se juega Tutorial.
            // Oculto si se abre pausa o si se muestra el menu principal.
            if (group != null)
            {
                bool visible = !pauseVisible && !mainMenuVisible;
                group.alpha = visible ? 1f : 0f;
                group.blocksRaycasts = false;
                group.interactable = false;
            }
        }
    }
}

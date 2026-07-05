using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Overlay simple para el Nivel 0 Tutorial.
    /// Avance 45:
    /// - Se muestra durante todo el tutorial.
    /// - Se oculta mientras el juego esta en pausa.
    /// - No aparece en el menu principal ni en otros niveles.
    /// </summary>
    public class TutorialOverlayController : MonoBehaviour
    {
        private CanvasGroup group;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateIfTutorial()
        {
            // Si se esta forzando el menu principal, no se crea el panel de tutorial
            // aunque el ultimo LevelData guardado siga siendo TUTORIAL.
            if (PlayerPrefs.GetInt(StartupFlowController.ForceMainMenuPrefsKey, 0) == 1)
                return;

            if (StartupFlowController.IsMainMenuVisible)
                return;

            LevelData current = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : null;
            if (current == null || current.levelName != "TUTORIAL") return;
            if (FindObjectOfType<TutorialOverlayController>() != null) return;

            GameObject go = new GameObject("TutorialOverlayController");
            go.AddComponent<TutorialOverlayController>();
        }

        private void Start()
        {
            LevelData current = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : null;
            if (current == null || current.levelName != "TUTORIAL")
            {
                Destroy(gameObject);
                return;
            }

            Canvas canvas = new GameObject("TutorialOverlayCanvas").AddComponent<Canvas>();
            canvas.transform.SetParent(transform, false);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 3500;
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);

            group = canvas.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 1f;

            Image panel = new GameObject("Panel").AddComponent<Image>();
            panel.transform.SetParent(canvas.transform, false);
            panel.color = new Color(0.03f, 0.035f, 0.045f, 0.72f);
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
            LevelData current = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : null;
            if (current == null || current.levelName != "TUTORIAL")
            {
                Destroy(gameObject);
                return;
            }

            PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
            bool pauseVisible = pauseMenu != null && pauseMenu.IsPausedForOverlay;
            bool mainMenuVisible = StartupFlowController.IsMainMenuVisible;

            // Visible solo mientras se juega Tutorial.
            // Oculto si se abre pausa o si se muestra el menu principal.
            if (group != null)
            {
                group.alpha = (pauseVisible || mainMenuVisible) ? 0f : 1f;
                group.blocksRaycasts = false;
                group.interactable = false;
            }
        }
    }
}

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

            if (StartupFlowController.SuppressGameplayStartup)
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

            Image panel = new GameObject("PB_TutorialPanel_Modern", typeof(RectTransform)).AddComponent<Image>();
            panel.transform.SetParent(canvas.transform, false);
            panel.color = new Color(0.055f, 0.010f, 0.105f, 0.88f);
            panel.raycastTarget = false;
            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.94f, 1f, 0.52f);
            outline.effectDistance = new Vector2(2f, -2f);
            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0f, 0.5f);
            prt.anchorMax = new Vector2(0f, 0.5f);
            prt.pivot = new Vector2(0f, 0.5f);
            prt.sizeDelta = new Vector2(358f, 660f);
            prt.anchoredPosition = new Vector2(14f, 0f);

            CreateDecorLine(panel.transform, new Vector2(0f, 300f), new Vector2(286f, 3f), new Color(1f, 0.12f, 0.82f, 0.92f));
            CreateDecorLine(panel.transform, new Vector2(0f, 286f), new Vector2(224f, 2f), new Color(0f, 0.94f, 1f, 0.86f));
            CreateDecorLine(panel.transform, new Vector2(0f, -300f), new Vector2(254f, 3f), new Color(1f, 0.12f, 0.82f, 0.78f));

            TMP_Text title = CreatePanelText(panel.transform, "Title", new Vector2(0f, 252f), new Vector2(304f, 48f), 24f, TextAlignmentOptions.Center, FontStyles.Bold, 4f);
            title.text = "<color=#00F1FF>TUTORIAL</color>";

            CreateKeyCaps(panel.transform);

            TMP_Text text = CreatePanelText(panel.transform, "Body", new Vector2(0f, -64f), new Vector2(306f, 420f), 14.4f, TextAlignmentOptions.TopLeft, FontStyles.Normal, 0.15f);
            text.text =
                "<color=#FF66D9><b>CONTROLES</b></color>\n" +
                "<color=#EAF4FF>D / F / J / K = carriles.</color>\n" +
                "<color=#BFC8E6>Golpea cuando la nota llegue a la barra.</color>\n\n" +
                "<color=#FFF36B><b>PRECISION</b></color>\n" +
                "<color=#FFF36B><b>PERFECTO</b></color>: golpe excelente.\n" +
                "<color=#7CFFB2><b>BIEN</b></color>: golpe correcto.\n" +
                "<color=#FFB25C><b>MAL</b></color>: golpe impreciso.\n" +
                "<color=#FF5A66><b>FALLO</b></color>: nota perdida.\n\n" +
                "<color=#FF66D9><b>COMBO</b></color>\n" +
                "<color=#BFC8E6>Mantén aciertos seguidos para subir combo.</color>\n" +
                "<color=#BFC8E6>Fallar rompe la racha.</color>\n\n" +
                "<color=#00F1FF><b>HUD</b></color>\n" +
                "<color=#BFC8E6>PREC muestra tu precisión.</color>\n" +
                "<color=#FF8A28>ESC</color> <color=#BFC8E6>pausa el juego.</color>";
        }

        private TMP_Text CreatePanelText(Transform parent, string name, Vector2 pos, Vector2 size, float fontSize, TextAlignmentOptions alignment, FontStyles style, float spacing)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.characterSpacing = spacing;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = true;
            tmp.richText = true;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.82f);
            shadow.effectDistance = new Vector2(1.6f, -1.6f);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return tmp;
        }

        private void CreateDecorLine(Transform parent, Vector2 pos, Vector2 size, Color color)
        {
            GameObject go = new GameObject("DecorLine", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        private void CreateKeyCaps(Transform parent)
        {
            string[] labels = { "D", "F", "J", "K" };
            float[] xs = { -108f, -36f, 36f, 108f };
            for (int i = 0; i < labels.Length; i++)
            {
                GameObject keyGO = new GameObject("Key_" + labels[i], typeof(RectTransform));
                keyGO.transform.SetParent(parent, false);
                RectTransform rt = keyGO.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(xs[i], 200f);
                rt.sizeDelta = new Vector2(48f, 38f);
                Image bg = keyGO.AddComponent<Image>();
                bg.color = new Color(0.075f, 0.018f, 0.160f, 0.94f);
                bg.raycastTarget = false;
                Outline outline = keyGO.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0.94f, 1f, 0.58f);
                outline.effectDistance = new Vector2(1.8f, -1.8f);
                TMP_Text label = CreatePanelText(keyGO.transform, "Label", Vector2.zero, new Vector2(48f, 38f), 20f, TextAlignmentOptions.Center, FontStyles.Bold, 1f);
                label.text = "<color=#FFF36B>" + labels[i] + "</color>";
            }
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
            GameplayUI gameplayUI = FindObjectOfType<GameplayUI>();
            bool resultsVisible = gameplayUI != null && gameplayUI.IsResultsVisibleForCursor;

            // Visible solo mientras se juega Tutorial.
            // Oculto si se abre pausa, si se muestra el menu principal o al mostrar resultados.
            if (group != null)
            {
                bool visible = !pauseVisible && !mainMenuVisible && !resultsVisible;
                group.alpha = visible ? 1f : 0f;
                group.blocksRaycasts = false;
                group.interactable = false;
            }
        }
    }
}

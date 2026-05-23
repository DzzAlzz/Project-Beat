using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        private Canvas canvas;
        private CanvasGroup rootGroup;
        private CanvasGroup splashGroup;
        private CanvasGroup menuGroup;
        private RectTransform tutorialButton;
        private RectTransform arcadeButton;
        private RectTransform futureButton;
        private TMP_Text tutorialLabel;
        private TMP_Text arcadeLabel;
        private TMP_Text futureLabel;
        private Image tutorialBg;
        private Image arcadeBg;
        private Image futureBg;
        private int selectedIndex;
        private float pulse;
        private GameController gameController;
        private PauseMenu pauseMenu;

        private static readonly Color BgDark = new Color(0.003f, 0.006f, 0.014f, 0.96f);
        private static readonly Color Panel = new Color(0.015f, 0.035f, 0.065f, 0.92f);
        private static readonly Color PanelSoft = new Color(0.04f, 0.10f, 0.16f, 0.58f);
        private static readonly Color NeonCyan = new Color(0.0f, 0.92f, 1f, 1f);
        private static readonly Color NeonYellow = new Color(1f, 0.94f, 0.04f, 1f);
        private static readonly Color NeonOrange = new Color(1f, 0.42f, 0.02f, 1f);
        private static readonly Color TextDim = new Color(0.62f, 0.72f, 0.86f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateOnSceneLoad()
        {
            if (SceneManager.GetActiveScene().name.ToLower().Contains("preview")) return;
            if (FindObjectOfType<StartupFlowController>() != null) return;

            if (PlayerPrefs.GetInt(SkipStartupPrefsKey, 0) == 1)
            {
                PlayerPrefs.SetInt(SkipStartupPrefsKey, 0);
                PlayerPrefs.Save();
                return;
            }

            GameObject go = new GameObject("StartupFlowController");
            go.AddComponent<StartupFlowController>();
        }

        private void Awake()
        {
            gameController = FindObjectOfType<GameController>();
            pauseMenu = FindObjectOfType<PauseMenu>();

            if (gameController != null)
                gameController.enabled = false;

            Time.timeScale = 1f;
            BuildUI();
            StartCoroutine(FlowRoutine());
        }

        private void Update()
        {
            pulse += Time.unscaledDeltaTime * 4.2f;
            AnimateButtons();

            if (menuGroup == null || menuGroup.alpha < 0.95f) return;

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) ||
                Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                selectedIndex += (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) ? -1 : 1;
                if (selectedIndex < 0) selectedIndex = 2;
                if (selectedIndex > 2) selectedIndex = 0;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                if (selectedIndex == 0)
                    StartCoroutine(OpenTutorialRoutine());
                else if (selectedIndex == 1)
                    StartCoroutine(OpenArcadeRoutine());
                else
                    StartCoroutine(ShakeLockedRoutine());
            }
        }

        private IEnumerator FlowRoutine()
        {
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
            menuGroup.interactable = false;
            menuGroup.blocksRaycasts = false;
            yield return Fade(rootGroup, 1f, 0f, 0.35f);

            if (LevelManager.Instance != null)
                LevelManager.Instance.SetLevel(0);

            if (FindObjectOfType<TutorialOverlayController>() == null)
                new GameObject("TutorialOverlayController").AddComponent<TutorialOverlayController>();

            if (gameController != null)
                gameController.enabled = true;

            Destroy(gameObject);
        }

        private IEnumerator OpenArcadeRoutine()
        {
            menuGroup.interactable = false;
            menuGroup.blocksRaycasts = false;
            yield return Fade(rootGroup, 1f, 0f, 0.35f);

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

        private void BuildUI()
        {
            canvas = new GameObject("StartupCanvas").AddComponent<Canvas>();
            canvas.transform.SetParent(transform, false);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;

            rootGroup = canvas.gameObject.AddComponent<CanvasGroup>();

            Image bg = CreateImage("Background", canvas.transform, BgDark);
            Stretch(bg.rectTransform);

            // Lineas neon sutiles para dar identidad de juego ritmico.
            CreateLine("TopNeon", canvas.transform, new Vector2(0f, 245f), new Vector2(620f, 3f), NeonOrange);
            CreateLine("BottomNeon", canvas.transform, new Vector2(0f, -245f), new Vector2(420f, 2f), NeonCyan);

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
            Image panel = CreateImage("MainPanel", menuGroup.transform, Panel);
            panel.rectTransform.sizeDelta = new Vector2(560f, 450f);
            panel.rectTransform.anchoredPosition = Vector2.zero;
            panel.type = Image.Type.Sliced;
            panel.sprite = MakeSprite(new Color(1f,1f,1f,1f));

            TMP_Text title = CreateText("MenuTitle", menuGroup.transform, "PROJECT BEAT", 42, NeonYellow, FontStyles.Bold);
            title.alignment = TextAlignmentOptions.Center;
            title.rectTransform.anchoredPosition = new Vector2(0f, 156f);
            title.rectTransform.sizeDelta = new Vector2(520f, 70f);

            TMP_Text label = CreateText("MenuSubtitle", menuGroup.transform, "SELECCIONA MODO DE JUEGO", 16, NeonCyan, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            label.characterSpacing = 4f;
            label.rectTransform.anchoredPosition = new Vector2(0f, 108f);
            label.rectTransform.sizeDelta = new Vector2(520f, 38f);

            tutorialButton = CreateModeButton(menuGroup.transform, "TutorialButton", new Vector2(0f, 48f), true, out tutorialBg, out tutorialLabel, "TUTORIAL", "Aprende a jugar");
            arcadeButton = CreateModeButton(menuGroup.transform, "ArcadeButton", new Vector2(0f, -32f), true, out arcadeBg, out arcadeLabel, "ARCADE", "Selector de niveles");
            futureButton = CreateModeButton(menuGroup.transform, "FutureButton", new Vector2(0f, -112f), false, out futureBg, out futureLabel, "PROXIMAMENTE", "Nuevo modo futuro");

            TMP_Text hint = CreateText("Hint", menuGroup.transform, "[W/S] Navegar     [ENTER] Confirmar", 14, TextDim, FontStyles.Bold);
            hint.alignment = TextAlignmentOptions.Center;
            hint.rectTransform.anchoredPosition = new Vector2(0f, -185f);
            hint.rectTransform.sizeDelta = new Vector2(520f, 32f);
        }

        private RectTransform CreateModeButton(Transform parent, string name, Vector2 pos, bool available, out Image bg, out TMP_Text label, string textOverride = null, string subOverride = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(420f, 62f);
            rt.anchoredPosition = pos;
            bg = go.AddComponent<Image>();
            bg.sprite = MakeSprite(Color.white);
            bg.type = Image.Type.Sliced;
            bg.color = available ? PanelSoft : new Color(0.08f, 0.08f, 0.12f, 0.62f);

            string text = textOverride ?? (available ? "ARCADE" : "PROXIMAMENTE");
            string sub = subOverride ?? (available ? "Selector de niveles" : "Nuevo modo futuro");

            label = CreateText("Text", go.transform, text + "\n" + sub, available ? 24 : 21, available ? NeonYellow : TextDim, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.sizeDelta = rt.sizeDelta;
            label.rectTransform.anchoredPosition = Vector2.zero;
            label.lineSpacing = -18f;
            return rt;
        }

        private void AnimateButtons()
        {
            AnimateButton(tutorialButton, tutorialBg, tutorialLabel, selectedIndex == 0, true);
            AnimateButton(arcadeButton, arcadeBg, arcadeLabel, selectedIndex == 1, true);
            AnimateButton(futureButton, futureBg, futureLabel, selectedIndex == 2, false);
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

        private void CreateLine(string name, Transform parent, Vector2 pos, Vector2 size, Color color)
        {
            Image img = CreateImage(name, parent, color);
            img.rectTransform.sizeDelta = size;
            img.rectTransform.anchoredPosition = pos;
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

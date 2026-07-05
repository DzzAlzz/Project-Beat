using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Pantalla unica de carga para transiciones.
    /// Fondo negro completo + texto CARGANDO... morado/neon.
    /// No toca la logica de niveles ni gameplay; solo cubre visualmente la escena.
    /// </summary>
    public class UnifiedLoadingScreen : MonoBehaviour
    {
        private static UnifiedLoadingScreen instance;

        private CanvasGroup group;
        private TMP_Text text;
        private Image topLine;
        private Image bottomLine;
        private float targetAlpha;
        private float fadeSpeed = 8f;
        private string baseMessage = "CARGANDO";

        private static readonly Color DeepBlack = new Color(0f, 0f, 0f, 1f);
        private static readonly Color NeonMagenta = new Color(1f, 0.22f, 0.78f, 1f);
        private static readonly Color NeonCyan = new Color(0f, 0.94f, 1f, 1f);
        private static readonly Color SoftPurple = new Color(0.55f, 0.18f, 1f, 1f);

        public static bool Visible => instance != null && instance.group != null && instance.group.alpha > 0.02f;

        public static void Show(string message = "CARGANDO...", bool instant = true)
        {
            EnsureInstance();
            if (instance == null) return;

            instance.baseMessage = string.IsNullOrWhiteSpace(message) ? "CARGANDO" : message.Replace(".", string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(instance.baseMessage)) instance.baseMessage = "CARGANDO";

            instance.targetAlpha = 1f;
            if (instance.group != null)
            {
                instance.group.blocksRaycasts = true;
                instance.group.interactable = true;
                if (instant) instance.group.alpha = 1f;
            }
            instance.transform.SetAsLastSibling();
            if (instance.text != null) instance.text.text = instance.baseMessage + "...";
        }

        public static void Hide(float fadeSeconds = 0.25f)
        {
            if (instance == null || instance.group == null) return;
            instance.fadeSpeed = fadeSeconds <= 0.01f ? 1000f : 1f / fadeSeconds;
            instance.targetAlpha = 0f;
            instance.group.interactable = false;
            instance.group.blocksRaycasts = false;
        }

        private static void EnsureInstance()
        {
            if (instance != null) return;

            GameObject go = new GameObject("PB_UnifiedLoadingScreen");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<UnifiedLoadingScreen>();
            instance.Build();
        }

        private void Build()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();
            group = gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            GameObject bgGO = new GameObject("FullBlackBackground", typeof(RectTransform));
            bgGO.transform.SetParent(transform, false);
            RectTransform bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            Image bg = bgGO.AddComponent<Image>();
            bg.color = DeepBlack;
            bg.raycastTarget = true;

            GameObject titleGO = new GameObject("LoadingText", typeof(RectTransform));
            titleGO.transform.SetParent(transform, false);
            RectTransform titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(900f, 130f);
            titleRT.anchoredPosition = Vector2.zero;

            text = titleGO.AddComponent<TextMeshProUGUI>();
            text.text = "CARGANDO...";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 52f;
            text.fontStyle = FontStyles.Bold;
            text.characterSpacing = 9f;
            text.color = NeonMagenta;
            text.raycastTarget = false;

            Outline outline = titleGO.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.95f, 1f, 0.38f);
            outline.effectDistance = new Vector2(1.8f, -1.8f);

            Shadow shadow = titleGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(1f, 0.12f, 0.76f, 0.42f);
            shadow.effectDistance = new Vector2(0f, -5f);

            topLine = CreateLine("LoadingTopLine", new Vector2(0f, 72f), new Vector2(620f, 4f), NeonCyan);
            bottomLine = CreateLine("LoadingBottomLine", new Vector2(0f, -72f), new Vector2(620f, 4f), NeonMagenta);
        }

        private Image CreateLine(string name, Vector2 pos, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        private void Update()
        {
            if (group == null) return;

            group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
            if (targetAlpha <= 0f && group.alpha <= 0.001f)
            {
                group.alpha = 0f;
                group.interactable = false;
                group.blocksRaycasts = false;
            }

            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 3.2f);
            int dotCount = 1 + Mathf.FloorToInt((Time.unscaledTime * 2.2f) % 3f);
            if (text != null)
            {
                text.text = baseMessage + new string('.', dotCount);
                text.color = Color.Lerp(NeonMagenta, SoftPurple, pulse * 0.55f);
                float scale = 1f + pulse * 0.025f;
                text.rectTransform.localScale = new Vector3(scale, scale, 1f);
            }
            if (topLine != null) topLine.color = Color.Lerp(NeonCyan, NeonMagenta, pulse * 0.35f);
            if (bottomLine != null) bottomLine.color = Color.Lerp(NeonMagenta, SoftPurple, pulse * 0.45f);
        }
    }
}

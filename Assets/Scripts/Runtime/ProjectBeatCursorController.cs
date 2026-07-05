using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Cursor personalizado para menus de Project Beat.
    /// Avance 56: mejora visual del cursor, feedback de hover y pulso visual al hacer clic.
    /// Solo modifica presentacion; no altera la logica de navegacion ni botones existentes.
    /// </summary>
    public class ProjectBeatCursorController : MonoBehaviour
    {
        private const int RipplePoolSize = 8;
        private const float RippleDuration = 0.32f;
        private const float RippleStartSize = 18f;
        private const float RippleEndSize = 86f;

        private static ProjectBeatCursorController instance;

        private Texture2D normalCursorTexture;
        private Texture2D hoverCursorTexture;
        private bool isHoverCursorApplied;
        private bool isNormalCursorApplied;

        private Canvas clickCanvas;
        private RectTransform clickCanvasRect;
        private readonly List<ClickRipple> ripplePool = new List<ClickRipple>(RipplePoolSize);
        private int nextRippleIndex;
        private readonly List<RaycastResult> raycastResults = new List<RaycastResult>(8);
        private PointerEventData pointerEventData;
        private EventSystem cachedEventSystem;
        private float cursorPulse;

        private struct ClickRipple
        {
            public RectTransform Rect;
            public Image Ring;
            public Image Core;
            public float Timer;
            public bool Active;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (instance != null) return;
            GameObject go = new GameObject("ProjectBeatCursorController");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<ProjectBeatCursorController>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            normalCursorTexture = BuildCursorTexture(false);
            hoverCursorTexture = BuildCursorTexture(true);
            EnsureClickCanvas();
        }

        private void Update()
        {
            bool inMenu = IsInterfaceContextVisible();

            if (inMenu)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                ApplyCursor(IsPointerOverInteractiveUi());

                if (Input.GetMouseButtonDown(0))
                    SpawnClickRipple(Input.mousePosition);

                UpdateRipples();
            }
            else
            {
                Cursor.visible = false;
                HideRipples();
            }
        }

        private bool IsInterfaceContextVisible()
        {
            bool inMenu = StartupFlowController.IsMainMenuVisible;

            PauseMenu pause = FindPauseMenu();
            if (pause != null && pause.IsPausedForOverlay)
                inMenu = true;

            return inMenu;
        }

        private PauseMenu FindPauseMenu()
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<PauseMenu>();
#else
            return Object.FindObjectOfType<PauseMenu>();
#endif
        }

        private void ApplyCursor(bool hover)
        {
            Texture2D selectedTexture = hover ? hoverCursorTexture : normalCursorTexture;
            if (selectedTexture == null) return;

            if (hover)
            {
                if (isHoverCursorApplied) return;
                Cursor.SetCursor(selectedTexture, new Vector2(5f, 5f), CursorMode.Auto);
                isHoverCursorApplied = true;
                isNormalCursorApplied = false;
            }
            else
            {
                if (isNormalCursorApplied) return;
                Cursor.SetCursor(selectedTexture, new Vector2(5f, 5f), CursorMode.Auto);
                isNormalCursorApplied = true;
                isHoverCursorApplied = false;
            }
        }

        private bool IsPointerOverInteractiveUi()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null) return false;

            if (pointerEventData == null || cachedEventSystem != eventSystem)
            {
                pointerEventData = new PointerEventData(eventSystem);
                cachedEventSystem = eventSystem;
            }

            pointerEventData.position = Input.mousePosition;
            raycastResults.Clear();
            eventSystem.RaycastAll(pointerEventData, raycastResults);

            for (int i = 0; i < raycastResults.Count; i++)
            {
                GameObject hit = raycastResults[i].gameObject;
                if (hit == null) continue;
                if (hit.GetComponentInParent<Selectable>() != null) return true;
                if (hit.GetComponentInParent<EventTrigger>() != null) return true;
            }

            return false;
        }

        private void EnsureClickCanvas()
        {
            if (clickCanvas != null) return;

            GameObject canvasGO = new GameObject("ProjectBeatCursorClickFeedbackCanvas");
            DontDestroyOnLoad(canvasGO);

            clickCanvas = canvasGO.AddComponent<Canvas>();
            clickCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            clickCanvas.sortingOrder = 32760;
            clickCanvasRect = canvasGO.GetComponent<RectTransform>();

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GraphicRaycaster raycaster = canvasGO.AddComponent<GraphicRaycaster>();
            raycaster.enabled = false;

            for (int i = 0; i < RipplePoolSize; i++)
                ripplePool.Add(CreateRipple(canvasGO.transform, i));
        }

        private ClickRipple CreateRipple(Transform parent, int index)
        {
            GameObject root = new GameObject("ClickPulse_" + index.ToString("00"));
            root.transform.SetParent(parent, false);

            RectTransform rect = root.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(RippleStartSize, RippleStartSize);

            GameObject ringGO = new GameObject("Ring");
            ringGO.transform.SetParent(root.transform, false);
            Image ring = ringGO.AddComponent<Image>();
            ring.raycastTarget = false;
            ring.sprite = CreateCircleSprite(96, true);
            ring.color = new Color(0f, 0.95f, 1f, 0f);
            RectTransform ringRect = ring.GetComponent<RectTransform>();
            ringRect.anchorMin = Vector2.zero;
            ringRect.anchorMax = Vector2.one;
            ringRect.offsetMin = Vector2.zero;
            ringRect.offsetMax = Vector2.zero;

            GameObject coreGO = new GameObject("Core");
            coreGO.transform.SetParent(root.transform, false);
            Image core = coreGO.AddComponent<Image>();
            core.raycastTarget = false;
            core.sprite = CreateCircleSprite(48, false);
            core.color = new Color(1f, 0.42f, 0.02f, 0f);
            RectTransform coreRect = core.GetComponent<RectTransform>();
            coreRect.anchorMin = coreRect.anchorMax = new Vector2(0.5f, 0.5f);
            coreRect.pivot = new Vector2(0.5f, 0.5f);
            coreRect.sizeDelta = new Vector2(16f, 16f);
            coreRect.anchoredPosition = Vector2.zero;

            root.SetActive(false);

            return new ClickRipple
            {
                Rect = rect,
                Ring = ring,
                Core = core,
                Timer = 0f,
                Active = false
            };
        }

        private void SpawnClickRipple(Vector2 screenPosition)
        {
            EnsureClickCanvas();
            if (clickCanvas == null || ripplePool.Count == 0) return;

            ClickRipple ripple = ripplePool[nextRippleIndex];
            nextRippleIndex = (nextRippleIndex + 1) % ripplePool.Count;

            ripple.Active = true;
            ripple.Timer = 0f;
            ripple.Rect.gameObject.SetActive(true);
            Vector2 localPosition;
            if (clickCanvasRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(clickCanvasRect, screenPosition, null, out localPosition))
                ripple.Rect.anchoredPosition = localPosition;
            else
                ripple.Rect.anchoredPosition = screenPosition;

            ripple.Rect.sizeDelta = new Vector2(RippleStartSize, RippleStartSize);
            ripple.Ring.color = new Color(0f, 0.95f, 1f, 0.86f);
            ripple.Core.color = new Color(1f, 0.42f, 0.02f, 0.75f);

            ripplePool[(nextRippleIndex + RipplePoolSize - 1) % RipplePoolSize] = ripple;
        }

        private void UpdateRipples()
        {
            cursorPulse += Time.unscaledDeltaTime * 7f;

            for (int i = 0; i < ripplePool.Count; i++)
            {
                ClickRipple ripple = ripplePool[i];
                if (!ripple.Active) continue;

                ripple.Timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(ripple.Timer / RippleDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                float size = Mathf.Lerp(RippleStartSize, RippleEndSize, eased);
                float alpha = 1f - t;
                float pulseScale = 1f + Mathf.Sin(cursorPulse) * 0.025f;

                ripple.Rect.sizeDelta = new Vector2(size, size) * pulseScale;
                ripple.Ring.color = new Color(0f, 0.95f, 1f, 0.78f * alpha);
                ripple.Core.color = new Color(1f, 0.42f, 0.02f, 0.55f * alpha);

                if (t >= 1f)
                {
                    ripple.Active = false;
                    ripple.Rect.gameObject.SetActive(false);
                }

                ripplePool[i] = ripple;
            }
        }

        private void HideRipples()
        {
            for (int i = 0; i < ripplePool.Count; i++)
            {
                ClickRipple ripple = ripplePool[i];
                if (ripple.Rect != null)
                    ripple.Rect.gameObject.SetActive(false);
                ripple.Active = false;
                ripple.Timer = 0f;
                ripplePool[i] = ripple;
            }
        }

        private Texture2D BuildCursorTexture(bool hover)
        {
            const int size = 48;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color cyan = hover ? new Color(0.20f, 1f, 1f, 1f) : new Color(0f, 0.92f, 1f, 1f);
            Color blueGlow = new Color(0f, 0.38f, 1f, 0.45f);
            Color purple = hover ? new Color(0.78f, 0.28f, 1f, 1f) : new Color(0.55f, 0.15f, 1f, 1f);
            Color orange = hover ? new Color(1f, 0.58f, 0.08f, 1f) : new Color(1f, 0.42f, 0.02f, 1f);
            Color white = new Color(0.96f, 1f, 1f, 1f);

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

            // Halo neon externo.
            DrawLine(tex, new Vector2Int(5, 5), new Vector2Int(5, 34), blueGlow, 3);
            DrawLine(tex, new Vector2Int(5, 5), new Vector2Int(28, 25), blueGlow, 3);
            DrawLine(tex, new Vector2Int(10, 28), new Vector2Int(18, 42), blueGlow, 3);

            // Silueta principal tipo puntero geometrico.
            DrawLine(tex, new Vector2Int(6, 6), new Vector2Int(6, 34), cyan, 2);
            DrawLine(tex, new Vector2Int(6, 6), new Vector2Int(29, 25), cyan, 2);
            DrawLine(tex, new Vector2Int(7, 34), new Vector2Int(14, 28), cyan, 2);
            DrawLine(tex, new Vector2Int(14, 28), new Vector2Int(21, 43), orange, 2);
            DrawLine(tex, new Vector2Int(21, 43), new Vector2Int(26, 40), orange, 2);
            DrawLine(tex, new Vector2Int(26, 40), new Vector2Int(19, 27), orange, 2);
            DrawLine(tex, new Vector2Int(19, 27), new Vector2Int(29, 25), cyan, 2);

            // Relleno interno con degradado manual.
            for (int y = 9; y <= 29; y++)
            {
                int maxX = Mathf.Clamp(6 + (y - 6), 7, 25);
                for (int x = 8; x <= maxX; x++)
                {
                    float mix = Mathf.InverseLerp(8f, 25f, x);
                    Color fill = Color.Lerp(purple, cyan, mix * 0.45f);
                    fill.a = hover ? 0.95f : 0.82f;
                    tex.SetPixel(x, size - 1 - y, fill);
                }
            }

            // Marca musical pequeña para integrarlo con Project Beat.
            DrawCircle(tex, new Vector2Int(31, 13), hover ? 4 : 3, orange);
            DrawLine(tex, new Vector2Int(34, 13), new Vector2Int(34, 5), orange, 2);
            DrawLine(tex, new Vector2Int(34, 5), new Vector2Int(39, 7), orange, 1);

            // Brillo en la punta.
            DrawCircle(tex, new Vector2Int(7, 7), hover ? 3 : 2, white);

            tex.Apply(false, true);
            return tex;
        }

        private Sprite CreateCircleSprite(int size, bool ringOnly)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color white = Color.white;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.43f;
            float innerRadius = ringOnly ? size * 0.33f : 0f;

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center);
                float outer = Mathf.Clamp01(radius - d + 1.2f);
                float inner = ringOnly ? Mathf.Clamp01(innerRadius - d + 1.2f) : 0f;
                float alpha = ringOnly ? Mathf.Clamp01(outer - inner) : outer;
                tex.SetPixel(x, y, alpha > 0f ? new Color(white.r, white.g, white.b, alpha) : clear);
            }

            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private void DrawLine(Texture2D tex, Vector2Int a, Vector2Int b, Color color, int thickness)
        {
            int dx = Mathf.Abs(b.x - a.x);
            int dy = Mathf.Abs(b.y - a.y);
            int sx = a.x < b.x ? 1 : -1;
            int sy = a.y < b.y ? 1 : -1;
            int err = dx - dy;
            int x = a.x;
            int y = a.y;

            while (true)
            {
                DrawCircle(tex, new Vector2Int(x, y), thickness, color);
                if (x == b.x && y == b.y) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }
            }
        }

        private void DrawCircle(Texture2D tex, Vector2Int center, int radius, Color color)
        {
            int size = tex.width;
            int sqr = radius * radius;
            for (int y = center.y - radius; y <= center.y + radius; y++)
            for (int x = center.x - radius; x <= center.x + radius; x++)
            {
                if (x < 0 || y < 0 || x >= size || y >= size) continue;
                int dx = x - center.x;
                int dy = y - center.y;
                if (dx * dx + dy * dy > sqr) continue;
                tex.SetPixel(x, size - 1 - y, color);
            }
        }
    }
}

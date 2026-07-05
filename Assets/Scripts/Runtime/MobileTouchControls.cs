using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Avance 95 – HUD tactil para Android.
    ///
    /// Este componente se crea solo en ejecucion. No modifica beatmaps, timing,
    /// hit detection, puntuacion ni LevelManager. Los botones tactiles llaman a
    /// LaneInput.HandleVirtualPress/Release para reutilizar la misma logica de
    /// entrada que ya usa el teclado D/F/J/K.
    /// </summary>
    [DefaultExecutionOrder(-25)]
    public sealed class MobileTouchControls : MonoBehaviour
    {
        private const string EditorTestPrefsKey = "ProjectBeat_ShowMobileTouchHUDInEditor";

        private static MobileTouchControls instance;
        private static Sprite roundedSprite;

        private CanvasGroup rootGroup;
        private RectTransform rootRect;
        private GameController gameController;
        private PauseMenu pauseMenu;
        private LaneInput[] lanes;
        private bool visible;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            EnsureInstance();
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureInstance();
            if (instance != null)
                instance.RefreshSceneReferences();
        }

        private static void EnsureInstance()
        {
            if (instance != null) return;

            GameObject go = new GameObject("ProjectBeat_MobileTouchControls");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<MobileTouchControls>();
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
            BuildHud();
            RefreshSceneReferences();
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        private void Update()
        {
            if (gameController == null || pauseMenu == null || lanes == null || lanes.Length == 0)
                RefreshSceneReferences();

            bool shouldShow = ShouldUseTouchHud() &&
                              gameController != null &&
                              gameController.IsGameplayRunning &&
                              (pauseMenu == null || !pauseMenu.IsPaused);

            SetVisible(shouldShow);
        }

        private void OnDisable()
        {
            ForceReleaseAllLanes();
        }

        private static bool ShouldUseTouchHud()
        {
            if (Application.platform == RuntimePlatform.Android)
                return true;

#if UNITY_EDITOR
            return PlayerPrefs.GetInt(EditorTestPrefsKey, 0) == 1;
#else
            return false;
#endif
        }

        private void RefreshSceneReferences()
        {
            gameController = FindObjectOfType<GameController>();
            pauseMenu = FindObjectOfType<PauseMenu>();
            lanes = FindObjectsOfType<LaneInput>();

            if (lanes != null && lanes.Length > 1)
            {
                System.Array.Sort(lanes, (a, b) => a.LaneIndex.CompareTo(b.LaneIndex));
            }
        }

        private void BuildHud()
        {
            EnsureEventSystem();
            EnsureRoundedSprite();

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            rootGroup = gameObject.AddComponent<CanvasGroup>();
            rootRect = transform as RectTransform;

            CreateLaneButton("PB_Touch_D", "D", 0, new Vector2(210f, 142f), TextAnchor.LowerLeft);
            CreateLaneButton("PB_Touch_F", "F", 1, new Vector2(360f, 142f), TextAnchor.LowerLeft);
            CreateLaneButton("PB_Touch_J", "J", 2, new Vector2(-360f, 142f), TextAnchor.LowerRight);
            CreateLaneButton("PB_Touch_K", "K", 3, new Vector2(-210f, 142f), TextAnchor.LowerRight);
            CreatePauseButton();
            CreateHintLabel();
        }

        private void CreateLaneButton(string name, string label, int laneIndex, Vector2 anchoredPosition, TextAnchor anchor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(126f, 126f);
            ApplyAnchor(rt, anchor, anchoredPosition);

            Image image = go.AddComponent<Image>();
            image.sprite = roundedSprite;
            image.color = new Color(0.02f, 0.92f, 1f, 0.34f);
            image.raycastTarget = true;

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.18f, 0.95f, 0.88f);
            outline.effectDistance = new Vector2(3f, -3f);

            TMP_Text text = CreateText(go.transform, "Label", label, 52f, FontStyles.Bold);
            text.color = new Color(1f, 0.96f, 1f, 0.96f);
            text.alignment = TextAlignmentOptions.Center;

            EventTrigger trigger = go.AddComponent<EventTrigger>();
            AddTrigger(trigger, EventTriggerType.PointerDown, data => PressLane(laneIndex));
            AddTrigger(trigger, EventTriggerType.PointerUp, data => ReleaseLane(laneIndex));
            AddTrigger(trigger, EventTriggerType.PointerExit, data => ReleaseLane(laneIndex));
        }

        private void CreatePauseButton()
        {
            GameObject go = new GameObject("PB_Touch_Pause");
            go.transform.SetParent(transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(156f, 70f);
            ApplyAnchor(rt, TextAnchor.UpperRight, new Vector2(-122f, -78f));

            Image image = go.AddComponent<Image>();
            image.sprite = roundedSprite;
            image.color = new Color(0.08f, 0.06f, 0.18f, 0.62f);
            image.raycastTarget = true;

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.42f, 0.02f, 0.90f);
            outline.effectDistance = new Vector2(3f, -3f);

            TMP_Text text = CreateText(go.transform, "Label", "PAUSA", 24f, FontStyles.Bold);
            text.color = new Color(1f, 0.94f, 0.20f, 0.98f);
            text.alignment = TextAlignmentOptions.Center;

            EventTrigger trigger = go.AddComponent<EventTrigger>();
            AddTrigger(trigger, EventTriggerType.PointerClick, data => TogglePause());
        }

        private void CreateHintLabel()
        {
            GameObject go = new GameObject("PB_Touch_Hint");
            go.transform.SetParent(transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(760f, 32f);
            ApplyAnchor(rt, TextAnchor.LowerCenter, new Vector2(0f, 34f));

            TMP_Text text = go.AddComponent<TextMeshProUGUI>();
            text.text = "Controles tactiles Android";
            text.fontSize = 18f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.95f, 0.95f, 1f, 0.58f);
            text.raycastTarget = false;
        }

        private static TMP_Text CreateText(Transform parent, string name, string value, float fontSize, FontStyles style)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            RectTransform rt = textObject.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.enableAutoSizing = false;
            text.raycastTarget = false;
            return text;
        }

        private void PressLane(int laneIndex)
        {
            LaneInput lane = GetLane(laneIndex);
            if (lane != null)
                lane.HandleVirtualPress();
        }

        private void ReleaseLane(int laneIndex)
        {
            LaneInput lane = GetLane(laneIndex);
            if (lane != null)
                lane.HandleVirtualRelease();
        }

        private LaneInput GetLane(int laneIndex)
        {
            if (lanes == null || lanes.Length <= laneIndex || lanes[laneIndex] == null)
                RefreshSceneReferences();

            return lanes != null && lanes.Length > laneIndex ? lanes[laneIndex] : null;
        }

        private void TogglePause()
        {
            if (pauseMenu == null)
                RefreshSceneReferences();

            if (pauseMenu == null)
                return;

            ForceReleaseAllLanes();

            if (pauseMenu.IsPaused)
                pauseMenu.ClosePause();
            else if (gameController != null && gameController.IsGameplayRunning)
                pauseMenu.OpenPause();
        }

        private void ForceReleaseAllLanes()
        {
            if (lanes == null) return;

            for (int i = 0; i < lanes.Length; i++)
                if (lanes[i] != null)
                    lanes[i].ForceReleaseVirtualInput();
        }

        private void SetVisible(bool value)
        {
            if (visible == value && rootGroup != null)
                return;

            visible = value;

            if (rootGroup == null)
                return;

            rootGroup.alpha = value ? 1f : 0f;
            rootGroup.interactable = value;
            rootGroup.blocksRaycasts = value;

            if (!value)
                ForceReleaseAllLanes();
        }

        private static void AddTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(callback);
            trigger.triggers.Add(entry);
        }

        private static void ApplyAnchor(RectTransform rt, TextAnchor anchor, Vector2 anchoredPosition)
        {
            switch (anchor)
            {
                case TextAnchor.LowerLeft:
                    rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    break;
                case TextAnchor.LowerRight:
                    rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    break;
                case TextAnchor.UpperRight:
                    rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    break;
                case TextAnchor.LowerCenter:
                    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    break;
                default:
                    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    break;
            }

            rt.anchoredPosition = anchoredPosition;
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
                return;

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static void EnsureRoundedSprite()
        {
            if (roundedSprite != null) return;

            Texture2D tex = new Texture2D(96, 96, TextureFormat.RGBA32, false);
            tex.name = "PB_MobileTouchButtonSprite";
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color32[] pixels = new Color32[96 * 96];
            float half = 47.5f;
            float radius = 21f;
            float inner = half - radius;

            for (int y = 0; y < 96; y++)
            {
                for (int x = 0; x < 96; x++)
                {
                    float px = Mathf.Abs(x - half);
                    float py = Mathf.Abs(y - half);
                    float dx = Mathf.Max(px - inner, 0f);
                    float dy = Mathf.Max(py - inner, 0f);
                    float distance = Mathf.Sqrt(dx * dx + dy * dy) - radius;
                    float alpha = 1f - Mathf.SmoothStep(-2f, 2f, distance);
                    pixels[y * 96 + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha) * 255f));
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            roundedSprite = Sprite.Create(tex, new Rect(0, 0, 96, 96), new Vector2(0.5f, 0.5f), 96f);
        }
    }
}

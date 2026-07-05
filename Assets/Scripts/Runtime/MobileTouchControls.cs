using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Avance 97 – correccion de mapeo tactil Android y limpieza de pausa movil.
    ///
    /// Mantiene el proyecto en Unity y no cambia beatmaps, timing, hit detection,
    /// puntuacion ni LevelManager. En Android usa lectura directa de multitouch
    /// para D/F/J/K, ordenando las pistas por nombre/posicion para que cada boton
    /// tactil golpee el carril correcto. Se oculta el boton PAUSA movil superior
    /// derecho porque en telefono se montaba sobre el HUD y generaba confusion.
    /// </summary>
    [DefaultExecutionOrder(-25)]
    public sealed class MobileTouchControls : MonoBehaviour
    {
        private const string EditorTestPrefsKey = "ProjectBeat_ShowMobileTouchHUDInEditor";
        private const bool ShowMobilePauseButton = false;

        private static MobileTouchControls instance;
        private static Sprite roundedSprite;

        private CanvasGroup rootGroup;
        private CanvasGroup controlsGroup;
        private CanvasGroup pauseOverlayGroup;
        private RectTransform[] laneButtonRects;
        private RectTransform pauseButtonRect;
        private RectTransform resumeButtonRect;
        private RectTransform restartButtonRect;
        private RectTransform menuButtonRect;

        private readonly bool[] lanePressed = new bool[4];
        private GameController gameController;
        private PauseMenu pauseMenu;
        private LaneInput[] lanes;
        private bool visible;
        private bool mobilePaused;

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
            {
                instance.mobilePaused = false;
                instance.ForceReleaseAllLanes();
                instance.RefreshSceneReferences();
                instance.SetPauseOverlayVisible(false);
            }
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
            SetPauseOverlayVisible(false);
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        private void Update()
        {
            if (ShouldUseTouchHud())
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.None;
            }

            if (gameController == null || lanes == null || !HasCompleteLaneOrder(lanes))
                RefreshSceneReferences();

            bool canShow = ShouldUseTouchHud() &&
                           gameController != null &&
                           gameController.IsGameplayRunning &&
                           (pauseMenu == null || !pauseMenu.IsPaused || mobilePaused);

            SetVisible(canShow);

            if (!canShow)
            {
                if (mobilePaused)
                    CloseMobilePause(false);
                return;
            }

            SetControlsVisible(!mobilePaused);
            SetPauseOverlayVisible(mobilePaused);

            if (Application.platform == RuntimePlatform.Android)
                PollAndroidTouches();
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
            lanes = ResolveOrderedLanes();
        }

        private static LaneInput[] ResolveOrderedLanes()
        {
            LaneInput[] found = FindObjectsOfType<LaneInput>();
            LaneInput[] ordered = new LaneInput[4];

            if (found == null || found.Length == 0)
                return ordered;

            // Primer intento: usar el nombre del GameObject (Lane_0, Lane_1, Lane_2, Lane_3).
            // Esto evita el bug de Android donde el HUD tactil podia tomar el orden interno
            // de FindObjectsOfType antes de que GameController inicializara los laneIndex.
            foreach (LaneInput lane in found)
            {
                int index = TryReadLaneIndexFromName(lane != null ? lane.gameObject.name : null);
                if (index >= 0 && index < ordered.Length && ordered[index] == null)
                    ordered[index] = lane;
            }

            if (HasCompleteLaneOrder(ordered))
                return ordered;

            // Segundo intento: usar LaneIndex si ya fue inicializado por GameController.
            System.Array.Clear(ordered, 0, ordered.Length);
            foreach (LaneInput lane in found)
            {
                if (lane == null) continue;
                int index = lane.LaneIndex;
                if (index >= 0 && index < ordered.Length && ordered[index] == null)
                    ordered[index] = lane;
            }

            if (HasCompleteLaneOrder(ordered))
                return ordered;

            // Fallback final: izquierda a derecha segun posicion X.
            System.Array.Sort(found, (a, b) =>
            {
                float ax = a != null ? a.transform.position.x : 0f;
                float bx = b != null ? b.transform.position.x : 0f;
                return ax.CompareTo(bx);
            });

            for (int i = 0; i < ordered.Length && i < found.Length; i++)
                ordered[i] = found[i];

            return ordered;
        }

        private static bool HasCompleteLaneOrder(LaneInput[] ordered)
        {
            if (ordered == null || ordered.Length < 4)
                return false;

            for (int i = 0; i < 4; i++)
                if (ordered[i] == null)
                    return false;

            return true;
        }

        private static int TryReadLaneIndexFromName(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
                return -1;

            int underscore = objectName.LastIndexOf('_');
            if (underscore < 0 || underscore >= objectName.Length - 1)
                return -1;

            int value;
            return int.TryParse(objectName.Substring(underscore + 1), out value) ? value : -1;
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
            laneButtonRects = new RectTransform[4];

            GameObject controls = new GameObject("PB_Mobile_ControlsLayer", typeof(RectTransform));
            controls.transform.SetParent(transform, false);
            RectTransform controlsRT = controls.GetComponent<RectTransform>();
            controlsRT.anchorMin = Vector2.zero;
            controlsRT.anchorMax = Vector2.one;
            controlsRT.offsetMin = Vector2.zero;
            controlsRT.offsetMax = Vector2.zero;
            controlsGroup = controls.AddComponent<CanvasGroup>();

            laneButtonRects[0] = CreateLaneButton(controls.transform, "PB_Touch_D", "D", 0, new Vector2(210f, 108f), TextAnchor.LowerLeft);
            laneButtonRects[1] = CreateLaneButton(controls.transform, "PB_Touch_F", "F", 1, new Vector2(348f, 108f), TextAnchor.LowerLeft);
            laneButtonRects[2] = CreateLaneButton(controls.transform, "PB_Touch_J", "J", 2, new Vector2(-348f, 108f), TextAnchor.LowerRight);
            laneButtonRects[3] = CreateLaneButton(controls.transform, "PB_Touch_K", "K", 3, new Vector2(-210f, 108f), TextAnchor.LowerRight);

            if (ShowMobilePauseButton)
            {
                pauseButtonRect = CreatePauseButton(controls.transform);
                BuildMobilePauseOverlay();
            }

            CreateHintLabel(controls.transform);
        }

        private RectTransform CreateLaneButton(Transform parent, string name, string label, int laneIndex, Vector2 anchoredPosition, TextAnchor anchor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(108f, 108f);
            ApplyAnchor(rt, anchor, anchoredPosition);

            Image image = go.AddComponent<Image>();
            image.sprite = roundedSprite;
            image.color = new Color(0.55f, 0.42f, 1f, 0.84f);
            image.raycastTarget = true;

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.18f, 0.95f, 0.88f);
            outline.effectDistance = new Vector2(3f, -3f);

            TMP_Text text = CreateText(go.transform, "Label", label, 46f, FontStyles.Bold);
            text.color = new Color(1f, 0.96f, 1f, 0.96f);
            text.alignment = TextAlignmentOptions.Center;

            // En Android se usa PollAndroidTouches() para multitouch real.
            // En editor/Windows esto permite probar con mouse sin activar Android.
            if (Application.platform != RuntimePlatform.Android)
            {
                EventTrigger trigger = go.AddComponent<EventTrigger>();
                AddTrigger(trigger, EventTriggerType.PointerDown, data => PressLane(laneIndex));
                AddTrigger(trigger, EventTriggerType.PointerUp, data => ReleaseLane(laneIndex));
                AddTrigger(trigger, EventTriggerType.PointerExit, data => ReleaseLane(laneIndex));
            }

            return rt;
        }

        private RectTransform CreatePauseButton(Transform parent)
        {
            GameObject go = new GameObject("PB_Touch_Pause_Mobile");
            go.transform.SetParent(parent, false);

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

            if (Application.platform != RuntimePlatform.Android)
            {
                EventTrigger trigger = go.AddComponent<EventTrigger>();
                AddTrigger(trigger, EventTriggerType.PointerClick, data => ToggleMobilePause());
            }

            return rt;
        }

        private void CreateHintLabel(Transform parent)
        {
            GameObject go = new GameObject("PB_Touch_Hint");
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(760f, 30f);
            ApplyAnchor(rt, TextAnchor.LowerCenter, new Vector2(0f, 20f));

            TMP_Text text = go.AddComponent<TextMeshProUGUI>();
            text.text = "Controles tactiles Android";
            text.fontSize = 16f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.95f, 0.95f, 1f, 0.58f);
            text.raycastTarget = false;
        }

        private void BuildMobilePauseOverlay()
        {
            GameObject overlay = new GameObject("PB_MobilePauseOverlay", typeof(RectTransform));
            overlay.transform.SetParent(transform, false);

            RectTransform overlayRT = overlay.GetComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;

            pauseOverlayGroup = overlay.AddComponent<CanvasGroup>();

            Image dim = new GameObject("PB_MobilePause_Dim", typeof(RectTransform)).AddComponent<Image>();
            dim.transform.SetParent(overlay.transform, false);
            dim.color = new Color(0f, 0f, 0f, 0.56f);
            dim.raycastTarget = true;
            RectTransform dimRT = dim.GetComponent<RectTransform>();
            dimRT.anchorMin = Vector2.zero;
            dimRT.anchorMax = Vector2.one;
            dimRT.offsetMin = Vector2.zero;
            dimRT.offsetMax = Vector2.zero;

            GameObject cardGO = new GameObject("PB_MobilePause_Card", typeof(RectTransform));
            cardGO.transform.SetParent(overlay.transform, false);
            RectTransform card = cardGO.GetComponent<RectTransform>();
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.anchoredPosition = Vector2.zero;
            card.sizeDelta = new Vector2(720f, 500f);
            Image cardImg = cardGO.AddComponent<Image>();
            cardImg.sprite = roundedSprite;
            cardImg.type = Image.Type.Sliced;
            cardImg.color = new Color(0.035f, 0.045f, 0.095f, 0.98f);
            cardImg.raycastTarget = true;
            Outline cardOutline = cardGO.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0f, 0.92f, 1f, 0.70f);
            cardOutline.effectDistance = new Vector2(3f, -3f);

            TMP_Text title = CreateText(cardGO.transform, "Title", "PAUSA", 48f, FontStyles.Bold);
            title.color = new Color(1f, 0.94f, 0.20f, 1f);
            title.alignment = TextAlignmentOptions.Center;
            RectTransform titleRT = title.GetComponent<RectTransform>();
            titleRT.anchorMin = titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(520f, 70f);
            titleRT.anchoredPosition = new Vector2(0f, 175f);

            TMP_Text sub = CreateText(cardGO.transform, "Subtitle", "CONTROLES MOVILES", 20f, FontStyles.Bold);
            sub.color = new Color(0f, 0.92f, 1f, 0.95f);
            sub.alignment = TextAlignmentOptions.Center;
            RectTransform subRT = sub.GetComponent<RectTransform>();
            subRT.anchorMin = subRT.anchorMax = new Vector2(0.5f, 0.5f);
            subRT.pivot = new Vector2(0.5f, 0.5f);
            subRT.sizeDelta = new Vector2(520f, 36f);
            subRT.anchoredPosition = new Vector2(0f, 124f);

            resumeButtonRect = CreatePauseOverlayButton(cardGO.transform, "PB_MobilePause_Resume", "CONTINUAR", new Vector2(0f, 45f), ResumeMobileGameplay, new Color(0.10f, 0.22f, 0.24f, 0.96f));
            restartButtonRect = CreatePauseOverlayButton(cardGO.transform, "PB_MobilePause_Restart", "REINICIAR", new Vector2(0f, -55f), RestartMobileLevel, new Color(0.16f, 0.08f, 0.22f, 0.96f));
            menuButtonRect = CreatePauseOverlayButton(cardGO.transform, "PB_MobilePause_Menu", "MENU PRINCIPAL", new Vector2(0f, -155f), ReturnMobileToMainMenu, new Color(0.19f, 0.07f, 0.10f, 0.96f));
        }

        private RectTransform CreatePauseOverlayButton(Transform parent, string name, string label, Vector2 position, System.Action action, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(460f, 74f);

            Image image = go.AddComponent<Image>();
            image.sprite = roundedSprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = true;

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.18f, 0.90f, 0.70f);
            outline.effectDistance = new Vector2(2.5f, -2.5f);

            TMP_Text text = CreateText(go.transform, "Label", label, 25f, FontStyles.Bold);
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(1f, 0.95f, 1f, 0.98f);

            if (Application.platform != RuntimePlatform.Android)
            {
                EventTrigger trigger = go.AddComponent<EventTrigger>();
                AddTrigger(trigger, EventTriggerType.PointerClick, data => action?.Invoke());
            }
            return rt;
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

        private void PollAndroidTouches()
        {
            if (Input.touchCount <= 0)
            {
                if (!mobilePaused)
                    ReleaseAllLaneTouchStates();
                return;
            }

            if (mobilePaused)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase != TouchPhase.Began)
                        continue;

                    Vector2 pos = touch.position;
                    if (ContainsScreenPoint(resumeButtonRect, pos)) { ResumeMobileGameplay(); return; }
                    if (ContainsScreenPoint(restartButtonRect, pos)) { RestartMobileLevel(); return; }
                    if (ContainsScreenPoint(menuButtonRect, pos)) { ReturnMobileToMainMenu(); return; }
                }
                return;
            }

            bool pausePressedThisFrame = false;
            bool[] touching = new bool[4];

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                Vector2 pos = touch.position;

                if (ShowMobilePauseButton && touch.phase == TouchPhase.Began && ContainsScreenPoint(pauseButtonRect, pos))
                    pausePressedThisFrame = true;

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    continue;

                for (int lane = 0; lane < 4; lane++)
                {
                    if (ContainsScreenPoint(laneButtonRects != null ? laneButtonRects[lane] : null, pos))
                        touching[lane] = true;
                }
            }

            if (ShowMobilePauseButton && pausePressedThisFrame)
            {
                ToggleMobilePause();
                return;
            }

            for (int lane = 0; lane < 4; lane++)
            {
                if (touching[lane] && !lanePressed[lane])
                {
                    lanePressed[lane] = true;
                    PressLane(lane);
                }
                else if (!touching[lane] && lanePressed[lane])
                {
                    lanePressed[lane] = false;
                    ReleaseLane(lane);
                }
            }
        }

        private static bool ContainsScreenPoint(RectTransform rect, Vector2 screenPoint)
        {
            if (rect == null || !rect.gameObject.activeInHierarchy)
                return false;
            return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, null);
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
            if (lanes == null || lanes.Length <= laneIndex || lanes[laneIndex] == null || !HasCompleteLaneOrder(lanes))
                RefreshSceneReferences();

            return lanes != null && lanes.Length > laneIndex ? lanes[laneIndex] : null;
        }

        private void ToggleMobilePause()
        {
            if (mobilePaused)
                ResumeMobileGameplay();
            else
                OpenMobilePause();
        }

        private void OpenMobilePause()
        {
            if (mobilePaused)
                return;

            mobilePaused = true;
            ForceReleaseAllLanes();
            Time.timeScale = 0f;
            if (gameController != null)
                gameController.PauseAudio(true);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
            SetControlsVisible(false);
            SetPauseOverlayVisible(true);
        }

        private void ResumeMobileGameplay()
        {
            CloseMobilePause(true);
        }

        private void CloseMobilePause(bool resumeAudio)
        {
            if (!mobilePaused && !resumeAudio)
                return;

            mobilePaused = false;
            SetPauseOverlayVisible(false);
            SetControlsVisible(visible);
            Time.timeScale = 1f;
            if (resumeAudio && gameController != null)
                gameController.PauseAudio(false);
            ForceReleaseAllLanes();
            Cursor.visible = false;
        }

        private void RestartMobileLevel()
        {
            mobilePaused = false;
            ForceReleaseAllLanes();
            Time.timeScale = 1f;
            if (gameController != null)
                gameController.PauseAudio(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }

        private void ReturnMobileToMainMenu()
        {
            mobilePaused = false;
            ForceReleaseAllLanes();
            Time.timeScale = 1f;
            if (gameController != null)
                gameController.PauseAudio(false);
            if (pauseMenu != null)
                pauseMenu.ForceCloseWithoutResumeCountdown();
            StartupFlowController.RequestMainMenuOnNextLoad();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }

        private void ForceReleaseAllLanes()
        {
            ReleaseAllLaneTouchStates();

            if (lanes == null) return;

            for (int i = 0; i < lanes.Length; i++)
                if (lanes[i] != null)
                    lanes[i].ForceReleaseVirtualInput();
        }

        private void ReleaseAllLaneTouchStates()
        {
            for (int i = 0; i < lanePressed.Length; i++)
            {
                if (lanePressed[i])
                {
                    lanePressed[i] = false;
                    ReleaseLane(i);
                }
            }
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
            {
                ForceReleaseAllLanes();
                SetPauseOverlayVisible(false);
            }
        }

        private void SetControlsVisible(bool value)
        {
            if (controlsGroup == null)
                return;

            controlsGroup.alpha = value ? 1f : 0f;
            controlsGroup.interactable = value;
            controlsGroup.blocksRaycasts = value;
        }

        private void SetPauseOverlayVisible(bool value)
        {
            if (pauseOverlayGroup == null)
                return;

            pauseOverlayGroup.alpha = value ? 1f : 0f;
            pauseOverlayGroup.interactable = value;
            pauseOverlayGroup.blocksRaycasts = value;
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

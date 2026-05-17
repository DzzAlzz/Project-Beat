using System.IO;
using ProjectBeat.Runtime;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectBeat.Editor
{
    [InitializeOnLoad]
    public static class DemoSceneBootstrap
    {
        static DemoSceneBootstrap()
        {
            EditorApplication.delayCall += () =>
            {
                if (File.Exists(DemoSceneBuilder.ScenePath)) return;
                bool build = EditorUtility.DisplayDialog(
                    "Project Beat",
                    "No se encontró la escena. ¿Generarla automáticamente?",
                    "Sí, crear escena", "No");
                if (build) DemoSceneBuilder.BuildDemoScene();
            };
        }
    }

    public static class DemoSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/ProjectBeat_Demo.unity";

        [MenuItem("Project Beat/Build Demo Scene")]
        public static void BuildDemoScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ProjectBeat_Demo";

            // ── Sprite import ────────────────────────────────────────────────
            EnsureSprite("Assets/Art/Backgrounds/bg_purple_wave.jpg");
            EnsureSprite("Assets/Art/Backgrounds/bg_red_circuit.png");
            EnsureSprite("Assets/Art/Backgrounds/bg_green_grid.png");
            EnsureSprite("Assets/Art/Sprites/note.png");
            EnsureSprite("Assets/Art/Sprites/lane.png");
            EnsureSprite("Assets/Art/Sprites/hitline.png");
            EnsureSprite("Assets/Art/Sprites/laneGlow.png");
            EnsureSprite("Assets/Art/Sprites/circle.png");

            // ── Camera ───────────────────────────────────────────────────────
            GameObject camGO = new GameObject("Main Camera");
            Camera cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            // Deep warm-dark background for ACELERADA / NeonOrange
            cam.backgroundColor = new Color(0.05f, 0.02f, 0.00f);
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0f, 0f, -10f);
            camGO.AddComponent<AudioListener>(); // sin AudioListener no hay audio en la escena

            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // ── Root ─────────────────────────────────────────────────────────
            GameObject root   = new GameObject("ProjectBeat");
            GameObject bgRoot = new GameObject("Background");
            bgRoot.transform.SetParent(root.transform);

            // ── Background layers — all with LOW alpha for visibility ─────────
            // bg_purple_wave tinted warm orange, very dim base layer
            var bg1  = CreateSprite("BG_Base",    "Assets/Art/Backgrounds/bg_purple_wave.jpg",  new Vector3(0f,  0f,    5f), new Vector3(11.2f, 6.2f, 1f), 0.14f, bgRoot.transform);
            // Circuit overlay — warm orange tint, barely visible
            var bg2a = CreateSprite("BG_Circ_A",  "Assets/Art/Backgrounds/bg_red_circuit.png",  new Vector3(0f,  0f,    4f), new Vector3(11.4f, 6.0f, 1f), 0.07f, bgRoot.transform);
            var bg2b = CreateSprite("BG_Circ_B",  "Assets/Art/Backgrounds/bg_red_circuit.png",  new Vector3(6f,  0f,    4f), new Vector3(11.4f, 6.0f, 1f), 0.07f, bgRoot.transform);
            // Grid floor — only at bottom half, very dim
            var bg3a = CreateSprite("BG_Grid_A",  "Assets/Art/Backgrounds/bg_green_grid.png",   new Vector3(0f, -2.6f, 3f), new Vector3(10.5f, 3.0f, 1f), 0.05f, bgRoot.transform);
            var bg3b = CreateSprite("BG_Grid_B",  "Assets/Art/Backgrounds/bg_green_grid.png",   new Vector3(6f, -2.6f, 3f), new Vector3(10.5f, 3.0f, 1f), 0.05f, bgRoot.transform);

            // Tint overlays orange
            TintSprite(bg2a, new Color(1f, 0.5f, 0.1f, 0.07f));
            TintSprite(bg2b, new Color(1f, 0.5f, 0.1f, 0.07f));

            // ── Neon Wave — placed at BOTTOM of screen, thin and subtle ──────
            // waveBaseY = -5.0 in NeonBackgroundController; we position the GO there
            GameObject waveGO = new GameObject("NeonWave");
            waveGO.transform.SetParent(bgRoot.transform);
            waveGO.transform.position = new Vector3(0f, -5.0f, 1f);  // bottom of screen
            LineRenderer line = waveGO.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            // Vivid orange/gold wave, semi-transparent
            line.startColor     = new Color(1f, 0.50f, 0.00f, 0.45f);
            line.endColor       = new Color(1f, 0.85f, 0.15f, 0.45f);
            line.widthMultiplier = 0.06f;
            line.numCapVertices = 8;
            line.sortingOrder   = -1;   // behind everything

            // ── BackgroundThemeController ────────────────────────────────────
            BackgroundThemeController bgTheme = bgRoot.AddComponent<BackgroundThemeController>();
            var bgThemeSO = new SerializedObject(bgTheme);
            bgThemeSO.FindProperty("bgBase").objectReferenceValue      = bg1.GetComponent<SpriteRenderer>();
            bgThemeSO.FindProperty("bgOverlayA").objectReferenceValue  = bg2a.GetComponent<SpriteRenderer>();
            bgThemeSO.FindProperty("bgOverlayB").objectReferenceValue  = bg2b.GetComponent<SpriteRenderer>();
            bgThemeSO.FindProperty("bgFloorA").objectReferenceValue    = bg3a.GetComponent<SpriteRenderer>();
            bgThemeSO.FindProperty("bgFloorB").objectReferenceValue    = bg3b.GetComponent<SpriteRenderer>();
            bgThemeSO.FindProperty("neonWave").objectReferenceValue    = line;
            bgThemeSO.ApplyModifiedPropertiesWithoutUndo();

            // ── Playfield ────────────────────────────────────────────────────
            GameObject playfield = new GameObject("Playfield");
            playfield.transform.SetParent(root.transform);

            // Lane separators — vertical glowing lines between lanes
            for (int s = 0; s < 3; s++)
            {
                float sx = -1.2f + s * 1.2f;
                GameObject sep = new GameObject($"LaneSep_{s}");
                sep.transform.SetParent(playfield.transform);
                sep.transform.position = new Vector3(sx, 1.2f, 0.5f);
                LineRenderer sepLine = sep.AddComponent<LineRenderer>();
                sepLine.material = new Material(Shader.Find("Sprites/Default"));
                sepLine.startColor = new Color(1f, 0.6f, 0.1f, 0.18f);
                sepLine.endColor   = new Color(1f, 0.6f, 0.1f, 0.04f);
                sepLine.widthMultiplier = 0.018f;
                sepLine.positionCount   = 2;
                sepLine.sortingOrder    = 1;
                sepLine.SetPosition(0, new Vector3(sx, 5.5f, 0f));
                sepLine.SetPosition(1, new Vector3(sx, -3.2f, 0f));
            }

            float[] laneX = { -1.8f, -0.6f, 0.6f, 1.8f };
            LaneInput[]  lanes  = new LaneInput[4];
            Transform[]  spawns = new Transform[4];
            Transform[]  hits   = new Transform[4];

            // ACELERADA orange palette per lane
            Color[] aceleradaColors = {
                new Color(1.00f, 0.55f, 0.05f),
                new Color(1.00f, 0.35f, 0.05f),
                new Color(1.00f, 0.80f, 0.10f),
                new Color(1.00f, 0.30f, 0.00f)
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject laneGO = new GameObject($"Lane_{i}");
                laneGO.transform.SetParent(playfield.transform);
                CreateSpriteChild(laneGO.transform, "LaneBody",  "Assets/Art/Sprites/lane.png",    Vector3.zero,         Vector3.one, new Color(1f, 1f, 1f, 0.40f), 2);
                var glowGO = CreateSpriteChild(laneGO.transform, "Glow", "Assets/Art/Sprites/laneGlow.png", new Vector3(0f, -3.2f, 0f), Vector3.one, new Color(aceleradaColors[i].r, aceleradaColors[i].g, aceleradaColors[i].b, 0.09f), 5);
                laneGO.transform.position = new Vector3(laneX[i], 0f, 0f);
                lanes[i] = laneGO.AddComponent<LaneInput>();
                var lso = new SerializedObject(lanes[i]);
                lso.FindProperty("glowRenderer").objectReferenceValue = glowGO.GetComponent<SpriteRenderer>();
                lso.ApplyModifiedPropertiesWithoutUndo();

                var spawn = new GameObject("SpawnPoint");
                spawn.transform.SetParent(laneGO.transform);
                spawn.transform.position = new Vector3(laneX[i], 5.5f, 0f);
                spawns[i] = spawn.transform;

                var hitPt = new GameObject("HitPoint");
                hitPt.transform.SetParent(laneGO.transform);
                hitPt.transform.position = new Vector3(laneX[i], -3.2f, 0f);
                hits[i] = hitPt.transform;
            }

            // Hit line — brighter, orange tint
            GameObject hitLine = new GameObject("HitLine");
            hitLine.transform.SetParent(playfield.transform);
            var hlSR = hitLine.AddComponent<SpriteRenderer>();
            hlSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/hitline.png");
            hlSR.color  = new Color(1f, 0.7f, 0.2f, 0.95f);
            hlSR.sortingOrder = 6;
            hitLine.transform.position   = new Vector3(0f, -3.2f, 0f);
            hitLine.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            // Note template
            GameObject noteTemplate = new GameObject("NoteTemplate");
            var noteSR = noteTemplate.AddComponent<SpriteRenderer>();
            noteSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/note.png");
            noteSR.sortingOrder = 10;
            noteTemplate.AddComponent<NoteObject>();
            noteTemplate.transform.position = new Vector3(1000f, 1000f, 0f);

            // Hit FX template
            GameObject hitFxTemplate = new GameObject("HitFXTemplate");
            var fxSR = hitFxTemplate.AddComponent<SpriteRenderer>();
            fxSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/circle.png");
            fxSR.sortingOrder = 12;
            hitFxTemplate.AddComponent<HitEffect>();
            hitFxTemplate.transform.position = new Vector3(1000f, 1002f, 0f);

            // ── Managers ─────────────────────────────────────────────────────
            GameObject managers = new GameObject("Managers");
            managers.transform.SetParent(root.transform);

            // Conductor se agrega primero — su [RequireComponent(typeof(AudioSource))]
            // crea automáticamente el AudioSource. Luego configuramos ese componente.
            // (Agregar AudioSource manualmente antes generaba un duplicado conflictivo.)
            Conductor conductor = managers.AddComponent<Conductor>();
            AudioSource audioSource = managers.GetComponent<AudioSource>();
            audioSource.clip        = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ACELERADA.mp3");
            audioSource.playOnAwake = false;
            audioSource.loop        = false;
            audioSource.volume      = 1f;
            audioSource.mute        = false;
            audioSource.spatialBlend = 0f; // audio 2D, sin atenuación por distancia
            audioSource.priority    = 0;   // máxima prioridad
            var condSO = new SerializedObject(conductor);
            condSO.FindProperty("audioSource").objectReferenceValue = audioSource;
            condSO.ApplyModifiedPropertiesWithoutUndo();

            ScoreManager scoreManager = managers.AddComponent<ScoreManager>();
            BeatmapPlayer player      = managers.AddComponent<BeatmapPlayer>();
            GameController gc         = managers.AddComponent<GameController>();

            // LevelManager
            GameObject lmGO = new GameObject("LevelManager");
            lmGO.transform.SetParent(root.transform);
            LevelManager lm = lmGO.AddComponent<LevelManager>();

            // NeonBackgroundController
            NeonBackgroundController bgCtrl = bgRoot.AddComponent<NeonBackgroundController>();
            var bgSO = new SerializedObject(bgCtrl);
            bgSO.FindProperty("layers").arraySize = 4;
            bgSO.FindProperty("layers").GetArrayElementAtIndex(0).objectReferenceValue = bg2a.transform;
            bgSO.FindProperty("layers").GetArrayElementAtIndex(1).objectReferenceValue = bg2b.transform;
            bgSO.FindProperty("layers").GetArrayElementAtIndex(2).objectReferenceValue = bg3a.transform;
            bgSO.FindProperty("layers").GetArrayElementAtIndex(3).objectReferenceValue = bg3b.transform;
            bgSO.FindProperty("speeds").arraySize = 4;
            bgSO.FindProperty("speeds").GetArrayElementAtIndex(0).floatValue = 0.10f;
            bgSO.FindProperty("speeds").GetArrayElementAtIndex(1).floatValue = 0.10f;
            bgSO.FindProperty("speeds").GetArrayElementAtIndex(2).floatValue = 0.14f;
            bgSO.FindProperty("speeds").GetArrayElementAtIndex(3).floatValue = 0.14f;
            bgSO.FindProperty("waveRenderer").objectReferenceValue = line;
            bgSO.FindProperty("conductor").objectReferenceValue    = conductor;
            bgSO.ApplyModifiedPropertiesWithoutUndo();

            // ── UI ───────────────────────────────────────────────────────────
            GameplayUI gameplayUI = null;
            PauseMenu  pauseMenu  = null;
            CreateUI(root.transform, out gameplayUI, out pauseMenu, gc);

            // ── Wire BeatmapPlayer ───────────────────────────────────────────
            var playerSO = new SerializedObject(player);
            playerSO.FindProperty("controller").objectReferenceValue   = gc;
            playerSO.FindProperty("beatmapJson").objectReferenceValue  = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Beatmaps/acelerada.json");
            playerSO.FindProperty("songOverride").objectReferenceValue = audioSource.clip;
            playerSO.ApplyModifiedPropertiesWithoutUndo();

            // ── Wire GameController ──────────────────────────────────────────
            var gcSO = new SerializedObject(gc);
            gcSO.FindProperty("conductor").objectReferenceValue              = conductor;
            gcSO.FindProperty("beatmapPlayer").objectReferenceValue          = player;
            gcSO.FindProperty("scoreManager").objectReferenceValue           = scoreManager;
            gcSO.FindProperty("gameplayUI").objectReferenceValue             = gameplayUI;
            gcSO.FindProperty("pauseMenu").objectReferenceValue              = pauseMenu;
            gcSO.FindProperty("backgroundThemeController").objectReferenceValue = bgTheme;
            gcSO.FindProperty("beatmapJson").objectReferenceValue            = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Beatmaps/acelerada.json");
            gcSO.FindProperty("songOverride").objectReferenceValue           = audioSource.clip;
            gcSO.FindProperty("notePrefab").objectReferenceValue             = noteTemplate;
            gcSO.FindProperty("hitEffectPrefab").objectReferenceValue        = hitFxTemplate;
            gcSO.FindProperty("lanes").arraySize           = 4;
            gcSO.FindProperty("laneSpawnPoints").arraySize = 4;
            gcSO.FindProperty("laneHitPoints").arraySize   = 4;
            for (int i = 0; i < 4; i++)
            {
                gcSO.FindProperty("lanes").GetArrayElementAtIndex(i).objectReferenceValue           = lanes[i];
                gcSO.FindProperty("laneSpawnPoints").GetArrayElementAtIndex(i).objectReferenceValue = spawns[i];
                gcSO.FindProperty("laneHitPoints").GetArrayElementAtIndex(i).objectReferenceValue   = hits[i];
            }
            gcSO.ApplyModifiedPropertiesWithoutUndo();

            var pmSO2 = new SerializedObject(pauseMenu);
            pmSO2.FindProperty("gameController").objectReferenceValue = gc;
            pmSO2.ApplyModifiedPropertiesWithoutUndo();

            // ── LevelManager — todos los beatmaps disponibles ───────────────
            string levelFolder = "Assets/Levels";
            if (!Directory.Exists(levelFolder)) Directory.CreateDirectory(levelFolder);

            // Definicion de cada nivel: archivo JSON, audio, nombre, artista, tema, colores
            var levelDefs = new (string bmap, string audio, bool isWav, string name, string artist, BackgroundTheme theme, Color[] cols)[]
            {
                ("tutorial",        "projectbeat_demo", false, "TUTORIAL",        "Nivel 0", BackgroundTheme.TutorialGray,
                    new[]{ new Color(0.68f,0.74f,0.82f), new Color(0.55f,0.62f,0.70f), new Color(0.86f,0.88f,0.92f), new Color(0.42f,0.48f,0.56f) }),
                ("acelerada",       "ACELERADA",        false, "ACELERADA",       "Nivel 1", BackgroundTheme.NeonOrange,
                    new[]{ new Color(1.00f,0.55f,0.05f), new Color(1.00f,0.35f,0.05f), new Color(1.00f,0.80f,0.10f), new Color(1.00f,0.30f,0.00f) }),
                ("level2_ritmofunk", "RitmoFunk_Level2", false, "RITMO FUNK",     "Nivel 2", BackgroundTheme.NeonGreen,
                    new[]{ new Color(0.00f,1.00f,0.35f), new Color(0.60f,1.00f,0.10f), new Color(0.00f,0.85f,1.00f), new Color(0.85f,1.00f,0.00f) }),
                ("summer_vacation", "Summer Vacation",  false, "SUMMER VACATION", "Nivel 3", BackgroundTheme.NeonBlue,
                    new[]{ new Color(0.00f,0.95f,1.00f), new Color(1.00f,0.82f,0.12f), new Color(1.00f,0.25f,0.78f), new Color(0.25f,1.00f,0.45f) }),
                ("estrelar",        "Estrelar",         false, "ESTRELAR",        "Nivel 4", BackgroundTheme.NeonWhite,
                    new[]{ new Color(0.05f,0.65f,1.00f), new Color(0.25f,0.85f,1.00f), new Color(0.78f,0.88f,0.95f), new Color(0.00f,0.35f,0.75f) }),
                ("frontlines",       "Frontlines",       false, "FRONTLINES",       "Nivel 5", BackgroundTheme.NeonRedBlack,
                    new[]{ new Color(1.00f,0.05f,0.05f), new Color(0.85f,0.00f,0.02f), new Color(1.00f,0.30f,0.05f), new Color(0.55f,0.00f,0.00f) }),
                ("requiem",          "Requiem",          false, "REQUIEM",          "Nivel 6", BackgroundTheme.NeonBossRequiem,
                    new[]{ new Color(0.95f,0.10f,1.00f), new Color(0.55f,0.00f,0.95f), new Color(1.00f,1.00f,1.00f), new Color(0.65f,0.00f,0.12f) }),
            };

            var allLevels = new System.Collections.Generic.List<LevelData>();
            foreach (var def in levelDefs)
            {
                string jsonPath  = $"Assets/Beatmaps/{def.bmap}.json";
                string audioExt  = def.isWav ? ".wav" : ".mp3";
                string audioPath = $"Assets/Audio/{def.audio}{audioExt}";

                TextAsset bjson = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);
                AudioClip bclip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
                if (bjson == null || bclip == null)
                {
                    Debug.LogWarning($"[LevelManager] Saltando {def.name}: no se encontraron assets ({jsonPath}, {audioPath})");
                    continue;
                }

                string ldAssetPath = $"{levelFolder}/Level_{def.bmap}.asset";
                LevelData ld2 = AssetDatabase.LoadAssetAtPath<LevelData>(ldAssetPath);
                if (ld2 == null)
                {
                    ld2 = ScriptableObject.CreateInstance<LevelData>();
                    AssetDatabase.CreateAsset(ld2, ldAssetPath);
                }
                var ld2SO = new SerializedObject(ld2);
                ld2SO.FindProperty("levelName").stringValue            = def.name;
                ld2SO.FindProperty("artistName").stringValue           = def.artist;
                ld2SO.FindProperty("beatmapJson").objectReferenceValue = bjson;
                ld2SO.FindProperty("audioClip").objectReferenceValue   = bclip;
                ld2SO.FindProperty("backgroundTheme").enumValueIndex   = (int)def.theme;
                var lc2 = ld2SO.FindProperty("laneColors");
                lc2.arraySize = 4;
                for (int ci = 0; ci < 4; ci++)
                    lc2.GetArrayElementAtIndex(ci).colorValue = def.cols[ci];
                ld2SO.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(ld2);
                allLevels.Add(ld2);
            }

            // Fallback: si no se cargo ningun nivel usa el primero disponible
            LevelData levelData = allLevels.Count > 0 ? allLevels[0] : null;

            var lmSO = new SerializedObject(lm);
            lmSO.FindProperty("levels").arraySize = allLevels.Count;
            for (int li = 0; li < allLevels.Count; li++)
                lmSO.FindProperty("levels").GetArrayElementAtIndex(li).objectReferenceValue = allLevels[li];
            lmSO.ApplyModifiedPropertiesWithoutUndo();

            // ── Save ─────────────────────────────────────────────────────────
            string folder = Path.GetDirectoryName(ScenePath);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenePath);
            EditorUtility.DisplayDialog("Project Beat", "Escena lista. Pulsa Play para jugar.", "OK");
        }

        // ── UI Builder ───────────────────────────────────────────────────────
        private static void CreateUI(Transform parent, out GameplayUI gameplayUI, out PauseMenu pauseMenu, GameController gc)
        {
            // Main HUD Canvas
            GameObject canvasGO = new GameObject("Canvas_HUD");
            canvasGO.transform.SetParent(parent);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            TMP_FontAsset font = TMP_Settings.defaultFontAsset;

            // HUD info block — top right, grouping song/score/precision/pause
            TMP_Text songText     = CreateText(canvasGO.transform, "SongText",      new Vector2(-54, -54),   new Vector2(360, 78),  font, 28, TextAlignmentOptions.TopRight);
            TMP_Text scoreText    = CreateText(canvasGO.transform, "ScoreText",     new Vector2(-54, -132),  new Vector2(360, 62),  font, 32, TextAlignmentOptions.TopRight);
            // Multiplier badge — right side, near score
            TMP_Text multText     = CreateText(canvasGO.transform, "MultText",      new Vector2(-54, -174),  new Vector2(300, 40),  font, 28, TextAlignmentOptions.TopRight);
            // Combo — center, upper area
            TMP_Text comboText    = CreateText(canvasGO.transform, "ComboText",     new Vector2(  0,  240),  new Vector2(400, 80),  font, 44, TextAlignmentOptions.Center);
            // Judgement — center
            TMP_Text judgText     = CreateText(canvasGO.transform, "JudgText",      new Vector2(  0,  175),  new Vector2(440, 80),  font, 46, TextAlignmentOptions.Center);
            // Milestone banner — center, bigger, above judgement
            TMP_Text milestoneText = CreateText(canvasGO.transform, "MilestoneText",new Vector2(  0,  310),  new Vector2(600, 70),  font, 40, TextAlignmentOptions.Center);
            // Accuracy + ESC hint grouped under score (Avance 25 HUD reorganizado)
            TMP_Text accText      = CreateText(canvasGO.transform, "AccText",       new Vector2(-54, -195),  new Vector2(360, 42),  font, 28, TextAlignmentOptions.TopRight);
            TMP_Text escHint      = CreateText(canvasGO.transform, "EscHint",       new Vector2(-54, -240),  new Vector2(360, 32),  font, 17, TextAlignmentOptions.TopRight);
            escHint.text  = "<color=#FF6A00>[ ESC ]</color> <color=#D7E5FF>Pausa</color>";

            // Results panel
            GameObject rpGO = new GameObject("ResultsPanel");
            rpGO.transform.SetParent(canvasGO.transform);
            var rpImg = rpGO.AddComponent<Image>();
            rpImg.color = new Color(0.04f, 0.01f, 0.00f, 0.93f);
            var rpRT = rpGO.GetComponent<RectTransform>();
            rpRT.anchorMin = rpRT.anchorMax = new Vector2(0.5f, 0.5f);
            rpRT.sizeDelta = new Vector2(560, 530);
            rpRT.anchoredPosition = Vector2.zero;
            CanvasGroup rpCG = rpGO.AddComponent<CanvasGroup>();

            // Neon border
            var bdrGO = new GameObject("Border");
            bdrGO.transform.SetParent(rpGO.transform);
            var bdrImg = bdrGO.AddComponent<Image>();
            bdrImg.color = new Color(1f, 0.5f, 0.0f, 0.45f);
            var bdrRT = bdrGO.GetComponent<RectTransform>();
            bdrRT.anchorMin = Vector2.zero; bdrRT.anchorMax = Vector2.one;
            bdrRT.offsetMin = new Vector2(-3, -3); bdrRT.offsetMax = new Vector2(3, 3);

            TMP_Text rpTitle = CreateText(rpGO.transform, "RPTitle", new Vector2(0,  175), new Vector2(500, 120), font, 34, TextAlignmentOptions.Center);
            TMP_Text rpBody  = CreateText(rpGO.transform, "RPBody",  new Vector2(0, -35),  new Vector2(480, 360), font, 26, TextAlignmentOptions.Center);

            gameplayUI = canvasGO.AddComponent<GameplayUI>();
            var uiSO = new SerializedObject(gameplayUI);
            uiSO.FindProperty("songText").objectReferenceValue       = songText;
            uiSO.FindProperty("scoreText").objectReferenceValue      = scoreText;
            uiSO.FindProperty("comboText").objectReferenceValue      = comboText;
            uiSO.FindProperty("accuracyText").objectReferenceValue   = accText;
            uiSO.FindProperty("judgementText").objectReferenceValue  = judgText;
            uiSO.FindProperty("multiplierText").objectReferenceValue = multText;
            uiSO.FindProperty("milestoneText").objectReferenceValue  = milestoneText;
            uiSO.FindProperty("resultGroup").objectReferenceValue    = rpCG;
            uiSO.FindProperty("resultTitleText").objectReferenceValue = rpTitle;
            uiSO.FindProperty("resultBodyText").objectReferenceValue  = rpBody;
            uiSO.ApplyModifiedPropertiesWithoutUndo();

            // ── Pause Canvas ─────────────────────────────────────────────────
            GameObject pauseCanGO = new GameObject("Canvas_Pause");
            pauseCanGO.transform.SetParent(parent);
            Canvas pauseCanvas = pauseCanGO.AddComponent<Canvas>();
            pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            pauseCanvas.sortingOrder = 200;
            var pScaler = pauseCanGO.AddComponent<CanvasScaler>();
            pScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            pScaler.referenceResolution = new Vector2(1920, 1080);
            pauseCanGO.AddComponent<GraphicRaycaster>();

            // Pause panel — mismas posiciones, fondo mejorado con bordes y barras de acento
            GameObject ppGO = new GameObject("PausePanel");
            ppGO.transform.SetParent(pauseCanGO.transform);
            // Fondo: negro calido (tinte naranja muy sutil) en vez de azul oscuro plano
            ppGO.AddComponent<Image>().color = new Color(0.05f, 0.018f, 0.00f, 0.93f);
            var ppRT = ppGO.GetComponent<RectTransform>();
            ppRT.anchorMin = Vector2.zero; ppRT.anchorMax = Vector2.one; ppRT.sizeDelta = Vector2.zero;
            CanvasGroup ppCG = ppGO.AddComponent<CanvasGroup>();
            ppCG.alpha = 0f; ppCG.interactable = ppCG.blocksRaycasts = false;

            // Borde naranja — cubre todo el panel, 3px mas grande en cada lado
            {
                var b = new GameObject("Border"); b.transform.SetParent(ppGO.transform);
                b.AddComponent<Image>().color = new Color(1f, 0.50f, 0.05f, 0.30f);
                var r = b.GetComponent<RectTransform>();
                r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
                r.offsetMin = new Vector2(-3f, -3f); r.offsetMax = new Vector2(3f, 3f);
            }

            // Barra naranja solida en la parte superior (6px alto, ancho completo)
            {
                var b = new GameObject("TopAccent"); b.transform.SetParent(ppGO.transform);
                b.AddComponent<Image>().color = new Color(1f, 0.52f, 0.05f, 1f);
                var r = b.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0f, 1f); r.anchorMax = new Vector2(1f, 1f);
                r.pivot = new Vector2(0.5f, 1f);
                r.sizeDelta = new Vector2(0f, 6f); r.anchoredPosition = Vector2.zero;
            }

            // Barra naranja solida en la parte inferior (6px alto, ancho completo)
            {
                var b = new GameObject("BotAccent"); b.transform.SetParent(ppGO.transform);
                b.AddComponent<Image>().color = new Color(1f, 0.52f, 0.05f, 1f);
                var r = b.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0f, 0f); r.anchorMax = new Vector2(1f, 0f);
                r.pivot = new Vector2(0.5f, 0f);
                r.sizeDelta = new Vector2(0f, 6f); r.anchoredPosition = Vector2.zero;
            }

            // Titulo "PAUSA" — misma posicion, sin caracteres especiales
            TMP_Text pauseTitle = CreateText(ppGO.transform, "PauseTitle", new Vector2(0, 165), new Vector2(500, 80), font, 52, TextAlignmentOptions.Center);
            pauseTitle.text = "<b><color=#ff8800>PAUSA</color></b>";

            // Linea separadora fina debajo del titulo
            {
                var d = new GameObject("TitleDivider"); d.transform.SetParent(ppGO.transform);
                d.AddComponent<Image>().color = new Color(1f, 0.50f, 0.05f, 0.35f);
                var r = d.GetComponent<RectTransform>();
                r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
                r.sizeDelta = new Vector2(400f, 1f); r.anchoredPosition = new Vector2(0f, 120f);
            }

            // Opciones — mismas posiciones exactas, texto ASCII puro (sin chars rotos)
            // El texto real lo pone PauseMenu.RefreshLabels() en runtime
            // Opciones: orden exacto de PauseMenu.RefreshLabels()
            // indice 0=CONTINUAR  1=ELEGIR NIVEL  2=REINICIAR  3=SALIR
            string[] optNames = { "CONTINUAR", "ELEGIR NIVEL", "REINICIAR", "SALIR" };
            TMP_Text[] opts   = new TMP_Text[4];
            float[]    optY   = { 75f, 13f, -49f, -111f };
            for (int i = 0; i < 4; i++)
            {
                opts[i] = CreateText(ppGO.transform, $"Opt_{i}", new Vector2(0, optY[i]), new Vector2(520, 58), font, 34, TextAlignmentOptions.Center);
                opts[i].text  = optNames[i];
                opts[i].color = new Color(0.80f, 0.68f, 0.50f, 1f);
            }

            // Hint de navegacion — misma posicion, texto ASCII puro
            TMP_Text pauseHint = CreateText(ppGO.transform, "PHint", new Vector2(0, -225), new Vector2(600, 40), font, 20, TextAlignmentOptions.Center);
            pauseHint.text = "<color=#554422>[^][v] Navegar     Enter Confirmar     ESC Cerrar</color>";

            // Level select panel (minimal — only one level)
            GameObject lsGO = new GameObject("LevelSelectPanel");
            lsGO.transform.SetParent(pauseCanGO.transform);
            lsGO.AddComponent<Image>().color = new Color(0.03f, 0.01f, 0f, 0.93f);
            var lsRT = lsGO.GetComponent<RectTransform>();
            lsRT.anchorMin = Vector2.zero; lsRT.anchorMax = Vector2.one; lsRT.sizeDelta = Vector2.zero;
            CanvasGroup lsCG = lsGO.AddComponent<CanvasGroup>();
            lsCG.alpha = 0f; lsCG.interactable = lsCG.blocksRaycasts = false;

            TMP_Text lsName   = CreateText(lsGO.transform, "LSName",   new Vector2(0, 30),  new Vector2(700, 90), font, 42, TextAlignmentOptions.Center);
            TMP_Text lsArtist = CreateText(lsGO.transform, "LSArtist", new Vector2(0, -45), new Vector2(600, 50), font, 26, TextAlignmentOptions.Center);
            lsArtist.color    = new Color(1f, 0.7f, 0.4f);
            TMP_Text lsHint   = CreateText(lsGO.transform, "LSHint",   new Vector2(0, -160), new Vector2(700, 40), font, 20, TextAlignmentOptions.Center);
            lsHint.text = "[<] [>] Cambiar     Enter Confirmar     ESC Volver";
            lsHint.color = new Color(0.5f, 0.4f, 0.3f, 1f);

            pauseMenu = pauseCanGO.AddComponent<PauseMenu>();
            var pmSO = new SerializedObject(pauseMenu);
            pmSO.FindProperty("pauseGroup").objectReferenceValue       = ppCG;
            pmSO.FindProperty("levelSelectGroup").objectReferenceValue = lsCG;
            pmSO.FindProperty("resumeLabel").objectReferenceValue      = opts[0]; // CONTINUAR
            pmSO.FindProperty("selectLevelLabel").objectReferenceValue = opts[1]; // ELEGIR NIVEL
            pmSO.FindProperty("restartLabel").objectReferenceValue     = opts[2]; // REINICIAR
            pmSO.FindProperty("quitLabel").objectReferenceValue        = opts[3]; // SALIR
            pmSO.FindProperty("levelNameText").objectReferenceValue    = lsName;
            pmSO.FindProperty("levelArtistText").objectReferenceValue  = lsArtist;
            pmSO.FindProperty("levelHintText").objectReferenceValue    = lsHint;
            pmSO.FindProperty("gameController").objectReferenceValue   = gc;
            pmSO.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private static GameObject CreateSprite(string name, string path, Vector3 pos, Vector3 scale, float alpha, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position   = pos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            sr.color  = new Color(1f, 1f, 1f, alpha);
            return go;
        }

        private static void TintSprite(GameObject go, Color tint)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = tint;
        }

        private static GameObject CreateSpriteChild(Transform parent, string name, string path, Vector3 localPos, Vector3 localScale, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = localPos;
            go.transform.localScale    = localScale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            sr.color  = color;
            sr.sortingOrder = order;
            return go;
        }

        private static void EnsureSprite(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType     = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
        }

        private static TMP_Text CreateText(Transform parent, string name, Vector2 anchoredPos, Vector2 size, TMP_FontAsset font, float fontSize, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var text = go.AddComponent<TextMeshProUGUI>();
            var rt   = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;
            text.font      = font;
            text.fontSize  = fontSize;
            text.alignment = alignment;
            text.color     = Color.white;
            text.richText  = true;
            text.text      = "";
            return text;
        }
    }
}

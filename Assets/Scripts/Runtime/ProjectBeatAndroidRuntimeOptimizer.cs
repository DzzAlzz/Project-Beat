using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Avance 99 – ajustes runtime Android finales.
    ///
    /// No modifica gameplay, LevelManager, timing, hit detection ni puntuacion.
    /// Solo estabiliza la ejecucion movil: orientacion horizontal, FPS objetivo,
    /// cursor oculto y pantalla activa durante la partida.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class ProjectBeatAndroidRuntimeOptimizer : MonoBehaviour
    {
        private static ProjectBeatAndroidRuntimeOptimizer instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            if (instance != null)
                return;

            GameObject go = new GameObject("ProjectBeat_AndroidRuntimeOptimizer");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<ProjectBeatAndroidRuntimeOptimizer>();
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
            ApplyAndroidRuntimeSettings();
        }

        private void Update()
        {
            ApplyAndroidRuntimeSettings();
        }

        private static void ApplyAndroidRuntimeSettings()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;

            // 60 FPS es mas estable para celulares y evita gasto innecesario de bateria.
            // No cambia ventanas de hit ni timing de notas.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Time.maximumDeltaTime = 1f / 30f;
        }
    }
}

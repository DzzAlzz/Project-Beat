using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Avance 83: asegura que PlayerPrefs se escriba al disco en eventos de ciclo de vida.
    /// No cambia la logica de perfiles ni gameplay; solo refuerza persistencia local.
    /// </summary>
    public sealed class ProfileAutosaveController : MonoBehaviour
    {
        private static ProfileAutosaveController instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (instance != null) return;
            GameObject go = new GameObject("ProjectBeat_ProfileAutosaveController");
            instance = go.AddComponent<ProfileAutosaveController>();
            DontDestroyOnLoad(go);
            ProfileStatsStorage.ForceSaveAll();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                ProfileStatsStorage.ForceSaveAll();
        }

        private void OnApplicationQuit()
        {
            ProfileStatsStorage.ForceSaveAll();
        }
    }
}

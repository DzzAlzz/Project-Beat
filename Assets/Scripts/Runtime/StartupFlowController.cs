using UnityEngine;

namespace ProjectBeat.Runtime
{
    // Avance 01: sin intro cinematográfica ni menú de modos.
    // La clase se conserva para evitar referencias rotas, pero no crea UI adicional.
    public class StartupFlowController : MonoBehaviour
    {
        public const string SkipStartupPrefsKey = "PB_SKIP_STARTUP";
    }
}

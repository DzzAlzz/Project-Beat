using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Avance 82: ajustes globales de suavidad/responsividad para builds y editor.
    /// No cambia gameplay, puntuación ni timing; solo configura el runtime para
    /// aprovechar monitores de 60Hz a 240Hz y reducir stutter visual.
    /// </summary>
    public static class ProjectBeatPerformanceSettings
    {
        public const int TargetFrameRate = 240;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplyOnLoad()
        {
            ApplyHighRefreshMode();
        }

        public static void ApplyHighRefreshMode()
        {
            Application.targetFrameRate = TargetFrameRate;
            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = Mathf.Max(QualitySettings.antiAliasing, 4);
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;

            // Evita saltos enormes de deltaTime después de pausas/cambios de foco.
            Time.maximumDeltaTime = 1f / 30f;

            // No se usa para mover notas, pero deja física auxiliar más estable
            // si algún efecto simple depende de FixedUpdate en el futuro.
            if (Time.fixedDeltaTime > 1f / 120f)
                Time.fixedDeltaTime = 1f / 120f;
        }
    }
}

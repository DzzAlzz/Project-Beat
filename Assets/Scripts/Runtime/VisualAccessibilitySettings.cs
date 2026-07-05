using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Avance 34 - Accesibilidad visual.
    /// Centraliza la intensidad de flashes, glow, partículas y pulsos sin alterar gameplay.
    /// Solo modifica presentación visual; no cambia BPM, timing, scoring ni beatmaps.
    /// </summary>
    public static class VisualAccessibilitySettings
    {
        public const string IntensityPrefsKey = "ProjectBeat_VisualEffectsIntensity";
        public const string SensitivityPrefsKey = "ProjectBeat_VisualSensitivityMode";

        // 0 Muy Bajo, 1 Bajo, 2 Medio, 3 Alto, 4 Extremo
        public static int IntensityIndex
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(IntensityPrefsKey, 2), 0, 4); }
        }

        public static bool SensitivityMode
        {
            get { return PlayerPrefs.GetInt(SensitivityPrefsKey, 0) == 1; }
        }

        public static string IntensityName
        {
            get
            {
                switch (IntensityIndex)
                {
                    case 0: return "MUY BAJO";
                    case 1: return "BAJO";
                    case 2: return "MEDIO";
                    case 3: return "ALTO";
                    default: return "EXTREMO";
                }
            }
        }

        public static void SetIntensityIndex(int index)
        {
            PlayerPrefs.SetInt(IntensityPrefsKey, Mathf.Clamp(index, 0, 4));
            PlayerPrefs.Save();
        }

        public static void AdjustIntensity(int delta)
        {
            SetIntensityIndex(IntensityIndex + delta);
        }

        public static void SetSensitivityMode(bool enabled)
        {
            PlayerPrefs.SetInt(SensitivityPrefsKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void ToggleSensitivityMode()
        {
            SetSensitivityMode(!SensitivityMode);
        }

        public static float EffectMultiplier
        {
            get
            {
                float value;
                switch (IntensityIndex)
                {
                    case 0: value = 0.22f; break;
                    case 1: value = 0.45f; break;
                    case 2: value = 1.00f; break;
                    case 3: value = 1.25f; break;
                    default: value = 1.55f; break;
                }
                return SensitivityMode ? Mathf.Min(value, 0.38f) : value;
            }
        }

        public static float FlashMultiplier
        {
            get
            {
                float value;
                switch (IntensityIndex)
                {
                    case 0: value = 0.08f; break;
                    case 1: value = 0.28f; break;
                    case 2: value = 0.75f; break;
                    case 3: value = 1.00f; break;
                    default: value = 1.25f; break;
                }
                return SensitivityMode ? Mathf.Min(value, 0.18f) : value;
            }
        }

        public static float ParticleMultiplier
        {
            get
            {
                float value;
                switch (IntensityIndex)
                {
                    case 0: value = 0.12f; break;
                    case 1: value = 0.35f; break;
                    case 2: value = 0.85f; break;
                    case 3: value = 1.00f; break;
                    default: value = 1.35f; break;
                }
                return SensitivityMode ? Mathf.Min(value, 0.28f) : value;
            }
        }

        public static float GlowMultiplier
        {
            get
            {
                float value;
                switch (IntensityIndex)
                {
                    case 0: value = 0.22f; break;
                    case 1: value = 0.45f; break;
                    case 2: value = 0.85f; break;
                    case 3: value = 1.10f; break;
                    default: value = 1.40f; break;
                }
                return SensitivityMode ? Mathf.Min(value, 0.40f) : value;
            }
        }

        public static float PulseMultiplier
        {
            get
            {
                float value;
                switch (IntensityIndex)
                {
                    case 0: value = 0.10f; break;
                    case 1: value = 0.35f; break;
                    case 2: value = 0.80f; break;
                    case 3: value = 1.00f; break;
                    default: value = 1.25f; break;
                }
                return SensitivityMode ? Mathf.Min(value, 0.25f) : value;
            }
        }
    }
}

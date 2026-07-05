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
        public const string VisualQualityPrefsKey = "ProjectBeat_VisualQuality";
        public const string ComboEffectsPrefsKey = "ProjectBeat_ComboEffects";
        public const string ComboAuraPrefsKey = "ProjectBeat_ComboAura";

        // 0 Muy Bajo, 1 Bajo, 2 Medio, 3 Alto, 4 Extremo
        public static int IntensityIndex
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(IntensityPrefsKey, 2), 0, 4); }
        }

        public static bool SensitivityMode
        {
            get { return PlayerPrefs.GetInt(SensitivityPrefsKey, 0) == 1; }
        }

        // Avance 88: opciones extra de experiencia visual.
        // Solo afectan presentación; no cambian timing, scoring ni hit detection.
        public static int VisualQualityIndex
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(VisualQualityPrefsKey, 2), 0, 3); }
        }

        public static string VisualQualityName
        {
            get
            {
                switch (VisualQualityIndex)
                {
                    case 0: return "BAJA";
                    case 1: return "MEDIA";
                    case 2: return "ALTA";
                    default: return "EXTREMA";
                }
            }
        }

        public static bool ComboEffectsEnabled
        {
            get { return PlayerPrefs.GetInt(ComboEffectsPrefsKey, 1) == 1; }
        }

        public static bool ComboAuraEnabled
        {
            get { return PlayerPrefs.GetInt(ComboAuraPrefsKey, 1) == 1; }
        }

        public static void SetVisualQualityIndex(int index)
        {
            PlayerPrefs.SetInt(VisualQualityPrefsKey, Mathf.Clamp(index, 0, 3));
            PlayerPrefs.Save();
        }

        public static void AdjustVisualQuality(int delta)
        {
            SetVisualQualityIndex(VisualQualityIndex + delta);
        }

        public static void SetComboEffectsEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(ComboEffectsPrefsKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void ToggleComboEffects()
        {
            SetComboEffectsEnabled(!ComboEffectsEnabled);
        }

        public static void SetComboAuraEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(ComboAuraPrefsKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void ToggleComboAura()
        {
            SetComboAuraEnabled(!ComboAuraEnabled);
        }

        public static void RestoreRecommendedVisualDefaults()
        {
            SetIntensityIndex(3);
            SetSensitivityMode(false);
            SetVisualQualityIndex(2);
            SetComboEffectsEnabled(true);
            SetComboAuraEnabled(true);
            PlayerPrefs.Save();
        }

        public static float VisualQualityMultiplier
        {
            get
            {
                switch (VisualQualityIndex)
                {
                    case 0: return 0.55f;
                    case 1: return 0.78f;
                    case 2: return 1.00f;
                    default: return 1.18f;
                }
            }
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
                return (SensitivityMode ? Mathf.Min(value, 0.38f) : value) * VisualQualityMultiplier;
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
                return (SensitivityMode ? Mathf.Min(value, 0.18f) : value) * VisualQualityMultiplier;
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
                return (SensitivityMode ? Mathf.Min(value, 0.28f) : value) * VisualQualityMultiplier;
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
                return (SensitivityMode ? Mathf.Min(value, 0.40f) : value) * VisualQualityMultiplier;
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
                return (SensitivityMode ? Mathf.Min(value, 0.25f) : value) * VisualQualityMultiplier;
            }
        }
    }
}

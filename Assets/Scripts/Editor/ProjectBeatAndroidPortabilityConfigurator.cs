#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ProjectBeat.Editor
{
    /// <summary>
    /// Avance 95 – configuracion tecnica para Android.
    ///
    /// Solo cambia datos de Player Settings relacionados con portabilidad.
    /// No toca escenas, LevelManager, gameplay, timing, beatmaps ni puntuacion.
    /// </summary>
    [InitializeOnLoad]
    public static class ProjectBeatAndroidPortabilityConfigurator
    {
        private const string IconPath = "Assets/Art/UI/Branding/ProjectBeat_ExecutableIcon.png";
        private const string AndroidPackageName = "com.projectbeat.demo";
        private const string EditorTouchHudPrefsKey = "ProjectBeat_ShowMobileTouchHUDInEditor";

        static ProjectBeatAndroidPortabilityConfigurator()
        {
            EditorApplication.delayCall += ApplyAndroidSettingsSilently;
        }

        [MenuItem("Project Beat/Android/Configurar portabilidad Android")]
        public static void ApplyAndroidSettingsFromMenu()
        {
            bool ok = ApplyAndroidSettings(showDialog: true);
            if (ok)
                Debug.Log("[Project Beat] Portabilidad Android configurada correctamente.");
        }

        [MenuItem("Project Beat/Android/Probar HUD tactil en Editor/Activar")]
        public static void EnableTouchHudPreview()
        {
            PlayerPrefs.SetInt(EditorTouchHudPrefsKey, 1);
            PlayerPrefs.Save();
            EditorUtility.DisplayDialog("Project Beat", "HUD tactil activado para pruebas dentro del Editor.\nEn build Windows seguira oculto.", "Aceptar");
        }

        [MenuItem("Project Beat/Android/Probar HUD tactil en Editor/Desactivar")]
        public static void DisableTouchHudPreview()
        {
            PlayerPrefs.SetInt(EditorTouchHudPrefsKey, 0);
            PlayerPrefs.Save();
            EditorUtility.DisplayDialog("Project Beat", "HUD tactil desactivado para pruebas dentro del Editor.", "Aceptar");
        }

        public static void ApplyAndroidSettingsSilently()
        {
            ApplyAndroidSettings(showDialog: false);
        }

        public static bool ApplyAndroidSettings(bool showDialog)
        {
            PlayerSettings.companyName = "Project Beat";
            PlayerSettings.productName = "Project Beat";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

#if UNITY_2021_2_OR_NEWER
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidPackageName);
#else
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, AndroidPackageName);
#endif

            ApplyAndroidIcon();
            AssetDatabase.SaveAssets();

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "Project Beat",
                    "Configuracion Android aplicada.\n\n" +
                    "Package Name: " + AndroidPackageName + "\n" +
                    "Orientacion: Landscape\n" +
                    "HUD tactil: automatico en Android\n\n" +
                    "Para generar APK: File > Build Profiles > Android > Switch Platform > Build.",
                    "Aceptar");
            }

            return true;
        }

        private static void ApplyAndroidIcon()
        {
            if (!File.Exists(IconPath))
                return;

            TextureImporter importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
            if (importer != null)
            {
                bool changed = false;
                if (importer.textureType != TextureImporterType.Default)
                {
                    importer.textureType = TextureImporterType.Default;
                    changed = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    changed = true;
                }

                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    changed = true;
                }

                if (changed)
                    importer.SaveAndReimport();
            }

            Texture2D iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (iconTexture == null)
                return;

#if UNITY_2021_2_OR_NEWER
            int[] iconSizes = PlayerSettings.GetIconSizes(NamedBuildTarget.Android, IconKind.Application);
            if (iconSizes == null || iconSizes.Length == 0)
                return;

            Texture2D[] icons = new Texture2D[iconSizes.Length];
            for (int i = 0; i < icons.Length; i++)
                icons[i] = iconTexture;

            PlayerSettings.SetIcons(NamedBuildTarget.Android, icons, IconKind.Application);
#else
            int[] iconSizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.Android);
            if (iconSizes == null || iconSizes.Length == 0)
                return;

            Texture2D[] icons = new Texture2D[iconSizes.Length];
            for (int i = 0; i < icons.Length; i++)
                icons[i] = iconTexture;

#pragma warning disable 0618
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, icons);
#pragma warning restore 0618
#endif
        }
    }

    public sealed class ProjectBeatAndroidPortabilityPreBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder => -900;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platformGroup == BuildTargetGroup.Android)
            {
                ProjectBeatAndroidPortabilityConfigurator.ApplyAndroidSettings(showDialog: false);
            }
        }
    }
}
#endif

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ProjectBeat.Editor
{
    /// <summary>
    /// Garantiza que el logo personalizado de Project Beat quede aplicado al ejecutable
    /// antes de generar un build Standalone de Windows/Mac/Linux.
    ///
    /// Este script es SOLO de editor: no afecta gameplay, notas, timing, LevelManager,
    /// puntuacion, menus ni sistemas del juego.
    /// </summary>
    [InitializeOnLoad]
    public static class ProjectBeatExecutableIconFinalizer
    {
        private const string IconPath = "Assets/Art/UI/Branding/ProjectBeat_ExecutableIcon.png";

        static ProjectBeatExecutableIconFinalizer()
        {
            EditorApplication.delayCall += ApplyIconSilently;
        }

        [MenuItem("Project Beat/Aplicar icono del ejecutable")]
        public static void ApplyIconFromMenu()
        {
            ApplyIcon(showDialog: true);
        }

        public static void ApplyIconSilently()
        {
            ApplyIcon(showDialog: false);
        }

        public static bool ApplyIcon(bool showDialog)
        {
            if (!File.Exists(IconPath))
            {
                if (showDialog)
                {
                    EditorUtility.DisplayDialog(
                        "Project Beat",
                        "No se encontro el icono del ejecutable en:\n" + IconPath,
                        "Aceptar");
                }
                return false;
            }

            // Para iconos de aplicacion Unity trabaja mejor con Texture2D normal.
            // No se cambia ningun sprite usado por el gameplay o la UI principal.
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
                {
                    importer.SaveAndReimport();
                }
            }

            Texture2D iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (iconTexture == null)
            {
                if (showDialog)
                {
                    EditorUtility.DisplayDialog(
                        "Project Beat",
                        "El archivo existe, pero Unity no pudo cargarlo como Texture2D. Revisa el importador del PNG.",
                        "Aceptar");
                }
                return false;
            }

            // Unity 6000.3 puede pedir 8 espacios de icono para Standalone.
            // No se debe escribir una cantidad fija, porque cambia segun version/plataforma.
            // Por eso se consulta primero cuantos tamanos espera Unity y se llena exactamente esa cantidad.
#if UNITY_2021_2_OR_NEWER
            int[] iconSizes = PlayerSettings.GetIconSizes(NamedBuildTarget.Standalone, IconKind.Application);
            Texture2D[] icons = new Texture2D[iconSizes.Length];

            for (int i = 0; i < icons.Length; i++)
            {
                icons[i] = iconTexture;
            }

            PlayerSettings.SetIcons(NamedBuildTarget.Standalone, icons, IconKind.Application);
#else
            int[] iconSizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.Standalone);
            Texture2D[] icons = new Texture2D[iconSizes.Length];

            for (int i = 0; i < icons.Length; i++)
            {
                icons[i] = iconTexture;
            }

#pragma warning disable 0618
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, icons);
#pragma warning restore 0618
#endif

            PlayerSettings.productName = "Project Beat";
            AssetDatabase.SaveAssets();

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "Project Beat",
                    "Icono del ejecutable aplicado correctamente.\n\nAhora genera el build en una carpeta nueva y limpia.",
                    "Aceptar");
            }

            return true;
        }
    }

    public sealed class ProjectBeatExecutableIconPreBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platformGroup == BuildTargetGroup.Standalone)
            {
                ProjectBeatExecutableIconFinalizer.ApplyIcon(showDialog: false);
            }
        }
    }
}
#endif

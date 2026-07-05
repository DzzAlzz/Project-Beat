using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Cursor personalizado para menus de Project Beat.
    /// Avance 46: cursor neon celeste/morado/naranja visible en menus y oculto en gameplay.
    /// </summary>
    public class ProjectBeatCursorController : MonoBehaviour
    {
        private static ProjectBeatCursorController instance;
        private Texture2D cursorTexture;
        private bool customCursorApplied;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (instance != null) return;
            GameObject go = new GameObject("ProjectBeatCursorController");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<ProjectBeatCursorController>();
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
            cursorTexture = BuildCursorTexture();
        }

        private void Update()
        {
            bool inMenu = StartupFlowController.IsMainMenuVisible;

            PauseMenu pause = FindObjectOfType<PauseMenu>();
            if (pause != null && pause.IsPausedForOverlay)
                inMenu = true;

            if (inMenu)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                ApplyCustomCursor();
            }
            else
            {
                Cursor.visible = false;
            }
        }

        private void ApplyCustomCursor()
        {
            if (customCursorApplied || cursorTexture == null) return;
            Cursor.SetCursor(cursorTexture, new Vector2(3f, 3f), CursorMode.Auto);
            customCursorApplied = true;
        }

        private Texture2D BuildCursorTexture()
        {
            const int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color cyan = new Color(0f, 0.95f, 1f, 1f);
            Color purple = new Color(0.65f, 0.18f, 1f, 1f);
            Color orange = new Color(1f, 0.42f, 0.02f, 1f);
            Color white = new Color(0.95f, 1f, 1f, 1f);

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

            // Flecha simple estilo neon: borde cyan, centro morado, acento naranja.
            for (int y = 4; y < 25; y++)
            {
                int width = Mathf.Clamp((y - 3) / 2 + 1, 1, 12);
                for (int x = 4; x < 4 + width; x++)
                {
                    bool border = x == 4 || x == 3 + width || y == 4 || y == 24;
                    tex.SetPixel(x, size - 1 - y, border ? cyan : purple);
                }
            }

            for (int i = 0; i < 8; i++)
            {
                tex.SetPixel(14 + i, size - 1 - (21 + i / 2), orange);
                tex.SetPixel(15 + i, size - 1 - (22 + i / 2), orange);
            }

            // Brillo pequeño en la punta.
            tex.SetPixel(5, size - 1 - 5, white);
            tex.SetPixel(6, size - 1 - 6, white);

            tex.Apply(false, true);
            return tex;
        }
    }
}

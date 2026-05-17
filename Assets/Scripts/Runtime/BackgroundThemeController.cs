using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Applies a colour palette to background layers.
    /// All overlay alphas are capped low so the play field is ALWAYS legible.
    /// ACELERADA uses NeonOrange: deep dark background + subtle warm overlays.
    /// </summary>
    public class BackgroundThemeController : MonoBehaviour
    {
        [Header("Background Layer Renderers")]
        [SerializeField] private SpriteRenderer bgBase;
        [SerializeField] private SpriteRenderer bgOverlayA;
        [SerializeField] private SpriteRenderer bgOverlayB;
        [SerializeField] private SpriteRenderer bgFloorA;
        [SerializeField] private SpriteRenderer bgFloorB;

        [Header("Wave Line")]
        [SerializeField] private LineRenderer neonWave;

        private static readonly ThemePalette[] Palettes = new[]
        {
            // 0 NeonPurple
            new ThemePalette(
                new Color(0.02f, 0.01f, 0.06f),
                new Color(0.45f, 0.08f, 0.75f, 0.18f),
                new Color(0.30f, 0.04f, 0.65f, 0.08f),
                new Color(0.08f, 0.70f, 0.85f, 0.07f),
                new Color(0.90f, 0.20f, 0.85f, 0.80f),
                new Color(0.90f, 0.60f, 1f,    0.80f)
            ),
            // 1 NeonOrange — ACELERADA
            // Very dark warm black bg, subtle orange tints on overlays
            // Overlays capped at alpha ≤ 0.10 so lanes are always visible
            new ThemePalette(
                new Color(0.05f, 0.02f, 0.00f),          // near-black warm dark
                new Color(1.00f, 0.40f, 0.02f, 0.08f),   // very subtle orange circuit overlay
                new Color(0.80f, 0.25f, 0.00f, 0.05f),   // near-invisible warm layer
                new Color(0.90f, 0.60f, 0.05f, 0.06f),   // barely-there floor glow
                new Color(1f,    0.50f, 0.00f, 0.85f),   // wave: vivid orange
                new Color(1f,    0.85f, 0.15f, 0.85f)    // wave end: golden yellow
            ),
            // 2 NeonBlue
            new ThemePalette(
                new Color(0.00f, 0.01f, 0.08f),
                new Color(0.04f, 0.25f, 0.90f, 0.10f),
                new Color(0.00f, 0.15f, 0.80f, 0.06f),
                new Color(0.15f, 0.80f, 0.95f, 0.07f),
                new Color(0.10f, 0.55f, 1.00f, 0.85f),
                new Color(0.45f, 0.90f, 1.00f, 0.85f)
            ),
            // 3 NeonGreen
            new ThemePalette(
                new Color(0.00f, 0.05f, 0.01f),
                new Color(0.04f, 0.80f, 0.18f, 0.10f),
                new Color(0.00f, 0.65f, 0.08f, 0.06f),
                new Color(0.60f, 0.95f, 0.08f, 0.07f),
                new Color(0.00f, 0.95f, 0.35f, 0.85f),
                new Color(0.65f, 0.95f, 0.25f, 0.85f)
            ),
            // 4 NeonPink
            new ThemePalette(
                new Color(0.07f, 0.00f, 0.05f),
                new Color(0.90f, 0.08f, 0.55f, 0.10f),
                new Color(0.80f, 0.00f, 0.45f, 0.06f),
                new Color(0.90f, 0.40f, 0.70f, 0.07f),
                new Color(0.95f, 0.08f, 0.65f, 0.85f),
                new Color(0.95f, 0.65f, 0.85f, 0.85f)
            )
        };

        public void ApplyTheme(BackgroundTheme theme)
        {
            int idx = Mathf.Clamp((int)theme, 0, Palettes.Length - 1);
            ThemePalette p = Palettes[idx];

            Camera cam = Camera.main;
            if (cam != null) cam.backgroundColor = p.camBg;

            SetSpriteColor(bgBase,     p.baseColor);
            SetSpriteColor(bgOverlayA, p.overlayColor);
            SetSpriteColor(bgOverlayB, p.overlayColor);
            SetSpriteColor(bgFloorA,   p.floorColor);
            SetSpriteColor(bgFloorB,   p.floorColor);

            if (neonWave != null)
            {
                neonWave.startColor = p.waveStart;
                neonWave.endColor   = p.waveEnd;
            }
        }

        private static void SetSpriteColor(SpriteRenderer sr, Color c)
        {
            if (sr != null) sr.color = c;
        }

        private readonly struct ThemePalette
        {
            public readonly Color camBg, baseColor, overlayColor, floorColor, waveStart, waveEnd;
            public ThemePalette(Color camBg, Color baseColor, Color overlayColor,
                                Color floorColor, Color waveStart, Color waveEnd)
            {
                this.camBg = camBg; this.baseColor = baseColor;
                this.overlayColor = overlayColor; this.floorColor = floorColor;
                this.waveStart = waveStart; this.waveEnd = waveEnd;
            }
        }
    }
}

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

        [Header("PRO Music Pulse")]
        [SerializeField] private bool enableMusicPulse = true;
        [SerializeField, Range(0.0f, 0.12f)] private float pulseIntensity = 0.045f;
        [SerializeField, Range(1f, 18f)] private float pulseSmooth = 8f;

        private Conductor conductor;
        private Vector3 baseScale = Vector3.one;
        private Color baseBaseColor;
        private Color baseOverlayAColor;
        private Color baseOverlayBColor;
        private Color baseFloorAColor;
        private Color baseFloorBColor;
        private float currentPulse;

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
            ),
            // 5 NeonWhite — ESTRELAR
            // Fondo blanco futurista con overlays celestes muy suaves.
            // Mantiene contraste usando notas azul/celeste y líneas plateadas.
            new ThemePalette(
                new Color(0.92f, 0.96f, 1.00f),
                new Color(0.96f, 0.985f, 1.00f, 1.00f),
                new Color(0.28f, 0.72f, 1.00f, 0.075f),
                new Color(0.70f, 0.86f, 0.96f, 0.085f),
                new Color(0.05f, 0.65f, 1.00f, 0.82f),
                new Color(0.78f, 0.92f, 1.00f, 0.82f)
            )
            ,// 6 NeonRedBlack — FRONTLINES
            // Fondo negro agresivo con detalles rojos neón/glitch.
            // Contraste alto para mantener la lectura de notas y UI.
            new ThemePalette(
                new Color(0.015f, 0.000f, 0.000f),
                new Color(0.055f, 0.000f, 0.005f),
                new Color(1.000f, 0.020f, 0.030f, 0.115f),
                new Color(0.95f, 0.05f, 0.02f, 0.090f),
                new Color(1.00f, 0.02f, 0.02f, 0.90f),
                new Color(1.00f, 0.28f, 0.08f, 0.88f)
            ),
            // 7 NeonBossRequiem — REQUIEM
            // Estética boss/final: negro profundo, morado oscuro, rojo vino y brillo blanco.
            // Mantiene overlays controlados para que las notas sigan siendo legibles.
            new ThemePalette(
                new Color(0.010f, 0.000f, 0.025f),
                new Color(0.020f, 0.000f, 0.050f),
                new Color(0.62f, 0.05f, 0.92f, 0.105f),
                new Color(0.55f, 0.00f, 0.10f, 0.095f),
                new Color(0.95f, 0.10f, 1.00f, 0.92f),
                new Color(1.00f, 1.00f, 1.00f, 0.88f)
            )
            ,// 8 NeonPastelSpider — HER NAME IS
            // Estética Geometry Dash: rosado pastel claro, telarañas negras y misterio elegante.
            // Fondo claro/apagado con capas oscuras sutiles para mantener contraste y lectura.
            new ThemePalette(
                new Color(0.86f, 0.66f, 0.76f),
                new Color(0.98f, 0.78f, 0.88f, 0.88f),
                new Color(0.02f, 0.00f, 0.02f, 0.105f),
                new Color(0.05f, 0.00f, 0.04f, 0.085f),
                new Color(0.98f, 0.72f, 0.90f, 0.90f),
                new Color(0.12f, 0.02f, 0.08f, 0.82f)
            )
            ,// 9 TutorialGray — TUTORIAL
            // Fondo gris/neutro para explicar controles sin saturar pantalla.
            new ThemePalette(
                new Color(0.10f, 0.11f, 0.13f),
                new Color(0.16f, 0.17f, 0.20f, 0.92f),
                new Color(0.48f, 0.54f, 0.62f, 0.10f),
                new Color(0.35f, 0.39f, 0.46f, 0.08f),
                new Color(0.72f, 0.80f, 0.92f, 0.86f),
                new Color(0.95f, 0.98f, 1.00f, 0.80f)
            )
        };

        public void ApplyTheme(BackgroundTheme theme)
        {
            int idx = Mathf.Clamp((int)theme, 0, Palettes.Length - 1);
            ThemePalette p = Palettes[idx];

            Camera cam = Camera.main;
            if (cam != null) cam.backgroundColor = p.camBg;

            baseBaseColor = p.baseColor;
            baseOverlayAColor = p.overlayColor;
            baseOverlayBColor = p.overlayColor;
            baseFloorAColor = p.floorColor;
            baseFloorBColor = p.floorColor;

            SetSpriteColor(bgBase,     baseBaseColor);
            SetSpriteColor(bgOverlayA, baseOverlayAColor);
            SetSpriteColor(bgOverlayB, baseOverlayBColor);
            SetSpriteColor(bgFloorA,   baseFloorAColor);
            SetSpriteColor(bgFloorB,   baseFloorBColor);

            baseScale = transform.localScale;
            conductor = FindObjectOfType<Conductor>();

            if (neonWave != null)
            {
                neonWave.startColor = p.waveStart;
                neonWave.endColor   = p.waveEnd;
            }
        }

        private void Update()
        {
            if (!enableMusicPulse) return;
            if (conductor == null) conductor = FindObjectOfType<Conductor>();
            if (conductor == null || !conductor.IsSongStarted) return;

            float beat = conductor.SongPositionInBeats;
            float beatFrac = beat - Mathf.Floor(beat);
            float beatHit = Mathf.Clamp01(1f - beatFrac / 0.18f);
            float wave = 0.5f + 0.5f * Mathf.Sin(beat * Mathf.PI * 2f);
            float targetPulse = (beatHit * 0.75f + wave * 0.25f) * pulseIntensity;
            currentPulse = Mathf.Lerp(currentPulse, targetPulse, Time.deltaTime * pulseSmooth);

            float scale = 1f + currentPulse;
            transform.localScale = new Vector3(baseScale.x * scale, baseScale.y * scale, baseScale.z);

            PulseSprite(bgBase, baseBaseColor, currentPulse * 0.85f);
            PulseSprite(bgOverlayA, baseOverlayAColor, currentPulse * 1.6f);
            PulseSprite(bgOverlayB, baseOverlayBColor, currentPulse * 1.6f);
            PulseSprite(bgFloorA, baseFloorAColor, currentPulse * 1.35f);
            PulseSprite(bgFloorB, baseFloorBColor, currentPulse * 1.35f);
        }

        private static void PulseSprite(SpriteRenderer sr, Color baseColor, float amount)
        {
            if (sr == null) return;
            Color c = baseColor;
            c.r = Mathf.Clamp01(c.r + amount);
            c.g = Mathf.Clamp01(c.g + amount);
            c.b = Mathf.Clamp01(c.b + amount);
            c.a = Mathf.Clamp01(baseColor.a + amount * 0.55f);
            sr.color = c;
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

using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Scrolls background layers and renders a neon wave at the BOTTOM of the screen,
    /// far below the play field so it never obstructs the player's view.
    /// The wave reacts to the beat (amplitude pulse) but stays decorative only.
    /// </summary>
    public class NeonBackgroundController : MonoBehaviour
    {
        [SerializeField] private Transform[] layers;
        [SerializeField] private float[]     speeds;
        [SerializeField] private LineRenderer waveRenderer;
        [SerializeField] private Conductor   conductor;

        [Header("Wave Settings")]
        [SerializeField] private int   wavePoints    = 80;
        [SerializeField] private float waveWidth     = 22f;
        // Wave sits at the very bottom of the screen — well below the hit line at Y=-3.2
        [SerializeField] private float waveBaseY     = -5.0f;
        // Kept small so it never creeps into the play area
        [SerializeField] private float waveAmplitude = 0.28f;
        [SerializeField] private float waveSpeed     = 0.9f;

        private float waveTime;

        private void Start()
        {
            if (waveRenderer != null)
            {
                waveRenderer.positionCount   = wavePoints;
                // Very thin line — purely atmospheric
                waveRenderer.widthMultiplier = 0.06f;
                waveRenderer.useWorldSpace   = false;
                // Push behind everything — sortingOrder -1 means it's under all gameplay
                waveRenderer.sortingOrder    = -1;
            }
        }

        private void Update()
        {
            // Scroll background layers
            for (int i = 0; layers != null && i < layers.Length && i < speeds.Length; i++)
            {
                if (layers[i] == null) continue;
                Vector3 p = layers[i].position;
                p.x -= speeds[i] * Time.deltaTime;
                if (p.x < -3f) p.x += 6f;
                layers[i].position = p;
            }

            if (waveRenderer == null) return;

            waveTime += Time.deltaTime * waveSpeed;

            // Beat pulse: amplitude briefly swells on each beat
            float beatPulse = 1f;
            if (conductor != null)
            {
                float frac = conductor.SongPositionInBeats % 1f;
                // Sharp attack, fast decay — only on first 30% of beat
                beatPulse = 1f + Mathf.Max(0f, 1f - frac / 0.30f) * 0.6f;
            }

            for (int i = 0; i < wavePoints; i++)
            {
                float t = i / (float)(wavePoints - 1);
                float x = Mathf.Lerp(-waveWidth * 0.5f, waveWidth * 0.5f, t);
                // Two overlapping sine waves for organic feel
                float y = waveBaseY
                          + Mathf.Sin((t * 5f + waveTime) * Mathf.PI * 2f) * waveAmplitude * beatPulse
                          + Mathf.Sin((t * 3f - waveTime * 0.7f) * Mathf.PI * 2f) * waveAmplitude * 0.4f;
                waveRenderer.SetPosition(i, new Vector3(x, y, 0f));
            }
        }
    }
}

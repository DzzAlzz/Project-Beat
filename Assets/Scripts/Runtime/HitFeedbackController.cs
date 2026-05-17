using UnityEngine;
using UnityEngine.UI;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Centraliza el feedback visual de aciertos sin tocar LevelManager ni los LevelData.
    /// Crea partículas, flash de pantalla y un pequeño duck de volumen en tiempo real.
    /// </summary>
    public class HitFeedbackController : MonoBehaviour
    {
        [Header("Screen Flash")]
        [SerializeField] private float flashDuration = 0.16f;
        [SerializeField] private float perfectFlashAlpha = 0.22f;
        [SerializeField] private float goodFlashAlpha = 0.13f;
        [SerializeField] private float badFlashAlpha = 0.08f;

        [Header("Audio Duck")]
        [SerializeField] private bool useAudioDuck = true;
        [SerializeField] private float duckVolume = 0.82f;
        [SerializeField] private float duckDuration = 0.055f;

        [Header("Particles")]
        [SerializeField] private int perfectParticles = 22;
        [SerializeField] private int goodParticles = 13;
        [SerializeField] private int badParticles = 7;

        private Image flashImage;
        private float flashTimer;
        private Color currentFlashColor;

        private AudioSource audioSource;
        private float baseVolume = 1f;
        private float duckTimer;

        private void Awake()
        {
            EnsureFlashCanvas();
        }

        public void Initialize(AudioSource source)
        {
            audioSource = source;
            if (audioSource != null)
                baseVolume = audioSource.volume;
        }

        public void PlayHitFeedback(JudgementType judgement, Vector3 worldPosition, Color laneColor)
        {
            if (judgement == JudgementType.Miss || judgement == JudgementType.None)
                return;

            bool perfect = judgement == JudgementType.Perfect;
            bool good = judgement == JudgementType.Good;

            int count = perfect ? perfectParticles : good ? goodParticles : badParticles;
            float speed = perfect ? 4.6f : good ? 3.4f : 2.3f;
            float size = perfect ? 0.13f : good ? 0.10f : 0.075f;
            float life = perfect ? 0.48f : good ? 0.34f : 0.22f;

            SpawnParticleBurst(worldPosition, laneColor, count, speed, size, life);

            Color flashColor = perfect
                ? new Color(laneColor.r, laneColor.g, laneColor.b, perfectFlashAlpha)
                : good
                    ? new Color(laneColor.r, laneColor.g, laneColor.b, goodFlashAlpha)
                    : new Color(1f, 0.25f, 0.20f, badFlashAlpha);

            TriggerScreenFlash(flashColor);

            if ((perfect || good) && useAudioDuck)
                TriggerAudioDuck();
        }

        private void Update()
        {
            UpdateScreenFlash();
            UpdateAudioDuck();
        }

        private void TriggerScreenFlash(Color color)
        {
            currentFlashColor = color;
            flashTimer = flashDuration;
            if (flashImage != null)
                flashImage.color = currentFlashColor;
        }

        private void UpdateScreenFlash()
        {
            if (flashImage == null || flashTimer <= 0f)
                return;

            flashTimer -= Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(flashTimer / flashDuration);
            Color c = currentFlashColor;
            c.a *= t * t;
            flashImage.color = c;

            if (flashTimer <= 0f)
                flashImage.color = Color.clear;
        }

        private void TriggerAudioDuck()
        {
            if (audioSource == null)
                return;

            baseVolume = Mathf.Max(baseVolume, audioSource.volume);
            duckTimer = duckDuration;
            audioSource.volume = Mathf.Min(audioSource.volume, duckVolume);
        }

        private void UpdateAudioDuck()
        {
            if (audioSource == null || duckTimer <= 0f)
                return;

            duckTimer -= Time.unscaledDeltaTime;
            float t = 1f - Mathf.Clamp01(duckTimer / duckDuration);
            audioSource.volume = Mathf.Lerp(duckVolume, baseVolume, t);

            if (duckTimer <= 0f)
                audioSource.volume = baseVolume;
        }

        private void SpawnParticleBurst(Vector3 position, Color color, int count, float speed, float size, float lifetime)
        {
            GameObject go = new GameObject("HitParticles_Runtime_Safe");
            go.transform.position = position;

            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            // AVANCE 05: configuración segura.
            // El sistema se crea detenido y limpio antes de modificar MainModule.
            // Esto evita el error de Unity: "Setting the duration while system is still playing".
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Clear(true);

            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = Mathf.Max(0.08f, lifetime);
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = Mathf.Max(count, 32);

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.08f;
            shape.arc = 360f;

            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(color, 0.25f),
                    new GradientColorKey(color, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.75f, 0.45f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = gradient;

            ParticleSystem.SizeOverLifetimeModule sol = ps.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve curve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.65f, 0.75f),
                new Keyframe(1f, 0f));
            sol.size = new ParticleSystem.MinMaxCurve(1f, curve);

            ps.Play(true);
            Destroy(go, lifetime + 0.35f);
        }

        private void EnsureFlashCanvas()
        {
            if (flashImage != null)
                return;

            GameObject canvasGO = new GameObject("HitFlashCanvas_Runtime");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            GameObject imageGO = new GameObject("HitFlashImage");
            imageGO.transform.SetParent(canvasGO.transform, false);
            flashImage = imageGO.AddComponent<Image>();
            flashImage.raycastTarget = false;
            flashImage.color = Color.clear;

            RectTransform rt = imageGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}

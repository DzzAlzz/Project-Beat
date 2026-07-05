using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Expanding ring effect. Perfect hits spawn a second larger ring from GameController.
    /// scaleMultiplier allows the second ring to start bigger.
    /// </summary>
    public class HitEffect : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float lifeTime   = 0.22f;
        [SerializeField] private float maxScale   = 2.4f;

        private float timer;
        private bool  isPerfect;
        private float startScale;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <param name="scaleMultiplier">Starting scale multiplier (default 1). Use >1 for outer ring.</param>
        public void Initialize(Color color, bool perfect = false, float scaleMultiplier = 1f)
        {
            timer      = lifeTime;
            isPerfect  = perfect;
            float visualMul = Mathf.Lerp(0.65f, 1.08f, VisualAccessibilitySettings.GlowMultiplier);
            startScale = (perfect ? 0.25f : 0.18f) * scaleMultiplier * visualMul;
            transform.localScale = Vector3.one * startScale;

            if (spriteRenderer != null)
            {
                color.a *= VisualAccessibilitySettings.EffectMultiplier;
                spriteRenderer.color = color;
            }
        }

        public void Initialize(Color color)
            => Initialize(color, false, 1f);

        private void Update()
        {
            timer -= Time.deltaTime;
            float progress = 1f - Mathf.Clamp01(timer / lifeTime);
            float scale    = Mathf.Lerp(startScale, maxScale * Mathf.Lerp(0.75f, 1.08f, VisualAccessibilitySettings.GlowMultiplier), Mathf.Pow(progress, 0.6f));
            transform.localScale = Vector3.one * scale;

            if (spriteRenderer != null)
            {
                // Fade: full alpha for first 60% then fade out
                float alpha = Mathf.Clamp01(timer / (lifeTime * 0.40f));
                Color c = spriteRenderer.color;
                c.a = alpha;
                spriteRenderer.color = c;
            }

            if (isPerfect)
                transform.Rotate(0f, 0f, 240f * Time.deltaTime);

            if (timer <= 0f) Destroy(gameObject);
        }
    }
}

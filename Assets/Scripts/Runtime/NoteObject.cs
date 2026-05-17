using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Nota normal / Hold Note.
    /// Avance 28:
    /// - duration <= 0: nota normal.
    /// - duration > 0: nota mantenida con cuerpo visual y validación de mantener tecla.
    /// Se mantiene el timing base: la cabeza de la nota llega al hit line en HitTime.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class NoteObject : MonoBehaviour
    {
        private GameController controller;
        private LaneInput      lane;
        private SpriteRenderer sr;
        private SpriteRenderer[] holdTrailSegments;
        private SpriteRenderer[] holdCoreSegments;
        private SpriteRenderer holdEndRenderer;
        private static Sprite holdTrailSprite;

        private Vector3 spawnPos;
        private Vector3 hitPos;
        private Vector3 holdDirection;

        private float spawnTime;
        private float despawnTime;
        private float holdDuration;
        private float holdEndTime;
        private float fullHoldLength;

        private bool initialized;
        private bool holdStarted;
        private bool holdCompleted;

        private float spawnPulseTimer;
        private const float PulseDur = 0.14f;
        private const float MinHoldDuration = 0.10f;
        private const int HoldTrailSegmentCount = 7;

        public float HitTime  { get; private set; }
        public bool  IsJudged { get; private set; }
        public bool  IsHoldNote => holdDuration > MinHoldDuration;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        public void Initialize(GameController gc, LaneInput assignedLane,
                               float hitTime, Vector3 startPos, Vector3 targetPos, Color color)
        {
            Initialize(gc, assignedLane, hitTime, 0f, startPos, targetPos, color);
        }

        public void Initialize(GameController gc, LaneInput assignedLane,
                               float hitTime, float duration, Vector3 startPos, Vector3 targetPos, Color color)
        {
            controller   = gc;
            lane         = assignedLane;
            HitTime      = hitTime;
            holdDuration = Mathf.Max(0f, duration);
            holdEndTime  = HitTime + holdDuration;

            spawnPos     = startPos;
            hitPos       = targetPos;
            spawnTime    = hitTime - gc.Beatmap.leadTime;
            despawnTime  = (IsHoldNote ? holdEndTime : hitTime) + gc.BadWindow + 0.20f;
            initialized  = true;
            IsJudged     = false;
            holdStarted  = false;
            holdCompleted = false;
            spawnPulseTimer = PulseDur;

            transform.position   = spawnPos;
            transform.localScale = Vector3.one * 0.5f;

            if (sr == null) sr = GetComponent<SpriteRenderer>();
            sr.color = color;

            holdDirection = (spawnPos - hitPos).sqrMagnitude > 0.0001f
                ? (spawnPos - hitPos).normalized
                : Vector3.up;

            fullHoldLength = CalculateHoldVisualLength(gc, holdDuration, spawnPos, hitPos);
            SetupHoldBody(color);

            lane.RegisterNote(this);
        }

        private static float CalculateHoldVisualLength(GameController gc, float duration, Vector3 startPos, Vector3 targetPos)
        {
            if (duration <= MinHoldDuration || gc == null || gc.Beatmap == null) return 0f;

            float travelDistance = Vector3.Distance(startPos, targetPos);
            float travelTime = Mathf.Max(0.1f, gc.Beatmap.leadTime);
            float length = travelDistance * Mathf.Clamp(duration / travelTime, 0.15f, 1.35f);
            return Mathf.Clamp(length, 0.55f, 4.50f);
        }

        private void SetupHoldBody(Color color)
        {
            if (!IsHoldNote)
                return;

            EnsureHoldTrailSprite();

            holdTrailSegments = new SpriteRenderer[HoldTrailSegmentCount];
            holdCoreSegments  = new SpriteRenderer[HoldTrailSegmentCount];

            for (int i = 0; i < HoldTrailSegmentCount; i++)
            {
                GameObject trail = new GameObject("HoldTrail_Soft_" + i);
                trail.transform.SetParent(transform, false);
                SpriteRenderer trailRenderer = trail.AddComponent<SpriteRenderer>();
                trailRenderer.sprite = holdTrailSprite;
                trailRenderer.sortingOrder = sr != null ? sr.sortingOrder - 2 : 5;
                trailRenderer.color = new Color(color.r, color.g, color.b, 0.22f);
                holdTrailSegments[i] = trailRenderer;

                GameObject core = new GameObject("HoldTrail_Core_" + i);
                core.transform.SetParent(transform, false);
                SpriteRenderer coreRenderer = core.AddComponent<SpriteRenderer>();
                coreRenderer.sprite = holdTrailSprite;
                coreRenderer.sortingOrder = sr != null ? sr.sortingOrder - 1 : 6;
                coreRenderer.color = new Color(1f, 1f, 1f, 0.35f);
                holdCoreSegments[i] = coreRenderer;
            }

            GameObject end = new GameObject("HoldTrail_EndMarker");
            end.transform.SetParent(transform, false);
            holdEndRenderer = end.AddComponent<SpriteRenderer>();
            holdEndRenderer.sprite = sr != null && sr.sprite != null ? sr.sprite : holdTrailSprite;
            holdEndRenderer.sortingOrder = sr != null ? sr.sortingOrder : 10;
            holdEndRenderer.color = new Color(color.r, color.g, color.b, 0.85f);

            UpdateHoldBody(fullHoldLength, 0.22f, 0.72f);
        }

        private static void EnsureHoldTrailSprite()
        {
            if (holdTrailSprite != null) return;

            Texture2D tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            tex.name = "PB_HoldTrail_RuntimeSprite";
            tex.filterMode = FilterMode.Bilinear;
            Color32 white = new Color32(255, 255, 255, 255);
            Color32[] pixels = new Color32[64];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = white;
            tex.SetPixels32(pixels);
            tex.Apply(false, true);

            holdTrailSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 8f);
        }

        private void SetHoldSorting(int headSorting)
        {
            if (holdTrailSegments != null)
            {
                for (int i = 0; i < holdTrailSegments.Length; i++)
                    if (holdTrailSegments[i] != null) holdTrailSegments[i].sortingOrder = headSorting - 3;
            }

            if (holdCoreSegments != null)
            {
                for (int i = 0; i < holdCoreSegments.Length; i++)
                    if (holdCoreSegments[i] != null) holdCoreSegments[i].sortingOrder = headSorting - 2;
            }

            if (holdEndRenderer != null)
                holdEndRenderer.sortingOrder = headSorting - 1;
        }

        private void Update()
        {
            if (!initialized || controller == null || controller.Conductor == null) return;

            float t = controller.CalibratedSongPosition;

            if (IsHoldNote && holdStarted)
            {
                UpdateActiveHold(t);
                return;
            }

            UpdateApproachVisual(t);

            if (!IsJudged && t > HitTime + controller.BadWindow)
                Judge(JudgementType.Miss);

            if (!IsJudged && t > despawnTime)
                DestroySelf();
        }

        private void UpdateApproachVisual(float t)
        {
            float span = HitTime - spawnTime;
            float raw  = span <= 0.0001f ? 1f : Mathf.InverseLerp(spawnTime, HitTime, t);
            float lerp = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(raw));

            transform.position = Vector3.Lerp(spawnPos, hitPos, lerp);

            // Perspectiva visual tipo rhythm highway.
            float perspectiveScale = Mathf.Lerp(0.50f, 1.18f, lerp);
            if (sr != null)
                sr.sortingOrder = Mathf.RoundToInt(Mathf.Lerp(7f, 16f, lerp));
            SetHoldSorting(sr != null ? sr.sortingOrder : 10);

            if (spawnPulseTimer > 0f)
            {
                spawnPulseTimer -= Time.deltaTime;
                float progress = 1f - spawnPulseTimer / PulseDur;
                transform.localScale = Vector3.one * Mathf.Lerp(0.42f, perspectiveScale, progress);
            }
            else
            {
                float approachFrac = Mathf.Clamp01((t - (HitTime - 0.25f)) / 0.25f);
                float squeeze      = 1f + Mathf.Sin(approachFrac * Mathf.PI) * 0.10f;
                transform.localScale = new Vector3(perspectiveScale * squeeze, perspectiveScale / squeeze, 1f);
            }

            if (IsHoldNote)
            {
                float visibleLength = fullHoldLength * Mathf.Lerp(0.70f, 1.12f, lerp);
                UpdateHoldBody(visibleLength, Mathf.Lerp(0.16f, 0.26f, lerp), Mathf.Lerp(0.45f, 0.78f, lerp));
            }
        }

        private void UpdateActiveHold(float t)
        {
            transform.position = hitPos;
            transform.localScale = Vector3.one * 1.18f;

            if (sr != null)
            {
                sr.sortingOrder = 17;
                Color c = sr.color;
                float pulse = 0.75f + Mathf.Sin(Time.time * 12f) * 0.12f;
                sr.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(pulse));
            }

            float holdProgress = holdDuration <= 0.001f ? 1f : Mathf.Clamp01((t - HitTime) / holdDuration);
            float remainingLength = Mathf.Lerp(fullHoldLength, 0.05f, holdProgress);
            UpdateHoldBody(remainingLength, 0.28f, Mathf.Lerp(0.82f, 0.18f, holdProgress));

            if (lane != null && lane.WasKeyReleasedThisFrame)
            {
                BreakHoldEarly();
                return;
            }

            if (t >= holdEndTime)
                CompleteHold();
        }

        public void TryHit()
        {
            if (IsJudged || controller == null || controller.Conductor == null) return;

            float delta = Mathf.Abs(HitTime - controller.CalibratedSongPosition);
            JudgementType jt = controller.GetJudgement(delta);
            if (jt == JudgementType.None) return;

            if (IsHoldNote)
                StartHold(jt);
            else
                Judge(jt);
        }

        private void StartHold(JudgementType jt)
        {
            if (IsJudged) return;

            IsJudged = true;
            holdStarted = true;
            lane.UnregisterNote(this);

            // El inicio se puntúa igual que una nota normal.
            controller.RegisterJudgement(jt, transform.position, lane.LaneIndex);
        }

        private void CompleteHold()
        {
            if (holdCompleted) return;
            holdCompleted = true;
            controller.RegisterHoldCompleted(transform.position, lane.LaneIndex, holdDuration);
            DestroySelf();
        }

        private void BreakHoldEarly()
        {
            if (holdCompleted) return;
            holdCompleted = true;
            controller.RegisterHoldBroken(transform.position, lane.LaneIndex);
            DestroySelf();
        }

        private void Judge(JudgementType jt)
        {
            if (IsJudged) return;
            IsJudged = true;
            lane.UnregisterNote(this);
            controller.RegisterJudgement(jt, transform.position, lane.LaneIndex);
            DestroySelf();
        }

        private void UpdateHoldBody(float length, float width, float alpha)
        {
            if (holdTrailSegments == null || holdTrailSegments.Length == 0) return;

            length = Mathf.Max(0.01f, length);
            width = Mathf.Clamp(width, 0.05f, 0.35f);
            alpha = Mathf.Clamp01(alpha);

            Quaternion laneRotation = Quaternion.FromToRotation(Vector3.up, holdDirection);
            float segmentLength = Mathf.Max(0.05f, length / HoldTrailSegmentCount * 0.78f);

            for (int i = 0; i < HoldTrailSegmentCount; i++)
            {
                float frac = (i + 0.5f) / HoldTrailSegmentCount;
                Vector3 segmentPos = transform.position + holdDirection * (length * frac);

                // Más ancho cerca de la nota principal y más delgado hacia el fondo.
                // Esto evita que la hold parezca un bloque gigante y la hace seguir la lectura de perspectiva.
                float perspectiveWidth = Mathf.Lerp(width * 1.10f, width * 0.42f, frac);
                float perspectiveAlpha = Mathf.Lerp(alpha, alpha * 0.38f, frac);

                SpriteRenderer trail = holdTrailSegments[i];
                if (trail != null)
                {
                    trail.transform.position = segmentPos;
                    trail.transform.rotation = laneRotation;
                    trail.transform.localScale = new Vector3(perspectiveWidth, segmentLength, 1f);
                    Color c = trail.color;
                    c.a = Mathf.Clamp01(perspectiveAlpha * 0.58f);
                    trail.color = c;
                    trail.enabled = length > 0.08f;
                }

                SpriteRenderer core = holdCoreSegments != null && i < holdCoreSegments.Length ? holdCoreSegments[i] : null;
                if (core != null)
                {
                    core.transform.position = segmentPos;
                    core.transform.rotation = laneRotation;
                    core.transform.localScale = new Vector3(perspectiveWidth * 0.34f, segmentLength * 0.94f, 1f);
                    Color c = core.color;
                    c.a = Mathf.Clamp01(perspectiveAlpha * 0.72f);
                    core.color = c;
                    core.enabled = length > 0.08f;
                }
            }

            if (holdEndRenderer != null)
            {
                holdEndRenderer.transform.position = transform.position + holdDirection * length;
                holdEndRenderer.transform.rotation = laneRotation;
                float endScale = Mathf.Clamp(width * 1.45f, 0.16f, 0.42f);
                holdEndRenderer.transform.localScale = new Vector3(endScale, endScale * 0.55f, 1f);
                Color c = holdEndRenderer.color;
                c.a = Mathf.Clamp01(alpha * 0.86f);
                holdEndRenderer.color = c;
                holdEndRenderer.enabled = length > 0.12f;
            }
        }

        private void DestroySelf()
        {
            if (lane != null) lane.UnregisterNote(this);
            Destroy(gameObject);
        }
    }
}

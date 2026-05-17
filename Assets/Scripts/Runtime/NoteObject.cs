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
        private SpriteRenderer holdBodyRenderer;

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

            GameObject body = new GameObject("HoldBody_Trail");
            body.transform.SetParent(transform, false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;

            holdBodyRenderer = body.AddComponent<SpriteRenderer>();
            holdBodyRenderer.sprite = sr != null ? sr.sprite : null;
            holdBodyRenderer.sortingOrder = sr != null ? sr.sortingOrder - 1 : 6;
            holdBodyRenderer.color = new Color(color.r, color.g, color.b, 0.38f);
            UpdateHoldBody(fullHoldLength, 0.55f, 0.88f);
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
            if (holdBodyRenderer != null)
                holdBodyRenderer.sortingOrder = (sr != null ? sr.sortingOrder : 10) - 1;

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
                UpdateHoldBody(visibleLength, Mathf.Lerp(0.42f, 0.60f, lerp), Mathf.Lerp(0.55f, 0.90f, lerp));
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
            UpdateHoldBody(remainingLength, 0.62f, Mathf.Lerp(0.85f, 0.25f, holdProgress));

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
            if (holdBodyRenderer == null) return;

            length = Mathf.Max(0.01f, length);
            width = Mathf.Max(0.05f, width);

            // El cuerpo queda detrás de la cabeza y sigue la dirección del carril.
            holdBodyRenderer.transform.position = transform.position + holdDirection * (length * 0.5f);
            holdBodyRenderer.transform.rotation = Quaternion.FromToRotation(Vector3.up, holdDirection);
            holdBodyRenderer.transform.localScale = new Vector3(width, length, 1f);

            Color c = holdBodyRenderer.color;
            c.a = Mathf.Clamp01(alpha);
            holdBodyRenderer.color = c;
        }

        private void DestroySelf()
        {
            if (lane != null) lane.UnregisterNote(this);
            Destroy(gameObject);
        }
    }
}

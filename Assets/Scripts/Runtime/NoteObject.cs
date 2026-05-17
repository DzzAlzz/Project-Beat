using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Moves note from spawn → hit, auto-misses, passes laneIndex on judgement
    /// so GameController can trigger lane flash. Visual: spawn pulse + approach squeeze.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class NoteObject : MonoBehaviour
    {
        private GameController controller;
        private LaneInput      lane;
        private SpriteRenderer sr;
        private Vector3        spawnPos;
        private Vector3        hitPos;
        private float          spawnTime;
        private float          despawnTime;
        private bool           initialized;

        private float spawnPulseTimer;
        private const float PulseDur = 0.14f;

        public float HitTime  { get; private set; }
        public bool  IsJudged { get; private set; }

        private void Awake() { sr = GetComponent<SpriteRenderer>(); }

        public void Initialize(GameController gc, LaneInput assignedLane,
                               float hitTime, Vector3 startPos, Vector3 targetPos, Color color)
        {
            controller   = gc;
            lane         = assignedLane;
            HitTime      = hitTime;
            spawnPos     = startPos;
            hitPos       = targetPos;
            spawnTime    = hitTime - gc.Beatmap.leadTime;
            despawnTime  = hitTime + gc.BadWindow + 0.18f;
            initialized  = true;
            IsJudged     = false;
            spawnPulseTimer = PulseDur;

            transform.position   = spawnPos;
            transform.localScale = Vector3.one * 0.5f;

            if (sr == null) sr = GetComponent<SpriteRenderer>();
            sr.color = color;

            lane.RegisterNote(this);
        }

        private void Update()
        {
            if (!initialized || controller == null || controller.Conductor == null) return;

            float t    = controller.CalibratedSongPosition;
            float span = HitTime - spawnTime;
            float raw  = span <= 0.0001f ? 1f : Mathf.InverseLerp(spawnTime, HitTime, t);
            float lerp = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(raw));
            transform.position = Vector3.Lerp(spawnPos, hitPos, lerp);

            // Spawn pop
            if (spawnPulseTimer > 0f)
            {
                spawnPulseTimer -= Time.deltaTime;
                float progress   = 1f - spawnPulseTimer / PulseDur;
                transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, progress);
            }
            else
            {
                // Approach squeeze: slight X stretch as note nears hit line
                float approachFrac = Mathf.Clamp01((t - (HitTime - 0.25f)) / 0.25f);
                float squeeze      = 1f + Mathf.Sin(approachFrac * Mathf.PI) * 0.10f;
                transform.localScale = new Vector3(squeeze, 1f / squeeze, 1f);
            }

            if (!IsJudged && t > HitTime + controller.BadWindow)
                Judge(JudgementType.Miss);
            if (!IsJudged && t > despawnTime)
                DestroySelf();
        }

        public void TryHit()
        {
            if (IsJudged || controller == null || controller.Conductor == null) return;
            float delta = Mathf.Abs(HitTime - controller.CalibratedSongPosition);
            var   jt    = controller.GetJudgement(delta);
            if (jt == JudgementType.None) return;
            Judge(jt);
        }

        private void Judge(JudgementType jt)
        {
            if (IsJudged) return;
            IsJudged = true;
            lane.UnregisterNote(this);
            // Pass lane index so GameController can flash the correct lane
            controller.RegisterJudgement(jt, transform.position, lane.LaneIndex);
            DestroySelf();
        }

        private void DestroySelf()
        {
            lane.UnregisterNote(this);
            Destroy(gameObject);
        }
    }
}

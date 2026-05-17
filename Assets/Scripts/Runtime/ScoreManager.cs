using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Score manager with combo multiplier tiers, streak bonuses and S+ rank.
    /// Combo tiers: 0-9 = x1, 10-24 = x2, 25-49 = x3, 50+ = x4
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        // ─── Base values ───────────────────────────────────────────────────
        private const int BASE_PERFECT = 1000;
        private const int BASE_GOOD    = 700;
        private const int BASE_BAD     = 200;

        // ─── Properties ───────────────────────────────────────────────────
        public int   Score       { get; private set; }
        public int   Combo       { get; private set; }
        public int   MaxCombo    { get; private set; }
        public int   Multiplier  { get; private set; } = 1;
        public int   PerfectCount { get; private set; }
        public int   GoodCount    { get; private set; }
        public int   BadCount     { get; private set; }
        public int   MissCount    { get; private set; }
        public int   TotalNotes   { get; private set; }
        public bool  IsFullCombo  => MissCount == 0 && BadCount == 0;

        public float Accuracy
        {
            get
            {
                if (TotalNotes == 0) return 100f;
                float earned = PerfectCount * 1f + GoodCount * 0.7f + BadCount * 0.3f;
                return Mathf.Clamp01(earned / TotalNotes) * 100f;
            }
        }

        // ─── Init ──────────────────────────────────────────────────────────
        public void Initialize(int totalNotes)
        {
            TotalNotes   = totalNotes;
            Score        = 0;
            Combo        = 0;
            MaxCombo     = 0;
            Multiplier   = 1;
            PerfectCount = 0;
            GoodCount    = 0;
            BadCount     = 0;
            MissCount    = 0;
        }

        // ─── Register ─────────────────────────────────────────────────────
        public void Register(JudgementType judgement)
        {
            switch (judgement)
            {
                case JudgementType.Perfect:
                    PerfectCount++;
                    Combo++;
                    Multiplier = GetMultiplier(Combo);
                    Score += (BASE_PERFECT + Combo * 10) * Multiplier;
                    break;

                case JudgementType.Good:
                    GoodCount++;
                    Combo++;
                    Multiplier = GetMultiplier(Combo);
                    Score += (BASE_GOOD + Combo * 5) * Multiplier;
                    break;

                case JudgementType.Bad:
                    BadCount++;
                    Combo = 0;
                    Multiplier = 1;
                    Score += BASE_BAD;
                    break;

                case JudgementType.Miss:
                    MissCount++;
                    Combo = 0;
                    Multiplier = 1;
                    break;
            }

            if (Combo > MaxCombo)
                MaxCombo = Combo;
        }

        // ─── Multiplier tier ──────────────────────────────────────────────
        private static int GetMultiplier(int combo)
        {
            if (combo >= 50) return 4;
            if (combo >= 25) return 3;
            if (combo >= 10) return 2;
            return 1;
        }

        // ─── Rank ─────────────────────────────────────────────────────────
        public string GetRank()
        {
            float acc = Accuracy;
            if (acc >= 99f && IsFullCombo) return "S+";
            if (acc >= 97f) return "S";
            if (acc >= 90f) return "A";
            if (acc >= 80f) return "B";
            if (acc >= 70f) return "C";
            return "D";
        }
    }
}

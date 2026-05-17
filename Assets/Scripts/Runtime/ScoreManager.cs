using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Avance 06: sistema de puntuación más justo.
    /// Ventanas de timing más amables, combo con multiplicador básico y rangos equilibrados.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        // ─── Base values ───────────────────────────────────────────────────
        private const int BASE_PERFECT = 1000;
        private const int BASE_GOOD    = 650;
        private const int BASE_BAD     = 250;

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
                float earned = PerfectCount * 1f + GoodCount * 0.75f + BadCount * 0.45f;
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
                    Score += (BASE_PERFECT + Combo * 8) * Multiplier;
                    break;

                case JudgementType.Good:
                    GoodCount++;
                    Combo++;
                    Multiplier = GetMultiplier(Combo);
                    Score += (BASE_GOOD + Combo * 4) * Multiplier;
                    break;

                case JudgementType.Bad:
                    BadCount++;
                    Combo = Mathf.Max(0, Combo / 2);
                    Multiplier = GetMultiplier(Combo);
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
            if (combo >= 80) return 5;
            if (combo >= 50) return 4;
            if (combo >= 25) return 3;
            if (combo >= 10) return 2;
            return 1;
        }

        // ─── Rank ─────────────────────────────────────────────────────────
        public string GetRank()
        {
            float acc = Accuracy;
            if (acc >= 94f && IsFullCombo) return "S";
            if (acc >= 85f) return "A";
            if (acc >= 72f) return "B";
            if (acc >= 60f) return "C";
            return "D";
        }
    }
}

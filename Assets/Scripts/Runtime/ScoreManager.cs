using UnityEngine;

namespace ProjectBeat.Runtime
{
    /// <summary>
    /// Sistema de puntuación más justo para Project Beat.
    /// Cambios principales:
    /// - Perfect/Good mantienen el combo.
    /// - Bad ya no destruye completamente el combo: lo baja a la mitad.
    /// - Miss sí corta el combo.
    /// - La precisión usa pesos más amigables para que el jugador pueda subir de D.
    /// - Los rangos se calculan por precisión, no solo por puntaje bruto.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        private const int BASE_PERFECT = 1000;
        private const int BASE_GOOD    = 780;
        private const int BASE_BAD     = 420;

        public int   Score        { get; private set; }
        public int   Combo        { get; private set; }
        public int   MaxCombo     { get; private set; }
        public int   Multiplier   { get; private set; } = 1;
        public int   PerfectCount { get; private set; }
        public int   GoodCount    { get; private set; }
        public int   BadCount     { get; private set; }
        public int   MissCount    { get; private set; }
        public int   TotalNotes   { get; private set; }

        public bool IsFullCombo => MissCount == 0;

        public float Accuracy
        {
            get
            {
                if (TotalNotes <= 0) return 100f;

                // Pesos tipo juego rítmico, pero más amigables:
                // Perfect = 100%, Good = 80%, Bad = 45%, Miss = 0%.
                float earned = PerfectCount * 1.00f +
                               GoodCount    * 0.80f +
                               BadCount     * 0.45f;

                return Mathf.Clamp01(earned / TotalNotes) * 100f;
            }
        }

        public void Initialize(int totalNotes)
        {
            TotalNotes   = Mathf.Max(0, totalNotes);
            Score        = 0;
            Combo        = 0;
            MaxCombo     = 0;
            Multiplier   = 1;
            PerfectCount = 0;
            GoodCount    = 0;
            BadCount     = 0;
            MissCount    = 0;
        }

        public void Register(JudgementType judgement)
        {
            switch (judgement)
            {
                case JudgementType.Perfect:
                    PerfectCount++;
                    Combo++;
                    Multiplier = GetMultiplier(Combo);
                    Score += (BASE_PERFECT + Combo * 14) * Multiplier;
                    break;

                case JudgementType.Good:
                    GoodCount++;
                    Combo++;
                    Multiplier = GetMultiplier(Combo);
                    Score += (BASE_GOOD + Combo * 9) * Multiplier;
                    break;

                case JudgementType.Bad:
                    BadCount++;
                    Score += BASE_BAD;
                    // Penalización suave: baja el combo, pero no lo borra completo.
                    Combo = Mathf.FloorToInt(Combo * 0.5f);
                    Multiplier = GetMultiplier(Combo);
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


        public void RegisterHoldBonus(float duration)
        {
            // Avance 30: bonus más controlado para que las Hold Notes sean
            // satisfactorias sin disparar demasiado el puntaje. No aumenta
            // TotalNotes, por lo que no rompe la precisión base.
            int bonus = Mathf.RoundToInt(Mathf.Clamp(duration, 0.2f, 4f) * 210f * Multiplier);
            Score += bonus;

            // Mantener una Hold completa refuerza el combo de forma suave.
            Combo++;
            Multiplier = GetMultiplier(Combo);

            if (Combo > MaxCombo)
                MaxCombo = Combo;
        }

        public void RegisterHoldBreak()
        {
            // Avance 30: soltar antes castiga, pero no destruye por completo
            // la partida. Se siente más justo que resetear todo el combo.
            Combo = Mathf.FloorToInt(Combo * 0.45f);
            Multiplier = GetMultiplier(Combo);
        }

        private static int GetMultiplier(int combo)
        {
            if (combo >= 75) return 5;
            if (combo >= 50) return 4;
            if (combo >= 25) return 3;
            if (combo >= 10) return 2;
            return 1;
        }

        public string GetRank()
        {
            float acc = Accuracy;

            if (acc >= 98f && IsFullCombo) return "S+";
            if (acc >= 94f) return "S";
            if (acc >= 85f) return "A";
            if (acc >= 72f) return "B";
            if (acc >= 55f) return "C";
            return "D";
        }
    }
}

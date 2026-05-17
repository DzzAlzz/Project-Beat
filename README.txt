╔══════════════════════════════════════════════════════════╗
║           PROJECT BEAT  ·  ACELERADA  v1.3              ║
║              — NIVEL 1 COMPLETO —                       ║
╚══════════════════════════════════════════════════════════╝

★ ACELERADA — Funk BR  |  150 BPM  |  Tema Naranja Neón

NOVEDADES v1.3
──────────────────────────────────────────
  BEATMAP ACELERADA — completamente rediseñado
    • 277 notas diseñadas musically a mano a 150 BPM
    • Intro suave (2s) → Build-up con rolls y acordes
      → Chorus 1 intenso → Breakdown dramático
      → Chorus 2 épico (16 fusas, paredes de acordes)
      → Outro acelerado + acorde final x4
    • noteSpeed: 9.0 para sensación de velocidad real
    • leadTime: 1.8s (notas visibles antes del impacto)

  GRÁFICOS — completamente mejorados
    • Fondo: negro cálido profundo, superposiciones
      al mínimo (alpha ≤ 0.07) — NUNCA obstruyen la vista
    • Onda neón: movida al fondo de pantalla (Y=-5)
      detrás de todo (sortingOrder=-1), muy delgada
      Reacciona al beat con pulso de amplitud, NO interfiere
    • Separadores de carril: líneas naranja sutiles
    • Línea de golpe: tintada naranja/dorado
    • Notas: paleta naranja-fuego (4 tonos) por carril
    • Glow de carril: breathing idle + flash blanco en PERFECT

  EFECTOS EN TIEMPO REAL
    • Hit PERFECTO: burst dorado + anillo exterior naranja
    • Hit BIEN: burst rosado
    • Las dos animaciones usan rotación y fade smooth
    • Lane flash: blanco en PERFECT durante 80ms

  SISTEMA DE PUNTUACIÓN
    • Multiplicador x1/x2/x3/x4 (combo 0/10/25/50+)
    • Banner de hito al alcanzar 10, 25, 50, 100 combos
    • Rango S+ (100% precisión + Full Combo)
    • Badge "★ FULL COMBO ★" en pantalla de resultados
    • Score count-up animado en el HUD

CONTROLES
──────────────────────────────────────────
  D  F  J  K   →  golpear carril 1/2/3/4
  ESC          →  pausa / reanudar
  R            →  reiniciar (al terminar)

  Pausa: ↑ ↓ navegar | Enter confirmar | ESC cerrar

CÓMO ABRIR EN UNITY
──────────────────────────────────────────
  1. Unity Hub → abrir carpeta "Project Beat"
  2. Menú: Project Beat › Build Demo Scene
     (o confirmarlo en el diálogo automático al abrir)
  3. ▶ Play

ARCHIVOS MODIFICADOS v1.3
──────────────────────────────────────────
  Assets/Beatmaps/acelerada.json           ← 277 notas
  Assets/Scripts/Runtime/
    GameController.cs          ← double burst, lane flash
    GameplayUI.cs              ← milestone banner, score anim
    ScoreManager.cs            ← multiplicador x1-x4, S+
    NoteObject.cs              ← spawn pulse, approach squeeze
    HitEffect.cs               ← scale multiplier, outer ring
    LaneInput.cs               ← breathing idle, perfect flash
    BackgroundThemeController.cs ← overlays ≤0.08 alpha
    NeonBackgroundController.cs  ← wave at Y=-5, sortingOrder=-1
  Assets/Scripts/Editor/DemoSceneBuilder.cs ← solo ACELERADA,
                                              wave corregida,
                                              UI mejorada

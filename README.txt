╔══════════════════════════════════════════════════════════╗
║              PROJECT BEAT · AVANCE 25                   ║
║        — GAMEPLAY UI + TECLAS VISUALES —                ║
╚══════════════════════════════════════════════════════════╝

★ Etapa de mejora visual del HUD durante el gameplay.
★ Proyecto con 7 niveles y configuración con sliders reales.
★ Aún sin Hold Notes para mantener evolución progresiva.

NOVEDADES AVANCE 25
──────────────────────────────────────────
  HUD DE GAMEPLAY
    • Se agregaron indicadores visuales bajo los carriles:
      D / F / J / K
    • Cada tecla aparece como un botón visual tipo juego de ritmo.
    • Las teclas tienen glow/cambio visual al presionarlas.
    • Mejor referencia visual para el jugador durante la partida.

  REORGANIZACIÓN DE HUD
    • PREC ya no queda encima de la zona inferior central.
    • PREC se movió al lado derecho, debajo de la puntuación.
    • Se deja más espacio para los carriles y barras de gameplay.
    • Título, puntaje y precisión quedan más ordenados.

  SISTEMAS CONSERVADOS
    • 7 niveles funcionales.
    • Configuración en pausa con sliders reales.
    • Scoring, partículas, resultados y offset funcionando.
    • Selector, intro y menús conservados.
    • LevelManager.cs intacto.

TODAVÍA NO INCLUYE
──────────────────────────────────────────
  • Hold Notes.
  • Nuevos niveles.
  • Cambios grandes de lógica principal.

CONTROLES
──────────────────────────────────────────
  D / F / J / K → Carriles
  ESC           → Pausa
  ENTER         → Confirmar
  W / S         → Navegar
  F2 / F3       → Ajustar offset
  F4            → Resetear offset

GITHUB
──────────────────────────────────────────
  Rama sugerida:
    avance-25-gameplay-ui-keys

  Comandos:
    git init
    git checkout -b avance-25-gameplay-ui-keys
    git remote add origin https://github.com/DzzAlzz/Project-Beat.git
    git add .
    git commit -m "Avance 25 - gameplay UI con teclas D F J K"
    git push origin avance-25-gameplay-ui-keys

ESTADO
──────────────────────────────────────────
  ✔ 7 niveles
  ✔ HUD reorganizado
  ✔ Teclas visuales D/F/J/K
  ✔ Configuración con sliders
  ✔ Sin Hold Notes todavía
  ✔ Listo para branch de avance


AVANCE 25 - HUD REORGANIZADO
- ESC Pausa, PREC, título, nivel y puntuación agrupados en un mismo bloque HUD.
- Indicadores D/F/J/K conservados debajo de los carriles.
- Sin Hold Notes todavía.
- Rama sugerida: avance-25-hud-reorganizado

╔══════════════════════════════════════════════════════════╗
║              PROJECT BEAT · AVANCE 08                  ║
║        — CALIBRACIÓN DE TIMING / OFFSET —              ║
╚══════════════════════════════════════════════════════════╝

Este avance representa una mejora técnica de sincronización sobre el avance 07.

NOVEDADES AVANCE 08
──────────────────────────────────────────
✔ Se mantienen los 2 niveles básicos.
✔ El Nivel 1 conserva el beatmap corregido del avance 07.
✔ Se agrega sistema básico de calibración de timing / offset.
✔ El jugador puede ajustar el desfase entre audio, notas e input.
✔ Texto simple en pantalla mostrando el offset actual.
✔ Scoring justo, combo y partículas básicas se mantienen.

CONTROLES OFFSET
──────────────────────────────────────────
F2 → bajar offset 5 ms
F3 → subir offset 5 ms
F4 → resetear offset

CONTROLES DEL JUEGO
──────────────────────────────────────────
D / F / J / K → golpear carriles
ESC → pausa
R → reiniciar al terminar

NO INCLUYE TODAVÍA
──────────────────────────────────────────
✘ Intro cinematográfica
✘ Configuración avanzada
✘ Hold Notes
✘ Sliders reales
✘ Selector de modos
✘ Resultados modernos
✘ Niveles 3 a 7
✘ UI moderna final

BRANCH SUGERIDA
──────────────────────────────────────────
avance-08-offset

COMANDOS GITHUB
──────────────────────────────────────────
git init
git checkout -b avance-08-offset
git remote add origin https://github.com/DzzAlzz/Project-Beat.git
git add .
git commit -m "Avance 08 - calibración de timing offset"
git push origin avance-08-offset

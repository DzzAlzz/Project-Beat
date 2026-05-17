╔══════════════════════════════════════════════════════════╗
║              PROJECT BEAT · AVANCE 21                  ║
║        — CONFIGURACION VISUAL CON BARRAS —             ║
╚══════════════════════════════════════════════════════════╝

RAMA RECOMENDADA:
  avance-21-config-barras

MENSAJE DE COMMIT:
  Avance 21 - barras configuración limpias

DESCRIPCION
──────────────────────────────────────────
Este avance continúa desde el avance 20 corregido.
Representa una mejora visual del submenú de CONFIGURACION
dentro del menú de pausa, manteniendo el sistema simple y estable.

NOVEDADES AVANCE 21
──────────────────────────────────────────
  CONFIGURACION EN PAUSA
    • Submenú CONFIGURACION funcionando sin errores.
    • Secciones mejor ordenadas:
      - VER CONTROLES
      - GRAFICOS
      - SONIDO
      - VOLVER

  BARRAS BASICAS LIMPIAS
    • Brillo con barra visual de texto más ordenada.
    • Volumen general con barra visual de texto más ordenada.
    • Sin sliders reales modernos todavía.
    • Mejor espaciado entre secciones.

  ESTABILIDAD
    • Sin MissingComponentException.
    • Sin IndexOutOfRangeException.
    • Navegación con teclado estable.
    • LevelManager intacto.

CONTENIDO DEL PROYECTO
──────────────────────────────────────────
  • 6 niveles jugables.
  • Intro y menú principal.
  • Pantalla de resultados.
  • Scoring, partículas y offset funcionando.
  • Configuración básica en pausa.

NO INCLUYE TODAVIA
──────────────────────────────────────────
  • Sliders reales modernos.
  • Hold Notes.
  • Nivel 7.
  • UI final completa.

COMANDOS PARA SUBIR A GITHUB
──────────────────────────────────────────
  git init
  git checkout -b avance-21-config-barras
  git remote add origin https://github.com/DzzAlzz/Project-Beat.git
  git add .
  git commit -m "Avance 21 - barras configuración limpias"
  git push origin avance-21-config-barras

NOTA
──────────────────────────────────────────
Este avance es una etapa intermedia del desarrollo, pensada para
mostrar cómo el sistema de configuración fue mejorando antes de llegar
a los sliders visuales reales de versiones posteriores.

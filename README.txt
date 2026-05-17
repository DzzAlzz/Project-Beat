╔══════════════════════════════════════════════════════════╗
║              PROJECT BEAT · AVANCE 22                  ║
║        — CONFIGURACION SIN SIMBOLOS RAROS —            ║
╚══════════════════════════════════════════════════════════╝

RAMA RECOMENDADA:
  avance-22-config-sin-cubitos

MENSAJE DE COMMIT:
  Avance 22 - configuración sin símbolos raros

DESCRIPCION
──────────────────────────────────────────
Este avance continúa desde el avance 21.
Representa una corrección visual del submenú de CONFIGURACION,
eliminando caracteres o símbolos que podían aparecer como cubitos
en algunas fuentes de TextMeshPro.

NOVEDADES AVANCE 22
──────────────────────────────────────────
  CONFIGURACION EN PAUSA
    • Submenú CONFIGURACION funcionando.
    • Secciones mantenidas:
      - VER CONTROLES
      - GRAFICOS
      - SONIDO
      - VOLVER

  CORRECCION VISUAL
    • Eliminados símbolos raros o caracteres no soportados.
    • Textos simples y compatibles.
    • Hints reemplazados por texto ASCII simple.
    • Mejor espaciado entre secciones.
    • Mejor alineación de textos y barras.

  BARRAS BASICAS
    • Brillo con barra simple de texto.
    • Volumen general con barra simple de texto.
    • Aún no se usan sliders reales modernos.

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
  git checkout -b avance-22-config-sin-cubitos
  git remote add origin https://github.com/DzzAlzz/Project-Beat.git
  git add .
  git commit -m "Avance 22 - configuración sin símbolos raros"
  git push origin avance-22-config-sin-cubitos

NOTA
──────────────────────────────────────────
Este avance es una corrección visual intermedia. La idea es mostrar
cómo el menú de configuración fue pasando de una primera versión básica
a una interfaz más limpia y compatible antes de llegar a sliders reales.

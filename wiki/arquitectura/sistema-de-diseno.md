---
title: Sistema de diseño del frontend
type: arquitectura
status: vigente
tags: [arquitectura/frontend, diseno, ui]
sources: ["DataTicket.html", "Instrucción de la persona líder del proyecto (2026-10-09)"]
aliases: [Sistema de diseño, Guía visual, Tokens de diseño, Estilo visual]
created: 2026-10-09
updated: 2026-10-09
---

# Sistema de diseño del frontend

Guía visual obligatoria del SPA. **El PRD y las HU de Scrum mandan**: definen qué pantallas existen y cómo se comportan. Esta guía solo decide cómo se ven y cómo se sienten. Combina dos referencias: el **estilo Apple** (skill local `apple-design`: movimiento físico, profundidad, tipografía cuidada, contención) y el **estilo** de [[fuente-prototipo-dataticket-html|DataTicket.html]] (paleta, tipografía, densidad, anatomía de componentes y movimiento), **no** en sus pantallas: cada vista se diseña a partir de lo que el backend y la base de datos realmente exponen ([[adr-0011-estilo-visual-inspirado-en-el-prototipo]]). El frontend lo opera el orquestador; la persona líder no tiene que supervisar el detalle visual.

## Principios

1. **Herramienta de trabajo densa y sobria**: cuerpo de 13 px, filas de 46 px y mucha información sin ruido. El carácter lo dan la tipografía de ancho variable y el color semántico, no la decoración.
2. **El color significa algo**: el azul Data Global es la acción y el foco; el verde de marca solo aparece en la entrada y la identidad; prioridad, estado y nota interna tienen paletas propias.
3. **Claro y oscuro intencionales**: los dos temas se definen con tokens y la entrada se ve siempre en claro (el logo a color necesita fondo blanco).
4. **Movimiento que explica**: rápido (120–240 ms), con salida suave y nunca en acciones de teclado frecuentes. Siempre con `prefers-reduced-motion`.
5. **Accesible por defecto**: foco visible (`--focus`), `role="alert"`/`status`, contraste AA y números tabulares en identificadores y contadores.

## Tokens (fuente: `:root` de `DataTicket.html`)

Viven en `frontend/src/styles/tokens.css`. Ningún componente usa colores sueltos.

| Grupo | Tokens claros (oscuro análogo) |
|---|---|
| Superficies | `--ground #EEF1F6`, `--surface #FFFFFF`, `--surface-2 #F6F8FB`, `--surface-3 #E6EBF2` |
| Líneas | `--line #D8DFE9`, `--line-strong #C0CAD8` |
| Tinta | `--ink #12203A`, `--ink-2 #44536B`, `--ink-3 #64728A` |
| Acento (azul Data Global) | `--accent #004098`, `--accent-hover #00337A`, `--accent-soft #E3ECF8`, `--accent-line #9FB9E0`, `--on-accent #FFF`, `--focus #2F6FD0` |
| Marca | `--brand-blue #004098`, `--brand-green #7DBF00`, `--brand-green-soft #E3ECB7`, `--brand-navy #0B1F44` |
| Nota interna | `--note-bg #FFF6DA`, `--note-line #D39A12`, `--note-ink #6B4B00`, `--note-hatch` |
| Prioridad | `--p-crit-*` (rojo sólido), `--p-alta-*`, `--p-media-*`, `--p-baja-*`; *muy baja* con borde neutro |
| Estado | `--s-nuevo`, `--s-asignado`, `--s-curso`, `--s-espera`, `--s-resuelto`, `--s-cerrado` (+ `-bg`) |
| Otros | `--danger #B42328`, `--stale #94540A`, `--overlay`, `--shadow-pop` |
| Forma | `--r-s 4px`, `--r 6px`, `--r-l 10px`; tarjetas 8–12 px; superficies destacadas (entrada) 16 px |

Los valores del tema oscuro se copian del bloque `@media (prefers-color-scheme:dark)` / `:root[data-theme="dark"]` del prototipo.

## Tipografía

- **Archivo** variable (`wdth` 62–125, `wght` 100–900). Propuesta: autoalojarla con un paquete `@fontsource-variable/archivo` verificado en npm, en lugar de Google Fonts. Así no se depende de un tercero y se simplifica la CSP ([[pendientes]] §4).
- Pila de respaldo: `"Segoe UI Variable Text","Segoe UI",system-ui,-apple-system,sans-serif`.
- Escala: cuerpo 13/1.45; títulos de bandeja 20 px, peso 660, `font-stretch:112%`; título de la entrada `clamp(28px,3.2vw,44px)`, peso 720; etiquetas 12 px, peso 600.
- Identificadores (`t-id`), contadores y fechas con `font-variant-numeric: tabular-nums`.

## Mapeo semántico al dominio (propuesta)

Los nombres del prototipo no son los del dominio. Este mapeo es una **propuesta** y se ratifica en la HU que pinte cada pantalla:

| Estado del dominio ([[estados-del-ticket]]) | Token |
|---|---|
| `New` | `--s-nuevo` (azul) |
| `InDevelopment` | `--s-curso` (violeta) |
| `PullRequestReview` | `--s-espera` (magenta) |
| `InProduction` | `--s-asignado` (verde azulado) |
| `SolutionDelivered` | `--s-resuelto` (verde) |
| `Closed` | `--s-cerrado` (gris) |

- **Prioridad** ([[matriz-de-prioridad]]): `Critical`, `High`, `Medium`, `Low` y `VeryLow` usan `prio-critica`, `prio-alta`, `prio-media`, `prio-baja` y `prio-muy_baja`, con «barras de señal» de 5 a 1. Las filas críticas llevan un filete rojo de 3 px a la izquierda.
- **Chat interno** ([[chat-interno]]): todo el chat es interno. Los mensajes usan la tarjeta `msg-card` neutra. La cinta rayada de «nota interna» marca la zona del chat dentro del detalle, para recordar que nada de eso llega al cliente. No existe el conmutador «respuesta pública».
- **Respuesta formal**: tarjeta con borde `--accent-line`, como el `msg-public` del prototipo.
- **Empresa cliente**: monograma de 20 px (`mono`) con color derivado de la empresa y avatares redondos para las personas.

## Anatomía de componentes clave

- **Botones**: alto de 30 px (26 px en la variante `sm`); variantes `primary`, `quiet`, `danger` e `icon`; `disabled` con opacidad .5.
- **Campos**: alto de 32 px; foco con borde `--accent` y anillo `0 0 0 3px` al 22 %. El error va debajo del campo y lo anuncia `role="alert"`.
- **Bandeja**: barra lateral de 236 px, barra superior de 52 px, filtros en chips (con borde punteado si están vacíos y sólido en `accent-soft` si están activos) y tabla con cabecera fija y filas de 46 px.
- **Detalle**: hilo centrado de 840 px como máximo, compositor con borde que se ilumina con `:focus-within` y panel lateral de datos.
- **Entrada (`/ingresar`, HU-003)**: un único formulario de correo y contraseña, como pide la HU. Del prototipo se toma solo el lenguaje visual: fondo claro con el azul y el verde de marca, título grande con `font-stretch`, campos de 46 px y botón de 48 px. Mensaje de error genérico único (invariante 9).
- **Avisos**: *toast* centrado abajo (fondo `--ink`) que entra en 180 ms; cajón lateral de 720 px; modal de 470 px.

## Estilo Apple (skill `apple-design`)

- **Movimiento físico e interrumpible**: resortes (*springs*) críticamente amortiguados en paneles, cajones y popovers. Una animación en curso se puede revertir sin saltos. No se anima lo que el usuario repite con el teclado.
- **Profundidad y materiales**: capas con sombra (`--shadow-pop`) y, en barras superiores y cajones, materiales translúcidos con `backdrop-filter: blur()` sobre `--surface` semitransparente, con alternativa opaca si no hay soporte o si se pide menos transparencia.
- **Tipografía**: interlineado y espaciado ajustados por tamaño (más apretado en títulos grandes, más aire en el cuerpo), números tabulares y jerarquía por escala y peso más que por color.
- **Contención y retroalimentación**: cada acción responde al instante (estado de pulsación, cambio optimista o indicador), sin decoración gratuita. `prefers-reduced-motion` y `prefers-reduced-transparency` siempre se respetan.
- **Consistencia espacial**: lo que entra por un lado sale por el mismo; los cajones vienen de su borde y los modales crecen desde su origen.

## Movimiento

- Curva de la marca: `--ease-out: cubic-bezier(0.23,1,0.32,1)`. Duraciones de 120–160 ms en *hover* y pulsación, 180–240 ms en entradas y 320 ms en transiciones de vista.
- Pulsación: `transform: scale(.97–.985)`. Solo se animan `transform`, `opacity` y `clip-path`.
- Llegada de datos en tiempo real: destello `flash` breve en filas o mensajes nuevos.
- Skills locales: las 14 de `emilkowalski/skill`, en `.claude/skills/`.
  - Dirección visual: `apple-design` y `emil-design-eng`, precargadas en `frontend-engineer`.
  - Movimiento: `animate`, `review-animations`, `improve-animations`, `find-animation-opportunities` y `animation-vocabulary`.
  - Datos extremos: `break-ui`.
  - Otras: `mobile-native`, `pick-ui-library`, `prototype` y `ask-sonner`.
  - `animate-expo` y `write-swift` quedan disponibles para una futura app nativa (fuera del MVP).

## Cómo se aplica

- `frontend-engineer` lleva precargadas `apple-design` y `emil-design-eng` y lee esta página antes de cualquier vista.
- El orquestador valida cada pantalla en el navegador integrado: tema claro y oscuro, anchos de 375, 768 y 1440 px, y teclado. Pasa `break-ui` a las vistas con datos variables (nombres largos, listas vacías, 1000 tickets).
- Las vistas siguen siendo puras ([[frontend-mvc]]): el estilo vive en CSS con tokens; no se añade Tailwind ni una librería de componentes sin ADR.

## Relacionado

- [[fuente-prototipo-dataticket-html]] · [[adr-0011-estilo-visual-inspirado-en-el-prototipo]] · [[frontend-mvc]] · [[estados-del-ticket]] · [[matriz-de-prioridad]] · [[chat-interno]] · [[hu-002-shell-y-navegacion-por-rol]] · [[hu-003-iniciar-y-cerrar-sesion]]

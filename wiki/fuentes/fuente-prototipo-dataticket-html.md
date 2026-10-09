---
title: Prototipo navegable DataTicket.html
type: fuente
status: vigente
tags: [fuente, diseno, arquitectura/frontend]
sources: ["DataTicket.html"]
aliases: [DataTicket.html, Prototipo de DataTicket, Mockup de DataTicket]
created: 2026-10-09
updated: 2026-10-09
---

# Prototipo navegable `DataTicket.html`

`DataTicket.html` está en la raíz del repositorio. Es un prototipo navegable de un solo archivo (≈240 KB, 3.316 líneas: HTML, CSS y JS en línea), citado en PRD §2. Es una **fuente cruda inmutable**: se lee y no se edita. La persona líder del proyecto lo fijó el 2026-10-09 como **referencia visual únicamente**: se toma su estilo (paleta, tipografía, estética), nunca sus pantallas, flujos ni reglas. El proyecto lo rigen el PRD y las HU de Scrum ([[adr-0011-estilo-visual-inspirado-en-el-prototipo]]).

## Qué contiene

- **Tokens de marca Data Global** en `:root`, con tema claro, tema oscuro (`prefers-color-scheme` y `data-theme`) y colores semánticos de prioridad y estado. Destilados en [[sistema-de-diseno]].
- **Tipografía**: Archivo variable (ejes `wdth` 62–125 y `wght` 100–900) desde Google Fonts, cuerpo de 13 px, pesos intermedios (520–720) y `font-stretch` para dar carácter a títulos e identificadores.
- **Pantallas** del recorrido de demostración:
  - entrada «dos puertas» (Data Global / Cliente);
  - bandeja del agente con barra lateral, filtros en chips y tabla densa;
  - detalle con hilo, compositor y panel lateral de datos;
  - portal del cliente (radicación y «tus tickets abiertos»);
  - constructor de formularios (catálogo, lienzo, propiedades).
- **Componentes**: botones (primario, silencioso, peligro, icono), controles de formulario con foco de anillo, prioridad con «barras de señal», estado en píldora con icono, monogramas de empresa, avatares, popovers, modal, cajón lateral, *toasts*, estado vacío y nota interna con cinta rayada.
- **Movimiento**: transiciones de 120 a 240 ms, curva `cubic-bezier(0.23,1,0.32,1)`, View Transitions en la entrada y `prefers-reduced-motion` respetado.
- **Responsivo**: cortes en 1240, 1000, 760 y 640 px.

## Lo que se adopta y lo que no

| Se adopta (estilo) | No se adopta (comportamiento del mockup) |
|---|---|
| Paleta, tokens claro/oscuro, tipografía, densidad, radios, sombras, foco | Datos y cuentas de demostración (`ACCOUNTS`), barra «demo» |
| Anatomía de componentes (prioridad, estado, monograma, chips, tabla, compositor) | Lógica en el navegador: en DataTicket la autorización es del backend |
| Lenguaje visual de la entrada (fondo claro, colores de marca, tipografía grande) | Entrada en «dos puertas», respuestas públicas en el hilo, estados propios |
| Curvas y duraciones de movimiento | Estados del ticket del mockup (se mapean a los del PRD) |

## Diferencias con el PRD (resueltas: prevalece el PRD)

> [!info] Decisión de la persona líder (2026-10-09)
> El prototipo es solo referencia visual. Cuando difiere del PRD o de una HU, **no se tiene en cuenta**. Las diferencias se listan para que nadie las implemente por error.

> [!note] Respuestas públicas en el hilo (no se implementan)
> El prototipo permite responder al cliente desde el compositor del hilo (`msg-public`, conmutador público/nota interna). El PRD prohíbe el chat visible para el cliente y separa la **respuesta formal** (PRD §5.6, §6.5, §13). Prevalece el PRD: el chat es solo interno y la respuesta formal es una acción aparte ([[hu-033-emitir-respuesta-formal]]). Del prototipo se reutiliza solo el estilo de tarjeta.

> [!note] Estados del ticket (se usan los del dominio)
> El prototipo usa `nuevo`, `asignado`, `en_proceso`, `espera_solicitante`, `espera_terceros`, `resuelto`, `cerrado` y `cancelado`. El dominio usa `New`, `InDevelopment`, `PullRequestReview`, `InProduction`, `SolutionDelivered` y `Closed`, y el portal muestra etiquetas propias ([[estados-del-ticket]]). Prevalece el PRD. [[sistema-de-diseno]] propone a qué color se asigna cada estado.

> [!note] Entrada en «dos puertas» (no se implementa)
> El prototipo separa el login en las puertas Data Global y Cliente. HU-003 define una única vista `/ingresar` con correo y contraseña y un único `POST /api/auth/login`: eso es lo que se construye.

- El logo de Data Global no viene incluido (`BRAND_LOGO = ''`): falta el archivo oficial ([[pendientes]] §6).

## Relacionado

- [[sistema-de-diseno]] · [[adr-0011-estilo-visual-inspirado-en-el-prototipo]] · [[frontend-mvc]] · [[fuente-prd-v0-1]] · [[pendientes]]

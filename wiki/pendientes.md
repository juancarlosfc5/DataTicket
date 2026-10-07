---
title: Pendientes, dudas y contradicciones
type: pendientes
status: vigente
tags: [pendientes, decision]
sources: ["PRD.md §16", "Revisión del PRD durante el setup (2026-10-07)"]
aliases: [Preguntas abiertas, Decisiones pendientes, Contradicciones]
created: 2026-10-07
updated: 2026-10-07
---

# Pendientes, dudas y contradicciones

Registro vivo de lo que falta decidir o aclarar. Cuando algo se resuelve, se convierte en ADR o en contenido de la página correspondiente y se tacha aquí con la fecha y el enlace. Responsable por defecto: la persona líder del proyecto con el dueño del producto.

## 1. Contradicciones detectadas

> [!warning] Contradicción — PRD §15 desactualizado
> Dice que el Compose levanta solo PostgreSQL y que no existen proyectos .NET/React. Desde el 2026-10-07 el repositorio tiene ambos y un Compose de cinco servicios ([[analisis-prd-vs-docker-compose]]). **Acción:** actualizar §15 en la próxima versión del PRD (no se editó por ser fuente inmutable).

> [!warning] Contradicción — tipos de adjunto en el chat
> PRD §7 (objetivo) habla de chat "de texto e imágenes"; PRD §7.8 y §14.9 permiten además PDF, XML y Excel. Se asume lo más amplio (§7.8/§14.9) hasta confirmar → [[chat-interno]], [[archivos-adjuntos]].

> [!success] Resuelta 2026-10-07 — fase del portal del cliente
> ~~PRD §13 ubica el "portal con estados resumidos" en la fase 3 (P1), pero CA-02 lo exige en el MVP y la fase 1 ya incluye "estados" y "bandejas".~~ La consulta básica del portal va en la Fase 1, y el resumen y las métricas en la Fase 3 → [[adr-0008-portal-cliente-en-fase-1]].

> [!warning] Contradicción — fase del dashboard y alcance del "MVP"
> CA-13 exige el dashboard en el MVP, pero PRD §13 lo ubica en la Fase 3 (P1). El PRD no define si el "MVP" abarca las fases 0–2 o las fases 0–3 → [[fases-y-alcance]], [[ep-012-dashboard-y-metricas]].

> [!warning] Contradicción — roles frente a equipos
> [[autenticacion-identity]] define `Development` y `Production` como roles de Identity. En cambio, [[glosario]], [[roles-y-permisos]] y [[modelo-de-dominio]] los tratan como equipos (`Team.Development`, `Team.Production`), separados de `Roles[]`. Hay que decidir el modelo antes de [[hu-004-contexto-de-usuario-y-autorizacion]] y [[hu-009-administrar-usuarios-internos-y-equipos]].

> [!warning] Contradicción — métrica "primera respuesta"
> PRD §13 (fase 3) menciona "medición de primera respuesta", métrica que §4 no define (solo define tiempo hasta respuesta formal) → [[dashboard-y-metricas]].

## 2. Decisiones de producto pendientes antes de producción (PRD §16)

- [ ] Infraestructura y responsable de despliegue, respaldos, monitoreo y actualizaciones (§16.1).
- [ ] Proveedor de correo, dominio remitente y entregabilidad (§16.2). En local: Mailpit.
- [ ] Retención/eliminación de tickets, mensajes, auditoría y archivos; respaldos, RPO/RTO (§16.3, §12).
- [ ] ¿Reportes exportables o vistas guardadas tras el piloto? (§16.4).
- [ ] Volumen de datos sintéticos: referencia ~50 empresas de prueba con tickets variados (§16.5).
- [ ] Aprobación explícita del riesgo de URL pública permanente en Azure Blob por la dirección responsable de los datos (§16.6) → [[adr-0006-urls-publicas-azure-blob]].

## 3. Vacíos y ambigüedades del PRD

| # | Sección | Pregunta | Página |
|---|---|---|---|
| V-01 | §6.4 | ¿Qué transiciones de estado son válidas, quién mueve cada una, hay retrocesos (PR rechazado) o reapertura? ¿`Closed` exige `SolutionDelivered`? | [[estados-del-ticket]] |
| V-02 | §6.3 | ¿En qué estado inicia un ticket asignado directamente a Julián? ¿Cómo es el camino de tickets que no pasan por Producción? | [[flujo-del-ticket]] |
| V-03 | §6.1 | ¿Qué roles pueden ajustar la prioridad? ¿Las colas ordenan por prioridad calculada o ajustada? | [[matriz-de-prioridad]] |
| V-04 | §6.3 | ¿Quién es la "persona autorizada" que registra la URL del PR? | [[flujo-del-ticket]] |
| V-05 | §5.5, §6.2 | ¿Cómo se determina la "ausencia" de Elizabeth? ¿La cola de cobertura está siempre visible para administradores? | [[roles-y-permisos]] |
| V-06 | §8 | Extensiones/MIME exactas de "imágenes" y "Excel"; validación de contenido más allá de la extensión (se propone validar firma) | [[archivos-adjuntos]] |
| V-07 | §11 | ¿Quién recibe el correo de la respuesta formal: solicitante, coordinador o ambos? | [[notificaciones]] |
| V-08 | §7.7, §11 | ¿Se reenvía el correo de vinculación a quien es retirado y vuelto a agregar? ¿Lo recibe el administrador que toma un ticket en cobertura? | [[notificaciones]] |
| V-09 | §4 | Días hábiles definidos (lun–vie, festivos cuentan), pero no horario hábil | [[dashboard-y-metricas]] |
| V-10 | §4, §6.2, §10 | Diferencia operativa entre "responsable", "asignación" y "participante" | [[flujo-del-ticket]] |
| V-11 | §12 | ¿Qué significa el campo "resultado" de la auditoría? ¿Se auditan intentos denegados? | [[auditoria]] |
| V-12 | §6.1, §9 | Catálogo de categorías y quién lo administra | [[formularios-configurables]] |
| V-13 | §6.5 | ¿Puede haber más de una respuesta formal o corregirse una ya enviada? | [[flujo-del-ticket]] |
| V-14 | §7 | ¿Se pueden editar o borrar mensajes del chat? | [[chat-interno]] |
| V-15 | §5 | Administradores del piloto y responsabilidades de Gerardo | [[equipo-data-global]] |
| V-16 | §5.5, §6.2 | Formato del número de ticket | [[modelo-de-dominio]] |
| V-17 | §14.1 | CA-01 protege la descarga "desde el portal", pero las URLs públicas del Blob permiten acceso entre empresas fuera del portal (riesgo aceptado; conviene explicitarlo ante la dirección) | [[adr-0006-urls-publicas-azure-blob]] |
| V-18 | §4, §10, §14.13 | "Área" se usa para filtrar y agrupar, pero no existe en el modelo: ¿es la categoría o el equipo? | [[dashboard-y-metricas]], [[hu-040-panel-global-de-la-pm]] |
| V-19 | §6.4, §6.5 | El diagrama inferido no permite `PullRequestReview → SolutionDelivered` (cierre tras PR sin pasar por Producción) | [[estados-del-ticket]], [[hu-021-cambiar-estado-interno]] |
| V-20 | §8, §5.6 | `Attachment` no indica su contexto (radicación, chat o respuesta) ni si es interno o visible al cliente, y eso hace falta para CA-10 | [[modelo-de-dominio]], [[archivos-adjuntos]] |
| V-21 | §6.5, §12 | `Ticket` no tiene campos de cierre (quién, cuándo, estado anterior): ¿solo en `AuditEntry` o también en la entidad? | [[modelo-de-dominio]], [[hu-036-cerrar-ticket-manualmente]] |
| V-22 | §7 | Formato y tamaño de página del cursor del historial; ¿hay paginación hacia atrás? | [[tiempo-real-signalr]], [[hu-026-historial-del-chat-por-cursor]] |
| V-23 | §9 | Almacenamiento de las respuestas variables de la Fase 4 (jsonb o tablas) y cómo se consultan | [[formularios-configurables]], [[hu-046-radicar-con-formulario-de-empresa]] |
| V-24 | §12 | ¿Quién puede consultar la bitácora de auditoría de un ticket? | [[auditoria]], [[hu-012-consultar-bitacora-del-ticket]] |
| V-25 | — | El mapa de puertos y el glosario no tienen casos de uso para reasignar, ajustar la prioridad, cambiar el estado ni registrar la URL del PR, ni los métodos del hub | [[backend-hexagonal]], [[glosario]] |

## 4. Decisiones técnicas abiertas

- [ ] Aceptar o cambiar [[adr-0004-autenticacion-cookie-mismo-origen]] (cookie same-origin) antes de implementar el login.
- [ ] Evaluar Row-Level Security de PostgreSQL como defensa adicional al filtro multiempresa (PRD §12) → [[persistencia-postgresql]].
- [ ] Enrutador del frontend y librería de estado de servidor (p. ej. React Router, TanStack Query) → [[frontend-mvc]].
- [ ] Estrategia de despliegue productivo que mantenga el mismo origen para `/api` y `/hubs` (nginx del frontend u otro proxy) → [[entorno-docker]].
- [ ] Backplane de SignalR (Redis o Azure SignalR) solo al escalar a varias instancias (PRD §7) → [[tiempo-real-signalr]].
- [ ] CI en GitHub Actions (pruebas backend, lint/pruebas/build frontend) → [[flujo-de-trabajo-github]].
- [ ] Evaluar migración a TypeScript 7 (nativo) cuando la plantilla de Vite lo adopte → [[stack-y-versiones]].
- [ ] Node 24 entra en *Maintenance LTS* el 2026-10-20 (Node 26 es LTS desde el 2026-10-28); reevaluar antes de producción → [[stack-y-versiones]].
- [ ] Modelo de dominio: validar la propuesta al implementar la fase 0/1 → [[modelo-de-dominio]].
- [ ] Máximo de archivos por solicitud (radicación, respuesta formal): define el límite de cuerpo del backend; nginx admite hoy 50 MB en `/api/` → [[archivos-adjuntos]], [[entorno-docker]].
- [ ] Content-Security-Policy del SPA: requiere conocer el dominio público del Blob para `img-src`/`connect-src` → [[entorno-docker]].
- [ ] Al desplegar tras un proxy/TLS: `UseForwardedHeaders` con proxies conocidos, `AllowedHosts` fijo y `X-Forwarded-Proto` correcto → [[autenticacion-identity]].

## 5. Equipo y colaboración

- [ ] Reparto de responsabilidades entre Laura, Juan David y Brayan → [[equipo-del-repositorio]].
- [ ] Usuarios de GitHub del equipo para `CODEOWNERS` y protección de `main` → [[flujo-de-trabajo-github]].

## 6. Fuentes por ingerir

- [ ] `mvp-sistema-tickets.md` (borrador del desarrollador) y `DataTicket.html` (prototipo), citados en PRD §2.
- [ ] ZIP de diseño del equipo (PRD §2).

Colócalas en `wiki/raw/` y pide su ingesta ([[trabajar-con-el-agente]]).

## Relacionado

- [[fuente-prd-v0-1]] · [[vision-general]] · [[index]] · [[tablero-scrum]]

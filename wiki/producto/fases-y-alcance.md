---
title: Fases y alcance
type: concepto
status: vigente
tags: [producto, producto/alcance, producto/fases]
sources: ["PRD.md §13", "PRD.md §16", "PRD.md §1", "PRD.md §2"]
aliases: [Fases, Roadmap, Alcance del MVP, Fuera del MVP]
created: 2026-10-07
updated: 2026-10-07
---

# Fases y alcance

Orden de construcción de DataTicket en cinco fases (0–4) y lista de lo que queda fuera del MVP. La prioridad es la trazabilidad del ticket y la colaboración interna; los formularios por cliente van al final (PRD §1, §13).

## Fases (PRD §13)

| Fase | Resultado esperado | Prioridad |
|---|---|---|
| **0. Base técnica** | Monolito modular .NET 10, React/TypeScript, PostgreSQL, autenticación, empresas, usuarios y permisos | Bloqueante |
| **1. Piloto operativo** | Formulario común, radicación, adjuntos, triage, participantes, estados, auditoría y bandejas | P0 |
| **2. Colaboración y entrega** | Chat SignalR persistente, adjuntos, lecturas, reconexión, respuesta formal, cierre manual y notificaciones acordadas | P0 |
| **3. Operación medible** | Dashboard interno y portal con estados resumidos; medición de primera respuesta y cierre | P1 |
| **4. Formularios configurables** | Constructor y plantillas por cliente, versionadas, antes de ampliar el producto | Última fase comprometida |

Páginas por fase:

- Fase 0 → [[roles-y-permisos]], [[arquitectura-general]], [[stack-y-versiones]]
- Fase 1 → [[flujo-del-ticket]], [[matriz-de-prioridad]], [[archivos-adjuntos]], [[estados-del-ticket]], [[auditoria]]
- Fase 2 → [[chat-interno]], [[notificaciones]], [[tiempo-real-signalr]]
- Fase 3 → [[dashboard-y-metricas]]
- Fase 4 → [[formularios-configurables]]

## Fuera del MVP o diferido (PRD §13)

| Tema | Nota |
|---|---|
| MFA, SSO y federación de identidad | Ver [[roles-y-permisos]] |
| Integración automática con GitHub/GitLab y sincronización de PR | La URL de PR es manual (PRD §6.3) |
| Personalización de formularios durante el piloto | Se implementa en la Fase 4 |
| Correo entrante y creación automática de tickets | — |
| Automatizaciones de asignación/escalamiento, SLA y recordatorios | Ver [[dashboard-y-metricas]] |
| Base de conocimiento, CMDB, encuesta de satisfacción, mensajería instantánea, app móvil nativa | La interfaz es web responsiva (PRD §12) |
| Chat visible para clientes o respuestas públicas tipo conversación | El cliente recibe una respuesta formal final |
| Redis / backplane multiinstancia | Se reconsidera al escalar (PRD §7) |

También diferidos: listados exportables y vistas guardadas, a priorizar tras el piloto (PRD §10, §16).

## Decisiones previas a producción (PRD §16)

No impiden el piloto funcional, pero deben resolverse antes de operar con clientes reales:

1. Infraestructura y responsable operativo (despliegue, respaldos, monitoreo, actualizaciones).
2. Proveedor de correo, dominio remitente y entregabilidad.
3. Retención/eliminación de tickets, mensajes, auditoría y archivos; respaldos y objetivos de recuperación.
4. Reportes exportables o vistas guardadas para clientes tras el piloto.
5. Volumen de datos sintéticos a sembrar: referencia de ~50 empresas de prueba con tickets en estados diversos, sin información real.
6. Aprobación explícita del riesgo de URL pública permanente de Azure Blob ([[adr-0006-urls-publicas-azure-blob]]).

Seguimiento en [[pendientes]].

> [!question] Pendiente
> La Fase 1 incluye "estados" y "bandejas", pero el "portal con estados resumidos" aparece en la Fase 3; no queda claro si el portal del cliente (consulta de sus tickets) se entrega en Fase 1 o en Fase 3. Ver [[pendientes]].

## Relacionado

- [[vision-general]] · [[criterios-de-aceptacion]] · [[pendientes]]
- [[formularios-configurables]] · [[dashboard-y-metricas]] · [[chat-interno]]
- [[arquitectura-general]] · [[stack-y-versiones]] · [[fuente-prd-v0-1]]

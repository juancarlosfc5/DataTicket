---
title: Índice de la wiki
type: indice
status: vigente
tags: [indice]
sources: []
aliases: [Índice, Inicio, Home]
created: 2026-10-07
updated: 2026-10-09
---

# Índice de la wiki de DataTicket

Catálogo de todas las páginas. **Agentes: leer primero.** Personas: empezar por [[vision-general]]. Convenciones y operaciones en `AGENTS.md` (raíz del repositorio); historial en [[log]].

## Inicio

- [[vision-general]] — Qué es DataTicket, actores, stack y estado actual (hub de la wiki).
- [[pendientes]] — Contradicciones, vacíos del PRD y decisiones abiertas.
- [[glosario]] — Lenguaje ubicuo: término de negocio ↔ nombre en código.
- [[log]] — Bitácora cronológica de cambios en el proyecto y la wiki.

## Producto

- [[flujo-del-ticket]] — Ciclo de vida: radicación → triage → Desarrollo/Producción → respuesta formal → cierre manual.
- [[estados-del-ticket]] — Estados internos vs. estado resumido del cliente y transiciones (inferidas, por validar).
- [[matriz-de-prioridad]] — Prioridad calculada por urgencia × impacto y ajuste manual auditado.
- [[roles-y-permisos]] — Roles, siete reglas de autorización, cobertura del administrador y ciclo de cuentas.
- [[chat-interno]] — Requisitos funcionales del chat privado por ticket.
- [[archivos-adjuntos]] — Tipos, 10 MB, validación en backend, Blob y riesgo de URL pública.
- [[notificaciones]] — Qué evento notifica a quién y por qué canal.
- [[auditoria]] — Bitácora append-only: campos y eventos auditados.
- [[dashboard-y-metricas]] — Métricas, cómputo de tiempos (America/Bogota, festivos cuentan), bandejas y paneles.
- [[formularios-configurables]] — Formulario común del piloto vs. formularios por cliente (última fase).
- [[fases-y-alcance]] — Fases 0–4 y lo que queda fuera del MVP.
- [[criterios-de-aceptacion]] — Checklist CA-01…CA-15 del MVP.

## Dominio

- [[modelo-de-dominio]] — Agregados, entidades e invariantes propuestos (por validar).

## Personas

- [[equipo-data-global]] — Equipos Desarrollo y Producción del piloto.
- [[elizabeth-pm]] — Elizabeth, Product Manager: triage, asignación, respuesta formal y cierre.
- [[julian-produccion]] — Julián, líder de Producción: revisión de PR y despliegues.
- [[equipo-del-repositorio]] — Quiénes construyen DataTicket en GitHub (persona líder, Laura, Juan David, Brayan).

## Arquitectura

- [[arquitectura-general]] — Monorepo, contenedores, proxy same-origin y estado actual.
- [[backend-hexagonal]] — Proyectos, regla de dependencias, mapa de puertos y cómo añadir funcionalidades.
- [[frontend-mvc]] — Estructura por módulos, reglas de capas (oxlint) y módulo de referencia.
- [[sistema-de-diseno]] — Guía visual obligatoria: estilo Apple + tokens, tipografía Archivo y componentes de `DataTicket.html`; el PRD manda.
- [[autenticacion-identity]] — Diseño de login con ASP.NET Core Identity (cookie same-origin aceptada; sin implementar).
- [[tiempo-real-signalr]] — Hub `/hubs/tickets`, persistir antes de publicar, reconexión y revocación.
- [[persistencia-postgresql]] — EF Core + Npgsql, migraciones, diseño de referencia `db.sql`, multiempresa y auditoría.
- [[entorno-docker]] — Servicios, Compose Watch, variables, Dockerfiles, verificación y problemas frecuentes.
- [[stack-y-versiones]] — Versiones verificadas, soporte y política de actualización.

## Decisiones (ADR)

- [[adr-0001-monorepo-contenedorizado]] — Monorepo con backend, frontend, wiki y Compose. *Aceptada.*
- [[adr-0002-backend-hexagonal]] — Puertos y adaptadores verificados por pruebas. *Aceptada.*
- [[adr-0003-frontend-mvc]] — MVC por módulo con fronteras en oxlint. *Aceptada.*
- [[adr-0004-autenticacion-cookie-mismo-origen]] — Identity con cookie same-origin. *Aceptada.*
- [[adr-0005-signalr-para-chat]] — SignalR + PostgreSQL para el chat. *Aceptada.*
- [[adr-0006-urls-publicas-azure-blob]] — URL pública permanente para adjuntos (riesgo aceptado). *Aceptada.*
- [[adr-0007-entorno-local-compose-watch]] — Compose Watch, Mailpit y Azurite. *Aceptada.*
- [[adr-0008-portal-cliente-en-fase-1]] — La consulta básica del portal del cliente se entrega en la Fase 1. *Aceptada.*
- [[adr-0009-db-sql-diseno-de-referencia]] — `db.sql` es el diseño de referencia aprobado; la base se implementa con migraciones de EF Core. *Aceptada.*
- [[adr-0010-enrutador-react-router]] — React Router 8.4.0 en modo librería; sin librería de estado de servidor por ahora. *Aceptada.*
- [[adr-0011-estilo-visual-inspirado-en-el-prototipo]] — Estilo Apple + estética de `DataTicket.html` (solo referencia visual; rigen el PRD y Scrum); el orquestador opera el frontend. *Aceptada.*

## Scrum

- [[tablero-scrum]] — Backlog: 13 épicas y 47 HU por fase, sprints sugeridos, cobertura de los CA y decisiones que lo condicionan. Seguimiento en `PLAN-DE-TRABAJO.md` (raíz).
- [[ep-001-fundaciones-tecnicas]] — EP-001 (Fase 0): CI en cada PR y shell de la app con navegación por rol.
- [[ep-002-identidad-y-acceso]] — EP-002 (Fase 0): login con cookie, autorización por rol, invitación y restablecimiento.
- [[ep-003-empresas-usuarios-y-equipos]] — EP-003 (Fase 0): empresas cliente, usuarios cliente e internos, equipos y datos sintéticos.
- [[ep-004-auditoria-append-only]] — EP-004 (Fase 1): registro append-only y consulta de la bitácora.
- [[ep-005-radicacion-de-tickets]] — EP-005 (Fase 1): formulario común, prioridad calculada, adjuntos y ajuste manual.
- [[ep-006-triage-asignacion-y-participantes]] — EP-006 (Fase 1): cola de la PM, cobertura del admin, asignación y participantes.
- [[ep-007-trabajo-interno-y-estados]] — EP-007 (Fase 1): detalle interno, estados internos y URL de PR.
- [[ep-008-bandejas-y-portal-del-cliente]] — EP-008 (Fase 1): bandejas internas y consulta del portal del cliente.
- [[ep-009-chat-interno-en-tiempo-real]] — EP-009 (Fase 2): chat SignalR con historial, lecturas, reconexión y revocación.
- [[ep-010-respuesta-formal-y-cierre]] — EP-010 (Fase 2): respuesta formal (portal y correo) y cierre manual.
- [[ep-011-notificaciones]] — EP-011 (Fase 2): notificaciones en la app y correo de vinculación.
- [[ep-012-dashboard-y-metricas]] — EP-012 (Fase 3): tiempos hábiles, paneles de la PM y del equipo, y resumen del portal.
- [[ep-013-formularios-configurables]] — EP-013 (Fase 4): constructor, versionado y radicación con formulario por empresa.

Las 47 historias (`hu-001…hu-047`) están en `wiki/scrum/historias-de-usuario/` y se enlazan desde su épica y desde el tablero.

## Proceso

- [[trabajar-con-el-agente]] — Orquestador, especialistas, ciclo por interacción y ejemplos.
- [[flujo-de-trabajo-github]] — Puesta en marcha del repo, ramas, commits, PR y wiki en equipo.
- [[estrategia-de-pruebas]] — TDD, niveles de prueba y pruebas obligatorias por invariante.

## Fuentes

- [[fuente-prd-v0-1]] — PRD v0.1 (2026-10-05): resumen y mapa de secciones a páginas.
- [[fuente-patron-llm-wiki]] — Metodología LLM Wiki y cómo se instanció aquí.
- [[fuente-prototipo-dataticket-html]] — Prototipo navegable `DataTicket.html`: qué se adopta (estilo) y contradicciones con el PRD.

## Síntesis

- [[analisis-prd-vs-docker-compose]] — Brechas del Compose original frente al PRD y cómo se resolvieron.
- [[plan-goal-login-y-loop-chat]] — Plan de ejecución: `/goal` del login (`goal_login.md`) y `/loop` del chat SignalR (`loop_chat.md`), decisiones D1–D6 y riesgos.

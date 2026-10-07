---
title: Índice de la wiki
type: indice
status: vigente
tags: [indice]
sources: []
aliases: [Índice, Inicio, Home]
created: 2026-10-07
updated: 2026-10-07
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
- [[autenticacion-identity]] — Diseño de login con ASP.NET Core Identity (propuesta).
- [[tiempo-real-signalr]] — Hub `/hubs/tickets`, persistir antes de publicar, reconexión y revocación.
- [[persistencia-postgresql]] — EF Core + Npgsql, migraciones, multiempresa y auditoría.
- [[entorno-docker]] — Servicios, Compose Watch, variables, Dockerfiles, verificación y problemas frecuentes.
- [[stack-y-versiones]] — Versiones verificadas, soporte y política de actualización.

## Decisiones (ADR)

- [[adr-0001-monorepo-contenedorizado]] — Monorepo con backend, frontend, wiki y Compose. *Aceptada.*
- [[adr-0002-backend-hexagonal]] — Puertos y adaptadores verificados por pruebas. *Aceptada.*
- [[adr-0003-frontend-mvc]] — MVC por módulo con fronteras en oxlint. *Aceptada.*
- [[adr-0004-autenticacion-cookie-mismo-origen]] — Identity con cookie same-origin. *Propuesta.*
- [[adr-0005-signalr-para-chat]] — SignalR + PostgreSQL para el chat. *Aceptada.*
- [[adr-0006-urls-publicas-azure-blob]] — URL pública permanente para adjuntos (riesgo aceptado). *Aceptada.*
- [[adr-0007-entorno-local-compose-watch]] — Compose Watch, Mailpit y Azurite. *Aceptada.*

## Proceso

- [[trabajar-con-el-agente]] — Orquestador, especialistas, ciclo por interacción y ejemplos.
- [[flujo-de-trabajo-github]] — Puesta en marcha del repo, ramas, commits, PR y wiki en equipo.
- [[estrategia-de-pruebas]] — TDD, niveles de prueba y pruebas obligatorias por invariante.

## Fuentes

- [[fuente-prd-v0-1]] — PRD v0.1 (2026-10-05): resumen y mapa de secciones a páginas.
- [[fuente-patron-llm-wiki]] — Metodología LLM Wiki y cómo se instanció aquí.

## Síntesis

- [[analisis-prd-vs-docker-compose]] — Brechas del Compose original frente al PRD y cómo se resolvieron.

---
title: "EP-003 — Empresas, usuarios y equipos"
type: epica
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/epica, producto/roles, dominio]
sources: ["PRD.md §5", "PRD.md §6.2", "PRD.md §12", "PRD.md §13", "PRD.md §16"]
aliases: ["EP-003", "Empresas, usuarios y equipos"]
fase_prd: "0"
criterios_prd: ["CA-01"]
historias: ["[[hu-007-administrar-empresas-cliente]]", "[[hu-008-administrar-usuarios-cliente]]", "[[hu-009-administrar-usuarios-internos-y-equipos]]", "[[hu-010-datos-sinteticos-de-desarrollo]]"]
dependencias: ["[[ep-002-identidad-y-acceso]]", "[[ep-004-auditoria-append-only]]"]
created: 2026-10-07
updated: 2026-10-07
---

# EP-003 — Empresas, usuarios y equipos

Data Global administra dentro de DataTicket las empresas cliente, sus usuarios (Solicitante y Coordinador), los usuarios internos con sus roles y la pertenencia a los equipos Desarrollo y Producción; el entorno de desarrollo cuenta con datos sintéticos reproducibles.

## Objetivo

Que el administrador pueda dar de alta, modificar y desactivar empresas y cuentas sin tocar la base de datos, que cada usuario cliente quede ligado a exactamente una empresa (base del aislamiento multiempresa) y que el equipo pueda probar todos los perfiles con datos sintéticos, nunca reales (PRD §5, §12, §16.5).

## Valor esperado

- «Usuarios y asignaciones futuras se administran dentro de DataTicket» (PRD §5): el piloto no depende de scripts manuales.
- La empresa de un usuario cliente es un dato administrado por Data Global, no elegido por el cliente, lo que sostiene PRD CA-01.
- Las fases 1 y 2 encuentran listos los equipos (Desarrollo, Producción), la PM y los administradores para triage, asignación y chat.
- Un entorno local con ~50 empresas sintéticas permite probar listados, filtros y aislamiento con volumen realista (PRD §16.5).

## Fase del PRD

Fase 0 — Base técnica: «autenticación, empresas, usuarios y permisos» (PRD §13). Prioridad: bloqueante.

## Actores

- Administrador de DataTicket (`Administrator`).
- PM (rol `ProductManager`, Elizabeth), como usuaria administrada y posible administradora de cuentas (a ratificar).
- Usuarios cliente administrados: Solicitante (`Requester`) y Coordinador de empresa (`CompanyCoordinator`).
- Integrantes de los equipos Desarrollo (`Team.Development`) y Producción (`Team.Production`).
- Integrante del equipo de desarrollo del repositorio (consumidor de los datos sintéticos).

## Alcance

- Crear, renombrar, listar, desactivar y reactivar empresas cliente ([[hu-007-administrar-empresas-cliente|HU-007]]).
- Crear (invitar), cambiar de perfil, desactivar y reactivar usuarios cliente de una empresa ([[hu-008-administrar-usuarios-cliente|HU-008]]).
- Crear (invitar) usuarios internos, asignar roles `ProductManager`/`Administrator` y pertenencia a equipos ([[hu-009-administrar-usuarios-internos-y-equipos|HU-009]]).
- Semilla idempotente de datos sintéticos solo en Development ([[hu-010-datos-sinteticos-de-desarrollo|HU-010]]).

## Fuera de alcance

- Que el cliente cree o administre sus propias cuentas (PRD §5: las crea Data Global).
- Borrado físico de empresas o usuarios (se desactivan para conservar tickets y auditoría; propuesta).
- Mover un usuario cliente a otra empresa (no previsto en el PRD; propuesta: no permitido).
- Catálogo de categorías (V-12) y plantillas de formulario por empresa ([[ep-013-formularios-configurables]]).
- Semilla de tickets: se amplía cuando existan tickets en la Fase 1 ([[ep-005-radicacion-de-tickets]]).

## Requisitos y reglas de negocio

- Cada ticket pertenece a una empresa cliente; toda consulta del portal se restringe por empresa y permisos en servidor (PRD §12).
- Las cuentas de empresas cliente las crea, invita y desactiva Data Global; no hay registro público (PRD §5).
- Perfiles cliente: Solicitante (ve sus propios tickets) y Coordinador (ve los de toda su empresa) (PRD §5).
- Equipos iniciales: Desarrollo (Juan David, Laura, Brayan, Cristian, Kevin) y Producción (Julián, Elizabeth, Gerardo); Elizabeth es PM y pertenece a Producción; Julián lidera Producción (PRD §5).
- Pertenecer a un equipo no da acceso al detalle ni al chat de un ticket: el acceso es por asociación explícita, salvo Elizabeth (PRD §6.2.5, §10).
- Los datos de clientes de prueba son sintéticos; no se copian nombres, documentos ni conversaciones reales (PRD §12). Referencia: ~50 empresas de prueba con tickets en estados diversos (PRD §16.5).

## Criterios del PRD cubiertos

- PRD CA-01 — Aislamiento empresa A ↔ empresa B (parcial: vinculación de cada usuario cliente a exactamente una empresa y pruebas negativas entre empresas en la administración) → [[criterios-de-aceptacion]]

## Dependencias

- [[ep-002-identidad-y-acceso]]: Identity, `ICurrentUser`, políticas por rol ([[hu-004-contexto-de-usuario-y-autorizacion|HU-004]]) y mecanismo de invitación ([[hu-005-invitar-y-activar-cuentas|HU-005]]).
- [[ep-004-auditoria-append-only]]: puerto `IAuditLog` y tabla `AuditEntry` ([[hu-011-registrar-eventos-auditables|HU-011]], sugerida para el Sprint 2). Riesgo de orden: HU-007 a HU-009 están en el Sprint 1.
- [[ep-001-fundaciones-tecnicas]]: shell del Espacio interno ([[hu-002-shell-y-navegacion-por-rol|HU-002]]).

## Historias de usuario

- [[hu-007-administrar-empresas-cliente|HU-007 — Administrar empresas cliente]]
- [[hu-008-administrar-usuarios-cliente|HU-008 — Administrar usuarios de empresas cliente]]
- [[hu-009-administrar-usuarios-internos-y-equipos|HU-009 — Administrar usuarios internos, equipos y roles]]
- [[hu-010-datos-sinteticos-de-desarrollo|HU-010 — Datos sintéticos de desarrollo]]

## Criterio de completitud

- [ ] HU-007 a HU-010 están `Completada` con su matriz de evidencia en `Cumple`.
- [ ] Ningún usuario cliente puede existir sin empresa ni con más de una (restricción de dominio y de base de datos probada).
- [ ] Desactivar una empresa o un usuario impide el inicio de sesión y revoca las sesiones vigentes dentro del intervalo de revalidación acordado.
- [ ] [[modelo-de-dominio]] y [[glosario]] reflejan lo implementado (`Company`, `User`, estado de la cuenta, equipos).
- [ ] La semilla solo se ejecuta en Development y ningún dato sembrado es real.
- [ ] No quedan dependencias bloqueantes dentro del alcance.

## Riesgos e incógnitas

- **Roles frente a equipos** (`Development`/`Production` como roles Identity en [[autenticacion-identity]] frente a `Team.Development`/`Team.Production` en [[glosario]]): decide cómo se modela la pertenencia a equipos en HU-009.
- **Auditoría de acciones administrativas**: el PRD §12 enumera eventos del ticket, no de administración. Auditar altas, cambios de perfil y desactivaciones es una propuesta (V-11).
- **Orden respecto de HU-011**: si `IAuditLog` no existe al empezar el Sprint 1, hay que adelantar un mínimo del puerto y la tabla o reordenar los sprints.
- **Administradores del piloto** sin nombrar (V-15): la semilla usa un administrador sintético.
- **Volumen de datos sintéticos** pendiente de confirmar (PRD §16.5, [[pendientes]] §2).
- **Liderazgo de Julián**: el PRD no le asocia permisos; modelarlo como dato es una propuesta.
- **Quién administra cuentas**: ¿solo `Administrator` o también la PM? ([[autenticacion-identity]] menciona a ambos para invitar).

## Relacionado

- [[tablero-scrum]] · [[fases-y-alcance]] · [[roles-y-permisos]] · [[modelo-de-dominio]] · [[equipo-data-global]] · [[elizabeth-pm]] · [[julian-produccion]] · [[autenticacion-identity]] · [[auditoria]] · [[persistencia-postgresql]] · [[ep-002-identidad-y-acceso]]

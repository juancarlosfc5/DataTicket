---
title: "HU-010 — Datos sintéticos de desarrollo"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, arquitectura/infra, arquitectura/datos, proceso]
sources: ["PRD.md §5", "PRD.md §12", "PRD.md §16"]
aliases: ["HU-010", "Datos sintéticos de desarrollo", "Semilla de desarrollo"]
epica: "[[ep-003-empresas-usuarios-y-equipos]]"
criterios_prd: []
componentes: ["Backend (Infrastructure: semilla)", "Backend (Api: arranque en Development)", "Persistencia PostgreSQL", "Identity", "Docker/Compose"]
dificultad: "Medio"
sprint_sugerido: "Sprint 1"
dependencias: ["[[hu-007-administrar-empresas-cliente]]", "[[hu-008-administrar-usuarios-cliente]]", "[[hu-009-administrar-usuarios-internos-y-equipos]]"]
relacionadas: ["[[hu-003-iniciar-y-cerrar-sesion]]", "[[hu-013-radicar-ticket]]", "[[hu-021-cambiar-estado-interno]]", "[[hu-040-panel-global-de-la-pm]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-010 — Datos sintéticos de desarrollo

Una semilla idempotente, que solo se ejecuta en el entorno Development, crea empresas cliente de prueba, usuarios de cada perfil y el equipo interno del piloto con correos ficticios, para que cualquier integrante del equipo levante el stack y pruebe todos los perfiles sin usar nunca datos reales. Se ampliará con tickets en estados diversos cuando existan en la Fase 1.

## Historia de usuario

**COMO** integrante del equipo de desarrollo del repositorio  
**QUIERO** que al levantar el entorno local la base tenga empresas, usuarios cliente, usuarios internos y equipos sintéticos listos para iniciar sesión  
**PARA** desarrollar y probar a mano cada perfil y el aislamiento entre empresas con volumen realista, sin copiar datos reales de clientes

## Contexto

PRD §12: «Los datos de clientes de prueba son sintéticos; no se copian nombres, documentos ni conversaciones reales». PRD §16.5: volumen a sembrar pendiente; «la referencia acordada es aproximadamente 50 empresas de prueba con tickets en estados diversos, sin replicar información real» (también en [[pendientes]] §2). PRD §5 fija el equipo del piloto. Los administradores del piloto no están nombrados (V-15). [[equipo-del-repositorio]] recuerda que Laura, Juan David y Brayan construyen y usarán DataTicket: sus cuentas de prueba deben ser sintéticas.

## Alcance

- Semilla en `Infrastructure` (propuesta: un sembrador por módulo detrás de una interfaz común, nombre propuesto `IDevelopmentSeeder`, ratificar en T-01) que se ejecuta al arrancar **solo** si el entorno es `Development` y `Seed:Enabled = true` (claves propuestas).
- Idempotencia por identificadores deterministas: una segunda ejecución no crea duplicados ni revierte cambios manuales.
- Empresas: 50 «Empresa Sintética 01» … «Empresa Sintética 50», 5 de ellas inactivas (propuesta de reparto).
- Usuarios cliente por empresa (propuesta): 2 Solicitantes y 1 Coordinador activos; además, en algunas empresas, una cuenta `Invited` y una `Deactivated`.
- Usuarios internos: el equipo del piloto (PRD §5) con correos ficticios `@dataticket.local` (Elizabeth `ProductManager` + Producción; Julián Producción y líder; Gerardo Producción; Juan David, Laura, Brayan, Cristian y Kevin en Desarrollo) y un administrador sintético `admin@dataticket.local`.
- Contraseña común de desarrollo leída de `Seed:DefaultPassword` en `appsettings.Development.json` (valor local de ejemplo, nunca un secreto real).
- Resumen en el log al terminar (creados y omitidos por tipo).
- Compose: activar la semilla en el servicio `backend` de desarrollo.

## Fuera de alcance

- Tickets en estados diversos: se añaden cuando existan tickets, como tarea de [[hu-013-radicar-ticket|HU-013]] (radicación) y [[hu-021-cambiar-estado-interno|HU-021]] (estados); esta HU deja el punto de extensión.
- Chat, adjuntos, respuestas formales y auditoría sintética (fases 1–2).
- Datos para entornos distintos de Development (prueba, producción).
- Fijar el volumen definitivo (PRD §16.5): esta HU usa la referencia de ~50 empresas y registra el pendiente.

## Requisitos y reglas de negocio

- Datos de clientes de prueba sintéticos; sin nombres, documentos ni conversaciones reales (PRD §12).
- Referencia de ~50 empresas de prueba con tickets en estados diversos (PRD §16.5).
- Equipos iniciales del piloto y doble rol de Elizabeth (PRD §5).
- Las cuentas se crean respetando las mismas reglas de dominio que la administración (un usuario cliente pertenece a exactamente una empresa; un interno no tiene empresa).
- Nunca se escriben secretos reales en el repositorio (`AGENTS.md` §11).

## Invariantes en juego

- Invariante 1 (`AGENTS.md` §7): la semilla respeta la vinculación única usuario cliente ↔ empresa y produce datos suficientes para probar A↔B a mano.
- Invariante 9: las cuentas sembradas no habilitan ningún registro público.

## Criterios del PRD cubiertos

- Ninguno de forma directa. Facilita la verificación manual de PRD CA-01, CA-02 y CA-03 con volumen realista → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-003-empresas-usuarios-y-equipos]]
- Dependencias: [[hu-007-administrar-empresas-cliente|HU-007]] (`Company`), [[hu-008-administrar-usuarios-cliente|HU-008]] (usuarios cliente), [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]] (roles y equipos), aplicación de migraciones al arrancar en Development ([[hu-003-iniciar-y-cerrar-sesion|HU-003]], T-05).
- Relacionadas: [[hu-003-iniciar-y-cerrar-sesion|HU-003]], [[hu-013-radicar-ticket|HU-013]] y [[hu-021-cambiar-estado-interno|HU-021]] (ampliación con tickets), [[hu-040-panel-global-de-la-pm|HU-040]] (paneles con volumen).
- Decisiones: volumen de datos sintéticos pendiente (PRD §16.5, [[pendientes]] §2); administradores del piloto (V-15).

## Componentes afectados

- Backend `Infrastructure`: sembradores de empresas, usuarios cliente y usuarios internos; uso de `UserManager` para crear cuentas con contraseña.
- Backend `Api`: invocación al arrancar, condicionada al entorno.
- Persistencia PostgreSQL: solo datos (sin cambio de esquema).
- Identity: cuentas, roles y equipos.
- Docker/Compose: variable `Seed__Enabled` del servicio `backend`.
- Configuración: `appsettings.Development.json` (`Seed:DefaultPassword`, valor local de ejemplo).

## Dificultad

**Nivel:** Medio

**Justificación:** Sin interfaz ni contrato REST, pero exige idempotencia real, condicionamiento estricto al entorno, coherencia con las reglas de dominio de tres HU previas y un diseño extensible para tickets.

## Contrato backend ↔ frontend

No aplica: la HU no expone rutas REST ni métodos del hub; el frontend consume las cuentas sembradas a través de los contratos ya existentes (`/api/auth/login`, `/api/auth/me`). Su contrato es de **configuración** (propuesta; ratificar en T-01):

| Clave | Dónde | Valor local | Efecto |
|---|---|---|---|
| `Seed:Enabled` | `docker-compose.yml` (`Seed__Enabled`) | `true` | Activa la semilla al arrancar; ignorada fuera de `Development` |
| `Seed:DefaultPassword` | `appsettings.Development.json` | valor de ejemplo que cumple la política de HU-003 | Contraseña de todas las cuentas sembradas activas |

Cuentas de referencia (propuesta):

| Perfil | Correo |
|---|---|
| Administrador sintético | `admin@dataticket.local` |
| PM | `elizabeth@dataticket.local` |
| Producción (líder) | `julian@dataticket.local` |
| Producción | `gerardo@dataticket.local` |
| Desarrollo | `juan.david@dataticket.local`, `laura@dataticket.local`, `brayan@dataticket.local`, `cristian@dataticket.local`, `kevin@dataticket.local` |
| Coordinador de la empresa 01 | `coordinador@empresa-sintetica-01.test` |
| Solicitantes de la empresa 01 | `solicitante1@empresa-sintetica-01.test`, `solicitante2@empresa-sintetica-01.test` |

## Tareas de desarrollo

- [ ] **T-01 — Contrato de configuración y reparto** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar claves de configuración, reparto (empresas activas/inactivas, usuarios por empresa, estados), correos de referencia, identificadores deterministas, y el punto de extensión para tickets. Registrar en [[pendientes]] que el volumen sigue siendo una referencia (PRD §16.5).
- [ ] **T-02 — Pruebas de la semilla primero** · Capa: Backend (pruebas) · Dificultad: Medio  
  Descripción: pruebas de integración en rojo para CHU-01 a CHU-08: conteos, idempotencia, condición de entorno, patrones sintéticos, reglas de dominio y login con cuentas sembradas.
- [ ] **T-03 — Sembradores** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: sembradores de empresas, usuarios cliente e internos usando las entidades de dominio y los servicios de cuentas (no SQL directo que salte validaciones); identificadores deterministas; omitir lo existente sin modificarlo; resumen en el log.
- [ ] **T-04 — Arranque condicionado** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: invocar la semilla tras aplicar migraciones solo si `IHostEnvironment.IsDevelopment()` y `Seed:Enabled = true`; fuera de Development registrar una advertencia y no ejecutar nada.
- [ ] **T-05 — Configuración local y Compose** · Capa: Docker/Compose · Dificultad: Bajo  
  Descripción: `Seed:DefaultPassword` de ejemplo en `appsettings.Development.json`; `Seed__Enabled: "true"` en el servicio `backend` de `docker-compose.yml`; comprobar que un volumen vacío queda sembrado con `docker compose up --build`.
- [ ] **T-06 — Frontend** · Capa: Frontend · Dificultad: Bajo  
  Descripción: sin cambios de código. Verificar manualmente que cada cuenta de referencia entra a su superficie (portal o interno) con el shell de [[hu-002-shell-y-navegacion-por-rol|HU-002]].
- [ ] **T-07 — Notas para la wiki** · Capa: Documentación · Dificultad: Bajo  
  Descripción: [[entorno-docker]] (semilla, cuentas de prueba, cómo reiniciar con `docker compose down -v`), [[persistencia-postgresql]] (semilla en Development), [[equipo-del-repositorio]] (cuentas sintéticas), [[pendientes]] (volumen).

## Criterios de aceptación

### CHU-01 — La semilla crea el volumen de referencia

**Dado** una base vacía con las migraciones aplicadas, entorno `Development` y `Seed:Enabled = true`  
**Cuando** arranca el backend  
**Entonces** existen 50 empresas «Empresa Sintética 01» … «Empresa Sintética 50» (45 activas y 5 inactivas), cada una con 2 Solicitantes y 1 Coordinador activos, al menos 3 cuentas cliente `Invited` y 3 `Deactivated` en total, el administrador sintético y los 8 integrantes del piloto; y el log muestra un resumen con esos conteos.

### CHU-02 — Idempotencia

**Dado** una base ya sembrada en la que además se renombró «Empresa Sintética 01» a «Empresa Sintética 01 Renombrada»  
**Cuando** el backend arranca de nuevo con la semilla activa  
**Entonces** los conteos de empresas y usuarios no cambian, no hay correos ni empresas duplicados, la empresa renombrada conserva su nombre nuevo y el log informa que todo se omitió.

### CHU-03 — Nunca fuera de Development

**Dado** el backend con entorno `Staging` (y otro con `Production`) y `Seed:Enabled = true`  
**Cuando** arranca  
**Entonces** no se crea ninguna empresa ni cuenta y el log registra una advertencia de que la semilla solo corre en Development; y en `Development` con `Seed:Enabled = false` tampoco se crea nada.

### CHU-04 — Solo datos sintéticos

**Dado** la base sembrada  
**Cuando** la prueba recorre empresas y cuentas  
**Entonces** todos los nombres de empresa cumplen el patrón `^Empresa Sintética \d{2}$`, todos los correos de clientes terminan en `.test` y todos los internos en `@dataticket.local`, y no hay ningún dato con formato de documento de identidad, teléfono ni dominio real.

### CHU-05 — La semilla respeta el dominio

**Dado** la base sembrada  
**Cuando** se consultan las cuentas  
**Entonces** cada usuario cliente pertenece a exactamente una empresa y tiene exactamente un perfil (`Requester` o `CompanyCoordinator`); ningún interno tiene empresa; Elizabeth tiene `ProductManager` y equipo Producción; Julián, equipo Producción (y liderazgo si [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]] lo adopta); los 5 de Desarrollo tienen solo ese equipo; y el administrador sintético tiene solo `Administrator`.

### CHU-06 — Cada perfil inicia sesión (prueba A↔B incluida)

**Dado** la base sembrada y la contraseña `Seed:DefaultPassword`  
**Cuando** se inicia sesión con `solicitante1@empresa-sintetica-01.test`, `coordinador@empresa-sintetica-02.test`, `elizabeth@dataticket.local`, `admin@dataticket.local` y `kevin@dataticket.local`  
**Entonces** cada uno obtiene `204`, y su `GET /api/auth/me` devuelve el perfil esperado; el Solicitante de la empresa 01 ve `company.name = "Empresa Sintética 01"` y nunca la 02.

### CHU-07 — Estados de cuenta y empresa se respetan

**Dado** un usuario cliente activo de una empresa sembrada como inactiva y una cuenta sembrada `Deactivated`  
**Cuando** intentan iniciar sesión con la contraseña de la semilla  
**Entonces** ambos reciben el `401` genérico de [[hu-003-iniciar-y-cerrar-sesion|HU-003]].

### CHU-08 — Sin secretos reales

**Dado** el repositorio tras el cambio  
**Cuando** se revisan `appsettings.Development.json`, `docker-compose.yml` y el código de la semilla  
**Entonces** la única contraseña presente es el valor local de ejemplo `Seed:DefaultPassword`, documentado como de desarrollo, y no se lee ningún secreto de fuentes externas.

### CHU-09 — El entorno local queda listo con un comando

**Dado** un clon limpio y `docker compose down -v` ejecutado  
**Cuando** se ejecuta `docker compose up --build`  
**Entonces** el backend arranca *healthy*, el log muestra el resumen de la semilla y en `http://localhost:5173/ingresar` se puede entrar con `admin@dataticket.local` al Espacio interno y con `solicitante1@empresa-sintetica-01.test` al Portal del cliente.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-09 validados con evidencia.
- [ ] **DoD-02** — Pruebas de integración escritas primero y en verde, con nombres por comportamiento: `Semilla_crea_50_empresas_y_sus_usuarios`, `Semilla_es_idempotente_y_no_revierte_cambios`, `Semilla_no_corre_fuera_de_Development`, `Semilla_no_corre_si_esta_desactivada`, `Datos_sembrados_son_sinteticos`, `Semilla_respeta_reglas_de_dominio`, `Cuentas_sembradas_inician_sesion_con_su_perfil`, `Empresa_inactiva_y_cuenta_desactivada_sembradas_no_entran`.
- [ ] **DoD-03** — `cd backend && dotnet test` en verde, incluidas `DataTicket.ArchitectureTests`.
- [ ] **DoD-04** — `npm --prefix frontend run lint`, `npm --prefix frontend test` y `npm --prefix frontend run build` siguen en verde (sin cambios de frontend).
- [ ] **DoD-05** — Sin migraciones nuevas (la HU no cambia el esquema); confirmado en el PR.
- [ ] **DoD-06** — `docker compose down -v && docker compose up --build` verificado (CHU-09) con el extracto del log de la semilla en el PR.
- [ ] **DoD-07** — Wiki actualizada vía Notas para la wiki: [[entorno-docker]], [[persistencia-postgresql]], [[equipo-del-repositorio]], [[pendientes]] (§2 volumen; V-15 administradores).
- [ ] **DoD-08** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos (condición de entorno, secretos, datos reales).
- [ ] **DoD-09** — PR revisado y aprobado por otra persona del equipo; trazabilidad de HU y épica actualizada.

## Evidencia de validación

| Elemento | Resultado | Evidencia | Observación |
|---|---|---|---|
| CHU-01 | Pendiente | — | — |
| CHU-02 | Pendiente | — | — |
| CHU-03 | Pendiente | — | — |
| CHU-04 | Pendiente | — | — |
| CHU-05 | Pendiente | — | — |
| CHU-06 | Pendiente | — | — |
| CHU-07 | Pendiente | — | — |
| CHU-08 | Pendiente | — | — |
| CHU-09 | Pendiente | — | — |
| DoD-01 | Pendiente | — | — |
| DoD-02 | Pendiente | — | — |
| DoD-03 | Pendiente | — | — |
| DoD-04 | Pendiente | — | — |
| DoD-05 | Pendiente | — | — |
| DoD-06 | Pendiente | — | — |
| DoD-07 | Pendiente | — | — |
| DoD-08 | Pendiente | — | — |
| DoD-09 | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- **Nombres del equipo interno**: el PRD §5 nombra al equipo del piloto; la semilla usa esos nombres de pila con correos ficticios `@dataticket.local`. Si el equipo prefiere nombres totalmente sintéticos también para internos, se cambia en T-01.
- **Ampliación con tickets** (PRD §16.5, «tickets en estados diversos»): [[hu-013-radicar-ticket|HU-013]] y [[hu-021-cambiar-estado-interno|HU-021]] deben añadir un sembrador de tickets que cubra todos los `TicketStatus` y varias combinaciones de `Urgency`/`Impact`, distribuidos entre empresas activas.
- El reparto (45/5 empresas, 2+1 usuarios por empresa) es propuesta; el volumen definitivo sigue pendiente ([[pendientes]] §2).
- La contraseña común de la semilla es aceptable solo porque la semilla no corre fuera de Development (CHU-03).

## Relacionado

- [[ep-003-empresas-usuarios-y-equipos]] · [[tablero-scrum]] · [[entorno-docker]] · [[persistencia-postgresql]] · [[equipo-data-global]] · [[equipo-del-repositorio]] · [[pendientes]] · [[fases-y-alcance]]

---
title: "HU-042 — Resumen de estados en el portal"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/portal, producto/dashboard, producto/seguridad, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §4", "PRD.md §5", "PRD.md §6.4", "PRD.md §10", "PRD.md §13", "PRD.md §14"]
aliases: ["HU-042", "Resumen de estados en el portal", "Resumen de estados en el portal del cliente"]
epica: "[[ep-012-dashboard-y-metricas]]"
criterios_prd: [CA-01, CA-02, CA-10, CA-13]
componentes: [Backend Application, Backend Infrastructure, Backend Api, Frontend models, Frontend controllers, Frontend views]
dificultad: "Bajo"
sprint_sugerido: "Sprint 8"
dependencias: ["[[hu-024-portal-solicitante-consulta-tickets]]", "[[hu-025-portal-coordinador-consulta-tickets]]"]
relacionadas: ["[[hu-039-calculo-de-tiempos-habiles]]", "[[hu-040-panel-global-de-la-pm]]", "[[hu-034-portal-respuesta-formal]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-042 — Resumen de estados en el portal

Resumen en el portal del cliente con el conteo de tickets por `ClientStatus` (el solicitante, los suyos; el coordinador, los de su empresa), sin SLA, metas, estados internos ni tiempos internos (PRD §10, §14.13).

## Historia de usuario

**COMO** solicitante o coordinador de una empresa cliente  
**QUIERO** ver cuántas de mis solicitudes (o de mi empresa) están recibidas, en atención, con solución entregada o cerradas  
**PARA** tener una visión rápida del estado del soporte sin revisar ticket por ticket

## Contexto

El PRD pide que el panel diferencie los "tickets visibles a una empresa cliente" (PRD §10) y ubica el "portal con estados resumidos" en la Fase 3 (PRD §13). Por [[adr-0008-portal-cliente-en-fase-1]], la consulta básica se adelantó a la Fase 1 ([[hu-024-portal-solicitante-consulta-tickets|HU-024]], [[hu-025-portal-coordinador-consulta-tickets|HU-025]]) y el resumen por estados queda en la Fase 3. Reutiliza el alcance de `IClientTicketQueries`. El PRD no dice qué tiempos ve el cliente; se propone que ninguno.

## Alcance

- Endpoint de resumen por `ClientStatus` con el alcance del rol (propio o empresa).
- Tarjetas de resumen en la parte superior de la lista del portal que actúan como filtro.

## Fuera de alcance

- Tiempos, antigüedad o promedios para el cliente (propuesta: ninguno, el PRD no los menciona para el portal).
- SLA, metas, semáforos y fechas comprometidas (PRD §4, §14.13).
- Conteos por estado interno.
- Exportación (PRD §10, §16.4).

## Requisitos y reglas de negocio

- El panel diferencia los tickets visibles a una empresa cliente de los datos internos (PRD §10).
- El solicitante ve sus tickets; el coordinador, los de su empresa (PRD §5 regla 1, §10).
- El cliente solo ve el estado resumido: `Received`, `InProgress`, `SolutionDelivered`, `Closed` (PRD §6.4, §14.2).
- El cliente no recibe metas de SLA no acordadas (PRD §14.13).
- El cliente no ve estados de Desarrollo/PR/Producción (PRD §14.10).
- (propuesta) Los cuatro estados aparecen siempre, con 0 cuando no hay tickets.
- (propuesta) No se muestran tiempos al cliente.

## Invariantes en juego

- AGENTS §7.1 — conteos filtrados por empresa y rol en backend.
- AGENTS §7.2 — sin estados técnicos.

## Criterios del PRD cubiertos

- PRD CA-13 (parcial: "el cliente no recibe metas de SLA no acordadas") → [[criterios-de-aceptacion]]
- PRD CA-01 (parcial: los conteos no incluyen tickets de otra empresa) → [[criterios-de-aceptacion]]
- PRD CA-02 (parcial: alcance solicitante/coordinador en el resumen) → [[criterios-de-aceptacion]]
- PRD CA-10 (parcial: sin estados técnicos) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-012-dashboard-y-metricas]]
- Dependencias: [[hu-024-portal-solicitante-consulta-tickets|HU-024]] y [[hu-025-portal-coordinador-consulta-tickets|HU-025]] (alcance, `IClientTicketQueries`, vistas del portal).
- Relacionadas: [[hu-039-calculo-de-tiempos-habiles|HU-039]] (sus tiempos no se exponen aquí), [[hu-040-panel-global-de-la-pm|HU-040]], [[hu-034-portal-respuesta-formal|HU-034]].
- Decisiones: [[adr-0008-portal-cliente-en-fase-1]]; hallazgo 2 (CA-13 en el MVP frente a Fase 3).

## Componentes afectados

- Backend Application: consulta de resumen (nombre propuesto `GetClientTicketSummary`, ratificar en T-01 y registrar en el glosario) sobre `IClientTicketQueries`.
- Backend Infrastructure: conteo agrupado por estado interno y mapeo a `ClientStatus`.
- Backend Api: `GET /api/portal/tickets/summary`.
- Frontend models/controllers/views: módulo `portal`.

## Dificultad

**Nivel:** Bajo

**Justificación:** consulta agregada localizada que reutiliza el alcance y las pruebas de HU-024/HU-025; el riesgo está cubierto por pruebas negativas ya diseñadas.

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01.

**`GET /api/portal/tickets/summary`**

Respuesta `200 OK`:

```json
{
  "scope": "company",
  "total": 9,
  "counts": [
    { "clientStatus": "Received", "count": 2 },
    { "clientStatus": "InProgress", "count": 5 },
    { "clientStatus": "SolutionDelivered", "count": 1 },
    { "clientStatus": "Closed", "count": 1 }
  ]
}
```

- `scope`: `own` (solicitante) o `company` (coordinador), calculado por el backend.
- Claves permitidas: solo las del ejemplo.

Errores (ProblemDetails):

| Código | Cuándo |
|---|---|
| 401 | Sin sesión |
| 403 | Usuario interno (propuesta, como en HU-024) |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar ruta, claves y la propuesta de no mostrar tiempos; confirmar que la ruta `summary` no colisiona con `/api/portal/tickets/{ticketId}` (el `ticketId` es GUID).
- [ ] **T-02 — Caso de uso** · Capa: Backend Application · Dificultad: Bajo  
  Descripción: primero pruebas unitarias: alcance según rol, cuatro estados siempre presentes, usuario interno prohibido; luego `GetClientTicketSummary`.
- [ ] **T-03 — Conteo agrupado** · Capa: Backend Infrastructure · Dificultad: Bajo  
  Descripción: `GROUP BY` del estado interno y mapeo a `ClientStatus` en el adaptador de `IClientTicketQueries`, con el mismo predicado de alcance que HU-024/HU-025; pruebas de integración con dos empresas.
- [ ] **T-04 — Endpoint** · Capa: Backend Api · Dificultad: Bajo  
  Descripción: `GET /api/portal/tickets/summary`; pruebas de integración de 200/401/403 y de allowlist de claves.
- [ ] **T-05 — Modelo y gateway** · Capa: Frontend models · Dificultad: Bajo  
  Descripción: tipo `ClientTicketSummary` y normalización (estado ausente → 0); pruebas Vitest.
- [ ] **T-06 — Controlador** · Capa: Frontend controllers · Dificultad: Bajo  
  Descripción: `useClientTicketSummaryController`; al elegir una tarjeta aplica el filtro `clientStatus` del controlador de lista de HU-024; pruebas Vitest.
- [ ] **T-07 — Vista** · Capa: Frontend views · Dificultad: Bajo  
  Descripción: `ClientTicketSummaryView` con cuatro tarjetas ("Recibido", "En atención", "Solución entregada", "Cerrado"), sin colores de cumplimiento ni tiempos.
- [ ] **T-08 — Migración** · Capa: Persistencia · Dificultad: Bajo  
  Descripción: no aplica (sin cambio de esquema; reutiliza índices de HU-024).

## Criterios de aceptación

### CHU-01 — Resumen del solicitante

**Dado** el solicitante A1 con 1 ticket en `New`, 2 en `InDevelopment`, 1 en `PullRequestReview` y 1 en `Closed`, y A2 (misma empresa) con 3 tickets  
**Cuando** A1 pide `GET /api/portal/tickets/summary`  
**Entonces** recibe `scope = "own"`, `total = 5` y `Received = 1`, `InProgress = 3`, `SolutionDelivered = 0`, `Closed = 1`.

### CHU-02 — Resumen del coordinador con prueba negativa multiempresa

**Dado** la empresa A con 8 tickets (de A1 y A2) y la empresa B con 6  
**Cuando** el coordinador de A pide el resumen  
**Entonces** recibe `scope = "company"` y `total = 8`; ningún conteo incluye tickets de B; **y** el coordinador de B recibe `total = 6`.

### CHU-03 — Sin estados técnicos ni tiempos

**Dado** cualquier respuesta 200 del resumen  
**Cuando** se inspecciona el JSON  
**Entonces** sus claves son exactamente `scope`, `total`, `counts`, `clientStatus` y `count`, y el cuerpo crudo no contiene `InDevelopment`, `PullRequestReview`, `InProduction`, `BusinessMinutes`, `sla`, `target` ni `goal`.

### CHU-04 — Los cuatro estados siempre presentes

**Dado** un solicitante sin tickets  
**Cuando** pide el resumen  
**Entonces** recibe `total = 0` y los cuatro `clientStatus` con `count = 0`.

### CHU-05 — Autorización por rol

**Dado** Laura (interna) y una petición sin sesión  
**Cuando** llaman al resumen  
**Entonces** Laura recibe 403 (propuesta) y la petición sin sesión 401.

### CHU-06 — Coherencia con la lista

**Dado** el coordinador de A con `InProgress = 5` en el resumen  
**Cuando** pide `GET /api/portal/tickets?clientStatus=InProgress`  
**Entonces** `totalCount = 5`.

### CHU-07 — UI de resumen

**Dado** el portal con el resumen cargado  
**Cuando** el usuario pulsa la tarjeta "En atención"  
**Entonces** la lista se filtra por `InProgress`; **ningún** valor se muestra con colores de cumplimiento ni con tiempos; **y** si el resumen falla (500) la lista sigue visible y las tarjetas muestran "No disponible" con "Reintentar".

## Definition of Done

- [ ] CHU-01 a CHU-07 validados con evidencia.
- [ ] Pruebas en verde (`cd backend && dotnet test`): `ClientSummary_Solicitante_SoloSusTickets`, `ClientSummary_Coordinador_NoCuentaOtraEmpresa`, `ClientSummary_SinEstadosTecnicosNiTiempos`, `ClientSummary_CuatroEstadosSiemprePresentes`, `ClientSummary_UsuarioInterno_Prohibido`.
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Vitest, `npm --prefix frontend run lint` y `npm --prefix frontend run build` en verde.
- [ ] El stack levanta con `docker compose up --build` y el resumen aparece en el portal sintético.
- [ ] Sin migración (justificado en T-08).
- [ ] Wiki actualizada: [[dashboard-y-metricas]] (portal: resumen sin tiempos), [[estados-del-ticket]] (uso de `ClientStatus` en conteos), [[criterios-de-aceptacion]] (evidencia parcial de CA-13 para el cliente), [[glosario]].
- [ ] `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos, con revisión explícita de aislamiento multiempresa.
- [ ] PR revisado y aprobado por otra persona del equipo.

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
| DoD-01 Pruebas backend nombradas | Pendiente | — | — |
| DoD-02 Pruebas de arquitectura | Pendiente | — | — |
| DoD-03 Vitest, lint y build | Pendiente | — | — |
| DoD-04 `docker compose up --build` | Pendiente | — | — |
| DoD-05 Migración (no aplica) | Pendiente | — | — |
| DoD-06 Wiki actualizada | Pendiente | — | — |
| DoD-07 `quality-reviewer` | Pendiente | — | — |
| DoD-08 PR revisado | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- (propuesta) El cliente no ve tiempos; si la persona responsable del producto quiere mostrar, por ejemplo, la antigüedad, debe decidirse expresamente porque podría leerse como compromiso de servicio (PRD §14.13).
- **Hallazgo 2:** esta HU aporta a CA-13 en su parte de cliente; su ubicación en la Fase 3 hereda la ambigüedad sobre qué fases componen el MVP.

## Relacionado

- [[ep-012-dashboard-y-metricas]] · [[tablero-scrum]] · [[adr-0008-portal-cliente-en-fase-1]] · [[dashboard-y-metricas]] · [[estados-del-ticket]]
- [[roles-y-permisos]] · [[frontend-mvc]] · [[criterios-de-aceptacion]]

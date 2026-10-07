---
title: "HU-033 — Emitir respuesta formal"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/respuesta-formal, producto/archivos, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §6.4", "PRD.md §6.5", "PRD.md §8", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-033", "Emitir respuesta formal"]
epica: "[[ep-010-respuesta-formal-y-cierre]]"
criterios_prd: ["CA-09", "CA-11", "CA-14"]
componentes: ["Backend (Domain)", "Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Persistencia PostgreSQL", "Archivos (Blob)", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 7"
dependencias: ["[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-011-registrar-eventos-auditables]]", "[[hu-014-adjuntos-en-radicacion]]", "[[hu-017-cola-de-cobertura-y-tomar-ticket]]", "[[hu-018-asignar-y-agregar-participantes]]", "[[hu-020-detalle-interno-del-ticket]]", "[[hu-021-cambiar-estado-interno]]"]
relacionadas: ["[[hu-034-portal-respuesta-formal]]", "[[hu-035-correo-de-respuesta-formal]]", "[[hu-036-cerrar-ticket-manualmente]]", "[[hu-029-adjuntos-e-imagenes-en-chat]]", "[[hu-039-calculo-de-tiempos-habiles]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-033 — Emitir respuesta formal

Elizabeth (PM) o un administrador redacta y emite, desde el detalle interno del ticket, la respuesta formal con cuerpo de texto y adjuntos validados en backend. El ticket pasa a `SolutionDelivered` y la acción queda auditada (PRD §6.5, §14.11).

## Historia de usuario

**COMO** Elizabeth, PM de Data Global (o administrador que cubre su ausencia)  
**QUIERO** emitir la respuesta formal de un ticket con un texto y los archivos de la solución  
**PARA** comunicar al cliente el resultado por el canal formal del producto y dejar constancia auditable de la entrega

## Contexto

La respuesta formal es el único canal del producto para comunicar al cliente el resultado (PRD §6.5). Los desarrolladores preparan contexto y adjuntos en el chat, pero solo Elizabeth o un administrador emiten la comunicación final (PRD §5.7, §6.5). Emitirla lleva el ticket a "Solución entregada" (PRD §6.4). Esta HU crea la respuesta y la persiste; su visualización en el portal es [[hu-034-portal-respuesta-formal|HU-034]] y su envío por correo es [[hu-035-correo-de-respuesta-formal|HU-035]].

Hallazgo del plan de backlog: el [[modelo-de-dominio]] propone `Attachment` sin marca de contexto (radicación, chat o respuesta formal) ni de visibilidad al cliente. Sin esa marca, el portal no puede distinguir los adjuntos de la respuesta (visibles) de los del chat (internos). Esta HU introduce la marca como propuesta.

## Alcance

- Caso de uso `DeliverFormalResponse` con autorización en servidor por rol y por participación.
- Cuerpo de texto obligatorio y adjuntos opcionales (PDF, imágenes, XML y Excel de hasta 10 MB por archivo), validados en backend por extensión, tipo MIME, firma del contenido y tamaño.
- Marca de contexto del adjunto: `AttachmentContext` = `FormalResponse` (nombre propuesto, ratificar en T-01 y registrar en el glosario), visible al cliente.
- Transición del ticket a `SolutionDelivered` desde los estados permitidos (propuesta sujeta a V-01).
- Entrada de auditoría con emisor, fecha/hora, estado anterior y nuevo.
- Formulario en el detalle interno del ticket con confirmación previa al envío.
- Rechazo de una segunda respuesta mientras V-13 siga abierta.

## Fuera de alcance

- Mostrar la respuesta en el portal → [[hu-034-portal-respuesta-formal|HU-034]].
- Enviar el correo al cliente → [[hu-035-correo-de-respuesta-formal|HU-035]] (esta HU solo deja la respuesta lista para enviarse).
- Corregir, reemplazar o emitir más de una respuesta (V-13).
- Reutilizar directamente los adjuntos del chat en la respuesta: en esta HU el emisor vuelve a subir los archivos (propuesta; promover un adjunto interno cambiaría su visibilidad y requiere decisión).
- Borradores guardados de la respuesta (no aparecen en el PRD).
- Editor de texto enriquecido: el cuerpo es texto plano (propuesta).

## Requisitos y reglas de negocio

- Solo Elizabeth o un administrador puede enviar la respuesta formal (PRD §5.7, §6.5).
- La respuesta incluye cuerpo de texto y archivos (PRD §6.5).
- Adjuntos: PDF, imágenes, XML y Excel; máximo 10 MB por archivo; la validación del navegador es solo ayuda (PRD §8).
- El binario va a Azure Blob (Azurite en local) y PostgreSQL guarda metadatos y referencia (PRD §8, [[adr-0006-urls-publicas-azure-blob]]).
- Emitir la respuesta lleva el ticket a `SolutionDelivered` (PRD §6.4).
- La respuesta formal es un evento auditable (PRD §12).
- Un administrador no obtiene acceso al contenido por serlo (PRD §5.5). Propuesta: para emitir, el administrador debe ser participante vigente del ticket (asociado vía [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]] o [[hu-018-asignar-y-agregar-participantes|HU-018]]). El PRD no lo exige expresamente; se registra en pendientes.
- Estados de origen permitidos (propuesta, V-01): `InDevelopment`, `PullRequestReview` e `InProduction`. Desde `New`, `SolutionDelivered` o `Closed` → 409.
- Cuerpo: entre 1 y 10 000 caracteres sin contar espacios iniciales y finales (límite superior propuesto).
- Máximo de archivos por solicitud: decisión abierta ([[pendientes]] §4). Se implementa como valor configurable (`Attachments:MaxFilesPerRequest`, nombre propuesto) y se fija en T-01.

## Invariantes en juego

- Invariante 6 (`AGENTS.md` §7): solo Elizabeth o un administrador emite la respuesta formal.
- Invariante 5: el administrador no ve contenido por serlo; debe estar asociado.
- Invariante 7: adjuntos validados en backend (tipo y 10 MB).
- Invariante 8: auditoría append-only con actor, fecha, acción, objeto y valores anterior/nuevo.
- Invariante 2: los adjuntos marcados como visibles al cliente son solo los de la respuesta formal (y los de radicación); nunca los del chat.

## Criterios del PRD cubiertos

- PRD CA-11 (parcial: emisión restringida y adjuntos; portal y correo en HU-034 y HU-035) → [[criterios-de-aceptacion]]
- PRD CA-09 (parcial: adjuntos de la respuesta formal) → [[criterios-de-aceptacion]]
- PRD CA-14 (parcial: cambio de estado a `SolutionDelivered` con actor y fecha/hora) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-010-respuesta-formal-y-cierre]]
- Dependencias: [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]] (roles e `ICurrentUser`), [[hu-011-registrar-eventos-auditables|HU-011]] (`IAuditLog`), [[hu-014-adjuntos-en-radicacion|HU-014]] (validación de adjuntos e `IFileStorage`), [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]] y [[hu-018-asignar-y-agregar-participantes|HU-018]] (asociación del administrador), [[hu-020-detalle-interno-del-ticket|HU-020]] (vista donde vive el formulario), [[hu-021-cambiar-estado-interno|HU-021]] (estados de origen).
- Relacionadas: [[hu-034-portal-respuesta-formal|HU-034]], [[hu-035-correo-de-respuesta-formal|HU-035]], [[hu-036-cerrar-ticket-manualmente|HU-036]], [[hu-029-adjuntos-e-imagenes-en-chat|HU-029]] (mismo validador de adjuntos), [[hu-039-calculo-de-tiempos-habiles|HU-039]] (tiempo hasta respuesta formal).
- Decisiones: [[adr-0006-urls-publicas-azure-blob]]; abiertas V-01, V-06, V-13 y máximo de archivos por solicitud ([[pendientes]]).

## Componentes afectados

- Backend (Domain): `Ticket`, `FormalResponse`, `Attachment` (+ `AttachmentContext` propuesto).
- Backend (Application): caso de uso `DeliverFormalResponse`; puertos `ITicketRepository`, `IFileStorage`, `IAuditLog`, `ICurrentUser`, `IClock`.
- Backend (Infrastructure): configuración EF Core de `FormalResponse` y `Attachment`; migración.
- Backend (Api): endpoint REST multipart.
- Persistencia PostgreSQL y Archivos (Blob/Azurite).
- Frontend (models, controllers, views): módulo interno de tickets.

## Dificultad

**Nivel:** Alto

**Justificación:** combina autorización por rol y participación, transición de estado sujeta a decisión abierta, carga multipart con validación de firma, escritura coordinada en Blob y PostgreSQL con auditoría en la misma transacción, cambio de esquema de `Attachment` y una vista con confirmación. No se divide porque emitir sin adjuntos no entregaría el resultado del PRD §6.5.

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01 y se alinea con el mecanismo de carga de [[hu-014-adjuntos-en-radicacion|HU-014]].

**`POST /api/tickets/{ticketId}/formal-response`** · `multipart/form-data` · requiere sesión y token antiforgery ([[hu-003-iniciar-y-cerrar-sesion|HU-003]]).

| Parte | Tipo | Regla |
|---|---|---|
| `body` | texto | Obligatorio; 1–10 000 caracteres tras recortar espacios (propuesta) |
| `files` | archivo (repetible) | Opcional; PDF, imagen, XML, Excel; ≤ 10 485 760 bytes cada uno; máximo por solicitud configurable |

Respuesta `201 Created` (`Location: /api/tickets/{ticketId}/formal-response`):

```json
{
  "formalResponseId": "6f1c2a9e-3b7d-4c55-9a51-0d2f7e1b8c40",
  "ticketId": "1b0e7c52-8d4a-4f0e-b3a9-5c2d6e7f8a90",
  "body": "Se corrigió el cálculo de retenciones en el módulo de facturación…",
  "deliveredAt": "2026-10-07T20:15:42Z",
  "deliveredBy": { "userId": "2c9d…", "displayName": "Elizabeth" },
  "previousStatus": "InProduction",
  "status": "SolutionDelivered",
  "attachments": [
    {
      "attachmentId": "a4e1…",
      "fileName": "acta-de-solucion.pdf",
      "contentType": "application/pdf",
      "sizeBytes": 248331,
      "context": "FormalResponse",
      "url": "http://localhost:10000/devstoreaccount1/attachments/a4e1…/acta-de-solucion.pdf"
    }
  ],
  "emailDelivery": "Pending"
}
```

`deliveredBy` solo se devuelve en endpoints internos; el portal no lo expone ([[hu-034-portal-respuesta-formal|HU-034]]). `emailDelivery` lo gestiona [[hu-035-correo-de-respuesta-formal|HU-035]].

**`GET /api/tickets/{ticketId}/formal-response`** (interno) → `200` con el mismo cuerpo o `404` si aún no existe.

Errores (ProblemDetails, RFC 9457; `type` propuesto):

| Código | Caso | `type` |
|---|---|---|
| 400 | `body` vacío, solo espacios o > 10 000 caracteres; supera el máximo de archivos | `urn:dataticket:validation` (con `errors`) |
| 401 | Sin sesión | — |
| 403 | Rol distinto de `ProductManager`/`Administrator` (desarrollador, Producción, cliente) o administrador no participante | `urn:dataticket:forbidden` |
| 404 | El ticket no existe | `urn:dataticket:not-found` |
| 409 | Estado de origen no permitido (`New`, `SolutionDelivered`, `Closed`) o ya existe respuesta formal (V-13); conflicto de concurrencia | `urn:dataticket:invalid-ticket-transition` |
| 413 | Algún archivo supera 10 MB | `urn:dataticket:file-too-large` |
| 415 | Tipo no admitido o firma que no coincide con la extensión | `urn:dataticket:unsupported-file-type` |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: ratificar ruta, partes multipart, DTO de respuesta, códigos y `type` de ProblemDetails; fijar el máximo de archivos por solicitud y el límite del cuerpo; acordar con HU-014 el validador común de adjuntos; registrar `AttachmentContext` en [[glosario]]; anotar en [[pendientes]] la regla "administrador debe ser participante".
- [ ] **T-02 — Dominio: emisión de la respuesta** · Capa: Backend (Domain) · Dificultad: Medio  
  Descripción: primero pruebas xUnit (`Deliver_FromInProduction_SetsSolutionDelivered`, `Deliver_FromNew_Throws`, `Deliver_WhenAlreadyDelivered_Throws`, `Deliver_WithBlankBody_Throws`, `Deliver_With10001Chars_Throws`). Luego `Ticket.DeliverFormalResponse(...)` que crea `FormalResponse`, valida estado de origen y devuelve el estado anterior; `Attachment` con `AttachmentContext` (`Submission`, `Chat`, `FormalResponse`).
- [ ] **T-03 — Caso de uso `DeliverFormalResponse`** · Capa: Backend (Application) · Dificultad: Alto  
  Descripción: primero pruebas con dobles de puertos: desarrollador → `Forbidden`; administrador no participante → `Forbidden`; PM no participante → permitido; archivo de 10 485 761 bytes → rechazo; firma PDF falsa → rechazo; si falla la persistencia no se registra auditoría ni queda la respuesta. Implementar: autorizar con `ICurrentUser`, validar adjuntos con el validador común, subir con `IFileStorage`, persistir respuesta + estado + `AuditEntry` (`FormalResponseDelivered`, valores anterior/nuevo) en una sola unidad de trabajo, usando `IClock`.
- [ ] **T-04 — Persistencia y migración** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: configuración EF Core de `FormalResponse` (1 por ticket mientras V-13 siga abierta, índice único en `TicketId`) y columna `context` en `Attachment`; token de concurrencia en `Ticket`; migración `AddFormalResponse`. Prueba de integración con PostgreSQL real.
- [ ] **T-05 — Endpoint** · Capa: Backend (Api) · Dificultad: Medio  
  Descripción: primero pruebas de integración con `WebApplicationFactory<Program>`: 201 (PM), 201 (admin participante), 403 (desarrollador participante), 403 (admin no participante), 409 (`New`), 413, 415, 400. Implementar el endpoint minimal API con límite de cuerpo coherente con nginx (`client_max_body_size 50m`) y mapeo a ProblemDetails.
- [ ] **T-06 — Modelo y gateway** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: primero Vitest: validación local (cuerpo vacío, > 10 000, extensión no permitida, > 10 MB) y normalización del DTO. Luego `formalResponse.ts` y `formalResponseGateway.ts` sobre `core/http`, en el módulo interno de tickets (nombre de módulo a alinear con HU-020).
- [ ] **T-07 — Controlador** · Capa: Frontend (controllers) · Dificultad: Medio  
  Descripción: `useFormalResponseController` con estados `editing/confirming/submitting/delivered/error`, mapeo de 403/409/413/415 a mensajes en español; visible solo para `ProductManager`/`Administrator`. Pruebas de controlador cuando se incorpore Testing Library ([[estrategia-de-pruebas]]).
- [ ] **T-08 — Vista** · Capa: Frontend (views) · Dificultad: Bajo  
  Descripción: `FormalResponseFormView` (texto, selector de archivos con tipos aceptados, lista con tamaños, diálogo de confirmación "Esta respuesta se enviará al cliente y no podrá corregirse") y `FormalResponseSummaryView` para el detalle interno.

## Criterios de aceptación

### CHU-01 — Emisión por la PM

**Dado** un ticket en `InProduction` y Elizabeth autenticada con rol `ProductManager`  
**Cuando** envía `POST /api/tickets/{ticketId}/formal-response` con un cuerpo de 250 caracteres y un PDF válido de 1 MB  
**Entonces** recibe `201`, el ticket queda en `SolutionDelivered`, la respuesta y el adjunto quedan persistidos con `context = FormalResponse` y el binario existe en Azurite.

### CHU-02 — Emisión por un administrador asociado

**Dado** un administrador que tomó el ticket (es participante vigente) en estado `InDevelopment`  
**Cuando** emite la respuesta con un cuerpo válido sin adjuntos  
**Entonces** recibe `201` y el ticket pasa a `SolutionDelivered`; **y dado** un administrador **no** asociado, la misma petición devuelve `403` y el ticket no cambia (propuesta).

### CHU-03 — Rechazo a roles no autorizados

**Dado** un desarrollador que es participante vigente del ticket, Julián (Producción) participante, y un solicitante de la empresa del ticket  
**Cuando** cada uno intenta emitir la respuesta por la API (sin pasar por la UI)  
**Entonces** los tres reciben `403`, no se crea `FormalResponse`, no se sube ningún archivo y el estado no cambia.

### CHU-04 — Validación del cuerpo con valores límite

**Dado** un ticket en `InProduction` y la PM autenticada  
**Cuando** envía un cuerpo vacío, uno de solo espacios, uno de 10 000 caracteres y otro de 10 001  
**Entonces** el vacío, el de espacios y el de 10 001 reciben `400` con el error en `body`, y el de 10 000 recibe `201`.

### CHU-05 — Validación de adjuntos en backend

**Dado** la PM autenticada y un ticket en `InProduction`  
**Cuando** adjunta (a) un XLSX de exactamente 10 485 760 bytes, (b) un PDF de 10 485 761 bytes, (c) un `.exe`, (d) un archivo `.pdf` cuyo contenido es PNG  
**Entonces** (a) se acepta, (b) recibe `413`, (c) y (d) reciben `415`; en los rechazos no se persiste la respuesta ni queda ningún objeto en Blob.

### CHU-06 — Transiciones no permitidas

**Dado** un ticket en `New`, otro en `Closed` y otro con respuesta formal ya emitida (`SolutionDelivered`)  
**Cuando** la PM intenta emitir la respuesta en cada uno  
**Entonces** recibe `409` con `type` `urn:dataticket:invalid-ticket-transition`, y el estado y la respuesta existente no cambian.

### CHU-07 — Auditoría append-only

**Dado** una emisión exitosa  
**Cuando** se consulta la bitácora del ticket ([[hu-012-consultar-bitacora-del-ticket|HU-012]])  
**Entonces** existe una entrada `FormalResponseDelivered` con actor, fecha/hora UTC, objeto (ticket y respuesta), valor anterior `InProduction`, valor nuevo `SolutionDelivered` y número de adjuntos; los intentos rechazados no crean entradas de éxito.

### CHU-08 — Concurrencia

**Dado** la PM y un administrador asociado que envían la respuesta del mismo ticket a la vez  
**Cuando** ambas peticiones se procesan  
**Entonces** exactamente una recibe `201` y la otra `409`; en la base hay una sola `FormalResponse`.

### CHU-09 — Comportamiento de la interfaz

**Dado** la PM en el detalle interno de un ticket  
**Cuando** pulsa "Emitir respuesta formal"  
**Entonces** ve un diálogo de confirmación antes de enviar; si el backend devuelve `413` o `415` la vista muestra el nombre del archivo rechazado y conserva el texto escrito; un desarrollador no ve el botón, y aun así la API lo rechaza (CHU-03).

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-09 validados con evidencia en la matriz.
- [ ] **DoD-02** — Pruebas escritas primero y en verde: dominio (`Deliver_*`), caso de uso (`DeliverFormalResponse_ByDeveloper_ReturnsForbidden`, `DeliverFormalResponse_ByUnassociatedAdmin_ReturnsForbidden`, `DeliverFormalResponse_WhenPersistenceFails_DoesNotAudit`) e integración del endpoint con PostgreSQL real; `cd backend && dotnet test` sin fallos.
- [ ] **DoD-03** — `DataTicket.ArchitectureTests` en verde: `Application` no referencia EF Core, ASP.NET Core ni el SDK de Azure.
- [ ] **DoD-04** — Validación de adjuntos en backend probada para tipo, firma y el límite exacto de 10 485 760 bytes.
- [ ] **DoD-05** — Migración EF Core `AddFormalResponse` creada, aplicada en el entorno local y revisada (índice único por ticket, columna `context` en adjuntos).
- [ ] **DoD-06** — Entrada `FormalResponseDelivered` en la auditoría con valores anterior/nuevo, verificada por prueba.
- [ ] **DoD-07** — Frontend: Vitest del modelo en verde (`npm --prefix frontend test`), `npm --prefix frontend run lint` sin errores (fronteras MVC) y `npm --prefix frontend run build` correcto.
- [ ] **DoD-08** — Contrato ratificado en T-01 coincide con OpenAPI (`/openapi/v1.json`) y con el gateway del frontend.
- [ ] **DoD-09** — `docker compose up --build` levanta el stack y la emisión funciona de punta a punta contra Azurite.
- [ ] **DoD-10** — Wiki actualizada: [[flujo-del-ticket]], [[archivos-adjuntos]] (marca de contexto), [[modelo-de-dominio]], [[glosario]] (`AttachmentContext`), [[persistencia-postgresql]] (migración), [[backend-hexagonal]] si cambia el mapa de puertos; notas para [[pendientes]] (admin participante, V-13).
- [ ] **DoD-11** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos y PR revisado por otra persona del equipo.
- [ ] **DoD-12** — La trazabilidad de la HU y de [[ep-010-respuesta-formal-y-cierre]] está actualizada.

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
| DoD-10 | Pendiente | — | — |
| DoD-11 | Pendiente | — | — |
| DoD-12 | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- **Propuesta:** el administrador debe ser participante vigente para emitir; el PRD solo dice "Elizabeth o un administrador" (PRD §5.7). Confirmar con el dueño del producto.
- **Propuesta (V-01):** estados de origen `InDevelopment`, `PullRequestReview`, `InProduction`.
- **Propuesta (V-13):** una sola respuesta formal por ticket; segunda emisión → 409.
- **Propuesta:** límite de 10 000 caracteres y cuerpo en texto plano.
- **Propuesta:** `AttachmentContext` como marca de contexto; la visibilidad al cliente se deriva del contexto (`Submission` y `FormalResponse` visibles; `Chat` nunca).
- **Riesgo:** si falla la escritura en PostgreSQL después de subir al Blob, quedan objetos huérfanos; se propone borrarlos en compensación y registrar el fallo en logs.

## Relacionado

- [[ep-010-respuesta-formal-y-cierre]] · [[tablero-scrum]] · [[flujo-del-ticket]] · [[estados-del-ticket]] · [[archivos-adjuntos]] · [[auditoria]] · [[roles-y-permisos]] · [[modelo-de-dominio]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[criterios-de-aceptacion]] · [[adr-0006-urls-publicas-azure-blob]]

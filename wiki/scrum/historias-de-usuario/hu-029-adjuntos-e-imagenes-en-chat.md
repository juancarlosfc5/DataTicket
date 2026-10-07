---
title: "HU-029 — Adjuntos e imágenes en el chat"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/chat, producto/archivos, signalr, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §7", "PRD.md §8", "PRD.md §10", "PRD.md §12", "PRD.md §14", "PRD.md §16"]
aliases: ["HU-029", "Adjuntos e imágenes en el chat"]
epica: "[[ep-009-chat-interno-en-tiempo-real]]"
criterios_prd: ["CA-08", "CA-09", "CA-10"]
componentes: ["Backend (Domain, Application, Infrastructure, Api)", "Hub SignalR", "Archivos (Blob)", "Persistencia PostgreSQL", "Frontend (models, controllers, views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 6"
dependencias: ["[[hu-014-adjuntos-en-radicacion]]", "[[hu-026-historial-del-chat-por-cursor]]", "[[hu-028-enviar-y-recibir-mensajes]]"]
relacionadas: ["[[hu-033-emitir-respuesta-formal]]", "[[hu-020-detalle-interno-del-ticket]]", "[[hu-024-portal-solicitante-consulta-tickets]]", "[[hu-025-portal-coordinador-consulta-tickets]]", "[[hu-034-portal-respuesta-formal]]", "[[hu-032-revocar-acceso-al-retirar-participante]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-029 — Adjuntos e imágenes en el chat

Un participante autorizado adjunta a un mensaje del chat PDF, imágenes, XML o Excel de hasta 10 MB, validados en el backend. Las imágenes se previsualizan en la conversación. Los adjuntos del chat quedan marcados como internos y nunca aparecen en el portal del cliente (PRD §7.8, §8, §10).

## Historia de usuario

**COMO** participante interno vigente de un ticket (o Elizabeth, PM)  
**QUIERO** adjuntar capturas, PDF, XML o Excel a mis mensajes del chat y ver las imágenes en la conversación  
**PARA** compartir la evidencia del caso con el equipo y preparar el material de la respuesta formal

## Contexto

El PRD define un chat "de texto e imágenes" (PRD §7, objetivo; §14.9), pero §7.8 y §8 admiten en el chat PDF, imágenes, XML y Excel de hasta 10 MB, con validación en el backend. Esa contradicción está registrada en [[pendientes]] §1 y se asume el conjunto amplio. Quien se incorpora tarde ve los adjuntos anteriores (PRD §7.3). El portal del cliente no expone "adjuntos internos" (PRD §10). El [[modelo-de-dominio]] propone `Attachment` **sin contexto ni marca interno/externo** (hallazgo 4), lo que impediría al portal distinguir los adjuntos del chat. [[hu-014-adjuntos-en-radicacion|HU-014]] y [[hu-033-emitir-respuesta-formal|HU-033]] proponen `AttachmentContext` (`Submission`, `Chat`, `FormalResponse`), y esta HU usa el valor `Chat`. Los binarios van a Azure Blob con URL pública permanente ([[adr-0006-urls-publicas-azure-blob]], riesgo aceptado y pendiente de aprobación por la dirección, PRD §16.6).

## Alcance

- Endpoint de subida de un adjunto del chat (un archivo por petición, propuesta), con validación de extensión, tipo MIME, firma del contenido y tamaño (validador común con HU-014 y HU-033).
- Vinculación de los adjuntos subidos a un mensaje mediante `attachmentIds` en `SendMessage` (extiende [[hu-028-enviar-y-recibir-mensajes|HU-028]]), en la misma transacción que el mensaje.
- `Attachment` con `Context = Chat` y referencia al `ChatMessage`; migración EF Core si cambia el esquema.
- `attachments` poblado en el `ChatMessageDto` del historial ([[hu-026-historial-del-chat-por-cursor|HU-026]]) y en `MessageCreated`.
- Frontend: selector de archivos, validación de ayuda en el navegador, progreso de subida, miniaturas de imágenes y enlaces para los demás tipos.
- Prueba de que el portal del cliente nunca devuelve adjuntos del chat.

## Fuera de alcance

- Cambiar la política de URL pública a SAS o Entra ID: requiere un ADR nuevo que reemplace a [[adr-0006-urls-publicas-azure-blob]].
- Reutilizar un adjunto del chat como adjunto de la respuesta formal: [[hu-033-emitir-respuesta-formal|HU-033]] propone volver a subirlo.
- Limpieza de adjuntos subidos y nunca enviados, y retención de archivos: depende de PRD §16.3 ([[pendientes]] §2).
- Antivirus o análisis de contenido.
- Edición de imágenes, galerías o visor de PDF integrado.

## Requisitos y reglas de negocio

- El chat acepta texto y adjuntos de los tipos y tamaños de §8 (PRD §7.8, §14.9).
- Tipos: PDF, imágenes, XML y Excel; 10 MB por archivo; varios archivos donde la interfaz lo indique (PRD §8).
- El backend aplica las restricciones; la validación del navegador es solo ayuda de interfaz (PRD §8).
- PostgreSQL guarda metadatos y referencias, no el binario; almacenamiento en Azure Blob (PRD §8).
- Al enviar, el servidor revalida la membresía, persiste mensaje **y adjuntos** y luego publica (PRD §7, flujo 4).
- Quien se agrega después ve el historial completo, adjuntos incluidos (PRD §7.3, §14.8).
- El portal no expone adjuntos internos (PRD §10) ni conversaciones internas (PRD §5.6, §14.10).
- La URL pública no hereda la autorización del portal y es un riesgo aceptado (PRD §8, §12).

## Invariantes en juego

- AGENTS §7.2: el cliente nunca ve el chat interno, tampoco sus adjuntos.
- AGENTS §7.3: solo participantes vigentes suben y envían adjuntos; Elizabeth siempre puede.
- AGENTS §7.4: el mensaje y sus adjuntos se persisten antes de publicarse.
- AGENTS §7.7: PDF, imágenes, XML y Excel de hasta 10 MB, validados en el backend.

## Criterios del PRD cubiertos

- PRD CA-09 (parcial: la parte del chat; la radicación está en HU-014 y la respuesta formal en HU-033) → [[criterios-de-aceptacion]]
- PRD CA-08 (parcial: el incorporado tarde ve los adjuntos anteriores) → [[criterios-de-aceptacion]]
- PRD CA-10 (parcial: los adjuntos del chat no llegan al portal) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-009-chat-interno-en-tiempo-real]]
- Dependencias: [[hu-014-adjuntos-en-radicacion|HU-014 — Adjuntos en la radicación]] (`Attachment`, `IFileStorage`, validador común, Azurite), [[hu-026-historial-del-chat-por-cursor|HU-026 — Historial del chat paginado por cursor]] (DTO con `attachments`), [[hu-028-enviar-y-recibir-mensajes|HU-028 — Enviar y recibir mensajes en tiempo real]] (`SendMessage`).
- Relacionadas: [[hu-033-emitir-respuesta-formal|HU-033]] (mismo validador y `AttachmentContext`), [[hu-020-detalle-interno-del-ticket|HU-020]] (el detalle no mezcla adjuntos del chat), [[hu-024-portal-solicitante-consulta-tickets|HU-024]], [[hu-025-portal-coordinador-consulta-tickets|HU-025]] y [[hu-034-portal-respuesta-formal|HU-034]] (el portal filtra por contexto), [[hu-032-revocar-acceso-al-retirar-participante|HU-032]].
- Decisiones: [[adr-0006-urls-publicas-azure-blob]] (aceptada, con riesgo pendiente de aprobación). Pendientes: contradicción de tipos en el chat ([[pendientes]] §1), V-06 (MIME y firma), máximo de archivos por solicitud ([[pendientes]] §4), retención (§16.3), CSP `img-src` con el dominio del Blob ([[pendientes]] §4).

## Componentes afectados

- Backend Domain: `Attachment` con `AttachmentContext.Chat` (nombre propuesto en HU-014/HU-033) y vínculo opcional a `ChatMessage`; regla "un adjunto se vincula una sola vez, al mensaje de su autor y en su ticket".
- Backend Application: `UploadChatAttachment` (nombre propuesto: ratificar en T-01 y registrar en el glosario); extensión de `SendChatMessage`; puertos `IFileStorage` y repositorio de adjuntos (el que defina HU-014), `IChatRepository`.
- Backend Infrastructure: `Storage/` (Blob en Azurite), configuración EF Core de la relación mensaje–adjunto, migración `AddChatAttachments` (nombre propuesto).
- Backend Api: endpoint multipart y extensión de `TicketsHub.SendMessage`.
- Archivos (Blob): contenedor de adjuntos, nombre de blob aleatorio, `Content-Type` del tipo validado.
- Frontend: módulo `chat` (modelos de adjunto, gateway de subida, controlador y vistas de compositor y miniaturas).

## Dificultad

**Nivel:** Alto

**Justificación:** es transversal (REST multipart, Blob, PostgreSQL, hub y frontend). La validación de seguridad de archivos por firma y tamaño es de detalle. La vinculación debe ser atómica con el mensaje, el cambio de esquema es compartido con otras HU y además hay que demostrar el aislamiento frente al portal. No se divide: un adjunto que no puede enviarse no aporta valor.

## Contrato backend ↔ frontend

> [!info] Propuesta
> Se ratifica en T-01, alineado con [[hu-014-adjuntos-en-radicacion|HU-014]] y [[hu-033-emitir-respuesta-formal|HU-033]], y se documenta en [[tiempo-real-signalr]] y [[archivos-adjuntos]].

**1. Subida:** `POST /api/tickets/{ticketId}/chat/attachments`, `multipart/form-data`, una parte `file` (un archivo por petición; el cliente sube varios en paralelo).

Respuesta **201** (sin URL: la URL solo se entrega cuando el adjunto queda vinculado a un mensaje):

```json
{
  "attachmentId": "0192a3f0-5b6c-7d8e-9f01-23456789abcd",
  "fileName": "captura-error.png",
  "contentType": "image/png",
  "sizeBytes": 248311
}
```

| Código | Cuándo |
|---|---|
| 400 | Falta la parte `file`, hay más de un archivo o el nombre es inválido |
| 401 | Sin sesión |
| 404 | Ticket inexistente o sin acceso al chat: no participante, retirado, cliente, administrador no asociado |
| 413 | El archivo supera 10 MB (10 485 760 bytes) |
| 415 | Extensión, MIME o firma no permitidos, o la firma no coincide con la extensión |

Tipos admitidos (propuesta, pendiente de V-06): `.pdf`; `.png`, `.jpg`/`.jpeg`, `.gif`, `.webp`; `.xml`; `.xls`, `.xlsx`. **SVG excluido** (puede contener scripts).

**2. Envío:** `SendMessage(ticketId, { "body": "Adjunto la captura del error", "attachmentIds": ["0192a3f0-…"] })` (extiende HU-028).

- `body` puede ir vacío si hay al menos un adjunto.
- Cada id debe existir, pertenecer al mismo ticket, tener `Context = Chat`, haberlo subido el mismo usuario y no estar vinculado. En caso contrario: `HubException` `validation_failed` con detalle `attachment_not_available` (propuesta) y no se persiste nada.
- Máximo de adjuntos por mensaje: valor configurable compartido con HU-033 (`Attachments:MaxFilesPerRequest`, nombre propuesto), pendiente de decisión.

**3. DTO en `MessageCreated` y en el historial** (`ChatMessageDto.attachments`):

```json
"attachments": [
  {
    "attachmentId": "0192a3f0-5b6c-7d8e-9f01-23456789abcd",
    "fileName": "captura-error.png",
    "contentType": "image/png",
    "sizeBytes": 248311,
    "url": "http://localhost:10000/devstoreaccount1/attachments/0192a3f0-5b6c-7d8e-9f01-23456789abcd"
  }
]
```

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: ratificar con HU-014 y HU-033 la lista de extensiones y MIME (V-06), la exclusión de SVG, la ruta de subida (un archivo por petición), la respuesta sin URL, el máximo de adjuntos por mensaje, el error `attachment_not_available` y la clave `attachmentId` del DTO, que debe coincidir en chat y respuesta formal. Registrar `AttachmentContext` y `UploadChatAttachment` en el [[glosario]].
- [ ] **T-02 — Dominio (prueba primero)** · Capa: Backend Domain · Dificultad: Medio  
  Descripción: pruebas de `Attachment` con `Context = Chat`: se vincula una sola vez; no se vincula a un mensaje de otro ticket ni de otro autor; un mensaje sin texto exige al menos un adjunto. Implementar las reglas.
- [ ] **T-03 — Caso de uso `UploadChatAttachment` (prueba primero)** · Capa: Backend Application · Dificultad: Medio  
  Descripción: pruebas con dobles de `IFileStorage`, del repositorio de adjuntos, de `ICurrentUser` y de la regla de acceso al chat: no autorizado → no sube nada; tipo o firma inválidos → no llama a `IFileStorage`; 10 485 761 bytes → rechazo; fallo al guardar metadatos → borra el blob (mejor esfuerzo); éxito → `Context = Chat`, `UploadedBy` y sin mensaje. Reutilizar el validador común de HU-014.
- [ ] **T-04 — Extender `SendChatMessage` (prueba primero)** · Capa: Backend Application · Dificultad: Medio  
  Descripción: pruebas: adjuntos válidos → mensaje y vínculos en una unidad de trabajo y luego notificación con `attachments` poblado; adjunto ajeno, ya vinculado, de otro ticket o inexistente → no persiste ni notifica; **fallo al persistir → `ITicketChatNotifier` no se invoca y los adjuntos siguen sin vincular**.
- [ ] **T-05 — Persistencia, Blob y migración** · Capa: Backend Infrastructure · Dificultad: Medio  
  Descripción: relación `Attachment` → `ChatMessage` (FK opcional, índice) y columna de contexto si HU-014 no la creó; migración `AddChatAttachments` (nombre propuesto). Subida a Blob con nombre aleatorio no enumerable y `Content-Type` del tipo validado. Lectura de adjuntos en la consulta de historial de HU-026, sin consultas N+1.
- [ ] **T-06 — Endpoint, hub e integración** · Capa: Backend Api · Dificultad: Alto  
  Descripción: pruebas de integración primero (`WebApplicationFactory<Program>`, PostgreSQL real, Azurite del Compose): subida válida, 413, 415 por firma falsa, 404 para no participante y cliente, envío con adjunto recibido por un segundo cliente SignalR, historial del incorporado tarde con adjuntos y **endpoint del portal ([[hu-024-portal-solicitante-consulta-tickets|HU-024]]) sin adjuntos del chat**. Después, el endpoint multipart con límite de cuerpo de 10 MB más margen y la extensión de `SendMessage`.
- [ ] **T-07 — Modelos y gateway** · Capa: Frontend models · Dificultad: Medio  
  Descripción: tipos `ChatAttachment` y `PendingUpload`; validación de ayuda (extensión y tamaño) e `isPreviewableImage(contentType)` probadas con Vitest; `chatGateway.uploadAttachment(ticketId, file, onProgress)` sobre `core/http`; `sendMessage` con `attachmentIds`.
- [ ] **T-08 — Controlador** · Capa: Frontend controllers · Dificultad: Medio  
  Descripción: `useTicketChatController` gestiona adjuntos pendientes (subiendo, listo o con error), quitar antes de enviar y bloquear "Enviar" mientras haya subidas en curso. Vista previa local con `URL.createObjectURL` y liberación con `revokeObjectURL`.
- [ ] **T-09 — Vistas** · Capa: Frontend views · Dificultad: Bajo  
  Descripción: `ChatComposer` con botón de adjuntar (`accept` como ayuda), lista de pendientes con progreso y error por archivo; `ChatAttachmentList` (nombre propuesto) con miniatura `<img>` para imágenes (texto `alt` con el nombre) y enlace con nombre y tamaño para PDF, XML y Excel (`rel="noopener noreferrer"`).

## Criterios de aceptación

### CHU-01 — Flujo feliz con imagen entre dos usuarios

**Dado** Laura y Brayan, participantes vigentes unidos al chat del ticket T  
**Cuando** Laura sube `captura-error.png` (2 MB) y envía un mensaje con ese `attachmentId`  
**Entonces** Laura recibe 201 en la subida y el `ChatMessageDto` al enviar; Brayan recibe `MessageCreated` con `attachments[0]` (nombre, tipo, tamaño y `url`); y ambos ven la miniatura de la imagen.

### CHU-02 — Tipos y tamaño permitidos

**Dado** un participante vigente  
**Cuando** sube un PDF, un XML, un `.xlsx` y una imagen JPEG de exactamente 10 485 760 bytes  
**Entonces** todos se aceptan con 201, el binario existe en Blob (Azurite) y PostgreSQL guarda solo los metadatos con `Context = Chat`.

### CHU-03 — Rechazo en el backend por tamaño, tipo o firma

**Dado** un participante vigente que salta la validación del navegador (por ejemplo, con `curl`)  
**Cuando** sube un archivo de 10 485 761 bytes, un `.exe`, un `.svg` o un `.exe` renombrado a `.pdf`  
**Entonces** recibe 413 en el primer caso y 415 en los demás, con ProblemDetails, y no queda ni blob ni metadatos.

### CHU-04 — Subida sin acceso rechazada

**Dado** el ticket T  
**Cuando** intentan subir un adjunto del chat de T un integrante de Desarrollo no asociado, un participante retirado, un `Administrator` no asociado o un usuario cliente de la empresa dueña de T  
**Entonces** cada uno recibe 404 (o 401 sin sesión) y no se guarda nada en Blob ni en PostgreSQL.

### CHU-05 — Solo se vinculan adjuntos propios y libres

**Dado** un adjunto subido por Brayan en T, otro subido por Laura en el ticket U y otro ya vinculado a un mensaje  
**Cuando** Laura envía en T un mensaje que referencia cualquiera de ellos o un id inexistente  
**Entonces** recibe `HubException` `validation_failed` (`attachment_not_available`), no se persiste el mensaje, ningún adjunto cambia de estado y nadie recibe `MessageCreated`.

### CHU-06 — Persistir antes de publicar, con adjuntos

**Dado** `SendChatMessage` con un repositorio que falla al guardar el mensaje con sus vínculos  
**Cuando** un participante envía un mensaje con dos adjuntos  
**Entonces** `ITicketChatNotifier` no se invoca, los adjuntos siguen sin vincular y el emisor recibe `internal_error`.

### CHU-07 — El incorporado tarde ve los adjuntos anteriores

**Dado** un ticket con mensajes con adjuntos enviados antes de que Cristian fuera agregado  
**Cuando** Cristian, ya participante vigente, carga el historial  
**Entonces** cada mensaje trae sus `attachments` con `url`, y las imágenes se previsualizan.

### CHU-08 — Los adjuntos del chat nunca llegan al portal

**Dado** el ticket T de la empresa A con un adjunto de radicación y un adjunto enviado en el chat  
**Cuando** el `Requester` que lo radicó y el `CompanyCoordinator` de A consultan T en el portal  
**Entonces** la respuesta del portal incluye solo el adjunto de radicación (y los de la respuesta formal cuando exista). Ni el `attachmentId` ni la `url` del adjunto del chat aparecen en el cuerpo JSON.

### CHU-09 — UI ante error y durante la subida

**Dado** el compositor con tres archivos seleccionados, uno de ellos de 12 MB  
**Cuando** se suben  
**Entonces** el de 12 MB se marca con error antes de enviarse (ayuda de interfaz) o al recibir 413; los otros muestran progreso; "Enviar" queda deshabilitado hasta que terminan; si se pierde la conexión al hub, los adjuntos subidos se conservan como pendientes y el texto no se pierde.

## Definition of Done

- [ ] DoD-01 — CHU-01 a CHU-09 validados con evidencia registrada en la matriz.
- [ ] DoD-02 — Pruebas unitarias escritas primero y en verde con `cd backend && dotnet test`: `UploadChatAttachment_NoAutorizado_NoSubeNada`, `UploadChatAttachment_FirmaFalsa_Rechaza`, `UploadChatAttachment_Supera10MB_Rechaza`, `UploadChatAttachment_FallaMetadatos_BorraBlob`, `SendChatMessage_AdjuntoAjeno_Rechaza`, `SendChatMessage_ConAdjuntos_FallaPersistencia_NoNotifica`, y pruebas de dominio del vínculo.
- [ ] DoD-03 — Pruebas de integración en verde con `WebApplicationFactory<Program>`, cliente SignalR .NET, PostgreSQL real y Azurite: `ChatAttachment_DosParticipantes_RecibenAdjunto`, `ChatAttachment_413`, `ChatAttachment_415_FirmaFalsa`, `ChatAttachment_NoParticipante_404`, `Historial_AgregadoTarde_IncluyeAdjuntos`, `Portal_NoExponeAdjuntosDelChat`.
- [ ] DoD-04 — Validación de adjuntos en backend (extensión, MIME, firma y 10 MB) con el validador común de HU-014 y HU-033, sin duplicarlo.
- [ ] DoD-05 — `DataTicket.ArchitectureTests` en verde: el SDK de Blob solo en `Infrastructure`.
- [ ] DoD-06 — Migración `AddChatAttachments` (nombre propuesto) creada, aplicada en una base limpia del Compose y revisada (FK opcional e índice).
- [ ] DoD-07 — Frontend: `npm --prefix frontend run lint` sin errores, `npm --prefix frontend test` en verde (validación de ayuda, `isPreviewableImage`, estado de pendientes) y `npm --prefix frontend run build` correcto.
- [ ] DoD-08 — `docker compose up --build` levanta el stack (incluido `azurite`). Verificación manual con dos navegadores en `http://localhost:5173`: imagen previsualizada en ambos, PDF descargable, archivo de 11 MB rechazado y el portal de un cliente sin el adjunto.
- [ ] DoD-09 — Wiki actualizada mediante Notas para la wiki: [[archivos-adjuntos]] (contexto y tipos exactos), [[modelo-de-dominio]] y [[glosario]] (`AttachmentContext`, `UploadChatAttachment`), [[chat-interno]], [[tiempo-real-signalr]], [[persistencia-postgresql]].
- [ ] DoD-10 — Revisión de `quality-reviewer` sin hallazgos CRÍTICO o ALTO abiertos (validación de archivos, exposición al portal, URL pública).
- [ ] DoD-11 — PR revisado y aprobado por otra persona del equipo.
- [ ] DoD-12 — Trazabilidad de la HU y de [[ep-009-chat-interno-en-tiempo-real]] actualizada.

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

> [!warning] Contradicción
> PRD §7 (objetivo) y §14.9 hablan de chat "de texto e imágenes", mientras PRD §7.8 y §8 admiten PDF, XML y Excel en el chat. Se asume el conjunto amplio ([[pendientes]] §1) hasta que se confirme.

> [!danger] Riesgo aceptado
> URL pública permanente ([[adr-0006-urls-publicas-azure-blob]]): un adjunto del chat es accesible para cualquiera que tenga el enlace, incluido un participante retirado que lo guardó antes del retiro. La revocación de [[hu-032-revocar-acceso-al-retirar-participante|HU-032]] no puede invalidarlo. Además, los adjuntos subidos y nunca enviados quedan en Blob con URL pública mientras no exista una política de retención (PRD §16.3). Mitigaciones propuestas: no devolver la URL hasta vincular el adjunto y usar nombres de blob aleatorios.

- **Hallazgo 4:** `Attachment` no tiene contexto ni marca interno/externo en el [[modelo-de-dominio]]. Esta HU usa `AttachmentContext.Chat` (nombre propuesto en HU-014 y HU-033). El portal debe filtrar por contexto: lista blanca `Submission` y `FormalResponse`.
- **CSP:** las miniaturas cargan imágenes desde el dominio del Blob. La CSP del SPA (`img-src`) sigue abierta ([[pendientes]] §4).
- **Clave del DTO de adjunto:** se usa `attachmentId`, como en HU-033. [[hu-026-historial-del-chat-por-cursor|HU-026]] reserva el campo con esa misma forma.

## Relacionado

- [[ep-009-chat-interno-en-tiempo-real]] · [[tablero-scrum]] · [[criterios-de-aceptacion]]
- [[archivos-adjuntos]] · [[adr-0006-urls-publicas-azure-blob]] · [[chat-interno]] · [[tiempo-real-signalr]] · [[modelo-de-dominio]] · [[persistencia-postgresql]] · [[entorno-docker]] · [[roles-y-permisos]] · [[pendientes]]

---
title: "HU-014 — Adjuntos en la radicación"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/archivos, producto/ticket, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §8", "PRD.md §6.1", "PRD.md §10", "PRD.md §12", "PRD.md §14", "PRD.md §16"]
aliases: ["HU-014", "Adjuntos en la radicación", "Adjuntar archivos al radicar"]
epica: "[[ep-005-radicacion-de-tickets]]"
criterios_prd: ["PRD CA-09", "PRD CA-01"]
componentes: ["Backend (Domain)", "Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Persistencia PostgreSQL", "Archivos (Blob)", "Frontend (models)", "Frontend (controllers)", "Frontend (views)", "Docker/Compose"]
dificultad: "Alto"
sprint_sugerido: "Sprint 2"
dependencias: ["[[hu-013-radicar-ticket]]", "[[hu-011-registrar-eventos-auditables]]", "[[hu-004-contexto-de-usuario-y-autorizacion]]"]
relacionadas: ["[[hu-029-adjuntos-e-imagenes-en-chat]]", "[[hu-033-emitir-respuesta-formal]]", "[[hu-024-portal-solicitante-consulta-tickets]]", "[[hu-017-cola-de-cobertura-y-tomar-ticket]]", "[[hu-020-detalle-interno-del-ticket]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-014 — Adjuntos en la radicación

Al radicar, el cliente puede adjuntar varios archivos PDF, imagen, XML o Excel de hasta 10 MB cada uno; el backend valida extensión, MIME y firma, guarda el binario en Azure Blob (Azurite en local) con URL pública permanente y los metadatos en PostgreSQL, todo de forma atómica con el ticket (PRD §8).

## Historia de usuario

**COMO** solicitante (o coordinador) de una empresa cliente  
**QUIERO** adjuntar capturas, PDF, XML o Excel al radicar mi ticket  
**PARA** dar a Data Global la evidencia del problema desde el primer momento y evitar idas y vueltas

## Contexto

Los archivos son opcionales en el formulario común (PRD §6.1). Se admiten PDF, imágenes, XML y Excel, 10 MB por archivo, varios archivos donde la interfaz lo indique; el backend aplica las restricciones; PostgreSQL guarda metadatos y referencias, nunca el binario; el almacenamiento es Azure Blob (PRD §8). Se acordó una URL pública permanente con riesgo aceptado ([[adr-0006-urls-publicas-azure-blob]]). En local, Compose ya provee Azurite y las claves `Storage__Blob__ConnectionString` y `Storage__Blob__PublicBaseUrl` ([[entorno-docker]]), pero no existe aún el adaptador `IFileStorage`. El modelo actual de `Attachment` no distingue el contexto (radicación, chat, respuesta formal) ni si es visible para el cliente (hallazgo 4).

## Alcance

- Value object/entidad `Attachment` con contexto (`AttachmentContext.Submission`, nombre propuesto; ratificar en T-01 y registrar en el glosario).
- Política de validación en dominio: lista blanca de extensiones, MIME coherente y firma de contenido (magic bytes), tamaño 1–10 485 760 bytes.
- Puerto `IFileStorage` y adaptador Azure Blob (Azurite en local) con contenedor de acceso público de lectura por blob.
- Extensión de `POST /api/tickets` a `multipart/form-data` con 0..N archivos, atómica con el ticket.
- Metadatos en PostgreSQL (tabla de adjuntos) y migración EF Core.
- Selector de archivos en el formulario de radicación con validación de ayuda y errores por archivo.

## Fuera de alcance

- Adjuntos del chat ([[hu-029-adjuntos-e-imagenes-en-chat|HU-029]]) y de la respuesta formal ([[hu-033-emitir-respuesta-formal|HU-033]]): reutilizan la política y el puerto.
- Adjuntar archivos después de radicar (el PRD solo los menciona en la radicación, el chat y la respuesta formal).
- URL SAS, Entra ID o descarga autenticada (requiere un ADR que reemplace ADR-0006).
- Antivirus o inspección profunda de contenido (no lo pide el PRD; propuesta para producción).
- Eliminación de objetos y retención (PRD §8, §16.3).
- Visualización de los adjuntos en el portal ([[hu-024-portal-solicitante-consulta-tickets|HU-024]]) y en el detalle interno ([[hu-020-detalle-interno-del-ticket|HU-020]]).

## Requisitos y reglas de negocio

- Tipos admitidos: PDF, imágenes, XML y Excel (PRD §8).
- 10 MB por archivo; se rechazan en backend formatos y tamaños no permitidos (PRD §8, §14.9).
- La validación del navegador es solo ayuda de interfaz (PRD §8).
- Metadatos y referencias en PostgreSQL; binario en Azure Blob (PRD §8).
- URL pública permanente; el acceso al archivo no hereda la autorización del portal (PRD §8; ADR-0006).
- Los adjuntos de radicación forman parte de los datos de radicación que el cliente puede ver (PRD §10; [[archivos-adjuntos]]).
- (propuesta) 10 MB = 10 485 760 bytes; archivos de 0 bytes se rechazan.
- (propuesta, V-06) Lista blanca:

  | Tipo | Extensiones | MIME canónico | Firma esperada |
  |---|---|---|---|
  | PDF | `.pdf` | `application/pdf` | `25 50 44 46 2D` (`%PDF-`) |
  | Imagen PNG | `.png` | `image/png` | `89 50 4E 47 0D 0A 1A 0A` |
  | Imagen JPEG | `.jpg`, `.jpeg` | `image/jpeg` | `FF D8 FF` |
  | Imagen GIF | `.gif` | `image/gif` | `GIF87a` / `GIF89a` |
  | Imagen WebP | `.webp` | `image/webp` | `RIFF` … `WEBP` |
  | XML | `.xml` | `application/xml` | texto UTF-8 (BOM opcional) cuyo primer carácter no blanco es `<` |
  | Excel 2007+ | `.xlsx` | `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` | ZIP `50 4B 03 04` con entrada `xl/workbook.xml` |
  | Excel 97-2003 | `.xls` | `application/vnd.ms-excel` | OLE `D0 CF 11 E0 A1 B1 1A E1` |

  Excluidos (propuesta): `.svg` (puede contener script), `.xlsm` (macros), `.heic` y cualquier otro.
- (propuesta) El MIME declarado por el navegador debe ser coherente con la extensión; el que se guarda es el canónico de la tabla.
- Máximo de archivos por radicación: **decisión abierta** ([[pendientes]] §4); condiciona el límite de cuerpo del backend (Kestrel limita por defecto a ~28,6 MiB; nginx admite 50 MB en `/api/`).

## Invariantes en juego

- Invariante 7: adjuntos PDF, imágenes, XML y Excel, máx. 10 MB, validados en backend.
- Invariante 1: los adjuntos quedan ligados al ticket de la empresa de la cuenta.
- Invariante 8: la radicación auditada incluye sus adjuntos.

## Criterios del PRD cubiertos

- PRD CA-09 (parcial: radicación; chat en HU-029 y respuesta formal en HU-033) → [[criterios-de-aceptacion]]
- PRD CA-01 (parcial: los adjuntos se ligan a un ticket de la empresa de la cuenta; la descarga desde el portal la cubre HU-024; la URL pública es la excepción aceptada, V-17) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-005-radicacion-de-tickets|EP-005 — Radicación de tickets]]
- Dependencias: [[hu-013-radicar-ticket|HU-013 — Radicar ticket]] · [[hu-011-registrar-eventos-auditables|HU-011 — Registrar eventos auditables]] · [[hu-004-contexto-de-usuario-y-autorizacion|HU-004 — Contexto de usuario y autorización]] · decisión abierta del máximo de archivos por solicitud
- Relacionadas: [[hu-029-adjuntos-e-imagenes-en-chat|HU-029]] · [[hu-033-emitir-respuesta-formal|HU-033]] · [[hu-024-portal-solicitante-consulta-tickets|HU-024]] · [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]] (los adjuntos no se muestran antes de tomar) · [[hu-020-detalle-interno-del-ticket|HU-020]]
- Decisiones: [[adr-0006-urls-publicas-azure-blob]] (riesgo aceptado, aprobación de la dirección pendiente, PRD §16.6) · V-06 · V-17

## Componentes afectados

- Backend (Domain): `Attachment`, `AttachmentContext`, política de archivos.
- Backend (Application): `SubmitTicket` ampliado, puerto `IFileStorage`.
- Backend (Infrastructure): adaptador Blob (`Azure.Storage.Blobs`, versión central en `Directory.Packages.props`), opciones `Storage:Blob:*`, configuración EF Core de adjuntos.
- Backend (Api): binding multipart, límites de cuerpo (`FormOptions`, Kestrel).
- Persistencia PostgreSQL: tabla de adjuntos.
- Archivos (Blob): contenedor de adjuntos con lectura pública por blob.
- Frontend: selector de archivos y errores por archivo en el módulo `tickets`.
- Docker/Compose: sin cambios previstos (Azurite y variables ya existen); verificar.

## Dificultad

**Nivel:** Alto

**Justificación:** integra un almacenamiento externo con atomicidad compensada, validación de contenido binario, límites de cuerpo en varias capas (Kestrel, formularios, nginx) y un riesgo de seguridad aceptado que debe quedar explícito; cruza backend, frontend y entorno.

## Contrato backend ↔ frontend

Propuesta, ratificar en T-01.

**`POST /api/tickets`** — `Content-Type: multipart/form-data` (sigue admitiendo `application/json` sin archivos, HU-013). Roles: `Requester`, `CompanyCoordinator`.

| Parte | Tipo | Regla |
|---|---|---|
| `categoryId`, `title`, `description`, `urgency`, `impact` | texto | Mismas reglas de HU-013 |
| `files` | archivo, 0..N | Lista blanca, firma coherente, 1–10 485 760 bytes; N ≤ máximo decidido |

Respuesta `201 Created` (forma de HU-013 con adjuntos):

```json
{
  "id": "3f4e…",
  "number": "DT-000123",
  "clientStatus": "Received",
  "calculatedPriority": "High",
  "createdAt": "2026-10-07T15:00:00Z",
  "attachments": [
    {
      "id": "a1b2…",
      "fileName": "factura-error.pdf",
      "contentType": "application/pdf",
      "sizeBytes": 204800,
      "url": "http://localhost:10000/devstoreaccount1/attachments/3f4e…/a1b2….pdf"
    }
  ]
}
```

Error de archivo (ProblemDetails):

```json
{
  "type": "https://dataticket.local/problems/attachment-too-large",
  "title": "Uno o más archivos superan 10 MB.",
  "status": 413,
  "errors": { "files[1]": ["captura.png: 10485761 bytes; máximo 10485760."] }
}
```

| Código | Cuándo |
|---|---|
| 400 | Archivo de 0 bytes, nombre ausente, más archivos que el máximo, campos de HU-013 inválidos, partes no admitidas (`companyId`, `ticketId`…) |
| 401 / 403 | Sin sesión / rol interno |
| 413 | Algún archivo > 10 485 760 bytes o cuerpo total por encima del límite configurado |
| 415 | Extensión no admitida, MIME incoherente o firma que no corresponde a la extensión |

Ante cualquier error no se crea el ticket, no quedan blobs ni entradas de auditoría.

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: fijar partes multipart, lista blanca (V-06), forma de errores por archivo, nombre del contenedor, esquema de nombres de blob y `AttachmentContext`; **obtener la decisión del máximo de archivos por radicación** y derivar de ella los límites de Kestrel, `FormOptions` y nginx. Registrar los nombres nuevos en [[glosario]].
- [ ] **T-02 — Domain: política de archivos y `Attachment`** · Capa: Backend (Domain) · Dificultad: Medio  
  Descripción: pruebas primero: `Validate_PdfWithPdfSignature_IsAccepted`, `Validate_ExeRenamedToPdf_IsRejected`, `Validate_PngRenamedToPdf_IsRejected`, `Validate_SizeOf10485760_IsAccepted`, `Validate_SizeOf10485761_IsRejected`, `Validate_EmptyFile_IsRejected`, `Validate_Svg_IsRejected`, `Validate_XlsxWithoutWorkbookEntry_IsRejected`, `SanitizeFileName_RemovesPathSegments`. Implementar la política pura (recibe extensión, MIME declarado, primeros bytes y tamaño) y `Attachment` con contexto `Submission`.
- [ ] **T-03 — Application: `SubmitTicket` con adjuntos e `IFileStorage`** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: pruebas unitarias primero con dobles: `SubmitTicket_WithOneInvalidFile_CreatesNothing`, `SubmitTicket_UploadsBeforeCommit_AndDeletesBlobsWhenCommitFails`, `SubmitTicket_AuditEntryIncludesAttachments`. Definir `IFileStorage` (subir con tipo de contenido, borrar, construir URL pública) y ampliar el caso de uso: validar todo → subir → confirmar transacción (ticket + adjuntos + auditoría) → compensar borrando blobs si falla.
- [ ] **T-04 — Infrastructure: adaptador Blob** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: prueba de integración primero contra Azurite: `BlobStorage_UploadsWithCanonicalContentType_AndPublicUrlIsReadableAnonymously`. Implementar con opciones tipadas `Storage:Blob:ConnectionString`/`PublicBaseUrl`, creación idempotente del contenedor con lectura pública por blob, nombre `{ticketId}/{attachmentId}{ext}` y `Content-Disposition: attachment` para tipos no imagen (propuesta). Añadir el paquete a `Directory.Packages.props`.
- [ ] **T-05 — Infrastructure: metadatos y migración** · Capa: Backend (Infrastructure) · Dificultad: Bajo  
  Descripción: prueba de integración `Attachments_PersistMetadataOnly_NoBinaryColumn`. Configuración EF Core (`ticket_id`, `context`, `file_name`, `content_type`, `size_bytes`, `blob_name`, `url`, `uploaded_by`, `uploaded_at`) y migración `AddAttachments`.
- [ ] **T-06 — Api: multipart y límites** · Capa: Backend (Api) · Dificultad: Medio  
  Descripción: pruebas de integración primero: `PostTicketMultipart_WithPdfAndPng_Returns201WithUrls`, `PostTicketMultipart_With10485761Bytes_Returns413AndCreatesNothing`, `PostTicketMultipart_WithFakePdf_Returns415`, `PostTicketMultipart_AsAdministrator_Returns403`. Binding multipart con lectura en streaming de los primeros bytes, límites alineados con la decisión del máximo y mapeo a ProblemDetails 413/415.
- [ ] **T-07 — Frontend models** · Capa: Frontend (models) · Dificultad: Medio  
  Descripción: pruebas Vitest primero: `prevalidateFile` (extensión, 10 485 760 / 10 485 761 bytes, 0 bytes), `buildSubmissionFormData`, `toFileErrors` (ProblemDetails `files[i]` → error por archivo). Ampliar `ticketSubmission.ts` y `ticketsGateway.ts` (envío multipart sobre `core/http`).
- [ ] **T-08 — Frontend controller** · Capa: Frontend (controllers) · Dificultad: Bajo  
  Descripción: ampliar `useSubmitTicketController` con `addFiles`, `removeFile`, errores por archivo y bloqueo durante la subida. Prueba Vitest del reducer (añadir, quitar, rechazar en cliente, errores del servidor).
- [ ] **T-09 — Frontend views** · Capa: Frontend (views) · Dificultad: Bajo  
  Descripción: `AttachmentPickerView` (lista de archivos con nombre, tamaño y error; `accept` con la lista blanca solo como ayuda) integrado en `SubmitTicketFormView`; aviso visible de que los archivos se publican mediante enlaces compartibles (propuesta, ADR-0006). `npm --prefix frontend run lint` sin violaciones MVC.
- [ ] **T-10 — Entorno** · Capa: Docker/Compose · Dificultad: Bajo  
  Descripción: verificar con `docker compose up --build` que el backend crea el contenedor en Azurite y que la URL devuelta abre desde el navegador; ajustar `client_max_body_size` de nginx solo si la decisión del máximo lo exige.

## Criterios de aceptación

### CHU-01 — Radicación con varios adjuntos válidos

**Dado** Ana, solicitante de la empresa A, con un PDF de 204 800 bytes y un PNG de 1 048 576 bytes  
**Cuando** radica un ticket válido en `multipart/form-data` con ambos archivos  
**Entonces** recibe `201` con dos `attachments` (`application/pdf` e `image/png`, tamaños exactos, `url` no vacía); existen dos blobs en Azurite; la tabla de adjuntos guarda solo metadatos (ninguna columna binaria) con contexto `Submission` y `uploaded_by` = Ana; y una petición anónima `GET` a cada `url` devuelve los mismos bytes (comportamiento aceptado por ADR-0006).

### CHU-02 — Límite de tamaño exacto

**Dado** un archivo PDF válido de 10 485 760 bytes y otro de 10 485 761 bytes  
**Cuando** se radica con cada uno por separado  
**Entonces** el primero responde `201`; el segundo responde `413` con `errors.files[0]` que nombra el archivo y el límite; y tras el rechazo no existe ticket nuevo, blob nuevo ni entrada `TicketSubmitted`.

### CHU-03 — Tipo no admitido

**Dado** archivos `instalador.exe`, `diagrama.svg`, `datos.zip`, `informe.docx` y `macro.xlsm`  
**Cuando** se radica con cada uno  
**Entonces** todos responden `415` con el nombre del archivo en `errors` y no se crea nada.

### CHU-04 — Firma que no corresponde a la extensión

**Dado** un ejecutable (`4D 5A`) renombrado a `factura.pdf`, un PNG renombrado a `reporte.pdf`, un `.xlsx` que es un ZIP sin `xl/workbook.xml` y un PDF enviado con `Content-Type: image/png`  
**Cuando** se radica con cada uno  
**Entonces** todos responden `415` y no se crea nada; la validación del navegador desactivada (petición directa con `curl`) no cambia el resultado.

### CHU-05 — Atomicidad con el almacenamiento

**Dado** una radicación con un PDF válido y un `.exe`, y otra radicación válida en la que se fuerza un fallo al confirmar la transacción después de subir los blobs  
**Cuando** se procesan  
**Entonces** la primera responde `415` sin subir ningún blob; en la segunda no queda ticket ni metadatos y los blobs subidos se eliminan (o, si el borrado falla, se registra un aviso con el nombre del blob huérfano).

### CHU-06 — Autorización y aislamiento

**Dado** un administrador, un desarrollador y Ana (empresa A) que añade la parte `companyId` = B o `ticketId` = ticket de B  
**Cuando** envían una radicación multipart con un PDF válido  
**Entonces** administrador y desarrollador reciben `403`; la petición de Ana responde `400` (parte no admitida) y ningún adjunto queda ligado a un ticket de la empresa B.

### CHU-07 — Auditoría con adjuntos

**Dado** la radicación exitosa de CHU-01  
**Cuando** se consulta la entrada `TicketSubmitted`  
**Entonces** su `new_value` incluye la lista de adjuntos con `id`, `fileName` y `sizeBytes`, con actor Ana y fecha UTC, en la misma transacción que el ticket.

### CHU-08 — Nombre de archivo saneado y tipo canónico

**Dado** un PDF válido enviado con nombre `../../etc/factura.pdf` y `Content-Type: application/pdf`  
**Cuando** se radica  
**Entonces** el metadato guarda `fileName` = `factura.pdf`, el blob se nombra `{ticketId}/{attachmentId}.pdf` (sin el nombre original) y su `Content-Type` es `application/pdf`.

### CHU-09 — Comportamiento de la UI

**Dado** el formulario de radicación en el navegador  
**Cuando** la persona elige un archivo de 11 MB, un `.exe` y un PDF válido, y luego el servidor responde `415` para un archivo que el navegador no detectó  
**Entonces** los dos primeros se marcan con error antes de enviar y no se envían; el PDF se adjunta; el error `415` del servidor aparece junto al archivo correspondiente; los datos del formulario se conservan; y durante la subida el botón de envío está deshabilitado.

## Definition of Done

- [ ] CHU-01 a CHU-09 validados con evidencia.
- [ ] Decisión del máximo de archivos por radicación registrada (ADR o [[pendientes]]) y límites de Kestrel/`FormOptions`/nginx coherentes con ella.
- [ ] Pruebas backend escritas primero y en verde (`cd backend && dotnet test`): `Validate_SizeOf10485760_IsAccepted`, `Validate_SizeOf10485761_IsRejected`, `Validate_ExeRenamedToPdf_IsRejected`, `SubmitTicket_WithOneInvalidFile_CreatesNothing`, `SubmitTicket_UploadsBeforeCommit_AndDeletesBlobsWhenCommitFails`, `BlobStorage_UploadsWithCanonicalContentType_AndPublicUrlIsReadableAnonymously`, `PostTicketMultipart_With10485761Bytes_Returns413AndCreatesNothing`, `PostTicketMultipart_WithFakePdf_Returns415`.
- [ ] `DataTicket.ArchitectureTests` en verde (`Application` no referencia `Azure.Storage.Blobs`).
- [ ] Migración EF Core `AddAttachments` creada, aplicada en Compose y reversible.
- [ ] Pruebas Vitest en verde (`npm --prefix frontend test`): `prevalidateFile`, `buildSubmissionFormData`, `toFileErrors`, reducer de archivos.
- [ ] `npm --prefix frontend run lint` sin errores y `npm --prefix frontend run build` correcto.
- [ ] `docker compose up --build`: radicación con adjuntos desde `http://localhost:5173` y apertura de la URL en Azurite (`http://localhost:10000`).
- [ ] Wiki: [[archivos-adjuntos]] (lista blanca, firma, contexto, límites), [[adr-0006-urls-publicas-azure-blob]] (consecuencias implementadas), [[entorno-docker]] (contenedor y límites), [[modelo-de-dominio]] (`Attachment` con contexto), [[persistencia-postgresql]], [[backend-hexagonal]] (`IFileStorage` implementado), [[glosario]] (`AttachmentContext`) mediante Notas para la wiki; V-06 propuesto para cerrar.
- [ ] `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos (revisión explícita de la validación de firma y de la compensación).
- [ ] PR revisado y aprobado por otra persona del equipo.
- [ ] Trazabilidad de la HU y de [[ep-005-radicacion-de-tickets]] actualizada.

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
| DoD-01 Decisión máximo de archivos | Pendiente | — | — |
| DoD-02 Pruebas backend | Pendiente | — | — |
| DoD-03 ArchitectureTests | Pendiente | — | — |
| DoD-04 Migración `AddAttachments` | Pendiente | — | — |
| DoD-05 Pruebas Vitest | Pendiente | — | — |
| DoD-06 Lint y build frontend | Pendiente | — | — |
| DoD-07 `docker compose up --build` | Pendiente | — | — |
| DoD-08 Wiki actualizada | Pendiente | — | — |
| DoD-09 quality-reviewer | Pendiente | — | — |
| DoD-10 PR revisado | Pendiente | — | — |
| DoD-11 Trazabilidad | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

> [!danger] Riesgo aceptado
> Con URL pública permanente, cualquiera con el enlace abre el archivo, incluso de otra empresa y sin sesión (V-17). CHU-01 lo verifica como comportamiento esperado, no como fallo. La aprobación explícita de la dirección sigue pendiente (PRD §16.6) → [[adr-0006-urls-publicas-azure-blob]].

- **Hallazgo 4** — se propone `AttachmentContext` {`Submission`, `Chat`, `FormalResponse`} y derivar de él la visibilidad para el cliente (`Submission` y `FormalResponse` visibles; `Chat` interno). Actualizar [[modelo-de-dominio]].
- (propuesta) Orden "validar todo → subir → confirmar → compensar" en vez de subir en dos pasos (subida previa y referencia): menos piezas para el piloto; el riesgo de blobs huérfanos se mitiga con la compensación y un aviso en el log.
- (propuesta) Nombres de blob con GUID: no aportan seguridad real (la URL sigue siendo pública) pero evitan exponer el nombre original y colisiones.

## Relacionado

- [[ep-005-radicacion-de-tickets]] · [[tablero-scrum]] · [[archivos-adjuntos]] · [[adr-0006-urls-publicas-azure-blob]] · [[entorno-docker]]
- [[persistencia-postgresql]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[modelo-de-dominio]] · [[criterios-de-aceptacion]] · [[pendientes]]

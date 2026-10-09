---
title: "HU-035 — Enviar la respuesta formal por correo"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/notificaciones, producto/respuesta-formal, arquitectura/backend]
sources: ["PRD.md §5", "PRD.md §6.5", "PRD.md §8", "PRD.md §11", "PRD.md §14", "PRD.md §16"]
aliases: ["HU-035", "Enviar la respuesta formal por correo", "Correo de respuesta formal"]
epica: "[[ep-010-respuesta-formal-y-cierre]]"
criterios_prd: ["CA-10", "CA-11"]
componentes: ["Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Persistencia PostgreSQL", "Docker/Compose", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Medio"
sprint_sugerido: "Sprint 7"
dependencias: ["[[hu-033-emitir-respuesta-formal]]", "[[hu-005-invitar-y-activar-cuentas]]", "[[hu-008-administrar-usuarios-cliente]]"]
relacionadas: ["[[hu-034-portal-respuesta-formal]]", "[[hu-038-correo-de-vinculacion]]", "[[hu-036-cerrar-ticket-manualmente]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-035 — Enviar la respuesta formal por correo

Al emitirse la respuesta formal, DataTicket envía al contacto del cliente un correo con el texto completo y los archivos asociados, mediante el puerto `IEmailSender` (Mailpit en local). Un fallo de envío no deshace la respuesta ya persistida (PRD §6.5, §11, §14.11).

## Historia de usuario

**COMO** solicitante o coordinador de una empresa cliente  
**QUIERO** recibir por correo la respuesta formal completa con sus archivos  
**PARA** enterarme de la solución sin tener que entrar al portal y conservarla en mi buzón

## Contexto

La respuesta formal "se envía completa por correo" (PRD §6.5) "al contacto de cliente correspondiente, con texto completo y archivos asociados" (PRD §11). El PRD no define quién es ese contacto (V-07) ni si los archivos van adjuntos o como enlaces. El proveedor productivo está pendiente (PRD §16.2); en local, Compose ya inyecta `Email:Smtp:Host=mailpit`, `Email:Smtp:Port=1025` y `Email:FromAddress` ([[entorno-docker]]). El enlace al portal se construye con `App:PublicBaseUrl`, nunca con el `Host` de la petición ([[autenticacion-identity]]); esa clave aún no está en `docker-compose.yml`.

## Alcance

- Enviar el correo tras confirmarse la persistencia de la respuesta ([[hu-033-emitir-respuesta-formal|HU-033]]).
- Plantilla con número, título, texto completo y archivos; enlace al ticket en el portal.
- Archivos como adjuntos MIME del correo; si el total supera el límite configurado del proveedor, enlaces públicos por archivo ([[adr-0006-urls-publicas-azure-blob]]) (propuesta).
- Estado de entrega del correo en la respuesta (`Pending`, `Sent`, `Failed`; nombre propuesto `EmailDeliveryStatus`) y reintento manual por la PM o un administrador.
- Regla de destinatarios encapsulada en un único punto, con valor provisional mientras V-07 siga abierta.
- Reutilizar `App:PublicBaseUrl`, que añade [[hu-005-invitar-y-activar-cuentas|HU-005]] a `docker-compose.yml`; completarlo solo si no existe al iniciar esta HU.

## Fuera de alcance

- Correo por cada transición del estado resumido (PRD §11: no garantizado).
- Correo con estado técnico al cliente (PRD §11).
- Elegir y configurar el proveedor productivo (PRD §16.2).
- Cola de mensajería, *outbox* o reintentos automáticos en segundo plano (propuesta: solución simple con reintento manual).
- Correo de vinculación interno → [[hu-038-correo-de-vinculacion|HU-038]].

## Requisitos y reglas de negocio

- La respuesta formal se envía completa por correo, con texto completo y archivos asociados (PRD §6.5, §11).
- No se revela el estado técnico por correo al cliente (PRD §11).
- El cliente nunca ve nombres de colaboradores internos, URL de PR ni chat (PRD §5.6).
- La auditoría, no el correo, es la fuente de trazabilidad (PRD §11).
- Destinatario: "contacto de cliente correspondiente" (PRD §11), sin definir → V-07. Valor provisional (propuesta): el solicitante del ticket; el coordinador se añade si V-07 lo confirma.
- Los enlaces de los correos se construyen con `App:PublicBaseUrl` ([[autenticacion-identity]]).
- Propuesta: un fallo de envío no revierte la respuesta persistida; se marca `Failed`, se registra en logs y la PM o el administrador pueden reintentar.
- Propuesta: límite total de adjuntos del correo configurable (`Email:MaxAttachmentBytes`, nombre propuesto); por encima se envían enlaces.

## Invariantes en juego

- Invariante 2 (`AGENTS.md` §7): el correo no expone chat, estados técnicos, URL de PR ni nombres internos.
- Invariante 6: solo Elizabeth o un administrador emite (y reintenta enviar) la respuesta.
- Invariante 1: el correo va solo a contactos de la empresa del ticket.

## Criterios del PRD cubiertos

- PRD CA-11 (parcial: "se envían por correo" la respuesta completa y sus adjuntos) → [[criterios-de-aceptacion]]
- PRD CA-10 (parcial: el correo no expone información interna) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-010-respuesta-formal-y-cierre]]
- Dependencias: [[hu-033-emitir-respuesta-formal|HU-033]] (respuesta persistida), [[hu-005-invitar-y-activar-cuentas|HU-005]] (adaptador SMTP de `IEmailSender` y `App:PublicBaseUrl`), [[hu-008-administrar-usuarios-cliente|HU-008]] (correo y rol del solicitante y del coordinador).
- Relacionadas: [[hu-034-portal-respuesta-formal|HU-034]] (enlace de destino), [[hu-038-correo-de-vinculacion|HU-038]] (misma infraestructura de correo), [[hu-036-cerrar-ticket-manualmente|HU-036]].
- Decisiones: abiertas V-07 (destinatarios) y proveedor de correo (PRD §16.2) → [[pendientes]]; [[adr-0006-urls-publicas-azure-blob]].

## Componentes afectados

- Backend (Application): paso de envío posterior a `DeliverFormalResponse` y caso de uso de reintento (`ResendFormalResponseEmail`, nombre propuesto); plantilla del correo; puerto `IEmailSender` (extensión para adjuntos si no la tiene).
- Backend (Infrastructure): adaptador SMTP con adjuntos MIME; lectura de archivos vía `IFileStorage`.
- Backend (Api): endpoint de reintento.
- Persistencia PostgreSQL: columna de estado de entrega en `FormalResponse` (migración).
- Docker/Compose: `App__PublicBaseUrl`.
- Frontend: indicador de estado de entrega y botón de reintento en el detalle interno.

## Dificultad

**Nivel:** Medio

**Justificación:** reutiliza el puerto de correo existente, pero añade adjuntos MIME, manejo de fallo sin revertir, una regla de destinatarios sujeta a decisión y verificación en Mailpit.

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01.

**Envío automático:** sin endpoint propio; ocurre al final de `POST /api/tickets/{ticketId}/formal-response` ([[hu-033-emitir-respuesta-formal|HU-033]]), después de confirmar la transacción. El `201` devuelve `"emailDelivery": "Pending"` o el resultado si el envío fue síncrono.

**Reintento:** `POST /api/tickets/{ticketId}/formal-response/email` (roles `ProductManager` o `Administrator` participante) → `202 Accepted`:

```json
{ "emailDelivery": "Sent", "lastAttemptAt": "2026-10-07T20:21:09Z", "recipients": 1 }
```

| Código | Caso |
|---|---|
| 401 | Sin sesión |
| 403 | Usuario cliente, o participante de Desarrollo o Producción (rol insuficiente) |
| 404 | Ticket inexistente o no visible (interno no participante, administrador no asociado; convención de [[hu-020-detalle-interno-del-ticket\|HU-020]]) o sin respuesta formal |
| 409 | El correo ya figura como `Sent` |
| 502 | El servidor de correo rechazó el envío (el estado queda `Failed`) |

El detalle interno (`GET /api/tickets/{ticketId}/formal-response`) incluye `emailDelivery` y `lastAttemptAt`. El portal no los expone.

**Plantilla del correo** (texto y HTML; propuesta):

| Campo | Valor |
|---|---|
| De | `Email:FromAddress` (local: `no-reply@dataticket.local`) |
| Para | Destinatarios según V-07 (provisional: solicitante del ticket) |
| Asunto | `[DataTicket] Solución entregada · Ticket {Number}: {Title}` |
| Saludo | `Hola, {RecipientDisplayName}:` |
| Cuerpo | `Data Global entregó la solución de tu ticket {Number} — {Title}.` + texto completo de la respuesta (escapado en HTML, saltos de línea conservados) |
| Archivos | Adjuntos MIME con su nombre original; o, si superan `Email:MaxAttachmentBytes`, lista `{FileName} ({tamaño}) — {url pública}` |
| Enlace | `Consultar en el portal: {App:PublicBaseUrl}/{ruta del detalle del portal}/{ticketId}` (ruta del SPA según el enrutador, decisión abierta) |
| Firma | `Equipo de soporte de Data Global` (sin nombres de personas) |
| Excluye | Estado interno, participantes, emisor, URL de PR, chat, adjuntos internos |

## Tareas de desarrollo

- [ ] **T-01 — Contrato y plantilla** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar plantilla, forma de los archivos (adjuntos MIME con alternativa de enlaces), estado de entrega, endpoint de reintento y destinatario provisional; registrar V-07 como dependencia viva en [[pendientes]].
- [ ] **T-02 — Plantilla y destinatarios** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: primero pruebas: `FormalResponseEmail_ContainsFullBody`, `FormalResponseEmail_DoesNotContainInternalStatusOrNames`, `FormalResponseEmail_EscapesHtml`, `FormalResponseEmail_LinkUsesPublicBaseUrl`, `Recipients_DefaultToRequester`. Luego el generador de la plantilla y la función única de destinatarios.
- [ ] **T-03 — Envío tras persistir y manejo de fallo** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: primero pruebas: `DeliverFormalResponse_SendsEmailAfterCommit`, `DeliverFormalResponse_WhenEmailFails_KeepsResponseAndMarksFailed`, `DeliverFormalResponse_WhenPersistenceFails_DoesNotSendEmail`. Luego el paso de envío y el caso de uso de reintento con autorización.
- [ ] **T-04 — Adaptador SMTP con adjuntos** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: extender el adaptador de `IEmailSender` para adjuntos MIME leídos con `IFileStorage`; alternativa de enlaces por encima del límite. Prueba de integración contra Mailpit (API `http://localhost:8025/api/v1/messages`) o, si no está disponible en CI, marcada como manual.
- [ ] **T-05 — Esquema y configuración** · Capa: Backend (Infrastructure) + Docker/Compose · Dificultad: Bajo  
  Descripción: migración `AddFormalResponseEmailDelivery` (estado y `LastAttemptAt`); `App__PublicBaseUrl: http://localhost:5173` en `docker-compose.yml` y variable en `.env.example` si no existen; opciones tipadas validadas al arrancar.
- [ ] **T-06 — Endpoint de reintento** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: primero pruebas de integración: 202 (PM), 403 (desarrollador participante), 403 (solicitante), 404 (administrador no asociado), 409 (ya enviado), 404 (sin respuesta). Luego el endpoint.
- [ ] **T-07 — Frontend interno** · Capa: Frontend (models/controllers/views) · Dificultad: Bajo  
  Descripción: Vitest del modelo (`emailDelivery` desconocido → `Failed`); el controlador expone `retryEmail()`; la vista muestra "Correo enviado", "Envío pendiente" o "No se pudo enviar el correo · Reintentar".

## Criterios de aceptación

### CHU-01 — Correo completo en Mailpit

**Dado** el stack local con Mailpit y un ticket del solicitante S con correo `s@empresa-a.test`  
**Cuando** la PM emite la respuesta con un cuerpo de 1 200 caracteres y dos archivos (PDF de 2 MB y XLSX de 1 MB)  
**Entonces** en `http://localhost:8025` aparece un correo para `s@empresa-a.test` con asunto `[DataTicket] Solución entregada · Ticket {Number}: {Title}`, el cuerpo íntegro y los dos archivos adjuntos con sus nombres originales.

### CHU-02 — Sin información interna en el correo

**Dado** un ticket con URL de PR, tres participantes internos, mensajes de chat y estado previo `InProduction`  
**Cuando** se genera el correo de la respuesta  
**Entonces** el texto y el HTML no contienen la URL de PR, los nombres de los participantes ni de la emisora, términos de estado interno (`InProduction`, "Revisión de PR", "En producción") ni adjuntos del chat; la firma es "Equipo de soporte de Data Global".

### CHU-03 — El fallo de envío no revierte la respuesta

**Dado** que el servidor SMTP está detenido  
**Cuando** la PM emite la respuesta  
**Entonces** recibe `201`, el ticket queda en `SolutionDelivered`, la respuesta queda persistida con `emailDelivery: "Failed"` y el fallo se registra en logs sin datos del cuerpo.

### CHU-04 — Reintento autorizado

**Dado** una respuesta con `emailDelivery: "Failed"` y Mailpit de nuevo disponible  
**Cuando** la PM llama `POST /api/tickets/{ticketId}/formal-response/email`  
**Entonces** recibe `202`, el correo llega a Mailpit y el estado pasa a `Sent`; un segundo reintento devuelve `409`.

### CHU-05 — Reintento rechazado a roles no autorizados

**Dado** una respuesta con envío fallido  
**Cuando** un desarrollador participante o el solicitante llaman al endpoint de reintento  
**Entonces** reciben `403` y no se envía ningún correo.

### CHU-06 — Enlace seguro al portal

**Dado** `App:PublicBaseUrl = http://localhost:5173`  
**Cuando** la emisión llega con la cabecera `Host: evil.example`  
**Entonces** el enlace del correo empieza por `http://localhost:5173/` y no contiene `evil.example`; abrir el enlace sin sesión lleva al inicio de sesión.

### CHU-07 — Destinatarios acotados a la empresa del ticket

**Dado** un ticket de la empresa A  
**Cuando** se envía el correo de la respuesta  
**Entonces** todos los destinatarios son usuarios activos de la empresa A según la regla provisional (solicitante); ningún usuario de la empresa B ni interno figura en `Para`, `CC` o `CCO`.

### CHU-08 — Archivos que superan el límite del correo

**Dado** `Email:MaxAttachmentBytes = 15728640` (15 MB) y una respuesta con dos archivos de 9 MB  
**Cuando** se envía el correo  
**Entonces** el correo no lleva adjuntos MIME y lista cada archivo con su nombre, tamaño y URL pública (propuesta).

### CHU-09 — Estado de entrega en la interfaz

**Dado** una respuesta con envío fallido  
**Cuando** la PM abre el detalle interno  
**Entonces** ve "No se pudo enviar el correo" con el botón "Reintentar"; un desarrollador participante ve el estado pero no el botón.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-09 validados con evidencia en la matriz.
- [ ] **DoD-02** — Pruebas escritas primero y en verde (`cd backend && dotnet test`): `FormalResponseEmail_*`, `DeliverFormalResponse_WhenEmailFails_KeepsResponseAndMarksFailed`, `DeliverFormalResponse_WhenPersistenceFails_DoesNotSendEmail` y las de integración del reintento.
- [ ] **DoD-03** — `DataTicket.ArchitectureTests` en verde (el adaptador SMTP vive en `Infrastructure`).
- [ ] **DoD-04** — Verificación en Mailpit (`http://localhost:8025`) con captura o salida de la API de Mailpit adjunta como evidencia (asunto, destinatario, cuerpo y adjuntos).
- [ ] **DoD-05** — Migración `AddFormalResponseEmailDelivery` creada y aplicada en local.
- [ ] **DoD-06** — `App__PublicBaseUrl` presente en `docker-compose.yml` y `.env.example`; `docker compose up --build` levanta el stack.
- [ ] **DoD-07** — Frontend: `npm --prefix frontend test`, `npm --prefix frontend run lint` y `npm --prefix frontend run build` correctos.
- [ ] **DoD-08** — Wiki actualizada: [[notificaciones]] (plantilla y política de fallo), [[entorno-docker]] (`App:PublicBaseUrl`), [[autenticacion-identity]] (si cambia `IEmailSender`), [[persistencia-postgresql]]; V-07 registrada en [[pendientes]].
- [ ] **DoD-09** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO y PR revisado por otra persona del equipo.
- [ ] **DoD-10** — La trazabilidad de la HU y de [[ep-010-respuesta-formal-y-cierre]] está actualizada.

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

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- **Dependencia V-07:** destinatarios sin definir; provisionalmente el solicitante. Si se decide "coordinador" o "ambos", solo cambia la función de destinatarios y CHU-07.
- **Propuesta:** archivos como adjuntos MIME; enlaces públicos por encima de `Email:MaxAttachmentBytes`. Reenviar el correo expone las URL públicas fuera del portal ([[adr-0006-urls-publicas-azure-blob]]).
- **Propuesta:** reintento manual en vez de cola o *outbox*.
- **Riesgo:** el proveedor productivo (PRD §16.2) puede imponer límites de tamaño o requerir SPF/DKIM; Mailpit no los reproduce.

## Relacionado

- [[ep-010-respuesta-formal-y-cierre]] · [[tablero-scrum]] · [[notificaciones]] · [[autenticacion-identity]] · [[entorno-docker]] · [[archivos-adjuntos]] · [[roles-y-permisos]] · [[criterios-de-aceptacion]] · [[adr-0006-urls-publicas-azure-blob]]

---
title: "HU-038 — Correo de vinculación en la primera asociación"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/notificaciones, producto/ticket, arquitectura/backend]
sources: ["PRD.md §5", "PRD.md §6.2", "PRD.md §7", "PRD.md §11", "PRD.md §12", "PRD.md §16"]
aliases: ["HU-038", "Correo de vinculación en la primera asociación", "Correo de vinculación"]
epica: "[[ep-011-notificaciones]]"
criterios_prd: []
componentes: ["Backend (Application)", "Backend (Infrastructure)", "Persistencia PostgreSQL", "Docker/Compose", "Identity"]
dificultad: "Medio"
sprint_sugerido: "Sprint 6"
dependencias: ["[[hu-018-asignar-y-agregar-participantes]]", "[[hu-017-cola-de-cobertura-y-tomar-ticket]]", "[[hu-005-invitar-y-activar-cuentas]]", "[[hu-019-reasignar-y-retirar-participantes]]"]
relacionadas: ["[[hu-037-notificaciones-en-la-app]]", "[[hu-020-detalle-interno-del-ticket]]", "[[hu-035-correo-de-respuesta-formal]]", "[[hu-003-iniciar-y-cerrar-sesion]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-038 — Correo de vinculación en la primera asociación

Cuando una persona interna queda asociada por primera vez a un ticket, recibe un correo con el número, el título y un enlace al ticket construido con `App:PublicBaseUrl`. El correo no lleva contenido sensible y no sustituye el control de acceso: el enlace exige iniciar sesión y ser participante (PRD §7.7, §11).

## Historia de usuario

**COMO** integrante de Desarrollo o Producción (o administrador) recién asociado a un ticket  
**QUIERO** recibir un correo que me avise de la vinculación con un enlace al ticket  
**PARA** enterarme de que tengo trabajo asignado aunque no esté conectado a DataTicket

## Contexto

PRD §7.7: "Cuando una persona es asociada por primera vez al ticket recibe un correo de vinculación/asignación. Ese correo no sustituye el control de acceso del portal". La asociación ocurre al asignar o agregar participantes ([[hu-018-asignar-y-agregar-participantes|HU-018]]) y cuando un administrador toma un ticket en cobertura ([[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]]). V-08 deja abierto si se reenvía a quien fue retirado y vuelto a agregar y si lo recibe el administrador que se asocia a sí mismo. Los enlaces de correo se construyen con `App:PublicBaseUrl` para evitar el envenenamiento por `Host` ([[autenticacion-identity]]); esa clave aún no está en `docker-compose.yml`.

## Alcance

- Detectar la **primera** asociación de una persona a un ticket: no existe ningún `TicketParticipant` previo (vigente o retirado) para ese par (ticket, usuario).
- Enviar el correo por `IEmailSender` después de confirmar la asociación.
- Plantilla con número, título, quién vinculó y enlace; sin descripción, adjuntos, chat ni datos del cliente más allá de lo imprescindible (propuesta de mínima exposición).
- Un solo correo por persona aunque llegue repetida en la misma petición.
- Manejo de fallo sin revertir la asociación.
- Reutilizar `App:PublicBaseUrl`, que añade [[hu-005-invitar-y-activar-cuentas|HU-005]] a `docker-compose.yml`; completarlo solo si no existe al iniciar esta HU.

## Fuera de alcance

- Notificaciones in-app → [[hu-037-notificaciones-en-la-app|HU-037]].
- Correo al retirar a una persona (no está en PRD §11).
- Correo a clientes por asignaciones (el cliente no ve participantes, PRD §5.6).
- Reintentos automáticos, colas u *outbox* (propuesta: solución simple).
- Proveedor productivo de correo (PRD §16.2).
- Preferencias para desactivar el correo (no aparecen en el PRD).

## Requisitos y reglas de negocio

- La primera asociación de una persona a un ticket dispara un correo de vinculación a esa persona (PRD §7.7, §11).
- El correo no sustituye el control de acceso del portal (PRD §7.7).
- El acceso al detalle y al chat es por asociación explícita (PRD §6.2).
- La auditoría, no el correo, es la fuente de trazabilidad (PRD §11); la asociación ya se audita en HU-017/HU-018.
- Enlaces con `App:PublicBaseUrl`, nunca con el `Host` de la petición ([[autenticacion-identity]]).
- V-08 (provisional, propuesta): re-asociación tras retiro **no** reenvía el correo; quien se asocia a sí mismo (administrador que toma un ticket, PM que se agrega) **no** lo recibe.
- Propuesta: el correo no incluye descripción, adjuntos, chat ni nombre de la empresa cliente.
- Propuesta: un fallo de envío no revierte la asociación; se registra en logs sin datos sensibles.

## Invariantes en juego

- Invariante 3 (`AGENTS.md` §7): el acceso depende de la participación vigente validada en servidor; el enlace no otorga acceso.
- Invariante 5: el administrador queda asociado antes de ver el detalle; el correo no cambia ese orden.
- Invariante 9 (relacionado): enlaces construidos con URL base configurada, como en invitaciones y restablecimiento.

## Criterios del PRD cubiertos

- Ningún criterio del PRD §14 cubre este correo de forma directa; se verifica contra PRD §7.7 y §11 → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-011-notificaciones]]
- Dependencias: [[hu-018-asignar-y-agregar-participantes|HU-018]] (`AssignParticipants`), [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]] (`TakeTicket`), [[hu-019-reasignar-y-retirar-participantes|HU-019]] (retiro conserva el registro), [[hu-005-invitar-y-activar-cuentas|HU-005]] (adaptador SMTP de `IEmailSender`, `App:PublicBaseUrl`).
- Relacionadas: [[hu-037-notificaciones-en-la-app|HU-037]], [[hu-020-detalle-interno-del-ticket|HU-020]] (destino del enlace), [[hu-035-correo-de-respuesta-formal|HU-035]] (misma infraestructura), [[hu-003-iniciar-y-cerrar-sesion|HU-003]] (redirección al login).
- Decisiones: abiertas V-08 y proveedor de correo (PRD §16.2); [[adr-0004-autenticacion-cookie-mismo-origen]] (propuesta) → [[pendientes]].

## Componentes afectados

- Backend (Application): paso posterior a `AssignParticipants` y `TakeTicket`; plantilla del correo; puerto `IEmailSender`.
- Backend (Infrastructure): adaptador SMTP existente; consulta "¿hubo asociación previa?" sobre `TicketParticipant`.
- Persistencia PostgreSQL: sin cambio de esquema si `TicketParticipant` conserva los retirados (`RemovedAt`); si no, migración.
- Docker/Compose: `App__PublicBaseUrl`.
- Identity: correo y nombre del destinatario.
- Frontend: no aplica (ver contrato).

## Dificultad

**Nivel:** Medio

**Justificación:** reutiliza el puerto de correo, pero la regla de "primera vez" depende del historial de participantes y de una decisión abierta (V-08), y exige pruebas de seguridad del enlace y verificación en Mailpit.

## Contrato backend ↔ frontend

**REST/hub:** no aplica. La HU no crea endpoints ni eventos: el correo es un efecto secundario de los endpoints de [[hu-018-asignar-y-agregar-participantes|HU-018]] y [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]], cuyos contratos no cambian. El frontend no se modifica; el enlace apunta a la ruta del detalle interno que defina [[hu-020-detalle-interno-del-ticket|HU-020]] (enrutador: decisión abierta).

**Plantilla del correo** (texto y HTML; propuesta, se ratifica en T-01):

| Campo | Valor |
|---|---|
| De | `Email:FromAddress` (local: `no-reply@dataticket.local`) |
| Para | Persona asociada (solo ella) |
| Asunto | `[DataTicket] Te vincularon al ticket {Number}: {Title}` |
| Cuerpo | `Hola, {RecipientDisplayName}: {ActorDisplayName} te vinculó al ticket {Number} — {Title} el {fecha y hora en America/Bogota}.` |
| Enlace | `Abrir el ticket: {App:PublicBaseUrl}/{ruta del detalle interno}/{ticketId}` |
| Aviso | `Para ver el ticket debes iniciar sesión en DataTicket. Este correo no da acceso por sí mismo.` |
| Excluye | Descripción, adjuntos y sus URL públicas, mensajes del chat, URL de PR, nombre de la empresa cliente y del solicitante |

Ejemplo de valores: `Number = DT-000123` (formato pendiente V-16), `Title = Error al generar factura electrónica`.

## Tareas de desarrollo

- [ ] **T-01 — Contrato del correo y reglas V-08** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar plantilla, campos excluidos y la regla provisional de V-08 con la persona usuaria; confirmar la ruta del detalle interno con HU-020; registrar V-08 en [[pendientes]].
- [ ] **T-02 — Regla de primera asociación** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: primero pruebas: `FirstAssociation_SendsLinkEmail`, `ReAssociationAfterRemoval_DoesNotSendEmail`, `SelfAssociation_ByAdminTakingTicket_DoesNotSendEmail`, `DuplicateUserInSameRequest_SendsSingleEmail`, `AssociationPersistenceFails_DoesNotSendEmail`. Luego el paso de envío tras el commit en `AssignParticipants` y `TakeTicket`.
- [ ] **T-03 — Plantilla segura** · Capa: Backend (Application) · Dificultad: Bajo  
  Descripción: primero pruebas: `LinkEmail_UsesPublicBaseUrl_IgnoringHostHeader`, `LinkEmail_DoesNotContainDescriptionAttachmentsOrCompany`, `LinkEmail_EscapesTitleHtml`. Luego el generador de la plantilla.
- [ ] **T-04 — Fallo de envío** · Capa: Backend (Application/Infrastructure) · Dificultad: Bajo  
  Descripción: primero prueba `EmailFailure_KeepsAssociationAndLogsWithoutContent`. Luego captura del error del adaptador SMTP y log estructurado (ticket, destinatario por id, sin título ni cuerpo).
- [ ] **T-05 — Configuración** · Capa: Docker/Compose + Backend (Api) · Dificultad: Bajo  
  Descripción: `App__PublicBaseUrl: http://localhost:5173` en `docker-compose.yml` y variable en `.env.example` si faltan; opciones tipadas validadas al arrancar (URL absoluta obligatoria).
- [ ] **T-06 — Verificación en Mailpit** · Capa: Transversal · Dificultad: Bajo  
  Descripción: prueba de integración que asocia a una persona y consulta la API de Mailpit (`http://localhost:8025/api/v1/messages`), o verificación manual documentada si Mailpit no está disponible en CI.

## Criterios de aceptación

### CHU-01 — Correo en la primera asociación

**Dado** Brayan (Desarrollo), nunca asociado al ticket T, y el stack local con Mailpit  
**Cuando** Elizabeth lo agrega como participante de T  
**Entonces** en `http://localhost:8025` aparece un único correo para Brayan con asunto `[DataTicket] Te vincularon al ticket {Number}: {Title}` y un enlace que empieza por el valor de `App:PublicBaseUrl`.

### CHU-02 — Varias personas en una sola asignación

**Dado** Laura y Kevin, nunca asociados a T  
**Cuando** Elizabeth asigna a ambos en una petición que repite a Kevin dos veces  
**Entonces** se envían exactamente dos correos: uno a Laura y uno a Kevin.

### CHU-03 — Re-asociación tras retiro (provisional V-08)

**Dado** Brayan agregado a T (recibió correo), luego retirado ([[hu-019-reasignar-y-retirar-participantes|HU-019]])  
**Cuando** Elizabeth lo vuelve a agregar  
**Entonces** no se envía un segundo correo y la asociación queda registrada en la auditoría (comportamiento provisional hasta resolver V-08).

### CHU-04 — Autoasociación (provisional V-08)

**Dado** un administrador que toma T en cobertura ([[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]])  
**Cuando** queda asociado como participante  
**Entonces** no recibe correo de vinculación, porque él mismo ejecutó la acción.

### CHU-05 — Enlace a prueba de `Host` falsificado

**Dado** `App:PublicBaseUrl = http://localhost:5173`  
**Cuando** la petición de asignación llega con la cabecera `Host: evil.example`  
**Entonces** el enlace del correo empieza por `http://localhost:5173/` y no contiene `evil.example`.

### CHU-06 — Contenido mínimo

**Dado** un ticket con descripción, dos adjuntos de radicación, mensajes de chat y URL de PR  
**Cuando** se genera el correo de vinculación  
**Entonces** el correo no contiene la descripción, las URL de los adjuntos, texto del chat, la URL de PR, el nombre de la empresa cliente ni el del solicitante.

### CHU-07 — El enlace no da acceso

**Dado** el enlace del correo  
**Cuando** se abre sin sesión  
**Entonces** el SPA lleva al inicio de sesión; **y cuando** lo abre con sesión Kevin después de haber sido retirado, el backend responde `404` al detalle interno ([[hu-020-detalle-interno-del-ticket|HU-020]]) y no entrega descripción, adjuntos ni chat.

### CHU-08 — El fallo de correo no revierte la asociación

**Dado** el servidor SMTP detenido  
**Cuando** Elizabeth agrega a Brayan a T  
**Entonces** la petición responde con el código de éxito de HU-018, Brayan queda como participante vigente y el log registra el fallo sin título ni cuerpo del correo.

### CHU-09 — Sin asociación, sin correo

**Dado** un desarrollador sin rol para asignar  
**Cuando** intenta agregar a Brayan a T por la API  
**Entonces** recibe `403` (HU-018) y no se envía ningún correo.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-09 validados con evidencia en la matriz.
- [ ] **DoD-02** — Pruebas escritas primero y en verde (`cd backend && dotnet test`): `FirstAssociation_SendsLinkEmail`, `ReAssociationAfterRemoval_DoesNotSendEmail`, `SelfAssociation_ByAdminTakingTicket_DoesNotSendEmail`, `DuplicateUserInSameRequest_SendsSingleEmail`, `LinkEmail_UsesPublicBaseUrl_IgnoringHostHeader`, `LinkEmail_DoesNotContainDescriptionAttachmentsOrCompany`, `EmailFailure_KeepsAssociationAndLogsWithoutContent`.
- [ ] **DoD-03** — `DataTicket.ArchitectureTests` en verde.
- [ ] **DoD-04** — Verificación en Mailpit (`http://localhost:8025`) con evidencia adjunta (asunto, destinatario, enlace).
- [ ] **DoD-05** — `App__PublicBaseUrl` en `docker-compose.yml` y `.env.example`; `docker compose up --build` levanta el stack.
- [ ] **DoD-06** — Frontend sin cambios: `npm --prefix frontend run lint`, `npm --prefix frontend test` y `npm --prefix frontend run build` siguen en verde.
- [ ] **DoD-07** — Wiki actualizada: [[notificaciones]] (plantilla y regla provisional V-08), [[entorno-docker]] (`App:PublicBaseUrl`), [[autenticacion-identity]] si cambia `IEmailSender`; V-08 actualizada en [[pendientes]].
- [ ] **DoD-08** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO y PR revisado por otra persona del equipo.
- [ ] **DoD-09** — La trazabilidad de la HU y de [[ep-011-notificaciones]] está actualizada.

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
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.

## Notas y decisiones

- **Dependencia V-08:** comportamiento provisional (sin reenvío tras re-asociación; sin correo en autoasociación). Si se decide otra cosa, solo cambian la regla de T-02 y CHU-03/CHU-04.
- **Propuesta:** contenido mínimo; el nombre de la empresa cliente queda fuera aunque el destinatario sea interno, para limitar la exposición si el correo se reenvía.
- **Riesgo:** proveedor productivo pendiente (PRD §16.2); Mailpit no valida entregabilidad.

## Relacionado

- [[ep-011-notificaciones]] · [[tablero-scrum]] · [[notificaciones]] · [[autenticacion-identity]] · [[entorno-docker]] · [[roles-y-permisos]] · [[flujo-del-ticket]] · [[chat-interno]] · [[criterios-de-aceptacion]]

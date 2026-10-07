---
title: "EP-010 — Respuesta formal y cierre manual"
type: epica
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/epica, producto/ticket, producto/respuesta-formal]
sources: ["PRD.md §5", "PRD.md §6.4", "PRD.md §6.5", "PRD.md §8", "PRD.md §10", "PRD.md §11", "PRD.md §12", "PRD.md §13", "PRD.md §14", "PRD.md §16"]
aliases: ["EP-010", "Respuesta formal y cierre manual"]
fase_prd: "2"
criterios_prd: ["CA-01", "CA-02", "CA-09", "CA-10", "CA-11", "CA-12", "CA-14"]
historias: ["[[hu-033-emitir-respuesta-formal]]", "[[hu-034-portal-respuesta-formal]]", "[[hu-035-correo-de-respuesta-formal]]", "[[hu-036-cerrar-ticket-manualmente]]"]
dependencias: ["[[ep-002-identidad-y-acceso]]", "[[ep-004-auditoria-append-only]]", "[[ep-005-radicacion-de-tickets]]", "[[ep-006-triage-asignacion-y-participantes]]", "[[ep-007-trabajo-interno-y-estados]]", "[[ep-008-bandejas-y-portal-del-cliente]]"]
created: 2026-10-07
updated: 2026-10-07
---

# EP-010 — Respuesta formal y cierre manual

Cierra el ciclo de vida del ticket: Elizabeth (PM) o un administrador emite la respuesta formal con texto y archivos, el cliente la consulta en el portal y la recibe completa por correo, y tras la confirmación telefónica el ticket se cierra a mano, sin automatismos (PRD §6.5, §11).

## Objetivo

Que el resultado de cada ticket llegue al cliente por el único canal formal del producto (portal + correo) y que el cierre sea un acto manual, restringido y trazable (PRD §6.5, §14.11, §14.12).

## Valor esperado

- El cliente sabe cuál fue la solución sin ver nada interno (PRD §3, §5.6).
- Data Global conserva evidencia de quién entregó la solución, quién cerró, cuándo y desde qué estado (PRD §6.5, §12).
- Se habilitan las métricas "tiempo hasta respuesta formal" y "tiempo hasta cierre" de la Fase 3 (PRD §4) → [[ep-012-dashboard-y-metricas]].

## Fase del PRD

Fase 2 — Colaboración y entrega (PRD §13). Con esta épica el MVP P0 queda completo (Sprint 7 en [[tablero-scrum]]).

## Actores

- Elizabeth — PM (rol `ProductManager`): emite la respuesta formal y cierra.
- Administrador (`Administrator`): emite y cierra; sin acceso al contenido por ser administrador (PRD §5.5).
- Solicitante (`Requester`) y coordinador de empresa (`CompanyCoordinator`): consultan y reciben la respuesta formal.
- Desarrollo y Producción: preparan contexto y adjuntos en el chat, pero **no** emiten la respuesta (PRD §6.5).

## Alcance

- Emitir la respuesta formal (`DeliverFormalResponse`) con cuerpo de texto y adjuntos validados en backend → [[hu-033-emitir-respuesta-formal|HU-033]].
- Mostrar la respuesta formal en el portal del cliente sin datos internos → [[hu-034-portal-respuesta-formal|HU-034]].
- Enviar la respuesta formal completa por correo vía `IEmailSender` (Mailpit en local) → [[hu-035-correo-de-respuesta-formal|HU-035]].
- Cerrar el ticket manualmente (`CloseTicket`) registrando quién, cuándo y estado anterior → [[hu-036-cerrar-ticket-manualmente|HU-036]].
- Marca de contexto/visibilidad de los adjuntos para distinguir los de la respuesta formal (visibles al cliente) de los internos (propuesta derivada de un hallazgo del plan).

## Fuera de alcance

- Cierre automático, temporizador de vencimiento y recordatorios de cierre (PRD §6.5, §13).
- Grabar, transcribir o guardar detalles de la llamada de confirmación (PRD §6.5).
- Correo automático por cada transición de estado del cliente (PRD §11: no garantizado en el MVP).
- Chat visible al cliente o respuestas tipo conversación (PRD §13).
- Encuesta de satisfacción y SLA (PRD §13).
- Reapertura de tickets cerrados y corrección o reemisión de respuestas formales mientras V-01 y V-13 sigan abiertas.

## Requisitos y reglas de negocio

- Solo Elizabeth o un administrador emiten la respuesta formal; los desarrolladores no (PRD §5.7, §6.5).
- La respuesta incluye cuerpo de texto y archivos, queda en el portal y se envía completa por correo (PRD §6.5, §11).
- Los adjuntos admitidos son PDF, imágenes, XML y Excel, de hasta 10 MB por archivo, validados en backend (PRD §8).
- Emitir la respuesta lleva el ticket a `SolutionDelivered` (PRD §6.4).
- El cierre es manual, por Elizabeth o un administrador, tras la confirmación verbal; se registra quién, cuándo y el estado anterior (PRD §6.5).
- El cliente nunca ve estados técnicos, chat, URL de PR ni nombres de colaboradores internos (PRD §5.6, §10).
- La respuesta formal y el cierre manual son eventos auditados (PRD §12).

## Criterios del PRD cubiertos

- PRD CA-11 — Solo Elizabeth o un administrador publica la respuesta formal; queda en el portal y se envía por correo con sus adjuntos (HU-033, HU-034, HU-035) → [[criterios-de-aceptacion]]
- PRD CA-12 — Cierre solo manual, sin cierre automático ni recordatorios (HU-036) → [[criterios-de-aceptacion]]
- PRD CA-09 (parcial) — Tipos y tamaño de los adjuntos de la respuesta formal (HU-033) → [[criterios-de-aceptacion]]
- PRD CA-01 y CA-02 (parcial) — La respuesta formal en el portal respeta el aislamiento por empresa y por rol (HU-034) → [[criterios-de-aceptacion]]
- PRD CA-10 (parcial) — La respuesta y su correo no exponen nada interno (HU-034, HU-035) → [[criterios-de-aceptacion]]
- PRD CA-14 (parcial) — El cierre manual conserva actor y fecha/hora en la auditoría (HU-036) → [[criterios-de-aceptacion]]

## Dependencias

- [[ep-002-identidad-y-acceso]]: roles, `ICurrentUser` y adaptador de correo (`IEmailSender`, `App:PublicBaseUrl`) de [[hu-005-invitar-y-activar-cuentas]].
- [[ep-004-auditoria-append-only]]: `IAuditLog` ([[hu-011-registrar-eventos-auditables]]).
- [[ep-005-radicacion-de-tickets]]: carga y validación de adjuntos, `IFileStorage` ([[hu-014-adjuntos-en-radicacion]]).
- [[ep-006-triage-asignacion-y-participantes]]: asociación del administrador ([[hu-017-cola-de-cobertura-y-tomar-ticket]], [[hu-018-asignar-y-agregar-participantes]]).
- [[ep-007-trabajo-interno-y-estados]]: detalle interno y estados ([[hu-020-detalle-interno-del-ticket]], [[hu-021-cambiar-estado-interno]]).
- [[ep-008-bandejas-y-portal-del-cliente]]: detalle del portal ([[hu-024-portal-solicitante-consulta-tickets]], [[hu-025-portal-coordinador-consulta-tickets]]).
- Decisiones abiertas: V-01 (transiciones), V-07 (destinatarios del correo), V-13 (varias respuestas), máximo de archivos por solicitud y V-06 (MIME/firma), proveedor de correo productivo (PRD §16.2) → [[pendientes]].

## Historias de usuario

- [[hu-033-emitir-respuesta-formal|HU-033 — Emitir respuesta formal]]
- [[hu-034-portal-respuesta-formal|HU-034 — Consultar la respuesta formal en el portal]]
- [[hu-035-correo-de-respuesta-formal|HU-035 — Enviar la respuesta formal por correo]]
- [[hu-036-cerrar-ticket-manualmente|HU-036 — Cerrar ticket manualmente]]

## Criterio de completitud

- [ ] HU-033, HU-034, HU-035 y HU-036 están `Completada`, con su matriz de evidencia registrada.
- [ ] PRD CA-11 y CA-12 tienen pruebas automáticas que los demuestran (unitarias por rol + integración con `WebApplicationFactory<Program>` y PostgreSQL real).
- [ ] La no exposición al cliente (CA-10) está probada sobre el JSON del portal y sobre el correo generado.
- [ ] Hay evidencia en Mailpit (`http://localhost:8025`) de un correo de respuesta formal con texto completo y archivos.
- [ ] Se demostró, por prueba o inspección registrada, que no existe ningún proceso programado que cierre tickets.
- [ ] V-01, V-07 y V-13 están resueltas o las HU documentan explícitamente el comportamiento provisional aceptado por la persona usuaria.
- [ ] No quedan dependencias bloqueantes dentro del alcance.

## Riesgos e incógnitas

- **V-13 sin resolver:** si se permiten varias respuestas o correcciones, cambia el modelo (`Ticket` 1→0..1 `FormalResponse`) y la UI del portal. Mientras tanto, una segunda emisión se rechaza con 409 (propuesta).
- **V-07 sin resolver:** el destinatario del correo (solicitante, coordinador o ambos) condiciona HU-035.
- **V-01 sin resolver:** desde qué estados se puede emitir la respuesta y si `Closed` exige `SolutionDelivered`.
- **Proveedor de correo productivo pendiente** (PRD §16.2): límites de tamaño del correo pueden impedir adjuntar archivos; la HU-035 propone un mecanismo alternativo con enlaces.
- **URL públicas del Blob** ([[adr-0006-urls-publicas-azure-blob]]): los archivos de la respuesta formal quedan accesibles a quien tenga el enlace, también fuera del portal; reenviar el correo los expone.
- **Adjuntos sin contexto en el modelo:** `Attachment` no distingue radicación, chat y respuesta formal; sin esa marca el portal podría mostrar adjuntos internos (riesgo para CA-10).
- **Administrador no asociado:** el PRD no dice si debe ser participante para emitir o cerrar; se propone exigirlo por coherencia con PRD §5.5.

## Relacionado

- [[tablero-scrum]] · [[fases-y-alcance]] · [[flujo-del-ticket]] · [[estados-del-ticket]] · [[notificaciones]] · [[archivos-adjuntos]] · [[auditoria]] · [[roles-y-permisos]] · [[criterios-de-aceptacion]] · [[adr-0006-urls-publicas-azure-blob]] · [[ep-011-notificaciones]]

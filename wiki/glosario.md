---
title: Glosario
type: concepto
status: vigente
tags: [dominio, glosario]
sources: ["PRD.md §5", "PRD.md §6", "PRD.md §7", "PRD.md §8", "PRD.md §9", "PRD.md §10", "PRD.md §12"]
aliases: [Lenguaje ubicuo, Diccionario de términos]
created: 2026-10-07
updated: 2026-10-07
---

# Glosario

Lenguaje ubicuo de DataTicket: cada término de negocio en español con su nombre en código (en inglés). Usa **exactamente** estos nombres en backend, frontend, base de datos y pruebas; si uno cambia, actualiza esta página y [[modelo-de-dominio]].

## Términos

| Término (negocio) | Nombre en código | Definición | Página |
|---|---|---|---|
| Ticket | `Ticket` | Solicitud de soporte radicada por un cliente; pertenece a una empresa (PRD §6, §12) | [[flujo-del-ticket]] |
| Empresa cliente | `Company` | Organización cliente de Data Global; delimita qué tickets ve cada usuario cliente (PRD §5, §12) | [[roles-y-permisos]] |
| Usuario | `User` (Identity: `ApplicationUser`) | Persona con cuenta en DataTicket, cliente o interna (PRD §5) | [[autenticacion-identity]] |
| Solicitante | `Requester` | Usuario cliente que radica y consulta sus propios tickets (PRD §5) | [[roles-y-permisos]] |
| Coordinador de empresa | `CompanyCoordinator` | Usuario cliente que consulta los tickets de toda su empresa y puede radicar (PRD §5) | [[roles-y-permisos]] |
| PM | rol `ProductManager` | Responsable de triage, asignación, respuesta formal y cierre; hoy Elizabeth (PRD §5) | [[elizabeth-pm]] |
| Administrador | `Administrator` | Administra el sistema y participantes; cubre triage; sin acceso automático al contenido (PRD §5) | [[roles-y-permisos]] |
| Equipo Desarrollo / Producción | `Team.Development` / `Team.Production` | Equipos internos; pertenecer no da acceso al ticket sin asociación (PRD §5, §6.2) | [[equipo-data-global]] |
| Radicación | `SubmitTicket` (caso de uso) | Creación de un ticket por un usuario cliente con el formulario común (PRD §6.1) | [[flujo-del-ticket]] |
| Triage | `Triage` | Revisión de categoría, urgencia e impacto y asignación inicial (PRD §6.2) | [[flujo-del-ticket]] |
| Cola de triage | `TriageQueue` | Tickets pendientes de triage; vista limitada para el admin de cobertura (PRD §5, §6.2) | [[roles-y-permisos]] |
| Asignación | `Assignment` | Designación de una o varias personas responsables de un ticket (PRD §6.2) | [[flujo-del-ticket]] |
| Participante | `TicketParticipant` | Persona interna asociada explícitamente a un ticket; da acceso a detalle y chat (PRD §5, §6.2) | [[chat-interno]] |
| Categoría | `Category` | Clasificación del ticket elegida al radicar (PRD §6.1) | [[formularios-configurables]] |
| Urgencia | `Urgency` (`Low`, `Medium`, `High`) | Nivel de urgencia que indica el cliente al radicar: baja, media o alta (PRD §6.1) | [[matriz-de-prioridad]] |
| Impacto | `Impact` (`Low`, `Medium`, `High`) | Nivel de impacto que indica el cliente al radicar: bajo, medio o alto (PRD §6.1) | [[matriz-de-prioridad]] |
| Prioridad calculada | `CalculatedPriority` (`VeryLow`, `Low`, `Medium`, `High`, `Critical`) | Resultado de la matriz urgencia × impacto (PRD §6.1) | [[matriz-de-prioridad]] |
| Ajuste manual de prioridad | `PriorityOverride` | Cambio manual con prioridad original, nueva, autor, fecha y motivo (PRD §6.1) | [[matriz-de-prioridad]] |
| Estado interno | `TicketStatus` (`New`, `InDevelopment`, `PullRequestReview`, `InProduction`, `SolutionDelivered`, `Closed`) | Etapa real del trabajo; solo visible para internos (PRD §6.4) | [[estados-del-ticket]] |
| Estado resumido del cliente | `ClientStatus` (`Received`, `InProgress`, `SolutionDelivered`, `Closed`) | Estado seguro que ve el cliente en el portal (PRD §6.4) | [[estados-del-ticket]] |
| Mensaje de chat | `ChatMessage` | Mensaje del chat interno de un ticket, persistido antes de publicarse (PRD §7) | [[chat-interno]] |
| Confirmación de lectura | `MessageReadReceipt` | Registro de qué persona leyó qué mensaje y cuándo (PRD §7) | [[chat-interno]] |
| Adjunto | `Attachment` | Archivo (PDF, imagen, XML, Excel ≤ 10 MB) de radicación, chat o respuesta formal (PRD §8) | [[archivos-adjuntos]] |
| Respuesta formal | `FormalResponse` | Comunicación final al cliente con texto y archivos; solo PM o admin (PRD §6.5) | [[flujo-del-ticket]] |
| Cierre manual | `CloseTicket` (caso de uso) | Cierre registrado por PM o admin tras confirmación telefónica (PRD §6.5) | [[flujo-del-ticket]] |
| Bitácora de auditoría | `AuditEntry` | Registro append-only de actor, fecha, acción, objeto, valores y resultado (PRD §12) | [[auditoria]] |
| URL de pull request | `PullRequestUrl` | Enlace al PR registrado a mano; solo para internos (PRD §6.3) | [[flujo-del-ticket]] |
| Bandeja | `Inbox` | Lista de trabajo de la PM, de un equipo o del portal (PRD §10) | [[dashboard-y-metricas]] |
| Plantilla de formulario | `FormTemplate` | Formulario versionado por empresa cliente; Fase 4 (PRD §9) | [[formularios-configurables]] |

## Correspondencia de valores

| Negocio | Código |
|---|---|
| Urgencia baja / media / alta | `Urgency.Low` / `Urgency.Medium` / `Urgency.High` |
| Impacto bajo / medio / alto | `Impact.Low` / `Impact.Medium` / `Impact.High` |
| Prioridad muy baja / baja / media / alta / crítica | `VeryLow` / `Low` / `Medium` / `High` / `Critical` |
| Nuevo · En desarrollo · Revisión de PR · En producción · Solución entregada · Cerrado | `New` · `InDevelopment` · `PullRequestReview` · `InProduction` · `SolutionDelivered` · `Closed` |
| Recibido · En atención · Solución entregada · Cerrado | `Received` · `InProgress` · `SolutionDelivered` · `Closed` |

## Reglas de uso

- Español en la wiki y en la interfaz; identificadores de código en inglés según esta tabla (`AGENTS.md` §5.3).
- Un término nuevo entra aquí **antes** de usarse en código, junto con su página de producto o dominio.
- `ApplicationUser` es el tipo de ASP.NET Core Identity en infraestructura; el dominio habla de `User`. Ver [[autenticacion-identity]] y [[backend-hexagonal]].

## Relacionado

- [[modelo-de-dominio]] · [[vision-general]] · [[flujo-del-ticket]] · [[estados-del-ticket]]
- [[roles-y-permisos]] · [[chat-interno]] · [[auditoria]] · [[fuente-prd-v0-1]]

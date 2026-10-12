---
title: Glosario
type: concepto
status: vigente
tags: [dominio, glosario]
sources: ["PRD.md §5", "PRD.md §6", "PRD.md §7", "PRD.md §8", "PRD.md §9", "PRD.md §10", "PRD.md §12", "backend/src (nombres implementados en B3 del goal del login)"]
aliases: [Lenguaje ubicuo, Diccionario de términos]
created: 2026-10-07
updated: 2026-10-12
---

# Glosario

Lenguaje ubicuo de DataTicket: cada término de negocio en español con su nombre en código (en inglés). Usa **exactamente** estos nombres en backend, frontend, base de datos y pruebas; si uno cambia, actualiza esta página y [[modelo-de-dominio]].

## Términos

| Término (negocio) | Nombre en código | Definición | Página |
|---|---|---|---|
| Ticket | `Ticket` | Solicitud de soporte radicada por un cliente; pertenece a una empresa (PRD §6, §12) | [[flujo-del-ticket]] |
| Empresa cliente | `Company` (entidad en `Domain/Companies`; tabla `dataticket.companies`) | Organización cliente de Data Global; delimita qué tickets ve cada usuario cliente (PRD §5, §12). Se desactiva, no se borra | [[roles-y-permisos]] |
| Usuario | `User` (Identity: `ApplicationUser`, en `Infrastructure/Identity`; tabla `identity.users`) | Persona con cuenta en DataTicket, cliente o interna (PRD §5). `ApplicationUser` añade `DisplayName`, `AccountStatus`, `CompanyId` y `CreatedAt` a la cuenta de Identity | [[autenticacion-identity]] |
| Solicitante | `Requester` | Usuario cliente que radica y consulta sus propios tickets (PRD §5) | [[roles-y-permisos]] |
| Coordinador de empresa | `CompanyCoordinator` | Usuario cliente que consulta los tickets de toda su empresa y puede radicar (PRD §5) | [[roles-y-permisos]] |
| PM | rol `ProductManager` | Responsable de triage, asignación, respuesta formal y cierre; hoy Elizabeth (PRD §5) | [[elizabeth-pm]] |
| Administrador | `Administrator` | Administra el sistema y participantes; cubre triage; sin acceso automático al contenido (PRD §5) | [[roles-y-permisos]] |
| Equipo Desarrollo / Producción | `Team.Development` / `Team.Production` (implementados como roles de Identity `Development` / `Production`, ver [[autenticacion-identity]]) | Equipos internos; pertenecer no da acceso al ticket sin asociación (PRD §5, §6.2) | [[equipo-data-global]] |
| Estado de la cuenta | `AccountStatus` (`Invited`, `Active`, `Deactivated`) | Invitada sin contraseña, activa o desactivada por Data Global; solo `Active` inicia sesión (PRD §5) | [[autenticacion-identity]] |
| Superficie | `Surface` (`client`, `internal`) | Portal del cliente o Espacio interno al que entra el usuario; campo `surface` de `/api/auth/me` | [[autenticacion-identity]] |
| Usuario actual | `ICurrentUser` (puerto backend) / `CurrentUser` (frontend) | Identidad, roles, equipos y empresa (solo cliente) de la sesión; la empresa sale del claim `company_id`, nunca de la petición (PRD §5) | [[autenticacion-identity]] |
| Claim de empresa | `company_id` | Claim de la cookie con la empresa del usuario cliente | [[autenticacion-identity]] |
| Acceso por superficie (frontend) | `SurfaceAccess` (`client`, `internal`, `none`) | `surface` normalizado de `/me`; un valor desconocido es `none` (sin acceso) | [[frontend-mvc]] |
| Navegación por rol (frontend) | `navigationFor`, `canAccessSection`, `sanitizeReturnUrl`, `signInPathFor` | Funciones puras del módulo `session`: ayuda de navegación, nunca la barrera de seguridad | [[frontend-mvc]] |
| Error HTTP tipado (frontend) | `HttpError` (`status`, `problem?`) | Error del cliente HTTP; `problem` es el ProblemDetails (RFC 9457) si la respuesta lo trae | [[frontend-mvc]] |
| Políticas de autorización | `ClientUser`, `InternalUser`, `Administrator`, `ProductManagerOrAdministrator` | Políticas con nombre del backend; la de reserva exige usuario autenticado | [[autenticacion-identity]] |
| Nombres de rol | `RoleNames` (`ProductManager`, `Administrator`, `Development`, `Production`, `Requester`, `CompanyCoordinator`; grupos `Client`, `Internal`, `All`) | Constantes de dominio con los seis roles de Identity. `Client` agrupa los roles de cliente e `Internal` los internos, incluidos los equipos | [[autenticacion-identity]] |
| Acceso de la cuenta | `AccountAccess` (`ResolveSurface`, `CanSignIn`) | Regla de dominio de los perfiles válidos: resuelve la superficie de un perfil o falla cerrado; solo una cuenta `Active` con perfil válido puede tener sesión (PRD §5) | [[autenticacion-identity]] |
| Iniciar sesión | `SignIn` (caso de uso; puerto `ISignIn`, `SignInCommand`, `SignInOutcome`) | Verifica correo y contraseña y emite la sesión solo si la cuenta puede tenerla; todo fallo devuelve `InvalidCredentials`, sin el motivo (PRD §5) | [[autenticacion-identity]] |
| Cerrar sesión | `SignOut` (caso de uso; puerto `ISignOut`) | Cierra la sesión del navegador actual; no revoca otras copias de la cookie | [[autenticacion-identity]] |
| Verificador de credenciales | `ICredentialVerifier` (puerto de salida) | Comprueba correo y contraseña contra el almacén de cuentas aplicando el bloqueo por intentos fallidos; no emite sesión ni revela por qué falló | [[autenticacion-identity]] |
| Cuenta verificada | `VerifiedAccount` (`UserId`, `Status`, `CompanyId`, `Roles`) | Datos que devuelve `ICredentialVerifier` para decidir si la cuenta puede tener sesión | [[autenticacion-identity]] |
| Hash señuelo | `DecoyPasswordHash` (Infrastructure) | Hash generado con el `IPasswordHasher` de la aplicación contra el que se verifica la clave cuando el correo no existe, no tiene contraseña o está bloqueado, para igualar el coste de todo intento de login | [[autenticacion-identity]] |
| Sesión del usuario | `IUserSession` (puerto de salida; `StartAsync`, `EndAsync`) | Emite y cierra la sesión en el canal actual; en el adaptador HTTP es la cookie de Identity | [[autenticacion-identity]] |
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

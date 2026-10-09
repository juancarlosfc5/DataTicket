---
title: "HU-005 — Invitar y activar cuentas"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, seguridad, identity, producto/notificaciones, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §12", "PRD.md §16"]
aliases: ["HU-005", "Invitar y activar cuentas"]
epica: "[[ep-002-identidad-y-acceso]]"
criterios_prd: []
componentes: ["Identity", "Backend (Application: InviteUser, AcceptInvitation)", "Backend (Infrastructure: Email SMTP, Identity)", "Backend (Api)", "Docker/Compose (App:PublicBaseUrl)", "Frontend (models, controllers, views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 1"
dependencias: ["[[hu-003-iniciar-y-cerrar-sesion]]", "[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-002-shell-y-navegacion-por-rol]]"]
relacionadas: ["[[hu-006-restablecer-contrasena]]", "[[hu-008-administrar-usuarios-cliente]]", "[[hu-009-administrar-usuarios-internos-y-equipos]]", "[[hu-038-correo-de-vinculacion]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-005 — Invitar y activar cuentas

Data Global invita a las personas por correo: el administrador envía (o reenvía) una invitación con un enlace de un solo uso y con expiración, construido con la URL base configurada; la persona invitada fija su propia contraseña según la política de Identity y su cuenta queda activa. No existe registro público.

## Historia de usuario

**COMO** administrador de DataTicket  
**QUIERO** enviar a una cuenta recién creada una invitación por correo con un enlace seguro para que la persona fije su contraseña  
**PARA** dar acceso solo a quien Data Global autoriza, sin conocer ni transmitir contraseñas y sin abrir un registro público

**COMO** persona invitada (cliente o interna)  
**QUIERO** abrir el enlace de la invitación y definir mi contraseña  
**PARA** activar mi cuenta y empezar a usar DataTicket

## Contexto

PRD §5: «Las cuentas de empresas cliente las crea, invita y desactiva Data Global. No hay registro público»; la implementación productiva debe usar «un flujo seguro de invitación y restablecimiento, sin revelar si un correo está registrado». [[autenticacion-identity]] prevé `POST /api/auth/invitations` y `POST /api/auth/invitations/accept`, envío por el puerto `IEmailSender` y **enlaces construidos con `App:PublicBaseUrl`, nunca con el `Host` de la petición** (un `Host` falsificado envenenaría los enlaces). En local, el correo llega a Mailpit (`http://localhost:8025`); el proveedor productivo está pendiente (PRD §16.2).

Reparto con otras HU: esta HU aporta el **mecanismo** (token, correo, aceptación, reenvío). La creación de la cuenta con su perfil y empresa, que dispara la primera invitación, está en [[hu-008-administrar-usuarios-cliente|HU-008]] (clientes) y [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]] (internos).

## Alcance

- Caso de uso `InviteUser` (nombre propuesto): genera un token de invitación de un solo uso y con expiración para una cuenta en estado `Invited` y envía el correo; reenviar invalida el token anterior (propuesta).
- Caso de uso `AcceptInvitation` (nombre propuesto): valida el token, aplica la política de contraseñas, fija la contraseña y pasa la cuenta a `Active`.
- Adaptador SMTP del puerto `IEmailSender` en `Infrastructure/Email` con `Email:Smtp:Host`, `Email:Smtp:Port` y `Email:FromAddress` (ya inyectados por Compose).
- Configuración tipada `App:PublicBaseUrl` (nueva clave; en Compose `http://localhost:5173`) y vida útil del token (propuesta: `App:InvitationTokenLifetime`, valor a ratificar en T-01).
- Endpoints `POST /api/auth/invitations` (administrador) y `POST /api/auth/invitations/accept` (público).
- Frontend: página pública `/activar-cuenta` y gateway de reenvío que reutilizan las vistas de administración de HU-008/HU-009.

## Fuera de alcance

- Crear la cuenta, su perfil y su empresa ([[hu-008-administrar-usuarios-cliente|HU-008]], [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]]).
- Restablecimiento de contraseña ([[hu-006-restablecer-contrasena|HU-006]]).
- Proveedor de correo productivo, dominio remitente y entregabilidad (PRD §16.2).
- Inicio de sesión automático tras activar (propuesta: no; la persona entra por `/ingresar`).
- Correo de vinculación a un ticket ([[hu-038-correo-de-vinculacion|HU-038]]).

## Requisitos y reglas de negocio

- Sin registro público; Data Global crea e invita las cuentas (PRD §5).
- Flujo de invitación seguro que no revela si un correo está registrado (PRD §5).
- Contraseñas con solución estándar de identidad y derivación segura (PRD §12).
- Token de un solo uso (PRD §12, por analogía con la recuperación; propuesta explícita para invitaciones).
- Los datos de prueba son sintéticos (PRD §12): los correos de prueba usan dominios `.test`/`.local`.

## Invariantes en juego

- Invariante 9 (`AGENTS.md` §7): no hay registro público; la aceptación de invitaciones no permite deducir si una cuenta existe.
- Regla de arquitectura: `IEmailSender` es un puerto de `Application/Ports/Out`; el adaptador SMTP está en `Infrastructure`. No confundir con `Microsoft.AspNetCore.Identity.IEmailSender<TUser>`.

## Criterios del PRD cubiertos

- Ninguno de forma directa. Implementa el requisito de invitación segura de PRD §5 y §12 → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-002-identidad-y-acceso]]
- Dependencias: [[hu-003-iniciar-y-cerrar-sesion|HU-003]] (Identity, estado de la cuenta, política de contraseñas, antiforgery), [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]] (política `Administrator`, lista de públicos), [[hu-002-shell-y-navegacion-por-rol|HU-002]] (ruta pública `/activar-cuenta`); aceptación de [[adr-0004-autenticacion-cookie-mismo-origen]].
- Relacionadas: [[hu-006-restablecer-contrasena|HU-006]] (comparte adaptador de correo y patrón de token), [[hu-008-administrar-usuarios-cliente|HU-008]], [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]], [[hu-038-correo-de-vinculacion|HU-038]].
- Decisiones: [[adr-0004-autenticacion-cookie-mismo-origen]] (propuesta). Pendiente: quién puede invitar (ver Notas).

## Componentes afectados

- Backend `Application`: casos de uso `InviteUser`, `AcceptInvitation`; puertos `IEmailSender`, `IClock` y puerto de cuentas (propuesto: `IUserAccountService`, ratificar en T-01 y registrar en el [[glosario]]).
- Backend `Infrastructure`: `Email/` (SMTP), `Identity/` (proveedor de tokens con propósito propio de invitación), opciones tipadas.
- Backend `Api`: endpoints y validación.
- Docker/Compose: variable `App__PublicBaseUrl` del servicio `backend` y su documentación en `.env.example` si se hace sobrescribible.
- Frontend: módulo `auth` (página de activación) y gateway de reenvío usado por los módulos de administración.

## Dificultad

**Nivel:** Alto

**Justificación:** Cruza backend (tokens de Identity, primer adaptador de correo, configuración nueva), Compose y frontend, con reglas de seguridad finas (un solo uso, expiración, envenenamiento de `Host`, no enumeración). Es el primer flujo que envía correo.

## Contrato backend ↔ frontend

Propuesta REST; ratificar en T-01. Errores `application/problem+json`. Ambos `POST` exigen cabecera antiforgery.

**`POST /api/auth/invitations`** — política `Administrator`

```json
// Petición
{ "userId": "6f1c2a9e-4c1d-4b8e-9a51-1f0e2d3c4b5a" }
// 202
{ "userId": "6f1c2a9e-4c1d-4b8e-9a51-1f0e2d3c4b5a", "sentAt": "2026-10-07T15:04:05Z" }
```

| Respuesta | Cuándo |
|---|---|
| `202` | Cuenta en estado `Invited`: se genera token nuevo (el anterior deja de valer) y se encola el correo |
| `400` | `userId` ausente o con formato inválido |
| `401` / `403` | Sin sesión / sin rol `Administrator` |
| `404` | No existe la cuenta (endpoint administrativo: no aplica la regla de no enumeración) |
| `409` | La cuenta está `Active` o `Deactivated` |

**`POST /api/auth/invitations/accept`** — público

```json
// Petición (uid y token salen del enlace del correo)
{ "userId": "6f1c2a9e-4c1d-4b8e-9a51-1f0e2d3c4b5a", "token": "CfDJ8…", "password": "Clave-Sintetica-2026" }
```

| Respuesta | Cuándo | Cuerpo |
|---|---|---|
| `204` | Token válido y contraseña conforme a la política | — |
| `400` | La contraseña no cumple la política (se valida antes del token) | `{ "title": "Uno o más campos no son válidos.", "errors": { "password": ["La contraseña debe tener al menos 12 caracteres."] } }` |
| `400` | Token inválido, expirado o ya usado; `userId` inexistente; cuenta no `Invited` | `{ "title": "El enlace de invitación no es válido o expiró.", "status": 400 }` (idéntico en todos estos casos salvo `traceId`) |

**Enlace del correo** (propuesta): `{App:PublicBaseUrl}/activar-cuenta?uid={userId}&token={token codificado para URL}`.

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: ratificar rutas, cuerpos, códigos, ruta del SPA del enlace, vida útil del token, si reenviar invalida el token previo, quién puede invitar (`Administrator`; ¿también `ProductManager`?), plantilla del correo (asunto, texto en español, sin contraseña) y el puerto de cuentas.
- [ ] **T-02 — Casos de uso (pruebas primero)** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: pruebas unitarias con dobles de `IEmailSender`, del puerto de cuentas e `IClock`: `InviteUser` rechaza cuentas no `Invited` (409) y llama al enviador una vez con el enlace construido desde la URL base configurada; `AcceptInvitation` no consume el token si la contraseña no cumple la política y devuelve el mismo error para token inválido, expirado, usado, cuenta inexistente o no `Invited`.
- [ ] **T-03 — Adaptador SMTP y opciones** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: implementar `IEmailSender` sobre SMTP con `Email:Smtp:*` y `Email:FromAddress` (patrón Options, validación al arrancar); si se usa un paquete, versionarlo en `Directory.Packages.props`. Prueba de integración contra Mailpit en Compose o contra un servidor SMTP de prueba.
- [ ] **T-04 — Tokens de invitación** · Capa: Backend (Infrastructure/Identity) · Dificultad: Medio  
  Descripción: proveedor de tokens con propósito propio de invitación (un token de restablecimiento no sirve como invitación y viceversa), vida útil configurable y reloj controlable en pruebas (`TimeProvider`); el sello de seguridad cambia al aceptar y al reenviar.
- [ ] **T-05 — Endpoints** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: pruebas de integración primero (CHU-01 a CHU-09); `POST /api/auth/invitations` con política `Administrator` y `POST /api/auth/invitations/accept` con `AllowAnonymous` incluido en la lista pública de HU-004; antiforgery en ambos.
- [ ] **T-06 — Configuración y Compose** · Capa: Docker/Compose · Dificultad: Bajo  
  Descripción: clave `App:PublicBaseUrl` con validación (URL absoluta) y valor local `http://localhost:5173` en el servicio `backend` de `docker-compose.yml`; documentarla en [[entorno-docker]].
- [ ] **T-07 — Frontend: modelo y gateway** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: pruebas Vitest primero: lectura de `uid` y `token` de la URL, validación local (contraseña y confirmación iguales; longitud mínima según T-01), `invitationGateway.accept()` y `invitationGateway.resend(userId)` sobre `postJson`.
- [ ] **T-08 — Frontend: controlador y vista** · Capa: Frontend (controllers, views) · Dificultad: Medio  
  Descripción: `useAcceptInvitationController` (estados `idle | submitting | invalidLink | error | done`) y `AcceptInvitationView` pura (contraseña, confirmación, errores por campo, mensaje genérico de enlace inválido); al terminar navega a `/ingresar` con el aviso «Tu cuenta está activa. Ingresa con tu correo y contraseña.». Pruebas con Testing Library.
- [ ] **T-09 — Notas para la wiki** · Capa: Documentación · Dificultad: Bajo  
  Descripción: [[autenticacion-identity]] (flujo implementado), [[entorno-docker]] (`App:PublicBaseUrl`), [[backend-hexagonal]] (puerto `IEmailSender` y adaptador SMTP), [[glosario]] (estado de la cuenta, casos de uso).

## Criterios de aceptación

### CHU-01 — El administrador envía la invitación

**Dado** una cuenta `ana.perez@empresa-sintetica-01.test` en estado `Invited` y un usuario con rol `Administrator`  
**Cuando** el administrador envía `POST /api/auth/invitations` con su `userId`  
**Entonces** recibe `202`; el doble de `IEmailSender` (o Mailpit en Compose) registra exactamente un correo a esa dirección, en español, sin ninguna contraseña, con un enlace `http://localhost:5173/activar-cuenta?uid=<userId>&token=<token>`.

### CHU-02 — El enlace no depende del `Host` de la petición

**Dado** `App:PublicBaseUrl = https://dataticket.example` en la configuración de la prueba  
**Cuando** la petición de invitación llega con `Host: evil.example` y `X-Forwarded-Host: evil.example`  
**Entonces** el enlace del correo empieza por `https://dataticket.example/activar-cuenta` y no contiene `evil.example`.

### CHU-03 — Activación correcta

**Dado** un enlace de invitación vigente  
**Cuando** la persona envía `POST /api/auth/invitations/accept` con `uid`, `token` y la contraseña `Clave-Sintetica-2026`  
**Entonces** recibe `204`, la cuenta pasa a `Active` y puede iniciar sesión con esa contraseña ([[hu-003-iniciar-y-cerrar-sesion|HU-003]]).

### CHU-04 — El token es de un solo uso

**Dado** un token ya usado para activar la cuenta  
**Cuando** se vuelve a enviar `POST /api/auth/invitations/accept` con el mismo token y otra contraseña  
**Entonces** responde `400` «El enlace de invitación no es válido o expiró.» y la contraseña sigue siendo la fijada en la primera activación.

### CHU-05 — El token expira

**Dado** un token emitido con la vida útil configurada  
**Cuando** se acepta con el reloj de pruebas avanzado un instante después de esa vida útil  
**Entonces** responde `400` con el mensaje genérico de CHU-04 y la cuenta sigue `Invited`; con el reloj un instante antes del límite, la misma aceptación responde `204`.

### CHU-06 — Política de contraseñas con valores límite

**Dado** un token vigente y la política ratificada en T-01 de HU-003 (propuesta: longitud mínima 12)  
**Cuando** se acepta con `Clave-Sint-1` (11 caracteres) y después con `Clave-Sint-12` (12 caracteres)  
**Entonces** el primer intento responde `400` con `errors.password` y **no consume** el token; el segundo responde `204`.

### CHU-07 — Reenviar invalida la invitación anterior

**Dado** una cuenta `Invited` con un primer token T1  
**Cuando** el administrador reenvía la invitación (token T2) y luego alguien intenta aceptar con T1  
**Entonces** T1 responde `400` genérico y T2 responde `204`.

### CHU-08 — Solo el administrador invita

**Dado** usuarios con rol `Requester`, `CompanyCoordinator`, un miembro de equipo Desarrollo y una petición sin sesión  
**Cuando** envían `POST /api/auth/invitations`  
**Entonces** los autenticados reciben `403`, la anónima `401`, y ninguno genera correo; si la cuenta destino ya está `Active` o `Deactivated`, el administrador recibe `409` y tampoco se envía correo.

### CHU-09 — La aceptación no revela cuentas

**Dado** un `userId` inexistente, un `userId` de una cuenta `Deactivated` y un token manipulado para una cuenta `Invited`  
**Cuando** se envía `POST /api/auth/invitations/accept` con cada uno y una contraseña válida  
**Entonces** las tres respuestas son `400` con el mismo `title` y los mismos campos (comparando el JSON sin `traceId`).

### CHU-10 — La página de activación guía a la persona

**Dado** la página `/activar-cuenta?uid=…&token=…`  
**Cuando** la persona escribe contraseñas distintas en «Contraseña» y «Confirmar contraseña»  
**Entonces** la vista muestra «Las contraseñas no coinciden.» sin enviar ninguna petición; ante un `400` de enlace inválido muestra «El enlace de invitación no es válido o expiró. Pide al administrador una nueva invitación.»; y ante `204` navega a `/ingresar` con el aviso de cuenta activa.

### CHU-11 — El correo llega a Mailpit en el entorno local

**Dado** el stack levantado con `docker compose up --build`  
**Cuando** se envía una invitación desde el backend  
**Entonces** el correo aparece en `http://localhost:8025` con remitente `no-reply@dataticket.local` y el enlace abre la página de activación en `http://localhost:5173`.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-11 validados con evidencia.
- [ ] **DoD-02** — Pruebas escritas primero y en verde, con nombres por comportamiento: `Invitar_envia_un_correo_con_enlace_desde_PublicBaseUrl`, `Enlace_ignora_Host_y_X_Forwarded_Host`, `Aceptar_activa_la_cuenta`, `Token_de_invitacion_es_de_un_solo_uso`, `Token_de_invitacion_expira`, `Contrasena_de_11_caracteres_no_consume_el_token`, `Reenviar_invalida_el_token_anterior`, `Solo_Administrator_invita`, `Aceptar_responde_igual_para_cuentas_inexistentes_o_desactivadas`.
- [ ] **DoD-03** — Pruebas Vitest de la página de activación (modelo, controlador, vista) en verde.
- [ ] **DoD-04** — `cd backend && dotnet test` en verde, incluidas `DataTicket.ArchitectureTests`.
- [ ] **DoD-05** — `npm --prefix frontend run lint`, `npm --prefix frontend test` y `npm --prefix frontend run build` en verde.
- [ ] **DoD-06** — Migración EF Core solo si T-01 añade columnas (p. ej. fecha de última invitación); si no, se documenta que no hubo cambio de esquema.
- [ ] **DoD-07** — `docker compose up --build` levanta con `App__PublicBaseUrl` configurada; CHU-11 verificado manualmente con captura de Mailpit en el PR.
- [ ] **DoD-08** — Wiki actualizada vía Notas para la wiki: [[autenticacion-identity]], [[entorno-docker]], [[backend-hexagonal]], [[glosario]].
- [ ] **DoD-09** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos (envenenamiento de enlaces, reutilización de tokens, enumeración).
- [ ] **DoD-10** — PR revisado y aprobado por otra persona del equipo; trazabilidad de HU y épica actualizada.

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
| CHU-10 | Pendiente | — | — |
| CHU-11 | Pendiente | — | — |
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

- **Quién invita**: [[autenticacion-identity]] dice «PM/administrador»; esta HU propone solo `Administrator` (PRD §5: el administrador «administra el sistema»). Ratificar en T-01 y registrar en [[pendientes]].
- **Auditoría**: el PRD §12 no lista invitaciones como evento auditable. Registrar `UserInvited`/`InvitationAccepted` (nombres propuestos) depende de [[hu-011-registrar-eventos-auditables|HU-011]] y es propuesta (V-11).
- **Fuga del token por la URL**: la página de activación debería retirar `token` de la barra de direcciones tras leerlo y no cargar recursos de terceros; nginx ya envía `Referrer-Policy` ([[entorno-docker]]). Propuesta.
- **Limitación de peticiones** en `/invitations/accept`: propuesta compartida con HU-006.
- El envío del correo puede desacoplarse de la respuesta HTTP (mismo mecanismo que HU-006).

## Relacionado

- [[ep-002-identidad-y-acceso]] · [[tablero-scrum]] · [[autenticacion-identity]] · [[roles-y-permisos]] · [[entorno-docker]] · [[notificaciones]] · [[backend-hexagonal]] · [[adr-0004-autenticacion-cookie-mismo-origen]]

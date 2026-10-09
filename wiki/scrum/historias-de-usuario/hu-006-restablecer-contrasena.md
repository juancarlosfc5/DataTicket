---
title: "HU-006 — Restablecer contraseña sin enumeración"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, seguridad, identity, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §12"]
aliases: ["HU-006", "Restablecer contraseña sin enumeración", "Restablecer contraseña"]
epica: "[[ep-002-identidad-y-acceso]]"
criterios_prd: []
componentes: ["Identity", "Backend (Application: RequestPasswordReset, ResetPassword)", "Backend (Infrastructure: Email, Identity)", "Backend (Api)", "Frontend (models, controllers, views)"]
dificultad: "Medio"
sprint_sugerido: "Sprint 1"
dependencias: ["[[hu-003-iniciar-y-cerrar-sesion]]", "[[hu-005-invitar-y-activar-cuentas]]"]
relacionadas: ["[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-002-shell-y-navegacion-por-rol]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-006 — Restablecer contraseña sin enumeración

Una persona que olvidó su contraseña pide un enlace de restablecimiento indicando su correo; la respuesta es la misma (y tarda lo mismo) exista o no la cuenta. El enlace lleva un token de un solo uso con expiración; al restablecer, las demás sesiones abiertas de esa cuenta dejan de valer (propuesta).

## Historia de usuario

**COMO** usuario de DataTicket que olvidó su contraseña  
**QUIERO** pedir un enlace de restablecimiento a mi correo y definir una contraseña nueva  
**PARA** recuperar el acceso sin intervención de Data Global y sin que nadie pueda averiguar si un correo tiene cuenta

## Contexto

PRD §5 exige un flujo seguro de restablecimiento «sin revelar si un correo está registrado»; PRD §12 pide «token de un solo uso» y evitar «confirmar si una cuenta existe». [[autenticacion-identity]] prevé `POST /api/auth/password/forgot` y `POST /api/auth/password/reset`, misma respuesta exista o no la cuenta, y enlaces con `App:PublicBaseUrl`. El adaptador de correo y la URL base llegan con [[hu-005-invitar-y-activar-cuentas|HU-005]].

## Alcance

- Caso de uso `RequestPasswordReset` (nombre propuesto): siempre responde igual; solo si la cuenta existe y está `Active` genera token y encola el correo.
- Caso de uso `ResetPassword` (nombre propuesto): valida la política de contraseñas, valida el token y fija la nueva contraseña; actualiza el sello de seguridad (invalida otras sesiones, propuesta) y reinicia el contador de bloqueo (propuesta).
- Envío del correo desacoplado de la respuesta HTTP para que el tiempo de respuesta no dependa de si la cuenta existe (mecanismo a ratificar en T-01; p. ej. cola en memoria atendida por un servicio en segundo plano).
- Endpoints públicos `POST /api/auth/password/forgot` y `POST /api/auth/password/reset`.
- Frontend: páginas públicas `/olvide-mi-contrasena` y `/restablecer-contrasena`.

## Fuera de alcance

- Cambio de contraseña de un usuario autenticado desde su perfil (no lo pide el PRD).
- Restablecimiento iniciado por el administrador para otra persona (no lo pide el PRD; propuesta futura).
- MFA o preguntas de seguridad (PRD §13).
- Proveedor de correo productivo (PRD §16.2).

## Requisitos y reglas de negocio

- La recuperación de contraseña usa token de un solo uso y evita confirmar si una cuenta existe (PRD §12).
- Flujo seguro de restablecimiento, sin revelar si un correo está registrado (PRD §5).
- Contraseñas con solución estándar de identidad y derivación segura (PRD §12).
- Cuentas `Invited` o `Deactivated` no reciben enlace de restablecimiento (propuesta: la invitación es el camino para las pendientes; las desactivadas no deben recuperar acceso).

## Invariantes en juego

- Invariante 9 (`AGENTS.md` §7): la recuperación de contraseña no revela si el correo existe.

## Criterios del PRD cubiertos

- Ninguno de forma directa. Implementa el requisito no funcional de recuperación de PRD §12 → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-002-identidad-y-acceso]]
- Dependencias: [[hu-003-iniciar-y-cerrar-sesion|HU-003]] (Identity, política de contraseñas, bloqueo, sello de seguridad), [[hu-005-invitar-y-activar-cuentas|HU-005]] (adaptador `IEmailSender`, `App:PublicBaseUrl`, patrón de tokens); aceptación de [[adr-0004-autenticacion-cookie-mismo-origen]].
- Relacionadas: [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]] (lista de públicos), [[hu-002-shell-y-navegacion-por-rol|HU-002]] (rutas públicas).
- Decisiones: [[adr-0004-autenticacion-cookie-mismo-origen]] (propuesta).

## Componentes afectados

- Backend `Application`: `RequestPasswordReset`, `ResetPassword`; puertos `IEmailSender`, `IClock`, puerto de cuentas de HU-005.
- Backend `Infrastructure`: proveedor de tokens de restablecimiento (propósito distinto del de invitación), mecanismo de envío desacoplado.
- Backend `Api`: endpoints públicos con antiforgery.
- Frontend: módulo `auth` (dos páginas, gateway, controladores).

## Dificultad

**Nivel:** Medio

**Justificación:** Reutiliza el correo y los tokens de HU-005, pero exige cuidar la equivalencia de respuestas y tiempos, el orden de validaciones y la invalidación de sesiones. Backend y frontend acotados.

## Contrato backend ↔ frontend

Propuesta REST; ratificar en T-01. Públicos, con cabecera antiforgery. Errores `application/problem+json`.

**`POST /api/auth/password/forgot`**

```json
// Petición
{ "email": "ana.perez@empresa-sintetica-01.test" }
// 202 — siempre que el formato sea válido, exista o no la cuenta
{ "message": "Si el correo corresponde a una cuenta activa, recibirás un enlace para restablecer tu contraseña." }
```

| Respuesta | Cuándo |
|---|---|
| `202` | `email` con formato válido (cuenta activa, inexistente, `Invited` o `Deactivated`: misma respuesta) |
| `400` | `email` vacío, con formato inválido o de más de 256 caracteres (no revela nada sobre cuentas) |

**`POST /api/auth/password/reset`**

```json
// Petición (uid y token salen del enlace)
{ "userId": "6f1c2a9e-4c1d-4b8e-9a51-1f0e2d3c4b5a", "token": "CfDJ8…", "newPassword": "Nueva-Clave-2026" }
```

| Respuesta | Cuándo | Cuerpo |
|---|---|---|
| `204` | Token válido y contraseña conforme | — |
| `400` | La contraseña no cumple la política (se valida antes del token) | `{ "errors": { "newPassword": ["La contraseña debe tener al menos 12 caracteres."] } }` |
| `400` | Token inválido, expirado o usado; `userId` inexistente; cuenta no `Active` | `{ "title": "El enlace de restablecimiento no es válido o expiró.", "status": 400 }` (idéntico en todos estos casos salvo `traceId`) |

**Enlace del correo** (propuesta): `{App:PublicBaseUrl}/restablecer-contrasena?uid={userId}&token={token codificado para URL}`.

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar rutas, cuerpos, mensaje genérico, vida útil del token, mecanismo de envío desacoplado, comportamiento con cuentas `Invited`/`Deactivated`, invalidación de sesiones y reinicio del bloqueo tras restablecer.
- [ ] **T-02 — Casos de uso (pruebas primero)** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: pruebas unitarias: `RequestPasswordReset` devuelve el mismo resultado para cuenta activa, inexistente, `Invited` y `Deactivated`, y solo encola correo para la activa; `ResetPassword` valida la política antes del token, no consume el token si la contraseña es inválida y devuelve el mismo error para todos los casos de token/cuenta inválidos.
- [ ] **T-03 — Envío desacoplado** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: el endpoint responde sin esperar al servidor SMTP (prueba con un `IEmailSender` doble que se bloquea hasta ser liberado); fallos de envío se registran en logs sin afectar la respuesta.
- [ ] **T-04 — Tokens y sesiones** · Capa: Backend (Infrastructure/Identity) · Dificultad: Bajo  
  Descripción: token con propósito de restablecimiento (no intercambiable con el de invitación), vida útil configurable y reloj controlable; al restablecer, actualizar el sello de seguridad y reiniciar el contador de bloqueo.
- [ ] **T-05 — Endpoints** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: pruebas de integración primero (CHU-01 a CHU-08); ambos endpoints con `AllowAnonymous`, incluidos en la lista pública de HU-004, y antiforgery.
- [ ] **T-06 — Frontend: modelos y gateway** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: pruebas Vitest primero: validación local del correo (formato, máximo 256), lectura de `uid`/`token`, confirmación de contraseña, `passwordGateway.forgot()` y `passwordGateway.reset()` sobre `postJson`.
- [ ] **T-07 — Frontend: controladores y vistas** · Capa: Frontend (controllers, views) · Dificultad: Medio  
  Descripción: `useForgotPasswordController` + `ForgotPasswordView` (tras `202` muestra el mensaje genérico del backend y no indica si la cuenta existe); `useResetPasswordController` + `ResetPasswordView` (errores por campo, mensaje genérico de enlace inválido con enlace a `/olvide-mi-contrasena`, éxito → `/ingresar` con aviso). Pruebas con Testing Library.
- [ ] **T-08 — Notas para la wiki** · Capa: Documentación · Dificultad: Bajo  
  Descripción: [[autenticacion-identity]] (flujo implementado, decisión sobre sesiones), [[roles-y-permisos]] (ciclo de vida de cuentas).

## Criterios de aceptación

### CHU-01 — Misma respuesta exista o no la cuenta

**Dado** el correo activo `ana.perez@empresa-sintetica-01.test`, el inexistente `nadie@empresa-sintetica-99.test`, uno `Invited` y uno `Deactivated`  
**Cuando** se envía `POST /api/auth/password/forgot` con cada uno  
**Entonces** las cuatro respuestas son `202` con el mismo cuerpo y las mismas cabeceras relevantes; solo la cuenta activa recibe un correo (verificado con el doble de `IEmailSender`).

### CHU-02 — El tiempo de respuesta no depende del envío del correo

**Dado** un `IEmailSender` doble que no termina hasta que la prueba lo libera  
**Cuando** se pide el restablecimiento para la cuenta activa  
**Entonces** el endpoint responde `202` antes de que el envío se libere, igual que para un correo inexistente; y en el mismo entorno de prueba, la mediana de 20 peticiones para la cuenta activa y la de 20 para el correo inexistente no difieren en más del margen ratificado en T-01.

### CHU-03 — Restablecimiento correcto

**Dado** un enlace vigente para la cuenta activa  
**Cuando** se envía `POST /api/auth/password/reset` con `newPassword = "Nueva-Clave-2026"`  
**Entonces** responde `204`; el inicio de sesión con la contraseña anterior responde `401` y con la nueva responde `204` ([[hu-003-iniciar-y-cerrar-sesion|HU-003]]).

### CHU-04 — El token es de un solo uso y expira

**Dado** un token ya usado, y otro token vigente para la misma cuenta evaluado con el reloj de pruebas avanzado un instante después de su vida útil  
**Cuando** se envía `POST /api/auth/password/reset` con cada uno  
**Entonces** ambos responden `400` «El enlace de restablecimiento no es válido o expiró.» y la contraseña no cambia.

### CHU-05 — Política de contraseñas con valores límite

**Dado** un token vigente y la política ratificada en HU-003 (propuesta: longitud mínima 12)  
**Cuando** se envía `newPassword = "Nueva-Clav1"` (11 caracteres) y después `"Nueva-Clave1"` (12 caracteres)  
**Entonces** el primero responde `400` con `errors.newPassword` y **no consume** el token; el segundo responde `204`.

### CHU-06 — El restablecimiento no revela cuentas

**Dado** un `userId` inexistente, uno de una cuenta `Deactivated` y un token manipulado de una cuenta activa  
**Cuando** se envía `POST /api/auth/password/reset` con una contraseña válida  
**Entonces** las tres respuestas son `400` con el mismo `title` y los mismos campos (comparando el JSON sin `traceId`).

### CHU-07 — Las demás sesiones dejan de valer (propuesta)

**Dado** la cuenta activa con una sesión abierta en otro navegador (cookie C1)  
**Cuando** se restablece la contraseña  
**Entonces** una petición autenticada con C1, una vez cumplido el intervalo de revalidación del sello de seguridad (en pruebas, cero), responde `401`; y el contador de intentos fallidos de la cuenta queda en cero.

### CHU-08 — Un token de invitación no sirve para restablecer

**Dado** un token de invitación válido de [[hu-005-invitar-y-activar-cuentas|HU-005]]  
**Cuando** se usa en `POST /api/auth/password/reset`  
**Entonces** responde `400` genérico (y, a la inversa, un token de restablecimiento falla en `POST /api/auth/invitations/accept`).

### CHU-09 — El enlace usa la URL base configurada

**Dado** `App:PublicBaseUrl = https://dataticket.example` y una petición con `Host: evil.example`  
**Cuando** se pide el restablecimiento para la cuenta activa  
**Entonces** el enlace del correo empieza por `https://dataticket.example/restablecer-contrasena` y no contiene `evil.example`.

### CHU-10 — Las páginas no revelan cuentas y manejan errores

**Dado** la página `/olvide-mi-contrasena`  
**Cuando** la persona envía un correo con formato válido  
**Entonces** ve el mismo mensaje genérico exista o no la cuenta; con `ana.perez` (sin dominio) ve «El correo no tiene un formato válido.» sin enviar la petición; y en `/restablecer-contrasena`, ante el `400` genérico, ve «El enlace de restablecimiento no es válido o expiró.» con un enlace para pedir uno nuevo.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-10 validados con evidencia (CHU-07 solo si T-01 confirma la propuesta; si no, se marca como fuera de alcance con la decisión enlazada).
- [ ] **DoD-02** — Pruebas escritas primero y en verde, con nombres por comportamiento: `Forgot_responde_igual_para_activa_inexistente_pendiente_y_desactivada`, `Forgot_no_espera_al_envio_del_correo`, `Reset_cambia_la_contrasena`, `Token_de_reset_es_de_un_solo_uso_y_expira`, `Contrasena_invalida_no_consume_el_token`, `Reset_responde_igual_ante_cuenta_inexistente_o_desactivada`, `Reset_invalida_otras_sesiones`, `Tokens_de_invitacion_y_reset_no_son_intercambiables`, `Enlace_de_reset_ignora_Host`.
- [ ] **DoD-03** — Pruebas Vitest de ambas páginas (modelos, controladores, vistas) en verde.
- [ ] **DoD-04** — `cd backend && dotnet test` en verde, incluidas `DataTicket.ArchitectureTests`.
- [ ] **DoD-05** — `npm --prefix frontend run lint`, `npm --prefix frontend test` y `npm --prefix frontend run build` en verde.
- [ ] **DoD-06** — Sin cambios de esquema previstos; si T-01 añade alguno, migración EF Core creada y revisada.
- [ ] **DoD-07** — `docker compose up --build`: el correo de restablecimiento llega a Mailpit (`http://localhost:8025`) y el enlace abre `http://localhost:5173/restablecer-contrasena` (captura en el PR).
- [ ] **DoD-08** — Wiki actualizada vía Notas para la wiki: [[autenticacion-identity]], [[roles-y-permisos]].
- [ ] **DoD-09** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos (enumeración por respuesta o por tiempo, reutilización de tokens).
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
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.

## Notas y decisiones

- La invalidación de otras sesiones (CHU-07) y el reinicio del bloqueo son **propuestas**: el PRD no los menciona; reducen el riesgo de que una sesión robada sobreviva al cambio de contraseña.
- El margen de tiempos de CHU-02 se fija en T-01; la prueba debe ser estable en CI (si resulta inestable, se conserva solo la parte determinista: respuesta antes de liberar el envío).
- **Limitación de peticiones** en `/password/forgot` y `/password/reset` (p. ej. por IP y por correo) para frenar abuso del envío: propuesta, no exigida por el PRD.
- **Auditoría**: el PRD §12 no lista el restablecimiento; registrar el evento es propuesta (V-11).

## Relacionado

- [[ep-002-identidad-y-acceso]] · [[tablero-scrum]] · [[autenticacion-identity]] · [[roles-y-permisos]] · [[adr-0004-autenticacion-cookie-mismo-origen]] · [[entorno-docker]]

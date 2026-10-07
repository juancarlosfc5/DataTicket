---
title: "EP-002 — Identidad y acceso"
type: epica
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/epica, seguridad, identity, arquitectura/backend]
sources: ["PRD.md §5", "PRD.md §12", "PRD.md §13", "PRD.md §14"]
aliases: ["EP-002", "Identidad y acceso"]
fase_prd: "0"
criterios_prd: ["CA-01"]
historias: ["[[hu-003-iniciar-y-cerrar-sesion]]", "[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-005-invitar-y-activar-cuentas]]", "[[hu-006-restablecer-contrasena]]"]
dependencias: ["[[ep-001-fundaciones-tecnicas]]"]
created: 2026-10-07
updated: 2026-10-07
---

# EP-002 — Identidad y acceso

Autenticación con ASP.NET Core Identity y autorización por rol en servidor: iniciar y cerrar sesión con cookie de mismo origen, exponer el contexto del usuario actual, activar cuentas por invitación y restablecer contraseñas sin revelar si un correo existe.

## Objetivo

Que solo las personas a las que Data Global invitó puedan entrar a DataTicket, que cada petición se autorice en el backend según el rol y la empresa de la cuenta, y que los flujos de invitación y restablecimiento sean seguros y no permitan enumerar cuentas (PRD §5, §12).

## Valor esperado

- Base de seguridad para todo el producto: el aislamiento multiempresa (PRD CA-01) se apoya en que la empresa sale de la sesión y nunca de la petición.
- Cero registro público y cero enumeración de cuentas desde el primer incremento (invariante 9 de `AGENTS.md` §7).
- Las HU de administración ([[ep-003-empresas-usuarios-y-equipos]]) y todas las funcionales reutilizan `ICurrentUser` y las políticas por rol.

## Fase del PRD

Fase 0 — Base técnica (PRD §13). Prioridad: bloqueante.

## Actores

- Usuario cliente: Solicitante (`Requester`) y Coordinador de empresa (`CompanyCoordinator`).
- Usuario interno: PM (rol `ProductManager`), Administrador (`Administrator`), integrantes de Desarrollo y Producción.
- Persona invitada que aún no ha activado su cuenta.

## Alcance

- Inicio y cierre de sesión con cookie de Identity `HttpOnly`, `SameSite=Lax`, `Secure` fuera de Development, antiforgery y bloqueo por intentos fallidos ([[hu-003-iniciar-y-cerrar-sesion|HU-003]]).
- `GET /api/auth/me`, políticas por rol, `ICurrentUser` en Application, claim `company_id` y exigencia de autenticación por defecto en todos los endpoints ([[hu-004-contexto-de-usuario-y-autorizacion|HU-004]]).
- Invitación con token de un solo uso y activación con contraseña propia ([[hu-005-invitar-y-activar-cuentas|HU-005]]).
- Restablecimiento de contraseña sin enumeración ([[hu-006-restablecer-contrasena|HU-006]]).

## Fuera de alcance

- MFA, SSO y federación de identidad (PRD §13).
- Registro público de clientes (PRD §5).
- `MapIdentityApi` completo: expone `/register`, contrario al PRD ([[adr-0004-autenticacion-cookie-mismo-origen]]).
- Crear empresas y usuarios con su perfil: lo hace [[ep-003-empresas-usuarios-y-equipos]].
- Proveedor de correo productivo y dominio remitente (PRD §16.2); en local se usa Mailpit.

## Requisitos y reglas de negocio

- Sin registro público; Data Global crea, invita y desactiva las cuentas de empresas cliente (PRD §5).
- Invitación y restablecimiento seguros, sin revelar si un correo está registrado (PRD §5).
- Token de un solo uso para recuperar contraseña; evita confirmar si una cuenta existe (PRD §12).
- Contraseñas con solución estándar de identidad y derivación segura; nada de cifrado reversible ni hash casero (PRD §12).
- Todo acceso exige autenticación, salvo páginas de entrada expresamente públicas (PRD §12).
- La autorización se aplica en servidor en cada operación (PRD §5 regla 2).
- Los usuarios cliente solo consultan tickets de la empresa a la que pertenece su cuenta (PRD §5 regla 1).
- Transporte productivo cifrado por HTTPS (PRD §12).

## Criterios del PRD cubiertos

- PRD CA-01 — Aislamiento empresa A ↔ empresa B (parcial: la empresa del usuario se toma solo de la sesión; las pruebas negativas sobre tickets llegan con [[ep-008-bandejas-y-portal-del-cliente]]) → [[criterios-de-aceptacion]]

## Dependencias

- [[adr-0004-autenticacion-cookie-mismo-origen]] en estado `propuesta`: debe aceptarse (o cambiarse) antes de implementar [[hu-003-iniciar-y-cerrar-sesion|HU-003]] ([[pendientes]] §4).
- [[ep-001-fundaciones-tecnicas]]: cliente HTTP con antiforgery y shell con rutas públicas/protegidas ([[hu-002-shell-y-navegacion-por-rol|HU-002]]).
- Para invitar cuentas con perfil hacen falta las HU de [[ep-003-empresas-usuarios-y-equipos]] ([[hu-008-administrar-usuarios-cliente|HU-008]], [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]]); HU-005 aporta el mecanismo que ellas invocan.

## Historias de usuario

- [[hu-003-iniciar-y-cerrar-sesion|HU-003 — Iniciar y cerrar sesión]]
- [[hu-004-contexto-de-usuario-y-autorizacion|HU-004 — Contexto del usuario y autorización por rol]]
- [[hu-005-invitar-y-activar-cuentas|HU-005 — Invitar y activar cuentas]]
- [[hu-006-restablecer-contrasena|HU-006 — Restablecer contraseña sin enumeración]]

## Criterio de completitud

- [ ] HU-003 a HU-006 están `Completada` con su matriz de evidencia en `Cumple`.
- [ ] ADR-0004 está `aceptada` (o reemplazada por otro ADR aceptado) y [[autenticacion-identity]] refleja lo implementado.
- [ ] Una prueba automatizada recorre todos los endpoints y demuestra que solo la lista explícita de públicos admite acceso anónimo.
- [ ] Las respuestas de login fallido, recuperación de contraseña y aceptación de invitación no permiten distinguir si un correo existe.
- [ ] No quedan dependencias bloqueantes dentro del alcance.

## Riesgos e incógnitas

- **ADR-0004 sin aceptar**: si el equipo cambiara a bearer tokens, cambian los contratos de HU-003, HU-004 y del hub SignalR.
- **Roles frente a equipos**: [[autenticacion-identity]] define roles Identity `Development` y `Production`, mientras [[glosario]] y [[roles-y-permisos]] usan `Team.Development` / `Team.Production`. Afecta a `/api/auth/me` (HU-004) y a la administración de equipos ([[hu-009-administrar-usuarios-internos-y-equipos|HU-009]]). No se resuelve aquí.
- **Quién invita**: [[autenticacion-identity]] dice «PM/administrador»; el PRD §5 dice «Data Global». Las HU proponen `Administrator` y lo dejan a ratificar.
- **Entregabilidad de correo** sin proveedor productivo (PRD §16.2): invitaciones y restablecimientos solo se verifican en Mailpit.
- **Despliegue tras proxy/TLS**: `UseForwardedHeaders`, `AllowedHosts` fijo y `X-Forwarded-Proto` correctos son necesarios para que `Secure` y los enlaces funcionen ([[pendientes]] §4).
- **Revocación de sesión**: la invalidación por sello de seguridad de Identity no es instantánea; el intervalo de revalidación debe ratificarse.

## Relacionado

- [[tablero-scrum]] · [[fases-y-alcance]] · [[autenticacion-identity]] · [[adr-0004-autenticacion-cookie-mismo-origen]] · [[roles-y-permisos]] · [[backend-hexagonal]] · [[entorno-docker]] · [[ep-001-fundaciones-tecnicas]] · [[ep-003-empresas-usuarios-y-equipos]]

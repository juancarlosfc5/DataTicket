---
title: Pendientes, dudas y contradicciones
type: pendientes
status: vigente
tags: [pendientes, decision]
sources: ["PRD.md §16", "Revisión del PRD durante el setup (2026-10-07)", "Revisión del código de B3 del goal del login (2026-10-12)", "Revisión del código de B4 del goal del login (2026-10-12)"]
aliases: [Preguntas abiertas, Decisiones pendientes, Contradicciones]
created: 2026-10-07
updated: 2026-10-12
---

# Pendientes, dudas y contradicciones

Registro vivo de lo que falta decidir o aclarar. Cuando algo se resuelve, se convierte en ADR o en contenido de la página correspondiente y se tacha aquí con la fecha y el enlace. Responsable por defecto: la persona líder del proyecto con el dueño del producto.

## 1. Contradicciones detectadas

> [!warning] Contradicción — PRD §15 desactualizado
> Dice que el Compose levanta solo PostgreSQL y que no existen proyectos .NET/React. Desde el 2026-10-07 el repositorio tiene ambos y un Compose de cinco servicios ([[analisis-prd-vs-docker-compose]]). **Acción:** actualizar §15 en la próxima versión del PRD (no se editó por ser fuente inmutable).

> [!warning] Contradicción — tipos de adjunto en el chat
> PRD §7 (objetivo) habla de chat "de texto e imágenes"; PRD §7.8 y §14.9 permiten además PDF, XML y Excel. Se asume lo más amplio (§7.8/§14.9) hasta confirmar → [[chat-interno]], [[archivos-adjuntos]].

> [!success] Resuelta 2026-10-07 — fase del portal del cliente
> ~~PRD §13 ubica el "portal con estados resumidos" en la fase 3 (P1), pero CA-02 lo exige en el MVP y la fase 1 ya incluye "estados" y "bandejas".~~ La consulta básica del portal va en la Fase 1, y el resumen y las métricas en la Fase 3 → [[adr-0008-portal-cliente-en-fase-1]].

> [!warning] Contradicción — fase del dashboard y alcance del "MVP"
> CA-13 exige el dashboard en el MVP, pero PRD §13 lo ubica en la Fase 3 (P1). El PRD no define si el "MVP" abarca las fases 0–2 o las fases 0–3 → [[fases-y-alcance]], [[ep-012-dashboard-y-metricas]].

> [!warning] Contradicción — roles frente a equipos
> [[autenticacion-identity]] define `Development` y `Production` como roles de Identity. En cambio, [[glosario]], [[roles-y-permisos]] y [[modelo-de-dominio]] los tratan como equipos (`Team.Development`, `Team.Production`), separados de `Roles[]`. Hay que decidir el modelo antes de [[hu-004-contexto-de-usuario-y-autorizacion]] y [[hu-009-administrar-usuarios-internos-y-equipos]].
> Actualización 2026-10-09: `db.sql` usa provisionalmente la opción de [[autenticacion-identity]] y siembra seis roles, entre ellos `Development` y `Production`, sin tablas de equipos. Si se eligen equipos separados, hay que añadir esas tablas al diseño de referencia y a la migración de HU-009 ([[persistencia-postgresql]]).
> Actualización 2026-10-09 (B1 de `goal_login.md`, DoD-06 de HU-004): se **implementa** la opción de `db.sql` (equipos = roles de Identity `Development`/`Production`), pero `/api/auth/me` los expone en un campo `teams` separado de `roles`, de modo que el contrato del frontend no cambia si HU-009 adopta tablas de equipos. La decisión definitiva sigue abierta para HU-009.
> Actualización 2026-10-12 (B4 de `goal_login.md`, DoD-06 de HU-004): el código **mantiene la decisión de B1**, sin cambios de esquema ni migración. Los equipos son los roles de Identity `Development` y `Production` (`RoleNames.Teams`), viajan en la cookie como `ClaimTypes.Role` y se exponen aparte en `teams` de `/api/auth/me` y en `ICurrentUser.Teams`; `ICurrentUser.Roles` e `IsInRole` no los incluyen. El claim `team` que la HU-004 proponía en sus notas no se creó. La decisión definitiva sigue abierta para HU-009 → [[roles-y-permisos]], [[autenticacion-identity]].

> [!success] Resuelta 2026-10-09 — DB-first frente a migraciones code-first
> `db.sql` (raíz) fue un paso previo de validación del esquema y no el mecanismo de implementación. La persona líder del proyecto lo aprobó como **diseño de referencia**. La base de datos se implementa con migraciones de EF Core, como piden [[persistencia-postgresql]] y los DoD de las HU, sin cambios en estas. Decisión: [[adr-0009-db-sql-diseno-de-referencia]].

> [!success] Resuelta 2026-10-09 — prototipo `DataTicket.html` frente al PRD
> La persona líder del proyecto decidió que el prototipo es **solo referencia visual**: lo que difiere del PRD o de las HU (respuestas públicas en el hilo, estados propios, entrada en dos puertas) no se tiene en cuenta. El mapeo de colores por estado de [[sistema-de-diseno]] sigue siendo una propuesta visual. Detalle en [[fuente-prototipo-dataticket-html]].

> [!warning] Contradicción — métrica "primera respuesta"
> PRD §13 (fase 3) menciona "medición de primera respuesta", métrica que §4 no define (solo define tiempo hasta respuesta formal) → [[dashboard-y-metricas]].

> [!warning] Contradicción menor — identificadores de los roles de referencia (2026-10-12)
> `db.sql` §12 genera al azar el `id` (`DEFAULT uuidv7()`) y el `concurrency_stamp` (`gen_random_uuid()`) de los seis roles. La migración `AddIdentityAndCompanies` (B3) los siembra con `HasData` y valores fijos (`a8000000-0000-7000-8000-00000000000N`), porque el modelo de EF Core debe ser estable entre migraciones. No cambia el comportamiento: los roles se referencian por `normalized_name`. Se aplicó el código y `db.sql` no se tocó. **Acción:** decidir si se alinea `db.sql` §12 con los identificadores fijos, como pide [[adr-0009-db-sql-diseno-de-referencia]] cuando una HU se aparta del diseño → [[persistencia-postgresql]].

> [!warning] Contradicción menor — `GET /api/auth/me` responde `403` y el contrato solo lista `200` y `401` (2026-10-12)
> El contrato T-01 ([[autenticacion-identity]], tabla *Endpoints*) y [[hu-004-contexto-de-usuario-y-autorizacion]] prevén para `/me` solo `200` y `401`. El código de B4 responde además `403` «No tienes permiso para realizar esta acción.» cuando hay sesión pero el perfil no es válido (cliente sin `company_id`, perfil mixto, interno con empresa, `company_id` mal formado, sesión sin correo o empresa inexistente). Es coherente con el «falla cerrado» de CHU-07, pero amplía el contrato. Por lo que se lee en el código (no hay prueba que acote cuándo ocurre), el login y la revalidación del sello ya rechazan los perfiles no válidos, así que el `403` quedaría para la ventana de hasta 5 minutos entre un cambio que invalida el perfil y la siguiente revalidación, y para el caso de los claims de tipo rol de §4, donde la cookie y `identity.user_roles` discrepan de forma permanente. El SPA no lo distingue: `fetchCurrentUser` solo trata el `401` como «sin sesión», de modo que un `403` de `/me` acaba en el estado de error de carga y no en la vista «sin permiso». No se tocó ni el contrato ni el código. **Acción:** ratificar el `403` (y añadirlo al contrato y al SPA) o decidir otra respuesta (p. ej. `401` y cerrar la sesión) → [[autenticacion-identity]].

## 2. Decisiones de producto pendientes antes de producción (PRD §16)

- [ ] Infraestructura y responsable de despliegue, respaldos, monitoreo y actualizaciones (§16.1).
- [ ] Proveedor de correo, dominio remitente y entregabilidad (§16.2). En local: Mailpit.
- [ ] Retención/eliminación de tickets, mensajes, auditoría y archivos; respaldos, RPO/RTO (§16.3, §12).
- [ ] ¿Reportes exportables o vistas guardadas tras el piloto? (§16.4).
- [ ] Volumen de datos sintéticos: referencia ~50 empresas de prueba con tickets variados (§16.5).
- [ ] Aprobación explícita del riesgo de URL pública permanente en Azure Blob por la dirección responsable de los datos (§16.6) → [[adr-0006-urls-publicas-azure-blob]].

## 3. Vacíos y ambigüedades del PRD

| # | Sección | Pregunta | Página |
|---|---|---|---|
| V-01 | §6.4 | ¿Qué transiciones de estado son válidas, quién mueve cada una, hay retrocesos (PR rechazado) o reapertura? ¿`Closed` exige `SolutionDelivered`? | [[estados-del-ticket]] |
| V-02 | §6.3 | ¿En qué estado inicia un ticket asignado directamente a Julián? ¿Cómo es el camino de tickets que no pasan por Producción? | [[flujo-del-ticket]] |
| V-03 | §6.1 | ¿Qué roles pueden ajustar la prioridad? ¿Las colas ordenan por prioridad calculada o ajustada? | [[matriz-de-prioridad]] |
| V-04 | §6.3 | ¿Quién es la "persona autorizada" que registra la URL del PR? | [[flujo-del-ticket]] |
| V-05 | §5.5, §6.2 | ¿Cómo se determina la "ausencia" de Elizabeth? ¿La cola de cobertura está siempre visible para administradores? | [[roles-y-permisos]] |
| V-06 | §8 | Extensiones/MIME exactas de "imágenes" y "Excel"; validación de contenido más allá de la extensión (se propone validar firma) | [[archivos-adjuntos]] |
| V-07 | §11 | ¿Quién recibe el correo de la respuesta formal: solicitante, coordinador o ambos? | [[notificaciones]] |
| V-08 | §7.7, §11 | ¿Se reenvía el correo de vinculación a quien es retirado y vuelto a agregar? ¿Lo recibe el administrador que toma un ticket en cobertura? | [[notificaciones]] |
| V-09 | §4 | Días hábiles definidos (lun–vie, festivos cuentan), pero no horario hábil | [[dashboard-y-metricas]] |
| V-10 | §4, §6.2, §10 | Diferencia operativa entre "responsable", "asignación" y "participante" | [[flujo-del-ticket]] |
| V-11 | §12 | ¿Qué significa el campo "resultado" de la auditoría? ¿Se auditan intentos denegados? | [[auditoria]] |
| V-12 | §6.1, §9 | Catálogo de categorías y quién lo administra | [[formularios-configurables]] |
| V-13 | §6.5 | ¿Puede haber más de una respuesta formal o corregirse una ya enviada? | [[flujo-del-ticket]] |
| V-14 | §7 | ¿Se pueden editar o borrar mensajes del chat? | [[chat-interno]] |
| V-15 | §5 | Administradores del piloto y responsabilidades de Gerardo | [[equipo-data-global]] |
| V-16 | §5.5, §6.2 | Formato del número de ticket | [[modelo-de-dominio]] |
| V-17 | §14.1 | CA-01 protege la descarga "desde el portal", pero las URLs públicas del Blob permiten acceso entre empresas fuera del portal (riesgo aceptado; conviene explicitarlo ante la dirección) | [[adr-0006-urls-publicas-azure-blob]] |
| V-18 | §4, §10, §14.13 | "Área" se usa para filtrar y agrupar, pero no existe en el modelo: ¿es la categoría o el equipo? | [[dashboard-y-metricas]], [[hu-040-panel-global-de-la-pm]] |
| V-19 | §6.4, §6.5 | El diagrama inferido no permite `PullRequestReview → SolutionDelivered` (cierre tras PR sin pasar por Producción) | [[estados-del-ticket]], [[hu-021-cambiar-estado-interno]] |
| V-20 | §8, §5.6 | `Attachment` no indica su contexto (radicación, chat o respuesta) ni si es interno o visible al cliente, y eso hace falta para CA-10 | [[modelo-de-dominio]], [[archivos-adjuntos]] |
| V-21 | §6.5, §12 | `Ticket` no tiene campos de cierre (quién, cuándo, estado anterior): ¿solo en `AuditEntry` o también en la entidad? | [[modelo-de-dominio]], [[hu-036-cerrar-ticket-manualmente]] |
| V-22 | §7 | Formato y tamaño de página del cursor del historial; ¿hay paginación hacia atrás? | [[tiempo-real-signalr]], [[hu-026-historial-del-chat-por-cursor]] |
| V-23 | §9 | Almacenamiento de las respuestas variables de la Fase 4 (jsonb o tablas) y cómo se consultan | [[formularios-configurables]], [[hu-046-radicar-con-formulario-de-empresa]] |
| V-24 | §12 | ¿Quién puede consultar la bitácora de auditoría de un ticket? | [[auditoria]], [[hu-012-consultar-bitacora-del-ticket]] |
| V-25 | — | El mapa de puertos y el glosario no tienen casos de uso para reasignar, ajustar la prioridad, cambiar el estado ni registrar la URL del PR, ni los métodos del hub | [[backend-hexagonal]], [[glosario]] |

## 3b. Propuestas del backlog Scrum por ratificar (2026-10-07)

Las HU de [[tablero-scrum]] cubren los vacíos del PRD con **propuestas** marcadas como tales. Cada una se ratifica o se cambia en la tarea T-01 de su HU, y si se acepta pasa al glosario o a un ADR. Las de mayor impacto:

| # | Propuesta | HU | Página |
|---|---|---|---|
| P-01 | **Recurso no visible:** 404 para internos no asociados, administradores no asociados y retirados, para no revelar que el ticket existe; 403 por rol insuficiente (p. ej. un cliente en rutas internas). La [[estrategia-de-pruebas]] admite "404/403" | HU-020, HU-024, HU-026, HU-033 | [[roles-y-permisos]] |
| P-02 | **Cuentas y plantillas:** solo `Administrator` invita y administra cuentas y plantillas; [[autenticacion-identity]] dice "PM/administrador" | HU-005, HU-007 a HU-009, HU-043 | [[roles-y-permisos]] |
| P-03 | **Administrador en respuesta y cierre:** debe estar asociado para emitir la respuesta formal o cerrar | HU-033, HU-036 | [[flujo-del-ticket]] |
| P-04 | **Respuesta formal (V-13):** una sola por ticket; la segunda devuelve 409 | HU-033 | [[flujo-del-ticket]] |
| P-05 | **Transiciones (V-01):** la respuesta se emite desde `InDevelopment`, `PullRequestReview` o `InProduction`; `Closed` exige `SolutionDelivered`; un ticket cerrado devuelve 409 ante cualquier acción; la URL del PR no es obligatoria para pasar a `PullRequestReview` | HU-021, HU-033, HU-036 | [[estados-del-ticket]] |
| P-06 | **Prioridad en el portal:** el cliente ve la calculada; no ve el ajuste ni su motivo | HU-015, HU-024 | [[matriz-de-prioridad]] |
| P-07 | **Correos (V-07, V-08):** la respuesta formal se envía al solicitante. Sin reenvío del correo de vinculación al reasociar y sin correo a quien se asocia a sí mismo. Si el correo falla, la respuesta no se revierte: queda un estado de entrega con reintento manual | HU-035, HU-038 | [[notificaciones]] |
| P-08 | **Adjuntos (V-06):** lista blanca pdf, png, jpg/jpeg, gif, webp, xml, xlsx y xls, con firma; se excluyen svg y xlsm. Se rechazan los archivos de 0 bytes. 10 MB = 10 485 760 bytes. `AttachmentContext` {`Submission`, `Chat`, `FormalResponse`} resuelve V-20 | HU-014, HU-029, HU-033 | [[archivos-adjuntos]] |
| P-09 | **Límites de cuerpo:** Kestrel admite unos 28,6 MiB por defecto frente a 50 MB en nginx; alinearlos al decidir el máximo de archivos por solicitud | HU-014, HU-033 | [[entorno-docker]] |
| P-10 | **Auditoría append-only:** con un trigger, porque `REVOKE` no sirve mientras la aplicación sea dueña del esquema. Chocará con la política de retención (§16.3). Las acciones administrativas también se auditan (extiende §12). Los mensajes del chat no se auditan uno por uno | HU-007, HU-011, HU-028 | [[auditoria]], [[persistencia-postgresql]] |
| P-11 | **Bitácora (V-24):** la consultan la PM, los participantes internos vigentes y el administrador asociado; nunca el cliente | HU-012 | [[auditoria]] |
| P-12 | **Cursor del chat (V-22):** token opaco base64url de (`CreatedAt`, `Id`); `limit` 50 por defecto y 100 como máximo. Tras cada `JoinTicket` se hace una consulta `after` para cubrir el hueco historial→unión del flujo del PRD §7 | HU-026, HU-031 | [[tiempo-real-signalr]] |
| P-13 | **Revocación:** sin cerrar la conexión; sale del grupo y recibe el evento `TicketAccessRevoked`. Se valida `Origin` en la negociación del hub como defensa contra el secuestro de WebSocket entre sitios. ¿Qué pasa con las conexiones al desactivar un usuario? Depende del intervalo del *security stamp* | HU-027, HU-032, HU-008 | [[tiempo-real-signalr]], [[autenticacion-identity]] |
| P-14 | **Lecturas:** se registran las de Elizabeth aunque no sea participante; el autor no genera lectura de su propio mensaje | HU-030 | [[chat-interno]] |
| P-15 | **Notificaciones in-app:** Elizabeth solo las recibe de los tickets donde es participante explícita; notificar a un usuario concreto exigirá backplane si hay varias instancias | HU-037 | [[notificaciones]] |
| P-16 | **Métricas:** "activo" = `New`, `InDevelopment`, `PullRequestReview` o `InProduction`; "sin asignar" = `New` sin ninguna `Assignment` vigente; tiempo medido en minutos hábiles de 24 h mientras V-09 siga abierta; el cliente no ve tiempos. Un administrador recibe 403 en el panel global. Hay que comprobar que las imágenes Docker incluyan datos de zona horaria (`America/Bogota`) | HU-039 a HU-042 | [[dashboard-y-metricas]] |
| P-17 | **Cierre (V-21):** `ClosedAt` y `ClosedBy` en `Ticket`, además de la auditoría; `Assignment` con `EndedAt` y `EndedBy`; `AuditEntry` con `ObjectType`, `ObjectId` y `TicketId` | HU-011, HU-019, HU-036 | [[modelo-de-dominio]] |
| P-18 | **Cuentas:** contraseña de mínimo 12 caracteres y bloqueo tras 5 intentos fallidos. Desactivar una empresa bloquea el acceso de sus usuarios; no está definido qué pasa con sus tickets abiertos | HU-003, HU-007 | [[autenticacion-identity]] |
| P-19 | **Radicación:** los internos no radican en nombre de un cliente (403); descripción de hasta 10 000 caracteres; categorías semilla provisionales ("Incidente", "Requerimiento", "Consulta") sujetas a V-12 | HU-013 | [[flujo-del-ticket]] |
| P-20 | **Formularios configurables:** hasta 30 campos y de 2 a 50 opciones; tipos `ShortText`, `LongText`, `Number`, `Date`, `SingleChoice` y `MultipleChoice`; solo los campos marcados como consultables se filtran en las bandejas internas. Almacenamiento: ver V-23 | HU-043 a HU-047 | [[formularios-configurables]] |
| P-21 | **Datos sintéticos:** la semilla usa los nombres reales del equipo interno con correos ficticios `@dataticket.local`; hay que confirmarlo | HU-010 | [[equipo-del-repositorio]] |
| P-22 | **Sesión (valores por defecto del goal del login, en uso):** cookie `dataticket.auth` con expiración deslizante de 8 h; antiforgery `X-XSRF-TOKEN` renovado tras login/logout; sello de seguridad revalidado cada 5 min; el logout no revoca otras sesiones (riesgo aceptado); perfiles de rol mixtos (cliente + interno) no inician sesión | HU-002, HU-003, HU-004 | [[autenticacion-identity]] |
| P-23 | **Autorización (B4 del goal del login, en uso, por ratificar):** (a) **`404` frente a `401` en rutas inexistentes.** La política de reserva se limita a los endpoints registrados (`EndpointScopedAuthorizationResultHandler`): una ruta que no existe responde `404` también a un anónimo, para conservar CHU-09 de HU-003; sin ese manejador ASP.NET Core 10 respondería `401`. Contrapartida: un middleware futuro que sirva contenido sin endpoint y colocado después de `UseAuthorization` (p. ej. archivos estáticos) no quedaría cubierto por la reserva; hoy el Api no tiene ninguno. (b) **Nombre del claim `display_name`** para el nombre visible: el contrato T-01 no fijaba el tipo del claim. (c) El `403` de `/me` con perfil no válido: ver la contradicción de §1 | HU-003, HU-004 | [[autenticacion-identity]] |

Los **nombres de código nuevos** que proponen las HU (casos de uso, puertos, DTO, acciones de auditoría, eventos del hub) se agregan a [[glosario]] cuando se ratifiquen en la T-01 de cada HU, antes de usarlos en código.

## 4. Decisiones técnicas abiertas

- [x] ~~Aceptar o cambiar [[adr-0004-autenticacion-cookie-mismo-origen]] (cookie same-origin) antes de implementar el login.~~ Aceptado el 2026-10-09 (D1 de [[plan-goal-login-y-loop-chat]]).
- [ ] Evaluar Row-Level Security de PostgreSQL como defensa adicional al filtro multiempresa (PRD §12) → [[persistencia-postgresql]].
- [x] ~~Enrutador del frontend~~ → [[adr-0010-enrutador-react-router]] (React Router 8.4.0, 2026-10-09).
- [ ] Librería de estado de servidor (p. ej. TanStack Query): no se adopta por ahora; reabrir si aparecen cachés compartidas entre pantallas → [[adr-0010-enrutador-react-router]].
- [ ] Estrategia de despliegue productivo que mantenga el mismo origen para `/api` y `/hubs` (nginx del frontend u otro proxy) → [[entorno-docker]].
- [ ] Backplane de SignalR (Redis o Azure SignalR) solo al escalar a varias instancias (PRD §7) → [[tiempo-real-signalr]].
- [ ] CI en GitHub Actions (pruebas backend, lint/pruebas/build frontend) → [[flujo-de-trabajo-github]].
- [ ] Evaluar migración a TypeScript 7 (nativo) cuando la plantilla de Vite lo adopte → [[stack-y-versiones]].
- [ ] Node 24 entra en *Maintenance LTS* el 2026-10-20 (Node 26 es LTS desde el 2026-10-28); reevaluar antes de producción → [[stack-y-versiones]].
- [ ] Modelo de dominio: validar la propuesta al implementar la fase 0/1 → [[modelo-de-dominio]].
- [ ] Máximo de archivos por solicitud (radicación, respuesta formal): define el límite de cuerpo del backend; nginx admite hoy 50 MB en `/api/` → [[archivos-adjuntos]], [[entorno-docker]].
- [ ] Content-Security-Policy del SPA: requiere conocer el dominio público del Blob para `img-src`/`connect-src` → [[entorno-docker]].
- [ ] Al desplegar tras un proxy/TLS: `UseForwardedHeaders` con proxies conocidos, `AllowedHosts` fijo y `X-Forwarded-Proto` correcto → [[autenticacion-identity]].
  - Ampliación 2026-10-12 (B3): fuera de Development las cookies `dataticket.auth` y `dataticket.xsrf` usan `CookieSecurePolicy.Always`, y con esa política antiforgery exige que la petición llegue por HTTPS. Tras un proxy que termina TLS el backend recibe HTTP: sin `UseForwardedHeaders` (hoy no está en `Program.cs`) el token antiforgery no se emite ni se valida y el login no funciona. Es comportamiento del framework y ninguna prueba del repositorio lo cubre todavía. Previsiblemente afecta también a probar la imagen `runtime` por HTTP en local → [[entorno-docker]].

Endurecimiento del login, detectado al cerrar B3 del goal del login (2026-10-12). Ninguno bloquea B3; deben resolverse antes de un entorno compartido:

- [ ] **Claves de Data Protection sin cifrar en reposo.** Se guardan en `identity.data_protection_keys` con `PersistKeysToDbContext`, sin `ProtectKeysWith*`: quien lea esa tabla o un respaldo puede falsificar cookies de sesión y tokens. Elegir el mecanismo de cifrado junto con la infraestructura (§16.1) → [[autenticacion-identity]], [[persistencia-postgresql]].
- [ ] **`POST /api/auth/login` sin limitador de peticiones y `password` sin longitud máxima.** El bloqueo por cuenta (5 intentos, P-18) no frena un barrido de muchas cuentas, y cada intento cuesta un cálculo de hash (también para correos inexistentes, por el hash señuelo). Definir un limitador y un máximo de longitud para `password` → [[autenticacion-identity]].
- [ ] **El login aún no rechaza a los usuarios de empresas inactivas.** `AccountAccess.CanSignIn` solo mira el estado de la cuenta y el perfil, tanto al iniciar sesión como al revalidar el sello. P-18 propone que desactivar una empresa bloquee el acceso de sus usuarios; es una tarea de [[hu-007-administrar-empresas-cliente]] → [[autenticacion-identity]].
  - Ampliación 2026-10-12 (B4): `GET /api/auth/me` tampoco lo comprueba. `GetCurrentUserProfile` solo exige que la empresa **exista**: `ICompanyDirectory.FindNameAsync` devuelve el nombre sin mirar `Company.IsActive`, así que el usuario de una empresa desactivada seguiría recibiendo `200` con su empresa y satisfaciendo `ClientUser` (lectura del código: ninguna ruta consulta `IsActive` y ninguna prueba cubre empresas inactivas). HU-007 tendrá que decidir dónde se corta (login, revalidación del sello, `/me` y políticas) y con qué respuesta.
- [ ] **FK compuesta de `tickets` hacia `identity.users (id, company_id)`.** `ux_users_id_company` se creó con SQL porque `company_id` admite nulos y no puede ser clave alternativa en EF Core; por lo mismo, la FK `tickets(requester_id, company_id)` del invariante 1 tendrá que crearse con SQL en la migración de [[hu-013-radicar-ticket]], no con `HasPrincipalKey` → [[persistencia-postgresql]].
- [x] ~~**Comprobar en Compose la migración al arrancar.**~~ Comprobado el 2026-10-12 al reconstruir `backend` → [[entorno-docker]].
- [ ] **Idioma del `400` por JSON mal formado.** El título es el del framework («Bad Request», en inglés); el contrato T-01 no lo fija. Decidir si se traduce como el resto de errores → [[autenticacion-identity]].
- [ ] **ProblemDetails de `UseStatusCodePages` y del manejador de excepciones con `Accept` no JSON.** Con `Accept: text/html`, un `400` generado por el framework sale como `text/plain`, y el `401` del login es `problem+json` sin `traceId`. No filtra información; unificar el formato → [[autenticacion-identity]].
- [ ] **Aviso de `dotnet watch` en el contenedor `backend`:** no puede leer `obj\Debug/net10.0/staticwebassets.development.json` (separador de Windows en el `obj` compartido con el host). No impide el arranque → [[entorno-docker]].

Autorización, detectado al cerrar B4 del goal del login (2026-10-12). No bloquea B4; conviene resolverlo antes de [[hu-009-administrar-usuarios-internos-y-equipos]], que es la primera HU que escribe roles desde la aplicación:

- [x] ~~**Una fila de tipo rol en `identity.user_claims` llega a la cookie como rol.**~~ Resuelto el 2026-10-12 dentro de B4 (hallazgo MEDIO de `quality-reviewer`): la fábrica emite una lista cerrada de claims y los roles salen solo de `identity.user_roles` → [[autenticacion-identity]].
- [ ] **Variante estricta del manejador de la política de reserva.** Hoy una petición sin endpoint sigue su curso (y termina en `404`); la alternativa es responder `404` sin continuar cuando no hay endpoint y la autorización no tuvo éxito. Cierra del todo el hueco del middleware sin endpoint, pero exige un punto de extensión en el Api para que el host de pruebas registre sus `TestEndpoints` en la tabla de rutas real. Mitigado por ahora con la regla «todo contenido es un endpoint» y sus dos pruebas → [[autenticacion-identity]].

Entorno y datos de desarrollo, detectado en B7 del goal del login (2026-10-12):

- [x] ~~**`appsettings.Development.json` viajaba en la imagen `runtime` del backend**~~ con la clave del sembrador activa y su contraseña sintética. Resuelto el 2026-10-12 (MEDIO de `quality-reviewer` en B7): `DataTicket.Api.csproj` lo excluye de la publicación (`CopyToPublishDirectory="Never"`); comprobado con `dotnet publish` (solo sale `appsettings.json`) → [[entorno-docker]].
- [ ] **Reconciliar el sembrador mínimo con [[hu-010-datos-sinteticos-de-desarrollo]]** en su T-01: dónde se activa `Seed:Enabled` (hoy `appsettings.Development.json`; la HU propone `Seed__Enabled` en Compose), la interfaz `IDevelopmentSeeder` que propone la HU, y el sello de seguridad aleatorio frente al determinista de `db.sql` §13.
- [ ] **Rol que migra en un despliegue real**: desde PostgreSQL 15 solo el dueño de la base crea objetos en `public`; si se migra con otro rol necesitará `CREATE` sobre `public` (historial) además de poder crear los esquemas `identity` y `dataticket` → [[persistencia-postgresql]].
- [ ] **Residuo local**: las bases migradas con B3 antes del arreglo del historial conservan una tabla vacía `dataticket."__EFMigrationsHistory"`; inofensiva, desaparece con `docker compose down -v` → [[entorno-docker]].
- [ ] **Sembrador: arranques concurrentes e idempotencia solo por id.** Dos backends que arranquen a la vez sobre una base vacía harían caer al segundo con `23505` (sin duplicados: la transacción revierte y el reinicio omite lo existente); y una cuenta o empresa con el mismo correo o nombre pero otro id impediría arrancar en Development. Tenerlo en cuenta al ampliar el sembrador en [[hu-010-datos-sinteticos-de-desarrollo]] (p. ej. `pg_advisory_xact_lock`). Además, ninguna prueba fija las fechas `created_at` de `db.sql` §13.
- [ ] **`/ingresar` con el backend caído**: si falla el `/me` inicial se muestra «Algo salió mal / Reintentar» en vez del formulario; es razonable, pero solo tiene prueba el fallo tras el login → [[frontend-mvc]].
- [ ] **Ratificar las propuestas de B4 en uso** (P-23 de §3b y la contradicción del `403` de `/me` en §1): alcance de la política de reserva (`404` en rutas inexistentes), nombre del claim `display_name` y respuesta de `/me` con perfil no válido → [[autenticacion-identity]].

## 5. Equipo y colaboración

- [ ] Reparto de responsabilidades entre Laura, Juan David y Brayan → [[equipo-del-repositorio]].
- [ ] Usuarios de GitHub del equipo para `CODEOWNERS` y protección de `main` → [[flujo-de-trabajo-github]].

## 6. Fuentes por ingerir

- [ ] `mvp-sistema-tickets.md` (borrador del desarrollador), citado en PRD §2.
- [x] ~~`DataTicket.html` (prototipo)~~ → ingerido el 2026-10-09: [[fuente-prototipo-dataticket-html]], [[sistema-de-diseno]].
- [ ] Logo oficial de Data Global para la entrada y la cabecera (el prototipo deja el espacio vacío).
- [ ] ZIP de diseño del equipo (PRD §2).

Colócalas en `wiki/raw/` y pide su ingesta ([[trabajar-con-el-agente]]).

## Relacionado

- [[fuente-prd-v0-1]] · [[vision-general]] · [[index]] · [[tablero-scrum]]

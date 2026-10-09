---
title: "HU-031 — Recuperar mensajes tras reconexión"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/chat, signalr, arquitectura/frontend, arquitectura/backend]
sources: ["PRD.md §7", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-031", "Recuperar mensajes tras reconexión"]
epica: "[[ep-009-chat-interno-en-tiempo-real]]"
criterios_prd: ["CA-05", "CA-06", "CA-08"]
componentes: ["Frontend (core/realtime, models, controllers, views)", "Hub SignalR", "Backend (Api, pruebas de integración)"]
dificultad: "Medio"
sprint_sugerido: "Sprint 6"
dependencias: ["[[hu-026-historial-del-chat-por-cursor]]", "[[hu-027-unirse-al-chat-del-ticket]]", "[[hu-028-enviar-y-recibir-mensajes]]"]
relacionadas: ["[[hu-030-confirmaciones-de-lectura]]", "[[hu-032-revocar-acceso-al-retirar-participante]]", "[[hu-003-iniciar-y-cerrar-sesion]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-031 — Recuperar mensajes tras reconexión

Cuando la conexión en tiempo real se cae y se recupera, o la persona refresca la página, el chat vuelve a unirse al ticket y recupera desde PostgreSQL los mensajes posteriores a su cursor, sin huecos ni duplicados. Si entretanto perdió el acceso, no recupera nada y lo ve claramente (PRD §7, flujo 6; §14.5).

## Historia de usuario

**COMO** participante interno vigente de un ticket (o Elizabeth, PM)  
**QUIERO** que, si se corta mi conexión o refresco la página, el chat recupere solo lo que me perdí  
**PARA** no tener huecos en la conversación ni mensajes repetidos, y saber en todo momento si estoy en línea

## Contexto

CA-05 exige que, al refrescar o reconectar, los participantes recuperen los mensajes perdidos desde PostgreSQL (PRD §14.5). El flujo del PRD: "después de reconexión, el cliente vuelve a autenticarse, solicita unirse de nuevo y recupera desde PostgreSQL los mensajes posteriores a su cursor. La conexión en tiempo real no reemplaza la consulta histórica" (PRD §7, flujo 6). [[tiempo-real-signalr]] lo concreta: en `onreconnected`, `JoinTicket` de nuevo y `GET .../messages?after=<cursor>`. `withAutomaticReconnect()` ya queda configurado en [[hu-027-unirse-al-chat-del-ticket|HU-027]]; por defecto reintenta a los 0, 2, 10 y 30 segundos y luego cierra la conexión. Esta HU añade la resincronización y su tratamiento en la UI. Además corrige un hueco del flujo inicial: un mensaje creado entre la carga del historial y `JoinTicket` se perdería si no se hace una consulta de alcance tras unirse.

## Alcance

- Resincronización del cliente tras cada `JoinTicket` exitoso (inicial y en `onreconnected`): `GET .../messages?after=<cursor>` repetido mientras `hasNewer`, y fusión sin duplicados (`mergeMessages` de [[hu-026-historial-del-chat-por-cursor|HU-026]]).
- Tratamiento de los eventos `MessageCreated` que llegan durante la resincronización.
- Estados de conexión en la UI: conectando, en línea, reconectando, sin conexión (con "Reconectar" manual), sesión expirada y sin acceso.
- Reunión con todos los tickets abiertos en la pestaña tras reconectar.
- Pruebas de integración en el backend que reproducen la desconexión y la recuperación por cursor, incluido el caso de quien fue retirado mientras estaba desconectado.

## Fuera de alcance

- Cambios en la API de historial o en el hub: se usan los contratos de HU-026 y HU-027 sin modificarlos.
- Modo sin conexión o cola de mensajes salientes: un mensaje no confirmado no se reenvía automáticamente ([[hu-028-enviar-y-recibir-mensajes|HU-028]] conserva el texto en el compositor).
- Recuperación de lecturas perdidas por evento: llegan con el historial (`readReceipts`, [[hu-030-confirmaciones-de-lectura|HU-030]]).
- Backplane y reconexión entre instancias (una sola instancia).

## Requisitos y reglas de negocio

- Los participantes autorizados reciben en tiempo real mientras están conectados y, al refrescar o reconectar, recuperan los mensajes perdidos desde PostgreSQL (PRD §14.5).
- Tras reconectar: volver a autenticarse, volver a unirse y recuperar desde el cursor (PRD §7, flujo 6).
- El servidor valida la autorización vigente en cada unión (PRD §7); una persona retirada no puede reconectarse ni volver a consultar el historial (PRD §7.5, §14.8).
- Validación de pertenencia en todas las rutas de chat e historial (PRD §12).

## Invariantes en juego

- AGENTS §7.3: en la reconexión se revalida la membresía; los grupos no son seguridad.
- AGENTS §7.4: PostgreSQL es el historial; el transporte en tiempo real no lo reemplaza.

## Criterios del PRD cubiertos

- PRD CA-05 (completa, junto con [[hu-028-enviar-y-recibir-mensajes|HU-028]]) → [[criterios-de-aceptacion]]
- PRD CA-06 (parcial: la revocación aplica en conexiones futuras y en las reconexiones) → [[criterios-de-aceptacion]]
- PRD CA-08 (parcial: el retirado no vuelve a consultar el historial al reconectar) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-009-chat-interno-en-tiempo-real]]
- Dependencias: [[hu-026-historial-del-chat-por-cursor|HU-026 — Historial del chat paginado por cursor]] (`after`, `hasNewer`, `mergeMessages`), [[hu-027-unirse-al-chat-del-ticket|HU-027 — Conectarse y unirse al chat del ticket]] (`core/realtime`, `JoinTicket`, estados de conexión), [[hu-028-enviar-y-recibir-mensajes|HU-028 — Enviar y recibir mensajes en tiempo real]] (`MessageCreated`).
- Relacionadas: [[hu-030-confirmaciones-de-lectura|HU-030]] (volver a marcar los visibles tras reconectar), [[hu-032-revocar-acceso-al-retirar-participante|HU-032]] (evento de revocación y "sin acceso"), [[hu-003-iniciar-y-cerrar-sesion|HU-003]] (sesión expirada).
- Decisiones: [[adr-0005-signalr-para-chat]] (aceptada). Pendientes: formato del cursor (propuesto en HU-026); enrutador del frontend para redirigir al login ([[pendientes]] §4).

## Componentes afectados

- Frontend `core/realtime`: exponer `onreconnecting`, `onreconnected`, `onclose` y el reinicio manual; distinguir el 401 de la negociación.
- Frontend `models`: orquestador puro de resincronización (`resyncTicketChat`, nombre propuesto) con dependencias inyectadas (unirse, pedir posteriores) y máquina de estados de conexión.
- Frontend `controllers`: `useTicketChatController` conecta los eventos de conexión con la resincronización.
- Frontend `views`: `ConnectionStatusBanner` (HU-027) ampliado con "Reconectar", "Sesión expirada" y "Ya no tienes acceso a este chat".
- Backend: solo pruebas de integración nuevas; el código del hub y del endpoint ya existe.

## Dificultad

**Nivel:** Medio

**Justificación:** el backend ya está construido. La complejidad está en el cliente: concurrencia entre eventos en vivo y consultas de alcance, paginación hasta agotar `hasNewer`, varios estados de error y pruebas de integración que simulan desconexiones.

## Contrato backend ↔ frontend

No hay contrato nuevo. Se usan los de [[hu-026-historial-del-chat-por-cursor|HU-026]] y [[hu-027-unirse-al-chat-del-ticket|HU-027]]. Esta HU fija la **secuencia de cliente** (propuesta, ratificar en T-01 y documentar en [[tiempo-real-signalr]]):

```text
Carga inicial (también al refrescar):
  1. GET /api/tickets/{id}/messages            → últimos N; cursor = último.cursor
  2. connection.start() (si no está iniciada)
  3. JoinTicket(id)
  4. GET /api/tickets/{id}/messages?after=cursor  (repetir mientras hasNewer)
  5. Fusionar con mergeMessages (dedupe por id, orden (createdAt, id))

onreconnecting → estado "reconectando"; compositor deshabilitado
onreconnected  → por cada ticket abierto: pasos 3–5
onclose        → estado "sin conexión" + botón "Reconectar" (start() y pasos 3–5)

Errores durante la resincronización:
  JoinTicket → HubException ticket_not_accessible  ⇒ estado "sin acceso"; no se piden mensajes
  GET → 404                                         ⇒ estado "sin acceso"
  Negociación → 401                                 ⇒ estado "sesión expirada"; sin reintentos en bucle
```

Los eventos `MessageCreated` que llegan entre los pasos 3 y 5 se fusionan con la misma función. El cursor solo avanza al último mensaje fusionado.

## Tareas de desarrollo

- [ ] **T-01 — Contrato de secuencia** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar la secuencia anterior, el tope de iteraciones de `after` por resincronización (propuesta: 20 páginas, para evitar bucles) y la política de reintentos (`withAutomaticReconnect` por defecto, más "Reconectar" manual). Qué ve la UI con "sin acceso": se oculta el contenido ya cargado (propuesta). Documentar en [[tiempo-real-signalr]].
- [ ] **T-02 — Integración: recuperación por cursor (prueba primero)** · Capa: Backend Api (pruebas) · Dificultad: Medio  
  Descripción: con `WebApplicationFactory<Program>`, PostgreSQL real y dos clientes SignalR .NET: A y B unidos; A se detiene (`StopAsync`); B envía 3 mensajes; A se conecta de nuevo, invoca `JoinTicket` y pide `after` con su último cursor → recibe exactamente esos 3, en orden. Variante con 130 mensajes perdidos y `limit=50` → 3 páginas, sin duplicados.
- [ ] **T-03 — Integración: retirado mientras estaba desconectado (prueba primero)** · Capa: Backend Api (pruebas) · Dificultad: Bajo  
  Descripción: A se detiene; la PM retira a A ([[hu-019-reasignar-y-retirar-participantes|HU-019]]); B envía; A vuelve a conectarse → `JoinTicket` rechazado con `ticket_not_accessible` y `GET after` → 404, sin mensajes en el cuerpo.
- [ ] **T-04 — Orquestador de resincronización** · Capa: Frontend models · Dificultad: Medio  
  Descripción: pruebas Vitest primero con dobles de `join` y `fetchAfter`: sin perdidos → 0 llamadas extra; `hasNewer` → itera hasta agotarlo; respeta el tope; `ticket_not_accessible` → resultado `accessDenied` sin pedir mensajes; 404 → `accessDenied`; evento en vivo intercalado → sin duplicados. Implementar `resyncTicketChat` y la máquina de estados `connecting → connected → reconnecting → connected | disconnected | sessionExpired | accessDenied`.
- [ ] **T-05 — Conexión** · Capa: Frontend core/realtime · Dificultad: Bajo  
  Descripción: exponer suscripciones a `onreconnecting`, `onreconnected` y `onclose`, `restart()` para el botón manual y el reconocimiento del 401 en la negociación (`HttpError.statusCode`) para pasar a `sessionExpired`.
- [ ] **T-06 — Controlador** · Capa: Frontend controllers · Dificultad: Medio  
  Descripción: `useTicketChatController` ejecuta la resincronización tras la unión inicial y en cada `onreconnected` para su ticket, aplica el resultado y expone `connectionStatus`, `reconnect()` y `accessDenied`. Al pasar a `accessDenied` limpia los mensajes en memoria (propuesta T-01). Coordina con HU-030 para volver a marcar los visibles.
- [ ] **T-07 — Vistas** · Capa: Frontend views · Dificultad: Bajo  
  Descripción: `ConnectionStatusBanner` con `role="status"` y los mensajes "Reconectando…", "Sin conexión" + "Reconectar", "Tu sesión expiró. Inicia sesión de nuevo." y "Ya no tienes acceso a este chat". Compositor deshabilitado fuera de "en línea".

## Criterios de aceptación

### CHU-01 — Flujo feliz con dos usuarios: recuperar tras un corte

**Dado** Laura y Brayan unidos al chat del ticket T en navegadores distintos  
**Cuando** Brayan pierde la red (DevTools en "Offline"), Laura envía tres mensajes y Brayan recupera la red  
**Entonces** el indicador de Brayan pasa por "Reconectando…" y vuelve a "En línea", y los tres mensajes aparecen una sola vez, en orden, sin recargar la página.

### CHU-02 — Refrescar recupera desde PostgreSQL

**Dado** Brayan con el ticket T cerrado mientras Laura envía mensajes  
**Cuando** Brayan abre o refresca el detalle de T  
**Entonces** ve los mensajes nuevos cargados desde la API de historial, aunque nunca recibió los eventos.

### CHU-03 — Sin duplicados ni desorden en la resincronización

**Dado** una resincronización en curso  
**Cuando** llega por `MessageCreated` un mensaje que también vuelve en la respuesta `after`, y otro mensaje con el mismo `createdAt` que uno ya mostrado  
**Entonces** cada mensaje aparece una vez y la lista queda ordenada por `(createdAt, id)`.

### CHU-04 — Sin hueco en la carga inicial

**Dado** un mensaje enviado entre la primera consulta de historial de Brayan y su `JoinTicket`  
**Cuando** termina la carga inicial  
**Entonces** ese mensaje aparece gracias a la consulta `after` posterior a la unión.

### CHU-05 — Muchos mensajes perdidos

**Dado** Brayan desconectado mientras se envían 130 mensajes, con `limit` 50  
**Cuando** se reconecta  
**Entonces** el cliente pide páginas `after` hasta `hasNewer: false` y muestra los 130, sin repetir ninguno.

### CHU-06 — Quien fue retirado mientras estaba desconectado no recupera nada

**Dado** Kevin desconectado y retirado de T por Elizabeth mientras tanto  
**Cuando** su cliente se reconecta  
**Entonces** `JoinTicket` falla con `ticket_not_accessible`, la consulta `after` no se hace (o responde 404), no se muestra ningún mensaje posterior al retiro, la vista muestra "Ya no tienes acceso a este chat" y el compositor desaparece.

### CHU-07 — Sesión expirada

**Dado** Brayan con la cookie de sesión caducada durante la desconexión  
**Cuando** el cliente intenta reconectar y la negociación responde 401  
**Entonces** el estado pasa a "Tu sesión expiró. Inicia sesión de nuevo.", sin reintentos en bucle ni mensajes nuevos.

### CHU-08 — Reintentos agotados y reconexión manual

**Dado** el backend detenido (`docker compose stop backend`) más tiempo del que cubren los reintentos automáticos  
**Cuando** se agotan  
**Entonces** se ve "Sin conexión" con el botón "Reconectar" y el compositor deshabilitado. Tras `docker compose start backend` y pulsar "Reconectar", el chat vuelve a "En línea" y recupera lo enviado entretanto.

## Definition of Done

- [ ] DoD-01 — CHU-01 a CHU-08 validados con evidencia registrada en la matriz.
- [ ] DoD-02 — Pruebas de integración escritas primero y en verde con `cd backend && dotnet test` (`WebApplicationFactory<Program>`, cliente SignalR .NET, PostgreSQL real): `Reconexion_RecuperaPerdidosPorCursor`, `Reconexion_130Perdidos_PaginaSinDuplicados`, `Reconexion_RetiradoMientrasDesconectado_NoRecuperaNada`.
- [ ] DoD-03 — Pruebas Vitest en verde con `npm --prefix frontend test`: orquestador de resincronización (sin perdidos, `hasNewer`, tope, `accessDenied`, eventos intercalados) y máquina de estados de conexión.
- [ ] DoD-04 — `npm --prefix frontend run lint` sin errores (ninguna vista importa `core/realtime` ni `@microsoft/signalr`) y `npm --prefix frontend run build` correcto.
- [ ] DoD-05 — `DataTicket.ArchitectureTests` en verde.
- [ ] DoD-06 — `docker compose up --build` levanta el stack. Verificación manual con dos navegadores en `http://localhost:5173`: corte con DevTools en "Offline", parada y arranque del servicio `backend`, y refresco. Los WebSockets atraviesan el proxy `/hubs` con `ws: true`.
- [ ] DoD-07 — Secuencia de reconexión documentada en [[tiempo-real-signalr]] (sección *Reconexión*, con el paso de alcance tras la unión inicial).
- [ ] DoD-08 — Wiki actualizada mediante Notas para la wiki: [[tiempo-real-signalr]], [[chat-interno]] (*Historial para incorporaciones tardías*), [[frontend-mvc]] (estados de conexión en `core/realtime`).
- [ ] DoD-09 — Revisión de `quality-reviewer` sin hallazgos CRÍTICO o ALTO abiertos.
- [ ] DoD-10 — PR revisado y aprobado por otra persona del equipo.
- [ ] DoD-11 — Trazabilidad de la HU y de [[ep-009-chat-interno-en-tiempo-real]] actualizada; en las Notas para la wiki se indica si PRD CA-05 puede marcarse en [[criterios-de-aceptacion]].

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
| DoD-11 | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.

## Notas y decisiones

- **Hueco en el flujo inicial del PRD:** el orden "consultar historial → conectar → unirse" (PRD §7) puede perder un mensaje creado entre la consulta y la unión. Se propone una consulta `after` tras cada unión. Conviene reflejarlo en [[tiempo-real-signalr]].
- **Redirección al login** ante sesión expirada: depende del enrutador del frontend, que sigue abierto ([[pendientes]] §4). Mientras tanto, la vista muestra el mensaje y un enlace a la entrada.
- **Ocultar el contenido ya cargado al perder el acceso** es una propuesta: el PRD dice que el retirado "deja de leer" (PRD §7.5), pero no regula lo que ya está en pantalla.

## Relacionado

- [[ep-009-chat-interno-en-tiempo-real]] · [[tablero-scrum]] · [[criterios-de-aceptacion]]
- [[tiempo-real-signalr]] · [[chat-interno]] · [[frontend-mvc]] · [[entorno-docker]] · [[adr-0005-signalr-para-chat]] · [[estrategia-de-pruebas]]

# Loop — Chat interno en tiempo real con SignalR (EP-009)

Archivo de control del `/loop` que construye **EP-009 (HU-026 a HU-032)**, una rebanada por iteración. Guarda el estado entre iteraciones: Claude lo relee al empezar cada una, porque el loop no conserva memoria fiable (puede compactarse y no se restaura con `--resume`). Las personas del equipo lo abren para ver qué se ejecutó y qué falta.

- Plan y decisiones: `wiki/sintesis/plan-goal-login-y-loop-chat.md`
- Se ejecuta **después** de completar `goal_login.md`
- Épica: `wiki/scrum/epicas/ep-009-chat-interno-en-tiempo-real.md` · Contrato: `wiki/arquitectura/tiempo-real-signalr.md`

**Leyenda:** `[ ]` pendiente · `[~]` en curso · `[x]` hecha y verificada · `[!]` bloqueada (con el motivo en la línea)

---

## 1. Cómo ejecutarlo

1. Comprueba que `goal_login.md` tenga todas sus casillas en `[x]`. Abre Claude Code en la raíz del repo, en **modo auto**. El stack de Docker ya está levantado y Claude ejecuta aquí los comandos de Docker.
2. Pega este comando tal cual. No lleva intervalo: Claude decide cuándo ejecutar la siguiente iteración.

```text
/loop Ejecuta UNA iteración de loop_chat.md siguiendo al pie de la letra su sección «2. Protocolo de cada iteración»: relee el archivo, toma la primera rebanada sin marcar, constrúyela y verifícala, márcala, regístrala, haz su commit y decide si continuar o detener el loop.
```

3. Para ver el avance: este archivo (casillas y «4. Registro de iteraciones») y `git log --oneline`.
4. Para detenerlo: pulsa `Esc` mientras espera la siguiente iteración, o pídele a Claude que lo detenga.

## 2. Protocolo de cada iteración

1. **Estado.** Relee este archivo completo, `git status -sb` y las 5 últimas entradas de `wiki/log.md` (`grep "^## \[" wiki/log.md | tail -5`).
2. **Rama.** Trabaja en `feature/chat-signalr`. Si no existe, créala desde `main` si `feature/auth-login` ya está fusionada; si no, desde `feature/auth-login`.
3. **Precondición (solo si la rebanada P0 no está marcada).** Ejecuta las comprobaciones de P0. Si alguna falla, márcala `[!]` con el motivo y **detén el loop**.
4. **Rebanada.** Toma la primera rebanada sin `[x]` ni `[!]` y márcala `[~]`. Lee su HU (Contrato, Tareas, CHU y DoD), `tiempo-real-signalr` y `sistema-de-diseno` si toca el frontend.
5. **Construcción.**
   - Rebanada de contrato: ratifícalo con las propuestas de `wiki/pendientes.md` §3b (P-01, P-12, P-13, P-14) y documéntalo en `tiempo-real-signalr` y `glosario` antes de usar los nombres en código.
   - Rebanada de código: delega en `backend-engineer` y/o `frontend-engineer` (en paralelo si el contrato ya está fijado), con pruebas primero. Después lanza `quality-reviewer` sobre los archivos tocados y corrige todo hallazgo CRÍTICO o ALTO.
6. **Verificación real:** `cd backend && dotnet test`, `npm --prefix frontend run lint`, `npm --prefix frontend test` y `npm --prefix frontend run build`. En las rebanadas con UI, revisa además en el navegador integrado (tema claro y oscuro, 375 y 1440 px).
7. **Cierre.**
   - Marca `[x]` en cada casilla de la rebanada **solo** si la verificación pasó, con la evidencia en la línea.
   - Actualiza la matriz «Evidencia de validación» de la HU y su `estado` (`En desarrollo` → `En validación` cuando terminen todas sus rebanadas).
   - Actualiza la wiki afectada y añade una entrada en `wiki/log.md`.
   - Añade una fila al «4. Registro de iteraciones».
   - Haz **un commit local por rebanada** (`feat(chat): R2 historial por cursor (HU-026)`, con el trailer `Co-Authored-By` de la sesión). Nunca push, merge ni force-push.
8. **Decisión.** Si quedan rebanadas, programa la siguiente iteración. **Detén el loop** si:
   - a) todas las rebanadas están `[x]` o `[!]`;
   - b) la misma rebanada falla la verificación en 2 iteraciones seguidas (márcala `[!]`);
   - c) aparece una decisión de producto o de arquitectura no resuelta (anótala en `wiki/pendientes.md`, márcala `[!]` y pregúntame);
   - d) el stack de Docker no responde (`docker compose ps`) y no se recupera con `docker compose up -d`.

**Docker, fuente de verdad y datos:**
- **Docker (lo opera Claude desde Claude Code):** el stack de Compose (`db`, `backend`, `frontend`, `mailpit`, `azurite`) ya está levantado. Claude ejecuta aquí todos los comandos (`docker compose ps/logs/up --build`, `curl`, pruebas). Para probar algo que el stack no tiene, crea un **contenedor desechable** (Testcontainers o `docker run --rm`, nombre `dt-*-check`) y elimínalo al terminar. Nunca borra volúmenes ni carga datos de prueba en el servicio `db` de Compose fuera de las migraciones y del sembrador de Development.
- **Fuente de verdad:** el PRD y las HU de Scrum. `DataTicket.html` es solo referencia visual (paleta, tipografía, estética), nunca de comportamiento ni de pantallas.
- **Datos semilla:** la referencia es la sección 13 (DML) de `db.sql`: IDs deterministas, cuentas `Active`/`Invited`/`Deactivated` y contraseña sintética común documentada allí. El sembrador de Development y los fixtures la replican con EF Core.

**Invariantes (no negociables):**
- Los grupos de SignalR no son seguridad: la participación se valida en cada unión y en cada operación.
- Se persiste en PostgreSQL antes de publicar.
- Nada del chat llega al portal del cliente.
- Un administrador no lee el chat por ser administrador.
- No se editan `PRD.md`, `DataTicket.html` ni `wiki/raw/`; no se desactivan pruebas, lint ni el hook. Si un cambio toca el esquema, `db.sql` se actualiza en el mismo commit (ADR-0009).

## 3. Rebanadas

### P0 — Precondición
- [ ] Todas las casillas de `goal_login.md` en `[x]` · Evidencia:
- [ ] En el código existen `GET /api/auth/me`, `ICurrentUser` y `DataTicket.IntegrationTests`; `dotnet test` en verde · Evidencia:
- [ ] Rama `feature/chat-signalr` creada · Evidencia:

### R0 — Dominio mínimo habilitante (adelanto parcial de HU-018/HU-019)
- [ ] `Ticket` y `TicketParticipant` (vigente/retirado), solo lo que exige el chat; si HU-018/019 ya existen en `main`, se reutilizan y se marca «no aplica» · Evidencia:
- [ ] Repositorio EF, configuración y migración coherentes con `db.sql` · Evidencia:
- [ ] Fixtures de prueba y sembrador de Development replicando `db.sql` §13 (T03 con Laura y Juan David vigentes, Brayan retirado, Kevin no participante, Elizabeth, `ana.perez`; mensajes e…01–e…06 con el par de marca de tiempo igual) · Evidencia:
- [ ] Nota en HU-018/019 y en PLAN-DE-TRABAJO: «adelanto parcial, sin UI ni endpoints de triage» · Evidencia:

### R1 — HU-026 · Contrato (T-01)
- [ ] Cursor opaco base64url (`CreatedAt` µs + `Id` Guid v7), `limit` 50/100, `ChatMessageDto` y errores (404 para no visible, `invalid_cursor`) · Evidencia:
- [ ] Documentado en `tiempo-real-signalr` y `glosario` · Evidencia:

### R2 — HU-026 · Backend del historial
- [ ] Dominio `ChatMessage`, regla de acceso y caso de uso `GetChatHistory` con pruebas primero (T-02 a T-04) · Evidencia:
- [ ] Persistencia, migración y `GET /api/tickets/{id}/messages` con `after`, `before` y `limit`, más `Cache-Control: no-store` (T-05, T-06) · Evidencia:
- [ ] CHU-01 a CHU-08 en verde (incorporado tarde, Elizabeth, rechazos, retirado, `after`, orden estable, parámetros, sin sesión) · Evidencia:

### R3 — HU-026 · Frontend del historial
- [ ] Módulo `src/modules/chat`: modelo, gateway, controlador y vistas (T-07 a T-09), con el estilo de `sistema-de-diseno` · Evidencia:
- [ ] Página mínima `/interno/tickets/:id` como anfitriona del chat (propuesta hasta HU-020) · Evidencia:
- [ ] CHU-09 en verde y revisión visual · Evidencia:

### R4 — HU-027 · Backend del hub y la unión
- [ ] Contrato del hub (T-01): `/hubs/tickets`, `JoinTicket`/`LeaveTicket`, códigos de `HubException`, validación de `Origin` (P-13) · Evidencia:
- [ ] Caso de uso de unión, registro conexión→usuario y `TicketsHub` (T-02 a T-04) · Evidencia:
- [ ] Prueba de integración con 2 o más clientes SignalR .NET autenticados; CHU-01 a CHU-07 en verde · Evidencia:

### R5 — HU-027 · Frontend de la conexión
- [ ] `@microsoft/signalr` (versión exacta verificada) y conexión compartida en `core/realtime` con `withAutomaticReconnect` (T-05) · Evidencia:
- [ ] Gateway del hub, controlador e indicador de estado de conexión (T-06 a T-08) · Evidencia:
- [ ] CHU-08 en verde y revisión visual · Evidencia:

### R6 — HU-028 · Backend del envío
- [ ] Reglas de dominio y `SendChatMessage` con pruebas primero (T-02, T-03) · Evidencia:
- [ ] Orden autorizar → persistir → publicar vía `ITicketChatNotifier`; prueba unitaria de que no publica si falla la persistencia · Evidencia:
- [ ] CHU-01 a CHU-07 en verde (dos usuarios, solo el grupo autorizado, revalidación, Elizabeth, validaciones) · Evidencia:

### R7 — HU-028 · Frontend del envío
- [ ] Compositor, recepción de `MessageCreated`, deduplicación por `id` y `parseHubError` (T-06 a T-08) · Evidencia:
- [ ] CHU-08 (texto seguro, sin `innerHTML`) y CHU-09 en verde · Evidencia:
- [ ] Prueba manual con dos navegadores a través del proxy · Evidencia:

### R8 — HU-032 · Revocar acceso al retirar
- [ ] Caso de uso, `IChatConnectionRevoker` y evento `TicketAccessRevoked`; orden persistir → revocar (T-02, T-03) · Evidencia:
- [ ] Integración del hub con varios clientes (T-04); CHU-01 a CHU-08 en verde · Evidencia:
- [ ] Frontend: la vista se vacía y avisa al recibir la revocación (T-05, T-06) · Evidencia:

### R9 — HU-031 · Recuperar mensajes tras reconexión
- [ ] Contrato de secuencia: carga del historial → conexión → `JoinTicket` → consulta `after` (P-12) (T-01) · Evidencia:
- [ ] Pruebas de integración de recuperación y de retiro durante la desconexión (T-02, T-03) · Evidencia:
- [ ] Orquestador de resincronización, `onreconnected`, sin duplicados ni desorden, y reconexión manual (T-04 a T-07) · Evidencia:
- [ ] CHU-01 a CHU-08 en verde · Evidencia:

### R10 — HU-030 · Confirmaciones de lectura
- [ ] Dominio, `MarkMessageRead` idempotente, persistencia y migración (T-02 a T-04) · Evidencia:
- [ ] `MarkAsRead` y `ReadReceiptUpdated` en el hub; lectura de Elizabeth y sin lectura propia (P-14) (T-05) · Evidencia:
- [ ] Frontend: solo marca lo visto y tolera fallos (T-06 a T-08); CHU-01 a CHU-09 en verde · Evidencia:

### R11 — HU-029 · Adjuntos e imágenes en el chat
- [ ] Si no existen `IFileStorage`/`Attachment` (HU-014), marca esta rebanada `[!]`, deja la HU en `Bloqueada` y continúa · Evidencia:
- [ ] Dominio, `UploadChatAttachment`, `SendChatMessage` extendido, Blob (Azurite) y migración (T-02 a T-06) · Evidencia:
- [ ] Frontend con subida, vista previa de imágenes y errores (T-07 a T-09); CHU-01 a CHU-09 en verde · Evidencia:

### R12 — Cierre de EP-009
- [ ] Criterio de completitud de la épica revisado punto por punto · Evidencia:
- [ ] `docker compose up --build -d backend frontend` (ejecutado por Claude): chat con dos pestañas del navegador integrado, refresco, corte de red simulado y retiro de un participante · Evidencia:
- [ ] `quality-reviewer` global de la rama: 0 CRÍTICO y 0 ALTO abiertos · Evidencia:
- [ ] Wiki: `tiempo-real-signalr`, `chat-interno`, `modelo-de-dominio`, `glosario`, `persistencia-postgresql`, `frontend-mvc`; matrices de HU-026 a HU-032; `PLAN-DE-TRABAJO` y `log.md` · Evidencia:
- [ ] Borrador de descripción del PR (HU y CA-05 a CA-10 cubiertos) e informe final · Evidencia:

## 4. Registro de iteraciones

Una fila por iteración. La añade Claude; no se borran filas.

| Fecha | Iteración | Rebanada | Resultado | Commit | Nota |
|---|---|---|---|---|---|
| — | 0 | — | — | — | Loop creado el 2026-10-09, sin ejecutar |

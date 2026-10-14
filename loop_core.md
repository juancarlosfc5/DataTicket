# Loop — Core de DataTicket (tickets, triage, trazabilidad, entrega y cierre)

Archivo de control del `/loop` que construye el **software como tal**: auditoría, empresas y cuentas, radicación con adjuntos, triage, asignación y participantes, detalle interno, estados, URL de PR, bitácora, bandejas, portal del cliente, respuesta formal, cierre manual, notificaciones, dashboard y formularios configurables. Avanza **una HU por iteración**, en el orden de dependencias del tablero Scrum. Corre **en paralelo** con `loop_chat.md`. Este archivo vive en `main`; al ejecutarse, el loop trabaja en su propio worktree, rama y stack de Docker, que se crean desde `main` cuando ya contiene el login.

- Plan y decisiones: `wiki/sintesis/plan-goal-login-y-loop-chat.md` (sección «Loop del core»)
- Tablero: `wiki/scrum/tablero-scrum.md` · Seguimiento: `PLAN-DE-TRABAJO.md` · Datos semilla: `db.sql` §13
- Fuera de este loop: HU-002 a HU-004 (`goal_login.md`) y HU-026 a HU-032 (`loop_chat.md`)

**Leyenda:** `[ ]` pendiente · `[~]` en curso · `[x]` hecha y verificada · `[!]` bloqueada (con el motivo en la línea) · `[-]` no aplica (con el motivo)

---

## 1. Cómo ejecutarlo

1. **Cuándo:** después de fusionar `feature/auth-login` en `main` (goal del login terminado).
2. **Preparar el espacio de trabajo (una sola vez).** Dos sesiones no pueden trabajar a la vez en la misma carpeta, así que el loop del core usa una carpeta propia. En la carpeta principal, con `main` actualizado, pide a Claude o ejecuta:

```bash
git worktree add "../DataGlobal2-core" -b feature/core main
```

   - El loop del chat sigue en la carpeta principal (rama `feature/chat-signalr`).
   - El del core trabaja en `C:\Users\JuanCarlosFlórez\Downloads\6. Projects\DataGlobal2-core` (rama `feature/core`).
   - Las dos ramas se integran a `main` como cualquier rama de trabajo (`AGENTS.md` §8).
3. **Dónde:** abre una sesión **nueva** de Claude Code en la carpeta `DataGlobal2-core`, en **modo auto**.
4. Pega este comando tal cual (sin intervalo: Claude decide cuándo hacer la siguiente iteración):

```text
/loop Ejecuta DOS ITERACIONES de loop_core.md siguiendo al pie de la letra su sección «2. Protocolo de cada iteración»: relee el archivo, sincroniza la rama, toma la primera HU disponible sin marcar, constrúyela y verifícala, márcala, regístrala, haz su commit y decide si continuar, esperar o detener el loop.
```

5. **Seguimiento:** este archivo (casillas, hitos y «5. Registro de iteraciones»), `git log --oneline feature/core` y `PLAN-DE-TRABAJO.md`.
6. **Detenerlo:** pulsa `Esc` mientras espera la siguiente iteración, o pídele a Claude que lo detenga.

## 2. Protocolo de cada iteración

1. **Estado.** Relee este archivo completo, `git status -sb`, `git branch --show-current` (debe ser `feature/core`) y las 5 últimas entradas de `wiki/log.md`.
2. **Sincronización (fusiones locales autorizadas por este archivo).**
   - Mientras P0 no esté hecha, comprueba lo siguiente:
     - que la sesión está en el worktree `DataGlobal2-core` y en la rama `feature/core`. Si estás en la carpeta principal o en otra rama, **detén el loop** e indica el paso 2 de la sección 1;
     - que `main` contiene el login (`goal_login.md` sin casillas `[ ]`). Si aún no, anota «esperando el login en main» en el registro, **no construyas nada** y programa la siguiente iteración en unos 30 minutos.
   - En cada iteración: si `main` avanzó, `git merge main` en `feature/core`, porque ahí entran el login, el chat y las correcciones del equipo. Resuelve los conflictos según la sección 3. Si no puedes resolverlos con seguridad, deja el árbol limpio (`git merge --abort`) y **detén el loop**.
3. **Elegir la HU.** Toma la primera HU sin `[x]`, `[!]` ni `[-]` cuyas dependencias (campo `dependencias` de su nota) estén hechas en esta rama o en `main`. Si sus dependencias no están, salta a la siguiente disponible y vuelve después. Márcala `[~]`.
4. **Leer.** Lee la nota completa de la HU (Contrato, Tareas, CHU, DoD), las páginas de la wiki que cita y `wiki/pendientes.md` §3b.
5. **Construir** (mismo flujo que el orquestador, `AGENTS.md`):
   - T-01: ratifica el contrato con las propuestas de §3b y documéntalo en la página de la wiki correspondiente y en `glosario` antes de usar los nombres en código.
   - Delega en `backend-engineer` y `frontend-engineer` **en paralelo** con ese contrato, con pruebas primero y los nombres exactos del DoD.
   - Lanza `quality-reviewer` sobre los archivos tocados y corrige todo hallazgo CRÍTICO o ALTO.
   - Si la HU no cabe en una iteración, haz commit de lo verificado, deja la HU en `[~]` con lo que falta escrito en su línea y continúa con ella en la siguiente iteración.
6. **Verificar con comandos reales:**
   - `cd backend && dotnet test` (unitarias, de integración con Testcontainers y `ArchitectureTests`);
   - `npm --prefix frontend run lint`, `npm --prefix frontend test` y `npm --prefix frontend run build`;
   - en las HU con UI, prueba manual en el **stack aislado del core** (sección 3) con el navegador integrado: tema claro y oscuro, 375 y 1440 px y teclado; pasa `break-ui` a las vistas con datos variables.
7. **Cerrar.**
   - Marca `[x]` cada casilla **solo** si la verificación pasó, con la evidencia en la línea.
   - Actualiza la matriz «Evidencia de validación» y el `estado` de la HU (`En desarrollo` → `En validación`).
   - En `PLAN-DE-TRABAJO.md`, pon Responsable `Claude (loop_core)`.
   - Actualiza la wiki afectada (tabla de `AGENTS.md` §5.6) y añade una entrada en `wiki/log.md`.
   - Añade una fila al registro (sección 5).
   - Haz **un commit local por HU** (`feat(tickets): HU-013 radicar ticket con prioridad calculada`, con el trailer `Co-Authored-By` de la sesión). Nunca push, nunca fusionar hacia `main` ni force-push: la persona líder fusiona.
8. **Hitos.** Al completar un hito de la sección 4, crea la etiqueta local indicada (`git tag <nombre>`), anótalo en el registro y avísalo en el resumen de la iteración, para que la persona líder fusione `feature/core` en `main` y el loop del chat lo aproveche.
9. **Decisión.** Si quedan HU, programa la siguiente iteración. **Detén el loop** si:
   - a) todas las HU están `[x]`, `[!]` o `[-]`;
   - b) la misma HU falla la verificación en 2 iteraciones seguidas (márcala `[!]`);
   - c) aparece una decisión de producto o de arquitectura no resuelta (anótala en `wiki/pendientes.md`, marca `[!]` y pregunta);
   - d) un conflicto de fusión no se puede resolver con seguridad;
   - e) el stack aislado del core no se puede levantar.

## 3. Reglas del trabajo en paralelo

- **Fuente de verdad:** el PRD y las HU de Scrum. `DataTicket.html` es solo referencia visual. Estilo: `wiki/arquitectura/sistema-de-diseno.md` (estilo Apple + estética del prototipo).
- **Aislamiento de Git:** este loop solo trabaja en el worktree `DataGlobal2-core` y en la rama `feature/core`. Nunca hace `checkout` de otras ramas ni toca la carpeta principal del repo.
- **Stack de Docker aislado (operado por Claude desde Claude Code):** el stack principal (`dataticket`: puertos 5432, 8080, 5173, 8025, 1025 y 10000) es del goal y del loop del chat, y **no se toca**. El core usa su propio proyecto de Compose con otros puertos:
  - Archivo `.env.core` en la raíz del worktree (lo ignora `.gitignore` por `.env.*`), creado en P0: `POSTGRES_PORT=55432`, `BACKEND_PORT=18080`, `FRONTEND_PORT=15173`, `MAILPIT_UI_PORT=18025`, `MAILPIT_SMTP_PORT=11025`, `AZURITE_BLOB_PORT=11000`. Las demás variables se copian de `.env.example`.
  - Comando: `docker compose -p dataticket-core --env-file .env.core up -d --build`. URLs: app `http://localhost:15173`, API `http://localhost:18080`, Mailpit `http://localhost:18025`.
  - Las pruebas de integración usan Testcontainers (contenedores desechables). Cualquier otro contenedor de prueba se llama `dt-core-*-check` y se elimina al terminar.
- **Propiedad del código** (evita que los dos loops escriban lo mismo):

| Dueño | Áreas |
|---|---|
| `loop_core` | Companies, Users y cuentas, Tickets (agregado, prioridad, estados, PR), TicketParticipant, Assignment, AuditEntry/`IAuditLog`, Attachment/`IFileStorage` (radicación y respuesta formal), FormalResponse, Notifications por correo, Dashboard y FormTemplates; módulos frontend `tickets`, `triage`, `portal`, `admin`, `dashboard` y `forms` |
| `loop_chat` | ChatMessage, MessageReadReceipt, `TicketsHub`, `ITicketChatNotifier`, adaptador real de `IChatConnectionRevoker`, adjuntos de contexto `Chat`; `core/realtime` y el módulo frontend `chat` |
| Compartidos (solo cambios aditivos y mínimos) | `Program.cs`, `DependencyInjection.cs` (un método de extensión por módulo, p. ej. `AddTicketsModule()`), `DataTicketDbContext` (una línea `DbSet` y su configuración en su propio archivo), rutas de `src/app`, `glosario`, `index` y `log` de la wiki |

- **Puntos de contacto con el chat:**
  - Hito N1: el núcleo compartido lo construye este loop.
  - HU-019: `RemoveParticipant` invoca el puerto `IChatConnectionRevoker`; aquí se registra un adaptador nulo que el chat reemplaza en HU-032.
  - HU-020: el detalle interno deja un **espacio para el panel del chat** (componente o ranura) que el chat ocupa.
  - HU-037 depende del chat (HU-027, HU-028 y HU-032 en `main`); si aún no está, queda `[!]` y se retoma al final.
- **Migraciones de EF Core:** una por HU, con nombre `HuNNN_<Descripcion>`, siempre coherente con `db.sql` (si cambia el esquema, actualiza `db.sql` en el mismo commit). Si al fusionar `main` hay conflicto en el `ModelSnapshot` o en migraciones con la misma marca de tiempo, conserva las de `main`, elimina las migraciones de `feature/core` aún no fusionadas en `main` y regénéralas sobre el modelo combinado; verifica con `dotnet test`.
- **Wiki:** `log.md` se fusiona por unión (`.gitattributes`). En `index.md`, `glosario.md` y las demás páginas, conserva el contenido de ambas ramas (`AGENTS.md` §5.7). Antes de crear un ADR, revisa el último número en `main` y en esta rama.
- **Datos:** el sembrador de Development (HU-010) y los fixtures replican `db.sql` §13 con los mismos IDs. Las credenciales son sintéticas y nunca se escriben en el chat.
- **Prohibido:** editar `PRD.md`, `DataTicket.html` y `wiki/raw/`; desactivar pruebas, lint o el hook `wiki-guard`; publicar al portal del cliente algo interno (invariantes de `AGENTS.md` §7).

## 4. Lista de control

### P0 — Arranque
- [x] Sesión en el worktree `DataGlobal2-core`, rama `feature/core` creada desde `main` con el login ya fusionado · Evidencia: 2026-10-14, worktree `D:\Documentos\Data Global\DataTicket\DataGlobal2-core` sobre `feature/core` (=`main` 2f070b2, `goal_login.md` sin `[ ]`); la carpeta principal volvió a `main`
- [x] `.env.core` creado y stack aislado `dataticket-core` arriba (`docker compose -p dataticket-core ps`) · Evidencia: 5 servicios Up, db y mailpit healthy; `curl :18080/api/health` → Healthy
- [x] `npm --prefix frontend ci` en el worktree; línea base en verde (`dotnet test`, `lint`, `test`, `build`) · Evidencia: oxlint 0 hallazgos; vitest 152/152; build OK; `dotnet test` 169/169 (en contenedor desechable `dt-core-tests-check` con SDK 10: la máquina solo tiene SDK 9)
- [x] Primer commit de `feature/core` con este archivo marcado (P0) · Evidencia: commit `chore(core): P0 arranque del loop del core`

### Etapa A — Fundaciones (Fase 0 restante + auditoría)
- [!] **HU-001** (diferida a la fase final por decisión de la persona líder el 2026-10-14: el CI no debe interferir con el loop del core; no bloquea las HU que la citan como dependencia) Integración continua en cada PR (GitHub Actions: backend con PostgreSQL, lint/test/build del frontend) · Contrato · Implementación · Workflow validado localmente (`act` o revisión de sintaxis) · Evidencia:
- [x] **HU-011** Registrar eventos auditables (`IAuditLog`, `AuditEntry` append-only con trigger, transacción con la acción) · Contrato · Backend · Pruebas CHU · Revisión · Evidencia: `dotnet test` 202/202; migración `Hu011_AddAuditEntries` aplicada en `dataticket-core` (Healthy); quality-reviewer 0 CRÍTICO/ALTO
- [ ] **N1 — Núcleo compartido de tickets** (adelanto de HU-013/018/019; desbloquea `loop_chat`):
  - [ ] `Ticket` (campos de `db.sql` §6), `TicketParticipant` (vigente/retirado), `Assignment`; repositorios, configuraciones y migración `HuN1_NucleoTickets` (incluye la FK `fk_audit_entries_ticket` pendiente de HU-011) · Evidencia:
  - [ ] Regla de acceso interno reutilizable («participante vigente o ProductManager»), con pruebas, y puerto `IChatConnectionRevoker` con adaptador nulo · Evidencia:
  - [ ] Fixtures de integración con T03 de `db.sql` §13 (Laura y Juan David vigentes, Brayan retirado, Kevin no participante) · Evidencia:
  - [ ] Etiqueta `core-nucleo-v1` creada y aviso en el resumen: «fusionar `feature/core` en `main` para que el chat use el núcleo» · Evidencia:
- [ ] **HU-007** Administrar empresas cliente · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-005** Invitar y activar cuentas (correo a Mailpit del stack del core; enlace con `App:PublicBaseUrl`) · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-008** Administrar usuarios de empresas cliente (CA-01) · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-009** Administrar usuarios internos, equipos y roles · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-010** Datos sintéticos de desarrollo (sembrador que replica `db.sql` §13, idempotente, solo Development) · Backend · Verificación en el stack del core · Evidencia:
- [ ] **HU-006** Restablecer contraseña sin enumeración · Contrato · Backend · Frontend · Revisión · Evidencia:

### Etapa B — Radicación, triage y participantes (Fase 1, sprints 2–3)
- [ ] **HU-013** Radicar ticket con formulario común y prioridad calculada (amplía el núcleo N1) · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-014** Adjuntos al radicar (`IFileStorage` sobre Azurite; tipos, 10 MB y firma validados en backend) · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-016** Cola global de triage de la PM · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-020** Detalle interno del ticket (con el espacio para el panel del chat; se amplía en las HU siguientes) · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-018** Asignar y agregar participantes · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-019** Reasignar y retirar participantes (invoca `IChatConnectionRevoker`) · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-017** Cola de cobertura y tomar ticket (el administrador queda asociado antes de cargar el detalle) · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-015** Ajustar manualmente la prioridad (con motivo; el cliente no ve el ajuste) · Contrato · Backend · Frontend · Revisión · Evidencia:

### Etapa C — Trazabilidad, estados, bandejas y portal (Fase 1, sprint 4)
- [ ] **HU-021** Cambiar estado interno · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-022** Registrar URL manual de PR (solo interna) · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-012** Consultar la bitácora del ticket · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-023** Bandeja «mis tickets» y de equipo · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-024** Portal: el solicitante consulta sus tickets (sin datos internos) · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-025** Portal: el coordinador consulta los tickets de su empresa · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **Hito F1 — Piloto operativo:** criterio de completitud de la Fase 1 revisado (EP-004 a EP-008), prueba del flujo extremo a extremo en el stack del core, etiqueta `core-fase-1` y aviso para fusionar · Evidencia:

### Etapa D — Entrega formal, cierre y notificaciones (Fase 2 sin chat)
- [ ] **HU-033** Emitir respuesta formal (solo PM o administrador asociado; texto + archivos) · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-034** El cliente consulta la respuesta formal en el portal · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-035** Enviar la respuesta formal por correo (Mailpit del stack del core) · Contrato · Backend · Revisión · Evidencia:
- [ ] **HU-036** Cerrar ticket manualmente (sin cierre automático) · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-038** Correo de vinculación en la primera asociación · Contrato · Backend · Revisión · Evidencia:
- [ ] **HU-037** Notificaciones en la app por mensajes nuevos (requiere HU-027, HU-028 y HU-032 en `main`; si no están, `[!]` y se retoma al final) · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **Hito F2 — Entrega y cierre:** flujo radicar → asignar → estados → respuesta formal → cierre verificado extremo a extremo; etiqueta `core-fase-2` y aviso para fusionar · Evidencia:

### Etapa E — Operación medible (Fase 3, P1)
- [ ] **HU-039** Cálculo de tiempos en días hábiles (America/Bogota, festivos) · Contrato · Backend · Revisión · Evidencia:
- [ ] **HU-040** Panel global de la PM · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-041** Panel del equipo interno · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-042** Resumen de estados en el portal del cliente · Contrato · Backend · Frontend · Revisión · Evidencia:

### Etapa F — Formularios configurables (Fase 4)
- [ ] **HU-043** Constructor de plantillas · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-044** Asociar plantilla a empresa y ordenar campos · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-045** Versionar y publicar plantillas (versiones inmutables) · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-046** Radicar con el formulario de la empresa · Contrato · Backend · Frontend · Revisión · Evidencia:
- [ ] **HU-047** Consultar tickets por campos variables · Contrato · Backend · Frontend · Revisión · Evidencia:

### Cierre
- [ ] `quality-reviewer` global de `feature/core`: 0 CRÍTICO y 0 ALTO abiertos · Evidencia:
- [ ] Criterios de completitud de las épicas EP-003 a EP-008 y EP-010 a EP-013 revisados; `PLAN-DE-TRABAJO.md` y `tablero-scrum` al día · Evidencia:
- [ ] Stack aislado del core detenido (`docker compose -p dataticket-core down`, sin `-v` salvo petición) · Evidencia:
- [ ] Borrador de descripción del PR (HU y CA del PRD cubiertos, páginas de la wiki) e informe final · Evidencia:

## 5. Registro de iteraciones

Una fila por iteración (también las de espera). La añade Claude; no se borran filas.

| Fecha | Iteración | HU / hito | Resultado | Commit | Nota |
|---|---|---|---|---|---|
| — | 0 | — | — | — | Loop creado el 2026-10-09 en `main`, sin ejecutar |
| 2026-10-14 | 1 | P0 | Hecho | chore(core): P0 | Worktree creado, Docker arrancado, línea base verde; `dotnet test` vía contenedor SDK 10 |
| 2026-10-14 | 1 | HU-001 / HU-011 | HU-001 diferida (`[!]`); HU-011 hecha | feat(audit): HU-011 | 202/202; HU-001 aplazada a la fase final por decisión de la persona líder |

-- =============================================================================
-- DataTicket — Diseño de referencia de la base de datos (ADR-0009)
-- Motor: PostgreSQL 18.6 · ORM: EF Core 10 + Npgsql (migraciones code-first)
-- =============================================================================
--
-- Qué es este archivo
--   Diseño de referencia aprobado (wiki/decisiones/adr-0009-db-sql-diseno-de-referencia.md).
--   La base real la crean SOLO las migraciones de EF Core de cada HU; este script no
--   se ejecuta en ningún entorno ni se usa para scaffolding. Sirve para:
--     * guiar el modelo de EF Core (tablas, columnas, CHECK, FK compuestas, índices);
--     * los datos semilla de la sección 13 (DML), referencia del sembrador de
--       Development (HU-010) y de los fixtures de pruebas;
--     * validarse en un contenedor desechable (ver «Cómo validarlo»).
--   Si una HU cambia el esquema, actualiza este archivo en el mismo PR.
--
-- Origen del diseño
--   PRD.md §5–§12, wiki: modelo-de-dominio, glosario, roles-y-permisos, auditoria,
--   archivos-adjuntos, chat-interno, notificaciones, formularios-configurables y
--   las HU aprobadas en wiki/scrum (EP-001…EP-013, HU-001…HU-047).
--   Lo que el PRD no fija y se tomó de una HU o del modelo de dominio está marcado
--   como "(propuesta)" en los comentarios; cambiarlo exige revisar la HU dueña.
--
-- Esquemas
--   identity   → ASP.NET Core Identity (IdentityDbContext<ApplicationUser,
--                IdentityRole<Guid>, Guid>) + claves de Data Protection.
--   dataticket → dominio (empresas, tickets, chat, adjuntos, auditoría, formularios).
--
-- Convenciones
--   * Nombres en snake_case; identificadores de dominio en inglés según el glosario.
--   * Claves primarias uuid con uuidv7() (nativa en PostgreSQL 18): ordenables por
--     tiempo. La aplicación también puede asignarlas (Guid.CreateVersion7()).
--   * Fechas en UTC con timestamptz; la presentación usa America/Bogota.
--   * Enumeraciones de dominio como varchar + CHECK (en EF: HasConversion<string>()),
--     con los valores exactos del glosario (New, InDevelopment, ...).
--   * Sin borrado físico de empresas, usuarios ni tickets: FK con ON DELETE RESTRICT.
--   * updated_at lo asigna la aplicación (IClock); no hay trigger.
--   * Concurrencia optimista: usar la columna de sistema xmin (Npgsql: IsRowVersion())
--     en tickets; no requiere columna propia.
--   * Binarios de adjuntos fuera de la base (Azure Blob / Azurite): solo metadatos.
--
-- Cómo validarlo (contenedor desechable; NUNCA contra el servicio db de Compose)
--   docker run -d --rm --name dt-dbsql-check -e POSTGRES_PASSWORD=check postgres:18.6
--   docker exec -i dt-dbsql-check psql -v ON_ERROR_STOP=1 -U postgres < db.sql
--   docker stop dt-dbsql-check
--
-- Cómo llevarlo a EF Core (guía completa en wiki/arquitectura/persistencia-postgresql.md)
--   * DataTicketDbContext hereda de IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>;
--     las tablas de identity se mapean con ToTable("users", "identity"), etc., y columnas
--     snake_case (EFCore.NamingConventions o HasColumnName).
--   * CHECK → HasCheckConstraint; FK compuestas → HasForeignKey + HasPrincipalKey;
--     índices de expresión, funciones y triggers → migrationBuilder.Sql(...).
--   * Datos semilla (sección 13) → sembrador de Development de HU-010 y fixtures, no HasData
--     (los hashes de contraseña y las fechas deben calcularse en tiempo de ejecución).
--
-- Decisiones abiertas que este script NO resuelve (ver wiki/pendientes.md)
--   * Row-Level Security: no se habilita; el aislamiento multiempresa se aplica en
--     los casos de uso y en filtros globales de EF Core (CA-01).
--   * Roles de base de datos separados (migraciones vs. aplicación): sin GRANT/REVOKE.
--     Mientras la app use el superusuario de Compose, puede desactivar los triggers
--     append-only; separar roles antes de cualquier entorno compartido.
--   * V-06 tipos MIME exactos de adjuntos: no se restringen aquí, solo el tamaño.
--   * V-12 catálogo de categorías y V-16 formato del número: semillas provisionales.
--   * V-13 una sola respuesta formal por ticket (índice único) mientras siga abierta.
--   * Almacenamiento de respuestas variables (Fase 4): jsonb (propuesta, ADR pendiente).
--   * Roles frente a equipos: los equipos se modelan como roles Development/Production.
-- =============================================================================

SET client_encoding = 'UTF8';
SET client_min_messages = warning;

BEGIN;

CREATE SCHEMA IF NOT EXISTS identity;
CREATE SCHEMA IF NOT EXISTS dataticket;

COMMENT ON SCHEMA identity   IS 'ASP.NET Core Identity y claves de Data Protection.';
COMMENT ON SCHEMA dataticket IS 'Dominio de DataTicket: empresas, tickets, chat, adjuntos, auditoría y formularios.';

-- =============================================================================
-- 1. Funciones de dominio
-- =============================================================================

-- Matriz urgencia × impacto (PRD §6.1, wiki matriz-de-prioridad). Simétrica.
CREATE FUNCTION dataticket.priority_matrix(p_urgency varchar, p_impact varchar)
RETURNS varchar
LANGUAGE sql
IMMUTABLE
STRICT
PARALLEL SAFE
AS $$
    SELECT CASE p_impact || ':' || p_urgency
        WHEN 'Low:Low'       THEN 'VeryLow'
        WHEN 'Low:Medium'    THEN 'Low'
        WHEN 'Low:High'      THEN 'Medium'
        WHEN 'Medium:Low'    THEN 'Low'
        WHEN 'Medium:Medium' THEN 'Medium'
        WHEN 'Medium:High'   THEN 'High'
        WHEN 'High:Low'      THEN 'Medium'
        WHEN 'High:Medium'   THEN 'High'
        WHEN 'High:High'     THEN 'Critical'
    END
$$;

COMMENT ON FUNCTION dataticket.priority_matrix(varchar, varchar)
    IS 'CalculatedPriority = matriz(Urgency, Impact) según PRD §6.1.';

-- Número visible del ticket generado en servidor y único bajo concurrencia (HU-013).
-- Formato provisional DT-000001 (V-16): crece en dígitos sin truncar.
CREATE SEQUENCE dataticket.ticket_number_seq AS bigint START WITH 1 INCREMENT BY 1;

CREATE FUNCTION dataticket.next_ticket_number()
RETURNS varchar
LANGUAGE plpgsql
VOLATILE
AS $$
DECLARE
    v_next bigint := nextval('dataticket.ticket_number_seq');
BEGIN
    RETURN 'DT-' || lpad(v_next::text, greatest(6, length(v_next::text)), '0');
END
$$;

COMMENT ON FUNCTION dataticket.next_ticket_number()
    IS 'Genera el número visible del ticket (formato provisional, V-16).';

-- Protección append-only (auditoría, PRD §12, HU-011) e inmutabilidad (versiones de formulario, HU-045).
CREATE FUNCTION dataticket.reject_mutation()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    RAISE EXCEPTION '%.% es inmutable: % no está permitido', TG_TABLE_SCHEMA, TG_TABLE_NAME, TG_OP
        USING ERRCODE = 'insufficient_privilege';
END
$$;

-- =============================================================================
-- 2. Empresas cliente
-- =============================================================================

CREATE TABLE dataticket.companies (
    id              uuid         NOT NULL DEFAULT uuidv7(),
    name            varchar(150) NOT NULL,
    is_active       boolean      NOT NULL DEFAULT true,
    created_at      timestamptz  NOT NULL DEFAULT now(),
    updated_at      timestamptz  NOT NULL DEFAULT now(),
    deactivated_at  timestamptz  NULL,
    CONSTRAINT pk_companies PRIMARY KEY (id),
    CONSTRAINT ck_companies_name_trimmed CHECK (name = btrim(name) AND char_length(name) >= 1),
    CONSTRAINT ck_companies_deactivation CHECK (is_active = (deactivated_at IS NULL))
);

-- Nombre único sin distinguir mayúsculas (HU-007, propuesta).
CREATE UNIQUE INDEX ux_companies_name_ci ON dataticket.companies (lower(name));

COMMENT ON TABLE dataticket.companies IS 'Empresa cliente (Company). Cada ticket pertenece a exactamente una (PRD §12). Se desactiva, no se borra.';

-- =============================================================================
-- 3. ASP.NET Core Identity (esquema identity)
-- =============================================================================

CREATE TABLE identity.users (
    id                      uuid         NOT NULL DEFAULT uuidv7(),
    user_name               varchar(256) NULL,
    normalized_user_name    varchar(256) NULL,
    email                   varchar(256) NULL,
    normalized_email        varchar(256) NULL,
    email_confirmed         boolean      NOT NULL DEFAULT false,
    password_hash           text         NULL,
    security_stamp          text         NULL,
    concurrency_stamp       text         NULL,
    phone_number            text         NULL,
    phone_number_confirmed  boolean      NOT NULL DEFAULT false,
    two_factor_enabled      boolean      NOT NULL DEFAULT false,
    lockout_end             timestamptz  NULL,
    lockout_enabled         boolean      NOT NULL DEFAULT true,
    access_failed_count     integer      NOT NULL DEFAULT 0,
    -- Extensiones de ApplicationUser (HU-003, HU-005, HU-008).
    display_name            varchar(100) NOT NULL,
    account_status          varchar(20)  NOT NULL DEFAULT 'Invited',
    company_id              uuid         NULL,
    created_at              timestamptz  NOT NULL DEFAULT now(),
    CONSTRAINT pk_users PRIMARY KEY (id),
    -- Destino de la FK compuesta tickets(requester_id, company_id) (invariante 1).
    CONSTRAINT ux_users_id_company UNIQUE (id, company_id),
    CONSTRAINT fk_users_company FOREIGN KEY (company_id)
        REFERENCES dataticket.companies (id) ON DELETE RESTRICT,
    CONSTRAINT ck_users_account_status CHECK (account_status IN ('Invited', 'Active', 'Deactivated')),
    CONSTRAINT ck_users_display_name CHECK (display_name = btrim(display_name) AND char_length(display_name) >= 1),
    CONSTRAINT ck_users_access_failed_count CHECK (access_failed_count >= 0)
);

CREATE UNIQUE INDEX ux_users_normalized_user_name ON identity.users (normalized_user_name);
-- Correo único en todo el sistema (HU-008, propuesta; Identity: RequireUniqueEmail).
CREATE UNIQUE INDEX ux_users_normalized_email ON identity.users (normalized_email)
    WHERE normalized_email IS NOT NULL;
CREATE INDEX ix_users_company_id ON identity.users (company_id) WHERE company_id IS NOT NULL;

COMMENT ON TABLE identity.users IS 'ApplicationUser. Sin registro público: Data Global crea, invita y desactiva cuentas (PRD §5).';
COMMENT ON COLUMN identity.users.company_id IS 'Empresa del usuario cliente (Requester/CompanyCoordinator); NULL para internos. Origen del claim company_id.';
COMMENT ON COLUMN identity.users.account_status IS 'Invited → Active → Deactivated (HU-005, HU-008).';

CREATE TABLE identity.roles (
    id                 uuid         NOT NULL DEFAULT uuidv7(),
    name               varchar(256) NULL,
    normalized_name    varchar(256) NULL,
    concurrency_stamp  text         NULL,
    CONSTRAINT pk_roles PRIMARY KEY (id)
);

CREATE UNIQUE INDEX ux_roles_normalized_name ON identity.roles (normalized_name);

COMMENT ON TABLE identity.roles IS 'IdentityRole<Guid>: ProductManager, Administrator, Development, Production, Requester, CompanyCoordinator.';

CREATE TABLE identity.user_roles (
    user_id  uuid NOT NULL,
    role_id  uuid NOT NULL,
    CONSTRAINT pk_user_roles PRIMARY KEY (user_id, role_id),
    CONSTRAINT fk_user_roles_user FOREIGN KEY (user_id) REFERENCES identity.users (id) ON DELETE CASCADE,
    CONSTRAINT fk_user_roles_role FOREIGN KEY (role_id) REFERENCES identity.roles (id) ON DELETE CASCADE
);

CREATE INDEX ix_user_roles_role_id ON identity.user_roles (role_id);

CREATE TABLE identity.user_claims (
    id           integer GENERATED BY DEFAULT AS IDENTITY,
    user_id      uuid    NOT NULL,
    claim_type   text    NULL,
    claim_value  text    NULL,
    CONSTRAINT pk_user_claims PRIMARY KEY (id),
    CONSTRAINT fk_user_claims_user FOREIGN KEY (user_id) REFERENCES identity.users (id) ON DELETE CASCADE
);

CREATE INDEX ix_user_claims_user_id ON identity.user_claims (user_id);

CREATE TABLE identity.role_claims (
    id           integer GENERATED BY DEFAULT AS IDENTITY,
    role_id      uuid    NOT NULL,
    claim_type   text    NULL,
    claim_value  text    NULL,
    CONSTRAINT pk_role_claims PRIMARY KEY (id),
    CONSTRAINT fk_role_claims_role FOREIGN KEY (role_id) REFERENCES identity.roles (id) ON DELETE CASCADE
);

CREATE INDEX ix_role_claims_role_id ON identity.role_claims (role_id);

-- Inicio de sesión externo: fuera del MVP (SSO/federación, PRD §13); se conserva por compatibilidad con Identity.
CREATE TABLE identity.user_logins (
    login_provider         varchar(128) NOT NULL,
    provider_key           varchar(128) NOT NULL,
    provider_display_name  text         NULL,
    user_id                uuid         NOT NULL,
    CONSTRAINT pk_user_logins PRIMARY KEY (login_provider, provider_key),
    CONSTRAINT fk_user_logins_user FOREIGN KEY (user_id) REFERENCES identity.users (id) ON DELETE CASCADE
);

CREATE INDEX ix_user_logins_user_id ON identity.user_logins (user_id);

CREATE TABLE identity.user_tokens (
    user_id         uuid         NOT NULL,
    login_provider  varchar(128) NOT NULL,
    name            varchar(128) NOT NULL,
    value           text         NULL,
    CONSTRAINT pk_user_tokens PRIMARY KEY (user_id, login_provider, name),
    CONSTRAINT fk_user_tokens_user FOREIGN KEY (user_id) REFERENCES identity.users (id) ON DELETE CASCADE
);

-- PersistKeysToDbContext: las cookies sobreviven reinicios del contenedor (wiki autenticacion-identity).
CREATE TABLE identity.data_protection_keys (
    id             integer GENERATED BY DEFAULT AS IDENTITY,
    friendly_name  text    NULL,
    xml            text    NULL,
    CONSTRAINT pk_data_protection_keys PRIMARY KEY (id)
);

-- =============================================================================
-- 4. Catálogo de categorías (V-12: semilla provisional, administrable como datos)
-- =============================================================================

CREATE TABLE dataticket.categories (
    id          uuid        NOT NULL DEFAULT uuidv7(),
    name        varchar(80) NOT NULL,
    sort_order  integer     NOT NULL DEFAULT 0,
    is_active   boolean     NOT NULL DEFAULT true,
    created_at  timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT pk_categories PRIMARY KEY (id),
    CONSTRAINT ck_categories_name CHECK (name = btrim(name) AND char_length(name) >= 1)
);

CREATE UNIQUE INDEX ux_categories_name_ci ON dataticket.categories (lower(name));

COMMENT ON TABLE dataticket.categories IS 'Category del formulario común (PRD §6.1). Catálogo no definido en el PRD (V-12).';

-- =============================================================================
-- 5. Formularios configurables (Fase 4, PRD §9; HU-043 a HU-047)
-- =============================================================================

CREATE TABLE dataticket.form_templates (
    id                uuid         NOT NULL DEFAULT uuidv7(),
    name              varchar(100) NOT NULL,
    company_id        uuid         NULL,
    draft_fields      jsonb        NOT NULL DEFAULT '[]'::jsonb,
    created_by        uuid         NOT NULL,
    created_at        timestamptz  NOT NULL DEFAULT now(),
    draft_updated_at  timestamptz  NOT NULL DEFAULT now(),
    CONSTRAINT pk_form_templates PRIMARY KEY (id),
    CONSTRAINT fk_form_templates_company FOREIGN KEY (company_id)
        REFERENCES dataticket.companies (id) ON DELETE RESTRICT,
    CONSTRAINT fk_form_templates_created_by FOREIGN KEY (created_by)
        REFERENCES identity.users (id) ON DELETE RESTRICT,
    CONSTRAINT ck_form_templates_name CHECK (name = btrim(name) AND char_length(name) >= 1),
    CONSTRAINT ck_form_templates_draft_fields CHECK (jsonb_typeof(draft_fields) = 'array')
);

-- Una plantilla por empresa (HU-044).
CREATE UNIQUE INDEX ux_form_templates_company_id ON dataticket.form_templates (company_id)
    WHERE company_id IS NOT NULL;

COMMENT ON TABLE dataticket.form_templates IS 'FormTemplate: plantilla de formulario por empresa; el borrador vive en draft_fields.';
COMMENT ON COLUMN dataticket.form_templates.draft_fields IS 'Arreglo de FormField {key,label,type,required,helpText,options}; type ∈ ShortText|LongText|Number|Date|SingleChoice|MultipleChoice (propuesta HU-043).';

CREATE TABLE dataticket.form_template_versions (
    id                uuid        NOT NULL DEFAULT uuidv7(),
    form_template_id  uuid        NOT NULL,
    version_number    integer     NOT NULL,
    fields            jsonb       NOT NULL,
    published_by      uuid        NOT NULL,
    published_at      timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT pk_form_template_versions PRIMARY KEY (id),
    CONSTRAINT fk_form_template_versions_template FOREIGN KEY (form_template_id)
        REFERENCES dataticket.form_templates (id) ON DELETE RESTRICT,
    CONSTRAINT fk_form_template_versions_published_by FOREIGN KEY (published_by)
        REFERENCES identity.users (id) ON DELETE RESTRICT,
    CONSTRAINT ux_form_template_versions_number UNIQUE (form_template_id, version_number),
    CONSTRAINT ck_form_template_versions_number CHECK (version_number >= 1),
    CONSTRAINT ck_form_template_versions_fields CHECK (jsonb_typeof(fields) = 'array')
);

CREATE TRIGGER trg_form_template_versions_immutable
    BEFORE UPDATE OR DELETE ON dataticket.form_template_versions
    FOR EACH ROW EXECUTE FUNCTION dataticket.reject_mutation();

COMMENT ON TABLE dataticket.form_template_versions IS 'Instantánea inmutable publicada; cada ticket conserva la versión con que se creó (PRD §9, HU-045).';

-- =============================================================================
-- 6. Tickets
-- =============================================================================

CREATE TABLE dataticket.tickets (
    id                        uuid          NOT NULL DEFAULT uuidv7(),
    number                    varchar(24)   NOT NULL DEFAULT dataticket.next_ticket_number(),
    company_id                uuid          NOT NULL,
    requester_id              uuid          NOT NULL,
    category_id               uuid          NOT NULL,
    title                     varchar(120)  NOT NULL,
    description               text          NOT NULL,
    urgency                   varchar(10)   NOT NULL,
    impact                    varchar(10)   NOT NULL,
    calculated_priority       varchar(10)   NOT NULL,
    current_priority          varchar(10)   NOT NULL,
    status                    varchar(20)   NOT NULL DEFAULT 'New',
    pull_request_url          varchar(2048) NULL,
    form_template_version_id  uuid          NULL,
    custom_field_values       jsonb         NULL,
    created_at                timestamptz   NOT NULL DEFAULT now(),
    updated_at                timestamptz   NOT NULL DEFAULT now(),
    closed_at                 timestamptz   NULL,
    closed_by                 uuid          NULL,
    CONSTRAINT pk_tickets PRIMARY KEY (id),
    CONSTRAINT ux_tickets_number UNIQUE (number),
    CONSTRAINT fk_tickets_company FOREIGN KEY (company_id)
        REFERENCES dataticket.companies (id) ON DELETE RESTRICT,
    -- El solicitante pertenece a la empresa del ticket; además impide mover de empresa
    -- a un usuario que ya tiene tickets (PRD §5, invariante 1, HU-008).
    CONSTRAINT fk_tickets_requester_company FOREIGN KEY (requester_id, company_id)
        REFERENCES identity.users (id, company_id) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_tickets_category FOREIGN KEY (category_id)
        REFERENCES dataticket.categories (id) ON DELETE RESTRICT,
    CONSTRAINT fk_tickets_form_template_version FOREIGN KEY (form_template_version_id)
        REFERENCES dataticket.form_template_versions (id) ON DELETE RESTRICT,
    CONSTRAINT fk_tickets_closed_by FOREIGN KEY (closed_by)
        REFERENCES identity.users (id) ON DELETE RESTRICT,
    CONSTRAINT ck_tickets_title CHECK (title = btrim(title) AND char_length(title) BETWEEN 1 AND 120),
    CONSTRAINT ck_tickets_description CHECK (char_length(btrim(description)) BETWEEN 1 AND 10000),
    CONSTRAINT ck_tickets_urgency CHECK (urgency IN ('Low', 'Medium', 'High')),
    CONSTRAINT ck_tickets_impact CHECK (impact IN ('Low', 'Medium', 'High')),
    CONSTRAINT ck_tickets_calculated_priority
        CHECK (calculated_priority = dataticket.priority_matrix(urgency, impact)),
    CONSTRAINT ck_tickets_current_priority
        CHECK (current_priority IN ('VeryLow', 'Low', 'Medium', 'High', 'Critical')),
    CONSTRAINT ck_tickets_status CHECK (status IN
        ('New', 'InDevelopment', 'PullRequestReview', 'InProduction', 'SolutionDelivered', 'Closed')),
    CONSTRAINT ck_tickets_pull_request_url CHECK (pull_request_url IS NULL OR pull_request_url ~ '^https://'),
    CONSTRAINT ck_tickets_closure CHECK (
        (status = 'Closed') = (closed_at IS NOT NULL) AND (closed_at IS NULL) = (closed_by IS NULL)),
    CONSTRAINT ck_tickets_custom_fields CHECK (
        (form_template_version_id IS NULL AND custom_field_values IS NULL)
        OR (form_template_version_id IS NOT NULL AND jsonb_typeof(custom_field_values) = 'object'))
);

-- Portal del solicitante (HU-024) y del coordinador (HU-025): siempre filtrado por empresa.
CREATE INDEX ix_tickets_company_requester_created ON dataticket.tickets (company_id, requester_id, created_at, id);
CREATE INDEX ix_tickets_company_created ON dataticket.tickets (company_id, created_at, id);
-- Cola de triage y bandejas internas (HU-016, HU-023) y paneles (HU-040).
CREATE INDEX ix_tickets_status_created ON dataticket.tickets (status, created_at, id);
CREATE INDEX ix_tickets_created_at ON dataticket.tickets (created_at);
CREATE INDEX ix_tickets_category_id ON dataticket.tickets (category_id);
CREATE INDEX ix_tickets_requester_id ON dataticket.tickets (requester_id);
CREATE INDEX ix_tickets_form_template_version_id ON dataticket.tickets (form_template_version_id)
    WHERE form_template_version_id IS NOT NULL;
-- Consulta por campos variables (HU-047; propuesta jsonb + GIN, pendiente de ADR).
CREATE INDEX ix_tickets_custom_field_values ON dataticket.tickets
    USING gin (custom_field_values jsonb_path_ops) WHERE custom_field_values IS NOT NULL;

COMMENT ON TABLE dataticket.tickets IS 'Ticket (raíz de agregado). company_id y requester_id salen de la cuenta autenticada y no cambian (PRD §6.1).';
COMMENT ON COLUMN dataticket.tickets.number IS 'Número visible generado en servidor (formato provisional, V-16). Si el agregado lo necesita antes de guardar, el adaptador ejecuta SELECT dataticket.next_ticket_number().';
COMMENT ON COLUMN dataticket.tickets.calculated_priority IS 'CalculatedPriority derivada de la matriz; la base de datos verifica la coherencia.';
COMMENT ON COLUMN dataticket.tickets.current_priority IS 'Prioridad vigente: igual a la calculada salvo PriorityOverride (HU-015, propuesta).';
COMMENT ON COLUMN dataticket.tickets.status IS 'TicketStatus interno. ClientStatus se deriva, no se guarda (nunca se expone al cliente).';
COMMENT ON COLUMN dataticket.tickets.pull_request_url IS 'PullRequestUrl manual e interna; nunca visible al cliente (PRD §5, §6.3).';
COMMENT ON COLUMN dataticket.tickets.custom_field_values IS 'Respuestas variables {key: valor} de la versión de formulario (Fase 4, propuesta jsonb).';

-- Ajuste manual de prioridad (PRD §6.1, HU-015).
CREATE TABLE dataticket.priority_overrides (
    id                   uuid        NOT NULL DEFAULT uuidv7(),
    ticket_id            uuid        NOT NULL,
    calculated_priority  varchar(10) NOT NULL,
    previous_priority    varchar(10) NOT NULL,
    new_priority         varchar(10) NOT NULL,
    reason               text        NOT NULL,
    changed_by           uuid        NOT NULL,
    changed_at           timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT pk_priority_overrides PRIMARY KEY (id),
    CONSTRAINT fk_priority_overrides_ticket FOREIGN KEY (ticket_id)
        REFERENCES dataticket.tickets (id) ON DELETE RESTRICT,
    CONSTRAINT fk_priority_overrides_changed_by FOREIGN KEY (changed_by)
        REFERENCES identity.users (id) ON DELETE RESTRICT,
    CONSTRAINT ck_priority_overrides_values CHECK (
        calculated_priority IN ('VeryLow', 'Low', 'Medium', 'High', 'Critical')
        AND previous_priority IN ('VeryLow', 'Low', 'Medium', 'High', 'Critical')
        AND new_priority IN ('VeryLow', 'Low', 'Medium', 'High', 'Critical')),
    CONSTRAINT ck_priority_overrides_changes CHECK (new_priority <> previous_priority),
    CONSTRAINT ck_priority_overrides_reason CHECK (char_length(btrim(reason)) BETWEEN 1 AND 500)
);

CREATE INDEX ix_priority_overrides_ticket_changed ON dataticket.priority_overrides (ticket_id, changed_at);

COMMENT ON TABLE dataticket.priority_overrides IS 'PriorityOverride: original calculada, anterior, nueva, autor, fecha y motivo obligatorio.';

-- Participantes: conceden acceso a detalle y chat (PRD §5, §6.2). Retirar no borra: marca removed_at.
CREATE TABLE dataticket.ticket_participants (
    id          uuid        NOT NULL DEFAULT uuidv7(),
    ticket_id   uuid        NOT NULL,
    user_id     uuid        NOT NULL,
    origin      varchar(20) NOT NULL,
    added_by    uuid        NOT NULL,
    added_at    timestamptz NOT NULL DEFAULT now(),
    removed_by  uuid        NULL,
    removed_at  timestamptz NULL,
    CONSTRAINT pk_ticket_participants PRIMARY KEY (id),
    CONSTRAINT fk_ticket_participants_ticket FOREIGN KEY (ticket_id)
        REFERENCES dataticket.tickets (id) ON DELETE RESTRICT,
    CONSTRAINT fk_ticket_participants_user FOREIGN KEY (user_id)
        REFERENCES identity.users (id) ON DELETE RESTRICT,
    CONSTRAINT fk_ticket_participants_added_by FOREIGN KEY (added_by)
        REFERENCES identity.users (id) ON DELETE RESTRICT,
    CONSTRAINT fk_ticket_participants_removed_by FOREIGN KEY (removed_by)
        REFERENCES identity.users (id) ON DELETE RESTRICT,
    CONSTRAINT ck_ticket_participants_origin CHECK (origin IN ('Assignment', 'Participant', 'CoverageTake')),
    CONSTRAINT ck_ticket_participants_removal CHECK (
        (removed_at IS NULL) = (removed_by IS NULL) AND (removed_at IS NULL OR removed_at >= added_at))
);

-- Una sola participación vigente por (ticket, persona) (HU-018); el historial conserva las anteriores.
CREATE UNIQUE INDEX ux_ticket_participants_active ON dataticket.ticket_participants (ticket_id, user_id)
    WHERE removed_at IS NULL;
CREATE INDEX ix_ticket_participants_user_removed ON dataticket.ticket_participants (user_id, removed_at);
CREATE INDEX ix_ticket_participants_ticket_id ON dataticket.ticket_participants (ticket_id);

COMMENT ON TABLE dataticket.ticket_participants IS 'TicketParticipant. Vigente = removed_at IS NULL. Origen: Assignment, Participant o CoverageTake (HU-017/018).';

-- Asignación de responsables operativos (PRD §6.2). Asignar implica una participación (propuesta).
CREATE TABLE dataticket.assignments (
    id             uuid        NOT NULL DEFAULT uuidv7(),
    ticket_id      uuid        NOT NULL,
    assignee_id    uuid        NOT NULL,
    assigned_by    uuid        NOT NULL,
    assigned_at    timestamptz NOT NULL DEFAULT now(),
    unassigned_by  uuid        NULL,
    unassigned_at  timestamptz NULL,
    CONSTRAINT pk_assignments PRIMARY KEY (id),
    CONSTRAINT fk_assignments_ticket FOREIGN KEY (ticket_id)
        REFERENCES dataticket.tickets (id) ON DELETE RESTRICT,
    CONSTRAINT fk_assignments_assignee FOREIGN KEY (assignee_id)
        REFERENCES identity.users (id) ON DELETE RESTRICT,
    CONSTRAINT fk_assignments_assigned_by FOREIGN KEY (assigned_by)
        REFERENCES identity.users (id) ON DELETE RESTRICT,
    CONSTRAINT fk_assignments_unassigned_by FOREIGN KEY (unassigned_by)
        REFERENCES identity.users (id) ON DELETE RESTRICT,
    CONSTRAINT ck_assignments_unassignment CHECK (
        (unassigned_at IS NULL) = (unassigned_by IS NULL)
        AND (unassigned_at IS NULL OR unassigned_at >= assigned_at))
);

CREATE UNIQUE INDEX ux_assignments_active ON dataticket.assignments (ticket_id, assignee_id)
    WHERE unassigned_at IS NULL;
-- Carga por responsable y tickets sin asignar (HU-040).
CREATE INDEX ix_assignments_assignee_active ON dataticket.assignments (assignee_id)
    WHERE unassigned_at IS NULL;
CREATE INDEX ix_assignments_ticket_id ON dataticket.assignments (ticket_id);

COMMENT ON TABLE dataticket.assignments IS 'Assignment. Vigente = unassigned_at IS NULL; la reasignación cierra la anterior y abre otra.';

-- =============================================================================
-- 7. Respuesta formal (PRD §6.5, HU-033 a HU-035)
-- =============================================================================

CREATE TABLE dataticket.formal_responses (
    id                     uuid        NOT NULL DEFAULT uuidv7(),
    ticket_id              uuid        NOT NULL,
    body                   text        NOT NULL,
    sent_by                uuid        NOT NULL,
    sent_at                timestamptz NOT NULL DEFAULT now(),
    email_delivery_status  varchar(10) NOT NULL DEFAULT 'Pending',
    email_last_attempt_at  timestamptz NULL,
    CONSTRAINT pk_formal_responses PRIMARY KEY (id),
    CONSTRAINT ux_formal_responses_ticket_id UNIQUE (ticket_id),
    CONSTRAINT ux_formal_responses_id_ticket UNIQUE (id, ticket_id),
    CONSTRAINT fk_formal_responses_ticket FOREIGN KEY (ticket_id)
        REFERENCES dataticket.tickets (id) ON DELETE RESTRICT,
    CONSTRAINT fk_formal_responses_sent_by FOREIGN KEY (sent_by)
        REFERENCES identity.users (id) ON DELETE RESTRICT,
    CONSTRAINT ck_formal_responses_body CHECK (char_length(btrim(body)) BETWEEN 1 AND 10000),
    CONSTRAINT ck_formal_responses_email_delivery CHECK (email_delivery_status IN ('Pending', 'Sent', 'Failed'))
);

COMMENT ON TABLE dataticket.formal_responses IS 'FormalResponse: solo ProductManager o Administrator. Una por ticket mientras V-13 siga abierta.';

-- =============================================================================
-- 8. Chat interno (PRD §7, HU-026 a HU-032)
-- =============================================================================

CREATE TABLE dataticket.chat_messages (
    id          uuid        NOT NULL DEFAULT uuidv7(),
    ticket_id   uuid        NOT NULL,
    author_id   uuid        NOT NULL,
    body        text        NOT NULL DEFAULT '',
    created_at  timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT pk_chat_messages PRIMARY KEY (id),
    CONSTRAINT ux_chat_messages_id_ticket UNIQUE (id, ticket_id),
    CONSTRAINT fk_chat_messages_ticket FOREIGN KEY (ticket_id)
        REFERENCES dataticket.tickets (id) ON DELETE RESTRICT,
    CONSTRAINT fk_chat_messages_author FOREIGN KEY (author_id)
        REFERENCES identity.users (id) ON DELETE RESTRICT,
    -- Puede ir vacío solo si lleva adjuntos (regla de aplicación, HU-029).
    CONSTRAINT ck_chat_messages_body CHECK (body = btrim(body) AND char_length(body) <= 4000)
);

-- Historial y recuperación tras reconexión por cursor (created_at, id) (HU-026, HU-031).
CREATE INDEX ix_chat_messages_ticket_cursor ON dataticket.chat_messages (ticket_id, created_at, id);
CREATE INDEX ix_chat_messages_author_id ON dataticket.chat_messages (author_id);

COMMENT ON TABLE dataticket.chat_messages IS 'ChatMessage: se persiste antes de publicarse por SignalR (PRD §7.2). Inmutable (propuesta, V-14).';

CREATE TABLE dataticket.message_read_receipts (
    chat_message_id  uuid        NOT NULL,
    user_id          uuid        NOT NULL,
    read_at          timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT pk_message_read_receipts PRIMARY KEY (chat_message_id, user_id),
    CONSTRAINT fk_message_read_receipts_message FOREIGN KEY (chat_message_id)
        REFERENCES dataticket.chat_messages (id) ON DELETE RESTRICT,
    CONSTRAINT fk_message_read_receipts_user FOREIGN KEY (user_id)
        REFERENCES identity.users (id) ON DELETE RESTRICT
);

CREATE INDEX ix_message_read_receipts_user_id ON dataticket.message_read_receipts (user_id);

COMMENT ON TABLE dataticket.message_read_receipts IS 'MessageReadReceipt: uno por (mensaje, persona), con fecha/hora (PRD §7, CA-07).';

-- =============================================================================
-- 9. Adjuntos (PRD §8, HU-014, HU-029, HU-033)
-- =============================================================================

CREATE TABLE dataticket.attachments (
    id                  uuid          NOT NULL DEFAULT uuidv7(),
    ticket_id           uuid          NOT NULL,
    context             varchar(20)   NOT NULL,
    chat_message_id     uuid          NULL,
    formal_response_id  uuid          NULL,
    file_name           varchar(255)  NOT NULL,
    content_type        varchar(150)  NOT NULL,
    size_bytes          bigint        NOT NULL,
    blob_name           varchar(300)  NOT NULL,
    url                 varchar(2048) NOT NULL,
    uploaded_by         uuid          NOT NULL,
    uploaded_at         timestamptz   NOT NULL DEFAULT now(),
    CONSTRAINT pk_attachments PRIMARY KEY (id),
    CONSTRAINT ux_attachments_blob_name UNIQUE (blob_name),
    CONSTRAINT fk_attachments_ticket FOREIGN KEY (ticket_id)
        REFERENCES dataticket.tickets (id) ON DELETE RESTRICT,
    -- FK compuestas: el mensaje o la respuesta pertenecen al mismo ticket del adjunto.
    CONSTRAINT fk_attachments_chat_message FOREIGN KEY (chat_message_id, ticket_id)
        REFERENCES dataticket.chat_messages (id, ticket_id) ON DELETE RESTRICT,
    CONSTRAINT fk_attachments_formal_response FOREIGN KEY (formal_response_id, ticket_id)
        REFERENCES dataticket.formal_responses (id, ticket_id) ON DELETE RESTRICT,
    CONSTRAINT fk_attachments_uploaded_by FOREIGN KEY (uploaded_by)
        REFERENCES identity.users (id) ON DELETE RESTRICT,
    CONSTRAINT ck_attachments_context CHECK (context IN ('Submission', 'Chat', 'FormalResponse')),
    -- El adjunto del chat se sube antes de enviar el mensaje y se vincula al enviarlo (HU-029).
    CONSTRAINT ck_attachments_context_links CHECK (
        (context = 'Submission'     AND chat_message_id IS NULL AND formal_response_id IS NULL)
        OR (context = 'Chat'           AND formal_response_id IS NULL)
        OR (context = 'FormalResponse' AND chat_message_id IS NULL AND formal_response_id IS NOT NULL)),
    -- Máximo 10 MB por archivo, validado también en backend (PRD §8, CA-09).
    CONSTRAINT ck_attachments_size CHECK (size_bytes BETWEEN 1 AND 10485760),
    CONSTRAINT ck_attachments_file_name CHECK (char_length(btrim(file_name)) >= 1)
);

CREATE INDEX ix_attachments_ticket_context ON dataticket.attachments (ticket_id, context);
CREATE INDEX ix_attachments_chat_message_id ON dataticket.attachments (chat_message_id)
    WHERE chat_message_id IS NOT NULL;
CREATE INDEX ix_attachments_formal_response_id ON dataticket.attachments (formal_response_id)
    WHERE formal_response_id IS NOT NULL;

COMMENT ON TABLE dataticket.attachments IS 'Attachment: solo metadatos; el binario vive en Azure Blob con URL pública (ADR-0006). Los de contexto Chat nunca se exponen al cliente.';

-- =============================================================================
-- 10. Notificaciones en la app (PRD §11, HU-037)
-- =============================================================================

CREATE TABLE dataticket.notifications (
    id                 uuid        NOT NULL DEFAULT uuidv7(),
    recipient_user_id  uuid        NOT NULL,
    ticket_id          uuid        NOT NULL,
    chat_message_id    uuid        NOT NULL,
    created_at         timestamptz NOT NULL DEFAULT now(),
    read_at            timestamptz NULL,
    CONSTRAINT pk_notifications PRIMARY KEY (id),
    CONSTRAINT ux_notifications_recipient_message UNIQUE (recipient_user_id, chat_message_id),
    CONSTRAINT fk_notifications_recipient FOREIGN KEY (recipient_user_id)
        REFERENCES identity.users (id) ON DELETE RESTRICT,
    CONSTRAINT fk_notifications_ticket FOREIGN KEY (ticket_id)
        REFERENCES dataticket.tickets (id) ON DELETE RESTRICT,
    CONSTRAINT fk_notifications_chat_message FOREIGN KEY (chat_message_id, ticket_id)
        REFERENCES dataticket.chat_messages (id, ticket_id) ON DELETE RESTRICT
);

CREATE INDEX ix_notifications_recipient_read ON dataticket.notifications (recipient_user_id, read_at, created_at);
CREATE INDEX ix_notifications_ticket_id ON dataticket.notifications (ticket_id);
CREATE INDEX ix_notifications_chat_message_id ON dataticket.notifications (chat_message_id);

COMMENT ON TABLE dataticket.notifications IS 'Notificación in-app por mensaje nuevo del chat; sin correo por mensaje (PRD §7, §11).';

-- =============================================================================
-- 11. Auditoría append-only (PRD §12, HU-011, HU-012)
-- =============================================================================

CREATE TABLE dataticket.audit_entries (
    id           uuid        NOT NULL DEFAULT uuidv7(),
    ticket_id    uuid        NULL,
    actor_id     uuid        NOT NULL,
    occurred_at  timestamptz NOT NULL DEFAULT now(),
    action       varchar(60) NOT NULL,
    object_type  varchar(40) NOT NULL,
    object_id    uuid        NOT NULL,
    old_value    jsonb       NULL,
    new_value    jsonb       NULL,
    result       varchar(20) NOT NULL DEFAULT 'Succeeded',
    CONSTRAINT pk_audit_entries PRIMARY KEY (id),
    CONSTRAINT fk_audit_entries_ticket FOREIGN KEY (ticket_id)
        REFERENCES dataticket.tickets (id) ON DELETE RESTRICT,
    CONSTRAINT fk_audit_entries_actor FOREIGN KEY (actor_id)
        REFERENCES identity.users (id) ON DELETE RESTRICT,
    CONSTRAINT ck_audit_entries_action CHECK (char_length(btrim(action)) >= 1),
    CONSTRAINT ck_audit_entries_object_type CHECK (char_length(btrim(object_type)) >= 1),
    -- V-11: la columna existe desde el inicio aunque aún no se decida si se auditan denegaciones.
    CONSTRAINT ck_audit_entries_result CHECK (result IN ('Succeeded', 'Denied', 'Failed'))
);

-- Bitácora del ticket en orden determinista (HU-012).
CREATE INDEX ix_audit_entries_ticket_cursor ON dataticket.audit_entries (ticket_id, occurred_at, id);
CREATE INDEX ix_audit_entries_actor_occurred ON dataticket.audit_entries (actor_id, occurred_at);
-- Auditoría de objetos sin ticket (empresas, usuarios, plantillas).
CREATE INDEX ix_audit_entries_object ON dataticket.audit_entries (object_type, object_id, occurred_at);

CREATE TRIGGER trg_audit_entries_no_update_delete
    BEFORE UPDATE OR DELETE ON dataticket.audit_entries
    FOR EACH ROW EXECUTE FUNCTION dataticket.reject_mutation();

CREATE TRIGGER trg_audit_entries_no_truncate
    BEFORE TRUNCATE ON dataticket.audit_entries
    FOR EACH STATEMENT EXECUTE FUNCTION dataticket.reject_mutation();

COMMENT ON TABLE dataticket.audit_entries IS 'AuditEntry append-only: actor, fecha, acción, objeto, valores anterior/nuevo (jsonb camelCase) y resultado (PRD §12).';
COMMENT ON COLUMN dataticket.audit_entries.action IS 'AuditAction (catálogo en wiki auditoria): TicketSubmitted, PriorityOverridden, StatusChanged, TicketAssigned, TicketReassigned, ParticipantAdded, ParticipantRemoved, FormalResponseDelivered, TicketClosed…';

-- =============================================================================
-- 12. Datos de referencia (catálogos mínimos de toda instalación, también producción)
-- =============================================================================

INSERT INTO identity.roles (name, normalized_name, concurrency_stamp) VALUES
    ('ProductManager',     'PRODUCTMANAGER',     gen_random_uuid()::text),
    ('Administrator',      'ADMINISTRATOR',      gen_random_uuid()::text),
    ('Development',        'DEVELOPMENT',        gen_random_uuid()::text),
    ('Production',         'PRODUCTION',         gen_random_uuid()::text),
    ('Requester',          'REQUESTER',          gen_random_uuid()::text),
    ('CompanyCoordinator', 'COMPANYCOORDINATOR', gen_random_uuid()::text);

-- Semilla provisional (V-12): no sale del PRD; se cambia como datos, sin tocar código.
INSERT INTO dataticket.categories (name, sort_order) VALUES
    ('Incidente',     1),
    ('Requerimiento', 2),
    ('Consulta',      3);

-- =============================================================================
-- 13. DML — Datos semilla de DESARROLLO Y PRUEBAS (referencia para EF Core)
-- =============================================================================
--
-- Propósito
--   Datos sintéticos para probar las HU a medida que se implementan. Igual que el
--   resto del script, es REFERENCIA (ADR-0009): la base real la crean las migraciones
--   de EF Core y estos datos los carga el sembrador de Development de HU-010
--   (IDevelopmentSeeder, solo con Seed:Enabled = true) y los fixtures de las pruebas
--   de integración. NUNCA se cargan en un entorno compartido ni en producción.
--
-- Reglas
--   * Identificadores deterministas (legibles por prefijo) para que pruebas y
--     sembrador sean idempotentes y referenciables:
--       c0000000-…-0000000000NN  empresas (NN = 01…50)
--       a0000000-…-0000000000NN  usuarios internos (01…09)
--       b0000000-…-EEEEEEKKKKKK  usuarios cliente (EEEEEE = empresa, KKKKKK = tipo 1…5)
--       d0…tickets · e0…mensajes · f0…respuestas formales · a1…adjuntos
--       a2…participaciones · a3…asignaciones · a4…ajustes de prioridad
--       a5…plantillas · a6…versiones de plantilla · a7…notificaciones
--   * Correos ficticios: internos en @dataticket.local (P-21, por confirmar) y
--     clientes en @empresa-sintetica-NN.test (dominio reservado .test).
--   * Contraseña común SINTÉTICA de todas las cuentas con contraseña:
--     "Clave-Sintetica-2026" (la misma de los criterios de HU-003). El hash es formato
--     V3 de ASP.NET Core Identity (PBKDF2-HMACSHA512, 100 000 iteraciones, sal fija
--     solo de desarrollo). En el sembrador de EF la contraseña sale de
--     Seed:DefaultPassword y el hash lo calcula PasswordHasher<ApplicationUser>.
--   * Fechas fijas en UTC (Bogotá = UTC−5) para que las métricas sean reproducibles.
--   * Cada bloque indica la HU que lo consume; los textos son ficticios.
--
-- Cobertura de escenarios
--   HU-003/004  cuentas Active, Invited y Deactivated; cliente y todos los roles internos.
--   CA-01       tickets de tres empresas distintas (aislamiento A↔B).
--   HU-016/017  tickets New sin asignar; uno tomado por el administrador (cobertura).
--   HU-018/019  asignaciones, participante retirado y reasignación.
--   HU-015      ajuste manual de prioridad con motivo.
--   HU-021/022  todos los estados internos; URL de PR registrada.
--   HU-026…032  chat con autor retirado, dos mensajes con la misma marca de tiempo
--               (orden estable por id), lecturas, notificaciones y adjunto de chat.
--   HU-033…036  respuestas formales y ticket cerrado manualmente.
--   HU-046      ticket radicado con la plantilla publicada de la Empresa 01.
-- =============================================================================

-- 13.1 Empresas cliente (HU-007, HU-010): 50 sintéticas, de la 46 a la 50 inactivas.
INSERT INTO dataticket.companies (id, name, is_active, created_at, updated_at, deactivated_at)
SELECT format('c0000000-0000-7000-8000-%s', lpad(n::text, 12, '0'))::uuid,
       format('Empresa Sintética %s', lpad(n::text, 2, '0')),
       n <= 45,
       timestamptz '2026-09-01 13:00:00+00',
       CASE WHEN n <= 45 THEN timestamptz '2026-09-01 13:00:00+00' ELSE timestamptz '2026-09-30 15:00:00+00' END,
       CASE WHEN n <= 45 THEN NULL ELSE timestamptz '2026-09-30 15:00:00+00' END
FROM generate_series(1, 50) AS n;

-- 13.2 Usuarios internos de Data Global (PRD §5; HU-009, HU-010). Equipo del piloto + administrador sintético.
INSERT INTO identity.users (id, user_name, normalized_user_name, email, normalized_email, email_confirmed,
                            password_hash, security_stamp, concurrency_stamp, lockout_enabled,
                            display_name, account_status, company_id, created_at)
SELECT u.id::uuid, u.email, upper(u.email), u.email, upper(u.email), true,
       'AQAAAAIAAYagAAAAELal/UjSBYf5CNgFDilKm5nn9oLjvgG/Z1BfyJpPseZMvFiPkQ1+cly0W4hDRB36Ig==',
       upper(replace(u.id, '-', '')), u.id, true,
       u.display_name, 'Active', NULL, timestamptz '2026-09-01 12:00:00+00'
FROM (VALUES
    ('a0000000-0000-7000-8000-000000000001', 'elizabeth@dataticket.local',  'Elizabeth'),
    ('a0000000-0000-7000-8000-000000000002', 'julian@dataticket.local',     'Julián'),
    ('a0000000-0000-7000-8000-000000000003', 'gerardo@dataticket.local',    'Gerardo'),
    ('a0000000-0000-7000-8000-000000000004', 'juan.david@dataticket.local', 'Juan David'),
    ('a0000000-0000-7000-8000-000000000005', 'laura@dataticket.local',      'Laura'),
    ('a0000000-0000-7000-8000-000000000006', 'brayan@dataticket.local',     'Brayan'),
    ('a0000000-0000-7000-8000-000000000007', 'cristian@dataticket.local',   'Cristian'),
    ('a0000000-0000-7000-8000-000000000008', 'kevin@dataticket.local',      'Kevin'),
    ('a0000000-0000-7000-8000-000000000009', 'admin@dataticket.local',      'Administrador DataTicket')
) AS u(id, email, display_name);

-- 13.3 Usuarios cliente (HU-008, HU-010): por empresa, 2 Solicitantes y 1 Coordinador activos;
--      en las empresas 01 a 05, además, una cuenta Invited (sin contraseña) y una Deactivated.
--      La cuenta de ejemplo de HU-003 es ana.perez@empresa-sintetica-01.test (Solicitante 1 de la Empresa 01).
INSERT INTO identity.users (id, user_name, normalized_user_name, email, normalized_email, email_confirmed,
                            password_hash, security_stamp, concurrency_stamp, lockout_enabled,
                            display_name, account_status, company_id, created_at)
SELECT s.id, s.email, upper(s.email), s.email, upper(s.email), s.kind <> 4,
       CASE WHEN s.kind = 4 THEN NULL
            ELSE 'AQAAAAIAAYagAAAAELal/UjSBYf5CNgFDilKm5nn9oLjvgG/Z1BfyJpPseZMvFiPkQ1+cly0W4hDRB36Ig==' END,
       upper(replace(s.id::text, '-', '')), s.id::text, true,
       s.display_name,
       CASE s.kind WHEN 4 THEN 'Invited' WHEN 5 THEN 'Deactivated' ELSE 'Active' END,
       s.company_id, timestamptz '2026-09-01 13:30:00+00'
FROM (
    SELECT format('b0000000-0000-7000-8000-%s%s', lpad(n::text, 6, '0'), lpad(k::text, 6, '0'))::uuid AS id,
           format('c0000000-0000-7000-8000-%s', lpad(n::text, 12, '0'))::uuid                   AS company_id,
           k AS kind,
           CASE
               WHEN n = 1 AND k = 1 THEN 'ana.perez@empresa-sintetica-01.test'
               ELSE format('%s@empresa-sintetica-%s.test',
                           CASE k WHEN 1 THEN 'solicitante1' WHEN 2 THEN 'solicitante2' WHEN 3 THEN 'coordinador'
                                  WHEN 4 THEN 'invitado' ELSE 'desactivado' END,
                           lpad(n::text, 2, '0'))
           END AS email,
           CASE
               WHEN n = 1 AND k = 1 THEN 'Ana Pérez'
               ELSE format('%s Empresa %s',
                           CASE k WHEN 1 THEN 'Solicitante 1' WHEN 2 THEN 'Solicitante 2' WHEN 3 THEN 'Coordinador'
                                  WHEN 4 THEN 'Invitado' ELSE 'Desactivado' END,
                           lpad(n::text, 2, '0'))
           END AS display_name
    FROM generate_series(1, 50) AS n
    CROSS JOIN generate_series(1, 5) AS k
    WHERE k <= 3 OR n <= 5
) AS s;

-- 13.4 Roles (HU-004, HU-009). Elizabeth es ProductManager y además de Producción (PRD §5).
INSERT INTO identity.user_roles (user_id, role_id)
SELECT ur.user_id::uuid, r.id
FROM (VALUES
    ('a0000000-0000-7000-8000-000000000001', 'PRODUCTMANAGER'),
    ('a0000000-0000-7000-8000-000000000001', 'PRODUCTION'),
    ('a0000000-0000-7000-8000-000000000002', 'PRODUCTION'),
    ('a0000000-0000-7000-8000-000000000003', 'PRODUCTION'),
    ('a0000000-0000-7000-8000-000000000004', 'DEVELOPMENT'),
    ('a0000000-0000-7000-8000-000000000005', 'DEVELOPMENT'),
    ('a0000000-0000-7000-8000-000000000006', 'DEVELOPMENT'),
    ('a0000000-0000-7000-8000-000000000007', 'DEVELOPMENT'),
    ('a0000000-0000-7000-8000-000000000008', 'DEVELOPMENT'),
    ('a0000000-0000-7000-8000-000000000009', 'ADMINISTRATOR')
) AS ur(user_id, role_name)
JOIN identity.roles AS r ON r.normalized_name = ur.role_name;

INSERT INTO identity.user_roles (user_id, role_id)
SELECT u.id, r.id
FROM identity.users AS u
JOIN identity.roles AS r
  ON r.normalized_name = CASE WHEN u.email LIKE 'coordinador@%' THEN 'COMPANYCOORDINATOR' ELSE 'REQUESTER' END
WHERE u.company_id IS NOT NULL;

-- 13.5 Plantilla de formulario publicada de la Empresa 01 (Fase 4: HU-043 a HU-046).
INSERT INTO dataticket.form_templates (id, name, company_id, draft_fields, created_by, created_at, draft_updated_at)
VALUES ('a5000000-0000-7000-8000-000000000001', 'Formulario Empresa Sintética 01',
        'c0000000-0000-7000-8000-000000000001',
        '[{"key":"modulo","label":"Módulo afectado","type":"SingleChoice","required":true,"helpText":"Elige el módulo del ERP.","options":["Facturación","Nómina","Inventario"]},
          {"key":"numeroFactura","label":"Número de factura","type":"ShortText","required":false,"helpText":"Solo si aplica.","options":[]},
          {"key":"fechaEvento","label":"Fecha del evento","type":"Date","required":true,"helpText":"","options":[]}]'::jsonb,
        'a0000000-0000-7000-8000-000000000009',
        timestamptz '2026-10-01 14:00:00+00', timestamptz '2026-10-07 16:00:00+00');

INSERT INTO dataticket.form_template_versions (id, form_template_id, version_number, fields, published_by, published_at)
SELECT 'a6000000-0000-7000-8000-000000000001', id, 1, draft_fields,
       'a0000000-0000-7000-8000-000000000009', timestamptz '2026-10-07 16:00:00+00'
FROM dataticket.form_templates
WHERE id = 'a5000000-0000-7000-8000-000000000001';

-- 13.6 Tickets en todos los estados (HU-013, HU-015, HU-021, HU-022, HU-036).
--      Prioridad calculada = matriz(urgencia, impacto); la base de datos verifica la coherencia.
INSERT INTO dataticket.tickets (id, number, company_id, requester_id, category_id, title, description,
                                urgency, impact, calculated_priority, current_priority, status,
                                pull_request_url, form_template_version_id, custom_field_values,
                                created_at, updated_at, closed_at, closed_by)
SELECT t.id::uuid, t.number, t.company_id::uuid, t.requester_id::uuid,
       (SELECT c.id FROM dataticket.categories AS c WHERE c.name = t.category),
       t.title, t.description, t.urgency, t.impact, t.calculated, t.current_priority, t.status,
       t.pr_url, t.form_version::uuid, t.custom_fields::jsonb,
       t.created_at::timestamptz, t.updated_at::timestamptz, t.closed_at::timestamptz, t.closed_by::uuid
FROM (VALUES
    -- T01 · Empresa 03 · New sin asignar, crítico (cola de triage, HU-016).
    ('d0000000-0000-7000-8000-000000000001', 'DT-000001', 'c0000000-0000-7000-8000-000000000003',
     'b0000000-0000-7000-8000-000003000001', 'Incidente',
     'El sistema no genera facturas electrónicas',
     'Desde esta mañana ninguna factura llega a la DIAN; el proceso queda en "pendiente".',
     'High', 'High', 'Critical', 'Critical', 'New', NULL, NULL, NULL,
     '2026-10-08 14:10:00+00', '2026-10-08 14:10:00+00', NULL, NULL),
    -- T02 · Empresa 02 · New sin asignar, muy baja.
    ('d0000000-0000-7000-8000-000000000002', 'DT-000002', 'c0000000-0000-7000-8000-000000000002',
     'b0000000-0000-7000-8000-000002000001', 'Consulta',
     '¿Cómo exporto el reporte de inventario?',
     'Necesito el reporte mensual de inventario en Excel y no encuentro la opción.',
     'Low', 'Low', 'VeryLow', 'VeryLow', 'New', NULL, NULL, NULL,
     '2026-10-08 15:00:00+00', '2026-10-08 15:00:00+00', NULL, NULL),
    -- T03 · Empresa 01 · InDevelopment; ticket principal del chat (HU-026 a HU-032).
    ('d0000000-0000-7000-8000-000000000003', 'DT-000003', 'c0000000-0000-7000-8000-000000000001',
     'b0000000-0000-7000-8000-000001000001', 'Incidente',
     'Consecutivo de facturas duplicado',
     'Dos facturas del 21 de septiembre salieron con el mismo consecutivo.',
     'Medium', 'High', 'High', 'High', 'InDevelopment', NULL, NULL, NULL,
     '2026-09-22 13:30:00+00', '2026-09-26 16:30:00+00', NULL, NULL),
    -- T04 · Empresa 01 · PullRequestReview con URL de PR (HU-022).
    ('d0000000-0000-7000-8000-000000000004', 'DT-000004', 'c0000000-0000-7000-8000-000000000001',
     'b0000000-0000-7000-8000-000001000002', 'Incidente',
     'Error al liquidar horas extra en nómina',
     'La liquidación de horas extra nocturnas no aplica el recargo del 35 %.',
     'High', 'Medium', 'High', 'High', 'PullRequestReview',
     'https://git.dataglobal.test/erp/nomina/pull/412', NULL, NULL,
     '2026-09-15 16:00:00+00', '2026-09-16 15:00:00+00', NULL, NULL),
    -- T05 · Empresa 02 · InProduction por ruta directa a Producción.
    ('d0000000-0000-7000-8000-000000000005', 'DT-000005', 'c0000000-0000-7000-8000-000000000002',
     'b0000000-0000-7000-8000-000002000001', 'Requerimiento',
     'Habilitar nuevo centro de costos',
     'Se requiere crear el centro de costos 4501 para la sede Barranquilla.',
     'Medium', 'Medium', 'Medium', 'Medium', 'InProduction', NULL, NULL, NULL,
     '2026-09-10 14:00:00+00', '2026-09-10 15:00:00+00', NULL, NULL),
    -- T06 · Empresa 02 · InDevelopment con ajuste manual de prioridad y reasignación (HU-015, HU-019).
    ('d0000000-0000-7000-8000-000000000006', 'DT-000006', 'c0000000-0000-7000-8000-000000000002',
     'b0000000-0000-7000-8000-000002000002', 'Requerimiento',
     'Campo adicional en la orden de compra',
     'Agregar el campo "proyecto" a la orden de compra impresa.',
     'Low', 'Medium', 'Low', 'High', 'InDevelopment', NULL, NULL, NULL,
     '2026-09-29 19:20:00+00', '2026-09-30 13:00:00+00', NULL, NULL),
    -- T07 · Empresa 02 · SolutionDelivered con respuesta formal enviada (HU-033 a HU-035).
    ('d0000000-0000-7000-8000-000000000007', 'DT-000007', 'c0000000-0000-7000-8000-000000000002',
     'b0000000-0000-7000-8000-000002000001', 'Consulta',
     'Diferencia en el informe de ventas',
     'El informe de ventas de agosto no cuadra con la contabilidad.',
     'Medium', 'Low', 'Low', 'Low', 'SolutionDelivered', NULL, NULL, NULL,
     '2026-09-03 13:00:00+00', '2026-09-08 22:00:00+00', NULL, NULL),
    -- T08 · Empresa 03 · Closed manualmente por la PM (HU-036).
    ('d0000000-0000-7000-8000-000000000008', 'DT-000008', 'c0000000-0000-7000-8000-000000000003',
     'b0000000-0000-7000-8000-000003000001', 'Incidente',
     'Caída del módulo de cartera',
     'El módulo de cartera muestra error 500 al abrir cualquier cliente.',
     'High', 'High', 'Critical', 'Critical', 'Closed', NULL, NULL, NULL,
     '2026-09-01 15:45:00+00', '2026-09-12 21:00:00+00', '2026-09-12 21:00:00+00',
     'a0000000-0000-7000-8000-000000000001'),
    -- T09 · Empresa 03 · New tomado por el administrador en cobertura (HU-017).
    ('d0000000-0000-7000-8000-000000000009', 'DT-000009', 'c0000000-0000-7000-8000-000000000003',
     'b0000000-0000-7000-8000-000003000002', 'Requerimiento',
     'Usuario nuevo para la tesorera',
     'Crear el usuario de la nueva tesorera con permisos de pagos.',
     'Low', 'High', 'Medium', 'Medium', 'New', NULL, NULL, NULL,
     '2026-10-06 18:00:00+00', '2026-10-06 19:00:00+00', NULL, NULL),
    -- T10 · Empresa 01 · New radicado con la plantilla publicada (HU-046, HU-047).
    ('d0000000-0000-7000-8000-000000000010', 'DT-000010', 'c0000000-0000-7000-8000-000000000001',
     'b0000000-0000-7000-8000-000001000001', 'Incidente',
     'Nómina de octubre con descuentos duplicados',
     'Varios empleados tienen el descuento de fondo de empleados aplicado dos veces.',
     'Medium', 'Medium', 'Medium', 'Medium', 'New', NULL, 'a6000000-0000-7000-8000-000000000001',
     '{"modulo":"Nómina","fechaEvento":"2026-10-06"}',
     '2026-10-07 16:30:00+00', '2026-10-07 16:30:00+00', NULL, NULL)
) AS t(id, number, company_id, requester_id, category, title, description, urgency, impact,
       calculated, current_priority, status, pr_url, form_version, custom_fields,
       created_at, updated_at, closed_at, closed_by);

-- El número visible se fijó a mano: la secuencia continúa en DT-000011.
SELECT setval('dataticket.ticket_number_seq', 10, true);

-- 13.7 Ajuste manual de prioridad de T06 (HU-015).
INSERT INTO dataticket.priority_overrides (id, ticket_id, calculated_priority, previous_priority, new_priority,
                                           reason, changed_by, changed_at)
VALUES ('a4000000-0000-7000-8000-000000000001', 'd0000000-0000-7000-8000-000000000006',
        'Low', 'Low', 'High', 'El cliente necesita el campo para el cierre contable de septiembre.',
        'a0000000-0000-7000-8000-000000000001', timestamptz '2026-09-29 19:40:00+00');

-- 13.8 Asignaciones (HU-018, HU-019). En T06 Cristian se reasigna a Kevin.
INSERT INTO dataticket.assignments (id, ticket_id, assignee_id, assigned_by, assigned_at, unassigned_by, unassigned_at)
SELECT a.id::uuid, a.ticket_id::uuid, a.assignee_id::uuid, 'a0000000-0000-7000-8000-000000000001'::uuid,
       a.assigned_at::timestamptz,
       CASE WHEN a.unassigned_at IS NULL THEN NULL ELSE 'a0000000-0000-7000-8000-000000000001'::uuid END,
       a.unassigned_at::timestamptz
FROM (VALUES
    ('a3000000-0000-7000-8000-000000000001', 'd0000000-0000-7000-8000-000000000003', 'a0000000-0000-7000-8000-000000000005', '2026-09-22 15:00:00+00', NULL),
    ('a3000000-0000-7000-8000-000000000002', 'd0000000-0000-7000-8000-000000000003', 'a0000000-0000-7000-8000-000000000004', '2026-09-22 15:00:00+00', NULL),
    ('a3000000-0000-7000-8000-000000000003', 'd0000000-0000-7000-8000-000000000004', 'a0000000-0000-7000-8000-000000000004', '2026-09-15 17:00:00+00', NULL),
    ('a3000000-0000-7000-8000-000000000004', 'd0000000-0000-7000-8000-000000000005', 'a0000000-0000-7000-8000-000000000002', '2026-09-10 15:00:00+00', NULL),
    ('a3000000-0000-7000-8000-000000000005', 'd0000000-0000-7000-8000-000000000006', 'a0000000-0000-7000-8000-000000000007', '2026-09-29 20:00:00+00', '2026-09-30 13:00:00+00'),
    ('a3000000-0000-7000-8000-000000000006', 'd0000000-0000-7000-8000-000000000006', 'a0000000-0000-7000-8000-000000000008', '2026-09-30 13:00:00+00', NULL),
    ('a3000000-0000-7000-8000-000000000007', 'd0000000-0000-7000-8000-000000000007', 'a0000000-0000-7000-8000-000000000008', '2026-09-03 14:00:00+00', NULL),
    ('a3000000-0000-7000-8000-000000000008', 'd0000000-0000-7000-8000-000000000008', 'a0000000-0000-7000-8000-000000000005', '2026-09-01 17:00:00+00', NULL)
) AS a(id, ticket_id, assignee_id, assigned_at, unassigned_at);

-- 13.9 Participaciones (HU-017 a HU-019, HU-032). Vigente = removed_at IS NULL.
--      T03: Brayan participó y fue retirado (sus mensajes se conservan; ya no accede).
--      T03: Kevin NO participa: sirve como «interno no participante» en las pruebas del chat.
--      T09: el administrador tomó el ticket en cobertura.
INSERT INTO dataticket.ticket_participants (id, ticket_id, user_id, origin, added_by, added_at, removed_by, removed_at)
SELECT p.id::uuid, p.ticket_id::uuid, p.user_id::uuid, p.origin, p.added_by::uuid, p.added_at::timestamptz,
       p.removed_by::uuid, p.removed_at::timestamptz
FROM (VALUES
    ('a2000000-0000-7000-8000-000000000001', 'd0000000-0000-7000-8000-000000000003', 'a0000000-0000-7000-8000-000000000005', 'Assignment',   'a0000000-0000-7000-8000-000000000001', '2026-09-22 15:00:00+00', NULL, NULL),
    ('a2000000-0000-7000-8000-000000000002', 'd0000000-0000-7000-8000-000000000003', 'a0000000-0000-7000-8000-000000000004', 'Assignment',   'a0000000-0000-7000-8000-000000000001', '2026-09-22 15:00:00+00', NULL, NULL),
    ('a2000000-0000-7000-8000-000000000003', 'd0000000-0000-7000-8000-000000000003', 'a0000000-0000-7000-8000-000000000006', 'Participant',  'a0000000-0000-7000-8000-000000000001', '2026-09-23 14:00:00+00', 'a0000000-0000-7000-8000-000000000001', '2026-09-25 20:00:00+00'),
    ('a2000000-0000-7000-8000-000000000004', 'd0000000-0000-7000-8000-000000000004', 'a0000000-0000-7000-8000-000000000004', 'Assignment',   'a0000000-0000-7000-8000-000000000001', '2026-09-15 17:00:00+00', NULL, NULL),
    ('a2000000-0000-7000-8000-000000000005', 'd0000000-0000-7000-8000-000000000004', 'a0000000-0000-7000-8000-000000000002', 'Participant',  'a0000000-0000-7000-8000-000000000001', '2026-09-16 15:00:00+00', NULL, NULL),
    ('a2000000-0000-7000-8000-000000000006', 'd0000000-0000-7000-8000-000000000005', 'a0000000-0000-7000-8000-000000000002', 'Assignment',   'a0000000-0000-7000-8000-000000000001', '2026-09-10 15:00:00+00', NULL, NULL),
    ('a2000000-0000-7000-8000-000000000007', 'd0000000-0000-7000-8000-000000000005', 'a0000000-0000-7000-8000-000000000003', 'Participant',  'a0000000-0000-7000-8000-000000000002', '2026-09-10 16:00:00+00', NULL, NULL),
    ('a2000000-0000-7000-8000-000000000008', 'd0000000-0000-7000-8000-000000000006', 'a0000000-0000-7000-8000-000000000007', 'Assignment',   'a0000000-0000-7000-8000-000000000001', '2026-09-29 20:00:00+00', 'a0000000-0000-7000-8000-000000000001', '2026-09-30 13:00:00+00'),
    ('a2000000-0000-7000-8000-000000000009', 'd0000000-0000-7000-8000-000000000006', 'a0000000-0000-7000-8000-000000000008', 'Assignment',   'a0000000-0000-7000-8000-000000000001', '2026-09-30 13:00:00+00', NULL, NULL),
    ('a2000000-0000-7000-8000-000000000010', 'd0000000-0000-7000-8000-000000000007', 'a0000000-0000-7000-8000-000000000008', 'Assignment',   'a0000000-0000-7000-8000-000000000001', '2026-09-03 14:00:00+00', NULL, NULL),
    ('a2000000-0000-7000-8000-000000000011', 'd0000000-0000-7000-8000-000000000008', 'a0000000-0000-7000-8000-000000000005', 'Assignment',   'a0000000-0000-7000-8000-000000000001', '2026-09-01 17:00:00+00', NULL, NULL),
    ('a2000000-0000-7000-8000-000000000012', 'd0000000-0000-7000-8000-000000000009', 'a0000000-0000-7000-8000-000000000009', 'CoverageTake', 'a0000000-0000-7000-8000-000000000009', '2026-10-06 19:00:00+00', NULL, NULL)
) AS p(id, ticket_id, user_id, origin, added_by, added_at, removed_by, removed_at);

-- 13.10 Respuestas formales (HU-033 a HU-035): una por ticket (V-13), enviadas por la PM.
INSERT INTO dataticket.formal_responses (id, ticket_id, body, sent_by, sent_at, email_delivery_status, email_last_attempt_at)
VALUES
    ('f0000000-0000-7000-8000-000000000001', 'd0000000-0000-7000-8000-000000000007',
     'La diferencia se debía a dos notas crédito registradas en septiembre con fecha de agosto. Adjuntamos el informe conciliado.',
     'a0000000-0000-7000-8000-000000000001', timestamptz '2026-09-08 22:00:00+00', 'Sent', timestamptz '2026-09-08 22:00:05+00'),
    ('f0000000-0000-7000-8000-000000000002', 'd0000000-0000-7000-8000-000000000008',
     'Corregimos la consulta del módulo de cartera y desplegamos la versión 3.4.2. El módulo ya opera con normalidad.',
     'a0000000-0000-7000-8000-000000000001', timestamptz '2026-09-11 20:00:00+00', 'Sent', timestamptz '2026-09-11 20:00:04+00');

-- 13.11 Chat interno (HU-026 a HU-032). T03: e…04 y e…05 comparten marca de tiempo para
--       probar el orden estable (created_at, id). e…03 es de Brayan, retirado después.
INSERT INTO dataticket.chat_messages (id, ticket_id, author_id, body, created_at)
SELECT m.id::uuid, m.ticket_id::uuid, m.author_id::uuid, m.body, m.created_at::timestamptz
FROM (VALUES
    ('e0000000-0000-7000-8000-000000000001', 'd0000000-0000-7000-8000-000000000003', 'a0000000-0000-7000-8000-000000000005', 'Ya revisé el caso: parece un problema en el cálculo del consecutivo.', '2026-09-22 15:10:00+00'),
    ('e0000000-0000-7000-8000-000000000002', 'd0000000-0000-7000-8000-000000000003', 'a0000000-0000-7000-8000-000000000004', 'Lo reproduje en pruebas con los datos del cliente.', '2026-09-22 15:25:00+00'),
    ('e0000000-0000-7000-8000-000000000003', 'd0000000-0000-7000-8000-000000000003', 'a0000000-0000-7000-8000-000000000006', 'En el log del servidor el error aparece desde la versión 3.4.', '2026-09-24 13:00:00+00'),
    ('e0000000-0000-7000-8000-000000000004', 'd0000000-0000-7000-8000-000000000003', 'a0000000-0000-7000-8000-000000000005', 'Subo la corrección a una rama para revisión.', '2026-09-26 14:00:00+00'),
    ('e0000000-0000-7000-8000-000000000005', 'd0000000-0000-7000-8000-000000000003', 'a0000000-0000-7000-8000-000000000001', 'Gracias. ¿Tenemos fecha estimada de entrega?', '2026-09-26 14:00:00+00'),
    ('e0000000-0000-7000-8000-000000000006', 'd0000000-0000-7000-8000-000000000003', 'a0000000-0000-7000-8000-000000000004', 'Adjunto la captura con el consecutivo ya corregido.', '2026-09-26 16:30:00+00'),
    ('e0000000-0000-7000-8000-000000000007', 'd0000000-0000-7000-8000-000000000004', 'a0000000-0000-7000-8000-000000000004', 'El PR está listo para revisión.', '2026-09-16 14:50:00+00'),
    ('e0000000-0000-7000-8000-000000000008', 'd0000000-0000-7000-8000-000000000004', 'a0000000-0000-7000-8000-000000000002', 'Lo reviso hoy en la tarde.', '2026-09-16 16:00:00+00')
) AS m(id, ticket_id, author_id, body, created_at);

-- 13.12 Confirmaciones de lectura (HU-030): el autor no genera lectura propia (P-14); e…06 sigue sin leer.
INSERT INTO dataticket.message_read_receipts (chat_message_id, user_id, read_at)
SELECT r.message_id::uuid, r.user_id::uuid, r.read_at::timestamptz
FROM (VALUES
    ('e0000000-0000-7000-8000-000000000001', 'a0000000-0000-7000-8000-000000000004', '2026-09-22 15:20:00+00'),
    ('e0000000-0000-7000-8000-000000000001', 'a0000000-0000-7000-8000-000000000001', '2026-09-22 16:00:00+00'),
    ('e0000000-0000-7000-8000-000000000002', 'a0000000-0000-7000-8000-000000000005', '2026-09-22 15:30:00+00'),
    ('e0000000-0000-7000-8000-000000000002', 'a0000000-0000-7000-8000-000000000001', '2026-09-22 16:00:00+00'),
    ('e0000000-0000-7000-8000-000000000003', 'a0000000-0000-7000-8000-000000000005', '2026-09-24 13:30:00+00'),
    ('e0000000-0000-7000-8000-000000000003', 'a0000000-0000-7000-8000-000000000004', '2026-09-24 14:00:00+00'),
    ('e0000000-0000-7000-8000-000000000004', 'a0000000-0000-7000-8000-000000000004', '2026-09-26 14:10:00+00'),
    ('e0000000-0000-7000-8000-000000000005', 'a0000000-0000-7000-8000-000000000005', '2026-09-26 14:05:00+00'),
    ('e0000000-0000-7000-8000-000000000007', 'a0000000-0000-7000-8000-000000000002', '2026-09-16 15:30:00+00')
) AS r(message_id, user_id, read_at);

-- 13.13 Notificaciones en la app (HU-037): una por destinatario y mensaje; e…06 pendiente para Laura.
INSERT INTO dataticket.notifications (id, recipient_user_id, ticket_id, chat_message_id, created_at, read_at)
SELECT n.id::uuid, n.recipient::uuid, n.ticket_id::uuid, n.message_id::uuid, n.created_at::timestamptz, n.read_at::timestamptz
FROM (VALUES
    ('a7000000-0000-7000-8000-000000000001', 'a0000000-0000-7000-8000-000000000004', 'd0000000-0000-7000-8000-000000000003', 'e0000000-0000-7000-8000-000000000005', '2026-09-26 14:00:00+00', '2026-09-26 14:10:00+00'),
    ('a7000000-0000-7000-8000-000000000002', 'a0000000-0000-7000-8000-000000000005', 'd0000000-0000-7000-8000-000000000003', 'e0000000-0000-7000-8000-000000000006', '2026-09-26 16:30:00+00', NULL),
    ('a7000000-0000-7000-8000-000000000003', 'a0000000-0000-7000-8000-000000000002', 'd0000000-0000-7000-8000-000000000004', 'e0000000-0000-7000-8000-000000000007', '2026-09-16 14:50:00+00', '2026-09-16 15:30:00+00')
) AS n(id, recipient, ticket_id, message_id, created_at, read_at);

-- 13.14 Adjuntos (HU-014, HU-029, HU-033): solo metadatos; los binarios no existen en Azurite
--       (las pruebas que descargan archivos deben subirlos primero). Contenedor «attachments».
INSERT INTO dataticket.attachments (id, ticket_id, context, chat_message_id, formal_response_id, file_name,
                                    content_type, size_bytes, blob_name, url, uploaded_by, uploaded_at)
SELECT a.id::uuid, a.ticket_id::uuid, a.context, a.message_id::uuid, a.response_id::uuid, a.file_name,
       a.content_type, a.size_bytes, a.blob_name,
       'http://localhost:10000/devstoreaccount1/attachments/' || a.blob_name,
       a.uploaded_by::uuid, a.uploaded_at::timestamptz
FROM (VALUES
    ('a1000000-0000-7000-8000-000000000001', 'd0000000-0000-7000-8000-000000000001', 'Submission', NULL, NULL,
     'captura-error-dian.png', 'image/png', 245760,
     'd0000000-0000-7000-8000-000000000001/a1000000-0000-7000-8000-000000000001.png',
     'b0000000-0000-7000-8000-000003000001', '2026-10-08 14:10:00+00'),
    ('a1000000-0000-7000-8000-000000000002', 'd0000000-0000-7000-8000-000000000003', 'Submission', NULL, NULL,
     'factura-electronica.xml', 'application/xml', 9120,
     'd0000000-0000-7000-8000-000000000003/a1000000-0000-7000-8000-000000000002.xml',
     'b0000000-0000-7000-8000-000001000001', '2026-09-22 13:30:00+00'),
    ('a1000000-0000-7000-8000-000000000003', 'd0000000-0000-7000-8000-000000000003', 'Chat', 'e0000000-0000-7000-8000-000000000006', NULL,
     'consecutivo-corregido.png', 'image/png', 312004,
     'd0000000-0000-7000-8000-000000000003/a1000000-0000-7000-8000-000000000003.png',
     'a0000000-0000-7000-8000-000000000004', '2026-09-26 16:29:00+00'),
    ('a1000000-0000-7000-8000-000000000004', 'd0000000-0000-7000-8000-000000000007', 'Submission', NULL, NULL,
     'reporte-ventas-agosto.xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 58211,
     'd0000000-0000-7000-8000-000000000007/a1000000-0000-7000-8000-000000000004.xlsx',
     'b0000000-0000-7000-8000-000002000001', '2026-09-03 13:00:00+00'),
    ('a1000000-0000-7000-8000-000000000005', 'd0000000-0000-7000-8000-000000000007', 'FormalResponse', NULL, 'f0000000-0000-7000-8000-000000000001',
     'informe-conciliado.pdf', 'application/pdf', 182340,
     'd0000000-0000-7000-8000-000000000007/a1000000-0000-7000-8000-000000000005.pdf',
     'a0000000-0000-7000-8000-000000000001', '2026-09-08 21:58:00+00')
) AS a(id, ticket_id, context, message_id, response_id, file_name, content_type, size_bytes, blob_name,
       uploaded_by, uploaded_at);

-- 13.15 Auditoría append-only (HU-011, HU-012): bitácora coherente con los datos anteriores.
--       Valores en jsonb camelCase. Solo INSERT: los triggers impiden UPDATE/DELETE/TRUNCATE.
INSERT INTO dataticket.audit_entries (ticket_id, actor_id, occurred_at, action, object_type, object_id, old_value, new_value)
SELECT t.id, t.requester_id, t.created_at, 'TicketSubmitted', 'Ticket', t.id, NULL,
       jsonb_build_object('number', t.number, 'status', 'New', 'calculatedPriority', t.calculated_priority)
FROM dataticket.tickets AS t;

INSERT INTO dataticket.audit_entries (ticket_id, actor_id, occurred_at, action, object_type, object_id, old_value, new_value)
SELECT a.ticket_id, a.assigned_by, a.assigned_at, 'TicketAssigned', 'Assignment', a.id, NULL,
       jsonb_build_object('assigneeId', a.assignee_id)
FROM dataticket.assignments AS a
WHERE a.id <> 'a3000000-0000-7000-8000-000000000006';

INSERT INTO dataticket.audit_entries (ticket_id, actor_id, occurred_at, action, object_type, object_id, old_value, new_value)
SELECT p.ticket_id, p.added_by, p.added_at, 'ParticipantAdded', 'TicketParticipant', p.id, NULL,
       jsonb_build_object('userId', p.user_id, 'origin', p.origin)
FROM dataticket.ticket_participants AS p
WHERE p.origin IN ('Participant', 'CoverageTake');

INSERT INTO dataticket.audit_entries (ticket_id, actor_id, occurred_at, action, object_type, object_id, old_value, new_value)
SELECT p.ticket_id, p.removed_by, p.removed_at, 'ParticipantRemoved', 'TicketParticipant', p.id,
       jsonb_build_object('userId', p.user_id), NULL
FROM dataticket.ticket_participants AS p
WHERE p.removed_at IS NOT NULL AND p.origin = 'Participant';

INSERT INTO dataticket.audit_entries (ticket_id, actor_id, occurred_at, action, object_type, object_id, old_value, new_value)
SELECT e.ticket_id::uuid, 'a0000000-0000-7000-8000-000000000001'::uuid, e.occurred_at::timestamptz, e.action,
       e.object_type, e.object_id::uuid, e.old_value::jsonb, e.new_value::jsonb
FROM (VALUES
    ('d0000000-0000-7000-8000-000000000006', '2026-09-30 13:00:00+00', 'TicketReassigned', 'Assignment', 'a3000000-0000-7000-8000-000000000006',
     '{"assigneeId":"a0000000-0000-7000-8000-000000000007"}', '{"assigneeId":"a0000000-0000-7000-8000-000000000008"}'),
    ('d0000000-0000-7000-8000-000000000006', '2026-09-29 19:40:00+00', 'PriorityOverridden', 'Ticket', 'd0000000-0000-7000-8000-000000000006',
     '{"currentPriority":"Low"}', '{"currentPriority":"High","reason":"El cliente necesita el campo para el cierre contable de septiembre."}'),
    ('d0000000-0000-7000-8000-000000000003', '2026-09-22 15:00:00+00', 'StatusChanged', 'Ticket', 'd0000000-0000-7000-8000-000000000003', '{"status":"New"}', '{"status":"InDevelopment"}'),
    ('d0000000-0000-7000-8000-000000000004', '2026-09-15 17:00:00+00', 'StatusChanged', 'Ticket', 'd0000000-0000-7000-8000-000000000004', '{"status":"New"}', '{"status":"InDevelopment"}'),
    ('d0000000-0000-7000-8000-000000000004', '2026-09-16 15:00:00+00', 'StatusChanged', 'Ticket', 'd0000000-0000-7000-8000-000000000004', '{"status":"InDevelopment"}', '{"status":"PullRequestReview"}'),
    ('d0000000-0000-7000-8000-000000000005', '2026-09-10 15:00:00+00', 'StatusChanged', 'Ticket', 'd0000000-0000-7000-8000-000000000005', '{"status":"New"}', '{"status":"InProduction"}'),
    ('d0000000-0000-7000-8000-000000000006', '2026-09-29 20:00:00+00', 'StatusChanged', 'Ticket', 'd0000000-0000-7000-8000-000000000006', '{"status":"New"}', '{"status":"InDevelopment"}'),
    ('d0000000-0000-7000-8000-000000000007', '2026-09-03 14:00:00+00', 'StatusChanged', 'Ticket', 'd0000000-0000-7000-8000-000000000007', '{"status":"New"}', '{"status":"InDevelopment"}'),
    ('d0000000-0000-7000-8000-000000000007', '2026-09-08 22:00:00+00', 'StatusChanged', 'Ticket', 'd0000000-0000-7000-8000-000000000007', '{"status":"InDevelopment"}', '{"status":"SolutionDelivered"}'),
    ('d0000000-0000-7000-8000-000000000007', '2026-09-08 22:00:00+00', 'FormalResponseDelivered', 'FormalResponse', 'f0000000-0000-7000-8000-000000000001', NULL, '{"emailDeliveryStatus":"Sent"}'),
    ('d0000000-0000-7000-8000-000000000008', '2026-09-01 17:00:00+00', 'StatusChanged', 'Ticket', 'd0000000-0000-7000-8000-000000000008', '{"status":"New"}', '{"status":"InDevelopment"}'),
    ('d0000000-0000-7000-8000-000000000008', '2026-09-11 20:00:00+00', 'StatusChanged', 'Ticket', 'd0000000-0000-7000-8000-000000000008', '{"status":"InDevelopment"}', '{"status":"SolutionDelivered"}'),
    ('d0000000-0000-7000-8000-000000000008', '2026-09-11 20:00:00+00', 'FormalResponseDelivered', 'FormalResponse', 'f0000000-0000-7000-8000-000000000002', NULL, '{"emailDeliveryStatus":"Sent"}'),
    ('d0000000-0000-7000-8000-000000000008', '2026-09-12 21:00:00+00', 'TicketClosed', 'Ticket', 'd0000000-0000-7000-8000-000000000008', '{"status":"SolutionDelivered"}', '{"status":"Closed"}')
) AS e(ticket_id, occurred_at, action, object_type, object_id, old_value, new_value);

COMMIT;

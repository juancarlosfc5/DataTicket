---
title: "ADR-0009: db.sql como diseño de referencia del esquema; implementación con migraciones de EF Core"
type: decision
status: aceptada
tags: [decision, arquitectura/datos, postgresql, efcore]
sources: ["db.sql", "Instrucción de la persona líder del proyecto (2026-10-09)"]
aliases: [ADR-0009, Diseño de referencia de la base de datos, db.sql]
created: 2026-10-09
updated: 2026-10-09
decision_date: 2026-10-09
deciders: [juancarlosfc5 (persona líder del proyecto)]
---

# ADR-0009: `db.sql` como diseño de referencia del esquema; implementación con migraciones de EF Core

El script `db.sql` de la raíz es el **diseño de referencia aprobado** de la base de datos. Se escribió y se validó antes de implementar. El esquema real se construye con **migraciones code-first de EF Core**, tal como piden [[persistencia-postgresql]] y los DoD de las HU.

## Contexto

Antes de empezar la ejecución, la persona líder del proyecto pidió un script SQL completo del esquema. Quería revisar el modelo de datos de todo el backlog ([[tablero-scrum]]) antes de implementarlo con Entity Framework Core. El script se escribió con enfoque DB-first, aunque [[persistencia-postgresql]] y las HU de persistencia piden migraciones. Esa diferencia quedó registrada como contradicción en [[pendientes]].

La persona líder aclaró que `db.sql` era un **paso previo de validación** y no el mecanismo de implementación. Revisó el script y lo **aprobó** el 2026-10-09.

## Opciones consideradas

1. **DB-first puro.** `db.sql` sería la fuente del esquema y el modelo se regeneraría con `dotnet ef dbcontext scaffold`.
   - A favor: el SQL queda explícito.
   - En contra: el scaffolding descarta las FK hacia `identity.users`, no hereda de `IdentityDbContext` y choca con los DoD de las HU.
2. **`db.sql` como diseño de referencia y migraciones code-first.** El modelo se escribe en el dominio y en las configuraciones de EF Core; cada HU genera la migración de su parte.
   - A favor: respeta [[backend-hexagonal]], las HU aprobadas y la reversibilidad de las migraciones (`Down`).
   - En contra: hay que mantener la equivalencia entre las migraciones y el diseño.

## Decisión

Se adopta la **opción 2**. `db.sql` no se ejecuta en ningún entorno del proyecto ni se usa para scaffolding. Las HU crean el esquema de forma incremental con `dotnet ef migrations add …`, y cada migración debe reproducir las tablas, columnas, restricciones e índices de su parte en `db.sql`.

## Consecuencias

- Positivas:
  - Las HU y sus DoD siguen vigentes sin cambios.
  - El equipo cuenta con un mapa aprobado de tablas, nombres, `CHECK`, índices y FK compuestas para revisar cada migración.
- Negativas o riesgos:
  - El diseño y las migraciones pueden divergir. Mitigación: la revisión de cada PR con migración compara contra `db.sql`.
  - Algunos elementos no tienen equivalente directo en el modelo de EF Core:
    - funciones (`priority_matrix`, `next_ticket_number`);
    - triggers (`reject_mutation`);
    - índices de expresión (`lower(name)`);
    - FK compuestas sobre claves alternativas.

    Se escriben en la migración con `migrationBuilder.Sql(...)`, `HasCheckConstraint`, `HasAlternateKey` y `HasIndex(...).HasFilter(...)`.
- Qué hacer en código, infraestructura o wiki:
  - **Cambios al diseño.** Si una HU necesita apartarse del diseño, se actualiza `db.sql` en el mismo PR y se registra el cambio en [[log]]. El script sigue siendo el diseño vigente.
  - **Cabecera de `db.sql`.** Todavía dice «enfoque DB-first» y trae el comando de scaffolding. Esas líneas son históricas y este ADR prevalece sobre ellas.
  - **Nombres.** Las migraciones usan los mismos nombres en snake_case, mediante `EFCore.NamingConventions` o `HasColumnName`. La versión del paquete se verifica en NuGet al adoptarlo.
  - **Decisiones abiertas.** Las que el script tomó de forma provisional siguen abiertas en [[pendientes]]: roles frente a equipos, `jsonb` para respuestas variables, V-06, V-12, V-13, V-16, RLS y roles de base de datos.

## Relacionado

- [[persistencia-postgresql]] · [[modelo-de-dominio]] · [[backend-hexagonal]] · [[autenticacion-identity]]
- [[pendientes]] · [[tablero-scrum]] · [[glosario]]

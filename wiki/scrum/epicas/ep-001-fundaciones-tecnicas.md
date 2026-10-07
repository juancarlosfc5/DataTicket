---
title: "EP-001 — Fundaciones técnicas y calidad continua"
type: epica
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/epica, arquitectura/frontend, proceso]
sources: ["PRD.md §1", "PRD.md §5", "PRD.md §12", "PRD.md §13", "PRD.md §14"]
aliases: ["EP-001", "Fundaciones técnicas y calidad continua"]
fase_prd: "0"
criterios_prd: []
historias: ["[[hu-001-integracion-continua]]", "[[hu-002-shell-y-navegacion-por-rol]]"]
dependencias: []
created: 2026-10-07
updated: 2026-10-07
---

# EP-001 — Fundaciones técnicas y calidad continua

Deja listo el andamiaje que toda funcionalidad posterior necesita: una verificación automática en cada Pull Request y un shell de frontend con dos superficies (Portal del cliente y Espacio interno), rutas protegidas y un cliente HTTP capaz de mutar datos de forma segura.

## Objetivo

Que cualquier cambio del equipo se verifique igual en todas las máquinas (pruebas de backend con reglas de arquitectura, lint con fronteras MVC, pruebas y build del frontend, Compose válido) y que las HU funcionales posteriores se monten sobre un shell común que ya separa las superficies por rol y maneja de forma uniforme los errores 401/403/ProblemDetails.

## Valor esperado

- Las reglas no negociables de `AGENTS.md` §6 (hexagonal y MVC) dejan de depender de la disciplina individual: un PR que las rompe se detecta antes de la revisión humana.
- El aislamiento entre Portal del cliente y Espacio interno existe desde la primera pantalla, de modo que ninguna HU posterior «olvida» separar superficies (PRD §1, §10).
- Las HU de identidad ([[ep-002-identidad-y-acceso]]) y de administración ([[ep-003-empresas-usuarios-y-equipos]]) reutilizan el mismo cliente HTTP con antiforgery y mapeo de errores.

## Fase del PRD

Fase 0 — Base técnica (PRD §13). Prioridad: bloqueante.

## Actores

- Integrante del equipo de desarrollo del repositorio ([[equipo-del-repositorio]]).
- Usuario cliente (Solicitante `Requester`, Coordinador `CompanyCoordinator`) e integrante interno de Data Global, como usuarios del shell.

## Alcance

- Flujo de integración continua que ejecuta en cada PR las verificaciones de `AGENTS.md` §10 ([[hu-001-integracion-continua|HU-001]]).
- Shell de la SPA: superficies Portal del cliente y Espacio interno, rutas protegidas, vista «sin permiso», layout base en `src/shared/ui`, cliente HTTP con métodos de mutación, cabecera antiforgery y mapeo de ProblemDetails ([[hu-002-shell-y-navegacion-por-rol|HU-002]]).

## Fuera de alcance

- Despliegue continuo a cualquier entorno (la infraestructura productiva está pendiente, PRD §16.1).
- Elección del enrutador y de la librería de estado de servidor del frontend: es una decisión abierta ([[pendientes]] §4); estas HU dependen de ella, no la toman.
- Pantallas funcionales (login, radicación, bandejas): pertenecen a otras épicas.
- Pruebas E2E con Playwright (previstas en [[estrategia-de-pruebas]], sin HU todavía).

## Requisitos y reglas de negocio

- Dos superficies con permisos distintos: Portal del cliente y Espacio de trabajo de Data Global (PRD §1, §10).
- La autorización se aplica en servidor en cada operación; ocultar elementos en la interfaz no constituye aislamiento (PRD §5 regla 2, §12). El shell solo es una ayuda de navegación.
- Todo acceso exige autenticación, salvo páginas de entrada expresamente públicas (PRD §12).
- La interfaz es web responsiva (PRD §12).
- Los clientes nunca ven estados técnicos, chat interno, URL de PR ni nombres de colaboradores internos (PRD §5 regla 6).

## Criterios del PRD cubiertos

- Ninguno de forma directa. La épica habilita la verificación automática continua de todos los criterios (PRD CA-01…CA-15) y prepara la separación de superficies que exigen PRD CA-02 y PRD CA-10 → [[criterios-de-aceptacion]].

## Dependencias

- Decisión abierta «CI en GitHub Actions» ([[pendientes]] §4) → bloquea la aprobación de [[hu-001-integracion-continua|HU-001]].
- Decisión abierta «Enrutador del frontend y librería de estado de servidor» ([[pendientes]] §4) → bloquea la implementación de rutas de [[hu-002-shell-y-navegacion-por-rol|HU-002]].
- [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]] (contrato de `GET /api/auth/me`) para la navegación por rol de HU-002.

## Historias de usuario

- [[hu-001-integracion-continua|HU-001 — Integración continua en cada PR]]
- [[hu-002-shell-y-navegacion-por-rol|HU-002 — Shell de la aplicación y navegación por rol]]

## Criterio de completitud

- [ ] HU-001 y HU-002 están `Completada` con su matriz de evidencia en `Cumple`.
- [ ] Un PR de prueba que viola una regla de arquitectura (backend) o una frontera MVC (frontend) queda en rojo en CI.
- [ ] El shell redirige a la entrada cuando la API responde 401 y muestra «sin permiso» ante 403, y las superficies Portal/Espacio interno están separadas por rol.
- [ ] Las decisiones abiertas de CI y de enrutador/estado están cerradas mediante ADR y tachadas en [[pendientes]].
- [ ] No quedan dependencias bloqueantes dentro del alcance.

## Riesgos e incógnitas

- **Decisiones abiertas sin cerrar**: si no se decide CI ni enrutador, el resto de la Fase 0 avanza sin red de seguridad y con navegación provisional.
- **Protección de `main`**: exigir los checks de CI depende de la configuración del repositorio en GitHub y de conocer los usuarios del equipo ([[pendientes]] §5).
- **Pruebas de integración con PostgreSQL**: el primer proyecto de integración llega con [[hu-003-iniciar-y-cerrar-sesion|HU-003]]; el CI debe poder ofrecer una base PostgreSQL (servicio del job o Testcontainers) sin credenciales reales.
- **Testing Library + jsdom** aún no está incorporado ([[estrategia-de-pruebas]]); HU-002 es la primera vista con interacción que lo necesita.

## Relacionado

- [[tablero-scrum]] · [[fases-y-alcance]] · [[flujo-de-trabajo-github]] · [[estrategia-de-pruebas]] · [[frontend-mvc]] · [[backend-hexagonal]] · [[entorno-docker]] · [[adr-0001-monorepo-contenedorizado]] · [[adr-0003-frontend-mvc]]

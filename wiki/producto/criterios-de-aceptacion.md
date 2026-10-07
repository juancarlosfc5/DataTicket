---
title: Criterios de aceptación del MVP
type: concepto
status: vigente
tags: [producto, producto/aceptacion]
sources: ["PRD.md §14"]
aliases: [Criterios de aceptación, CA, Definition of Done del MVP]
created: 2026-10-07
updated: 2026-10-07
---

# Criterios de aceptación del MVP

Los 15 criterios que el MVP debe cumplir (PRD §14), como lista de verificación. Se marca `[x]` un criterio cuando está implementado **y** verificado con pruebas; junto a la marca se enlaza la prueba o el PR que lo demuestra.

## Lista de verificación

- [ ] **CA-01** — Una cuenta de cliente de la empresa A no puede listar, consultar ni descargar desde el portal un ticket de la empresa B; el control se verifica en backend.
  - Página: [[roles-y-permisos]]
- [ ] **CA-02** — El solicitante ve sus tickets y el coordinador los de su empresa; ambos ven solo estado resumido, datos de radicación y respuesta formal final.
  - Página: [[roles-y-permisos]] · [[estados-del-ticket]]
- [ ] **CA-03** — Un ticket nuevo aparece en la cola global de Elizabeth. El administrador de cobertura ve solo los campos de cola y, al abrirlo, queda asociado antes de recibir descripción, archivos y chat.
  - Página: [[roles-y-permisos]] · [[flujo-del-ticket]]
- [ ] **CA-04** — Elizabeth puede asignar una o varias personas, reasignar y agregar/quitar participantes; cada acción queda auditada.
  - Página: [[flujo-del-ticket]] · [[auditoria]]
- [ ] **CA-05** — Los participantes autorizados reciben los mensajes en tiempo real mientras están conectados; al refrescar o reconectar recuperan los perdidos desde PostgreSQL.
  - Página: [[chat-interno]] · [[tiempo-real-signalr]]
- [ ] **CA-06** — Un no participante no puede unirse al grupo, cargar historial, enviar mensajes ni marcar lecturas. Retirar a alguien revoca su acceso en conexiones existentes y futuras.
  - Página: [[chat-interno]] · [[roles-y-permisos]]
- [ ] **CA-07** — Cada confirmación de lectura identifica persona, mensaje y fecha/hora, y sigue disponible tras cerrar sesión y volver a entrar.
  - Página: [[chat-interno]]
- [ ] **CA-08** — Una persona agregada más tarde consulta el historial completo del chat; una persona retirada no puede volver a consultarlo.
  - Página: [[chat-interno]]
- [ ] **CA-09** — El chat acepta texto e imágenes. Radicación, chat y respuesta formal aceptan PDF, imágenes, XML y Excel de hasta 10 MB por archivo; el backend rechaza formatos y tamaños no permitidos.
  - Página: [[archivos-adjuntos]]
- [ ] **CA-10** — El cliente no ve conversaciones internas, estados de Desarrollo/PR/Producción, nombres de participantes internos ni enlaces internos de PR.
  - Página: [[estados-del-ticket]] · [[roles-y-permisos]]
- [ ] **CA-11** — Solo Elizabeth o un administrador publica la respuesta formal; la respuesta completa y sus adjuntos quedan en el portal y se envían por correo.
  - Página: [[flujo-del-ticket]] · [[notificaciones]]
- [ ] **CA-12** — El ticket pasa a cerrado únicamente por acción manual de Elizabeth o un administrador tras la confirmación telefónica; no hay cierre automático ni recordatorio de vencimiento.
  - Página: [[flujo-del-ticket]] · [[estados-del-ticket]]
- [ ] **CA-13** — El dashboard interno presenta cola, estado, área, asignaciones, antigüedad, carga y tiempos; el cliente no recibe metas de SLA no acordadas.
  - Página: [[dashboard-y-metricas]]
- [ ] **CA-14** — Cambios de estado, participantes, prioridad, asignación y cierre manual conservan actor y fecha/hora en la auditoría.
  - Página: [[auditoria]]
- [ ] **CA-15** — Los formularios por cliente no bloquean el piloto: el piloto opera con el formulario común y la personalización es una fase separada.
  - Página: [[formularios-configurables]] · [[fases-y-alcance]]

## Cómo usar esta página

- Al planificar un cambio, identifica los CA involucrados (protocolo de `AGENTS.md` §4).
- Los CA de aislamiento (CA-01, CA-02, CA-06, CA-10) deben probarse contra el **backend**, no solo la interfaz (PRD §5, §12).
- Si un CA resulta ambiguo al implementarlo, regístralo en [[pendientes]] en vez de reinterpretarlo en silencio.

## Relacionado

- [[fases-y-alcance]] · [[flujo-del-ticket]] · [[roles-y-permisos]] · [[chat-interno]]
- [[archivos-adjuntos]] · [[auditoria]] · [[dashboard-y-metricas]] · [[pendientes]] · [[fuente-prd-v0-1]]

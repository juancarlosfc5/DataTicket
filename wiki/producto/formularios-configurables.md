---
title: Formularios configurables
type: concepto
status: vigente
tags: [producto, producto/formularios]
sources: ["PRD.md §9", "PRD.md §6.1", "PRD.md §1", "PRD.md §4", "PRD.md §13", "PRD.md §14"]
aliases: [Formulario común, Plantillas de formulario, Constructor de formularios, FormTemplate]
created: 2026-10-07
updated: 2026-10-07
---

# Formularios configurables

El piloto opera con un **formulario común** para todos los clientes. La personalización de formularios por cliente es la **última fase comprometida** y no debe bloquear el piloto (PRD §1, §9, §13).

## Fase piloto: formulario común (PRD §6.1, §9)

| Campo | Regla |
|---|---|
| Categoría | Catálogo de valores no definido en el PRD |
| Título | Hasta 120 caracteres |
| Descripción | Texto del caso |
| Urgencia | Baja, media o alta |
| Impacto | Bajo, medio o alto |
| Prioridad calculada | Derivada de urgencia e impacto; ver [[matriz-de-prioridad]] |
| Adjuntos | Opcionales; ver [[archivos-adjuntos]] |

- **Empresa y solicitante** se derivan de la cuenta autenticada; no son campos del formulario (PRD §6.1, §9).
- **No** se crea un selector genérico de producto (PRD §9).
- Los campos de ejemplo del prototipo (sede, servicio afectado, planta, centro de costo, bodega, número de pedido, sección o periodo académico, placa de vehículo…) son **semillas de diseño**, no valores reales ni campos obligatorios (PRD §9).

## Última fase: formularios por cliente (PRD §9)

| Capacidad | Detalle |
|---|---|
| Constructor | Administrado desde DataTicket |
| Campos configurables | Tipo, etiqueta, obligatoriedad, ayuda y opciones |
| Asociación | Plantilla (`FormTemplate`) asociada a una empresa, con orden visible de campos |
| Versionado | Cada ticket conserva los campos y la versión de formulario con que se creó |
| Datos | Se conservan los campos núcleo; respuestas variables en PostgreSQL, con capacidad de consultar los campos usados operativamente |

- No se requiere que cada cliente cree o modifique sus propias plantillas (PRD §9).
- Su construcción no debe bloquear el piloto (PRD §9, §14.15).

## Por qué este orden

Validar el proceso común antes de construir formularios configurables es un objetivo explícito del producto (PRD §4). La personalización avanzada se aplazó por decisión del dueño del producto (PRD §2).

> [!question] Pendiente
> - El PRD no define el catálogo de **categorías** del formulario común ni quién lo administra.
> - No se especifica cómo se almacenan las respuestas variables (p. ej. columnas JSON vs. tablas) — es decisión técnica de la Fase 4; ver [[persistencia-postgresql]].
> Ver [[pendientes]].

## Relacionado

- [[fases-y-alcance]] · [[flujo-del-ticket]] · [[matriz-de-prioridad]] · [[archivos-adjuntos]]
- [[modelo-de-dominio]] · [[persistencia-postgresql]] · [[criterios-de-aceptacion]] · [[glosario]]

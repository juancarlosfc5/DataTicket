---
title: "ADR-0006: URLs públicas permanentes en Azure Blob"
type: decision
status: aceptada
tags: [decision, seguridad, archivos]
sources: ["PRD.md §8, §12, §16.6"]
aliases: [ADR-0006]
created: 2026-10-07
updated: 2026-10-07
decision_date: 2026-10-05
deciders: [Dueño del producto (PRD)]
---

# ADR-0006: URLs públicas permanentes en Azure Blob

## Contexto

Los adjuntos (radicación, chat, respuesta formal) se guardan en Azure Blob Storage con metadatos en PostgreSQL (PRD §8).

## Decisión (tomada en el PRD)

Se usará una **URL pública permanente** por archivo (PRD §8).

> [!danger] Riesgo aceptado
> El acceso al archivo **no hereda la autorización del portal**: cualquiera con la URL puede abrirlo mientras exista, y desactivar un usuario no invalida enlaces ya compartidos. Microsoft recomienda evitar acceso anónimo y preferir Entra ID o SAS de corta duración (PRD §8). CA-01 solo protege la descarga *desde el portal*.

## Consecuencias

- Las URLs se tratan como enlaces compartibles, no como autenticación.
- Antes de un storage productivo falta: aprobación explícita del riesgo por la dirección responsable de los datos, y definir quién administra cuenta, costos, respaldos y eliminación (PRD §8, §16.6) → [[pendientes]].
- En local, Azurite emula el Blob; `Storage:Blob:PublicBaseUrl` produce URLs alcanzables desde el navegador ([[entorno-docker]]).
- Cambiar a SAS o Entra ID requeriría una decisión de producto nueva (un ADR que reemplace a este).

## Relacionado

- [[archivos-adjuntos]] · [[roles-y-permisos]] · [[pendientes]]

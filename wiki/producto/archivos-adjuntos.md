---
title: Archivos adjuntos
type: concepto
status: vigente
tags: [producto, producto/archivos]
sources: ["PRD.md §8", "PRD.md §6.1", "PRD.md §6.5", "PRD.md §7", "PRD.md §10", "PRD.md §12", "PRD.md §14", "PRD.md §16"]
aliases: [Adjuntos, Archivos, Attachment]
created: 2026-10-07
updated: 2026-10-07
---

# Archivos adjuntos

Reglas para los archivos que acompañan la radicación, el chat interno y la respuesta formal: tipos, tamaño, validación, dónde se guardan y el riesgo aceptado de las URL públicas (PRD §8).

## Dónde se adjuntan

| Contexto | Quién adjunta | Lo ve el cliente |
|---|---|---|
| Radicación | Solicitante o coordinador (PRD §6.1) | Se entiende que sí, como parte de los datos de radicación (PRD §10) |
| Chat interno | Participantes internos (PRD §7) | No: son adjuntos internos (PRD §10) |
| Respuesta formal | Elizabeth o un administrador (PRD §6.5) | Sí, en el portal y por correo (PRD §6.5, §11) |

## Restricciones

| Regla | Valor |
|---|---|
| Tipos admitidos | PDF, imágenes, XML y Excel (PRD §8) |
| Tamaño máximo | **10 MB por archivo** (PRD §8) |
| Varios archivos | Permitido donde la interfaz lo indique (PRD §8) |
| Validación | **En backend**; la del navegador es solo ayuda de interfaz (PRD §8) |
| Rechazo | Formatos o tamaños no permitidos se rechazan en backend (PRD §14.9) |

## Almacenamiento

- PostgreSQL guarda **metadatos y referencias**, nunca el binario (PRD §8). Ver [[persistencia-postgresql]].
- Almacenamiento objetivo de los binarios: **Azure Blob Storage** (PRD §8).
- El prototipo no demuestra ninguna integración con Blob Storage (PRD §8).
- Entorno local: ver [[entorno-docker]].

> [!danger] Riesgo aceptado
> Se acordó usar una **URL pública permanente** por archivo (PRD §8). Consecuencias:
> - El acceso al archivo **no hereda** la autorización del portal: quien conozca o reciba la URL puede abrirlo mientras el objeto exista.
> - Dar de baja a un usuario no invalida una URL que ya se compartió.
> - No es una garantía de privacidad por empresa; es la excepción conocida al aislamiento multiempresa (PRD §12).
> - Las URL deben tratarse como enlaces compartibles, no como autenticación.
>
> Microsoft recomienda evitar el acceso anónimo y preferir Microsoft Entra ID o URL SAS limitadas; cambiar a ese modelo requiere una decisión de producto posterior (PRD §8). Decisión registrada en [[adr-0006-urls-publicas-azure-blob]].

## Antes de un storage productivo

Data Global debe (PRD §8, §16):

- Confirmar que puede almacenar los archivos bajo esta política pública.
- Definir quién administra la cuenta, costos, respaldos y eliminación de objetos.
- Obtener revisión y aprobación explícitas del riesgo por la dirección responsable de los datos (PRD §16).

## Criterios de aceptación ligados

- **CA-01**: un cliente de la empresa A no puede descargar **desde el portal** un ticket de la empresa B (PRD §14.1).
- **CA-09**: tipos y tamaño validados en backend para radicación, chat y respuesta formal (PRD §14.9).
- **CA-11**: la respuesta formal y sus adjuntos quedan en el portal y se envían por correo (PRD §14.11).

> [!question] Pendiente
> El PRD no concreta qué formatos de imagen y de Excel se aceptan (extensiones / tipos MIME) ni si se valida el contenido real del archivo además de la extensión. Ver [[pendientes]].

## Relacionado

- [[adr-0006-urls-publicas-azure-blob]] · [[persistencia-postgresql]] · [[entorno-docker]]
- [[chat-interno]] · [[flujo-del-ticket]] · [[roles-y-permisos]] · [[notificaciones]]
- [[criterios-de-aceptacion]] · [[modelo-de-dominio]] · [[glosario]]

# DataTicket — Product Requirements Document

**Empresa:** Data Global S.A.S.  
**Versión:** 0.1 — borrador funcional para revisión  
**Fecha:** 5 de octubre de 2026  
**Estado:** Base de producto acordada; algunos aspectos operativos quedan pendientes de validar  
**Nombre del producto:** DataTicket

## 1. Resumen ejecutivo

DataTicket será el portal web de soporte para los clientes que adquirieron productos de desarrollo de Data Global. Centralizará la radicación de solicitudes, la clasificación por parte de la PM, la asignación de responsables, el trabajo entre Desarrollo y Producción y la entrega formal de la solución al cliente.

El producto tendrá dos superficies relacionadas, pero con permisos distintos:

1. **Portal del cliente:** permite radicar solicitudes, consultar el estado resumido de sus tickets y recibir la respuesta formal final.
2. **Espacio de trabajo de Data Global:** permite hacer triage, asignar varias personas a un ticket, coordinar el trabajo interno mediante un chat persistente en tiempo real y preparar la respuesta formal.

La prioridad de producto es que el ticket sea trazable y que la colaboración interna sea funcional. La personalización de formularios por cliente se implementará en la última fase, una vez esté construido y validado el flujo principal.

## 2. Fuentes, alcance y precedencia

Este PRD consolida la conversación de producto y las dos fuentes compartidas:

- `mvp-sistema-tickets.md` es un borrador del desarrollador que se identifica como pendiente de validación. Sus recomendaciones se usan como insumo, no como decisiones aprobadas por sí mismas.
- `DataTicket.html` es un prototipo navegable con datos y comportamiento simulados en el navegador. Sirve como referencia de pantallas y campos; sus cuentas, empresas, tickets y datos de ejemplo no son datos productivos ni una implementación de backend.
- Las decisiones expresas del dueño del producto en esta conversación prevalecen cuando difieren de esos insumos. En particular, el chat en tiempo real sí forma parte de la prioridad del producto; la personalización avanzada de formularios se aplaza; y el cierre lo ejecuta manualmente la PM o un administrador después de confirmar verbalmente con el cliente.

El ZIP de diseño mencionado por el equipo podrá complementar el prototipo visual cuando esté disponible. No se requiere para definir el alcance funcional de este documento.

## 3. Problema y oportunidad

Las solicitudes de soporte requieren un lugar único donde el cliente pueda radicarlas y Data Global pueda seguirlas desde la recepción hasta el cierre. Durante la atención participan personas de distintas áreas; parte de la información corresponde a coordinación interna y no debe mostrarse al cliente. Al mismo tiempo, el cliente necesita saber que el caso fue recibido, si sigue en atención y cuál fue la solución formal.

DataTicket busca reducir solicitudes dispersas, pérdidas de contexto al cambiar de responsable y falta de visibilidad operativa. La coordinación entre participantes y la trazabilidad del ticket son requisitos centrales, no complementos opcionales.

## 4. Objetivos y señales de éxito

### Objetivos del producto

1. Permitir a clientes autorizados radicar tickets con contexto y archivos relevantes.
2. Dar a Elizabeth, PM, una cola central de triage y una forma auditable de asignar y reasignar tickets.
3. Permitir colaborar entre varias personas de Desarrollo y Producción sin exponer el chat interno a los clientes.
4. Mantener historial durable de mensajes, participantes, adjuntos, cambios de estado y respuesta final.
5. Asegurar que los usuarios cliente consulten únicamente tickets de su propia empresa según su rol.
6. Dar a la PM y a los equipos una vista operativa de la carga, antigüedad, estado y tiempos de los tickets.
7. Validar el proceso común antes de construir formularios configurables por cliente.

### Métricas que se instrumentarán en el MVP

| Métrica | Definición inicial |
|---|---|
| Tickets recibidos | Cantidad de tickets creados en un periodo, con filtros por estado, empresa y área |
| Tickets sin asignar | Tickets en cola de triage sin responsable operativo |
| Antigüedad | Tiempo desde creación hasta el momento de consulta o cierre |
| Carga por responsable | Tickets activos asociados a cada persona |
| Tiempo hasta respuesta formal | Tiempo entre radicación y envío de la respuesta formal final |
| Tiempo hasta cierre | Tiempo entre radicación y cierre manual del ticket |

El MVP medirá y mostrará estos tiempos, pero no prometerá SLA ni metas numéricas. El cómputo usa zona horaria `America/Bogota`, cuenta de lunes a viernes, excluye sábados y domingos y **cuenta los festivos**. Las metas se definirán después de obtener una línea base operativa.

## 5. Usuarios, equipos y permisos

### Usuarios y responsabilidades

| Usuario o perfil | Responsabilidad y acceso |
|---|---|
| Solicitante de cliente | Crea tickets para sí mismo y consulta sus propios tickets, estado resumido y respuesta formal final. No accede al chat interno. |
| Coordinador de empresa cliente | Consulta los tickets de su empresa completa, además de poder radicar solicitudes. |
| Elizabeth — Product Manager | Responsable de triage; puede consultar todos los tickets, asignar y reasignar personas, gestionar participantes y emitir respuesta formal final. |
| Administrador de DataTicket | Administra el sistema y puede agregar o retirar participantes. No obtiene acceso automático al contenido del chat por ser administrador. Puede cubrir triage con el flujo descrito abajo. |
| Desarrollo | Trabaja los tickets asociados a sus integrantes y participa en el chat interno del ticket. |
| Producción | Recibe los casos que corresponden a producción; Julián lidera esta área. |

### Equipos iniciales

- **Desarrollo:** Juan David, Laura, Brayan, Cristian y Kevin.
- **Producción:** Julián, Elizabeth y Gerardo.
- Elizabeth ejerce la responsabilidad de PM y también pertenece a Producción.

La lista inicial de personas sirve para el piloto. Usuarios y asignaciones futuras se administran dentro de DataTicket.

### Reglas de autorización

1. Los usuarios cliente solo consultan tickets de la empresa a la que pertenece su cuenta. El solicitante ve los propios; el coordinador ve los de toda su empresa.
2. La autorización se aplica en servidor en cada operación. Ocultar una fila en la interfaz no constituye aislamiento.
3. Solo las personas participantes vigentes pueden leer o escribir en el chat de un ticket. Elizabeth siempre puede acceder para hacer triage y seguimiento.
4. La PM y los administradores pueden agregar o retirar participantes. Agregar a alguien le da acceso al historial completo del chat. Retirarlo revoca su acceso futuro; se conserva su participación pasada en el historial y auditoría.
5. Un administrador no obtiene acceso general a mensajes y archivos por tener privilegios de administración. Durante la ausencia de Elizabeth puede consultar la cola de triage, que muestra número, empresa, título, categoría, urgencia, impacto, prioridad calculada, fecha de creación y estado resumido. No muestra descripción completa, archivos ni chat. Al abrir/tomar un ticket, la acción lo asocia como participante antes de cargar el detalle y sus adjuntos.
6. Los clientes nunca ven mensajes internos, estados técnicos, URL de pull request, nombres de colaboradores internos ni actividad privada de otros clientes.
7. Solo Elizabeth o un administrador puede enviar la respuesta formal final al cliente.

Las cuentas de empresas cliente las crea, invita y desactiva Data Global. No hay registro público de clientes. El prototipo incluye recuperación de contraseña; la implementación productiva debe utilizar un flujo seguro de invitación y restablecimiento, sin revelar si un correo está registrado. MFA queda fuera del MVP.

## 6. Flujo funcional del ticket

### 6.1 Radicación

El solicitante inicia sesión en el portal y crea un ticket con el formulario común del piloto. La cuenta determina la empresa y el usuario que radica; esos datos no se eligen libremente en el formulario.

El formulario común contiene:

- Categoría.
- Título de hasta 120 caracteres.
- Descripción del caso.
- Urgencia: baja, media o alta.
- Impacto: bajo, medio o alto.
- Prioridad calculada a partir de urgencia e impacto.
- Archivos adjuntos opcionales.

La prioridad calculada utiliza la matriz de referencia del prototipo y el borrador del desarrollador:

| Impacto \ Urgencia | Baja | Media | Alta |
|---|---:|---:|---:|
| Bajo | Muy baja | Baja | Media |
| Medio | Baja | Media | Alta |
| Alto | Media | Alta | Crítica |

Si un integrante de Data Global ajusta manualmente la prioridad, debe quedar registrada la prioridad calculada, la nueva prioridad, quién hizo el cambio, cuándo y el motivo.

### 6.2 Triage y asignación

1. Al radicarse, el ticket entra en la cola general de Elizabeth con estado interno **Nuevo / pendiente de triage**.
2. Elizabeth revisa categoría, urgencia e impacto y asigna una o varias personas. Puede asignarlo directamente a Julián si corresponde a Producción.
3. Si Elizabeth está ausente, el administrador ve la cola de triage con número, empresa, título, categoría, urgencia, impacto, prioridad calculada, fecha de creación y estado resumido. Al abrir/tomar un caso, el sistema lo añade como participante antes de cargar la descripción completa, adjuntos o chat.
4. Elizabeth o un administrador puede añadir más personas a lo largo de la vida del ticket, o retirar participantes. Cada cambio queda auditado.
5. La asignación a un equipo no sustituye la lista de participantes: el acceso al detalle y al chat es por asociación explícita, excepto Elizabeth.

### 6.3 Trabajo de Desarrollo y Producción

- **Desarrollo:** uno o más desarrolladores trabajan el ticket y coordinan en el chat privado. Si se genera un pull request, una persona autorizada agrega manualmente su URL.
- **Revisión de PR:** Julián lidera la revisión cuando el ticket requiere paso a Producción. La URL se muestra a participantes internos autorizados.
- **Producción:** Julián lidera la puesta en producción. El desarrollador que realizó el cambio puede permanecer asociado como colaborador.
- **Ruta directa:** los tickets que correspondan a Producción pueden asignarse directamente a Julián, sin pasar por Desarrollo.
- La integración automática con GitHub, GitLab u otro proveedor no forma parte del MVP; los repositorios son privados y el vínculo se maneja como URL manual.

### 6.4 Estados internos y estado del cliente

Los estados internos describen el trabajo real; el portal usa estados resumidos seguros para clientes.

| Estado interno | Significado | Estado resumido para el cliente |
|---|---|---|
| Nuevo / pendiente de triage | Recibido y esperando clasificación/asignación | Recibido |
| En desarrollo | Análisis o implementación por Desarrollo | En atención |
| Revisión de PR | Revisión técnica previa a Producción | En atención |
| En producción | Despliegue o validación en Producción | En atención |
| Solución entregada | La PM o administrador envió la respuesta formal final | Solución entregada |
| Cerrado | La PM o administrador cerró manualmente después de confirmación verbal | Cerrado |

El cliente no ve si el trabajo está en Desarrollo, revisión o Producción. Los movimientos internos, reasignaciones y cambios de participantes se registran en la bitácora.

### 6.5 Respuesta formal y cierre

- La respuesta formal es el canal del producto para comunicar al cliente el resultado. Incluye cuerpo de texto y archivos; queda disponible en el portal y se envía completa por correo.
- Solo Elizabeth o un administrador puede emitirla. Los desarrolladores pueden preparar contexto y adjuntos en el chat, pero no enviar directamente la comunicación final.
- Tras entregar la solución, Elizabeth confirma verbalmente por teléfono con el cliente que el caso quedó resuelto. El sistema no graba ni transcribe la llamada.
- Elizabeth o un administrador registra el cierre manualmente. No hay cierre automático, temporizador de vencimiento ni recordatorios de cierre.
- El registro conserva quién cerró, cuándo y el estado anterior. La existencia de una conversación telefónica se representa únicamente por el acto de cierre manual, sin almacenar detalles de la llamada.

## 7. Chat interno en tiempo real

### Objetivo y alcance

Cada ticket tiene un chat privado de texto e imágenes para las personas internas asociadas. El cliente no participa ni puede consultar el chat; su comunicación en DataTicket se limita a la radicación, el estado resumido y la respuesta formal final.

### Requisitos funcionales

1. Un mensaje enviado por una persona autorizada aparece en tiempo real para las demás personas conectadas al ticket.
2. El servidor guarda cada mensaje en PostgreSQL antes de confirmar su publicación. SignalR distribuye el evento, pero no es el almacenamiento del historial.
3. Una persona añadida posteriormente obtiene el historial completo disponible del chat, incluidos mensajes anteriores y adjuntos.
4. Cada participante puede ver quién leyó cada mensaje y en qué fecha/hora. Las confirmaciones de lectura se persisten por mensaje y persona.
5. Una persona retirada deja de leer, enviar, marcar lectura o reconectarse al chat de ese ticket. La auditoría conserva sus mensajes y su anterior participación.
6. Los mensajes nuevos generan notificación dentro de la aplicación. No se envía un correo por cada mensaje del chat.
7. Cuando una persona es asociada por primera vez al ticket recibe un correo de vinculación/asignación. Ese correo no sustituye el control de acceso del portal.
8. El chat acepta texto y adjuntos de los tipos y tamaños definidos en la sección 8.

### Decisión de transporte y componentes

Se utilizará **ASP.NET Core SignalR** dentro del backend .NET 10 y el cliente oficial `@microsoft/signalr` en React/TypeScript. SignalR usará WebSockets cuando estén disponibles y puede recurrir a otros transportes compatibles; no se implementará el protocolo WebSocket de forma manual. Microsoft recomienda SignalR para la mayoría de las aplicaciones frente a WebSockets en crudo y documenta su fallback de transportes.

Flujo recomendado:

1. React consulta por API el historial y su cursor más reciente.
2. El cliente establece conexión con el hub de tickets autenticado.
3. Solicita unirse al ticket. El servidor valida la autorización vigente antes de agregar esa conexión al grupo de distribución del ticket.
4. Al enviar un mensaje, el servidor vuelve a comprobar la membresía, persiste mensaje y adjuntos, y luego publica el evento en el grupo autorizado.
5. Al marcar como leído, el servidor persiste persona, mensaje y hora; publica la actualización a los participantes autorizados.
6. Después de reconexión, el cliente vuelve a autenticarse, solicita unirse de nuevo y recupera desde PostgreSQL los mensajes posteriores a su cursor. La conexión en tiempo real no reemplaza la consulta histórica.

Los grupos SignalR solo enrutan eventos; no son una barrera de seguridad. La pertenencia se verifica en el servidor en cada unión y operación relevante. Si se retira una persona mientras sigue conectada, sus conexiones activas se deben desconectar o dejar sin autorización antes de permitir nuevas operaciones.

Para la primera instalación de una sola instancia no se requiere Redis ni un backplane. El escalamiento a varias instancias deberá incorporar un mecanismo de distribución compatible (por ejemplo, Redis self-hosted o Azure SignalR según el despliegue), decisión que no es necesaria para el MVP local.

### Interfaces técnicas propuestas

- Hub autenticado: `/hubs/tickets`.
- Operaciones conceptuales del hub: unirse/salir del ticket, publicar mensaje y marcar lectura.
- API de historial: consultar mensajes paginados por cursor y recuperar mensajes faltantes tras reconexión.
- Eventos del servidor: mensaje creado y confirmación de lectura actualizada.

Los nombres de métodos y el esquema JSON definitivo se fijarán en el diseño de API durante la construcción. El requisito invariable es autorizar por ticket y usuario en servidor y mantener el historial persistido y recuperable.

## 8. Archivos y almacenamiento

### Requisitos

- Archivos admitidos para radicación, chat y respuesta formal: PDF, imágenes, XML y Excel.
- Tamaño máximo: 10 MB por archivo. Se permite adjuntar varios archivos donde la interfaz lo indique.
- El backend debe aplicar las restricciones; la validación del navegador es solo una ayuda de interfaz.
- La aplicación conserva en PostgreSQL los metadatos y referencias de archivo, no el binario.
- Almacenamiento objetivo: Azure Blob Storage.

### Decisión de acceso y riesgo registrado

Se acordó utilizar una URL pública permanente para los archivos. Por tanto, el acceso al archivo no hereda la autorización del portal: cualquier persona que conozca o reciba la URL puede abrirlo mientras el objeto siga disponible. La baja de un usuario de DataTicket no invalida una URL pública que ya se compartió.

Este comportamiento es un riesgo explícito aceptado para el alcance acordado, no una garantía de privacidad por empresa. Las URLs deben tratarse como enlaces compartibles, no como autenticación. La recomendación de seguridad vigente de Microsoft es evitar acceso anónimo y preferir Microsoft Entra ID o URL SAS de duración y permisos limitados; cambiar a esos modelos requeriría una decisión de producto posterior.

Antes de conectar un storage productivo, Data Global debe confirmar que puede almacenar esos archivos bajo esta política pública y definir quién administra la cuenta, costos, respaldos y eliminación de objetos. El prototipo no demuestra ninguna integración con Blob Storage.

## 9. Formularios y campos

### Fase piloto: formulario común

El piloto usa un formulario común con categoría, título, descripción, urgencia, impacto y archivos. Empresa y solicitante se derivan de la cuenta autenticada. No se crea un selector genérico de producto.

El prototipo presenta ejemplos de campos que podrían variar por organización: sede/centro operativo, servicio afectado, planta u oficina, centro de costo, bodega, número de pedido, sección académica, periodo académico o placa de vehículo. Estos son semillas de diseño, no valores reales ni campos obligatorios del formulario común.

### Última fase antes de ampliación

Una vez construido y validado el producto principal se implementará la personalización de formularios por cliente. La fase incluirá:

- Constructor de formularios administrado desde DataTicket.
- Campos configurables por cliente, con tipo, etiqueta, obligatoriedad, ayuda y opciones.
- Asociación de plantilla a empresa y orden visible de sus campos.
- Versionado: cada ticket conserva los campos y la versión de formulario con que se creó.
- Conservación de campos núcleo y soporte para almacenar respuestas variables en PostgreSQL, con capacidad de consultar campos usados operativamente.

La construcción de esta fase no debe bloquear el piloto con formulario común. No se requiere habilitar a cada cliente a crear o modificar sus plantillas.

## 10. Bandejas, portal y dashboard

### Espacio interno

- Elizabeth cuenta con una bandeja global para triage y seguimiento.
- El administrador de cobertura ve número, empresa, título, categoría, urgencia, impacto, prioridad calculada, fecha de creación y estado resumido; la descripción completa, archivos y chat se muestran después de asociarse al ticket.
- Desarrollo y Producción usan bandejas por equipo y responsabilidad. El detalle/chat requiere asociación al ticket, además de la pertenencia al equipo.
- Los paneles muestran tickets por estado y área, tickets sin asignar, antigüedad y carga por responsable.
- Se muestran tiempos hasta respuesta formal y cierre manual sin semáforos SLA ni metas numéricas.
- El panel debe diferenciar datos globales de PM, datos del equipo interno y tickets visibles a una empresa cliente.

### Portal de cliente

- El solicitante consulta los tickets creados por sí mismo.
- El coordinador consulta los tickets de toda su empresa.
- Cada ticket muestra estado resumido, datos de radicación y la respuesta formal final cuando esté disponible.
- No se exponen chat, notas internas, adjuntos internos, participantes de Data Global ni estados de Desarrollo/PR/Producción.

Listados exportables y vistas guardadas aparecen como propuestas del borrador del desarrollador, pero no son bloqueantes para el flujo principal descrito en este PRD. Se podrán priorizar después del piloto, una vez que los equipos validen qué filtros y exportaciones necesitan realmente.

## 11. Notificaciones

| Evento | Canal y destinatario |
|---|---|
| Nuevo mensaje en el chat interno | Notificación dentro de la aplicación para participantes del ticket; sin correo por mensaje |
| Primera asociación de una persona a un ticket | Correo de vinculación a esa persona |
| Respuesta formal final | Aparece en el portal y se envía por correo al contacto de cliente correspondiente, con texto completo y archivos asociados |
| Cambio de estado interno | Se refleja en las bandejas internas; no se revela el estado técnico por correo al cliente |
| Cambio de estado resumido del cliente | Se refleja en el portal; el correo automático por cada transición no está confirmado y queda fuera de la garantía del MVP |

La política debe evitar notificar al usuario retirado de un ticket por mensajes futuros. El registro de auditoría, y no el correo, es la fuente de trazabilidad.

## 12. Seguridad, privacidad y auditoría

### Aislamiento multiempresa

- Cada ticket pertenece a una empresa cliente.
- Toda consulta del portal se restringe por empresa y permisos del usuario en el servidor.
- La capa de datos debe prevenir consultas accidentales entre empresas; se recomienda evaluar filtros globales y seguridad a nivel de fila de PostgreSQL.
- Los archivos públicos constituyen la excepción conocida: su enlace puede compartirse fuera de la autorización multiempresa del portal.

### Auditoría

La bitácora es append-only desde la aplicación. Debe registrar al menos actor, fecha/hora, acción, objeto, valores anterior/nuevo cuando aplique y resultado. Eventos relevantes: radicación, cambio de prioridad/estado, asignación, reasignación, incorporación/retiro de participantes, respuesta formal y cierre manual. La participación histórica de una persona retirada permanece registrada.

### Requisitos no funcionales mínimos

- Todo acceso exige autenticación, salvo páginas de entrada expresamente públicas.
- Contraseñas se gestionan mediante una solución estándar de identidad con derivación segura de contraseña, no con cifrado reversible o hash casero.
- La recuperación de contraseña utiliza token de un solo uso y evita confirmar si una cuenta existe.
- Transporte productivo cifrado por HTTPS.
- Validación de pertenencia a ticket en todas las rutas de chat e historial.
- Los datos de clientes de prueba son sintéticos; no se copian nombres, documentos ni conversaciones reales.
- La interfaz es web responsiva; aplicación móvil nativa fuera del MVP.
- Retención exacta de tickets, archivos y auditoría, frecuencia de respaldos, RPO/RTO y ubicación productiva quedan pendientes de decisión de infraestructura.

## 13. Fases y prioridades

| Fase | Resultado esperado | Prioridad |
|---|---|---|
| 0. Base técnica | Monolito modular .NET 10, React/TypeScript, PostgreSQL, autenticación, empresas, usuarios y permisos | Bloqueante |
| 1. Piloto operativo | Formulario común, radicación, adjuntos, triage, participantes, estados, auditoría y bandejas | P0 |
| 2. Colaboración y entrega | Chat SignalR persistente, adjuntos, lecturas, reconexión, respuesta formal, cierre manual y notificaciones acordadas | P0 |
| 3. Operación medible | Dashboard interno y portal con estados resumidos; medición de primera respuesta y cierre | P1 |
| 4. Formularios configurables | Constructor y plantillas específicas por cliente, versionadas, antes de ampliar el producto | Última fase comprometida |

### Fuera del MVP o diferido

- MFA, SSO y federación de identidad.
- Integración automática con GitHub/GitLab y sincronización de PR.
- Personalización de formularios durante el piloto (se implementa en la última fase indicada).
- Recepción de correo entrante y creación automática de tickets.
- Automatizaciones de asignación/escalamiento, SLA y recordatorios.
- Base de conocimiento, CMDB, encuesta de satisfacción, mensajería instantánea y app móvil nativa.
- Chat visible para clientes o respuestas públicas tipo conversación. El producto entrega al cliente una respuesta formal final.
- Redis/backplane para despliegue multiinstancia; se reconsidera al escalar.

## 14. Criterios de aceptación del MVP

1. Una cuenta de cliente de la empresa A no puede listar, consultar ni descargar desde el portal un ticket de la empresa B; el control se verifica en backend.
2. El solicitante ve sus tickets y el coordinador ve los de su empresa, pero ambos solo ven el estado resumido, sus propios datos de radicación y la respuesta formal final.
3. Un ticket nuevo aparece en la cola global de Elizabeth. El administrador de cobertura ve solo los campos de cola enumerados y, al abrirlo, queda asociado antes de recibir la descripción, los archivos y el chat.
4. Elizabeth puede asignar una o varias personas, reasignar y agregar/quitar participantes. Cada acción queda auditada.
5. Los participantes autorizados reciben en tiempo real los mensajes de chat mientras están conectados; al refrescar o reconectar recuperan mensajes perdidos desde PostgreSQL.
6. Un usuario no participante no puede unirse al grupo, cargar historial, enviar mensajes ni marcar lecturas. Retirarlo revoca el acceso en conexiones existentes y futuras.
7. Cada confirmación de lectura identifica persona, mensaje y fecha/hora y sigue disponible tras cerrar sesión y volver a entrar.
8. Una persona que se agrega más tarde puede consultar el historial completo del chat; una persona retirada no puede volver a consultarlo.
9. El chat acepta texto e imágenes. Radicación, chat y respuesta formal aceptan PDF, imágenes, XML y Excel de hasta 10 MB por archivo; se rechazan formatos/tamaños no permitidos en backend.
10. El cliente no ve conversaciones internas, estado de desarrollo/PR/producción, nombres de participantes internos ni enlaces internos de PR.
11. Solo Elizabeth o administrador puede publicar la respuesta formal. La respuesta completa y sus adjuntos quedan en el portal y se envían por correo.
12. La solución pasa a cerrado únicamente por acción manual de Elizabeth o administrador después de la confirmación telefónica; no hay cierre automático ni recordatorio de vencimiento.
13. Dashboard interno presenta cola, estado, área, asignaciones, antigüedad, carga y tiempos; el cliente no recibe metas de SLA que no han sido acordadas.
14. Cambios de estado, participantes, prioridad, asignación y cierre manual conservan su actor y fecha/hora en la auditoría.
15. Los formularios por cliente no bloquean el piloto: el piloto opera con el formulario común y la fase de personalización está separada.

## 15. Entorno local de desarrollo

El archivo `docker-compose.yml` de este entregable levanta **solo PostgreSQL 18.6**. El workspace no tiene todavía solución .NET, aplicación React ni Dockerfiles; por eso no afirma arrancar API o frontend. Cuando existan esos proyectos, se añadirá su configuración de servicios.

```powershell
docker compose up -d db
docker compose ps
docker compose logs -f db
```

Valores por defecto locales:

| Variable | Valor predeterminado | Uso |
|---|---|---|
| `POSTGRES_DB` | `dataticket` | Base inicial |
| `POSTGRES_USER` | `dataticket` | Usuario local |
| `POSTGRES_PASSWORD` | `dataticket_local_only` | Contraseña únicamente de desarrollo; sobreescribir en `.env` |
| `POSTGRES_PORT` | `5432` | Puerto expuesto en el equipo anfitrión |

Desde el host se conecta a `localhost:${POSTGRES_PORT}`. Desde un servicio añadido a este Compose se conecta al host `db`, puerto `5432`. El volumen `postgres_data` conserva la base entre reinicios y `docker compose down`; eliminarlo con `docker compose down -v` borra los datos locales.

Este Compose es una ayuda de desarrollo, no una configuración productiva. No incluye servicios de aplicación, certificados, respaldos, Azure Blob, correo real ni manejo de secretos productivos.

## 16. Decisiones pendientes antes de producción

Estas decisiones no impiden implementar el piloto funcional, pero deben resolverse antes de operar con clientes reales:

1. Infraestructura y propietario operativo de despliegue, respaldos, monitoreo y actualizaciones.
2. Proveedor, dominio remitente y configuración de entregabilidad de correo.
3. Retención/eliminación de tickets, mensajes, auditoría y archivos; y frecuencia de respaldo con objetivos de recuperación.
4. Si cliente necesita reportes exportables o vistas guardadas tras el piloto, y cuáles.
5. Volumen de tickets sintéticos a sembrar; la referencia acordada es aproximadamente 50 empresas de prueba con tickets en estados diversos, sin replicar información real.
6. Revisión y aprobación explícitas del riesgo de URL pública permanente de Azure Blob por la dirección responsable de los datos.

## 17. Stack y fuentes técnicas

Stack acordado: backend C# sobre .NET 10, frontend React con TypeScript, PostgreSQL y despliegue inicial local con Docker Compose. Google AI Studio se usa como origen de prototipado/interfaz y no como servicio productivo ni fuente de versiones de runtime.

Como referencia de actualidad al 5 de octubre de 2026, la política oficial lista .NET 10.0.12 como parche y LTS activo hasta el 14 de noviembre de 2028; React publica 19.3 como versión más reciente en su documentación; PostgreSQL publicó la versión de mantenimiento 18.6 el 13 de agosto de 2026. Estos datos de versión deben volver a verificarse al iniciar la implementación.

- [.NET: política oficial de soporte](https://dotnet.microsoft.com/en-us/platform/support/policy)
- [React: versiones](https://react.dev/versions) y [React 19.3](https://react.dev/blog/2026/09/09/react-19-3)
- [PostgreSQL: versión 18.6](https://www.postgresql.org/docs/current/release-18-6.html) y [política de versiones soportadas](https://www.postgresql.org/support/versioning/)
- [ASP.NET Core: soporte WebSockets y recomendación de SignalR sobre WebSockets en crudo](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-10.0)
- [SignalR: autenticación y autorización](https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-10.0)
- [SignalR: grupos no son una característica de seguridad](https://learn.microsoft.com/en-us/aspnet/core/signalr/groups?view=aspnetcore-10.0)
- [SignalR JavaScript: reconexión automática](https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client?view=aspnetcore-10.0)
- [SignalR: opciones de escalamiento](https://learn.microsoft.com/en-us/aspnet/core/signalr/scale?view=aspnetcore-10.0)
- [Imagen oficial de PostgreSQL: directorio persistente para PostgreSQL 18](https://github.com/docker-library/docs/blob/master/postgres/content.md#pgdata)
- [Azure Blob Storage: recomendaciones de seguridad](https://learn.microsoft.com/en-us/azure/storage/blobs/secure-blobs)

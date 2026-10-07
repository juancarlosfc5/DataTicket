# Validación y cierre de historias

## Principio

El cierre es una afirmación basada en evidencia. Una confirmación verbal de que “terminó” no demuestra los criterios.

## Evidencia prioritaria

1. código y configuración observables en `backend/` y `frontend/`;
2. pruebas relacionadas y sus resultados (xUnit v3, `DataTicket.ArchitectureTests`, Vitest);
3. CI o reportes existentes (cobertura en `backend/TestResults/`);
4. historial y diff de Git en lectura;
5. migraciones EF Core y contratos (OpenAPI en `/openapi/v1.json`, métodos/eventos del hub);
6. respuestas HTTP, encabezados y cookies cuando apliquen;
7. pruebas negativas de multiempresa y de participación en el chat;
8. páginas de la wiki y ADR aceptados.

No concluyas cumplimiento por nombres de archivos o intención aparente.

## Comandos de verificación permitidos

Solo los que no modifican archivos versionados ni datos del entorno:

```bash
cd backend && dotnet build
cd backend && dotnet test
docker build --target test ./backend
npm --prefix frontend run lint
npm --prefix frontend test
npm --prefix frontend run build
docker compose ps
```

`dotnet` se ejecuta dentro de `backend/` (allí está `global.json`). No apliques migraciones, no borres volúmenes ni levantes servicios contra entornos compartidos. Si Docker o el SDK no están disponibles, marca el elemento `No verificable` e incluye la salida.

## Matriz

| Elemento | Resultado | Evidencia | Observación |
|---|---|---|---|
| CHU-01 | Cumple / No cumple / No verificable | ruta, símbolo, prueba, respuesta o artefacto | explicación breve |

- `Cumple`: la evidencia demuestra razonablemente la condición.
- `No cumple`: la evidencia contradice la condición o muestra una ausencia necesaria.
- `No verificable`: falta ejecución o contexto seguro. No cuenta como cumplimiento.

## Cierre

Una HU pasa a `Completada` solo cuando todos los `CHU` obligatorios y la DoD aplicable están en `Cumple`, no hay bloqueadores y la matriz está registrada.

Si la HU cubre un `PRD CA-xx`, indica en las Notas para la wiki si ya puede marcarse en `criterios-de-aceptacion` (con la prueba o el PR que lo demuestra); no lo marques tú.

No repares código, tests, configuración o migraciones durante la validación Scrum. Entrega los gaps al orquestador para que delegue en `backend-engineer` o `frontend-engineer`.

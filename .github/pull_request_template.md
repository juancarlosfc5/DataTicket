## Qué cambia

<!-- Resumen en 1–3 viñetas. -->

## Criterios de aceptación del PRD cubiertos

<!-- Ej.: CA-01, CA-05 (ver wiki/producto/criterios-de-aceptacion.md) -->

## Cómo se verificó

- [ ] `cd backend && dotnet test` (incluye pruebas de arquitectura)
- [ ] `npm --prefix frontend run lint` · `npm --prefix frontend test` · `npm --prefix frontend run build`
- [ ] `docker compose up --build` levanta todos los servicios

## Checklist

- [ ] Respeta la arquitectura: backend hexagonal / frontend MVC
- [ ] Autorización aplicada en el backend (multiempresa, participantes del chat)
- [ ] Sin secretos ni credenciales reales
- [ ] **Wiki actualizada**: páginas afectadas + entrada en `wiki/log.md` (+ `wiki/index.md` si hay páginas nuevas)

## Páginas de la wiki modificadas

<!-- [[pagina-1]], [[pagina-2]] -->

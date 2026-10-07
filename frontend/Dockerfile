# syntax=docker/dockerfile:1

# Etapas:
#   dev     -> Vite dev server con HMR (lo usa docker-compose.yml con Compose Watch)
#   build   -> typecheck + bundle de producción en /app/dist
#   runtime -> nginx sin root que sirve el SPA y hace proxy de /api y /hubs (WebSocket) al backend:
#              docker build --target runtime ./frontend

ARG NODE_VERSION=24
ARG NGINX_VERSION=1.30

# ---------- deps: capa cacheable de dependencias ----------
FROM node:${NODE_VERSION}-alpine AS deps
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund

# ---------- dev ----------
FROM deps AS dev
COPY . .
EXPOSE 5173
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]

# ---------- build ----------
FROM deps AS build
COPY . .
RUN npm run build

# ---------- runtime ----------
FROM nginxinc/nginx-unprivileged:${NGINX_VERSION}-alpine AS runtime
# La imagen oficial procesa /etc/nginx/templates/*.template con envsubst al arrancar.
ENV BACKEND_UPSTREAM=backend:8080 \
    NGINX_RESOLVER=127.0.0.11
COPY nginx/default.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 8080

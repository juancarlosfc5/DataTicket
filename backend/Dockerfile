# syntax=docker/dockerfile:1

# Etapas:
#   dev     -> SDK + dotnet watch (lo usa docker-compose.yml con Compose Watch)
#   test    -> ejecuta todas las pruebas:  docker build --target test ./backend
#   runtime -> imagen final mínima sin root: docker build --target runtime ./backend

ARG DOTNET_VERSION=10.0

# ---------- restore: capa cacheable con solo los archivos de proyecto ----------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS restore
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props DataTicket.slnx ./
# Al crear un proyecto nuevo en src/, añade aquí su .csproj.
COPY src/DataTicket.Domain/DataTicket.Domain.csproj src/DataTicket.Domain/
COPY src/DataTicket.Application/DataTicket.Application.csproj src/DataTicket.Application/
COPY src/DataTicket.Infrastructure/DataTicket.Infrastructure.csproj src/DataTicket.Infrastructure/
COPY src/DataTicket.Api/DataTicket.Api.csproj src/DataTicket.Api/
RUN dotnet restore src/DataTicket.Api/DataTicket.Api.csproj

# ---------- dev: hot reload dentro del contenedor ----------
FROM restore AS dev
COPY src/ src/
ENV ASPNETCORE_ENVIRONMENT=Development \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true \
    DOTNET_WATCH_SUPPRESS_LAUNCH_BROWSER=true
EXPOSE 8080
CMD ["dotnet", "watch", "run", "--project", "src/DataTicket.Api/DataTicket.Api.csproj", "--no-launch-profile", "--non-interactive"]

# ---------- test: compila la solución completa y ejecuta las pruebas ----------
FROM restore AS test
COPY . .
RUN dotnet test --solution DataTicket.slnx

# ---------- build: publicación Release ----------
FROM restore AS build
COPY src/ src/
RUN dotnet publish src/DataTicket.Api/DataTicket.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- runtime: ASP.NET Core 10 sin SDK, usuario no root ----------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "DataTicket.Api.dll"]

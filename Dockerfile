# syntax=docker/dockerfile:1
#
# Container image for the Blazor Server UI host (Modules.Records.UI).
# Built and pushed to GHCR by .github/workflows/deploy.yml and run on the AWS
# Lightsail instance via deploy/aws/docker-compose.yml.

# ── Build ────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, using only the project files, so the restore layer is cached
# until a project or package reference changes.
COPY global.json ./
COPY Modules.Records.Domain/Modules.Records.Domain.csproj           Modules.Records.Domain/
COPY Modules.Records.Application/Modules.Records.Application.csproj Modules.Records.Application/
COPY Shared.Infrastructure/Shared.Infrastructure.csproj             Shared.Infrastructure/
COPY Modules.Records.UI/Modules.Records.UI.csproj                   Modules.Records.UI/
RUN dotnet restore Modules.Records.UI/Modules.Records.UI.csproj

COPY Modules.Records.Domain/      Modules.Records.Domain/
COPY Modules.Records.Application/ Modules.Records.Application/
COPY Shared.Infrastructure/       Shared.Infrastructure/
COPY Modules.Records.UI/          Modules.Records.UI/
# Charge catalog the UI project links into its output (SeedData/charges.json)
# for the demo seeder and the Test Data page.
COPY Documents/charges.json       Documents/
RUN dotnet publish Modules.Records.UI/Modules.Records.UI.csproj \
      -c Release -o /app/publish --no-restore /p:UseAppHost=false
# Fail the build if the Blazor script is missing. Without it the site renders but no
# button, form or map works (see RequiresAspNetWebAssets in Modules.Records.UI.csproj).
RUN test -f /app/publish/wwwroot/_framework/blazor.server.js \
 || { echo "ERROR: wwwroot/_framework/blazor.server.js missing from publish output" >&2; exit 1; }

# ── Runtime ──────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Persistent state lives on Docker volumes mounted at these paths:
#   /data/db              SQLite databases (app.db, auth.db)
#   /data/keys            ASP.NET Core Data Protection keys (auth cookies, antiforgery)
#   /data/uploads         Mugshot uploads (LocalMugshotStorageService). Outside the web
#                         root: images are only served by the authorized /mugshots endpoint.
# Creating them here, owned by the non-root user, means new named volumes inherit
# that ownership on first mount.
RUN mkdir -p /data/db /data/keys /data/uploads/mugshots \
 && chown -R "$APP_UID" /data

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    DefaultDatabaseProvider=Sqlite \
    ConnectionStrings__SqliteAppConnection="Data Source=/data/db/app.db;Cache=Shared;Foreign Keys=True" \
    ConnectionStrings__SqliteAuthConnection="Data Source=/data/db/auth.db;Cache=Shared;Foreign Keys=True" \
    DataProtection__KeysPath=/data/keys \
    Mugshots__StoragePath=/data/uploads/mugshots

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Modules.Records.UI.dll"]
